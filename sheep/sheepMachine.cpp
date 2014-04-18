#include <sstream>
#include "sheepMachine.h"
#include "sheepCodeBuffer.h"
#include "sheepLog.h"


SheepMachine::SheepMachine()
{
	m_callback = NULL;
	m_compilerCallback = NULL;
	m_endWaitCallback = NULL;

	m_verbosityLevel = Verbosity_Silent;

	m_executingDepth = 0;

	// add Call() as an import
	SheepImport* call = m_imports.NewImport("Call", SheepSymbolType::Void);
	call->Parameters.push_back(SheepSymbolType::String);

	SetImportCallback("Call", s_call);

	m_contextTree = new SheepContextTree();

	m_tag = NULL;
	m_enhancementsEnabled = false;
}

SheepMachine::~SheepMachine()
{
	delete m_contextTree;
}

void SheepMachine::SetOutputCallback(void (*callback)(const char *))
{
	// btw, NULL is perfectly fine
	m_callback = callback;
}

void SheepMachine::SetCompileOutputCallback(SHP_MessageCallback callback)
{
	// btw, NULL is perfectly fine
	m_compilerCallback = callback;
}

int SheepMachine::SetImportCallback(const char* importName, Sheep::ImportCallback callback)
{
	if (importName == nullptr)
		return SHEEP_ERR_INVALID_ARGUMENT;

	m_importCallbacks.InsertOrUpdate(importName, callback);

	return SHEEP_SUCCESS;
}

int SheepMachine::PopStringFromStack(const char** result)
{
	SheepContext* current = m_contextTree->GetCurrent();

	if (current == NULL)
		return SHEEP_ERR_NO_CONTEXT_AVAILABLE;
	if (current->Stack.empty())
		return SHEEP_ERR_EMPTY_STACK;

	StackItem item = current->Stack.top();
	current->Stack.pop();

	if (item.Type != SheepSymbolType::String)
		return SHEEP_ERR_WRONG_TYPE_ON_STACK;

	if (result != nullptr)
	{
		IntermediateOutput* code = current->FullCode;
		for (std::vector<SheepStringConstant>::iterator itr = code->Constants.begin();
			itr != code->Constants.end(); itr++)
		{
			if ((*itr).Offset == item.IValue)
			{
				*result = (*itr).Value.c_str();
				return SHEEP_SUCCESS;
			}
		}

		throw SheepMachineException("Invalid string offset found on stack");
	}

	return SHEEP_SUCCESS;
}

IntermediateOutput* SheepMachine::Compile(const std::string &script)
{
	SheepCodeTree tree;
	SheepLog log;
	tree.Lock(script, &log);

	SheepCodeGenerator generator(&tree, &m_imports, m_enhancementsEnabled);
	IntermediateOutput* output = generator.BuildIntermediateOutput();

	tree.Unlock();

	// copy compiler errors into the output's output
	std::vector<SheepLogEntry> entries = log.GetEntries();
	for (unsigned int i = 0; i < entries.size(); i++)
	{
		CompilerOutput co;
		co.LineNumber = entries[i].LineNumber;
		co.Output = entries[i].Text;
		output->Errors.push_back(co);
	}

	// report any errors
	if (output->Errors.empty() == false && m_compilerCallback)
	{
		for (size_t i = 0; i < output->Errors.size(); i++)
		{
			m_compilerCallback(output->Errors[i].LineNumber, output->Errors[i].Output.c_str());
		}
	}

	return output;
}


void SheepMachine::prepareVariables(SheepContext* context)
{
	context->PrepareVariables();
}

void SheepMachine::Run(IntermediateOutput* code, const std::string &function)
{
	if (code == NULL)
		throw SheepMachineException("No code to execute.");

	// find the requsted function
	SheepFunction* sheepfunction = NULL;
	for (std::vector<SheepFunction>::iterator itr = code->Functions.begin();
		itr != code->Functions.end(); itr++)
	{
		if (CIEqual((*itr).Name, function))
		{
			sheepfunction = &(*itr);
			break;
		}
	}

	if (sheepfunction == NULL)
		throw NoSuchFunctionException(function);


	SheepContext* c = SHEEP_NEW SheepContext();
	c->FullCode = code;
	c->CodeBuffer = sheepfunction->Code;
	c->FunctionOffset = sheepfunction->CodeOffset;
	c->InstructionOffset = 0;
	
	prepareVariables(c);

	m_contextTree->Add(c);

	m_executingDepth++;
	execute(c);
	m_executingDepth--;

	if (c->UserSuspended == false &&
		c->ChildSuspended == false)
	{
		c->FullCode->Release();
		m_contextTree->KillContext(c);
	}
}
 
int SheepMachine::RunSnippet(const std::string& snippet, int noun, int verb, int* result)
{
	try
	{
		std::stringstream ss;
		ss << "symbols { int result$; int n$; int v$; } code { snippet$() { result$ = " << snippet << "; } }";


	SheepCodeTree tree;
	tree.Lock(ss.str(), NULL);

	SheepCodeGenerator generator(&tree, &m_imports, m_enhancementsEnabled);
	IntermediateOutput* code = generator.BuildIntermediateOutput();

	tree.Unlock();

	if (code->Errors.empty() == false)
	{
		if (m_compilerCallback)
		{
			for (std::vector<CompilerOutput>::iterator itr = code->Errors.begin();
				itr != code->Errors.end(); itr++)
			{
				m_compilerCallback((*itr).LineNumber, (*itr).Output.c_str());
			}
		}

		return SHEEP_ERROR;
	}

	
	if (code->Functions.empty())
	{
		return SHEEP_ERROR;
	}

	SheepContext* c = SHEEP_NEW SheepContext();
	c->FullCode = code;
	c->CodeBuffer = code->Functions[0].Code;
	c->FunctionOffset = code->Functions[0].CodeOffset;
	c->InstructionOffset = 0;
	prepareVariables(c);
	
	c->SetVariableInt(1, noun);
	c->SetVariableInt(2, verb);

	m_contextTree->Add(c);
	m_executingDepth++;
	execute(c);
	m_executingDepth--;

	if (result != NULL)
		c->GetVariableInt(0, result);

	c->FullCode->Release();
	m_contextTree->KillContext(c);

	return SHEEP_SUCCESS;

	}
	catch(SheepException& ex)
	{
		if (m_compilerCallback)
		{
			m_compilerCallback(0, ex.GetMessage().c_str());
		}

		return -5;
	}
}

SheepContext* SheepMachine::Suspend()
{
	SheepContext* currentContext = m_contextTree->GetCurrent();

	if (currentContext == NULL)
		throw SheepMachineException("No context available", SHEEP_ERR_NO_CONTEXT_AVAILABLE);

	currentContext->UserSuspended = true;
	return currentContext;
}

int SheepMachine::Resume(SheepContext* context)
{
	assert(context != NULL);
	if (context->Dead == true)
	{
		throw SheepMachineException("Cannot resume context because it's dead", SHEEP_ERR_CANT_RESUME);
	}

	context->UserSuspended = false;

	if (context->ChildSuspended == false ||
		context->AreAnyChildrenSuspended() == false)
	{
		// children are obviously done
		context->ChildSuspended = false;

		// not waiting on anything, so run some code
		execute(context);

		if (context->ChildSuspended == false &&
			context->UserSuspended == false)
		{
			// before we can kill the context we need
			// to check its ancestors, since one of them
			// may have been waiting on this child to finish
			SheepContext* parent = context->Parent;
			while(parent != NULL)
			{
				if (parent->Dead == false &&
					parent->ChildSuspended == true &&
					parent->UserSuspended == false &&
					parent->AreAnyChildrenSuspended() == false)
				{
					parent->ChildSuspended = false;
					Resume(parent);

					// no need to continue the loop, since the
					// Resume() call we just made will handle the
					// rest of the ancestors
					break;
				}

				parent = parent->Parent;
			}

			// now then, this context is all finished, so we can kill it
			context->FullCode->Release();
			m_contextTree->KillContext(context);

			return SHEEP_SUCCESS;
		}
		else
		{
			return SHEEP_SUSPENDED;
		}
	}
	else
	{
		return SHEEP_SUSPENDED;
	}
}

void SheepMachine::PrintStackTrace()
{
	SheepContext* context = m_contextTree->GetCurrent();
	while(context != NULL)
	{
		// find the function
		SheepFunction* sheepfunction = NULL;
		for (std::vector<SheepFunction>::iterator itr = context->FullCode->Functions.begin();
			itr != context->FullCode->Functions.end(); itr++)
		{
			if ((*itr).CodeOffset == context->FunctionOffset)
			{
				sheepfunction = &(*itr);
				break;
			}
		}

		if (sheepfunction != NULL)
		{
			printf("%s:%x,%x\n", sheepfunction->Name.c_str(), context->FunctionOffset + context->InstructionOffset, context->InstructionOffset);
		}

		context = context->Parent;
	}
}

void SheepMachine::SetLanguageEnhancementsEnabled(bool enabled)
{
	m_enhancementsEnabled = enabled;
}

void SheepMachine::executeNextInstruction(SheepContext* context)
{
	assert(context->Dead == false);

	// make sure the current context is the one we're working on
	// so that when the user tries to push/pop the stack it will
	// be the right one
	m_contextTree->SetCurrent(context);

	std::vector<SheepImport>& imports = context->FullCode->Imports;

	if (m_verbosityLevel > Verbosity_Annoying)
	{
		printf("stack size: %d:", context->Stack.size());
		
		SheepStack tmp;
		while(context->Stack.empty() == false)
		{
			printf("%d, ", context->Stack.top().Type);

			tmp.push(context->Stack.top());
			context->Stack.pop();
		}

		printf("\n");

		while(tmp.empty() == false)
		{
			context->Stack.push(tmp.top());
			tmp.pop();
		}
	}
	else if (m_verbosityLevel > Verbosity_Polite)
		printf("stack size: %d\n", context->Stack.size());

	if (context->InstructionOffset != context->CodeBuffer->Tell())
		context->CodeBuffer->SeekFromStart(context->InstructionOffset);

	unsigned char instruction = context->CodeBuffer->ReadByte();
	context->InstructionOffset++;

	int iparam1, iparam2;
	float fparam1, fparam2;
	
	switch(instruction)
	{
		case SitnSpin:
			break;
		case Yield:
			throw SheepMachineInstructionException("Yield instruction not supported yet.");
		case CallSysFunctionV:
			context->InstructionOffset += 4;
			callVoidFunction(context, context->CodeBuffer->ReadInt());
			break;
		case CallSysFunctionI:
			context->InstructionOffset += 4;
			callIntFunction(context, context->CodeBuffer->ReadInt());
			if (context->Stack.top().Type != SheepSymbolType::Int)
			{
				throw SheepMachineException("CallSysFunctionI instruction requires integer on stack afterwards", SHEEP_ERR_WRONG_TYPE_ON_STACK);
			}
			break;
		case CallSysFunctionF:
		case CallSysFunctionS:
			throw SheepMachineInstructionException("Function calling not supported yet.");
		case Branch:
		case BranchGoto:
			context->InstructionOffset = context->CodeBuffer->ReadInt() - context->FunctionOffset;
			break;
		case BranchIfZero:
			if (context->Stack.top().Type == SheepSymbolType::Int)
			{
				if (context->Stack.top().IValue == 0)
					context->InstructionOffset = context->CodeBuffer->ReadInt() - context->FunctionOffset;
				else
				{
					context->CodeBuffer->ReadInt(); // throw it away
					context->InstructionOffset += 4;
				}
				context->Stack.pop();
			}
			else
			{
				throw SheepMachineException("BranchIfZero instruction expected integer on stack", SHEEP_ERR_WRONG_TYPE_ON_STACK);
			}
			break;
		case BeginWait:
			assert(context->InWaitSection == false);
			context->InWaitSection = true;
			break;
		case EndWait:
			assert(context->InWaitSection == true);
			context->InWaitSection = false;
			context->ChildSuspended = context->AreAnyChildrenSuspended();
			if (m_endWaitCallback) m_endWaitCallback(this);
			break;
		case ReturnV:
			return;
		case StoreI:
			storeI(context->Stack, context, context->CodeBuffer->ReadInt());
			context->InstructionOffset += 4;
			break;
		case StoreF:
			storeF(context->Stack, context, context->CodeBuffer->ReadInt());
			context->InstructionOffset += 4;
			break;
		case StoreS:
			storeS(context->Stack, context, context->CodeBuffer->ReadInt());
			context->InstructionOffset += 4;
			break;
		case LoadI:
			loadI(context->Stack, context, context->CodeBuffer->ReadInt());
			context->InstructionOffset += 4;
			break;
		case LoadF:
			loadF(context->Stack, context, context->CodeBuffer->ReadInt());
			context->InstructionOffset += 4;
			break;
		case LoadS:
			loadS(context->Stack, context, context->CodeBuffer->ReadInt());
			context->InstructionOffset += 4;
			break;
		case PushI:
			context->Stack.push(StackItem(SheepSymbolType::Int, context->CodeBuffer->ReadInt()));
			context->InstructionOffset += 4;
			break;
		case PushF:
			context->Stack.push(StackItem(SheepSymbolType::Float, context->CodeBuffer->ReadFloat()));
			context->InstructionOffset += 4;
			break;
		case PushS:
			context->Stack.push(StackItem(SheepSymbolType::String, context->CodeBuffer->ReadInt()));
			context->InstructionOffset += 4;
			break;
		case Pop:
			context->Stack.pop();
			break;
		case AddI:
			addI(context->Stack);
			break;
		case AddF:
			addF(context->Stack);
			break;
		case SubtractI:
			subI(context->Stack);
			break;
		case SubtractF:
			subF(context->Stack);
			break;
		case MultiplyI:
			mulI(context->Stack);
			break;
		case MultiplyF:
			mulF(context->Stack);
			break;
		case DivideI:
			divI(context->Stack);
			break;
		case DivideF:
			divF(context->Stack);
			break;
		case NegateI:
			negI(context->Stack);
			break;
		case NegateF:
			negF(context->Stack);
			break;
		case IsEqualI:
			get2Ints(context->Stack, iparam1, iparam2, IsEqualI);
			if (iparam1 == iparam2)
				context->Stack.push(StackItem(SheepSymbolType::Int, 1));
			else
				context->Stack.push(StackItem(SheepSymbolType::Int, 0));
			break;
		case IsEqualF:
			get2Floats(context->Stack, fparam1, fparam2);
			if (fparam1 == fparam2)
				context->Stack.push(StackItem(SheepSymbolType::Int, 1));
			else
				context->Stack.push(StackItem(SheepSymbolType::Int, 0));
			break;
		case NotEqualI:
			get2Ints(context->Stack, iparam1, iparam2, NotEqualI);
			if (iparam1 != iparam2)
				context->Stack.push(StackItem(SheepSymbolType::Int, 1));
			else
				context->Stack.push(StackItem(SheepSymbolType::Int, 0));
			break;
		case NotEqualF:
			get2Floats(context->Stack, fparam1, fparam2);
			if (fparam1 != fparam2)
				context->Stack.push(StackItem(SheepSymbolType::Int, 1));
			else
				context->Stack.push(StackItem(SheepSymbolType::Int, 0));
			break;
		case IsGreaterI:
			get2Ints(context->Stack, iparam1, iparam2, IsGreaterI);
			if (iparam1 > iparam2)
				context->Stack.push(StackItem(SheepSymbolType::Int, 1));
			else
				context->Stack.push(StackItem(SheepSymbolType::Int, 0));
			break;
		case IsGreaterF:
			get2Floats(context->Stack, fparam1, fparam2);
			if (fparam1 > fparam2)
				context->Stack.push(StackItem(SheepSymbolType::Int, 1));
			else
				context->Stack.push(StackItem(SheepSymbolType::Int, 0));
			break;
		case IsLessI:
			get2Ints(context->Stack, iparam1, iparam2, IsLessI);
			if (iparam1 < iparam2)
				context->Stack.push(StackItem(SheepSymbolType::Int, 1));
			else
				context->Stack.push(StackItem(SheepSymbolType::Int, 0));
			break;
		case IsLessF:
			get2Floats(context->Stack, fparam1, fparam2);
			if (fparam1 < fparam2)
				context->Stack.push(StackItem(SheepSymbolType::Int, 1));
			else
				context->Stack.push(StackItem(SheepSymbolType::Int, 0));
			break;
		case IsGreaterEqualI:
			get2Ints(context->Stack, iparam1, iparam2, IsGreaterEqualI);
			if (iparam1 >= iparam2)
				context->Stack.push(StackItem(SheepSymbolType::Int, 1));
			else
				context->Stack.push(StackItem(SheepSymbolType::Int, 0));
			break;
		case IsGreaterEqualF:
			get2Floats(context->Stack, fparam1, fparam2);
			if (fparam1 >= fparam2)
				context->Stack.push(StackItem(SheepSymbolType::Int, 1));
			else
				context->Stack.push(StackItem(SheepSymbolType::Int, 0));
			break;
		case IsLessEqualI:
			get2Ints(context->Stack, iparam1, iparam2, IsLessEqualI);
			if (iparam1 <= iparam2)
				context->Stack.push(StackItem(SheepSymbolType::Int, 1));
			else
				context->Stack.push(StackItem(SheepSymbolType::Int, 0));
			break;
		case IsLessEqualF:
			get2Floats(context->Stack, fparam1, fparam2);
			if (fparam1 <= fparam2)
				context->Stack.push(StackItem(SheepSymbolType::Int, 1));
			else
				context->Stack.push(StackItem(SheepSymbolType::Int, 0));
			break;
		case IToF:
			itof(context->Stack, context->CodeBuffer->ReadInt());
			context->InstructionOffset += 4;
			break;
		case FToI:
			ftoi(context->Stack, context->CodeBuffer->ReadInt());
			context->InstructionOffset += 4;
			break;
		case And:
			andi(context->Stack);
			break;
		case Or:
			ori(context->Stack);
			break;
		case Not:
			noti(context->Stack);
			break;
		case GetString:
			if (context->Stack.top().Type != SheepSymbolType::String)
				throw SheepMachineException("Expected string on stack");
			break;
		case DebugBreakpoint:
			throw SheepMachineInstructionException("DebugBreakpoint instruction not supported yet.");
		default:
			throw SheepMachineInstructionException("Unknown instruction");
	}
}

void SheepMachine::execute(SheepContext* context)
{
	std::vector<SheepImport> imports = context->FullCode->Imports;

	context->CodeBuffer->SeekFromStart(context->InstructionOffset);
	while(!context->UserSuspended && !context->ChildSuspended && context->CodeBuffer->Tell() < context->CodeBuffer->GetSize())
	{
		executeNextInstruction(context);
	}
}

void SheepMachine::s_call(Sheep::IVirtualMachine* vm)
{
	SheepMachine* machine = static_cast<SheepMachine*>(vm);

	const char* f;
	machine->PopStringFromStack(&f);
	std::string function = f;

	// make sure there's a '$' at the end
	if (function[function.length()-1] != '$')
		function += '$';


	// find the requsted function
	SheepContext* currentContext = machine->m_contextTree->GetCurrent();
	SheepFunction* sheepfunction = NULL;
	for (std::vector<SheepFunction>::iterator itr = currentContext->FullCode->Functions.begin();
		itr != currentContext->FullCode->Functions.end(); itr++)
	{
		if ((*itr).Name == function)
		{
			sheepfunction = &(*itr);
			break;
		}
	}

	if (sheepfunction == NULL)
		throw NoSuchFunctionException(function);

	SheepContext* c = SHEEP_NEW(SheepContext);
	*c = *currentContext;
	c->CodeBuffer = sheepfunction->Code;
	c->FunctionOffset = sheepfunction->CodeOffset;
	c->InstructionOffset = 0;
	c->FullCode->AddRef();
	c->Sibling = NULL;
	c->FirstChild = NULL;
	c->Parent = NULL;
	c->InWaitSection = false;
	// TODO: this context should share variables with the previous context
	// so that the functions within the same scripts can modify the same global variables
	

	machine->prepareVariables(c);
	machine->m_contextTree->Add(c);

	machine->m_executingDepth++;
	machine->execute(c);
	machine->m_executingDepth--;

	if (c->UserSuspended == false && c->ChildSuspended == false)
	{
		c->FullCode->Release();
		machine->m_contextTree->KillContext(c);
	}
}

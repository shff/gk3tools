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
	SheepImport* call = m_imports.NewImport("Call", SYM_VOID, s_call);
	call->Parameters.push_back(SYM_STRING);

	m_parentContext = NULL;
	m_currentContext = NULL;
}

SheepMachine::~SheepMachine()
{
	// TODO: clean up the contexts!
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

std::string& SheepMachine::PopStringFromStack()
{
	if (m_currentContext == NULL)
		throw SheepMachineException("No contexts", SHEEP_ERR_NO_CONTEXT_AVAILABLE);
	if (m_currentContext->Stack.empty())
		throw SheepMachineException("Stack is empty", SHEEP_ERR_EMPTY_STACK);

	StackItem item = m_currentContext->Stack.top();
	m_currentContext->Stack.pop();

	if (item.Type != SYM_STRING)
		throw SheepMachineException("Expected string on stack", SHEEP_ERR_WRONG_TYPE_ON_STACK);

	IntermediateOutput* code = m_currentContext->FullCode;
	for (std::vector<SheepStringConstant>::iterator itr = code->Constants.begin();
		itr != code->Constants.end(); itr++)
	{
		if ((*itr).Offset == item.IValue)
			return (*itr).Value;
	}

	throw SheepMachineException("Invalid string offset found on stack");
}

IntermediateOutput* SheepMachine::Compile(const std::string &script)
{
	SheepCodeTree tree;
	SheepLog log;
	tree.Lock(script, &log);

	SheepCodeGenerator generator(&tree, &m_imports);
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
	assert(context->FullCode != NULL);

	for (std::vector<SheepSymbol>::iterator itr = context->FullCode->Symbols.begin();
		itr != context->FullCode->Symbols.end(); itr++)
	{
		if ((*itr).Type == SYM_INT)
			context->Variables.push_back(StackItem(SYM_INT, (*itr).InitialIntValue));
		else if ((*itr).Type == SYM_FLOAT)
			context->Variables.push_back(StackItem(SYM_FLOAT, (*itr).InitialFloatValue));
		else if ((*itr).Type == SYM_STRING)
			context->Variables.push_back(StackItem(SYM_STRING, (*itr).InitialStringValue));
		else
			throw SheepMachineException("Unsupported variable type");
	}
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


	SheepContext* c = new SheepContext();
	c->FullCode = code;
	c->CodeBuffer = sheepfunction->Code;
	c->FunctionOffset = sheepfunction->CodeOffset;
	c->InstructionOffset = 0;
	
	prepareVariables(c);

	addContext(c);

	m_executingDepth++;
	execute(c);
	m_executingDepth--;

	if (c->UserSuspended == false && c->ChildSuspended == false)
	{
		SHEEP_DELETE(c->FullCode);
		removeContext(c);

		if (c->Parent == NULL)
		{
			SHEEP_DELETE(c);
		}
	}
}
 
int SheepMachine::RunSnippet(const std::string& snippet, int* result)
{
	try
	{
	SheepCodeTree tree;
	tree.Lock(snippet, NULL);

	SheepCodeGenerator generator(&tree, &m_imports);
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

	SheepContext* c = new SheepContext();
	c->FullCode = code;
	c->CodeBuffer = code->Functions[0].Code;
	c->FunctionOffset = code->Functions[0].CodeOffset;
	c->InstructionOffset = 0;
	
	addContext(c);
	m_executingDepth++;
	execute(c);
	m_executingDepth--;

	if (c->Stack.empty() == false)
	{
		if (result != NULL)
		{
			if (c->Stack.top().Type == SYM_INT)
				*result = c->Stack.top().IValue;
			else if (c->Stack.top().Type == SYM_FLOAT)
				*result = (int)c->Stack.top().FValue;
		}

		c->Stack.pop();
	}

	SHEEP_DELETE(c->FullCode);
	removeContext(c);

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
	if (m_currentContext == NULL)
		throw SheepMachineException("No context available", SHEEP_ERR_NO_CONTEXT_AVAILABLE);

	m_currentContext->UserSuspended = true;
	return m_currentContext;
}

void SheepMachine::Resume(SheepContext* context)
{
	//if (m_executingDepth != 0)
	//	throw SheepMachineException("Cannot resume while execution is happening", SHEEP_ERR_CANT_RESUME);

	context->UserSuspended = false;

	if (context->ChildSuspended == false)
	{
		execute(context);

		if (context->ChildSuspended == false && context->UserSuspended == false)
		{
			if (context->Parent != NULL &&
				context->Parent->ChildSuspended == true &&
				context->Parent->UserSuspended == false &&
				context->Parent->AreAnyChildrenSuspended() == false)
			{
				context->Parent->ChildSuspended = false;
				Resume(context->Parent);
			}
		}
	}
}

void SheepMachine::SetEndWaitCallback(SHP_EndWaitCallback callback)
{
	m_endWaitCallback = callback;
}

void SheepMachine::executeNextInstruction(SheepContext* context)
{
	// make sure the current context is the one we're working on
	// so that when the user tries to push/pop the stack it will
	// be the right one
	m_currentContext = context;

	std::vector<SheepImport>& imports = context->FullCode->Imports;

	if (m_verbosityLevel > Verbosity_Polite)
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
			callVoidFunction(context->Stack, imports, context->CodeBuffer->ReadInt());
			break;
		case CallSysFunctionI:
			context->InstructionOffset += 4;
			callIntFunction(context->Stack, imports, context->CodeBuffer->ReadInt());
			break;
		case CallSysFunctionF:
		case CallSysFunctionS:
			throw SheepMachineInstructionException("Function calling not supported yet.");
		case Branch:
		case BranchGoto:
			context->InstructionOffset = context->CodeBuffer->ReadInt() - context->FunctionOffset;
			break;
		case BranchIfZero:
			if (context->Stack.top().Type == SYM_INT)
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
				throw SheepMachineException("Expected integer on stack");
			}
			break;
		case BeginWait:
			context->InWaitSection = true;
			break;
		case EndWait:
			context->InWaitSection = false;
			// see if any child contexts are suspended
			if (context->AreAnyChildrenSuspended() == false)
			{
				if (m_endWaitCallback) m_endWaitCallback(this, (SheepVMContext*)context);
			}
			else
			{
				context->ChildSuspended = true;
			}
			break;
		case ReturnV:
			return;
		case StoreI:
			storeI(context->Stack, context->Variables, context->CodeBuffer->ReadInt());
			context->InstructionOffset += 4;
			break;
		case StoreF:
			storeF(context->Stack, context->Variables, context->CodeBuffer->ReadInt());
			context->InstructionOffset += 4;
			break;
		case StoreS:
			storeS(context->Stack, context->Variables, context->CodeBuffer->ReadInt());
			context->InstructionOffset += 4;
			break;
		case LoadI:
			loadI(context->Stack, context->Variables, context->CodeBuffer->ReadInt());
			context->InstructionOffset += 4;
			break;
		case LoadF:
			loadF(context->Stack, context->Variables, context->CodeBuffer->ReadInt());
			context->InstructionOffset += 4;
			break;
		case LoadS:
			throw SheepMachineInstructionException("Loading string variables not supported yet.");
		case PushI:
			context->Stack.push(StackItem(SYM_INT, context->CodeBuffer->ReadInt()));
			context->InstructionOffset += 4;
			break;
		case PushF:
			context->Stack.push(StackItem(SYM_FLOAT, context->CodeBuffer->ReadFloat()));
			context->InstructionOffset += 4;
			break;
		case PushS:
			context->Stack.push(StackItem(SYM_STRING, context->CodeBuffer->ReadInt()));
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
			get2Ints(context->Stack, iparam1, iparam2);
			if (iparam1 == iparam2)
				context->Stack.push(StackItem(SYM_INT, 1));
			else
				context->Stack.push(StackItem(SYM_INT, 0));
			break;
		case IsEqualF:
			get2Floats(context->Stack, fparam1, fparam2);
			if (fparam1 == fparam2)
				context->Stack.push(StackItem(SYM_INT, 1));
			else
				context->Stack.push(StackItem(SYM_INT, 0));
			break;
		case NotEqualI:
			get2Ints(context->Stack, iparam1, iparam2);
			if (iparam1 != iparam2)
				context->Stack.push(StackItem(SYM_INT, 1));
			else
				context->Stack.push(StackItem(SYM_INT, 0));
			break;
		case NotEqualF:
			get2Floats(context->Stack, fparam1, fparam2);
			if (fparam1 != fparam2)
				context->Stack.push(StackItem(SYM_INT, 1));
			else
				context->Stack.push(StackItem(SYM_INT, 0));
			break;
		case IsGreaterI:
			get2Ints(context->Stack, iparam1, iparam2);
			if (iparam1 > iparam2)
				context->Stack.push(StackItem(SYM_INT, 1));
			else
				context->Stack.push(StackItem(SYM_INT, 0));
			break;
		case IsGreaterF:
			get2Floats(context->Stack, fparam1, fparam2);
			if (fparam1 > fparam2)
				context->Stack.push(StackItem(SYM_INT, 1));
			else
				context->Stack.push(StackItem(SYM_INT, 0));
			break;
		case IsLessI:
			get2Ints(context->Stack, iparam1, iparam2);
			if (iparam1 < iparam2)
				context->Stack.push(StackItem(SYM_INT, 1));
			else
				context->Stack.push(StackItem(SYM_INT, 0));
			break;
		case IsLessF:
			get2Floats(context->Stack, fparam1, fparam2);
			if (fparam1 < fparam2)
				context->Stack.push(StackItem(SYM_INT, 1));
			else
				context->Stack.push(StackItem(SYM_INT, 0));
			break;
		case IsGreaterEqualI:
			get2Ints(context->Stack, iparam1, iparam2);
			if (iparam1 >= iparam2)
				context->Stack.push(StackItem(SYM_INT, 1));
			else
				context->Stack.push(StackItem(SYM_INT, 0));
			break;
		case IsGreaterEqualF:
			get2Floats(context->Stack, fparam1, fparam2);
			if (fparam1 >= fparam2)
				context->Stack.push(StackItem(SYM_INT, 1));
			else
				context->Stack.push(StackItem(SYM_INT, 0));
			break;
		case IsLessEqualI:
			get2Ints(context->Stack, iparam1, iparam2);
			if (iparam1 <= iparam2)
				context->Stack.push(StackItem(SYM_INT, 1));
			else
				context->Stack.push(StackItem(SYM_INT, 0));
			break;
		case IsLessEqualF:
			get2Floats(context->Stack, fparam1, fparam2);
			if (fparam1 <= fparam2)
				context->Stack.push(StackItem(SYM_INT, 1));
			else
				context->Stack.push(StackItem(SYM_INT, 0));
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
			if (context->Stack.top().Type != SYM_STRING)
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


void SheepMachine::addContext(SheepContext* context)
{
	if (m_parentContext == NULL)
		m_parentContext = context;
	else
	{
		assert(m_currentContext != NULL);
		SheepContext* itr = m_currentContext->FirstChild;
		if (itr == NULL)
		{
			m_currentContext->FirstChild = context;
			context->Parent = m_currentContext;
		}
		else
		{
			while(itr->Sibling != NULL)
			{
				itr = itr->Sibling;
			}
			assert(itr != NULL);

			itr->Sibling = context;
			context->Parent = itr->Parent;
		}
	}
}

void SheepMachine::removeContext(SheepContext* context)
{
	if (m_parentContext == context)
		m_parentContext = NULL;
	else
	{
		SheepContext* parent = context->Parent;
		assert(parent != NULL);
		SheepContext* itr = parent->FirstChild;
		if (itr == context)
		{
			parent->FirstChild = itr->Sibling;
		}
		else
		{
			while(itr->Sibling != context)
			{
				itr = itr->Sibling;
			}
			assert(itr != NULL);
			assert(itr->Sibling = context);

			itr->Sibling = context->Sibling;
		}
	}
}

void SheepMachine::s_call(SheepVM* vm)
{
	SheepMachine* machine = static_cast<SheepMachine*>(vm);

	std::string function = machine->PopStringFromStack();

	// make sure there's a '$' at the end
	if (function[function.length()-1] != '$')
		function += '$';

	// this stuff is pretty similar to what's inside Run(),
	// the only difference is 


	// find the requsted function
	SheepFunction* sheepfunction = NULL;
	for (std::vector<SheepFunction>::iterator itr = machine->m_currentContext->FullCode->Functions.begin();
		itr != machine->m_currentContext->FullCode->Functions.end(); itr++)
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
	*c = *machine->m_currentContext;
	c->CodeBuffer = sheepfunction->Code;
	c->FunctionOffset = sheepfunction->CodeOffset;
	c->InstructionOffset = 0;
	// TODO: this context should share variables with the previous context
	// so that the functions within the same scripts can modify the same global variables
	
	machine->prepareVariables(c);
	machine->addContext(c);

	machine->m_executingDepth++;
	machine->execute(c);
	machine->m_executingDepth--;

	if (c->UserSuspended == false && c->ChildSuspended == false)
	{
		machine->removeContext(c);

		if (c->Parent == NULL)
		{
			SHEEP_DELETE(c);
		}
	}
}

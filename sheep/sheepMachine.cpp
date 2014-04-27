#include <sstream>
#include "sheepMachine.h"
#include "sheepCodeBuffer.h"
#include "sheepLog.h"
#include "Internal/script.h"
#include "Internal/compiler.h"


SheepMachine::SheepMachine(Sheep::SheepLanguageVersion version)
{
	m_refCount = 0;

	m_callback = NULL;
	m_compilerCallback = NULL;
	m_endWaitCallback = NULL;

	m_verbosityLevel = Verbosity_Silent;

	m_executingDepth = 0;

	// add Call() as an import
	SetImportCallback("Call", s_call);

	m_contextTree = new SheepContextTree();

	m_tag = NULL;
	m_version = version;
}

SheepMachine::~SheepMachine()
{
	delete m_contextTree;
}

void SheepMachine::Release()
{
	m_refCount--;

	if (m_refCount <= 0)
	{
		// delete this, the context tree, and all unreleased contexts
		delete this;
	}
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

int SheepMachine::PrepareScriptForExecution(Sheep::IScript* script, const char* function, Sheep::IExecutionContext** context)
{
	if (script == nullptr || function == nullptr)
		return SHEEP_ERR_INVALID_ARGUMENT;

	IntermediateOutput* code = static_cast<Sheep::Internal::Script*>(script)->GetIntermediateOutput();

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
		return SHEEP_ERR_NO_SUCH_FUNCTION;

	SheepContext* c = SHEEP_NEW SheepContext(this);
	c->FullCode = code;
	c->CodeBuffer = sheepfunction->Code;
	c->FunctionOffset = sheepfunction->CodeOffset;
	c->InstructionOffset = 0;
	
	c->PrepareVariables();

	m_contextTree->Add(c);

	if (context != nullptr)
	{
		c->Aquire();
		*context = c;
	}

	return SHEEP_SUCCESS;
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

void SheepMachine::executeNextInstruction(SheepContext* context)
{
	assert(context->Dead == false);

	// make sure the current context is the one we're working on
	// so that when the user tries to push/pop the stack it will
	// be the right one
	m_contextTree->SetCurrent(context);

	std::vector<SheepImport>& imports = context->FullCode->Imports;
	SheepStack* stack = context->GetStack();

	if (m_verbosityLevel > Verbosity_Annoying)
	{
		printf("stack size: %d:", context->GetStack()->size());
		
		SheepStack tmp;
		while(stack->empty() == false)
		{
			printf("%d, ",  stack->top().Type);

			tmp.push(stack->top());
			stack->pop();
		}

		printf("\n");

		while(tmp.empty() == false)
		{
			stack->push(tmp.top());
			tmp.pop();
		}
	}
	else if (m_verbosityLevel > Verbosity_Polite)
		printf("stack size: %d\n", context->GetStack()->size());

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
			if (context->GetStack()->top().Type != SheepSymbolType::Int)
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
			if (stack->top().Type == SheepSymbolType::Int)
			{
				if (stack->top().IValue == 0)
					context->InstructionOffset = context->CodeBuffer->ReadInt() - context->FunctionOffset;
				else
				{
					context->CodeBuffer->ReadInt(); // throw it away
					context->InstructionOffset += 4;
				}
				stack->pop();
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
			if (m_endWaitCallback) m_endWaitCallback(this, context);
			break;
		case ReturnV:
			return;
		case StoreI:
			storeI(context, context->CodeBuffer->ReadInt());
			context->InstructionOffset += 4;
			break;
		case StoreF:
			storeF(context, context->CodeBuffer->ReadInt());
			context->InstructionOffset += 4;
			break;
		case StoreS:
			storeS(context, context->CodeBuffer->ReadInt());
			context->InstructionOffset += 4;
			break;
		case LoadI:
			loadI(context, context->CodeBuffer->ReadInt());
			context->InstructionOffset += 4;
			break;
		case LoadF:
			loadF(context, context->CodeBuffer->ReadInt());
			context->InstructionOffset += 4;
			break;
		case LoadS:
			loadS(context, context->CodeBuffer->ReadInt());
			context->InstructionOffset += 4;
			break;
		case PushI:
			stack->push(StackItem(SheepSymbolType::Int, context->CodeBuffer->ReadInt()));
			context->InstructionOffset += 4;
			break;
		case PushF:
			stack->push(StackItem(SheepSymbolType::Float, context->CodeBuffer->ReadFloat()));
			context->InstructionOffset += 4;
			break;
		case PushS:
			stack->push(StackItem(SheepSymbolType::String, context->CodeBuffer->ReadInt()));
			context->InstructionOffset += 4;
			break;
		case Pop:
			stack->pop();
			break;
		case AddI:
			addI(stack);
			break;
		case AddF:
			addF(stack);
			break;
		case SubtractI:
			subI(stack);
			break;
		case SubtractF:
			subF(stack);
			break;
		case MultiplyI:
			mulI(stack);
			break;
		case MultiplyF:
			mulF(stack);
			break;
		case DivideI:
			divI(stack);
			break;
		case DivideF:
			divF(stack);
			break;
		case NegateI:
			negI(stack);
			break;
		case NegateF:
			negF(stack);
			break;
		case IsEqualI:
			get2Ints(stack, iparam1, iparam2, IsEqualI);
			if (iparam1 == iparam2)
				stack->push(StackItem(SheepSymbolType::Int, 1));
			else
				stack->push(StackItem(SheepSymbolType::Int, 0));
			break;
		case IsEqualF:
			get2Floats(stack, fparam1, fparam2);
			if (fparam1 == fparam2)
				stack->push(StackItem(SheepSymbolType::Int, 1));
			else
				stack->push(StackItem(SheepSymbolType::Int, 0));
			break;
		case NotEqualI:
			get2Ints(stack, iparam1, iparam2, NotEqualI);
			if (iparam1 != iparam2)
				stack->push(StackItem(SheepSymbolType::Int, 1));
			else
				stack->push(StackItem(SheepSymbolType::Int, 0));
			break;
		case NotEqualF:
			get2Floats(stack, fparam1, fparam2);
			if (fparam1 != fparam2)
				stack->push(StackItem(SheepSymbolType::Int, 1));
			else
				stack->push(StackItem(SheepSymbolType::Int, 0));
			break;
		case IsGreaterI:
			get2Ints(stack, iparam1, iparam2, IsGreaterI);
			if (iparam1 > iparam2)
				stack->push(StackItem(SheepSymbolType::Int, 1));
			else
				stack->push(StackItem(SheepSymbolType::Int, 0));
			break;
		case IsGreaterF:
			get2Floats(stack, fparam1, fparam2);
			if (fparam1 > fparam2)
				stack->push(StackItem(SheepSymbolType::Int, 1));
			else
				stack->push(StackItem(SheepSymbolType::Int, 0));
			break;
		case IsLessI:
			get2Ints(stack, iparam1, iparam2, IsLessI);
			if (iparam1 < iparam2)
				stack->push(StackItem(SheepSymbolType::Int, 1));
			else
				stack->push(StackItem(SheepSymbolType::Int, 0));
			break;
		case IsLessF:
			get2Floats(stack, fparam1, fparam2);
			if (fparam1 < fparam2)
				stack->push(StackItem(SheepSymbolType::Int, 1));
			else
				stack->push(StackItem(SheepSymbolType::Int, 0));
			break;
		case IsGreaterEqualI:
			get2Ints(stack, iparam1, iparam2, IsGreaterEqualI);
			if (iparam1 >= iparam2)
				stack->push(StackItem(SheepSymbolType::Int, 1));
			else
				stack->push(StackItem(SheepSymbolType::Int, 0));
			break;
		case IsGreaterEqualF:
			get2Floats(stack, fparam1, fparam2);
			if (fparam1 >= fparam2)
				stack->push(StackItem(SheepSymbolType::Int, 1));
			else
				stack->push(StackItem(SheepSymbolType::Int, 0));
			break;
		case IsLessEqualI:
			get2Ints(stack, iparam1, iparam2, IsLessEqualI);
			if (iparam1 <= iparam2)
				stack->push(StackItem(SheepSymbolType::Int, 1));
			else
				stack->push(StackItem(SheepSymbolType::Int, 0));
			break;
		case IsLessEqualF:
			get2Floats(stack, fparam1, fparam2);
			if (fparam1 <= fparam2)
				stack->push(StackItem(SheepSymbolType::Int, 1));
			else
				stack->push(StackItem(SheepSymbolType::Int, 0));
			break;
		case IToF:
			itof(stack, context->CodeBuffer->ReadInt());
			context->InstructionOffset += 4;
			break;
		case FToI:
			ftoi(stack, context->CodeBuffer->ReadInt());
			context->InstructionOffset += 4;
			break;
		case And:
			andi(stack);
			break;
		case Or:
			ori(stack);
			break;
		case Not:
			noti(stack);
			break;
		case GetString:
			if (stack->top().Type != SheepSymbolType::String)
				throw SheepMachineException("Expected string on stack");
			break;
		case DebugBreakpoint:
			throw SheepMachineInstructionException("DebugBreakpoint instruction not supported yet.");
		default:
			throw SheepMachineInstructionException("Unknown instruction");
	}
}

void SheepMachine::Execute(SheepContext* context)
{
	std::vector<SheepImport> imports = context->FullCode->Imports;

	context->CodeBuffer->SeekFromStart(context->InstructionOffset);
	while(!context->UserSuspended && !context->ChildSuspended && context->CodeBuffer->Tell() < context->CodeBuffer->GetSize())
	{
		executeNextInstruction(context);
	}
}

void SheepMachine::s_call(Sheep::IExecutionContext* context)
{
	SheepContext* sheepContext = static_cast<SheepContext*>(context);

	const char* f;
	context->PopStringFromStack(&f);
	std::string function = f;

	// make sure there's a '$' at the end
	if (function[function.length()-1] != '$')
		function += '$';


	// find the requsted function
	SheepFunction* sheepfunction = NULL;
	for (std::vector<SheepFunction>::iterator itr = sheepContext->FullCode->Functions.begin();
		itr != sheepContext->FullCode->Functions.end(); itr++)
	{
		if ((*itr).Name == function)
		{
			sheepfunction = &(*itr);
			break;
		}
	}

	if (sheepfunction == NULL)
		throw NoSuchFunctionException(function);

	SheepMachine* machine = static_cast<SheepMachine*>(sheepContext->GetParentVirtualMachine());
	SheepContext* c = SHEEP_NEW SheepContext(sheepContext);
	c->CodeBuffer = sheepfunction->Code;
	c->FunctionOffset = sheepfunction->CodeOffset;
	c->FullCode->AddRef();
	// TODO: this context should share variables with the previous context
	// so that the functions within the same scripts can modify the same global variables
	

	c->PrepareVariables();
	machine->m_contextTree->Add(c);

	machine->m_executingDepth++;
	machine->Execute(c);
	machine->m_executingDepth--;

	if (c->UserSuspended == false && c->ChildSuspended == false)
	{
		c->FullCode->Release();
		machine->m_contextTree->KillContext(c);
	}
}

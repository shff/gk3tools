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
	if (script == nullptr || function == nullptr || context == nullptr)
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

	SheepContext* c = m_contextTree->Create(this, sheepfunction);
	
	c->PrepareVariables();

	c->Aquire();
	*context = c;

	return SHEEP_SUCCESS;
}

int SheepMachine::PrepareScriptForExecutionWithParent(Sheep::IScript* script, const char* function, Sheep::IExecutionContext* parent, Sheep::IExecutionContext** context)
{
	if (script == nullptr || function == nullptr || context == nullptr)
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

	SheepContext* c = m_contextTree->Create(static_cast<SheepContext*>(parent), sheepfunction);
	
	c->PrepareVariables();

	c->Aquire();
	*context = c;

	return SHEEP_SUCCESS;
}

void SheepMachine::PrintStackTrace()
{
	// TODO
}

void SheepMachine::executeNextInstruction(SheepContext* context)
{
	assert(context->IsDead() == false);

	std::vector<SheepImport>& imports = context->GetFunction()->ParentCode->Imports;
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

	SheepCodeBuffer* code = context->GetFunction()->Code;

	if (context->InstructionOffset != code->Tell())
		code->SeekFromStart(context->InstructionOffset);

	unsigned char instruction = code->ReadByte();
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
			callVoidFunction(context, code->ReadInt());
			break;
		case CallSysFunctionI:
			context->InstructionOffset += 4;
			callIntFunction(context, code->ReadInt());
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
			context->InstructionOffset = code->ReadInt() - context->GetFunction()->CodeOffset;
			break;
		case BranchIfZero:
			if (stack->top().Type == SheepSymbolType::Int)
			{
				if (stack->top().IValue == 0)
					context->InstructionOffset = code->ReadInt() - context->GetFunction()->CodeOffset;
				else
				{
					code->ReadInt(); // throw it away
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
			storeI(context, code->ReadInt());
			context->InstructionOffset += 4;
			break;
		case StoreF:
			storeF(context, code->ReadInt());
			context->InstructionOffset += 4;
			break;
		case StoreS:
			storeS(context, code->ReadInt());
			context->InstructionOffset += 4;
			break;
		case StoreArgI:
			storeArgI(context, code->ReadInt());
			context->InstructionOffset += 4;
			break;
		case StoreArgF:
			storeArgF(context, code->ReadInt());
			context->InstructionOffset += 4;
			break;
		case StoreArgS:
			throw SheepMachineInstructionException("String parameters not supported yet.");
		case LoadI:
			loadI(context, code->ReadInt());
			context->InstructionOffset += 4;
			break;
		case LoadF:
			loadF(context, code->ReadInt());
			context->InstructionOffset += 4;
			break;
		case LoadS:
			loadS(context, code->ReadInt());
			context->InstructionOffset += 4;
			break;
		case LoadArgI:
			loadArgI(context, code->ReadInt());
			context->InstructionOffset += 4;
			break;
		case LoadArgF:
			loadArgF(context, code->ReadInt());
			context->InstructionOffset += 4;
			break;
		case LoadArgS:
			throw SheepMachineInstructionException("String parameters not supported yet.");
			break;
		case PushI:
			stack->push(StackItem(SheepSymbolType::Int, code->ReadInt()));
			context->InstructionOffset += 4;
			break;
		case PushF:
			stack->push(StackItem(SheepSymbolType::Float, code->ReadFloat()));
			context->InstructionOffset += 4;
			break;
		case PushS:
			stack->push(StackItem(SheepSymbolType::String, code->ReadInt()));
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
			itof(stack, code->ReadInt());
			context->InstructionOffset += 4;
			break;
		case FToI:
			ftoi(stack, code->ReadInt());
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
	std::vector<SheepImport> imports = context->GetFunction()->ParentCode->Imports;

	SheepCodeBuffer* code = context->GetFunction()->Code;

	code->SeekFromStart(context->InstructionOffset);
	while(!context->UserSuspended && !context->ChildSuspended && code->Tell() < code->GetSize())
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

	IntermediateOutput* fullCode = sheepContext->GetFunction()->ParentCode;

	// find the requsted function
	SheepFunction* sheepfunction = NULL;
	for (std::vector<SheepFunction>::iterator itr = fullCode->Functions.begin();
		itr != fullCode->Functions.end(); itr++)
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
	SheepContext* c = machine->GetContextTree()->Create(sheepContext, sheepfunction);

	c->PrepareVariables();

	machine->m_executingDepth++;
	machine->Execute(c);
	machine->m_executingDepth--;

	// TODO: what if we're waiting on the call to complete? We should handle a suspended context.
}

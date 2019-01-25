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
		itr != code->Functions.end(); ++itr)
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
		case (unsigned char)SheepInstruction::SitnSpin:
			break;
		case (unsigned char)SheepInstruction::Yield:
			throw SheepMachineInstructionException("Yield instruction not supported yet.");
		case (unsigned char)SheepInstruction::CallSysFunctionV:
			context->InstructionOffset += 4;
			callVoidFunction(context, code->ReadInt());
			break;
		case (unsigned char)SheepInstruction::CallSysFunctionI:
			context->InstructionOffset += 4;
			callIntFunction(context, code->ReadInt());
			if (context->GetStack()->top().Type != SheepSymbolType::Int)
			{
				throw SheepMachineException("CallSysFunctionI instruction requires integer on stack afterwards", SHEEP_ERR_WRONG_TYPE_ON_STACK);
			}
			break;
		case (unsigned char)SheepInstruction::CallSysFunctionF:
		case (unsigned char)SheepInstruction::CallSysFunctionS:
			throw SheepMachineInstructionException("Function calling not supported yet.");
		case (unsigned char)SheepInstruction::Branch:
		case (unsigned char)SheepInstruction::BranchGoto:
			context->InstructionOffset = code->ReadInt() - context->GetFunction()->CodeOffset;
			break;
		case (unsigned char)SheepInstruction::BranchIfZero:
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
		case (unsigned char)SheepInstruction::BeginWait:
			assert(context->InWaitSection == false);
			context->InWaitSection = true;
			break;
		case (unsigned char)SheepInstruction::EndWait:
			assert(context->InWaitSection == true);
			context->InWaitSection = false;
			context->ChildSuspended = context->AreAnyChildrenSuspended();
			if (m_endWaitCallback) m_endWaitCallback(this, context);
			break;
		case (unsigned char)SheepInstruction::ReturnV:
			return;
		case (unsigned char)SheepInstruction::StoreI:
			storeI(context, code->ReadInt());
			context->InstructionOffset += 4;
			break;
		case (unsigned char)SheepInstruction::StoreF:
			storeF(context, code->ReadInt());
			context->InstructionOffset += 4;
			break;
		case (unsigned char)SheepInstruction::StoreS:
			storeS(context, code->ReadInt());
			context->InstructionOffset += 4;
			break;
		case (unsigned char)SheepInstruction::StoreArgI:
			storeArgI(context, code->ReadInt());
			context->InstructionOffset += 4;
			break;
		case (unsigned char)SheepInstruction::StoreArgF:
			storeArgF(context, code->ReadInt());
			context->InstructionOffset += 4;
			break;
		case (unsigned char)SheepInstruction::StoreArgS:
			throw SheepMachineInstructionException("String parameters not supported yet.");
		case (unsigned char)SheepInstruction::LoadI:
			loadI(context, code->ReadInt());
			context->InstructionOffset += 4;
			break;
		case (unsigned char)SheepInstruction::LoadF:
			loadF(context, code->ReadInt());
			context->InstructionOffset += 4;
			break;
		case (unsigned char)SheepInstruction::LoadS:
			loadS(context, code->ReadInt());
			context->InstructionOffset += 4;
			break;
		case (unsigned char)SheepInstruction::LoadArgI:
			loadArgI(context, code->ReadInt());
			context->InstructionOffset += 4;
			break;
		case (unsigned char)SheepInstruction::LoadArgF:
			loadArgF(context, code->ReadInt());
			context->InstructionOffset += 4;
			break;
		case (unsigned char)SheepInstruction::LoadArgS:
			throw SheepMachineInstructionException("String parameters not supported yet.");
			break;
		case (unsigned char)SheepInstruction::PushI:
			stack->push(StackItem(SheepSymbolType::Int, code->ReadInt()));
			context->InstructionOffset += 4;
			break;
		case (unsigned char)SheepInstruction::PushF:
			stack->push(StackItem(SheepSymbolType::Float, code->ReadFloat()));
			context->InstructionOffset += 4;
			break;
		case (unsigned char)SheepInstruction::PushS:
			stack->push(StackItem(SheepSymbolType::String, code->ReadInt()));
			context->InstructionOffset += 4;
			break;
		case (unsigned char)SheepInstruction::Pop:
			stack->pop();
			break;
		case (unsigned char)SheepInstruction::AddI:
			addI(stack);
			break;
		case (unsigned char)SheepInstruction::AddF:
			addF(stack);
			break;
		case (unsigned char)SheepInstruction::SubtractI:
			subI(stack);
			break;
		case (unsigned char)SheepInstruction::SubtractF:
			subF(stack);
			break;
		case (unsigned char)SheepInstruction::MultiplyI:
			mulI(stack);
			break;
		case (unsigned char)SheepInstruction::MultiplyF:
			mulF(stack);
			break;
		case (unsigned char)SheepInstruction::DivideI:
			divI(stack);
			break;
		case (unsigned char)SheepInstruction::DivideF:
			divF(stack);
			break;
		case (unsigned char)SheepInstruction::NegateI:
			negI(stack);
			break;
		case (unsigned char)SheepInstruction::NegateF:
			negF(stack);
			break;
		case (unsigned char)SheepInstruction::IsEqualI:
			get2Ints(stack, iparam1, iparam2, SheepInstruction::IsEqualI);
			if (iparam1 == iparam2)
				stack->push(StackItem(SheepSymbolType::Int, 1));
			else
				stack->push(StackItem(SheepSymbolType::Int, 0));
			break;
		case (unsigned char)SheepInstruction::IsEqualF:
			get2Floats(stack, fparam1, fparam2);
			if (fparam1 == fparam2)
				stack->push(StackItem(SheepSymbolType::Int, 1));
			else
				stack->push(StackItem(SheepSymbolType::Int, 0));
			break;
		case (unsigned char)SheepInstruction::NotEqualI:
			get2Ints(stack, iparam1, iparam2, SheepInstruction::NotEqualI);
			if (iparam1 != iparam2)
				stack->push(StackItem(SheepSymbolType::Int, 1));
			else
				stack->push(StackItem(SheepSymbolType::Int, 0));
			break;
		case (unsigned char)SheepInstruction::NotEqualF:
			get2Floats(stack, fparam1, fparam2);
			if (fparam1 != fparam2)
				stack->push(StackItem(SheepSymbolType::Int, 1));
			else
				stack->push(StackItem(SheepSymbolType::Int, 0));
			break;
		case (unsigned char)SheepInstruction::IsGreaterI:
			get2Ints(stack, iparam1, iparam2, SheepInstruction::IsGreaterI);
			if (iparam1 > iparam2)
				stack->push(StackItem(SheepSymbolType::Int, 1));
			else
				stack->push(StackItem(SheepSymbolType::Int, 0));
			break;
		case (unsigned char)SheepInstruction::IsGreaterF:
			get2Floats(stack, fparam1, fparam2);
			if (fparam1 > fparam2)
				stack->push(StackItem(SheepSymbolType::Int, 1));
			else
				stack->push(StackItem(SheepSymbolType::Int, 0));
			break;
		case (unsigned char)SheepInstruction::IsLessI:
			get2Ints(stack, iparam1, iparam2, SheepInstruction::IsLessI);
			if (iparam1 < iparam2)
				stack->push(StackItem(SheepSymbolType::Int, 1));
			else
				stack->push(StackItem(SheepSymbolType::Int, 0));
			break;
		case (unsigned char)SheepInstruction::IsLessF:
			get2Floats(stack, fparam1, fparam2);
			if (fparam1 < fparam2)
				stack->push(StackItem(SheepSymbolType::Int, 1));
			else
				stack->push(StackItem(SheepSymbolType::Int, 0));
			break;
		case (unsigned char)SheepInstruction::IsGreaterEqualI:
			get2Ints(stack, iparam1, iparam2, SheepInstruction::IsGreaterEqualI);
			if (iparam1 >= iparam2)
				stack->push(StackItem(SheepSymbolType::Int, 1));
			else
				stack->push(StackItem(SheepSymbolType::Int, 0));
			break;
		case (unsigned char)SheepInstruction::IsGreaterEqualF:
			get2Floats(stack, fparam1, fparam2);
			if (fparam1 >= fparam2)
				stack->push(StackItem(SheepSymbolType::Int, 1));
			else
				stack->push(StackItem(SheepSymbolType::Int, 0));
			break;
		case (unsigned char)SheepInstruction::IsLessEqualI:
			get2Ints(stack, iparam1, iparam2, SheepInstruction::IsLessEqualI);
			if (iparam1 <= iparam2)
				stack->push(StackItem(SheepSymbolType::Int, 1));
			else
				stack->push(StackItem(SheepSymbolType::Int, 0));
			break;
		case (unsigned char)SheepInstruction::IsLessEqualF:
			get2Floats(stack, fparam1, fparam2);
			if (fparam1 <= fparam2)
				stack->push(StackItem(SheepSymbolType::Int, 1));
			else
				stack->push(StackItem(SheepSymbolType::Int, 0));
			break;
		case (unsigned char)SheepInstruction::IToF:
			itof(stack, code->ReadInt());
			context->InstructionOffset += 4;
			break;
		case (unsigned char)SheepInstruction::FToI:
			ftoi(stack, code->ReadInt());
			context->InstructionOffset += 4;
			break;
		case (unsigned char)SheepInstruction::And:
			andi(stack);
			break;
		case (unsigned char)SheepInstruction::Or:
			ori(stack);
			break;
		case (unsigned char)SheepInstruction::Not:
			noti(stack);
			break;
		case (unsigned char)SheepInstruction::GetString:
			if (stack->top().Type != SheepSymbolType::String)
				throw SheepMachineException("Expected string on stack");
			break;
		case (unsigned char)SheepInstruction::DebugBreakpoint:
			throw SheepMachineInstructionException("DebugBreakpoint instruction not supported yet.");
		default:
			throw SheepMachineInstructionException("Unknown instruction");
	}
}

void SheepMachine::Execute(SheepContext* context)
{
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
		itr != fullCode->Functions.end(); ++itr)
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

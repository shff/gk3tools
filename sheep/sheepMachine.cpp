#include "sheepMachine.h"
#include "sheepCodeBuffer.h"



SheepMachine::SheepMachine()
{
	m_callback = NULL;
	m_compilerCallback = NULL;
	m_endWaitCallback = NULL;

	// add Call() as an import
	SheepImport* call = m_imports.NewImport("Call", SYM_VOID, s_call);
	call->Parameters.push_back(SYM_STRING);
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
	StackItem item = m_contexts.top().Stack.top();
	m_contexts.top().Stack.pop();

	if (item.Type != SYM_STRING)
		throw SheepMachineException("Expected float on stack");

	IntermediateOutput* code = m_contexts.top().FullCode;
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
	tree.Lock(script, NULL);

	SheepCodeGenerator generator(&tree, &m_imports);
	IntermediateOutput* output = generator.BuildIntermediateOutput();

	if (output->Errors.empty() == false && m_compilerCallback)
	{
		for (size_t i = 0; i < output->Errors.size(); i++)
		{
			m_compilerCallback(output->Errors[i].LineNumber, output->Errors[i].Output.c_str());
		}
	}

	return output;
}


void SheepMachine::prepareVariables(SheepContext& context)
{
	assert(context.FullCode != NULL);

	for (std::vector<SheepSymbol>::iterator itr = context.FullCode->Symbols.begin();
		itr != context.FullCode->Symbols.end(); itr++)
	{
		if ((*itr).Type == SYM_INT)
			context.Variables.push_back(StackItem(SYM_INT, (*itr).InitialIntValue));
		else if ((*itr).Type == SYM_FLOAT)
			context.Variables.push_back(StackItem(SYM_FLOAT, (*itr).InitialFloatValue));
		else if ((*itr).Type == SYM_STRING)
			context.Variables.push_back(StackItem(SYM_STRING, (*itr).InitialStringValue));
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


	SheepContext c;
	c.FullCode = code;
	c.CodeBuffer = sheepfunction->Code;
	c.FunctionOffset = sheepfunction->CodeOffset;
	c.InstructionOffset = 0;
	
	prepareVariables(c);
	m_contexts.push(c);

	execute(m_contexts.top());

	if (m_contexts.top().Suspended == false)
	{
		delete m_contexts.top().FullCode;
		m_contexts.pop();
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

	SheepContext c;
	c.FullCode = code;
	c.CodeBuffer = code->Functions[0].Code;
	c.FunctionOffset = code->Functions[0].CodeOffset;
	c.InstructionOffset = 0;
	
	m_contexts.push(c);
	execute(m_contexts.top());

	int returnValue = 0;
	if (m_contexts.top().Stack.empty() == false)
	{
		if (result != NULL)
		{
			if (m_contexts.top().Stack.top().Type == SYM_INT)
				*result = m_contexts.top().Stack.top().IValue;
			else if (m_contexts.top().Stack.top().Type == SYM_FLOAT)
				*result = (int)m_contexts.top().Stack.top().FValue;
		}

		m_contexts.top().Stack.pop();
	}

	delete m_contexts.top().FullCode;
	m_contexts.pop();

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

int SheepMachine::Suspend()
{
	if (m_contexts.empty() || m_contexts.top().Suspended)
		return SHEEP_ERROR;

	m_contexts.top().Suspended = true;
	return SHEEP_SUCCESS;
}

int SheepMachine::Resume()
{
	if (m_contexts.empty() || !m_contexts.top().Suspended)
		return SHEEP_ERROR;

	try
	{
		m_contexts.top().Suspended = false;
		executeContextsUntilSuspendedOrFinished();

		return SHEEP_SUCCESS;
	}
	catch(SheepException& ex)
	{
		if (m_compilerCallback)
		{
			m_compilerCallback(0, ex.GetMessage().c_str());
		}

		return SHEEP_ERROR;
	}
}

void SheepMachine::SetEndWaitCallback(SHP_EndWaitCallback callback)
{
	m_endWaitCallback = callback;
}

void SheepMachine::execute(SheepContext& context)
{
	std::vector<SheepImport> imports = context.FullCode->Imports;
	SheepStack::size_type numItemsOnStack = context.Stack.size();

	context.CodeBuffer->SeekFromStart(context.InstructionOffset);
	while(!context.Suspended && context.CodeBuffer->Tell() < context.CodeBuffer->GetSize())
	{
		printf("stack size: %d\n", context.Stack.size());

		if (context.InstructionOffset != context.CodeBuffer->Tell())
			context.CodeBuffer->SeekFromStart(context.InstructionOffset);	

		unsigned char instruction = context.CodeBuffer->ReadByte();
		context.InstructionOffset++;

		int iparam1, iparam2;
		float fparam1, fparam2;

		switch(instruction)
		{
		case SitnSpin:
			break;
		case Yield:
			throw SheepMachineInstructionException("Yield instruction not supported yet.");
		case CallSysFunctionV:
			context.InstructionOffset += 4;
			callVoidFunction(context.Stack, imports, context.CodeBuffer->ReadInt());
			break;
		case CallSysFunctionI:
			context.InstructionOffset += 4;
			callIntFunction(context.Stack, imports, context.CodeBuffer->ReadInt());
			break;
		case CallSysFunctionF:
		case CallSysFunctionS:
			throw SheepMachineInstructionException("Function calling not supported yet.");
		case Branch:
		case BranchGoto:
			context.InstructionOffset = context.CodeBuffer->ReadInt() - context.FunctionOffset;
			break;
		case BranchIfZero:
			if (context.Stack.top().Type == SYM_INT)
			{
				if (context.Stack.top().IValue == 0)
					context.InstructionOffset = context.CodeBuffer->ReadInt() - context.FunctionOffset;
				else
				{
					context.CodeBuffer->ReadInt(); // throw it away
					context.InstructionOffset += 4;
				}
				context.Stack.pop();
			}
			else
			{
				throw SheepMachineException("Expected integer on stack");
			}
			break;
		case BeginWait:
			context.InWaitSection = true;
			break;
		case EndWait:
			context.InWaitSection = false;
			if (m_endWaitCallback) m_endWaitCallback(this);
			break;
		case ReturnV:
			return;
		case StoreI:
			storeI(context.Stack, context.Variables, context.CodeBuffer->ReadInt());
			context.InstructionOffset += 4;
			break;
		case StoreF:
			storeF(context.Stack, context.Variables, context.CodeBuffer->ReadInt());
			context.InstructionOffset += 4;
			break;
		case StoreS:
			storeS(context.Stack, context.Variables, context.CodeBuffer->ReadInt());
			context.InstructionOffset += 4;
			break;
		case LoadI:
			loadI(context.Stack, context.Variables, context.CodeBuffer->ReadInt());
			context.InstructionOffset += 4;
			break;
		case LoadF:
			loadF(context.Stack, context.Variables, context.CodeBuffer->ReadInt());
			context.InstructionOffset += 4;
			break;
		case LoadS:
			throw SheepMachineInstructionException("Loading string variables not supported yet.");
		case PushI:
			context.Stack.push(StackItem(SYM_INT, context.CodeBuffer->ReadInt()));
			context.InstructionOffset += 4;
			break;
		case PushF:
			context.Stack.push(StackItem(SYM_FLOAT, context.CodeBuffer->ReadFloat()));
			context.InstructionOffset += 4;
			break;
		case PushS:
			context.Stack.push(StackItem(SYM_STRING, context.CodeBuffer->ReadInt()));
			context.InstructionOffset += 4;
			break;
		case Pop:
			context.Stack.pop();
			break;
		case AddI:
			addI(context.Stack);
			break;
		case AddF:
			addF(context.Stack);
			break;
		case SubtractI:
			subI(context.Stack);
			break;
		case SubtractF:
			subF(context.Stack);
			break;
		case MultiplyI:
			mulI(context.Stack);
			break;
		case MultiplyF:
			mulF(context.Stack);
			break;
		case DivideI:
			divI(context.Stack);
			break;
		case DivideF:
			divF(context.Stack);
			break;
		case NegateI:
			negI(context.Stack);
			break;
		case NegateF:
			negF(context.Stack);
			break;
		case IsEqualI:
			get2Ints(context.Stack, iparam1, iparam2);
			if (iparam1 == iparam2)
				context.Stack.push(StackItem(SYM_INT, 1));
			else
				context.Stack.push(StackItem(SYM_INT, 0));
			break;
		case IsEqualF:
			get2Floats(context.Stack, fparam1, fparam2);
			if (fparam1 == fparam2)
				context.Stack.push(StackItem(SYM_INT, 1));
			else
				context.Stack.push(StackItem(SYM_INT, 0));
			break;
		case NotEqualI:
			get2Ints(context.Stack, iparam1, iparam2);
			if (iparam1 != iparam2)
				context.Stack.push(StackItem(SYM_INT, 1));
			else
				context.Stack.push(StackItem(SYM_INT, 0));
			break;
		case NotEqualF:
			get2Floats(context.Stack, fparam1, fparam2);
			if (fparam1 != fparam2)
				context.Stack.push(StackItem(SYM_INT, 1));
			else
				context.Stack.push(StackItem(SYM_INT, 0));
			break;
		case IsGreaterI:
			get2Ints(context.Stack, iparam1, iparam2);
			if (iparam1 > iparam2)
				context.Stack.push(StackItem(SYM_INT, 1));
			else
				context.Stack.push(StackItem(SYM_INT, 0));
			break;
		case IsGreaterF:
			get2Floats(context.Stack, fparam1, fparam2);
			if (fparam1 > fparam2)
				context.Stack.push(StackItem(SYM_INT, 1));
			else
				context.Stack.push(StackItem(SYM_INT, 0));
			break;
		case IsLessI:
			get2Ints(context.Stack, iparam1, iparam2);
			if (iparam1 < iparam2)
				context.Stack.push(StackItem(SYM_INT, 1));
			else
				context.Stack.push(StackItem(SYM_INT, 0));
			break;
		case IsLessF:
			get2Floats(context.Stack, fparam1, fparam2);
			if (fparam1 < fparam2)
				context.Stack.push(StackItem(SYM_INT, 1));
			else
				context.Stack.push(StackItem(SYM_INT, 0));
			break;
		case IsGreaterEqualI:
			get2Ints(context.Stack, iparam1, iparam2);
			if (iparam1 >= iparam2)
				context.Stack.push(StackItem(SYM_INT, 1));
			else
				context.Stack.push(StackItem(SYM_INT, 0));
			break;
		case IsGreaterEqualF:
			get2Floats(context.Stack, fparam1, fparam2);
			if (fparam1 >= fparam2)
				context.Stack.push(StackItem(SYM_INT, 1));
			else
				context.Stack.push(StackItem(SYM_INT, 0));
			break;
		case IsLessEqualI:
			get2Ints(context.Stack, iparam1, iparam2);
			if (iparam1 <= iparam2)
				context.Stack.push(StackItem(SYM_INT, 1));
			else
				context.Stack.push(StackItem(SYM_INT, 0));
			break;
		case IsLessEqualF:
			get2Floats(context.Stack, fparam1, fparam2);
			if (fparam1 <= fparam2)
				context.Stack.push(StackItem(SYM_INT, 1));
			else
				context.Stack.push(StackItem(SYM_INT, 0));
			break;
		case IToF:
			itof(context.Stack, context.CodeBuffer->ReadInt());
			context.InstructionOffset += 4;
			break;
		case FToI:
			ftoi(context.Stack, context.CodeBuffer->ReadInt());
			context.InstructionOffset += 4;
			break;
		case And:
			andi(context.Stack);
			break;
		case Or:
			ori(context.Stack);
			break;
		case Not:
			noti(context.Stack);
			break;
		case GetString:
			if (context.Stack.top().Type != SYM_STRING)
				throw SheepMachineException("Expected string on stack");
			break;
		case DebugBreakpoint:
			throw SheepMachineInstructionException("DebugBreakpoint instruction not supported yet.");
		default:
			throw SheepMachineInstructionException("Unknown instruction");
		}
	}
}

void SheepMachine::executeContextsUntilSuspendedOrFinished()
{
	while(m_contexts.empty() == false)
	{
		printf("Executing... (%d left)\n", m_contexts.size());
		execute(m_contexts.top());

		if (m_contexts.empty() || m_contexts.top().Suspended)
			break;
		
		delete m_contexts.top().FullCode;
		m_contexts.pop();
		printf("Popped context... (%d left)\n", m_contexts.size());
	}

	printf("Executed everything!\n");
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
	for (std::vector<SheepFunction>::iterator itr = machine->m_contexts.top().FullCode->Functions.begin();
		itr != machine->m_contexts.top().FullCode->Functions.end(); itr++)
	{
		if ((*itr).Name == function)
		{
			sheepfunction = &(*itr);
			break;
		}
	}

	if (sheepfunction == NULL)
		throw NoSuchFunctionException(function);

	SheepContext c = machine->m_contexts.top();
	c.CodeBuffer = sheepfunction->Code;
	c.FunctionOffset = sheepfunction->CodeOffset;
	c.InstructionOffset = 0;
	// TODO: this context should share variables with the previous context
	// so that the functions within the same scripts can modify the same global variables
	
	machine->prepareVariables(c);
	machine->m_contexts.push(c);

	machine->execute(machine->m_contexts.top());

	if (machine->m_contexts.top().Suspended == false)
	{
		machine->m_contexts.pop();
	}
}

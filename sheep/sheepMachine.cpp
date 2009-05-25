#include "sheepMachine.h"
#include "sheepCodeBuffer.h"



SheepMachine::SheepMachine()
{
	m_code = NULL;
	m_callback = NULL;
	m_compilerCallback = NULL;
	m_inWaitSection = false;
	m_suspendedInstruction = 0;
}

SheepMachine::~SheepMachine()
{
	if (m_code != NULL)
		delete m_code;
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

void SheepMachine::Prepare(const std::string &script)
{
	if (m_code != NULL)
	{
		delete m_code;
		m_code = NULL;
	}

	SheepCodeTree tree;
	tree.Lock(script, NULL);

	SheepCodeGenerator generator(&tree, &m_imports);
	IntermediateOutput* output = generator.BuildIntermediateOutput();

	m_code = output;

	if (output->Errors.empty() == false && m_compilerCallback)
	{
		for (size_t i = 0; i < output->Errors.size(); i++)
		{
			m_compilerCallback(output->Errors[i].LineNumber, output->Errors[i].Output.c_str());
		}
	}

	prepareVariables();
}

void SheepMachine::Prepare(IntermediateOutput* code)
{
	if (m_code != NULL)
	{
		delete m_code;
		m_code = NULL;
	}

	m_code = code;

	// create the variables
	prepareVariables();
}

void SheepMachine::prepareVariables()
{
	assert(m_code != NULL);

	for (std::vector<SheepSymbol>::iterator itr = m_code->Symbols.begin();
		itr != m_code->Symbols.end(); itr++)
	{
		if ((*itr).Type == SYM_INT)
			m_variables.push_back(StackItem(SYM_INT, (*itr).InitialIntValue));
		else if ((*itr).Type == SYM_FLOAT)
			m_variables.push_back(StackItem(SYM_FLOAT, (*itr).InitialFloatValue));
		else if ((*itr).Type == SYM_STRING)
			m_variables.push_back(StackItem(SYM_STRING, (*itr).InitialStringValue));
		else
			throw SheepMachineException("Unsupported variable type");
	}
}

void SheepMachine::Run(const std::string &function)
{
	if (m_code == NULL)
		throw SheepMachineException("No code to execute.");

	// find the requsted function
	SheepFunction* sheepfunction = NULL;
	for (std::vector<SheepFunction>::iterator itr = m_code->Functions.begin();
		itr != m_code->Functions.end(); itr++)
	{
		if ((*itr).Name == function)
		{
			sheepfunction = &(*itr);
			break;
		}
	}

	if (sheepfunction == NULL)
		throw NoSuchFunctionException(function);

	execute(sheepfunction->Code, m_code->Imports, sheepfunction->CodeOffset);
}
 
int SheepMachine::RunSnippet(const std::string& snippet, int* result)
{
	if (m_code != NULL)
	{
		delete m_code;
		m_code = NULL;
	}

	try
	{
	SheepCodeTree tree;
	tree.Lock(snippet, NULL);

	SheepCodeGenerator generator(&tree, &m_imports);
	m_code = generator.BuildIntermediateOutput();

	if (m_code->Errors.empty() == false)
	{
		if (m_compilerCallback)
		{
			for (std::vector<CompilerOutput>::iterator itr = m_code->Errors.begin();
				itr != m_code->Errors.end(); itr++)
			{
				m_compilerCallback((*itr).LineNumber, (*itr).Output.c_str());
			}
		}

		return SHEEP_ERROR;
	}

	size_t numItemsOnStack = m_currentStack.size();

	if (m_code->Functions.empty())
	{
		return SHEEP_ERROR;
	}

	execute(m_code->Functions[0].Code, m_code->Imports, m_code->Functions[0].CodeOffset);

	int returnValue = 0;
	if (m_currentStack.size() > numItemsOnStack)
	{
		if (result != NULL)
		{
			if (m_currentStack.top().Type == SYM_INT)
				*result = m_currentStack.top().IValue;
			else if (m_currentStack.top().Type == SYM_FLOAT)
				*result = (int)m_currentStack.top().FValue;
		}

		m_currentStack.pop();
	}

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
	if (m_suspended) return SHEEP_ERROR;

	m_suspended = true;
	return SHEEP_SUCCESS;
}

int SheepMachine::Resume()
{
	if (!m_suspended) return SHEEP_ERROR;

	try
	{
		m_suspended = false;
		execute(m_suspendedCodeBuffer, m_code->Imports, m_suspendedCodeOffset, m_suspendedInstruction);

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

void SheepMachine::execute(SheepCodeBuffer* code, std::vector<SheepImport>& imports,
	unsigned int offset, unsigned int firstInstruction)
{
	// HACK: originally there was a parameter called 'stack', but it was removed!
	// Go back and fix this right!
	SheepStack& stack = m_currentStack;

	unsigned int nextInstruction = firstInstruction;
	SheepStack::size_type numItemsOnStack = stack.size();

	code->SeekFromStart(nextInstruction);
	while(!m_suspended && code->Tell() < code->GetSize())
	{
		if (nextInstruction != code->Tell())
			code->SeekFromStart(nextInstruction);

		unsigned char instruction = code->ReadByte();
		nextInstruction++;

		int iparam1, iparam2;
		float fparam1, fparam2;

		switch(instruction)
		{
		case SitnSpin:
			break;
		case Yield:
			throw SheepMachineInstructionException("Yield instruction not supported yet.");
		case CallSysFunctionV:
			callVoidFunction(stack, imports, code->ReadInt());
			nextInstruction += 4;
			break;
		case CallSysFunctionI:
			callIntFunction(stack, imports, code->ReadInt());
			nextInstruction += 4;
			break;
		case CallSysFunctionF:
		case CallSysFunctionS:
			throw SheepMachineInstructionException("Function calling not supported yet.");
		case Branch:
		case BranchGoto:
			nextInstruction = code->ReadInt() - offset;
			break;
		case BranchIfZero:
			if (stack.top().Type == SYM_INT)
			{
				if (stack.top().IValue == 0)
					nextInstruction = code->ReadInt() - offset;
				else
				{
					code->ReadInt(); // throw it away
					nextInstruction += 4;
				}
				stack.pop();
			}
			else
			{
				throw SheepMachineException("Expected integer on stack");
			}
			break;
		case BeginWait:
			m_inWaitSection = true;
			break;
		case EndWait:
			m_inWaitSection = false;
			if (m_endWaitCallback) m_endWaitCallback();
			break;
		case ReturnV:
			return;
		case StoreI:
			storeI(stack, m_variables, code->ReadInt());
			nextInstruction += 4;
			break;
		case StoreF:
			storeF(stack, m_variables, code->ReadInt());
			nextInstruction += 4;
			break;
		case StoreS:
			storeS(stack, m_variables, code->ReadInt());
			nextInstruction += 4;
			break;
		case LoadI:
			loadI(stack, m_variables, code->ReadInt());
			nextInstruction += 4;
			break;
		case LoadF:
			loadF(stack, m_variables, code->ReadInt());
			nextInstruction += 4;
			break;
		case LoadS:
			throw SheepMachineInstructionException("Loading string variables not supported yet.");
		case PushI:
			stack.push(StackItem(SYM_INT, code->ReadInt()));
			nextInstruction += 4;
			break;
		case PushF:
			stack.push(StackItem(SYM_FLOAT, code->ReadFloat()));
			nextInstruction += 4;
			break;
		case PushS:
			stack.push(StackItem(SYM_STRING, code->ReadInt()));
			nextInstruction += 4;
			break;
		case Pop:
			stack.pop();
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
			get2Ints(stack, iparam1, iparam2);
			if (iparam1 == iparam2)
				stack.push(StackItem(SYM_INT, 1));
			else
				stack.push(StackItem(SYM_INT, 0));
			break;
		case IsEqualF:
			get2Floats(stack, fparam1, fparam2);
			if (fparam1 == fparam2)
				stack.push(StackItem(SYM_INT, 1));
			else
				stack.push(StackItem(SYM_INT, 0));
			break;
		case NotEqualI:
			get2Ints(stack, iparam1, iparam2);
			if (iparam1 != iparam2)
				stack.push(StackItem(SYM_INT, 1));
			else
				stack.push(StackItem(SYM_INT, 0));
			break;
		case NotEqualF:
			get2Floats(stack, fparam1, fparam2);
			if (fparam1 != fparam2)
				stack.push(StackItem(SYM_INT, 1));
			else
				stack.push(StackItem(SYM_INT, 0));
			break;
		case IsGreaterI:
			get2Ints(stack, iparam1, iparam2);
			if (iparam1 > iparam2)
				stack.push(StackItem(SYM_INT, 1));
			else
				stack.push(StackItem(SYM_INT, 0));
			break;
		case IsGreaterF:
			get2Floats(stack, fparam1, fparam2);
			if (fparam1 > fparam2)
				stack.push(StackItem(SYM_INT, 1));
			else
				stack.push(StackItem(SYM_INT, 0));
			break;
		case IsLessI:
			get2Ints(stack, iparam1, iparam2);
			if (iparam1 < iparam2)
				stack.push(StackItem(SYM_INT, 1));
			else
				stack.push(StackItem(SYM_INT, 0));
			break;
		case IsLessF:
			get2Floats(stack, fparam1, fparam2);
			if (fparam1 < fparam2)
				stack.push(StackItem(SYM_INT, 1));
			else
				stack.push(StackItem(SYM_INT, 0));
			break;
		case IsGreaterEqualI:
			get2Ints(stack, iparam1, iparam2);
			if (iparam1 >= iparam2)
				stack.push(StackItem(SYM_INT, 1));
			else
				stack.push(StackItem(SYM_INT, 0));
			break;
		case IsGreaterEqualF:
			get2Floats(stack, fparam1, fparam2);
			if (fparam1 >= fparam2)
				stack.push(StackItem(SYM_INT, 1));
			else
				stack.push(StackItem(SYM_INT, 0));
			break;
		case IsLessEqualI:
			get2Ints(stack, iparam1, iparam2);
			if (iparam1 <= iparam2)
				stack.push(StackItem(SYM_INT, 1));
			else
				stack.push(StackItem(SYM_INT, 0));
			break;
		case IsLessEqualF:
			get2Floats(stack, fparam1, fparam2);
			if (fparam1 <= fparam2)
				stack.push(StackItem(SYM_INT, 1));
			else
				stack.push(StackItem(SYM_INT, 0));
			break;
		case IToF:
			itof(stack, code->ReadInt());
			nextInstruction += 4;
			break;
		case FToI:
			ftoi(stack, code->ReadInt());
			nextInstruction += 4;
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
			if (stack.top().Type != SYM_STRING)
				throw SheepMachineException("Expected string on stack");
			break;
		case DebugBreakpoint:
			throw SheepMachineInstructionException("DebugBreakpoint instruction not supported yet.");
		default:
			throw SheepMachineInstructionException("Unknown instruction");
		}
	}

	// if we were suspended then remember where we were so we can pick it up later
	if (m_suspended)
	{
		m_suspendedInstruction = nextInstruction;
		m_suspendedCodeBuffer = code;
		m_suspendedCodeOffset = offset;
	}
}

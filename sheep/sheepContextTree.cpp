#include "sheepContextTree.h"
#include "sheepCodeGenerator.h"
#include "sheepMachine.h"

SheepContext::~SheepContext()
{
	m_function->ParentCode->Release();

	if (m_ownStackAndVariables)
	{
		delete m_variables;
		delete m_stack;
	}
}

void SheepContext::PrepareVariables()
{
	assert(m_function != nullptr);
	assert(m_function->ParentCode != nullptr);

	IntermediateOutput* fullCode = m_function->ParentCode;

	for (std::vector<SheepSymbol>::iterator itr = fullCode->Symbols.begin();
		itr != fullCode->Symbols.end(); itr++)
	{
		if ((*itr).Type == SheepSymbolType::Int)
			m_variables->push_back(StackItem(SheepSymbolType::Int, (*itr).InitialIntValue));
		else if ((*itr).Type == SheepSymbolType::Float)
			m_variables->push_back(StackItem(SheepSymbolType::Float, (*itr).InitialFloatValue));
		else if ((*itr).Type == SheepSymbolType::String)
			m_variables->push_back(StackItem(SheepSymbolType::String, (*itr).InitialStringValue));
		else
			throw SheepMachineException("Unsupported variable type");
	}


}

void SheepContext::Release()
{
	m_refCount--;

	if (m_refCount <= 0)
	{
		delete this;
	}
}

int SheepContext::Execute()
{
	if (Dead == true || 
		(m_state != Sheep::ExecutionContextState::Prepared && m_state != Sheep::ExecutionContextState::Suspended))
		return SHEEP_ERR_INVALID_OPERATION;

	m_state = Sheep::ExecutionContextState::Executing;
	UserSuspended = false;

	if (ChildSuspended == false ||
		AreAnyChildrenSuspended() == false)
	{
		// children are obviously done
		ChildSuspended = false;

		// not waiting on anything, so run some code
		m_parentVM->Execute(this);

		if (ChildSuspended == false &&
			UserSuspended == false)
		{
			// before we can kill the context we need
			// to check its ancestors, since one of them
			// may have been waiting on this child to finish
			SheepContext* parent = Parent;
			while(parent != NULL)
			{
				if (parent->Dead == false &&
					parent->ChildSuspended == true &&
					parent->UserSuspended == false &&
					parent->AreAnyChildrenSuspended() == false)
				{
					parent->ChildSuspended = false;
					parent->Execute();

					// no need to continue the loop, since the
					// Resume() call we just made will handle the
					// rest of the ancestors
					break;
				}

				parent = parent->Parent;
			}

			m_state = Sheep::ExecutionContextState::Finished;
			return SHEEP_SUCCESS;
		}
		else
		{
			m_state = Sheep::ExecutionContextState::Suspended;
			return SHEEP_SUSPENDED;
		}
	}
	else
	{
		m_state = Sheep::ExecutionContextState::Suspended;
		return SHEEP_SUSPENDED;
	}
}

int SheepContext::Suspend()
{
	if (m_state != Sheep::ExecutionContextState::Executing)
		return SHEEP_ERR_INVALID_OPERATION;

	UserSuspended = true;
	m_state = Sheep::ExecutionContextState::Suspended;

	return SHEEP_SUCCESS;
}

Sheep::IVirtualMachine* SheepContext::GetParentVirtualMachine()
{
	return m_parentVM;
}

const char* SheepContext::GetVariableName(int index)
{
	if (index < 0 || index >= m_function->ParentCode->Symbols.size())
		return nullptr;

	m_function->ParentCode->Symbols[index].Name.c_str();
}

int SheepContext::PopStringFromStack(const char** result)
{
	if (m_stack->empty())
		return SHEEP_ERR_EMPTY_STACK;

	StackItem item = m_stack->top();
	
	if (item.Type != SheepSymbolType::String)
		return SHEEP_ERR_WRONG_TYPE_ON_STACK;

	m_stack->pop();

	if (result != nullptr)
	{
		IntermediateOutput* code = m_function->ParentCode;
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

void SheepContext::init(SheepFunction* function)
{
	assert(function != nullptr);

	m_refCount = 0;
	m_variables = nullptr;
	m_stack = nullptr;
	m_ownStackAndVariables = false;

	assert(function->ParentCode != nullptr);
	m_function = function;
	m_function->ParentCode->AddRef();

	InWaitSection = false;
	UserSuspended = false;
	ChildSuspended = false;
	InstructionOffset = 0;
	m_parentVM = nullptr;

	Parent = NULL;
	FirstChild = NULL;
	Sibling = NULL;

	Dead = false;
	m_state = Sheep::ExecutionContextState::Prepared;
}

SheepContextTree::~SheepContextTree()
{
	// delete ALL the child contexts, whether they've been released or not
	deleteContextAndChildren(m_parentContext);
}

#include "sheepContextTree.h"
#include "sheepCodeGenerator.h"
#include "sheepMachine.h"

void SheepContext::PrepareVariables()
{
	assert(FullCode != NULL);

	for (std::vector<SheepSymbol>::iterator itr = FullCode->Symbols.begin();
		itr != FullCode->Symbols.end(); itr++)
	{
		if ((*itr).Type == SheepSymbolType::Int)
			m_variables.push_back(StackItem(SheepSymbolType::Int, (*itr).InitialIntValue));
		else if ((*itr).Type == SheepSymbolType::Float)
			m_variables.push_back(StackItem(SheepSymbolType::Float, (*itr).InitialFloatValue));
		else if ((*itr).Type == SheepSymbolType::String)
			m_variables.push_back(StackItem(SheepSymbolType::String, (*itr).InitialStringValue));
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

const char* SheepContext::GetVariableName(int index)
{
	if (index < 0 || index >= FullCode->Symbols.size())
		return nullptr;

	FullCode->Symbols[index].Name.c_str();
}

SheepContextTree::~SheepContextTree()
{
	// delete ALL the child contexts, whether they've been released or not
	deleteContextAndChildren(m_parentContext);
}

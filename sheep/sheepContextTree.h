#ifndef SHEEPCONTEXTTREE_H
#define SHEEPCONTEXTTREE_H

#include <cassert>
#include <stack>
#include "sheepTypes.h"
#include "sheepMemoryAllocator.h"
#include "sheepcpp.h"

struct StackItem
{
	StackItem()
	{
		Type = SheepSymbolType::Void;
		IValue = 0;
	}

	StackItem(SheepSymbolType type, int value)
	{
		Type = type;
		IValue = value;
	}

	StackItem(SheepSymbolType type, float value)
	{
		Type = type;
		FValue = value;
	}

	SheepSymbolType Type;

	union
	{
		int IValue;
		float FValue;
	};
};

typedef std::stack<StackItem> SheepStack;

class IntermediateOutput;
class SheepMachine;

class SheepContext : public Sheep::IExecutionContext
{
	int m_refCount;
	std::vector<StackItem>* m_variables;
	SheepStack* m_stack;
	bool m_ownStackAndVariables;
	std::vector<StackItem> m_parameterVariables;
	std::vector<StackItem> m_localVariables;
	Sheep::ExecutionContextState m_state;
	SheepMachine* m_parentVM;
	SheepFunction* m_function;

public:
	SheepContext(SheepMachine* parentVM, SheepFunction* function)
	{
		init(function);

		m_variables = new std::vector<StackItem>();
		m_stack = new SheepStack();
		m_ownStackAndVariables = true;

		m_parentVM = parentVM;
	}

	SheepContext(SheepContext* parent, SheepFunction* function)
	{
		init(function);

		m_variables = parent->m_variables;
		m_stack = parent->m_stack;
		m_ownStackAndVariables = false;

		m_parentVM = parent->m_parentVM;
	}

	virtual ~SheepContext();

	SheepStack* GetStack() { return m_stack; }
	SheepFunction* GetFunction() { return m_function; }
	
	bool InWaitSection;
	bool UserSuspended;
	bool ChildSuspended;
	unsigned int InstructionOffset;

	SheepContext* Parent;
	SheepContext* FirstChild;
	SheepContext* Sibling;

	bool Dead;

	bool AreAnyChildrenSuspended()
	{
		SheepContext* child = FirstChild;
		while(child != NULL)
		{
			if (child->UserSuspended || child->ChildSuspended)
				return true;

			child = child->Sibling;
		}

		return false;
	}

	void PrepareVariables();

	void Aquire() { m_refCount++; }
	void Release() override;

	int Execute() override;
	int Suspend() override;

	Sheep::ExecutionContextState GetState() override { return m_state; }

	Sheep::IVirtualMachine* GetParentVirtualMachine() override;

	bool IsInWaitSection() override { return InWaitSection; }

	int GetNumVariables() override
	{
		return (int)m_variables->size();
	}

	const char* GetVariableName(int index) override;

	Sheep::SymbolType GetVariableType(int index) override
	{
		if (index < 0 || index >= m_variables->size())
			return Sheep::SymbolType::Void;

		StackItem& item = (*m_variables)[index];

		return static_cast<Sheep::SymbolType>(item.Type);
	}

	int SetVariableInt(int index, int value) override
	{
		if (index < 0 || index >= m_variables->size())
			return SHEEP_ERR_INVALID_ARGUMENT;

		StackItem& item = (*m_variables)[index];

		if (item.Type != SheepSymbolType::Int)
			return SHEEP_ERR_VARIABLE_INCORRECT_TYPE;

		item.IValue = value;

		return SHEEP_SUCCESS;
	}

	int SetVariableFloat(int index, float value) override
	{
		if (index < 0 || index >= m_variables->size())
			return SHEEP_ERR_INVALID_ARGUMENT;

		StackItem& item = (*m_variables)[index];

		if (item.Type != SheepSymbolType::Float)
			return SHEEP_ERR_VARIABLE_INCORRECT_TYPE;

		item.FValue = value;

		return SHEEP_SUCCESS;
	}

	int SetVariableString(int index, const char* value) override
	{
		if (index < 0 || index >= m_variables->size())
			return SHEEP_ERR_INVALID_ARGUMENT;

		StackItem& item = (*m_variables)[index];

		if (item.Type != SheepSymbolType::String)
			return SHEEP_ERR_VARIABLE_INCORRECT_TYPE;

		// TODO
		return SHEEP_ERROR;
	}

	int SetVariableStringIndex(int index, int value)
	{
		if (index < 0 || index >= m_variables->size())
			return SHEEP_ERR_INVALID_ARGUMENT;

		StackItem& item = (*m_variables)[index];

		if (item.Type != SheepSymbolType::String)
			return SHEEP_ERR_VARIABLE_INCORRECT_TYPE;

		item.IValue = value;

		return SHEEP_SUCCESS;
	}

	int GetVariableInt(int index, int* result) override
	{
		if (index < 0 || index >= m_variables->size())
			return SHEEP_ERR_INVALID_ARGUMENT;
		if (result == nullptr)
			return SHEEP_ERR_INVALID_ARGUMENT;

		StackItem& item = (*m_variables)[index];

		if (item.Type != SheepSymbolType::Int)
			return SHEEP_ERR_VARIABLE_INCORRECT_TYPE;

		*result = item.IValue;

		return SHEEP_SUCCESS;
	}

	int GetVariableFloat(int index, float* result) override
	{
		if (index < 0 || index >= m_variables->size())
			return SHEEP_ERR_INVALID_ARGUMENT;
		if (result == nullptr)
			return SHEEP_ERR_INVALID_ARGUMENT;

		StackItem& item = (*m_variables)[index];

		if (item.Type != SheepSymbolType::Float)
			return SHEEP_ERR_VARIABLE_INCORRECT_TYPE;

		*result = item.FValue;

		return SHEEP_SUCCESS;
	}

	int GetVariableString(int index, const char** result) override
	{
		if (index < 0 || index >= m_variables->size())
			return SHEEP_ERR_INVALID_ARGUMENT;
		if (result == nullptr)
			return SHEEP_ERR_INVALID_ARGUMENT;

		StackItem& item = (*m_variables)[index];

		if (item.Type != SheepSymbolType::String)
			return SHEEP_ERR_VARIABLE_INCORRECT_TYPE;

		// TODO
		return SHEEP_ERROR;
	}

	int GetVariableStringIndex(int index, int* result)
	{
		if (index < 0 || index >= m_variables->size())
			return SHEEP_ERR_INVALID_ARGUMENT;
		if (result == nullptr)
			return SHEEP_ERR_INVALID_ARGUMENT;

		StackItem& item = (*m_variables)[index];

		if (item.Type != SheepSymbolType::String)
			return SHEEP_ERR_VARIABLE_INCORRECT_TYPE;

		*result = item.IValue;

		// TODO
		return SHEEP_ERROR;
	}

	int SetParamVariableInt(int index, int value)
	{
		if (index < 0 || index >= m_parameterVariables.size())
			return SHEEP_ERR_INVALID_ARGUMENT;

		StackItem& item = m_parameterVariables[index];

		if (item.Type != SheepSymbolType::Int)
			return SHEEP_ERR_VARIABLE_INCORRECT_TYPE;

		item.IValue = value;

		return SHEEP_SUCCESS;
	}

	int SetParamVariableFloat(int index, int value)
	{
		if (index < 0 || index >= m_parameterVariables.size())
			return SHEEP_ERR_INVALID_ARGUMENT;

		StackItem& item = m_parameterVariables[index];

		if (item.Type != SheepSymbolType::Float)
			return SHEEP_ERR_VARIABLE_INCORRECT_TYPE;

		item.FValue = value;

		return SHEEP_SUCCESS;
	}

	int GetParamVariableInt(int index, int* result)
	{
		if (index < 0 || index >= m_parameterVariables.size())
			return SHEEP_ERR_INVALID_ARGUMENT;
		if (result == nullptr)
			return SHEEP_ERR_INVALID_ARGUMENT;

		StackItem& item = m_parameterVariables[index];

		if (item.Type != SheepSymbolType::Int)
			return SHEEP_ERR_VARIABLE_INCORRECT_TYPE;

		*result = item.IValue;

		return SHEEP_SUCCESS;
	}

	int GetParamVariableFloat(int index, float* result)
	{
		if (index < 0 || index >= m_parameterVariables.size())
			return SHEEP_ERR_INVALID_ARGUMENT;
		if (result == nullptr)
			return SHEEP_ERR_INVALID_ARGUMENT;

		StackItem& item = m_parameterVariables[index];

		if (item.Type != SheepSymbolType::Float)
			return SHEEP_ERR_VARIABLE_INCORRECT_TYPE;

		*result = item.FValue;

		return SHEEP_SUCCESS;
	}

	int PopIntFromStack(int* result) override
	{
		if (m_stack->empty())
			return SHEEP_ERR_EMPTY_STACK;

		StackItem item = m_stack->top();
		
		if (item.Type != SheepSymbolType::Int)
			return SHEEP_ERR_WRONG_TYPE_ON_STACK;

		m_stack->pop();

		if (result != nullptr)
			*result = item.IValue;

		return SHEEP_SUCCESS;
	}

	int PopFloatFromStack(float* result) override
	{
		if (m_stack->empty())
			return SHEEP_ERR_EMPTY_STACK;

		StackItem item = m_stack->top();

		if (item.Type != SheepSymbolType::Float)
			return SHEEP_ERR_WRONG_TYPE_ON_STACK;
		
		m_stack->pop();

		if (result != nullptr)
			*result = item.FValue;

		return SHEEP_SUCCESS;
	}

	int PopStringFromStack(const char** result) override;

	int PushIntOntoStack(int i) override
	{
		m_stack->push(StackItem(SheepSymbolType::Int, i));

		return SHEEP_SUCCESS;
	}
	
	int PushFloatOntoStack(float f) override
	{
		m_stack->push(StackItem(SheepSymbolType::Float, f));

		return SHEEP_SUCCESS;
	}

	int PushStringOntoStack(int value)
	{
		m_stack->push(StackItem(SheepSymbolType::String, value));

		return SHEEP_SUCCESS;
	}

private:
	void init(SheepFunction* function);
};

class SheepContextTree
{
	SheepContext* m_parentContext;
	SheepContext* m_currentContext;

public:

	SheepContextTree()
	{
		m_parentContext = nullptr;
		m_currentContext = nullptr;
	}

	~SheepContextTree();

	SheepContext* GetCurrent() { return m_currentContext; }
	void SetCurrent(SheepContext* context)
	{ 
		assert(isContextInTree(context, m_parentContext)); 
		m_currentContext = context; 
	}

	void Add(SheepContext* context)
	{
		assert(context->Parent == NULL);
		assert(context->FirstChild == NULL);
		assert(context->Sibling == NULL);

		// Aquire() a reference to the context so it won't be deleted until we Release() it
		context->Aquire();

		if (m_parentContext == NULL)
		{
			// tree doesn't exist yet, so make this context the root
			m_parentContext = context;
		}
		else if (m_currentContext == NULL)
		{
			addAsSibling(m_parentContext, context);
		}
		else
		{
			if (m_currentContext->FirstChild == NULL)
			{
				m_currentContext->FirstChild = context;
				context->Parent = m_currentContext;
			}
			else
			{
				addAsSibling(m_currentContext->FirstChild, context);
			}
		}

	#ifndef NDEBUG
		if (m_parentContext)
			validateContextTree(m_parentContext);
	#endif
	}

	void KillContext(SheepContext* context)
	{
		assert(context != NULL);
		assert(context->Dead == false);

		// mark the context as dead
		context->Dead = true;

		// current context should never point to a dead context
		if (m_currentContext == context)
			m_currentContext = NULL;

		// work the way up the tree, removing all dead contexts
		// and their descendants
		// (this context could have been the last alive descendant
		// in a whole family of dead contexts)
		SheepContext* itr = context;
		while(itr != NULL)
		{
			SheepContext* parent = itr->Parent;

			// check the context's descendant to see if they're all dead
			if (itr->Dead && areAllContextDecendantsDead(itr))
			{
				// all descendant are dead, so this context (and its children)
				// are safe to remove
				Remove(itr);
			}
			else
			{
				// found an ancestor with alive descendants,
				// which cannot be removed, so we can stop now
				break;
			}

			itr = parent;
		}
	}

	void Remove(SheepContext* context)
	{
		assert(context != NULL);
		assert(m_parentContext != NULL);

		// find the context's prior sibling
		SheepContext* priorSibling = NULL;
		if (context->Parent == NULL)
			priorSibling = m_parentContext;
		else
			priorSibling = context->Parent->FirstChild;

		// loop until we find the context's prior sibling
		SheepContext* itr = priorSibling, *prev = NULL;
		while(itr != NULL)
		{
			// have we found what we're looking for?
			if (itr == context)
			{
				if (prev == NULL)
				{
					if (context->Parent == NULL)
					{
						// this is the root
						assert(m_parentContext == context);
						m_parentContext = context->Sibling;
					}
					else
					{
						// this was the parent's first child
						assert(context->Parent != NULL);
						assert(context->Parent->FirstChild == context);
						context->Parent->FirstChild = context->Sibling;
					}
				}
				else
				{
					// just an ordinary sibling
					prev->Sibling = context->Sibling;
				}

				// context has now been removed from the tree,
				// so we can delete it and all its children
				SHEEP_DELETE(context);
				
				// mission accomplished!
				return;
			}
			else
			{
				// we haven't found the context's prior sibling, so keep looking
				prev = itr;
				itr = itr->Sibling;
			}
		}

		// we should NEVER end up here!
		assert(false);
	}

	
private:

	void deleteContextAndChildren(SheepContext* context)
	{
		if (context != nullptr)
		{
			SheepContext* sibling = context->Sibling;

			deleteContextAndChildren(sibling);

			deleteContextAndChildren(context->FirstChild);

			delete context;
		}
	}

	static void addAsSibling(SheepContext* child, SheepContext* toAdd)
	{
		SheepContext* itr = child;
		while(itr->Sibling != NULL)
		{
			itr = itr->Sibling;
		}

		itr->Sibling = toAdd;
		toAdd->Parent = child->Parent;
	}


	static void updateChildrensParent(SheepContext* firstChild, SheepContext* newParent)
	{
		SheepContext* itr = firstChild;
		while(itr->Sibling != NULL)
		{
			itr->Parent = newParent;
			itr = itr->Sibling;
		}

		itr = newParent->FirstChild;
		while(itr->Sibling != NULL)
		{
			itr = itr->Sibling;
		}
		itr->Sibling = firstChild;
	}

	static void validateContextTree(SheepContext* context)
	{
		SheepContext* itr = context->FirstChild;
		while(itr != NULL)
		{
			assert(itr->Parent == context);
			assert(itr->FirstChild != context->FirstChild);

			itr = itr->Sibling;
		}
	}

	static bool areAllContextDecendantsDead(SheepContext* context)
	{
		SheepContext* itr = context->FirstChild;

		while(itr != NULL)
		{
			if (itr->Dead == false)
				return false;

			if (areAllContextDecendantsDead(itr) == false)
				return false;

			itr = itr->Sibling;
		}

		return true;
	}
	static bool isContextInTree(SheepContext* context, SheepContext* root)
	{
		SheepContext* itr = root;

		while(itr != NULL)
		{
			if (itr == context)
				return true;

			if (isContextInTree(context, itr->FirstChild))
				return true;

			itr = itr->Sibling;
		}

		return false;
	}
};



#endif // SHEEPCONTEXTTREE_H

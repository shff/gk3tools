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
class SheepContextTree;

class SheepContext : public Sheep::IExecutionContext
{
	int m_refCount;
	bool m_dead;

	std::vector<StackItem>* m_variables;
	SheepStack* m_stack;
	bool m_ownStackAndVariables;
	std::vector<StackItem> m_parameterVariables;
	std::vector<StackItem> m_localVariables;
	Sheep::ExecutionContextState m_state;
	SheepMachine* m_parentVM;
	SheepFunction* m_function;
	SheepContextTree* m_parentTree;

public:
	SheepContext(SheepContextTree* parentTree, SheepMachine* parentVM, SheepFunction* function)
	{
		init(parentTree, function);

		m_variables = new std::vector<StackItem>();
		m_stack = new SheepStack();
		m_ownStackAndVariables = true;

		m_parentVM = parentVM;
	}

	SheepContext(SheepContextTree* parentTree, SheepContext* parent, SheepFunction* function)
	{
		init(parentTree, function);

		m_variables = parent->m_variables;
		m_stack = parent->m_stack;
		m_ownStackAndVariables = false;

		m_parentVM = parent->m_parentVM;
	}

	virtual ~SheepContext();

	SheepStack* GetStack() { return m_stack; }
	SheepFunction* GetFunction() { return m_function; }
	bool IsDead() { return m_dead; }
	
	bool InWaitSection;
	bool UserSuspended;
	bool ChildSuspended;
	unsigned int InstructionOffset;

	SheepContext* Parent;
	SheepContext* FirstChild;
	SheepContext* Sibling;

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

	void AddAsSibling(SheepContext* newSibling)
	{
		SheepContext* toAdd = this;

		while(toAdd->Sibling != nullptr)
			toAdd = toAdd->Sibling;

		toAdd->Sibling = newSibling;
		newSibling->Parent = toAdd->Parent;
	}

	void AddAsChild(SheepContext* context)
	{
		if (FirstChild == nullptr)
		{
			FirstChild = context;
			context->Parent = this;
		}
		else
			FirstChild->AddAsSibling(context);
	}

private:
	void init(SheepContextTree* parentTree, SheepFunction* function);
};

class SheepContextTree
{
	SheepContext* m_parentContext;

public:

	SheepContextTree()
	{
		m_parentContext = nullptr;
	}

	~SheepContextTree();

	SheepContext* Create(SheepContext* parent, SheepFunction* function)
	{
		SheepContext* newContext = SHEEP_NEW SheepContext(this, parent, function);
		newContext->Aquire();

		if (parent == nullptr)
		{
			if (m_parentContext == nullptr)
				m_parentContext = newContext;
			else
				m_parentContext->AddAsSibling(newContext);
		}
		else
		{
			parent->AddAsChild(newContext);
		}

#ifndef NDEBUG
			validateContextTree(m_parentContext);
#endif

		return newContext;
	}

	SheepContext* Create(SheepMachine* parentVM, SheepFunction* function)
	{
		SheepContext* newContext = SHEEP_NEW SheepContext(this, parentVM, function);
		newContext->Aquire();

		if (m_parentContext == nullptr)
			m_parentContext = newContext;
		else
			m_parentContext->AddAsSibling(newContext);
		
#ifndef NDEBUG
			validateContextTree(m_parentContext);
#endif

		return newContext;
	}

	void TrimTree()
	{
		// trimming the tree means starting at the top of the tree
		// and working down to the leafs. Delete dead leaves. Then
		// back up to the parents. Are they now dead leaves? Delete them.
		// Repeat.

		TrimTree(m_parentContext);
	}

	void TrimTree(SheepContext* context)
	{
		SheepContext* prev = nullptr;
		while(context != nullptr)
		{
			TrimTree(context->FirstChild);

			if (context->IsDead() && context->FirstChild == nullptr)
			{
				if (m_parentContext == context)
					m_parentContext = context->Sibling;

				if (prev != nullptr)
					prev->Sibling = context->Sibling;
				if (context->Parent != nullptr && context->Parent->FirstChild == context)
					context->Parent->FirstChild = context->Sibling;

				SHEEP_DELETE(context);
			}

			prev = context;
			context = context->Sibling;
		}
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

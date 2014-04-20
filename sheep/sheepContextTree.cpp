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

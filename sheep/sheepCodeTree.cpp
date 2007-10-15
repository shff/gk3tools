#include <cassert>
#include "sheepCodeTree.h"
#include "sheepScanner.h"
#include "sheepLog.h"

SheepCodeTreeNode* g_codeTree = NULL;

SheepCodeTree::SheepCodeTree()
{
	m_locked = false;
	m_log = NULL;
}

void SheepCodeTree::Lock(const std::string& script, SheepLog* log)
{
	if (m_locked == true)
		throw SheepException("Sheep code tree already locked");
	
	m_locked = true;
	m_log = log;
	
	YY_BUFFER_STATE buffer = yy_scan_string(script.c_str());
	if (buffer != NULL)
	{
		yyparse();
		yy_delete_buffer(buffer);
	}
}

void SheepCodeTree::Unlock()
{
	m_locked = false;
	m_log = NULL;
}

void SheepCodeTree::Print()
{
	if (m_locked == true && g_codeTree != NULL)
		g_codeTree->Print(0);
}

SheepCodeTreeNode::SheepCodeTreeNode(CodeTreeNodeType type, int lineNumber)
{
	m_type = type;
	m_lineNumber = lineNumber;
	
	m_parent = NULL;
	m_sibling = NULL;

	for (int i = 0; i < NUM_CHILD_NODES; i++)
		m_children[i] = NULL;
}

SheepCodeTreeNode* SheepCodeTreeNode::CreateDeclaration(CodeTreeDeclarationNodeType type, int lineNumber)
{
	SheepCodeTreeNode* node = new SheepCodeTreeDeclarationNode(type, lineNumber);
	
	return node;
}

SheepCodeTreeNode* SheepCodeTreeNode::CreateIntegerConstant(int value, int lineNumber)
{
	return new SheepCodeTreeIntegerConstantNode(value, lineNumber);
}

SheepCodeTreeNode* SheepCodeTreeNode::CreateFloatConstant(float value, int lineNumber)
{
	return new SheepCodeTreeFloatConstantNode(value, lineNumber);
}

SheepCodeTreeNode* SheepCodeTreeNode::CreateIdentifierReference(const std::string& name, bool global, int lineNumber)
{
	return new SheepCodeTreeIdentifierReferenceNode(name, global, lineNumber);
}

void SheepCodeTreeNode::AttachSibling(SheepCodeTreeNode* sibling)
{
	SheepCodeTreeNode* t = this;
	
	while (t->m_sibling != NULL)
	{
		t = t->m_sibling;
	}
	
	t->m_sibling = sibling;
	
	if (sibling != NULL) sibling->m_parent = t;
}

void SheepCodeTreeNode::SetChild(int index, SheepCodeTreeNode* node)
{
	assert(index >= 0 && index < NUM_CHILD_NODES);
	
	m_children[index] = node;
}

void SheepCodeTreeNode::Print(int indent)
{
	PrintIndents(indent);
	
	PrintData();
	
	// print the children
	for (int i = 0; i < NUM_CHILD_NODES; i++)
		if (m_children[i] != NULL)
			m_children[i]->Print(indent + 2);
	
	// tell the sibling to print
	if (m_sibling)
		m_sibling->Print(indent);
}

void SheepCodeTreeNode::PrintIndents(int indent)
{
	for (int i = 0; i < indent; i++)
		printf(" ");
}

void SheepCodeTreeNode::PrintData()
{
	printf("Regular ol' node\n");
}

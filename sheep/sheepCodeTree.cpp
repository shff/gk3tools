#include <cassert>
#include "sheepCodeTree.h"
#include "sheepScanner.h"
#include "sheepLog.h"
#include "sheepException.h"

SheepCodeTreeNode* g_codeTreeRoot = NULL;
SheepCodeTree* g_codeTree = NULL;

SheepCodeTree::SheepCodeTree()
{
	m_locked = false;
	m_log = NULL;

	m_nextStringConstantOffset = 0;
}

const SheepCodeTreeNode* SheepCodeTree::GetCodeTree() const
{
	assert(m_locked);
	return g_codeTreeRoot;
}


SheepCodeTreeNode* SheepCodeTree::GetCodeTree()
{
	assert(m_locked);
	return g_codeTreeRoot;
}

void SheepCodeTree::Lock(const std::string& script, SheepLog* log)
{
	if (m_locked == true)
		throw SheepException("Sheep code tree already locked");
	
	m_locked = true;
	m_log = log;
	g_codeTree = this;
	m_stringConstants.clear();

	// the real GK3 compiler seems to do this too
	AddStringConstant("");
	
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
	delete g_codeTreeRoot;
	g_codeTreeRoot = NULL;
	g_codeTree = NULL;
}

void SheepCodeTree::Print()
{
	if (m_locked == true && g_codeTreeRoot != NULL)
		g_codeTreeRoot->Print(0);
}

int SheepCodeTree::AddStringConstant(const std::string& value)
{
	std::map<std::string, StringConst>::iterator itr = m_stringConstants.find(value);

	// if the constant doesn't exist then add it
	if (itr == m_stringConstants.end())
	{
		StringConst str;
		str.Offset = m_nextStringConstantOffset;
		str.Value = value;
		m_stringConstants.insert(std::pair<std::string, StringConst>(value, str));
		
		m_nextStringConstantOffset += value.length() + 1;
		return str.Offset;
	}

	// constant already exists, so just return the existing offset
	return (*itr).second.Offset;
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

SheepCodeTreeNode::~SheepCodeTreeNode()
{
	// delete my siblings (be careful that previous siblings get deleted too!)
	if (m_sibling != NULL)
		delete m_sibling;
	
	// delete my children
	for (int i = 0; i < NUM_CHILD_NODES; i++)
		if (m_children[i] != NULL)
			delete m_children[i];
}

SheepCodeTreeNode* SheepCodeTreeNode::CreateSymbolSection(int lineNumber)
{
	return new SheepCodeTreeSectionNode(SECTIONTYPE_SYMBOLS, lineNumber);
}

SheepCodeTreeNode* SheepCodeTreeNode::CreateCodeSection(int lineNumber)
{
	return new SheepCodeTreeSectionNode(SECTIONTYPE_CODE, lineNumber);
}

SheepCodeTreeNode* SheepCodeTreeNode::CreateSnippet(int lineNumber)
{
	return new SheepCodeTreeSectionNode(SECTIONTYPE_SNIPPET, lineNumber);
}

SheepCodeTreeNode* SheepCodeTreeNode::CreateDeclaration(CodeTreeDeclarationNodeType type, int lineNumber)
{
	SheepCodeTreeNode* node = new SheepCodeTreeDeclarationNode(type, lineNumber);
	
	return node;
}

SheepCodeTreeNode* SheepCodeTreeNode::CreateIntegerConstant(int value, int lineNumber)
{
	SheepCodeTreeConstantNode* constant = new SheepCodeTreeConstantNode(EXPRVAL_INT, lineNumber);
	constant->SetIntValue(value);

	return constant;
}

SheepCodeTreeNode* SheepCodeTreeNode::CreateFloatConstant(float value, int lineNumber)
{
	SheepCodeTreeConstantNode* constant = new SheepCodeTreeConstantNode(EXPRVAL_FLOAT, lineNumber);
	constant->SetFloatValue(value);

	return constant;
}

SheepCodeTreeNode* SheepCodeTreeNode::CreateStringConstant(const std::string& value, int lineNumber)
{
	int offset = g_codeTree->AddStringConstant(value);
	
	SheepCodeTreeConstantNode* constant = new SheepCodeTreeConstantNode(EXPRVAL_STRING, lineNumber);
	constant->SetStringValue(offset);

	return constant;
}

SheepCodeTreeNode* SheepCodeTreeNode::CreateIdentifierReference(const std::string& name, bool global, int lineNumber)
{
	return new SheepCodeTreeIdentifierReferenceNode(name, global, lineNumber);
}

SheepCodeTreeNode* SheepCodeTreeNode::CreateOperation(CodeTreeOperationType type, int lineNumber)
{
	return new SheepCodeTreeOperationNode(type, lineNumber);
}

SheepCodeTreeNode* SheepCodeTreeNode::CreateKeywordStatement(CodeTreeKeywordStatementType type, int lineNumber)
{
	return new SheepCodeTreeStatementNode(type, lineNumber);
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
	{
		m_sibling->Print(indent);
	}
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


std::string SheepCodeTreeDeclarationNode::GetDeclarationName() const
{
	SheepCodeTreeIdentifierReferenceNode* identifier =
		static_cast<SheepCodeTreeIdentifierReferenceNode*>(m_children[0]);

	assert(identifier != NULL);
	return identifier->GetName();
}
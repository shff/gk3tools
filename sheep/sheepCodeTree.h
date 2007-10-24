#ifndef SHEEPCODETREE_H
#define SHEEPCODETREE_H

#include <string>
#include "sheepScanner.h"

class SheepCodeTreeNode;
class SheepLog;

class SheepCodeTree
{
public:

	SheepCodeTree();

	void Lock(const std::string& script, SheepLog* log);
	
	void Unlock();
	
	const SheepCodeTreeNode* GetCodeTree();

	/// For debugging. Writes out the parse tree.
	void Print();
private:
	bool m_locked;
	SheepLog* m_log;
};

enum CodeTreeNodeType
{
	NODETYPE_INVALID = 0,
	NODETYPE_DECLARATION,
	NODETYPE_STATEMENT,
	NODETYPE_EXPRESSION
};

enum CodeTreeDeclarationNodeType
{
	DECLARATIONTYPE_INT,
	DECLARATIONTYPE_FLOAT,
	DECLARATIONTYPE_STRING,
	DECLARATIONTYPE_FUNCTION
};

enum CodeTreeOperationType
{
	OP_ADD,
	OP_MINUS,
	OP_TIMES,
	OP_DIVIDE,
	OP_GT,
	OP_LT,
	OP_GTE,
	OP_LTE,
	OP_EQ,
	OP_NE,
	OP_ASSIGN
};

enum CodeTreeKeywordStatementType
{
	SMT_RETURN,
	SMT_WAIT,
	SMT_GOTO,
};

class SheepCodeTreeNode
{
public:
	static SheepCodeTreeNode* CreateDeclaration(CodeTreeDeclarationNodeType type, int lineNumber);
	static SheepCodeTreeNode* CreateStatement(int lineNumber);
	static SheepCodeTreeNode* CreateLocalFunction(int lineNumber);

	static SheepCodeTreeNode* CreateIntegerConstant(int value, int lineNumber);
	static SheepCodeTreeNode* CreateFloatConstant(float value, int lineNumber);
	static SheepCodeTreeNode* CreateStringConstant(const std::string& value, int lineNumber);

	static SheepCodeTreeNode* CreateIdentifierReference(const std::string& name, bool global, int lineNumber);

	static SheepCodeTreeNode* CreateOperation(CodeTreeOperationType type, int lineNumber);

	static SheepCodeTreeNode* CreateKeywordStatement(CodeTreeKeywordStatementType type, int lineNumber);

	void AttachSibling(SheepCodeTreeNode* sibling);
	void SetChild(int index, SheepCodeTreeNode* node);

	CodeTreeNodeType GetType() { return m_type; }
	
	void Print(int indent);
	
protected:
	
	virtual void PrintData();

	SheepCodeTreeNode(CodeTreeNodeType type, int lineNumber);
	void PrintIndents(int indents);

	static const int NUM_CHILD_NODES = 4;

	SheepCodeTreeNode* m_parent;
	SheepCodeTreeNode* m_sibling;
	SheepCodeTreeNode* m_children[NUM_CHILD_NODES];

private:
	CodeTreeNodeType m_type;
	int m_lineNumber;
};

class SheepCodeTreeDeclarationNode : public SheepCodeTreeNode
{
public:
	SheepCodeTreeDeclarationNode(CodeTreeDeclarationNodeType type, int lineNumber)
		: SheepCodeTreeNode(NODETYPE_DECLARATION, lineNumber)
	{
		m_declarationType = type;
	}
	
protected:
	
	void PrintData()
	{		
		if (m_declarationType == DECLARATIONTYPE_INT)
			printf("Declaration of integer\n");
		else if (m_declarationType == DECLARATIONTYPE_FLOAT)
			printf("Declaration of float\n");
		else if (m_declarationType == DECLARATIONTYPE_STRING)
			printf("Declaration of string\n");
		else if (m_declarationType == DECLARATIONTYPE_FUNCTION)
			printf("Declaration of local function\n");
	}

private:
	CodeTreeDeclarationNodeType m_declarationType;
};

class SheepCodeTreeStatementNode : public SheepCodeTreeNode
{
public:
	SheepCodeTreeStatementNode(CodeTreeKeywordStatementType type, int lineNumber)
		: SheepCodeTreeNode(NODETYPE_STATEMENT, lineNumber)
	{
		m_type = type;
	}
	
protected:
	
	void PrintData()
	{
		if (m_type == SMT_RETURN)
			printf("RETURN\n");
		else if (m_type == SMT_WAIT)
			printf("WAIT\n");
		else
			printf("UNKNOWN KEYWORD STATEMENT!\n");
	}
	
private:
	CodeTreeKeywordStatementType m_type;
};

class SheepCodeTreeExpressionNode : public SheepCodeTreeNode
{
public:
	SheepCodeTreeExpressionNode(int lineNumber)
		: SheepCodeTreeNode(NODETYPE_EXPRESSION, lineNumber)
	{
	}
};

class SheepCodeTreeIntegerConstantNode : public SheepCodeTreeExpressionNode
{
public:
	
	SheepCodeTreeIntegerConstantNode(int value, int lineNumber)
		: SheepCodeTreeExpressionNode(lineNumber)
	{
		m_value = value;
	}

protected:
	
	void PrintData()
	{
		printf("Integer constant with value %d!\n", m_value);
	}
	
private:
	
	int m_value;
};

class SheepCodeTreeFloatConstantNode : public SheepCodeTreeExpressionNode
{
public:
	
	SheepCodeTreeFloatConstantNode(float value, int lineNumber)
		: SheepCodeTreeExpressionNode(lineNumber)
	{
		m_value = value;
	}

protected:
	
	void PrintData()
	{
		printf("Float constant with value %f!\n", m_value);
	}
	
private:
	
	float m_value;
};

class SheepCodeTreeStringConstantNode : public SheepCodeTreeExpressionNode
{
public:
	
	SheepCodeTreeStringConstantNode(const std::string& value, int lineNumber)
		: SheepCodeTreeExpressionNode(lineNumber)
	{
		m_value = value;
	}

protected:
	
	void PrintData()
	{
		printf("String constant with value \"%s\"\n", m_value.c_str());
	}
	
private:
	
	std::string m_value;
};

class SheepCodeTreeIdentifierReferenceNode : public SheepCodeTreeExpressionNode
{
public:
	SheepCodeTreeIdentifierReferenceNode(const std::string& name, bool global, int lineNumber)
		: SheepCodeTreeExpressionNode(lineNumber)
	{
		m_name = name;
		m_global = global;
	}
	
protected:
	
	void PrintData()
	{
		printf("Identifier reference: %s\n", m_name.c_str());
	}
	
private:
	
	bool m_global;
	std::string m_name;
};

class SheepCodeTreeOperationNode : public SheepCodeTreeExpressionNode
{
public:
	SheepCodeTreeOperationNode(CodeTreeOperationType type, int lineNumber)
		: SheepCodeTreeExpressionNode(lineNumber)
	{
		m_type = type;
	}
	
protected:
	
	void PrintData()
	{
		std::string operationText = "????";
		
		if (m_type == OP_ADD)
			operationText = "ADD";
		else if (m_type == OP_MINUS)
			operationText = "MINUS";
		else if (m_type == OP_TIMES)
			operationText = "TIMES";
		else if (m_type == OP_DIVIDE)
			operationText = "DIVIDE";
		else if (m_type == OP_ASSIGN)
			operationText = "ASSIGN";
		else if (m_type == OP_LT)
			operationText = "LESS THAN";
		else if (m_type == OP_GT)
			operationText = "GREATER THAN";
		else if (m_type == OP_LTE)
			operationText = "LESS THAN OR EQUAL";
		else if (m_type == OP_GTE)
			operationText = "GREATER THAN OR EQUAL";
		else if (m_type == OP_EQ)
			operationText = "EQUALS";
		else if (m_type == OP_NE)
			operationText = "NOT EQUAL";
		
		printf("Operation: %s\n", operationText.c_str());
	}
	
private:
	
	CodeTreeOperationType m_type;
};

class SheepException : public std::exception
{
public:
	SheepException(const std::string& message) throw()
	{
		m_message = message;
	}
	
	virtual ~SheepException() throw() {}

	std::string GetMessage() { return m_message; }
	const char* what() const throw() { return m_message.c_str(); }

private:
	std::string m_message;

};

int yyparse(void);

#endif // SHEEPCODETREE_H

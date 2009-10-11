#ifndef SHEEPCODETREE_H
#define SHEEPCODETREE_H

#include <cassert>
#include <string>
#include <map>
#include "sheepScanner.h"

class SheepCodeTreeNode;
class SheepLog;

class SheepCodeTree
{
public:

	SheepCodeTree();

	void Lock(const std::string& script, SheepLog* log);
	
	void Unlock();

	void LogError(int lineNumber, const char* msg);
	
	const SheepCodeTreeNode* GetCodeTree() const;
	SheepCodeTreeNode* GetCodeTree();

	/// For debugging. Writes out the parse tree.
	void Print();

	/// Adds the string to the list of string constants.
	/// Returns the offset of the string. If the string has
	/// already been added then it returns the offset of the
	/// existing string, and no new string is added.
	int AddStringConstant(const std::string& value);

	struct StringConst
	{
		int Offset;
		std::string Value;
	};

	std::map<std::string, StringConst>::const_iterator GetFirstConstant() const { return m_stringConstants.begin(); }
	std::map<std::string, StringConst>::const_iterator GetEndOfConstants() const { return m_stringConstants.end(); }

private:
	bool m_locked;
	SheepLog* m_log;

	int m_nextStringConstantOffset;
	std::map<std::string, StringConst> m_stringConstants;
};

enum CodeTreeNodeType
{
	NODETYPE_INVALID = 0,
	NODETYPE_SECTION,
	NODETYPE_DECLARATION,
	NODETYPE_STATEMENT,
	NODETYPE_EXPRESSION
};

enum CodeTreeSectionType
{
	SECTIONTYPE_SYMBOLS,
	SECTIONTYPE_CODE,
	SECTIONTYPE_SNIPPET
};

enum CodeTreeDeclarationNodeType
{
	DECLARATIONTYPE_INT,
	DECLARATIONTYPE_FLOAT,
	DECLARATIONTYPE_STRING,
	DECLARATIONTYPE_FUNCTION,
	DECLARATIONTYPE_LABEL
};

enum CodeTreeExpressionType
{
	EXPRTYPE_OPERATION,
	EXPRTYPE_IDENTIFIER,
	EXPRTYPE_CONSTANT
};

enum CodeTreeExpressionValueType
{
	EXPRVAL_UNKNOWN,
	EXPRVAL_VOID,
	EXPRVAL_INT,
	EXPRVAL_FLOAT,
	EXPRVAL_STRING
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

	OP_NEGATE,

	OP_NOT,
	OP_AND,
	OP_OR
};

enum CodeTreeKeywordStatementType
{
	SMT_EXPR,
	SMT_ASSIGN,
	SMT_RETURN,
	SMT_WAIT,
	SMT_GOTO,
	SMT_IF
};

class SheepCodeTreeNode
{
public:	

	static SheepCodeTreeNode* CreateSymbolSection(int lineNumber);
	static SheepCodeTreeNode* CreateCodeSection(int lineNumber);
	static SheepCodeTreeNode* CreateSnippet(int lineNumber);

	static SheepCodeTreeNode* CreateDeclaration(CodeTreeDeclarationNodeType type, int lineNumber);
	static SheepCodeTreeNode* CreateStatement(int lineNumber);
	static SheepCodeTreeNode* CreateLocalFunction(int lineNumber);

	static SheepCodeTreeNode* CreateIntegerConstant(int value, int lineNumber);
	static SheepCodeTreeNode* CreateFloatConstant(float value, int lineNumber);
	static SheepCodeTreeNode* CreateStringConstant(const std::string& value, int lineNumber);

	static SheepCodeTreeNode* CreateIdentifierReference(const char* name, bool global, int lineNumber, char* errorBuffer, int bufferLength);

	static SheepCodeTreeNode* CreateOperation(CodeTreeOperationType type, int lineNumber);

	static SheepCodeTreeNode* CreateKeywordStatement(CodeTreeKeywordStatementType type, int lineNumber);

	virtual ~SheepCodeTreeNode();

	void AttachSibling(SheepCodeTreeNode* sibling);
	void SetChild(int index, SheepCodeTreeNode* node);

	SheepCodeTreeNode* GetChild(int index) { assert(index >= 0 && index < NUM_CHILD_NODES); return m_children[index]; }
	const SheepCodeTreeNode* GetChild(int index) const { assert(index >= 0 && index < NUM_CHILD_NODES); return m_children[index]; }
	
	SheepCodeTreeNode* GetNextSibling() { return m_sibling; }
	const SheepCodeTreeNode* GetNextSibling() const { return m_sibling; }

	CodeTreeNodeType GetType() const { return m_type; }
	int GetLineNumber() const { return m_lineNumber; }
	
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

class SheepCodeTreeSectionNode : public SheepCodeTreeNode
{
public:
	SheepCodeTreeSectionNode(CodeTreeSectionType type, int lineNumber)
		: SheepCodeTreeNode(NODETYPE_SECTION, lineNumber)
	{
		m_sectionType = type;
	}

	virtual ~SheepCodeTreeSectionNode() {}

	CodeTreeSectionType GetSectionType() const { return m_sectionType; }

protected:

	void PrintData()
	{
		if (m_sectionType == SECTIONTYPE_SYMBOLS)
			printf("Symbols section\n");
		else if (m_sectionType == SECTIONTYPE_CODE)
			printf("Code section\n");
		else if (m_sectionType == SECTIONTYPE_SNIPPET)
			printf("Snippet\n");
		else
			printf("UNKNOWN SECTION TYPE!\n");
	}

	CodeTreeSectionType m_sectionType;
};

class SheepCodeTreeDeclarationNode : public SheepCodeTreeNode
{
public:
	SheepCodeTreeDeclarationNode(CodeTreeDeclarationNodeType type, int lineNumber)
		: SheepCodeTreeNode(NODETYPE_DECLARATION, lineNumber)
	{
		m_declarationType = type;
	}

	CodeTreeDeclarationNodeType GetDeclarationType() const { return m_declarationType; }
	
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
		else if (m_declarationType == DECLARATIONTYPE_LABEL)
			printf("Declaration of label\n");
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
	
	CodeTreeKeywordStatementType GetStatementType() const { return m_type; }

protected:
	
	void PrintData()
	{
		if (m_type == SMT_EXPR)
			printf("Expression statement\n");
		else if (m_type == SMT_ASSIGN)
			printf("ASSIGN\n");
		else if (m_type == SMT_RETURN)
			printf("RETURN\n");
		else if (m_type == SMT_WAIT)
			printf("WAIT\n");
		else if (m_type == SMT_GOTO)
			printf("GOTO\n");
		else if (m_type == SMT_IF)
			printf("IF\n");
		else
			printf("UNKNOWN KEYWORD STATEMENT!\n");
	}

	
	
private:
	CodeTreeKeywordStatementType m_type;
};

class SheepCodeTreeExpressionNode : public SheepCodeTreeNode
{
public:
	SheepCodeTreeExpressionNode(CodeTreeExpressionType expressionType, int lineNumber)
		: SheepCodeTreeNode(NODETYPE_EXPRESSION, lineNumber)
	{
		m_expressionType = expressionType;
		m_valueType = EXPRVAL_UNKNOWN;
	}

	virtual ~SheepCodeTreeExpressionNode() {}

	CodeTreeExpressionType GetExpressionType() const { return m_expressionType; }
	CodeTreeExpressionValueType GetValueType() const { return m_valueType; }

protected:

	CodeTreeExpressionValueType m_valueType;

private:

	CodeTreeExpressionType m_expressionType;
	
};


class SheepCodeTreeConstantNode : public SheepCodeTreeExpressionNode
{
public:

	SheepCodeTreeConstantNode(CodeTreeExpressionValueType valueType, int lineNumber)
		: SheepCodeTreeExpressionNode(EXPRTYPE_CONSTANT, lineNumber)
	{
		m_valueType = valueType;

		m_intValue = 0;
		m_floatValue = 0;
		m_stringValue = 0;
	}

	int GetIntValue() { return m_intValue; }
	float GetFloatValue() { return m_floatValue; }
	int GetStringValue() { return m_stringValue; }

	void SetIntValue(int value) { m_intValue = value; }
	void SetFloatValue(float value) { m_floatValue = value; }
	void SetStringValue(int value) { m_stringValue = value; }

protected:

	void PrintData()
	{
		if (m_valueType == EXPRVAL_INT)
			printf("Integer constant with value %d\n", m_intValue);
		else if (m_valueType == EXPRVAL_FLOAT)
			printf("Float constant with value %f\n", m_floatValue);
		else if (m_valueType == EXPRVAL_STRING)
			printf("String constant with offset %d\n", m_stringValue);
		else
			printf("UNKNOWN CONSTANT TYPE!\n");
	}

private:

	int m_intValue;
	float m_floatValue;
	int m_stringValue;

};

class SheepCodeTreeIdentifierReferenceNode : public SheepCodeTreeExpressionNode
{
public:
	SheepCodeTreeIdentifierReferenceNode(const std::string& name, bool global, int lineNumber)
		: SheepCodeTreeExpressionNode(EXPRTYPE_IDENTIFIER, lineNumber)
	{
		m_name = name;
		m_global = global;
	}
	
	virtual ~SheepCodeTreeIdentifierReferenceNode() {}

	std::string GetName() const { return m_name; }
	bool IsGlobal() const { return m_global; }

	void SetValueType(CodeTreeExpressionValueType type) { m_valueType = type; }

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
		: SheepCodeTreeExpressionNode(EXPRTYPE_OPERATION, lineNumber)
	{
		m_type = type;
	}

	CodeTreeOperationType GetOperationType() const { return m_type; }

	void SetValueType(CodeTreeExpressionValueType type) { m_valueType = type; }
	
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
		else if (m_type == OP_NOT)
			operationText = "NOT";
		else if (m_type == OP_AND)
			operationText = "AND";
		else if (m_type == OP_OR)
			operationText = "OR";
		
		printf("Operation: %s\n", operationText.c_str());
	}
	
private:
	
	CodeTreeOperationType m_type;
};

int yyparse(void);

#endif // SHEEPCODETREE_H

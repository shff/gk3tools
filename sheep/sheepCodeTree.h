#ifndef SHEEPCODETREE_H
#define SHEEPCODETREE_H

#include <cstdio>
#include <cassert>
#include <string>
#include <map>
#include "sheepScanner.h"
#include "sheepConfig.h"

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

SHEEP_ENUM(CodeTreeNodeType)
	Invalid = 0,
	Section,
	Declaration,
	Statement,
	Expression,
	TypeReference
END_SHEEP_ENUM(CodeTreeNodeType)

SHEEP_ENUM(CodeTreeSectionType)
	Symbols,
	Code
END_SHEEP_ENUM(CodeTreeSectionType)


SHEEP_ENUM(CodeTreeDeclarationNodeType)
	Variable,
	Function,
	Label
END_SHEEP_ENUM(CodeTreeDeclarationNodeType)

SHEEP_ENUM(CodeTreeExpressionType)
	Operation,
	Identifier,
	Constant
END_SHEEP_ENUM(CodeTreeExpressionType)

SHEEP_ENUM(CodeTreeTypeReferenceType)
	Int,
	Float,
	String,
	Handle,
	Custom
END_SHEEP_ENUM(CodeTreeTypeReferenceType)

SHEEP_ENUM(CodeTreeExpressionValueType)
	Unknown,
	Void,
	Int,
	Float,
	String
END_SHEEP_ENUM(CodeTreeExpressionValueType)

SHEEP_ENUM(CodeTreeOperationType)
	Add,
	Minus,
	Times,
	Divide,
	GreaterThan,
	LessThan,
	GreaterThanEqual,
	LessThanEqual,
	Equal,
	NotEqual,

	Negate,

	Not,
	And,
	Or
END_SHEEP_ENUM(CodeTreeOperationType)

SHEEP_ENUM(CodeTreeKeywordStatementType)
	Expression,
	Assignment,
	Return,
	Wait,
	Goto,
	If
END_SHEEP_ENUM(CodeTreeKeywordStatementType)

class SheepCodeTreeNode
{
public:	

	static SheepCodeTreeNode* CreateSymbolSection(int lineNumber);
	static SheepCodeTreeNode* CreateCodeSection(int lineNumber);

	static SheepCodeTreeNode* CreateVariableDeclaration(int lineNumber);
	static SheepCodeTreeNode* CreateFunctionDeclaration(int lineNumber);
	static SheepCodeTreeNode* CreateLabelDeclaration(int lineNumber);
	static SheepCodeTreeNode* CreateStatement(int lineNumber);
	static SheepCodeTreeNode* CreateLocalFunction(int lineNumber);
	static SheepCodeTreeNode* CreateLocalFunctionParam(int lineNumber);

	static SheepCodeTreeNode* CreateIntegerConstant(int value, int lineNumber);
	static SheepCodeTreeNode* CreateFloatConstant(float value, int lineNumber);
	static SheepCodeTreeNode* CreateStringConstant(const std::string& value, int lineNumber);

	static SheepCodeTreeNode* CreateIdentifierReference(const char* name, bool global, int lineNumber, char* errorBuffer, int bufferLength);

	static SheepCodeTreeNode* CreateOperation(CodeTreeOperationType type, int lineNumber);

	static SheepCodeTreeNode* CreateKeywordStatement(CodeTreeKeywordStatementType type, int lineNumber);

	static SheepCodeTreeNode* CreateTypeReference(CodeTreeTypeReferenceType type, int lineNumber);

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
		: SheepCodeTreeNode(CodeTreeNodeType::Section, lineNumber)
	{
		m_sectionType = type;
	}

	virtual ~SheepCodeTreeSectionNode() {}

	CodeTreeSectionType GetSectionType() const { return m_sectionType; }

protected:

	void PrintData()
	{
		if (m_sectionType == CodeTreeSectionType::Symbols)
			printf("Symbols section\n");
		else if (m_sectionType == CodeTreeSectionType::Code)
			printf("Code section\n");
		else
			printf("UNKNOWN SECTION TYPE!\n");
	}

	CodeTreeSectionType m_sectionType;
};

class SheepCodeTreeDeclarationNode : public SheepCodeTreeNode
{
public:
	SheepCodeTreeDeclarationNode(CodeTreeDeclarationNodeType type, int lineNumber)
		: SheepCodeTreeNode(CodeTreeNodeType::Declaration, lineNumber)
	{
		m_declarationType = type;
	}

	CodeTreeDeclarationNodeType GetDeclarationType() const { return m_declarationType; }
	
protected:
	
	void PrintData()
	{		
		if (m_declarationType == CodeTreeDeclarationNodeType::Variable)
			printf("Declaration of variable\n");
		else if (m_declarationType == CodeTreeDeclarationNodeType::Function)
			printf("Declaration of local function\n");
		else if (m_declarationType == CodeTreeDeclarationNodeType::Label)
			printf("Declaration of label\n");
	}

private:
	CodeTreeDeclarationNodeType m_declarationType;
};

class SheepCodeTreeStatementNode : public SheepCodeTreeNode
{
public:
	SheepCodeTreeStatementNode(CodeTreeKeywordStatementType type, int lineNumber)
		: SheepCodeTreeNode(CodeTreeNodeType::Statement, lineNumber)
	{
		m_type = type;
	}
	
	CodeTreeKeywordStatementType GetStatementType() const { return m_type; }

protected:
	
	void PrintData()
	{
		if (m_type == CodeTreeKeywordStatementType::Expression)
			printf("Expression statement\n");
		else if (m_type == CodeTreeKeywordStatementType::Assignment)
			printf("ASSIGN\n");
		else if (m_type == CodeTreeKeywordStatementType::Return)
			printf("RETURN\n");
		else if (m_type == CodeTreeKeywordStatementType::Wait)
			printf("WAIT\n");
		else if (m_type == CodeTreeKeywordStatementType::Goto)
			printf("GOTO\n");
		else if (m_type == CodeTreeKeywordStatementType::If)
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
		: SheepCodeTreeNode(CodeTreeNodeType::Expression, lineNumber)
	{
		m_expressionType = expressionType;
		m_valueType = CodeTreeExpressionValueType::Unknown;
	}

	virtual ~SheepCodeTreeExpressionNode() {}

	CodeTreeExpressionType GetExpressionType() const { return m_expressionType; }
	CodeTreeExpressionValueType GetValueType() const { return m_valueType; }

protected:

	CodeTreeExpressionValueType m_valueType;

private:

	CodeTreeExpressionType m_expressionType;
	
};

class SheepCodeTreeSymbolTypeNode : public SheepCodeTreeNode
{
	CodeTreeTypeReferenceType m_refType;

public:
	SheepCodeTreeSymbolTypeNode(CodeTreeTypeReferenceType type, int lineNumber)
		: SheepCodeTreeNode(CodeTreeNodeType::TypeReference, lineNumber)
	{
		m_refType = type;
	}

	CodeTreeTypeReferenceType GetRefType() { return m_refType; }
};

class SheepCodeTreeConstantNode : public SheepCodeTreeExpressionNode
{
public:

	SheepCodeTreeConstantNode(CodeTreeExpressionValueType valueType, int lineNumber)
		: SheepCodeTreeExpressionNode(CodeTreeExpressionType::Constant, lineNumber)
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
		if (m_valueType == CodeTreeExpressionValueType::Int)
			printf("Integer constant with value %d\n", m_intValue);
		else if (m_valueType == CodeTreeExpressionValueType::Float)
			printf("Float constant with value %f\n", m_floatValue);
		else if (m_valueType == CodeTreeExpressionValueType::String)
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
		: SheepCodeTreeExpressionNode(CodeTreeExpressionType::Identifier, lineNumber)
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
		: SheepCodeTreeExpressionNode(CodeTreeExpressionType::Operation, lineNumber)
	{
		m_type = type;
	}

	CodeTreeOperationType GetOperationType() const { return m_type; }

	void SetValueType(CodeTreeExpressionValueType type) { m_valueType = type; }
	
protected:
	
	void PrintData()
	{
		std::string operationText = "????";
		
		if (m_type == CodeTreeOperationType::Add)
			operationText = "ADD";
		else if (m_type == CodeTreeOperationType::Minus)
			operationText = "MINUS";
		else if (m_type == CodeTreeOperationType::Times)
			operationText = "TIMES";
		else if (m_type == CodeTreeOperationType::Divide)
			operationText = "DIVIDE";
		else if (m_type == CodeTreeOperationType::LessThan)
			operationText = "LESS THAN";
		else if (m_type == CodeTreeOperationType::GreaterThan)
			operationText = "GREATER THAN";
		else if (m_type == CodeTreeOperationType::LessThanEqual)
			operationText = "LESS THAN OR EQUAL";
		else if (m_type == CodeTreeOperationType::GreaterThanEqual)
			operationText = "GREATER THAN OR EQUAL";
		else if (m_type == CodeTreeOperationType::Equal)
			operationText = "EQUALS";
		else if (m_type == CodeTreeOperationType::NotEqual)
			operationText = "NOT EQUAL";
		else if (m_type == CodeTreeOperationType::Not)
			operationText = "NOT";
		else if (m_type == CodeTreeOperationType::And)
			operationText = "AND";
		else if (m_type == CodeTreeOperationType::Or)
			operationText = "OR";
		
		printf("Operation: %s\n", operationText.c_str());
	}
	
private:
	
	CodeTreeOperationType m_type;
};

int yyparse(void);

#endif // SHEEPCODETREE_H

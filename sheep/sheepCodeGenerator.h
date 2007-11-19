#ifndef SHEEPCODEGENERATOR_H
#define SHEEPCODEGENERATOR_H

#include <cassert>
#include <map>
#include "sheepTypes.h"
#include "sheepCodeTree.h"

class IntermediateOutput
{
public:
	std::vector<SheepSymbol> Symbols;
	std::vector<SheepStringConstant> Constants;
	std::vector<SheepImport> Imports;
	std::vector<SheepFunction> Functions;

	std::vector<CompilerOutput> Warnings;
	std::vector<CompilerOutput> Errors;

	/// for debugging
	void Print();
};

class SheepCodeTree;
class SheepImportTable;

class SheepCodeGenerator
{
public:

	SheepCodeGenerator(SheepCodeTree* tree, SheepImportTable* imports);

	IntermediateOutput* BuildIntermediateOutput();

private:
	void loadStringConstants(IntermediateOutput* output);
	void buildSymbolMap(SheepCodeTreeNode* node);
	void determineExpressionTypes(SheepCodeTreeNode* node);
	SheepFunction writeFunction(SheepCodeTreeDeclarationNode* node);
	void writeCode(SheepFunction& function, SheepCodeTreeNode* node);
	void writeStatement(SheepFunction& function, SheepCodeTreeStatementNode* statement);
	void writeExpression(SheepFunction& function, SheepCodeTreeExpressionNode* expression);
	void writeBinaryOperator(SheepFunction& function, SheepCodeTreeOperationNode* operation);

	static SheepSymbolType convertToSymbolType(CodeTreeDeclarationNodeType type);
	static CodeTreeExpressionValueType convertToExpressionValueType(SheepSymbolType type);

	SheepCodeTree* m_tree;
	SheepImportTable* m_imports;

	std::vector<SheepImport> m_usedImports;
	std::vector<SheepSymbol> m_variables;
	
	size_t getIndexOfImport(SheepImport& import);
	size_t getIndexOfVariable(SheepSymbol& symbol);

	typedef std::map<std::string, SheepSymbol> SymbolMap;
	SymbolMap m_symbolMap;
	
	/// Gets the type of symbol. Throws a SheepCompilerException if the symbol doesn't exist.
	SheepSymbolType getSymbolType(int lineNumber, const std::string& name);
};

#endif // SHEEPCODEGENERATOR_H

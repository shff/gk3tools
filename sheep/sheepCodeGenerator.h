#ifndef SHEEPCODEGENERATOR_H
#define SHEEPCODEGENERATOR_H

#include <cassert>
#include <map>
#include "sheepTypes.h"
#include "sheepCodeTree.h"
#include "sheepCaseInsensitiveStringCompare.h"
#include "sheepMemoryAllocator.h"

/// Class used to represent immediate output of the compiler.
/// This can be used for a dynamic interpreter that doesn't
/// want to have to parse full .shp files when interpreting.
class IntermediateOutput
{
	int m_numRefs;
	Sheep::SheepLanguageVersion m_languageVersion;

public:

	IntermediateOutput(Sheep::SheepLanguageVersion version) { m_numRefs = 1; m_languageVersion = version; }
	~IntermediateOutput();

	void AddRef() { m_numRefs++; }
	void Release() { m_numRefs--; if (m_numRefs <= 0) SHEEP_DELETE(this); }

	Sheep::SheepLanguageVersion GetLanguageVersion() { return m_languageVersion; }

	std::vector<SheepSymbol> Symbols;
	std::vector<SheepStringConstant> Constants;
	std::vector<SheepImport> Imports;
	std::vector<SheepFunction> Functions;

	std::vector<CompilerOutput> Warnings;

	/// A list of compile errors. If there are any errors then the
	/// state of everything else in this class is undefined.
	std::vector<CompilerOutput> Errors;

	/// for debugging
	void Print();
};

class SheepCodeTree;
class SheepImportTable;

class SheepCodeGenerator
{
public:
	static IntermediateOutput* BuildIntermediateOutput(SheepCodeTree* tree, SheepImportTable* imports, Sheep::SheepLanguageVersion languageVersion);

private:
	typedef std::map<std::string, size_t, ci_less> LabelMap;

	struct InternalContext
	{
		Sheep::SheepLanguageVersion LanguageVersion;

		std::vector<SheepSymbol> Variables;

		typedef std::map<std::string, SheepSymbol, ci_less> SymbolMap;
		SymbolMap Symbols;

		typedef std::map<SheepCodeTreeDeclarationNode*, LabelMap> FunctionLabelMap;
		FunctionLabelMap Labels;
		
		SheepImportTable* Imports;
		std::vector<SheepImport> UsedImports;

		std::vector<CompilerOutput> Output;

		int GetIndexOfVariable(SheepSymbol& symbol);
		int GetIndexOfImport(SheepImport& import);
		
		/// Gets the type of symbol. Throws a SheepCompilerException if the symbol doesn't exist.
		SheepSymbolType GetSymbolType(int lineNumber, const std::string& name);
	};

	static void loadStringConstants(SheepCodeTree* tree, IntermediateOutput* output);
	static void buildSymbolMap(InternalContext* ctx, SheepCodeTreeNode* node);
	static void gatherFunctionLabels(LabelMap& labels, SheepCodeTreeNode* node);
	static void determineExpressionTypes(InternalContext* ctx, SheepFunction& function, SheepCodeTreeNode* node);
	static SheepFunction writeFunction(InternalContext* ctx, SheepCodeTreeFunctionDeclarationNode* function, int codeOffset);
	static void writeCode(InternalContext* ctx, SheepFunction& function, SheepCodeTreeNode* node);
	static void writeStatement(InternalContext* ctx, SheepFunction& function, SheepCodeTreeStatementNode* statement);
	static int writeExpression(InternalContext* ctx, SheepFunction& function, SheepCodeTreeExpressionNode* expression);
	static void writeBinaryOperator(SheepFunction& function, SheepCodeTreeOperationNode* operation);

	static CodeTreeExpressionValueType convertToExpressionValueType(SheepSymbolType type);
	
};

#endif // SHEEPCODEGENERATOR_H

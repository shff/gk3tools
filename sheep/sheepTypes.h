#ifndef SHEEPTYPES_H
#define SHEEPTYPES_H

#include <string>
#include <vector>
#include <map>
#include "sheepc.h"
#include "sheepcpp.h"
#include "sheepConfig.h"

enum class SheepSymbolType
{
	Void = 0,
	Int,
	Float,
	String,
	LocalFunction,
	Import,
	Label
};

static const char* SheepSymbolTypeNames[] =
{
	"void",
	"int",
	"float",
	"string",
	"local function",
	"import",
	"label"
};

struct SheepSymbol
{
	SheepSymbol()
	{
		Type = SheepSymbolType::Int;
		InitialIntValue = 0;
		InitialFloatValue = 0;
		InitialStringValue = 0;
	}

	std::string Name;
	SheepSymbolType Type;

	int InitialIntValue;
	float InitialFloatValue;
	int InitialStringValue;
};

class SheepCodeBuffer;
class SheepCodeTreeDeclarationNode;
class IntermediateOutput;

struct SheepFunction
{
	SheepFunction(SheepCodeTreeDeclarationNode* declaration) { Code = nullptr; ParentCode = nullptr; Declaration = declaration; ReturnType = SheepSymbolType::Void; }

	SheepCodeTreeDeclarationNode* Declaration;
	SheepSymbolType ReturnType;
	std::vector<SheepSymbol> Parameters;
	std::string Name;
	SheepCodeBuffer* Code;
	int CodeOffset;
	IntermediateOutput* ParentCode;

	std::vector<std::string> ImportList;
	std::vector<std::pair<size_t, size_t&> > Gotos; // first = offset of the offset, second = ref to the offset of the label
};

struct SheepImport : public SheepImportFunction
{
	SheepImport() { }

	std::string Name;
	SheepSymbolType ReturnType;
	std::vector<SheepSymbolType> Parameters;
};

struct SheepStringConstant
{
	std::string Value;
	int Offset;
};

struct CompilerOutput
{
	int LineNumber;
	std::string Output;

	CompilerOutput()
	{
		LineNumber = 0;
	}

	CompilerOutput(int lineNumber, const std::string& text)
	{
		LineNumber = lineNumber;
		Output = text;
	}
};

#endif // SHEEPTYPES_H

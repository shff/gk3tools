#ifndef SHEEPTYPES_H
#define SHEEPTYPES_H

#include <string>
#include <vector>
#include <map>
#include "sheepc.h"

enum SheepSymbolType
{
	SYM_VOID = 0,
	SYM_INT,
	SYM_FLOAT,
	SYM_STRING,
	SYM_LOCALFUNCTION,
	SYM_IMPORT,
	SYM_LABEL
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
		Type = SYM_INT;
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

struct SheepFunction
{
	SheepFunction(SheepCodeTreeDeclarationNode* declaration) { Code = NULL; Declaration = declaration; }

	SheepCodeTreeDeclarationNode* Declaration;
	std::string Name;
	SheepCodeBuffer* Code;
	int CodeOffset;

	std::vector<std::string> ImportList;
	std::vector<std::pair<size_t, size_t&> > Gotos; // first = offset of the offset, second = ref to the offset of the label
};

struct SheepImport : public SheepImportFunction
{
	SheepImport() { Callback = NULL; }

	std::string Name;
	SheepSymbolType ReturnType;
	std::vector<SheepSymbolType> Parameters;

	SHP_ImportCallback Callback;
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
};

#endif // SHEEPTYPES_H

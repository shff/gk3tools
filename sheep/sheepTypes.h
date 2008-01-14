#ifndef SHEEPTYPES_H
#define SHEEPTYPES_H

#include <string>
#include <vector>
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

static std::string SheepSymbolTypeNames[] =
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

struct SheepFunction
{
	SheepFunction() { Code = NULL; }

	std::string Name;
	SheepCodeBuffer* Code;
	int CodeOffset;

	std::vector<std::string> ImportList;
};

typedef void (*SheepImportCallback)(SheepVM*);

struct SheepImport : public SheepImportFunction
{
	SheepImport() { Callback = NULL; }

	std::string Name;
	SheepSymbolType ReturnType;
	std::vector<SheepSymbolType> Parameters;

	SheepImportCallback Callback;
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

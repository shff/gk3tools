#include <map>
#include <vector>
#include <string>
#include "symbols.h"
#include "compiler.h"

extern "C"
{
	void yyerror(const char* str); // defined by sheep.y
}


void AddIntSymbol(char* name, int value)
{
	SheepCompiler::Compiler::AddIntSymbol(name, value);
}

void AddFloatSymbol(char* name, float value)
{
	SheepCompiler::Compiler::AddFloatSymbol(name, value);
}

void AddStringSymbol(char* name, char* value)
{
	if (value == NULL)
		SheepCompiler::Compiler::AddStringSymbol(name, "");
	else
		SheepCompiler::Compiler::AddStringSymbol(name, value);
}

void AssignSymbolValue(char* name)
{
	SheepCompiler::Compiler::AssignSymbolValue(name);
}

void AddLocalFunction(char* name, int makeCurrent)
{
	SheepCompiler::Compiler::AddFunction(name);
}

void AddIntegerToStack(int i)
{
	SheepCompiler::Compiler::AddIntegerToStack(i);
}

void AddStringToStack(char* string)
{
	SheepCompiler::Compiler::AddStringToStack(string);
}

void AddFunctionCall(char* function)
{
	SheepCompiler::Compiler::AddFunctionCall(function);
}

void AddLocalValueToStack(char* valueName)
{
	SheepCompiler::Compiler::AddLocalValueToStack(valueName);
}


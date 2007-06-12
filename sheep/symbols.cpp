#include <map>
#include <vector>
#include <string>
#include "symbols.h"
#include "compiler.h"

extern "C"
{
	void yyerror(const char* str); // defined by sheep.y
}



typedef std::map<std::string, Symbol> SymbolMap;
SymbolMap g_symbols;

bool addSymbol(const std::string& name, Symbol symbol)
{
	return (g_symbols.insert(SymbolMap::value_type(name, symbol))).second;
}

void AddIntSymbol(char* name, int value)
{
	Symbol symbol;
	symbol.Name = name;
	symbol.Type = Integer;
	symbol.Value.IntValue = value;

	if (addSymbol(name, symbol) == false)
		yyerror("symbol already defined");
}

void AddFloatSymbol(char* name, float value)
{
	Symbol symbol;
	symbol.Name = name;
	symbol.Type = Float;
	symbol.Value.FloatValue = value;

	if (addSymbol(name, symbol) == false)
		yyerror("symbol already defined");
}

void AddStringSymbol(char* name, char* value)
{
	Symbol symbol;
	symbol.Name = name;
	symbol.Type = String;
	symbol.Value.StringValue = (value == NULL ? "" : value);

	if (addSymbol(name, symbol) == false)
		yyerror("symbol already defined");
}

void AddLocalFunction(char* name, int makeCurrent)
{
	Symbol symbol;
	symbol.Name = name;
	symbol.Type = LocalFunction;

	if (addSymbol(name, symbol) == false)
		yyerror("symbol already defined");

	Compiler::AddFunction(name);
}

void AddStringToStack(char* string)
{
	Compiler::AddStringToStack(string);
}

void AddFunctionCall(char* function)
{
	Compiler::AddFunctionCall(function);
}

int GetTotalSymbolCount()
{
	return g_symbols.size();
}

std::vector<Symbol> GetSymbols()
{
	std::vector<Symbol> symbols;

	for (SymbolMap::iterator itr = g_symbols.begin();
		itr != g_symbols.end(); itr++)
	{
		symbols.push_back((*itr).second);
	}

	return symbols;
}

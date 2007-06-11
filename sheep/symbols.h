#ifndef SYMBOLS_H
#define SYMBOLS_H

#ifdef __cplusplus

enum SymbolType
{
	GlobalFunction,
	LocalFunction,	
	Integer,
	Float,
	String
};

struct SymbolValue
{
	float FloatValue;
	int IntValue;
	std::string StringValue;
};

struct Symbol
{
	std::string Name;
	SymbolType Type;
	SymbolValue Value;
};

int GetTotalSymbolCount();
std::vector<Symbol> GetSymbols();

extern "C"
{
#endif

void AddIntSymbol(char* name, int value);
void AddFloatSymbol(char* name, float value);
void AddStringSymbol(char* name, char* value);

void AddLocalFunction(char* name, int makeCurrent);

void AddCallGlobalFunctionInstruction(char* functionName);

#ifdef __cplusplus
}
#endif

#endif // SYMBOLS_H

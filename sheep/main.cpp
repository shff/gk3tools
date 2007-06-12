#include <iostream>
#include <vector>
#include "symbols.h"
#include "compiler.h"

extern "C"
{
int yyparse(void);
}

int main(int argc, char** argv)
{
	if (argc > 1)
		freopen(argv[1], "r", stdin);

	Compiler::Init();

	yyparse();

	std::vector<Symbol> symbols = GetSymbols();
	for (int i = 0; i < symbols.size(); i++)
	{
		std::cout << "Symbol found: " << symbols[i].Name;

		if (symbols[i].Type == Integer)
			std::cout << " Integer Value: " << symbols[i].Value.IntValue;
		else if (symbols[i].Type == Float)
			std::cout << " Float value: " << symbols[i].Value.FloatValue;
		else if (symbols[i].Type == String)
			std::cout << " String value: " << symbols[i].Value.StringValue;
		else if (symbols[i].Type == LocalFunction)
			std::cout << " Local function!";		

		std::cout << std::endl;
	}
	
	Compiler::WriteCompiledSheep("output.shp");

	return 0;
}

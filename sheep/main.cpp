#include <iostream>
#include <vector>
#include "symbols.h"

extern "C"
{
int yyparse(void);
}

int main(int argc, char** argv)
{
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
	
	return 0;
}

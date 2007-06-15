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

	SheepCompiler::Compiler::Init();

	yyparse();
	
	SheepCompiler::Compiler::WriteCompiledSheep("output.shp");

	return 0;
}

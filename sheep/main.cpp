#include <iostream>
#include <fstream>
#include <vector>
#include "symbols.h"
#include "compiler.h"
#include "disassembler.h"

extern "C"
{
int yyparse(void);
}

int showDisassembly(const std::string& filename);

int main(int argc, char** argv)
{
	if (argc > 1)
	{
		if (std::string(argv[1]) == "-d" && argc > 2)
		{
			return showDisassembly(argv[2]);
		}
	}
	else
	{
		std::cout << "Usage: " << argv[0] << " [input file]" << std::endl;
		return 0;
	}

	try
	{
		SheepCompiler::Compiler::Init(false);

		SheepCompiler::Compiler::Compile(argv[1]);
		
		SheepCompiler::Compiler::WriteCompiledSheep("output.shp");

		//SheepCompiler::Compiler::CompileScript("csnippet { 1 && 2 && 0 }");
	}
	catch(SheepCompiler::CompilerException& e)
	{
		std::cout << "Error: " << e.GetError() << std::endl;
		return -1;
	}

	return 0;
}

int showDisassembly(const std::string& filename)
{
	try
	{
		std::string disassembly = SheepCompiler::Disassembler::GetDisassembly(filename);
		std::cout << disassembly << std::endl;
	}
	catch(SheepCompiler::CompilerException& e)
	{
		std::cout << "Error: " << e.GetError() << std::endl;
		return -1;
	}

	return 0;
}
#include <map>
#include <vector>
#include <string>
#include "symbols.h"
#include "compiler.h"

extern "C"
{
	void yyerror(const char* str); // defined by sheep.y
}

struct _LastError
{
	int line;
	std::string message;
} LastError;

int AddIntSymbol(char* name, int value)
{
	try
	{
		SheepCompiler::Compiler::AddIntSymbol(name, value);
	}
	catch(SheepCompiler::CompilerException& e)
	{
		LastError.line = 0;
		LastError.message = e.GetError();
		return -1;
	}
	
	return 0;
}

int AddFloatSymbol(char* name, float value)
{
	try
	{
		SheepCompiler::Compiler::AddFloatSymbol(name, value);
	}
	catch(SheepCompiler::CompilerException& e)
	{
		LastError.line = 0;
		LastError.message = e.GetError();
		return -1;
	}
	
	return 0;
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

void AddFloatToStack(float f)
{
	SheepCompiler::Compiler::AddFloatToStack(f);
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

void Addition()
{
	SheepCompiler::Compiler::AddAddition();
}

void Subtraction()
{
	SheepCompiler::Compiler::AddSubtraction();
}

void Multiplication()
{
	SheepCompiler::Compiler::AddMultiplication();
}

void Division()
{
	SheepCompiler::Compiler::AddDivision();
}

void GreaterThan()
{
	SheepCompiler::Compiler::GreaterThan();
}

void LessThan()
{
	SheepCompiler::Compiler::LessThan();
}

void And()
{
	SheepCompiler::Compiler::And();
}

void Or()
{
	SheepCompiler::Compiler::Or();
}

void AddIf()
{
	SheepCompiler::Compiler::AddIf();
}

void AddElse()
{
	SheepCompiler::Compiler::AddElse();
}

void EndIf()
{
	SheepCompiler::Compiler::EndIf();
}

void CreateSnippet()
{
	SheepCompiler::Compiler::CreateSnippet();
}


#include <iostream>
#include <fstream>
#include "compiler.h"

std::vector<Compiler::Function> Compiler::m_functions;
std::map<std::string, Compiler::StringConstant> Compiler::m_stringConstants;
unsigned int Compiler::m_currentStringConstantOffset;
std::map<std::string, Compiler::ImportFunction> Compiler::m_importFunctions;

unsigned int Compiler::m_currentFunctionOffset = 0;
std::vector<byte> Compiler::m_instructions;

void Compiler::Init()
{
}

void Compiler::AddStringToStack(const std::string& str)
{
	StringConstant constant;

	// is the string in the list of string constants?
	if (getStringConstant(str, &constant) == false)
		constant = addStringToConstantsList(str);

	// "push" the string
	addByteToInstructions(PushS);
	addIntToInstructions(constant.Offset);
}

void Compiler::AddFunction(const std::string& name)
{
	Function func;
	func.Name = name;
	func.Offset = m_currentFunctionOffset;
	m_functions.push_back(func);

	m_currentFunctionOffset = m_instructions.size();
}

void Compiler::AddFunctionCall(const std::string& str)
{
	std::cout << "compiler adding function call: " << str << std::endl;
	ImportFunction function;

	// is the function in the list of import functions?
	if (getImportFunction(str, &function) == false)
		function = addImportFunction(str);

	addByteToInstructions(CallSysFunctionV);
	addIntToInstructions(function.Index);
}



void Compiler::PrintDebugInfo()
{
	std::cout << "Imports:" << std::endl;
	for (std::map<std::string, ImportFunction>::iterator itr = m_importFunctions.begin();
		itr != m_importFunctions.end(); itr++)
	{
		std::cout << (*itr).second.Name << std::endl;
	}

	std::cout << "Constants:" << std::endl;
	for (std::map<std::string, StringConstant>::iterator itr = m_stringConstants.begin();
		itr != m_stringConstants.end(); itr++)
	{
		std::cout << (*itr).second.String << std::endl;
	}

	std::cout << "Functions:" << std::endl;
	for (unsigned int i = 0; i < m_functions.size(); i++)
	{
		std::cout << m_functions[i].Name << std::endl;
	}
}

void Compiler::addByteToInstructions(byte b)
{
	m_instructions.push_back(b);
}

void Compiler::addIntToInstructions(int i)
{
	byte bytes[4];
	bytes[0] = (byte)((i & 0xff000000) >> 24);
	bytes[1] = (byte)((i & 0x00ff0000) >> 16);
	bytes[2] = (byte)((i & 0x0000ff00) >> 8);
	bytes[3] = (byte)(i & 0x000000ff);

	addByteToInstructions(bytes[0]);
	addByteToInstructions(bytes[1]);
	addByteToInstructions(bytes[2]);
	addByteToInstructions(bytes[3]);
}

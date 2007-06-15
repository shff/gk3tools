#include <iostream>
#include <fstream>
#include <sstream>
#include "compiler.h"

namespace SheepCompiler
{
	std::vector<Compiler::LocalFunction> Compiler::m_functions;
	std::vector<Compiler::StringConstant> Compiler::m_stringConstantsList;
	std::map<std::string, Compiler::StringConstant> Compiler::m_stringConstants;
	unsigned int Compiler::m_currentStringConstantOffset;
	std::map<std::string, Compiler::ImportFunction> Compiler::m_importFunctions;
	std::map<std::string, Compiler::ImportFunction> Compiler::m_validSheepFunctions;
	std::map<std::string, Symbol> Compiler::m_symbols;
	std::vector<Symbol> Compiler::m_symbolList;

	unsigned int Compiler::m_currentFunctionOffset = 0;
	std::vector<byte> Compiler::m_instructions;

	void Compiler::Init()
	{
		loadSheepFunctions();

		// add an empty string to the list of string constants
		addStringToConstantsList("");
	}

	void Compiler::AddIntSymbol(const std::string& name, int value)
	{
		Symbol symbol;

		symbol.Name = name;
		symbol.Type = Symbol_Integer;
		symbol.Value.IntValue = value;
		symbol.Index = m_symbolList.size();

		addToSymbolList(name, symbol);
	}

	void Compiler::AddFloatSymbol(const std::string& name, float value)
	{
		Symbol symbol;

		symbol.Name = name;
		symbol.Type = Symbol_Float;
		symbol.Value.FloatValue = value;
		symbol.Index = m_symbolList.size();

		addToSymbolList(name, symbol);
	}

	void Compiler::AddStringSymbol(const std::string& name, const std::string& value)
	{
		StringConstant constant;
		if (getStringConstant(value, &constant) == false)
			constant = addStringToConstantsList(value);

		Symbol symbol;
		symbol.Name = name;
		symbol.Type = Symbol_String;
		symbol.Value.StringValue = value;
		symbol.Index = m_symbolList.size();

		addToSymbolList(name, symbol);
	}

	void Compiler::AssignSymbolValue(const std::string& name)
	{
		Symbol s;
		if (getFromSymbolList(name, &s) == false)
			throw CompilerException(name + " not defined");

		if (s.Type == Symbol_Integer)
		{
			addByteToInstructions(StoreI);
			addIntToInstructions(s.Value.IntValue);
		}
		else
			throw CompilerException("type not supported yet :(");

	}

	void Compiler::AddIntegerToStack(int i)
	{
		addByteToInstructions(PushI);
		addIntToInstructions(i);
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

		addByteToInstructions(GetString);
	}

	void Compiler::AddLocalValueToStack(const std::string& name)
	{
		Symbol symbol;
		if (getFromSymbolList(name, &symbol) == false)
			throw CompilerException(name + " not defined");

		if (symbol.Type == Symbol_String)
		{
			addByteToInstructions(LoadS);
			addIntToInstructions(symbol.Index);
		}
		else if (symbol.Type == Symbol_Integer)
		{
			addByteToInstructions(LoadI);
			addIntToInstructions(symbol.Index);
		}
		else
			throw CompilerException("sorry, that variable type not supported yet :(");
	}

	void Compiler::AddFunction(const std::string& name)
	{
		LocalFunction func;
		func.Name = name;
		func.Offset = m_currentFunctionOffset;
		m_functions.push_back(func);

		addByteToInstructions(ReturnV);
		m_currentFunctionOffset = m_instructions.size();
	}

	void Compiler::AddFunctionCall(const std::string& str)
	{
		std::cout << "compiler adding function call: " << str << std::endl;
		ImportFunction function;

		// is the function in the list of import functions?
		if (getImportFunction(str, &function) == false)
			function = addImportFunction(str);

		int numParameters = function.Parameters.size();
		addByteToInstructions(PushI);
		addIntToInstructions(numParameters);

		addByteToInstructions(CallSysFunctionV);
		addIntToInstructions(function.Index);

		addByteToInstructions(Pop);
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
		bytes[0] = (byte)(i & 0x000000ff);
		bytes[1] = (byte)((i & 0x0000ff00) >> 8);
		bytes[2] = (byte)((i & 0x00ff0000) >> 16);
		bytes[3] = (byte)((i & 0xff000000) >> 24);

		addByteToInstructions(bytes[0]);
		addByteToInstructions(bytes[1]);
		addByteToInstructions(bytes[2]);
		addByteToInstructions(bytes[3]);
	}

	void Compiler::addToSymbolList(const std::string& name, Symbol symbol)
	{
		if (m_symbols.insert(std::map<std::string, Symbol>::value_type(name, symbol)).first == false)
			throw CompilerException(name + " already defined");

		m_symbolList.push_back(symbol);
	}

	bool Compiler::getFromSymbolList(const std::string& name, Symbol* symbol)
	{
		std::map<std::string, Symbol>::iterator itr = m_symbols.find(name);

		if (itr == m_symbols.end()) return false;

		*symbol = (*itr).second;
		return true;
	}

	void Compiler::loadSheepFunctions()
	{
		std::ifstream file("imports.txt");
		if (!file)
		{
			std::cout << "WARNING: unable to load imports.txt" << std::endl;
			return;
		}

		std::string line;
		std::string returnType, functionName, parameterType;
		while(std::getline(file, line))
		{
			ImportFunction f;

			std::stringstream ss(line);
			
			ss >> returnType >> functionName;
			if (returnType == "void")
				f.ReturnType = Param_Void;
			else if (returnType == "int")
				f.ReturnType = Param_Integer;
			else if (returnType == "float")
				f.ReturnType = Param_Float;
			else if (returnType == "string")
				f.ReturnType = Param_String;

			f.Name = functionName;

			while(true)
			{
				ss >> parameterType;
				if (ss.fail()) break;

				if (parameterType == "int")
					f.Parameters.push_back(Param_Integer);
				else if (parameterType == "float")
					f.Parameters.push_back(Param_Float);
				else if (parameterType == "string")
					f.Parameters.push_back(Param_String);
			}

			m_validSheepFunctions.insert(std::map<std::string, ImportFunction>::value_type(f.Name, f));
		}

		file.close();
	}
}

#include <iostream>
#include <fstream>
#include <sstream>
#include "compiler.h"

extern "C"
{
int yyparse();
}

namespace SheepCompiler
{
	std::vector<LocalFunction> Compiler::m_functions;
	std::vector<StringConstant> Compiler::m_stringConstantsList;
	std::map<std::string, StringConstant> Compiler::m_stringConstants;
	unsigned int Compiler::m_currentStringConstantOffset;
	std::map<std::string, Compiler::ImportFunction> Compiler::m_importFunctions;
	std::map<std::string, Compiler::ImportFunction> Compiler::m_validSheepFunctions;
	std::map<std::string, Symbol> Compiler::m_symbols;
	std::vector<Symbol> Compiler::m_symbolList;
	std::stack<SymbolType> Compiler::m_stackTypes;
	std::stack<int> Compiler::m_ifOffsetStack;

	unsigned int Compiler::m_currentFunctionOffset = 0;
	std::vector<byte> Compiler::m_instructions;

	void Compiler::Init()
	{
		loadSheepFunctions();

		// add an empty string to the list of string constants
		addStringToConstantsList("");
	}

	int Compiler::Compile(const std::string& inputFile)
	{
		if (freopen(inputFile.c_str(), "r", stdin) == NULL)
		{
			printf("Error: Unable to open file");
			return -1;
		}

		yyparse();

		return 0;
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

		if (m_stackTypes.empty())
			throw CompilerException("wtf? nothing on the type stack?");

		if (s.Type == Symbol_Integer)
		{
			if (m_stackTypes.top() == Symbol_Float)
			{
				addByteToInstructions(FToI);
				addIntToInstructions(0);
			}
			else if (m_stackTypes.top() == Symbol_String)
			{
				throw CompilerException("Can't put a string into an integer");
			}

			addByteToInstructions(StoreI);
			addIntToInstructions(s.Index);
		}
		else if (s.Type == Symbol_Float && m_stackTypes.top() == Symbol_Float)
		{
			if (m_stackTypes.top() == Symbol_Integer)
			{
				addByteToInstructions(IToF);
				addIntToInstructions(0);
			}
			else if (m_stackTypes.top() == Symbol_String)
			{
				throw CompilerException("Can't put a string into an integer");
			}

			addByteToInstructions(StoreF);
			addIntToInstructions(s.Index);
		}
		else
			throw CompilerException("type not supported yet :(");
		
		m_stackTypes.pop();

	}

	void Compiler::AddIntegerToStack(int i)
	{
		addByteToInstructions(PushI);
		addIntToInstructions(i);

		m_stackTypes.push(Symbol_Integer);
	}

	void Compiler::AddFloatToStack(float f)
	{
		addByteToInstructions(PushF);
		addIntToInstructions(f);

		m_stackTypes.push(Symbol_Float);
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

		m_stackTypes.push(Symbol_String);
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
	
		m_stackTypes.push(symbol.Type);
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

		// check parameter types
		for (int i = 0; i < numParameters; i++)
		{
			if (m_stackTypes.top() != function.Parameters[i])
				throw CompilerException("Parameter is not the correct type");

			m_stackTypes.pop();
		}

		addByteToInstructions(PushI);
		addIntToInstructions(numParameters);

		addByteToInstructions(CallSysFunctionV);
		addIntToInstructions(function.Index);

		addByteToInstructions(Pop);
	}

	void Compiler::AddAddition()
	{
		addMathOperator(AddI, AddF);
	}

	void Compiler::AddSubtraction()
	{
		addMathOperator(SubtractI, SubtractF);
	}

	void Compiler::AddMultiplication()
	{
		addMathOperator(MultiplyI, MultiplyF);
	}

	void Compiler::AddDivision()
	{
		addMathOperator(DivideI, DivideF);
	}

	void Compiler::AddIf()
	{
		addByteToInstructions(BranchIfZero);

		m_ifOffsetStack.push(m_instructions.size());
		addIntToInstructions(0xffffffff);
	}

	void Compiler::AddElse()
	{
		// add a branch placeholder to jump over the Else block
		addByteToInstructions(Branch);
		addIntToInstructions(0xffffffff);

		// set the original If's branch to just after that new branch
		int offset = m_ifOffsetStack.top();
		m_ifOffsetStack.pop();
		modifyIntInstruction(offset, m_instructions.size());

		// save the offset of the new branch 
		// so we can come back later and fill it in
		m_ifOffsetStack.push(m_instructions.size()-4);
	}

	void Compiler::EndIf()
	{
		int offset = m_ifOffsetStack.top();
		m_ifOffsetStack.pop();
		modifyIntInstruction(offset, m_instructions.size());
	}
	
	void Compiler::GreaterThan()
	{
		addComparisonOperator(IsGreaterI, IsGreaterF);
	}

	void Compiler::LessThan()
	{
		addComparisonOperator(IsLessI, IsLessF);
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

	void Compiler::addMathOperator(byte intOp, byte floatOp)
	{
		SymbolType type2 = m_stackTypes.top();
		SymbolType type1 = m_stackTypes.top();

		m_stackTypes.pop();
		m_stackTypes.pop();

		bool error = false;
		
		if (type1 == Symbol_Integer)
		{
			if (type2 == Symbol_Integer)
			{
				addByteToInstructions(intOp);
				m_stackTypes.push(Symbol_Integer);
			}
			else if (type2 == Symbol_Float)
			{
				addByteToInstructions(IToF);
				addIntToInstructions(1);

				addByteToInstructions(floatOp);
				
				m_stackTypes.push(Symbol_Float);
			}
			else
			{
				error = true;
			}
		}
		else if (type1 == Symbol_Float)
		{
			if (type2 == Symbol_Float)
			{
				addByteToInstructions(floatOp);
				m_stackTypes.push(Symbol_Float);
			}
			else if (type2 == Symbol_Integer)
			{
				addByteToInstructions(IToF);
				addIntToInstructions(0);

				addByteToInstructions(floatOp);

				m_stackTypes.push(Symbol_Float);
			}
			else
			{
				error = true;
			}
		}
		else
		{
			error = true;
		}

		if (error)
			throw CompilerException("Addition between these two types is not defined");
	}

	void Compiler::addComparisonOperator(byte intOp, byte floatOp)
	{
		SymbolType type2 = m_stackTypes.top();
		SymbolType type1 = m_stackTypes.top();

		m_stackTypes.pop();
		m_stackTypes.pop();

		bool error = false;

		if (type1 == Symbol_Integer)
		{
			if (type2 == Symbol_Integer)
			{
				addByteToInstructions(intOp);
				m_stackTypes.push(Symbol_Integer);
			}
			else if (type2 == Symbol_Float)
			{
				addByteToInstructions(IToF);
				addIntToInstructions(1);

				addByteToInstructions(floatOp);
				m_stackTypes.push(Symbol_Integer);
			}
			else
			{
				error = true;
			}
		}
		else if (type1 == Symbol_Float)
		{
			if (type2 == Symbol_Float)
			{
				addByteToInstructions(floatOp);
				m_stackTypes.push(Symbol_Integer);
			}
			else if (type2 == Symbol_Integer)
			{
				addByteToInstructions(IToF);
				addIntToInstructions(0);

				addByteToInstructions(floatOp);
				m_stackTypes.push(Symbol_Integer);
			}
			else
			{
				error = true;
			}
		}
		else
		{
			error = true;
		}

		if (error)
			throw CompilerException("Comparison between these two types is not defined");
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

	void Compiler::modifyIntInstruction(int offset, int value)
	{
		byte bytes[4];
		bytes[0] = (byte)(value & 0x000000ff);
		bytes[1] = (byte)((value & 0x0000ff00) >> 8);
		bytes[2] = (byte)((value & 0x00ff0000) >> 16);
		bytes[3] = (byte)((value & 0xff000000) >> 24);

		m_instructions[offset+0] = bytes[0];
		m_instructions[offset+1] = bytes[1];
		m_instructions[offset+2] = bytes[2];
		m_instructions[offset+3] = bytes[3];
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
				f.ReturnType = Symbol_Void;
			else if (returnType == "int")
				f.ReturnType = Symbol_Integer;
			else if (returnType == "float")
				f.ReturnType = Symbol_Float;
			else if (returnType == "string")
				f.ReturnType = Symbol_String;

			f.Name = functionName;

			while(true)
			{
				ss >> parameterType;
				if (ss.fail()) break;

				if (parameterType == "int")
					f.Parameters.push_back(Symbol_Integer);
				else if (parameterType == "float")
					f.Parameters.push_back(Symbol_Float);
				else if (parameterType == "string")
					f.Parameters.push_back(Symbol_String);
			}

			m_validSheepFunctions.insert(std::map<std::string, ImportFunction>::value_type(f.Name, f));
		}

		file.close();
	}
}

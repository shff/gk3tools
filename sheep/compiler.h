
#ifndef COMPILER_H
#define COMPILER_H

#include <iostream>
#include <vector>
#include <map>
#include <string>

namespace SheepCompiler
{
	typedef unsigned char byte;

	enum Instruction
	{
		SitnSpin             = 0x00,
		CallSysFunctionV     = 0x02,
		CallSysFunctionI     = 0x03,
		Branch,
		BranchIfZero,
		
		BeginWait,
		EndWait,
		ReturnV              = 0x0b,

		StoreI               = 0x0D,
		LoadI                = 0x10,
		LoadS                = 0x12,
		PushI                = 0x13,
		PushS                = 0x15,
		Pop                  = 0x16,

		IsEqualI,
		IsLessI,

		IToF,
		And,
		Or,
		GetString            = 0x33
	};

	enum ParameterType
	{
		Param_Void,
		Param_Integer,
		Param_Float,
		Param_String
	};

	enum SymbolType
	{
		Symbol_Void,
		Symbol_Integer,
		Symbol_Float,
		Symbol_String
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
		int Index;
	};

	class CompilerException
	{
	public:
		CompilerException(const std::string& error)
		{
			m_error = error;
		}

		std::string GetError() { return m_error; }

	private:
		std::string m_error;
	};

	class Compiler
	{
	public:

		static void Init();

		static void AddIntSymbol(const std::string& name, int value);
		static void AddFloatSymbol(const std::string& name, float value);
		static void AddStringSymbol(const std::string& name, const std::string& value);

		static void AssignSymbolValue(const std::string& name);

		static void AddFunction(const std::string& name);
		
		static void AddIntegerToStack(int i);
		static void AddStringToStack(const std::string& string);
		static void AddLocalValueToStack(const std::string& name);
		static void AddFunctionCall(const std::string& function);

		static void WriteCompiledSheep(const std::string& outputFile);
		static void PrintDebugInfo();

	private:

		struct StringConstant
		{
			std::string String;
			unsigned int Offset;
		};

		struct ImportFunction
		{
			std::string Name;
			ParameterType ReturnType;
			std::vector<ParameterType> Parameters;

			unsigned int Index;
		};

		struct LocalFunction
		{
			std::string Name;
			unsigned int Offset;
		};

		static bool getStringConstant(const std::string& str, StringConstant* constant)
		{
			std::map<std::string, StringConstant>::iterator itr
				= m_stringConstants.find(str);
			
			if (itr == m_stringConstants.end())
				return false;
			
			*constant = (*itr).second;
			return true;
		}

		static StringConstant addStringToConstantsList(const std::string& str)
		{
			StringConstant constant;
			constant.String = str;
			constant.Offset = m_currentStringConstantOffset;

			if (m_stringConstants.insert(std::map<std::string, StringConstant>::value_type(str, constant)).first == false)
				throw CompilerException("String constant already defined");
			m_stringConstantsList.push_back(constant);
			
			m_currentStringConstantOffset += str.length() + 1;

			return constant;
		}

		static bool getImportFunction(const std::string& str, ImportFunction* function)
		{
			std::map<std::string, ImportFunction>::iterator itr
				= m_importFunctions.find(str);
			
			if (itr == m_importFunctions.end())
				return false;
			
			*function = (*itr).second;
			return true;
		}

		static ImportFunction addImportFunction(const std::string& function)
		{
			ImportFunction f;

			std::map<std::string, ImportFunction>::iterator itr = m_validSheepFunctions.find(function);
			if (itr == m_validSheepFunctions.end())
			{
				std::cout << function << " not declared" << std::endl;
				return f;
			}

			f = (*itr).second;
			f.Index = m_importFunctions.size();

			m_importFunctions.insert(std::map<std::string, ImportFunction>::value_type(function, f));

			return f;
		}

		static void addByteToInstructions(byte b);
		static void addIntToInstructions(int i);
		static void addFloatToInstructions(float f);

		static LocalFunction m_currentFunction;
		static std::vector<LocalFunction> m_functions;

		static unsigned int m_currentStringConstantOffset;
		static std::map<std::string, StringConstant> m_stringConstants;
		static std::vector<StringConstant> m_stringConstantsList;

		static std::map<std::string, ImportFunction> m_importFunctions;

		static unsigned int m_currentFunctionOffset;
		static std::vector<byte> m_instructions;

		static void loadSheepFunctions();
		static std::map<std::string, ImportFunction> m_validSheepFunctions;
	
		static void addToSymbolList(const std::string& name, Symbol symbol);
		static bool getFromSymbolList(const std::string& name, Symbol* symbol);
		static std::map<std::string, Symbol> m_symbols;
		static std::vector<Symbol> m_symbolList;
	};

}

#endif // COMPILER_H

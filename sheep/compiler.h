
#ifndef COMPILER_H
#define COMPILER_H

#include <iostream>
#include <vector>
#include <map>
#include <string>

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
	ReturnV,

	PushI,
	PushS                = 0x15,
	Pop,

	IsEqualI,
	IsLessI,

	IToF,
	And,
	Or,
	GetString
};


class Compiler
{
public:

	static void Init();

	static void AddFunction(const std::string& name);
	
	static void AddStringToStack(const std::string& string);
	static void AddFunctionCall(const std::string& function);

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
		unsigned int Index;
	};

	struct Function
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

		m_stringConstants.insert(std::map<std::string, StringConstant>::value_type(str, constant));
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
		f.Name = function;
		f.Index = m_importFunctions.size();

		m_importFunctions.insert(std::map<std::string, ImportFunction>::value_type(function, f));

		return f;
	}

	static void addByteToInstructions(byte b);
	static void addIntToInstructions(int i);
	static void addFloatToInstructions(float f);

	static Function m_currentFunction;
	static std::vector<Function> m_functions;

	static unsigned int m_currentStringConstantOffset;
	static std::map<std::string, StringConstant> m_stringConstants;

	static std::map<std::string, ImportFunction> m_importFunctions;

	static unsigned int m_currentFunctionOffset;
	static std::vector<byte> m_instructions;
};

#endif // COMPILER_H

#include <cassert>
#include <fstream>
#include <sstream>
#include <iomanip>
//#include "compiler.h"
#include "sheepTypes.h"
#include "sheepCodeBuffer.h"
#include "sheepDisassembler.h"
#include "sheepCodeGenerator.h"
//#include "sheepfile.h"
#include "sheepException.h"

namespace Sheep
{
	#define READ4(p) file.read((char*)p, 4);
	#define READ2(p) file.read((char*)p, 2);
	#define READ1(p) file.read((char*)p, 1);

	Disassembly* Disassembler::GetDisassembly(IntermediateOutput* intermediateOutput)
	{
        std::stringstream output;

		// imports
		output << std::endl << "SysImports: " << std::endl;
		for (unsigned int i = 0; i < intermediateOutput->Imports.size(); i++)
		{
			output << "\t" << i << "\t- " << intermediateOutput->Imports[i].Name << "(";

			for (unsigned int j = 0; j < intermediateOutput->Imports[i].Parameters.size(); j++)
			{
				if (j > 0) output << ", ";
				SheepSymbolType parameterType = intermediateOutput->Imports[i].Parameters[j];

				if (parameterType == SheepSymbolType::Void)
					output << "void";
				else if (parameterType == SheepSymbolType::Int)
					output << "int";
				else if (parameterType == SheepSymbolType::Float)
					output << "float";
				else if (parameterType == SheepSymbolType::String)
					output << "string";
				else
					output << "??";
			}

			output << ")" << std::endl;
		}

		// string constants
		output << std::endl << "StringConsts" << std::endl;

		for (unsigned int i = 0; i < intermediateOutput->Constants.size(); i++)
		{
			output << "\t" << i << "\t" << intermediateOutput->Constants[i].Offset
				<< "\t-\"" << intermediateOutput->Constants[i].Value << "\"" << std::endl;
		}

		// symbols
		output << std::endl << "Variables" << std::endl;
		
		for (unsigned int i = 0; i < intermediateOutput->Symbols.size(); i++)
		{	
			output << "\t" << i << "\t";
			SheepSymbolType type = intermediateOutput->Symbols[i].Type;
			
			if (type == SheepSymbolType::Void)
				output << "void";
			else if (type == SheepSymbolType::Int)
				output << "int";
			else if (type == SheepSymbolType::Float)
				output << "float";
			else if (type == SheepSymbolType::String)
				output << "string";
			else
				output << "??";
			
			output << " " << intermediateOutput->Symbols[i].Name << " = ??" << std::endl;
		}

		// functions
		output << std::endl << "Functions" << std::endl;

		for (unsigned int i = 0; i < intermediateOutput->Functions.size(); i++)
		{

			output << "\t" << i << "\t(" << intermediateOutput->Functions[i].CodeOffset << ")\t"
				<< "\t-\"" << intermediateOutput->Functions[i].Name << "\"" << std::endl;
		}

		// code
		output << std::endl << "Code" << std::endl;
		for (unsigned int i = 0; i < intermediateOutput->Functions.size(); i++)
		{
			output << "\t" << i << " - " << intermediateOutput->Functions[i].Name << std::endl;

		
			const byte* data = (byte*)intermediateOutput->Functions[i].Code->GetData();

			unsigned int currentCodeOffset = 0;
			while(currentCodeOffset < intermediateOutput->Functions[i].Code->GetSize())
			{	
				output << "\t" << currentCodeOffset + intermediateOutput->Functions[i].CodeOffset << ":\t";

				currentCodeOffset += printNextInstruction(&data[currentCodeOffset], output, intermediateOutput->Imports, intermediateOutput->Constants);
			}

			output << std::endl;
		}

		return SHEEP_NEW Disassembly(output.str().c_str());
	}

	unsigned int Disassembler::printNextInstruction(const byte* code, std::ostream& output, std::vector<SheepImport>& imports, std::vector<SheepStringConstant>& constants)
	{
#define PRINT_INT_OP(o,b1,b2,b3,b4,s) output << std::setw(2) << std::setbase(16) << o << " " << b1 << " " << b2 << " " << b3 << " " << b4 << "\t " << s << std::endl;

		unsigned char op;
		unsigned char param[4];
		memcpy(&op, code, 1);
		unsigned int iop = op;

		if (op == (unsigned char)SheepInstruction::SitnSpin)
		{
			printDisassembly(output, op, "SitnSpin");
			return 1;
		}
		else if (op == (unsigned char)SheepInstruction::CallSysFunctionV)
		{
			memcpy(param, &code[1], 4);

			int function = convertBytesToInt(param[0], param[1], param[2], param[3]);

			printDisassembly(output, op, param[0], param[1], param[2], param[3],
				"CallSysFunctionV", imports[function].Name);

			return 5;
		}
		else if (op == (unsigned char)SheepInstruction::CallSysFunctionI)
		{
			memcpy(param, &code[1], 4);

			int function = convertBytesToInt(param[0], param[1], param[2], param[3]);

			printDisassembly(output, op, param[0], param[1], param[2], param[3],
				"CallSysFunctionI", imports[function].Name);
			
			return 5;
		}
		else if (op == (unsigned char)SheepInstruction::CallSysFunctionF)
		{
			memcpy(param, &code[1], 4);

			int function = convertBytesToInt(param[0], param[1], param[2], param[3]);

			printDisassembly(output, op, param[0], param[1], param[2], param[3],
				"CallSysFunctionF", imports[function].Name);
			
			return 5;
		}
		else if (op == (unsigned char)SheepInstruction::CallSysFunctionS)
		{
			memcpy(param, &code[1], 4);

			int function = convertBytesToInt(param[0], param[1], param[2], param[3]);

			printDisassembly(output, op, param[0], param[1], param[2], param[3],
				"CallSysFunctionS", imports[function].Name);
			
			return 5;
		}
		else if (op == (unsigned char)SheepInstruction::Branch)
		{
			memcpy(param, &code[1], 4);
			printDisassembly(output, op, param[0], param[1], param[2], param[3],
				"Branch");
			return 5;
		}
		else if (op == (unsigned char)SheepInstruction::BranchGoto)
		{
			memcpy(param, &code[1], 4);
			printDisassembly(output, op, param[0], param[1], param[2], param[3],
				"BranchGoto");
			return 5;
		}
		else if (op == (unsigned char)SheepInstruction::BranchIfZero)
		{
			memcpy(param, &code[1], 4);
			printDisassembly(output, op, param[0], param[1], param[2], param[3],
				"BranchIfZero");
			return 5;
		}
		else if (op == (unsigned char)SheepInstruction::BeginWait)
		{
			printDisassembly(output, op, "BeginWait");
			return 1;
		}
		else if (op == (unsigned char)SheepInstruction::EndWait)
		{
			printDisassembly(output, op, "EndWait");
			return 1;
		}
		else if (op == (unsigned char)SheepInstruction::ReturnV)
		{
			printDisassembly(output, op, "ReturnV");
			return 1;
		}
		else if (op == (unsigned char)SheepInstruction::ReturnI)
		{
			printDisassembly(output, op, "ReturnI");
			return 1;
		}
		else if (op == (unsigned char)SheepInstruction::ReturnF)
		{
			printDisassembly(output, op, "ReturnF");
			return 1;
		}
		else if (op == (unsigned char)SheepInstruction::ReturnS)
		{
			printDisassembly(output, op, "ReturnS");
			return 1;
		}
		else if (op == (unsigned char)SheepInstruction::StoreI)
		{
			memcpy(param, &code[1], 4);
			printDisassembly(output, op, param[0], param[1], param[2], param[3], "StoreI");
			return 5;
		}
		else if (op == (unsigned char)SheepInstruction::StoreF)
		{
			memcpy(param, &code[1], 4);
			printDisassembly(output, op, param[0], param[1], param[2], param[3], "StoreF");
			return 5;
		}
		else if (op == (unsigned char)SheepInstruction::StoreS)
		{
			memcpy(param, &code[1], 4);
			printDisassembly(output, op, param[0], param[1], param[2], param[3], "StoreS");
			return 5;
		}
		else if (op == (unsigned char)SheepInstruction::LoadI)
		{
			memcpy(param, &code[1], 4);
			printDisassembly(output, op, param[0], param[1], param[2], param[3], "LoadI");
			return 5;
		}
		else if (op == (unsigned char)SheepInstruction::LoadF)
		{
			memcpy(param, &code[1], 4);
			printDisassembly(output, op, param[0], param[1], param[2], param[3], "LoadF");
			return 5;
		}
		else if (op == (unsigned char)SheepInstruction::LoadS)
		{
			memcpy(param, &code[1], 4);
			printDisassembly(output, op, param[0], param[1], param[2], param[3], "LoadS");
			return 5;
		}
		else if (op == (unsigned char)SheepInstruction::PushI)
		{
			memcpy(param, &code[1], 4);
			printDisassembly(output, op, param[0], param[1], param[2], param[3], "PushI");
			return 5;
		}
		else if (op == (unsigned char)SheepInstruction::PushF)
		{
			memcpy(param, &code[1], 4);
			printDisassembly(output, op, param[0], param[1], param[2], param[3], "PushF");
			return 5;
		}
		else if (op == (unsigned char)SheepInstruction::PushS)
		{
			memcpy(param, &code[1], 4);

			unsigned int offset = convertBytesToInt(param[0], param[1], param[2], param[3]);

			std::string constant = "";
			for (unsigned int i = 0; i < constants.size(); i++)
			{
				if (constants[i].Offset == offset)
				{
					constant = constants[i].Value;
					break;
				}
			}

			printDisassembly(output, op, param[0], param[1], param[2], param[3], "PushS", constant);
			return 5;
		}
		else if (op == (unsigned char)SheepInstruction::Pop)
		{
			printDisassembly(output, op, "Pop");
			return 1;
		}
		else if (op == (unsigned char)SheepInstruction::AddI)
		{
			printDisassembly(output, op, "AddI");
			return 1;
		}
		else if (op == (unsigned char)SheepInstruction::AddF)
		{
			printDisassembly(output, op, "AddF");
			return 1;
		}
		else if (op == (unsigned char)SheepInstruction::SubtractI)
		{
			printDisassembly(output, op, "SubtractI");
			return 1;
		}
		else if (op == (unsigned char)SheepInstruction::SubtractF)
		{
			printDisassembly(output, op, "SubtractF");
			return 1;
		}
		else if (op == (unsigned char)SheepInstruction::MultiplyI)
		{
			printDisassembly(output, op, "MultiplyI");
			return 1;
		}
		else if (op == (unsigned char)SheepInstruction::MultiplyF)
		{
			printDisassembly(output, op, "MultiplyF");
			return 1;
		}
		else if (op == (unsigned char)SheepInstruction::DivideI)
		{
			printDisassembly(output, op, "DivideI");
			return 1;
		}
		else if (op == (unsigned char)SheepInstruction::DivideF)
		{
			printDisassembly(output, op, "DivideF");
			return 1;
		}
		else if (op == (unsigned char)SheepInstruction::NegateI)
		{
			printDisassembly(output, op, "NegateI");
			return 1;
		}
		else if (op == (unsigned char)SheepInstruction::NegateF)
		{
			printDisassembly(output, op, "NegateF");
			return 1;
		}
		else if (op == (unsigned char)SheepInstruction::IsEqualI)
		{
			printDisassembly(output, op, "IsEqualI");
			return 1;
		}
		else if (op == (unsigned char)SheepInstruction::IsEqualF)
		{
			printDisassembly(output, op, "IsEqualF");
			return 1;
		}
		else if (op == (unsigned char)SheepInstruction::NotEqualI)
		{
			printDisassembly(output, op, "NotEqualI");
			return 1;
		}
		else if (op == (unsigned char)SheepInstruction::NotEqualF)
		{
			printDisassembly(output, op, "NotEqualF");
			return 1;
		}
		else if (op == (unsigned char)SheepInstruction::IsGreaterI)
		{
			printDisassembly(output, op, "IsGreaterI");
			return 1;
		}
		else if (op == (unsigned char)SheepInstruction::IsGreaterF)
		{
			printDisassembly(output, op, "IsGreaterF");
			return 1;
		}
		else if (op == (unsigned char)SheepInstruction::IsLessI)
		{
			printDisassembly(output, op, "IsLessI");
			return 1;
		}
		else if (op == (unsigned char)SheepInstruction::IsLessF)
		{
			printDisassembly(output, op, "IsLessF");
			return 1;
		}
		else if (op == (unsigned char)SheepInstruction::IsGreaterEqualI)
		{
			printDisassembly(output, op, "IsGreaterEqualI");
			return 1;
		}
		else if (op == (unsigned char)SheepInstruction::IsGreaterEqualF)
		{
			printDisassembly(output, op, "IsGreaterEqualF");
			return 1;
		}
		else if (op == (unsigned char)SheepInstruction::IsLessEqualI)
		{
			printDisassembly(output, op, "IsLessEqualI");
			return 1;
		}
		else if (op == (unsigned char)SheepInstruction::IsLessEqualF)
		{
			printDisassembly(output, op, "IsLessEqualF");
			return 1;
		}
		else if (op == (unsigned char)SheepInstruction::IToF)
		{
			memcpy(param, &code[1], 4);
			printDisassembly(output, op, param[0], param[1], param[2], param[3], "IToF");
			return 5;
		}
		else if (op == (unsigned char)SheepInstruction::FToI)
		{
			memcpy(param, &code[1], 4);
			printDisassembly(output, op, param[0], param[1], param[2], param[3], "FToI");
			return 5;
		}
		else if (op == (unsigned char)SheepInstruction::And)
		{
			printDisassembly(output, op, "And");
			return 1;
		}
		else if (op == (unsigned char)SheepInstruction::Or)
		{
			printDisassembly(output, op, "Or");
			return 1;
		}
		else if (op == (unsigned char)SheepInstruction::Not)
		{
			printDisassembly(output, op, "Not");
			return 1;
		}
		else if (op == (unsigned char)SheepInstruction::GetString)
		{
			printDisassembly(output, op, "GetString");
			return 1;
		}
		else
		{
			output << "WARNING: UNKNOWN OPCODE: " << iop << std::endl;
			return 1;
		}
	}

	void Disassembler::printDisassembly(std::ostream& output, int op, const std::string& name)
	{
		output << std::setw(2) << std::setbase(16) << op << " " << "           \t " << name << std::endl;
	}
		
	void Disassembler::printDisassembly(std::ostream& output, int op,
		int byte1, int byte2, int byte3, int byte4,
		const std::string& name, unsigned int param)
	{
		assert(byte1 < 256 && byte1 >= 0);
		assert(byte2 < 256 && byte2 >= 0);
		assert(byte3 < 256 && byte3 >= 0);
		assert(byte4 < 256 && byte4 >= 0);

		output << std::setw(2) << std::setbase(16) << op << " " <<
			byte1 << " " << byte2 << " " << byte3 << " " << byte4 <<
			"\t " << name << " " << param << std::endl;
	}

	void Disassembler::printDisassembly(std::ostream& output, int op,
		int byte1, int byte2, int byte3, int byte4,
		const std::string& name)
	{
		unsigned int p = convertBytesToInt(byte1, byte2, byte3, byte4);

		printDisassembly(output, op, byte1, byte2, byte3, byte4,
			name, p);
	}

	void Disassembler::printDisassembly(std::ostream& output, int op,
		int byte1, int byte2, int byte3, int byte4,
		const std::string& name, const std::string param)
	{
		output << std::setw(2) << std::setbase(16) << op << " " <<
			byte1 << " " << byte2 << " " << byte3 << " " << byte4 <<
			"\t " << name << " \"" << param << "\"" << std::endl;
	}
}
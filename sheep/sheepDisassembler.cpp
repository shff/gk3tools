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

namespace SheepCompiler
{
	#define READ4(p) file.read((char*)p, 4);
	#define READ2(p) file.read((char*)p, 2);
	#define READ1(p) file.read((char*)p, 1);

    std::string Disassembler::GetDisassembly(const std::string& inputFile)
    {
		std::ifstream file(inputFile.c_str(), std::ios_base::binary);
		if (!file)
		{
			throw SheepException("Unable to open input file", SHEEP_ERR_FILE_NOT_FOUND);
		}

		unsigned int fileSize = getFileSize(file);

		//byte* buffer = new byte[fileSize];
		byte* buffer = SHEEP_NEW_ARRAY(byte, fileSize);
		file.read((char*)buffer, fileSize);
		file.close();

        std::string disassembly = GetDisassembly(buffer, fileSize);

        SHEEP_DELETE_ARRAY(buffer);

        return disassembly;
    }

	std::string Disassembler::GetDisassembly(const byte* data, int length)
	{
        std::stringstream output;

		SheepFileReader* reader = SHEEP_NEW SheepFileReader(data, length);
		IntermediateOutput* io = reader->GetIntermediateOutput();
		
		// imports
		output << std::endl << "SysImports: " << std::endl;
		for (int i = 0; i < io->Imports.size(); i++)
		{
			output << "\t" << i << "\t- " << io->Imports[i].Name << "(";

			for (int j = 0; j < io->Imports[i].Parameters.size(); j++)
			{
				if (j > 0) output << ", ";
				int parameterType = io->Imports[i].Parameters[j];

				if (parameterType == SYM_VOID)
					output << "void";
				else if (parameterType == SYM_INT)
					output << "int";
				else if (parameterType == SYM_FLOAT)
					output << "float";
				else if (parameterType == SYM_STRING)
					output << "string";
				else
					output << "??";
			}

			output << ")" << std::endl;
		}

		// string constants
		output << std::endl << "StringConsts" << std::endl;

		for (unsigned int i = 0; i < io->Constants.size(); i++)
		{
			output << "\t" << i << "\t" << io->Constants[i].Offset
				<< "\t-\"" << io->Constants[i].Value << "\"" << std::endl;
		}

		// symbols
		output << std::endl << "Variables" << std::endl;
		
		for (unsigned int i = 0; i < io->Symbols.size(); i++)
		{	
			output << "\t" << i << "\t";
			int type = io->Symbols[i].Type;
			
			if (type == SYM_VOID)
				output << "void";
			else if (type == SYM_INT)
				output << "int";
			else if (type == SYM_FLOAT)
				output << "float";
			else if (type == SYM_STRING)
				output << "string";
			else
				output << "??";
			
			output << " " << io->Symbols[i].Name << " = ??" << std::endl;
		}

		// functions
		output << std::endl << "Functions" << std::endl;

		for (unsigned int i = 0; i < io->Functions.size(); i++)
		{

			output << "\t" << i << "\t(" << io->Functions[i].CodeOffset << ")\t"
				<< "\t-\"" << io->Functions[i].Name << "\"" << std::endl;
		}

		// code
		output << std::endl << "Code" << std::endl;
		for (unsigned int i = 0; i < io->Functions.size(); i++)
		{
			output << "\t" << i << " - " << io->Functions[i].Name << std::endl;

		
			const byte* data = (byte*)io->Functions[i].Code->GetData();

			unsigned int currentCodeOffset = 0;
			while(currentCodeOffset < io->Functions[i].Code->GetSize())
			{	
				output << "\t" << currentCodeOffset + io->Functions[i].CodeOffset << ":\t";

				currentCodeOffset += printNextInstruction(&data[currentCodeOffset], output, io->Imports, io->Constants);
			}

			output << std::endl;
		}

		SHEEP_DELETE(io);
		SHEEP_DELETE(reader);

		return output.str();
	}

	SectionHeader Disassembler::readSectionHeader(std::ifstream& file, const std::string& name)
	{
		SectionHeader header;
		
		strncpy(header.Label, name.c_str(), 12);
		READ4(&header.ExtraOffset);
		READ4(&header.DataOffset);
		READ4(&header.DataSize);
		READ4(&header.DataCount);

		//header.OffsetArray = new unsigned int[header.DataCount];
		header.OffsetArray = SHEEP_NEW_ARRAY(unsigned int, header.DataCount);

		for (unsigned int i = 0; i < header.DataCount; i++)
			READ4(&header.OffsetArray[i]);

		return header;
	}

	std::string Disassembler::readString(std::ifstream& file)
	{
		char c;
		std::string str;
		
		file.read(&c, 1);
		while(c != 0)
		{
			str.push_back(c);
			file.read(&c, 1);
		}

		return str;
	}

	std::string Disassembler::readString(std::ifstream& file, unsigned int length)
	{
		char c;
		std::string str;

		for (unsigned int i = 0; i < length; i++)
		{
			file.read(&c, 1);
			if (c == 0) break;
				
			str.push_back(c);
		}

		return str;
	}

	unsigned int Disassembler::printNextInstruction(const byte* code, std::ostream& output, std::vector<SheepImport>& imports, std::vector<SheepStringConstant>& constants)
	{
#define PRINT_INT_OP(o,b1,b2,b3,b4,s) output << std::setw(2) << std::setbase(16) << o << " " << b1 << " " << b2 << " " << b3 << " " << b4 << "\t " << s << std::endl;

		unsigned char op;
		unsigned char param[4];
		memcpy(&op, code, 1);
		unsigned int iop = op;

		if (op == SitnSpin)
		{
			printDisassembly(output, op, "SitnSpin");
			return 1;
		}
		else if (op == CallSysFunctionV)
		{
			memcpy(param, &code[1], 4);

			int function = convertBytesToInt(param[0], param[1], param[2], param[3]);

			printDisassembly(output, op, param[0], param[1], param[2], param[3],
				"CallSysFunctionV", imports[function].Name);

			return 5;
		}
		else if (op == CallSysFunctionI)
		{
			memcpy(param, &code[1], 4);

			int function = convertBytesToInt(param[0], param[1], param[2], param[3]);

			printDisassembly(output, op, param[0], param[1], param[2], param[3],
				"CallSysFunctionI", imports[function].Name);
			
			return 5;
		}
		else if (op == CallSysFunctionF)
		{
			memcpy(param, &code[1], 4);

			int function = convertBytesToInt(param[0], param[1], param[2], param[3]);

			printDisassembly(output, op, param[0], param[1], param[2], param[3],
				"CallSysFunctionF", imports[function].Name);
			
			return 5;
		}
		else if (op == CallSysFunctionS)
		{
			memcpy(param, &code[1], 4);

			int function = convertBytesToInt(param[0], param[1], param[2], param[3]);

			printDisassembly(output, op, param[0], param[1], param[2], param[3],
				"CallSysFunctionS", imports[function].Name);
			
			return 5;
		}
		else if (op == Branch)
		{
			memcpy(param, &code[1], 4);
			printDisassembly(output, op, param[0], param[1], param[2], param[3],
				"Branch");
			return 5;
		}
		else if (op == BranchGoto)
		{
			memcpy(param, &code[1], 4);
			printDisassembly(output, op, param[0], param[1], param[2], param[3],
				"BranchGoto");
			return 5;
		}
		else if (op == BranchIfZero)
		{
			memcpy(param, &code[1], 4);
			printDisassembly(output, op, param[0], param[1], param[2], param[3],
				"BranchIfZero");
			return 5;
		}
		else if (op == BeginWait)
		{
			printDisassembly(output, op, "BeginWait");
			return 1;
		}
		else if (op == EndWait)
		{
			printDisassembly(output, op, "EndWait");
			return 1;
		}
		else if (op == ReturnV)
		{
			printDisassembly(output, op, "ReturnV");
			return 1;
		}
		else if (op == ReturnI)
		{
			printDisassembly(output, op, "ReturnI");
			return 1;
		}
		else if (op == ReturnF)
		{
			printDisassembly(output, op, "ReturnF");
			return 1;
		}
		else if (op == ReturnS)
		{
			printDisassembly(output, op, "ReturnS");
			return 1;
		}
		else if (op == StoreI)
		{
			memcpy(param, &code[1], 4);
			printDisassembly(output, op, param[0], param[1], param[2], param[3], "StoreI");
			return 5;
		}
		else if (op == StoreF)
		{
			memcpy(param, &code[1], 4);
			printDisassembly(output, op, param[0], param[1], param[2], param[3], "StoreF");
			return 5;
		}
		else if (op == StoreS)
		{
			memcpy(param, &code[1], 4);
			printDisassembly(output, op, param[0], param[1], param[2], param[3], "StoreS");
			return 5;
		}
		else if (op == LoadI)
		{
			memcpy(param, &code[1], 4);
			printDisassembly(output, op, param[0], param[1], param[2], param[3], "LoadI");
			return 5;
		}
		else if (op == LoadF)
		{
			memcpy(param, &code[1], 4);
			printDisassembly(output, op, param[0], param[1], param[2], param[3], "LoadF");
			return 5;
		}
		else if (op == LoadS)
		{
			memcpy(param, &code[1], 4);
			printDisassembly(output, op, param[0], param[1], param[2], param[3], "LoadS");
			return 5;
		}
		else if (op == PushI)
		{
			memcpy(param, &code[1], 4);
			printDisassembly(output, op, param[0], param[1], param[2], param[3], "PushI");
			return 5;
		}
		else if (op == PushF)
		{
			memcpy(param, &code[1], 4);
			printDisassembly(output, op, param[0], param[1], param[2], param[3], "PushF");
			return 5;
		}
		else if (op == PushS)
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
		else if (op == Pop)
		{
			printDisassembly(output, op, "Pop");
			return 1;
		}
		else if (op == AddI)
		{
			printDisassembly(output, op, "AddI");
			return 1;
		}
		else if (op == AddF)
		{
			printDisassembly(output, op, "AddF");
			return 1;
		}
		else if (op == SubtractI)
		{
			printDisassembly(output, op, "SubtractI");
			return 1;
		}
		else if (op == SubtractF)
		{
			printDisassembly(output, op, "SubtractF");
			return 1;
		}
		else if (op == MultiplyI)
		{
			printDisassembly(output, op, "MultiplyI");
			return 1;
		}
		else if (op == MultiplyF)
		{
			printDisassembly(output, op, "MultiplyF");
			return 1;
		}
		else if (op == DivideI)
		{
			printDisassembly(output, op, "DivideI");
			return 1;
		}
		else if (op == DivideF)
		{
			printDisassembly(output, op, "DivideF");
			return 1;
		}
		else if (op == NegateI)
		{
			printDisassembly(output, op, "NegateI");
			return 1;
		}
		else if (op == NegateF)
		{
			printDisassembly(output, op, "NegateF");
			return 1;
		}
		else if (op == IsEqualI)
		{
			printDisassembly(output, op, "IsEqualI");
			return 1;
		}
		else if (op == IsEqualF)
		{
			printDisassembly(output, op, "IsEqualF");
			return 1;
		}
		else if (op == NotEqualI)
		{
			printDisassembly(output, op, "NotEqualI");
			return 1;
		}
		else if (op == NotEqualF)
		{
			printDisassembly(output, op, "NotEqualF");
			return 1;
		}
		else if (op == IsGreaterI)
		{
			printDisassembly(output, op, "IsGreaterI");
			return 1;
		}
		else if (op == IsGreaterF)
		{
			printDisassembly(output, op, "IsGreaterF");
			return 1;
		}
		else if (op == IsLessI)
		{
			printDisassembly(output, op, "IsLessI");
			return 1;
		}
		else if (op == IsLessF)
		{
			printDisassembly(output, op, "IsLessF");
			return 1;
		}
		else if (op == IsGreaterEqualI)
		{
			printDisassembly(output, op, "IsGreaterEqualI");
			return 1;
		}
		else if (op == IsGreaterEqualF)
		{
			printDisassembly(output, op, "IsGreaterEqualF");
			return 1;
		}
		else if (op == IsLessEqualI)
		{
			printDisassembly(output, op, "IsLessEqualI");
			return 1;
		}
		else if (op == IsLessEqualF)
		{
			printDisassembly(output, op, "IsLessEqualF");
			return 1;
		}
		else if (op == IToF)
		{
			memcpy(param, &code[1], 4);
			printDisassembly(output, op, param[0], param[1], param[2], param[3], "IToF");
			return 5;
		}
		else if (op == FToI)
		{
			memcpy(param, &code[1], 4);
			printDisassembly(output, op, param[0], param[1], param[2], param[3], "FToI");
			return 5;
		}
		else if (op == And)
		{
			printDisassembly(output, op, "And");
			return 1;
		}
		else if (op == Or)
		{
			printDisassembly(output, op, "Or");
			return 1;
		}
		else if (op == Not)
		{
			printDisassembly(output, op, "Not");
			return 1;
		}
		else if (op == GetString)
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
#include <cassert>
#include <fstream>
#include <sstream>
#include <iomanip>
//#include "compiler.h"
#include "sheepTypes.h"
#include "sheepCodeBuffer.h"
#include "sheepDisassembler.h"
//#include "sheepfile.h"
#include "sheepException.h"

namespace SheepCompiler
{
	#define READ4(p) file.read((char*)p, 4);
	#define READ2(p) file.read((char*)p, 2);
	#define READ1(p) file.read((char*)p, 1);

	std::string Disassembler::GetDisassembly(const std::string& inputFile)
	{
		std::stringstream output;

		std::ifstream file(inputFile.c_str(), std::ios_base::binary);
		if (!file)
		{
			throw SheepException("Unable to open input file");
		}

		unsigned int fileSize = getFileSize(file);
		std::string disassembly;

		// read the header
		SheepHeader header;
		READ4(&header.Magic1);
		READ4(&header.Magic2);

		if (header.Magic1 != SheepHeader::Magic1Value || header.Magic2 != SheepHeader::Magic2Value)
		{
			file.close();
			throw SheepException("Input file is not a valid sheep file");
		}

		READ4(&header.Unknown);
		READ4(&header.ExtraOffset);
		READ4(&header.DataOffset);
		READ4(&header.DataSize);
		READ4(&header.DataCount);

		header.OffsetArray = new unsigned int[header.DataCount];

		for (unsigned int i = 0; i < header.DataCount; i++)
			READ4(&header.OffsetArray[i]);

		SectionHeader importHeader, constantsHeader, functionsHeader, variablesHeader, codeHeader;
		std::vector<Import> imports;
		std::vector<StringConstant> constants;
		std::vector<LocalFunction> functions;

		for (unsigned int i = 0; i < header.DataCount; i++)
		{
			if (header.OffsetArray[i] >= fileSize)
			{
				delete[] header.OffsetArray;
				throw SheepException("Input file is not a valid sheep file");
			}

			file.seekg(header.DataOffset + header.OffsetArray[i]);
	
			char label[13] = {0};
			file.read(label, 12);
			if (std::string(label) == "SysImports")
			{
				output << std::endl << "SysImports" << std::endl;

				importHeader = readSectionHeader(file, label);
				unsigned int currentOffset = file.tellg();

				for (unsigned int j = 0; j < importHeader.DataCount; j++)
				{
					Import import;
					
					file.seekg(currentOffset + importHeader.OffsetArray[j], std::ios_base::beg);

					READ2(&import.LengthOfName);
					import.Name = readString(file, import.LengthOfName);
					READ1(&import.NumReturns);
					READ1(&import.NumParameters);

					output << "\t" << j << "\t- " << import.Name << "(";

					for (byte k = 0; k < import.NumParameters; k++)
					{
						byte parameterType;
						READ1(&parameterType);

						if (k > 0) output << ", ";

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
					
					imports.push_back(import);
				}
			}
			else if (std::string(label) == "StringConsts")
			{
				output << std::endl << "StringConsts" << std::endl;

				constantsHeader = readSectionHeader(file, "StringConsts");
		
				unsigned int currentOffset = file.tellg();

				for (unsigned int j = 0; j < constantsHeader.DataCount; j++)
				{
					file.seekg(currentOffset + constantsHeader.OffsetArray[j], std::ios_base::beg);

					StringConstant constant;
					constant.Offset = constantsHeader.OffsetArray[j];
					constant.String = readString(file);

					constants.push_back(constant);

					output << "\t" << j << "\t" << constant.Offset
						<< "\t-\"" << constant.String << "\"" << std::endl;
				}

			}
			else if (std::string(label) == "Variables")
			{
				output << std::endl << "Variables" << std::endl;
				
				variablesHeader = readSectionHeader(file, "Variables");
				
				unsigned int currentOffset = file.tellg();
				
				for (unsigned int j = 0; j < variablesHeader.DataCount; j++)
				{
					file.seekg(currentOffset + variablesHeader.OffsetArray[j], std::ios_base::beg);
					
					unsigned short len;
					READ2(&len);
					
					std::string name = readString(file, len+1);
					unsigned int type, value;
					READ4(&type);
					READ4(&value);
					
					output << "\t" << j << "\t";
					
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
					
					output << " " << name << " = " << value << std::endl;
				}
			}
			else if (std::string(label) == "Functions")
			{
				output << std::endl << "Functions" << std::endl;

				functionsHeader = readSectionHeader(file, "Functions");
				unsigned int currentOffset = file.tellg();
				
				for (unsigned int j = 0; j < functionsHeader.DataCount; j++)
				{
					file.seekg(currentOffset + functionsHeader.OffsetArray[j], std::ios_base::beg);
				
					LocalFunction func;

					unsigned short len;
					READ2(&len);

					func.Name = readString(file, len+1);
					char dummy;
					READ1(&dummy);
					READ1(&dummy);
					READ4(&func.Offset);

					functions.push_back(func);

					output << "\t" << j << "\t(" << func.Offset << ")\t"
						<< "\t-\"" << func.Name << "\"" << std::endl;
				}
			}
			else if (std::string(label) == "Code")
			{
				output << std::endl << "Code" << std::endl;

				codeHeader = readSectionHeader(file, "Code");
				unsigned int currentOffset = file.tellg();

				if (codeHeader.DataCount > 1)
					throw SheepException("Extra code sections found");

				file.seekg(currentOffset + codeHeader.OffsetArray[0], std::ios_base::beg);
				currentOffset = file.tellg();
				
				for (unsigned int j = 0; j < functions.size(); j++)
				{
					output << "\t" << j << " - " << functions[j].Name << std::endl;
	
					file.seekg(currentOffset + functions[j].Offset, std::ios_base::beg);

					unsigned int currentCodeOffset = 0;
					while((j < functions.size()-1 && currentCodeOffset + functions[j].Offset < functions[j+1].Offset)
						|| (j == functions.size()-1 && currentCodeOffset + functions[j].Offset < codeHeader.DataSize))
					{	
						output << "\t" << currentCodeOffset << ":\t";

						currentCodeOffset += printNextInstruction(file, output, imports, constants);
					}

					output << std::endl;
				}
			}
			else
			{
				throw SheepException("Unrecognized data section");
				
			}
		}

		file.close();

		// clean up
		delete[] header.OffsetArray;
		if (importHeader.OffsetArray) delete[] importHeader.OffsetArray;
		if (constantsHeader.OffsetArray) delete[] constantsHeader.OffsetArray;
		if (variablesHeader.OffsetArray) delete[] variablesHeader.OffsetArray;
		if (functionsHeader.OffsetArray) delete[] functionsHeader.OffsetArray;
		if (codeHeader.OffsetArray) delete[] codeHeader.OffsetArray;
		
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

		header.OffsetArray = new unsigned int[header.DataCount];

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

	unsigned int Disassembler::printNextInstruction(std::ifstream& file, std::ostream& output, std::vector<Import>& imports, std::vector<StringConstant>& constants)
	{
#define PRINT_INT_OP(o,b1,b2,b3,b4,s) output << std::setw(2) << std::setbase(16) << o << " " << b1 << " " << b2 << " " << b3 << " " << b4 << "\t " << s << std::endl;

		unsigned char op;
		unsigned char param[4];
		file.read((char*)&op, 1);
		unsigned int iop = op;

		if (op == SitnSpin)
		{
			printDisassembly(output, op, "SitnSpin");
			return 1;
		}
		else if (op == CallSysFunctionV)
		{
			file.read((char*)param, 4);

			int function = convertBytesToInt(param[0], param[1], param[2], param[3]);

			printDisassembly(output, op, param[0], param[1], param[2], param[3],
				"CallSysFunctionV", imports[function].Name);

			return 5;
		}
		else if (op == CallSysFunctionI)
		{
			file.read((char*)param, 4);

			int function = convertBytesToInt(param[0], param[1], param[2], param[3]);

			printDisassembly(output, op, param[0], param[1], param[2], param[3],
				"CallSysFunctionI", imports[function].Name);
			
			return 5;
		}
		else if (op == Branch)
		{
			file.read((char*)param, 4);
			printDisassembly(output, op, param[0], param[1], param[2], param[3],
				"Branch");
			return 5;
		}
		else if (op == BranchIfZero)
		{
			file.read((char*)param, 4);
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
		else if (op == StoreI)
		{
			file.read((char*)param, 4);
			printDisassembly(output, op, param[0], param[1], param[2], param[3], "StoreI");
			return 5;
		}
		else if (op == StoreF)
		{
			file.read((char*)param, 4);
			printDisassembly(output, op, param[0], param[1], param[2], param[3], "StoreF");
			return 5;
		}
		else if (op == StoreS)
		{
			file.read((char*)param, 4);
			printDisassembly(output, op, param[0], param[1], param[2], param[3], "StoreS");
			return 5;
		}
		else if (op == LoadI)
		{
			file.read((char*)param, 4);
			printDisassembly(output, op, param[0], param[1], param[2], param[3], "LoadI");
			return 5;
		}
		else if (op == LoadF)
		{
			file.read((char*)param, 4);
			printDisassembly(output, op, param[0], param[1], param[2], param[3], "LoadF");
			return 5;
		}
		else if (op == LoadS)
		{
			file.read((char*)param, 4);
			printDisassembly(output, op, param[0], param[1], param[2], param[3], "LoadS");
			return 5;
		}
		else if (op == PushI)
		{
			file.read((char*)param, 4);
			printDisassembly(output, op, param[0], param[1], param[2], param[3], "PushI");
			return 5;
		}
		else if (op == PushF)
		{
			file.read((char*)param, 4);
			printDisassembly(output, op, param[0], param[1], param[2], param[3], "PushF");
			return 5;
		}
		else if (op == PushS)
		{
			file.read((char*)param, 4);

			unsigned int offset = convertBytesToInt(param[0], param[1], param[2], param[3]);

			std::string constant = "";
			for (unsigned int i = 0; i < constants.size(); i++)
			{
				if (constants[i].Offset == offset)
				{
					constant = constants[i].String;
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
		else if (op == IsEqualI)
		{
			printDisassembly(output, op, "IsEqualI");
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
			file.read((char*)param, 4);
			printDisassembly(output, op, param[0], param[1], param[2], param[3], "IToF");
			return 4;
		}
		else if (op == FToI)
		{
			file.read((char*)param, 4);
			printDisassembly(output, op, param[0], param[1], param[2], param[3], "FToI");
			return 4;
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
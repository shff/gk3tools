#ifndef DISASSEMBLER_H
#define DISASSEMBLER_H

#include <string>
#include <fstream>
#include <vector>
#include <memory.h>
#include "sheepc.h"
//#include "sheepfile.h"

namespace SheepCompiler
{
	struct SheepHeader
	{
		unsigned int Magic1;
		unsigned int Magic2;
		unsigned int Unknown;
		unsigned int ExtraOffset;
		unsigned int DataOffset;
		unsigned int DataSize;
		unsigned int DataCount;

		unsigned int* OffsetArray;

		static const unsigned int SheepHeaderSize = 28;
		static const unsigned int Magic1Value = 0x53334b47;
		static const unsigned int Magic2Value = 0x70656568;
	};

	struct SectionHeader
	{
		SectionHeader() { memset(Label, 0, 12); OffsetArray = NULL; }

		char Label[12];
		unsigned int ExtraOffset;
		unsigned int DataOffset;
		unsigned int DataSize;
		unsigned int DataCount;

		unsigned int* OffsetArray;

		static const unsigned int SectionHeaderSize = 28;
	};

	struct Import
	{
		unsigned short LengthOfName;
		std::string Name;
		byte NumReturns;
		byte NumParameters;

		byte* ParametersTypes;
	};

	struct StringConstant
	{
		std::string String;
		unsigned int Offset;
	};

	struct LocalFunction
	{
		std::string Name;
		unsigned int Offset;
	};

	class Disassembler
	{
	public:
		static std::string GetDisassembly(const std::string& inputFile);
	
	private:

		static SectionHeader readSectionHeader(std::ifstream& file, const std::string& name);

		static std::string readString(std::ifstream& file);
		static std::string readString(std::ifstream& file, unsigned int length);

		static unsigned int printNextInstruction(std::ifstream& file, std::ostream& output, std::vector<Import>& imports, std::vector<StringConstant>& constants);
		
		static void printDisassembly(std::ostream& output, int op, const std::string& name);
		static void printDisassembly(std::ostream& output, int op,
			int byte1, int byte2, int byte3, int byte4,
			const std::string& name, unsigned int param);
		static void printDisassembly(std::ostream& output, int op,
			int byte1, int byte2, int byte3, int byte4,
			const std::string& name, const std::string param);
		static void printDisassembly(std::ostream& output, int op,
			int byte1, int byte2, int byte3, int byte4,
			const std::string& name);
		
		static unsigned int convertBytesToInt(int byte1, int byte2, int byte3, int byte4)
		{
			return (byte4 << 24) | (byte3 << 16) | (byte2 << 8) | byte1;
		}

		static unsigned int getFileSize(std::ifstream& file)
		{
			std::ifstream::pos_type pos = file.tellg();
			file.seekg(0, std::ios_base::end);

			std::ifstream::pos_type end = file.tellg();
			file.seekg(pos, std::ios_base::beg);

			return end;
		}
	};
}

#endif // DISASSEMBLER
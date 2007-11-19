#ifndef SHEEPFILE_H
#define SHEEPFILE_H

#include <string>

namespace SheepCompiler
{
	typedef unsigned char byte;

	const unsigned int Magic1 = 0x53334b47;
	const unsigned int Magic2 = 0x70656568;

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
}

#endif // SHEEPFILE_H

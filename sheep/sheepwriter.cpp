#include <fstream>
#include "compiler.h"

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
};

struct SectionHeader
{
	SectionHeader() { memset(Label, 0, 12); }

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

struct Function
{
	unsigned short LengthOfName;
	std::string Name;
	byte NumReturns;
	byte NumParameters;

	unsigned int CodeOffset;
};

void Compiler::WriteCompiledSheep(const std::string& outputFile)
{
	unsigned int currentFileOffset = 0;

	// header
	SheepHeader header;

	header.Magic1 = 0x53334b47;
	header.Magic2 = 0x70656568;
	header.Unknown = 0;
	header.ExtraOffset = 0x2c;
	header.DataOffset = 0x2c;
	header.DataCount = 4;
	header.OffsetArray = new unsigned int[4];

	currentFileOffset = SheepHeader::SheepHeaderSize + header.DataCount * 4;
	
	header.OffsetArray[0] = 0; // import section offset

	// imports
	SectionHeader importHeader;
	strncpy(importHeader.Label, "SysImports", 12);
	importHeader.DataCount = m_importFunctions.size();
	importHeader.ExtraOffset = SectionHeader::SectionHeaderSize + importHeader.DataCount * 4;
	importHeader.DataOffset = importHeader.ExtraOffset;
	importHeader.OffsetArray = new unsigned int[importHeader.DataCount];

	currentFileOffset += importHeader.SectionHeaderSize + importHeader.DataCount * 4;

	Import* imports = new Import[importHeader.DataCount];
	unsigned int currentOffset = 0;
	unsigned int counter = 0;
	for (std::map<std::string, ImportFunction>::iterator itr = m_importFunctions.begin();
		itr != m_importFunctions.end(); itr++)
	{
		imports[counter].LengthOfName = (*itr).second.Name.length();
		imports[counter].Name = (*itr).second.Name;
		imports[counter].NumReturns = 0;
		imports[counter].NumParameters = 0;

		importHeader.OffsetArray[counter] = currentOffset;

		currentOffset += 2 + imports[counter].LengthOfName+1 + 2 + imports[counter].NumParameters;
		currentFileOffset += currentOffset;
		counter++;
	}
	importHeader.DataSize = currentOffset;

	header.OffsetArray[1] = currentFileOffset - SheepHeader::SheepHeaderSize - header.DataCount * 4;

	// string constants
	SectionHeader constantHeader;
	strncpy(constantHeader.Label, "StringConsts", 12);
	constantHeader.DataCount = m_stringConstants.size();
	constantHeader.ExtraOffset = SectionHeader::SectionHeaderSize + constantHeader.DataCount * 4;
	constantHeader.DataOffset = constantHeader.ExtraOffset;
	constantHeader.OffsetArray = new unsigned int[constantHeader.DataCount];
	
	currentFileOffset += SectionHeader::SectionHeaderSize + constantHeader.DataCount * 4;

	currentOffset = 0;
	counter = 0;
	for (std::map<std::string, StringConstant>::iterator itr = m_stringConstants.begin();
		itr != m_stringConstants.end(); itr++)
	{
		constantHeader.OffsetArray[counter] = currentOffset;
		currentOffset += (*itr).second.String.length()+1;

		counter++;
	}

	constantHeader.DataSize = currentOffset;
	currentFileOffset += currentOffset;

	header.OffsetArray[2] = currentFileOffset - SheepHeader::SheepHeaderSize - header.DataCount * 4;

	// functions section
	SectionHeader functionHeader;
	strncpy(functionHeader.Label, "Functions", 12);
	functionHeader.DataCount = m_functions.size();
	functionHeader.ExtraOffset = SectionHeader::SectionHeaderSize + functionHeader.DataCount * 4;
	functionHeader.DataOffset = functionHeader.ExtraOffset;
	functionHeader.OffsetArray = new unsigned int[functionHeader.DataCount];

	currentFileOffset += SectionHeader::SectionHeaderSize + functionHeader.DataCount * 4;

	Function* functions = new Function[functionHeader.DataCount];
	currentOffset = 0;
	counter = 0;
	for (unsigned int i = 0; i < m_functions.size(); i++)
	{
		functions[i].LengthOfName = m_functions[i].Name.length();
		functions[i].Name = m_functions[i].Name;
		functions[i].NumReturns = 0;
		functions[i].NumParameters = 0;
		functions[i].CodeOffset = m_functions[i].Offset;

		functionHeader.OffsetArray[i] = currentOffset;
		currentOffset += 2 + functions[i].LengthOfName+1 + 6;
	}

	functionHeader.DataSize = currentOffset;
	currentFileOffset += currentOffset;

	header.OffsetArray[3] = currentFileOffset - SheepHeader::SheepHeaderSize - header.DataCount * 4;

	// code section
	SectionHeader codeHeader;
	strncpy(codeHeader.Label, "Code", 12);
	codeHeader.DataCount = 1;
	codeHeader.ExtraOffset = 32;
	codeHeader.DataOffset = 32;
	codeHeader.DataSize = m_instructions.size();

	currentFileOffset += SectionHeader::SectionHeaderSize + 4;
	header.OffsetArray[4] = currentFileOffset;
	header.DataSize = currentFileOffset + m_instructions.size() - SheepHeader::SheepHeaderSize - header.DataCount * 4;


	// TODO: set the data size header field!

	// whew! all that's done! let's write it out!
	#define WRITE4(p) file.write((char*)p, 4);
	#define WRITE2(p) file.write((char*)p, 2);
	#define WRITE1(p) file.write((char*)p, 1);

	std::ofstream file(outputFile.c_str(), std::ios_base::binary);

	// write the header
	WRITE4(&header.Magic1);
	WRITE4(&header.Magic2);
	WRITE4(&header.Unknown);
	WRITE4(&header.ExtraOffset);
	WRITE4(&header.DataOffset);
	WRITE4(&header.DataSize);
	WRITE4(&header.DataCount);

	for (unsigned int i = 0; i < header.DataCount; i++)
	{
		WRITE4(&header.OffsetArray[i]);
	}

	// write the SysImports section
	file.write(importHeader.Label, 12);
	WRITE4(&importHeader.ExtraOffset);
	WRITE4(&importHeader.DataOffset);
	WRITE4(&importHeader.DataSize);
	WRITE4(&importHeader.DataCount);

	for (unsigned int i = 0; i < importHeader.DataCount; i++)
	{
		WRITE4(&importHeader.OffsetArray[i]);
	}

	for (unsigned int i = 0; i < importHeader.DataCount; i++)
	{
		WRITE2(&imports[i].LengthOfName);
		file.write(imports[i].Name.c_str(), imports[i].LengthOfName+1);
		WRITE1(&imports[i].NumReturns);
		WRITE1(&imports[i].NumParameters);

		for (unsigned int j = 0; j < imports[i].NumParameters; j++)
			WRITE1(&imports[i].ParametersTypes[j]);
	}

	// write the string constants section
	file.write(constantHeader.Label, 12);
	WRITE4(&constantHeader.ExtraOffset);
	WRITE4(&constantHeader.DataOffset);
	WRITE4(&constantHeader.DataSize);
	WRITE4(&constantHeader.DataCount);

	for (unsigned int i = 0; i < constantHeader.DataCount; i++)
	{
		WRITE4(&constantHeader.OffsetArray[i]);
	}

	for (std::map<std::string, StringConstant>::iterator itr = m_stringConstants.begin();
		itr != m_stringConstants.end(); itr++)
	{
		file.write((*itr).second.String.c_str(), (*itr).second.String.length()+1);
	}

	// write the function section
	file.write(functionHeader.Label, 12);
	WRITE4(&functionHeader.ExtraOffset);
	WRITE4(&functionHeader.DataOffset);
	WRITE4(&functionHeader.DataSize);
	WRITE4(&functionHeader.DataCount);

	for (unsigned int i = 0; i < functionHeader.DataCount; i++)
	{
		WRITE4(&functionHeader.OffsetArray[i]);
	}

	for (unsigned int i = 0; i < functionHeader.DataCount; i++)
	{
		WRITE2(&functions[i].LengthOfName);
		file.write(functions[i].Name.c_str(), functions[i].LengthOfName+1);
		WRITE1(&functions[i].NumReturns);
		WRITE1(&functions[i].NumParameters);
		WRITE4(&functions[i].CodeOffset);
	}

	// write the code section
	file.write(codeHeader.Label, 12);
	WRITE4(&codeHeader.ExtraOffset);
	WRITE4(&codeHeader.DataOffset);
	WRITE4(&codeHeader.DataSize);
	WRITE4(&codeHeader.DataCount);

	unsigned int zero = 0;
	WRITE4(&zero);

	byte b;
	for (unsigned int i = 0; i < m_instructions.size(); i++)
	{
		b = m_instructions[i];
		WRITE1(&b);
	}

	file.close();

	// TODO: cleanup
}

}
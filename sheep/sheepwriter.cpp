#include <fstream>
#include <cassert>
#include "compiler.h"
#include "sheepfile.h"
#include "rbuffer.h"

namespace SheepCompiler
{


struct Function
{
	unsigned short LengthOfName;
	std::string Name;
	byte NumReturns;
	byte NumParameters;

	unsigned int CodeOffset;
};

struct Variable
{
	unsigned short LengthOfName;
	std::string Name;
	int Type;
	int Value;
};

void Compiler::WriteCompiledSheep(const std::string& outputFile)
{
	::ResizableBuffer* buffer = WriteCompiledSheep();

	buffer->SaveToFile(outputFile);

	delete buffer;
}

ResizableBuffer* Compiler::WriteCompiledSheep()
{
	ResizableBuffer* buffer = new ResizableBuffer;

	bool includeVariablesSection = false;

	if (m_symbolList.empty() == false)
		includeVariablesSection = true;

	addByteToInstructions(SitnSpin);
	addByteToInstructions(SitnSpin);
	addByteToInstructions(SitnSpin);
	addByteToInstructions(SitnSpin);

	unsigned int currentFileOffset = 0;
	unsigned int currentSectionIndex = 0;

	// header
	SheepHeader header;

	header.Magic1 = Magic1;
	header.Magic2 = Magic2;
	header.Unknown = 0;
	if (includeVariablesSection)
	{
		header.ExtraOffset = 0x30;
		header.DataOffset = 0x30;
	}
	else
	{
		header.ExtraOffset = 0x2c;
		header.DataOffset = 0x2c;
	}

	if (includeVariablesSection == false)
	{
		header.DataCount = 4;
		header.OffsetArray = new unsigned int[4];
	}
	else
	{
		header.DataCount = 5;
		header.OffsetArray = new unsigned int[5];
	}

	currentFileOffset = SheepHeader::SheepHeaderSize + header.DataCount * 4;
	
	header.OffsetArray[currentSectionIndex++] = 0; // import section offset

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
		imports[counter].NumReturns = (*itr).second.ReturnType == Symbol_Void ? 0 : 1;
		imports[counter].NumParameters = (*itr).second.Parameters.size();

		imports[counter].ParametersTypes = new byte[imports[counter].NumParameters];
		for (int i = 0; i < imports[counter].NumParameters; i++)
		{
			imports[counter].ParametersTypes[i] = (*itr).second.Parameters[i];
		}

		importHeader.OffsetArray[counter] = currentOffset;

		currentOffset += 2 + imports[counter].LengthOfName+1 + 2 + imports[counter].NumParameters;
		currentFileOffset += currentOffset;
		counter++;
	}

	importHeader.DataSize = currentOffset;

	header.OffsetArray[currentSectionIndex++] = currentFileOffset - SheepHeader::SheepHeaderSize - header.DataCount * 4;

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
	for (unsigned int i = 0; i < m_stringConstantsList.size(); i++)
	{
		constantHeader.OffsetArray[counter] = currentOffset;
		currentOffset += m_stringConstantsList[i].String.length()+1;

		counter++;
	}

	constantHeader.DataSize = currentOffset;
	currentFileOffset += currentOffset;

	header.OffsetArray[currentSectionIndex++] = currentFileOffset - SheepHeader::SheepHeaderSize - header.DataCount * 4;

	// Variables
	SectionHeader variablesHeader;
	Variable* variables = NULL;
	if (includeVariablesSection)
	{
		strncpy(variablesHeader.Label, "Variables", 12);
		variablesHeader.DataCount = m_symbolList.size();
		variablesHeader.ExtraOffset = SectionHeader::SectionHeaderSize + variablesHeader.DataCount * 4;
		variablesHeader.DataOffset = variablesHeader.ExtraOffset;
		variablesHeader.OffsetArray = new unsigned int[variablesHeader.DataCount];

		currentFileOffset += SectionHeader::SectionHeaderSize + variablesHeader.DataCount * 4;

		variables = new Variable[variablesHeader.DataCount];
		currentOffset = 0;
		
		for (unsigned int i = 0; i < m_symbolList.size(); i++)
		{
			variables[i].LengthOfName = m_symbolList[i].Name.length();
			variables[i].Name = m_symbolList[i].Name;
			variables[i].Type = m_symbolList[i].Type;

			if (m_symbolList[i].Type == Symbol_Integer)
				variables[i].Value = m_symbolList[i].Value.IntValue;
			else if (m_symbolList[i].Type == Symbol_Float)
				memcpy(&variables[i].Value, &m_symbolList[i].Value.FloatValue, 4);
			else if (m_symbolList[i].Type == Symbol_String)
			{
				StringConstant constant;
				getStringConstant(m_symbolList[i].Value.StringValue, &constant);

				variables[i].Value = constant.Offset;
			}

			variablesHeader.OffsetArray[i] = currentOffset;
			currentOffset += 2 + m_symbolList[i].Name.length()+1 + 4 + 4;
		}
		
		variablesHeader.DataSize = currentOffset;
		currentFileOffset += currentOffset;

		header.OffsetArray[currentSectionIndex++] = currentFileOffset - SheepHeader::SheepHeaderSize - header.DataCount * 4;
	}

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

	header.OffsetArray[currentSectionIndex++] = currentFileOffset - SheepHeader::SheepHeaderSize - header.DataCount * 4;

	// code section
	SectionHeader codeHeader;
	strncpy(codeHeader.Label, "Code", 12);
	codeHeader.DataCount = 1;
	codeHeader.ExtraOffset = 32;
	codeHeader.DataOffset = 32;
	codeHeader.DataSize = m_instructions.size();

	currentFileOffset += SectionHeader::SectionHeaderSize + 4;
	header.DataSize = currentFileOffset + m_instructions.size() - SheepHeader::SheepHeaderSize - header.DataCount * 4;
	
	// TODO: set the data size header field!

	// whew! all that's done! let's write it out!
	#define WRITE4(p) buffer->Write((char*)p, 4);
	#define WRITE2(p) buffer->Write((char*)p, 2);
	#define WRITE1(p) buffer->Write((char*)p, 1);

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
	buffer->Write(importHeader.Label, 12);
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
		buffer->Write(imports[i].Name.c_str(), imports[i].LengthOfName+1);
		WRITE1(&imports[i].NumReturns);
		WRITE1(&imports[i].NumParameters);

		for (unsigned int j = 0; j < imports[i].NumParameters; j++)
			WRITE1(&imports[i].ParametersTypes[j]);
	}

	// write the string constants section
	buffer->Write(constantHeader.Label, 12);
	WRITE4(&constantHeader.ExtraOffset);
	WRITE4(&constantHeader.DataOffset);
	WRITE4(&constantHeader.DataSize);
	WRITE4(&constantHeader.DataCount);

	for (unsigned int i = 0; i < constantHeader.DataCount; i++)
	{
		WRITE4(&constantHeader.OffsetArray[i]);
	}

	for (unsigned int i = 0; i < m_stringConstantsList.size(); i++)
	{
		buffer->Write(m_stringConstantsList[i].String.c_str(), m_stringConstantsList[i].String.length()+1);
	}

	// write the variables section
	if (includeVariablesSection)
	{
		buffer->Write(variablesHeader.Label, 12);
		WRITE4(&variablesHeader.ExtraOffset);
		WRITE4(&variablesHeader.DataOffset);
		WRITE4(&variablesHeader.DataSize);
		WRITE4(&variablesHeader.DataCount);

		for (unsigned int i = 0; i < variablesHeader.DataCount; i++)
		{
			WRITE4(&variablesHeader.OffsetArray[i]);
		}

		for (unsigned int i = 0; i < variablesHeader.DataCount; i++)
		{
			WRITE2(&variables[i].LengthOfName);
			buffer->Write(variables[i].Name.c_str(), variables[i].LengthOfName+1);
			WRITE4(&variables[i].Type);
			WRITE4(&variables[i].Value);
		}
	}

	// write the function section
	buffer->Write(functionHeader.Label, 12);
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
		buffer->Write(functions[i].Name.c_str(), functions[i].LengthOfName+1);
		WRITE1(&functions[i].NumReturns);
		WRITE1(&functions[i].NumParameters);
		WRITE4(&functions[i].CodeOffset);
	}

	// write the code section
	buffer->Write(codeHeader.Label, 12);
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

	// cleanup
	delete[] header.OffsetArray;
	delete[] importHeader.OffsetArray;
	delete[] constantHeader.OffsetArray;
	delete[] functionHeader.OffsetArray;
	delete[] functions;
	if (variablesHeader.OffsetArray) delete[] variablesHeader.OffsetArray;
	delete[] imports;
	delete[] variables;

	return buffer;
}

}
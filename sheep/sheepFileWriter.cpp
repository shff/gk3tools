#include "sheepFileWriter.h"
#include "sheepFileReader.h"
#include "sheepCodeGenerator.h"
#include "sheepCodeBuffer.h"
#include "rbuffer.h"
#include <ctime>

#ifdef _MSC_VER
#pragma warning(error:4267)
#endif


SheepFileWriter::SheepFileWriter(IntermediateOutput* output)
{
	assert(output != NULL);

	m_intermediateOutput = output;
	m_buffer = SHEEP_NEW ResizableBuffer();

	writeToBuffer();
}

SheepFileWriter::~SheepFileWriter()
{
}

void SheepFileWriter::Write(const std::string &filename)
{
	// save!
	m_buffer->SaveToFile(filename);
}

void SheepFileWriter::writeToBuffer()
{
	// how many sections are we writing?
	int dataCount = 3;
	if (m_intermediateOutput->Symbols.empty() == false) dataCount++;
	if (m_intermediateOutput->Imports.empty() == false) dataCount++;

	// write the main header
	writeFileHeader(dataCount);

	int offsetAfterHeader = (int)m_buffer->Tell();

	int currentSection = 0;

	// write the variables
	if (m_intermediateOutput->Symbols.empty() == false)
	{
		m_buffer->WriteUIntAt((int)m_buffer->Tell() - offsetAfterHeader, DataSectionHeaderSize + currentSection * 4);
		writeVariablesSection();
		currentSection++;
	}

	// write the imports section
	if (m_intermediateOutput->Imports.empty() == false)
	{
		m_buffer->WriteUIntAt((int)m_buffer->Tell() - offsetAfterHeader, DataSectionHeaderSize + currentSection * 4);
		writeImportsSection();
		currentSection++;
	}

	// write the constants section
	m_buffer->WriteUIntAt((int)m_buffer->Tell() - offsetAfterHeader, DataSectionHeaderSize + currentSection * 4);
	writeConstantsSection();
	currentSection++;

	// write the function section
	m_buffer->WriteUIntAt((int)m_buffer->Tell() - offsetAfterHeader, DataSectionHeaderSize + currentSection * 4);
	writeFunctionsSection();
	currentSection++;

	// write the code section
	m_buffer->WriteUIntAt((int)m_buffer->Tell() - offsetAfterHeader, DataSectionHeaderSize + currentSection * 4);
	writeCodeSection();

	// write the total size
	m_buffer->WriteUIntAt((int)m_buffer->GetSize() - offsetAfterHeader, 20); 
}

void SheepFileWriter::writeFileHeader(int sectionCount)
{
	int headerSize = DataSectionHeaderSize + sectionCount * 4;
	writeSectionHeader("GK3Sheep", headerSize, sectionCount);

	// write the "extra" section
	Sheep::SheepLanguageVersion version = m_intermediateOutput->GetLanguageVersion();
	if (version == Sheep::SheepLanguageVersion::V100)
	{
		m_buffer->WriteUShort(1);
		m_buffer->WriteUShort(0);
	}
	else
	{
		m_buffer->WriteUShort(2);
		m_buffer->WriteUShort(0);
	}

	// checksum
	m_buffer->WriteInt(0);

	// SYSTEMTIME
	time_t currentTime;
	time(&currentTime);
	tm* t =	gmtime(&currentTime);
	m_buffer->WriteUShort(1900 + t->tm_year);
	m_buffer->WriteUShort(1 + t->tm_mon);
	m_buffer->WriteUShort(t->tm_wday);
	m_buffer->WriteUShort(t->tm_mday);
	m_buffer->WriteUShort(t->tm_hour);
	m_buffer->WriteUShort(t->tm_min);
	m_buffer->WriteUShort(t->tm_sec);
	m_buffer->WriteUShort(0);

	// copyright
	const char* copyright = "Built by sheepc";
	m_buffer->Write(copyright, strlen(copyright) + 1);

	// now go back and update the data offset
	size_t position = m_buffer->Tell();
	m_buffer->WriteAt((const char*)&position, 4, 16);
}

void SheepFileWriter::writeSectionHeader(const std::string& label, int dataOffset, int dataCount)
{
	const unsigned int baddummy = 0xdddddddd;

	char buffer[12] = {0};
	label.copy(buffer, 12); 
	m_buffer->Write(buffer, 12);

	m_buffer->WriteUInt(dataOffset);
	m_buffer->WriteUInt(dataOffset);
	m_buffer->WriteUInt(baddummy);
	m_buffer->WriteUInt(dataCount);

	// create the offset array
	for (size_t i = 0; i < (size_t)dataCount; i++)
		m_buffer->WriteUInt(baddummy);
}

template<typename T, typename Adder>
void SheepFileWriter::writeSection(const std::string& label, std::vector<T> collection, Adder adder)
{
	int offsetAtStart = (int)m_buffer->Tell();

	writeSectionHeader(label, DataSectionHeaderSize + (int)collection.size() * 4, (int)collection.size());

	int offsetAfterHeader = (int)m_buffer->Tell();

	int counter = 0;
	for (typename std::vector<T>::iterator itr = collection.begin(); itr != collection.end(); itr++)
	{
		// go back and write the offset to this position
		m_buffer->WriteUIntAt((int)m_buffer->Tell() - offsetAfterHeader, offsetAtStart + DataSectionHeaderSize + counter * 4);
		
		// add the thingy
		adder(*itr);

		counter++;
	}

	int size = (int)m_buffer->Tell() - offsetAfterHeader;
	m_buffer->WriteUIntAt(size, offsetAtStart + 20);
}

class VariableAdder
{
public:
	VariableAdder(ResizableBuffer* buffer)
	{
		m_buffer = buffer;
	}

	void operator()(SheepSymbol symbol)
	{
		unsigned short lengthOfName = (unsigned short)symbol.Name.length();
		m_buffer->WriteUShort(lengthOfName);
		m_buffer->Write(symbol.Name.c_str(), lengthOfName + 1);

		m_buffer->WriteUInt((unsigned int)symbol.Type);
		
		if (symbol.Type == SheepSymbolType::Int)
			m_buffer->WriteInt(symbol.InitialIntValue);
		else if (symbol.Type == SheepSymbolType::Float)
			m_buffer->WriteFloat(symbol.InitialFloatValue);
		else
		{
			assert(symbol.Type == SheepSymbolType::String);
			m_buffer->WriteUInt(symbol.InitialStringValue);
		}
	}

private:

	ResizableBuffer* m_buffer;
};

void SheepFileWriter::writeVariablesSection()
{
	writeSection<SheepSymbol>("Variables", m_intermediateOutput->Symbols, VariableAdder(m_buffer));
}

void SheepFileWriter::writeCodeSection()
{
	size_t offsetAtStart = m_buffer->Tell();
	writeSectionHeader("Code", DataSectionHeaderSize + 4, 1);

	int offsetAfterHeader = (int)m_buffer->Tell();

	// write the offset to the point
	m_buffer->WriteUIntAt(0, offsetAtStart + DataSectionHeaderSize);

	// write the code!
	for (std::vector<SheepFunction>::iterator itr = m_intermediateOutput->Functions.begin(); 
		itr != m_intermediateOutput->Functions.end(); itr++)
	{
		m_buffer->Write((*itr).Code->GetData(), (*itr).Code->GetSize());
	}

	int size = (int)m_buffer->Tell() - offsetAfterHeader;
	m_buffer->WriteUIntAt(size, offsetAtStart + 20);
}

class ImportAdder
{
public:

	ImportAdder(ResizableBuffer* buffer)
	{
		m_buffer = buffer;
	}

	void operator()(SheepImport import)
	{
		unsigned short lengthOfName = (unsigned short)import.Name.length();
		m_buffer->WriteUShort(lengthOfName);
		m_buffer->Write(import.Name.c_str(), lengthOfName + 1);
		
		char returnType = (char)import.ReturnType;
		char numParameters = (char)import.Parameters.size();

		m_buffer->Write(&returnType, 1);
		m_buffer->Write(&numParameters, 1);

		for (char i = 0; i < numParameters; i++)
		{
			char parameterType = (char)import.Parameters[i];
			m_buffer->Write(&parameterType, 1);
		}
	}

private:

	ResizableBuffer* m_buffer;
};

void SheepFileWriter::writeImportsSection()
{
	writeSection<SheepImport>("SysImports", m_intermediateOutput->Imports, ImportAdder(m_buffer));
}

class FunctionAdder
{
public:

	FunctionAdder(ResizableBuffer* buffer)
	{
		m_buffer = buffer;
		m_currentCodeOffset = 0;
	}

	void operator()(SheepFunction function)
	{
		unsigned short lengthOfName = (unsigned short)function.Name.length();
		m_buffer->WriteUShort(lengthOfName);
		m_buffer->Write(function.Name.c_str(), lengthOfName + 1);

		char returnType = (char)function.ReturnType;
		char numParameters = (char)function.Parameters.size();

		m_buffer->Write(&returnType, 1);
		m_buffer->Write(&numParameters, 1);
		for (int i = 0; i < numParameters; i++)
		{
			char paramType = (char)function.Parameters[i].Type;
			m_buffer->Write(&paramType, 1);
		}

		m_buffer->WriteUInt(m_currentCodeOffset);

		m_currentCodeOffset += (int)function.Code->GetSize();
	}

private:

	int m_currentCodeOffset;
	ResizableBuffer* m_buffer;
};

void SheepFileWriter::writeFunctionsSection()
{
	writeSection<SheepFunction>("Functions", m_intermediateOutput->Functions, FunctionAdder(m_buffer));
}

class ConstantAdder
{
public:

	ConstantAdder(ResizableBuffer* buffer)
	{
		m_buffer = buffer;
	}

	void operator()(SheepStringConstant constant)
	{
		m_buffer->Write(constant.Value.c_str(), constant.Value.length() + 1);
	}

private:

	ResizableBuffer* m_buffer;
};

void SheepFileWriter::writeConstantsSection()
{
	writeSection<SheepStringConstant>("StringConsts", m_intermediateOutput->Constants, ConstantAdder(m_buffer));
}

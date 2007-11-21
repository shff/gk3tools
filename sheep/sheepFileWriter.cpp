#include "sheepFileWriter.h"
#include "sheepCodeGenerator.h"
#include "rbuffer.h"

SheepFileWriter::SheepFileWriter(IntermediateOutput* output)
{
	assert(output != NULL);

	m_intermediateOutput = output;
}

void SheepFileWriter::Write(const std::string &filename)
{
	m_buffer = new ResizableBuffer();

	const int baddummy = 0xdddddddd;

	// how many sections are we writing?
	int dataCount = 3;
	if (m_intermediateOutput->Symbols.empty() == false) dataCount++;
	if (m_intermediateOutput->Imports.empty() == false) dataCount++;

	// write the main header
	int headerSize = DataSectionHeaderSize + dataCount * 4;
	writeSectionHeader("GK3Sheep", headerSize, dataCount);


	int currentSection = 0;

	// write the variables
	if (m_intermediateOutput->Symbols.empty() == false)
	{
		m_buffer->WriteUIntAt(m_buffer->Tell() - headerSize, DataSectionHeaderSize + currentSection * 4);
		writeVariablesSection();
		currentSection++;
	}

	// write the imports section
	if (m_intermediateOutput->Imports.empty() == false)
	{
		m_buffer->WriteUIntAt(m_buffer->Tell() - headerSize, DataSectionHeaderSize + currentSection * 4);
		writeImportsSection();
		currentSection++;
	}

	// write the constants section
	m_buffer->WriteUIntAt(m_buffer->Tell() - headerSize, DataSectionHeaderSize + currentSection * 4);
	writeConstantsSection();
	currentSection++;

	// write the function section
	m_buffer->WriteUIntAt(m_buffer->Tell() - headerSize, DataSectionHeaderSize + currentSection * 4);
	writeFunctionsSection();
	currentSection++;

/*	// write the code section
	m_buffer->WriteUIntAt(m_buffer->Tell() - headerSize, DataSectionHeaderSize + currentSection * 4);
//	writeCodeSection();

	// TODO: write the total size
*/
	// save!
	m_buffer->SaveToFile(filename);

	delete m_buffer;
	m_buffer = NULL;
}

void SheepFileWriter::writeSectionHeader(const std::string& label, size_t dataOffset, size_t dataCount)
{
	char buffer[12] = {0};
	label.copy(buffer, 12); 
	m_buffer->Write(buffer, 12);

	m_buffer->WriteUInt(dataOffset);
	m_buffer->WriteUInt(dataOffset);
	m_buffer->WriteUInt(0xdddddddd);
	m_buffer->WriteUInt(dataCount);

	// create the offset array
	for (size_t i = 0; i < dataCount; i++)
		m_buffer->WriteUInt(0xdddddddd);
}

template<typename T, typename Adder>
void SheepFileWriter::writeSection(const std::string& label, std::vector<T> collection, Adder adder)
{
	size_t offsetAtStart = m_buffer->Tell();

	writeSectionHeader(label, DataSectionHeaderSize + collection.size() * 4, collection.size());

	size_t offsetAfterHeader = m_buffer->Tell();

	int counter = 0;
	for (std::vector<T>::iterator itr = collection.begin(); itr != collection.end(); itr++)
	{
		// go back and write the offset to this position
		m_buffer->WriteUIntAt(m_buffer->Tell() - offsetAfterHeader, offsetAtStart + DataSectionHeaderSize + counter * 4);
		
		// add the thingy
		adder(*itr);

		counter++;
	}

	size_t size = m_buffer->Tell() - offsetAfterHeader;
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

		m_buffer->WriteUInt(symbol.Type);
		
		if (symbol.Type == SYM_INT)
			m_buffer->WriteInt(symbol.InitialIntValue);
		else if (symbol.Type == SYM_FLOAT)
			m_buffer->WriteFloat(symbol.InitialFloatValue);
		else
		{
			assert(symbol.Type == SYM_STRING);
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
	}

	void operator()(SheepFunction function)
	{
		unsigned short lengthOfName = (unsigned short)function.Name.length();
		m_buffer->WriteUShort(lengthOfName);
		m_buffer->Write(function.Name.c_str(), lengthOfName + 1);

		// TODO: finish this!
		//char returnType = 0;
		//char numParameters = 0;
		//function.
	}

private:

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

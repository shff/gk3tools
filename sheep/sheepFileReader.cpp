#include <sstream>
#include <iostream>
#include <fstream>
#include "sheepFileReader.h"
#include "sheepCodeGenerator.h"
#include "sheepCodeBuffer.h"
#include "sheepTypes.h"
#include "sheepException.h"
#include "sheepImportTable.h"
#include "sheepMachine.h"
#include "sheepCaseInsensitiveStringCompare.h"


SheepFileReader::SheepFileReader(const std::string& filename)
{
	std::stringstream output;

	std::ifstream file(filename.c_str(), std::ios_base::binary);
	if (!file)
	{
		throw SheepException("Unable to open input file", SHEEP_ERR_FILE_NOT_FOUND);
	}

	unsigned int fileSize = getFileSize(file);
	//byte* data = new byte[fileSize];
	byte* data = SHEEP_NEW_ARRAY(byte, fileSize);
	file.read((char*)data, fileSize);
	file.close();

	read(data, fileSize);
	delete[] data;
}

SheepFileReader::SheepFileReader(std::ifstream& file)
{
	unsigned int fileSize = getFileSize(file);
	//byte* data = new byte[fileSize];
	byte* data = SHEEP_NEW_ARRAY(byte, fileSize);
	file.read((char*)data, fileSize);
	file.close();

	read(data, fileSize);
	delete[] data;
}

SheepFileReader::SheepFileReader(const byte* data, int length)
{
	read(data, length);
}


#define READ4(d,i) { memcpy(d, &data[i], 4); i+=4; }
#define READ2(d,i) { memcpy(d, &data[i], 2); i+=2; }
#define READ1(d,i) { memcpy(d, &data[i], 1); i+=1; }
#define READN(d,i,n) { memcpy(d, &data[i], n); i+=n; }

void SheepFileReader::read(const byte* data, unsigned int length)
{
	Sheep::SheepLanguageVersion languageVersion = Sheep::SheepLanguageVersion::V100;

	// read the header
	SheepHeader header;
	unsigned int offset = 0;
	READ4(&header.Magic1, offset);
	READ4(&header.Magic2, offset);
	READ4(&header.Magic3, offset);

	if (header.Magic1 != SheepHeader::Magic1Value || header.Magic2 != SheepHeader::Magic2Value || header.Magic3 != 0)
	{
		throw SheepException("Input file is not a valid sheep file", SHEEP_ERR_INVALID_FILE_FORMAT);
	}

	READ4(&header.ExtraOffset, offset);
	READ4(&header.DataOffset, offset);
	READ4(&header.DataSize, offset);
	READ4(&header.DataCount, offset);

	//header.OffsetArray = new unsigned int[header.DataCount];
	header.OffsetArray = SHEEP_NEW_ARRAY(unsigned int, header.DataCount);

	for (unsigned int i = 0; i < header.DataCount; i++)
		READ4(&header.OffsetArray[i], offset);

	if (header.ExtraOffset != header.DataOffset)
	{
		// read the "extra" header info
		SheepHeaderExtra extra;
		READ2(&extra.VersionMajor, offset);
		READ2(&extra.VersionMinor, offset);
		READ4(&extra.Checksum, offset);
		READ2(&extra.Year, offset);
		READ2(&extra.Month, offset);
		READ2(&extra.DayOfWeek, offset);
		READ2(&extra.Day, offset);
		READ2(&extra.Hour, offset);
		READ2(&extra.Minute, offset);
		READ2(&extra.Second, offset);
		READ2(&extra.Milliseconds, offset);
		
		// we don't care what the copyright info says, so just read until we find the end
		char c;
		do 
		{
			READ1(&c, offset);
		} while (c != 0);

		// update the version
		if (extra.VersionMajor == 1 && extra.VersionMinor == 0)
			languageVersion = Sheep::SheepLanguageVersion::V100;
		else if (extra.VersionMajor == 2 && extra.VersionMinor == 0)
			languageVersion = Sheep::SheepLanguageVersion::V200;
		else
		{
			// unknown version! we don't support it!
			throw SheepException("Input file is not a supported version of Sheep", SHEEP_ERR_INVALID_FILE_FORMAT);
		}
	}

	m_intermediateOutput = SHEEP_NEW IntermediateOutput(languageVersion);

	SectionHeader importHeader, constantsHeader, functionsHeader, variablesHeader, codeHeader;

	for (unsigned int i = 0; i < header.DataCount; i++)
	{
		if (header.OffsetArray[i] >= length)
		{
			delete[] header.OffsetArray;
			throw SheepException("Input file is not a valid sheep file", SHEEP_ERR_INVALID_FILE_FORMAT);
		}

		// seek
		offset = header.DataOffset + header.OffsetArray[i];
	
		char label[13] = {0};
		READN(label, offset, 12);
		if (std::string(label) == "SysImports")
		{
			int bytesRead = 0;
			importHeader = readSectionHeader(data + offset, label, &bytesRead);
			offset += bytesRead;
			unsigned int currentOffset = offset;

			for (unsigned int j = 0; j < importHeader.DataCount; j++)
			{
				SheepImport import;
				
				// seek
				offset = currentOffset + importHeader.OffsetArray[j];

				short lengthOfName;
				byte numReturns, numParameters;

				READ2(&lengthOfName, offset);
				import.Name = readString(data + offset, lengthOfName, &bytesRead);
				offset += bytesRead + 1;
				READ1(&numReturns, offset);
				READ1(&numParameters, offset);
				
				import.ReturnType = (SheepSymbolType)numReturns;

				for (byte k = 0; k < numParameters; k++)
				{
					byte parameterType;
					READ1(&parameterType, offset);

					if (parameterType == (byte)SheepSymbolType::Int ||
						parameterType == (byte)SheepSymbolType::Float ||
						parameterType == (byte)SheepSymbolType::String)
						import.Parameters.push_back((SheepSymbolType)parameterType);
				}
					
				m_intermediateOutput->Imports.push_back(import);
			}
		}
		else if (std::string(label) == "StringConsts")
		{
			int bytesRead = 0;
			constantsHeader = readSectionHeader(data + offset, "StringConsts", &bytesRead);
			offset += bytesRead;
	
			unsigned int currentOffset = offset;

			for (unsigned int j = 0; j < constantsHeader.DataCount; j++)
			{
				// seek
				offset = currentOffset + constantsHeader.OffsetArray[j];

				SheepStringConstant constant;
				constant.Offset = constantsHeader.OffsetArray[j];
				constant.Value = readString(data + offset, &bytesRead);
				offset += bytesRead;

				m_intermediateOutput->Constants.push_back(constant);
			}
		}
		else if (std::string(label) == "Variables")
		{				
			int bytesRead = 0;
			variablesHeader = readSectionHeader(data + offset, "Variables", &bytesRead);
			offset += bytesRead;
			
			unsigned int currentOffset = offset;
			
			for (unsigned int j = 0; j < variablesHeader.DataCount; j++)
			{
				// seek
				offset = currentOffset + variablesHeader.OffsetArray[j];
				
				unsigned short len;
				READ2(&len, offset);

				SheepSymbol symbol;
				symbol.Name = readString(data + offset, len+1, &bytesRead);
				offset += bytesRead;

				unsigned int type, value;
				READ4(&type, offset);
				READ4(&value, offset);

				if (type == (unsigned int)SheepSymbolType::Int)
				{
					symbol.Type = SheepSymbolType::Int;
					symbol.InitialIntValue = offset;
				}
				else if (type == (unsigned int)SheepSymbolType::Float)
				{
					symbol.Type = SheepSymbolType::Float;
					symbol.InitialFloatValue = offset;
				}
				else if (type == (unsigned int)SheepSymbolType::String)
				{
					symbol.Type = SheepSymbolType::String;
					symbol.InitialStringValue = offset;
				}
				
				m_intermediateOutput->Symbols.push_back(symbol);
			}
		}
		else if (std::string(label) == "Functions")
		{
			int bytesRead = 0;
			functionsHeader = readSectionHeader(data + offset, "Functions", &bytesRead);
			offset += bytesRead;
			unsigned int currentOffset = offset;
			
			for (unsigned int j = 0; j < functionsHeader.DataCount; j++)
			{
				// seek
				offset = currentOffset + functionsHeader.OffsetArray[j];
			
				SheepFunction func(NULL);

				unsigned short len;
				READ2(&len, offset);

				func.Name = readString(data + offset, len+1, &bytesRead);
				offset += bytesRead;

				char returnType, numParameters;
				READ1(&returnType, offset);
				READ1(&numParameters, offset);

				func.ReturnType = (SheepSymbolType)returnType;
				for (int k = 0; k < numParameters; k++)
				{
					char paramType;
					READ1(&paramType, offset);

					SheepSymbol param;
					param.Type = (SheepSymbolType)paramType;
					func.Parameters.push_back(param);
				}
				
				READ4(&func.CodeOffset, offset);

				func.ParentCode = m_intermediateOutput;
				m_intermediateOutput->Functions.push_back(func);
			}
		}
		else if (std::string(label) == "Code")
		{
			int bytesRead = 0;
			codeHeader = readSectionHeader(data + offset, "Code", &bytesRead);
			offset += bytesRead;
			unsigned int currentOffset = offset;

			if (codeHeader.DataCount > 1)
				throw SheepException("Extra code sections found", SHEEP_ERR_INVALID_FILE_FORMAT);

			// seek
			offset = currentOffset + codeHeader.OffsetArray[0];
			currentOffset = offset;
			
			std::vector<SheepFunction>& functions = m_intermediateOutput->Functions;
			for (unsigned int j = 0; j < functions.size(); j++)
			{	
				// seek
				offset = currentOffset + functions[j].CodeOffset;
				unsigned int size = 0;
				if (j == functions.size() - 1)
					size = codeHeader.DataSize - functions[j].CodeOffset;
				else
					size = functions[j+1].CodeOffset - functions[j].CodeOffset;

				functions[j].Code = SHEEP_NEW SheepCodeBuffer(size);
				functions[j].Code->Write((const char*)data + offset, size); 
			}
		}
		else
		{
			throw SheepException("Unrecognized data section", SHEEP_ERR_INVALID_FILE_FORMAT);
			
		}
	}

	// clean up
	delete[] header.OffsetArray;
	if (importHeader.OffsetArray) delete[] importHeader.OffsetArray;
	if (constantsHeader.OffsetArray) delete[] constantsHeader.OffsetArray;
	if (variablesHeader.OffsetArray) delete[] variablesHeader.OffsetArray;
	if (functionsHeader.OffsetArray) delete[] functionsHeader.OffsetArray;
	if (codeHeader.OffsetArray) delete[] codeHeader.OffsetArray;
}

void SheepFileReader::WireImportCallbacks(SheepMachine* vm)
{
}

SectionHeader SheepFileReader::readSectionHeader(const byte* data, const std::string& name, int* bytesRead)
{
	SectionHeader header;
	
	int offset = 0;
	strncpy(header.Label, name.c_str(), 12);
	READ4(&header.ExtraOffset, offset);
	READ4(&header.DataOffset, offset);
	READ4(&header.DataSize, offset);
	READ4(&header.DataCount, offset);

	//header.OffsetArray = new unsigned int[header.DataCount];
	header.OffsetArray = SHEEP_NEW_ARRAY(unsigned int, header.DataCount);

	for (unsigned int i = 0; i < header.DataCount; i++)
		READ4(&header.OffsetArray[i], offset);

	if (bytesRead) *bytesRead = offset;
	return header;
}

std::string SheepFileReader::readString(const byte* data, int* bytesRead)
{
	char c;
	std::string str;
	
	int offset = 0;

	c = (char)data[offset++];
	while(c != 0)
	{
		str.push_back(c);
		c = (char)data[offset++];
	}

	if (bytesRead) *bytesRead = offset;
	return str;
}

std::string SheepFileReader::readString(const byte* data, unsigned int length, int* bytesRead)
{
	char c;
	std::string str;
	
	int offset = 0;

	for (unsigned int i = 0; i < length; i++)
	{
		c = (char)data[offset++];
		if (c == 0) break;
			
		str.push_back(c);
	}

	if (bytesRead) *bytesRead = offset;
	return str;
}

unsigned int SheepFileReader::getFileSize(std::ifstream& file)
{
	std::ifstream::pos_type pos = file.tellg();
	file.seekg(0, std::ios_base::end);

	std::ifstream::pos_type end = file.tellg();
	file.seekg(pos, std::ios_base::beg);

	return end;
}

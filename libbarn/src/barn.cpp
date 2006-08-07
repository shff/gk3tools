// Copyright (c) 2006 Brad Farris
// This file is licensed under the MIT license. You can do whatever you
// want with this file as long as this notice remains at the top.

// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#include "barn.h"
#include "barn_internal.h"
#include <vector>
#include <iostream>

BarnHandle brn_OpenBarn(const char* filename)
{
	try
	{
		Barn::Barn* brn = new Barn::Barn(filename);
		
		return brn;
	}
	catch(Barn::BarnException& ex)
	{
		std::cout << "BARN ERROR: "  << ex.Message << std::endl;
		return NULL;
	}
}

void brn_CloseBarn(BarnHandle barn)
{
	Barn::Barn* brn = static_cast<Barn::Barn*>(barn);
	
	delete brn;
}

unsigned int brn_GetNumFilesInBarn(BarnHandle barn)
{
	Barn::Barn* brn = static_cast<Barn::Barn*>(barn);
	
	return brn->GetNumberOfFiles();
}

int brn_GetFileName(BarnHandle barn, unsigned int index, char* buffer, int size)
{
	Barn::Barn* brn = static_cast<Barn::Barn*>(barn);
	
	
	if (index >= brn->GetNumberOfFiles())
		return BARNERR_INVALID_INDEX;
	
	std::string name = brn->GetFileName(index);
		
	strncpy(buffer, name.c_str(), size);
	
	return BARN_SUCCESS;
}

int brn_GetFileSizeByIndex(BarnHandle barn, unsigned int index)
{
	Barn::Barn* brn = static_cast<Barn::Barn*>(barn);

	if (index >= brn->GetNumberOfFiles())
		return BARNERR_INVALID_INDEX;
	
	return brn->GetFileSize(index);
}

int brn_GetFileBarn(BarnHandle barn, unsigned int index, char* buffer, int size)
{
	Barn::Barn* brn = static_cast<Barn::Barn*>(barn);
	
	if (index >= brn->GetNumberOfFiles())
		return BARNERR_INVALID_INDEX;
	
	std::string str = brn->GetFileBarn(index);
	
	strncpy(buffer, str.c_str(), size);
	
	return BARN_SUCCESS;
}

int brn_GetFileCompressionByIndex(BarnHandle barn, unsigned int index)
{
	Barn::Barn* brn = static_cast<Barn::Barn*>(barn);
	
	return brn->GetFileCompression(index);
}

int brn_GetFileOffsetByIndex(BarnHandle barn, unsigned int index)
{
	Barn::Barn* brn = static_cast<Barn::Barn*>(barn);
	
	return brn->GetFileOffset(index);
}

int brn_ExtractFileCompressedByIndex(BarnHandle barn, unsigned int index, 
	const char* outputPath, bool openChildBarns, bool decompress, bool convertBitmaps)
{
	Barn::Barn* brn = static_cast<Barn::Barn*>(barn);
	
	return brn->ExtractFileByIndex(index, outputPath, openChildBarns, false);
}

namespace Barn
{
	Barn::Barn(const std::string& filename)
	{
		m_file = new std::ifstream(filename.c_str(), std::ios::in | std::ios::binary);
		
		if (m_file->good() == false)
		{
			delete m_file;
			throw BarnException("Unable to open barn file", BARNERR_FILE_NOT_FOUND);
		}
		
		if (readUInt32(m_file) != Magic1 || readUInt32(m_file) != Magic2)
		{
			m_file->close();
			delete m_file;
			throw BarnException("Barn is not valid", BARNERR_INVALID_BARN);
		}
		
		// read some boring stuff
		readUInt16(m_file);
		readUInt16(m_file);
		readUInt16(m_file);
		readUInt16(m_file);
		readUInt32(m_file);
			
		unsigned int dirOffset = readUInt32(m_file);
		
		// seek to the directory section
		m_file->seekg(dirOffset);
		
		unsigned int numDirectories = readUInt32(m_file);
		
		std::cout << "Num directories: " << numDirectories << std::endl;

		// load all the DDir offsets
		std::vector<unsigned int> headerOffsets;
		std::vector<unsigned int> dataOffsets;
		
		for (unsigned int i = 0; i < numDirectories; i++)
		{
			unsigned int type = readUInt32(m_file);
			
			readUInt16(m_file);
			readUInt16(m_file);
			readUInt32(m_file);
			readUInt32(m_file);
			readUInt32(m_file);
			
			unsigned int headerOffset = readUInt32(m_file);
			unsigned int dataOffset = readUInt32(m_file);
			
			if (type == DDir)
			{
				headerOffsets.push_back(headerOffset);
				dataOffsets.push_back(dataOffset);
			}
		}
		
		// go through all the DDir's and gather all the files
		m_numFiles = 0;
		for (unsigned int i = 0; i < headerOffsets.size(); i++)
		{
			m_file->seekg(headerOffsets[i]);
			
			std::string barnName = readString(m_file, 32);
			std::cout << "Barn name: " << barnName << std::endl;
			readUInt32(m_file);
			readString(m_file, 40);
			readUInt32(m_file);
			
			unsigned int numFiles = readUInt32(m_file);
			
			m_file->seekg(dataOffsets[i]);
			
			for (unsigned int j = 0; j < numFiles; j++)
			{
				unsigned int size = readUInt32(m_file);
				unsigned int offset = readUInt32(m_file);
				readUInt32(m_file);
				readByte(m_file);
				Compression compression = (Compression)readByte(m_file);
				unsigned char len = readByte(m_file);
				std::string fileName = readString(m_file, len + 1);
				
				BarnFile barn(fileName, size, compression, barnName, offset);
				m_fileMap.insert(std::pair<std::string, BarnFile>(fileName, barn));
				m_fileList.push_back(barn);
				m_numFiles++;
			}
		}
	}
	
	Barn::~Barn()
	{
		if (m_file)
		{
			m_file->close();
			delete m_file;
		}
	}
	
	std::string Barn::GetFileName(unsigned int index)
	{
		return m_fileList[index].name;
	}

	unsigned int Barn::GetFileSize(unsigned int index)
	{
		return m_fileList[index].size;
	}

	std::string Barn::GetFileBarn(unsigned int index)
	{
		return m_fileList[index].barn;
	}

	unsigned int Barn::GetFileOffset(unsigned int index)
	{
		return m_fileList[index].offset;
	}

	Compression Barn::GetFileCompression(unsigned int index)
	{
		return m_fileList[index].compression;
	}
	
	int Barn::ExtractFileByIndex(unsigned int index, const std::string& outputPath, 
		bool openChild, bool uncompress)
	{
		if (index >= m_fileList.size())
			return BARNERR_INVALID_INDEX;
		
		unsigned int size = m_fileList[index].size;
		
		ExtractBuffer* buffer = new ExtractBuffer(size);
		buffer->ReadFromFile(*m_file,  m_fileList[index].offset);
		
		if (m_fileList[index].barn != "")
			buffer->WriteToFile(outputPath + "/" + m_fileList[index].name);
		else
		{
			delete buffer;
			return BARNERR_UNABLE_TO_OPEN_CHILD_BARN;
		}
		
		delete buffer;
		
		return BARN_SUCCESS;
	}
	
	unsigned char Barn::readByte(std::ifstream* file)
	{
		return readRaw<unsigned char>(file);
	}
	
	unsigned short Barn::readUInt16(std::ifstream* file)
	{
		return readRaw<unsigned short>(file);
	}
	
	unsigned int Barn::readUInt32(std::ifstream* file)
	{
		return readRaw<unsigned int>(file);
	}
	
	std::string Barn::readString(std::ifstream* file, unsigned int length)
	{
		char* cstr = new char[length];
		
		file->read(cstr, length);
		
		std::string str = cstr;
		
		delete[] cstr;
		
		return str;
	}
};

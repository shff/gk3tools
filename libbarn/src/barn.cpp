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

namespace Barn
{
	Barn::Barn(const std::string& filename)
	{
		std::ifstream file(filename.c_str(), std::ios::in | std::ios::binary);
		
		if (file.good() == false)
			throw BarnException("Unable to open barn file", BARNERR_FILE_NOT_FOUND);
		
		if (readUInt32(file) != Magic1 || readUInt32(file) != Magic2)
		{
			file.close();
			throw BarnException("Barn is not valid", BARNERR_INVALID_BARN);
		}
		
		// read some boring stuff
		readUInt16(file);
		readUInt16(file);
		readUInt16(file);
		readUInt16(file);
		readUInt32(file);
			
		unsigned int dirOffset = readUInt32(file);
		
		// seek to the directory section
		file.seekg(dirOffset);
		
		unsigned int numDirectories = readUInt32(file);
		
		std::cout << "Num directories: " << numDirectories << std::endl;

		// load all the DDir offsets
		std::vector<unsigned int> headerOffsets;
		std::vector<unsigned int> dataOffsets;
		
		for (unsigned int i = 0; i < numDirectories; i++)
		{
			unsigned int type = readUInt32(file);
			
			readUInt16(file);
			readUInt16(file);
			readUInt32(file);
			readUInt32(file);
			readUInt32(file);
			
			unsigned int headerOffset = readUInt32(file);
			unsigned int dataOffset = readUInt32(file);
			
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
			file.seekg(headerOffsets[i]);
			
			std::string barnName = readString(file, 32);
			std::cout << "Barn name: " << barnName << std::endl;
			readUInt32(file);
			readString(file, 40);
			readUInt32(file);
			
			unsigned int numFiles = readUInt32(file);
			
			file.seekg(dataOffsets[i]);
			
			for (unsigned int j = 0; j < numFiles; j++)
			{
				unsigned int size = readUInt32(file);
				unsigned int offset = readUInt32(file);
				readUInt32(file);
				readByte(file);
				Compression compression = (Compression)readByte(file);
				unsigned char len = readByte(file);
				std::string fileName = readString(file, len + 1);
				
				BarnFile barn(fileName, size, compression, barnName, offset);
				m_fileMap.insert(std::pair<std::string, BarnFile>(fileName, barn));
				m_fileList.push_back(barn);
				m_numFiles++;
			}
		}
		
		file.close();
	}
	
	Barn::~Barn()
	{
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
	
	unsigned char Barn::readByte(std::ifstream& file)
	{
		return readRaw<unsigned char>(file);
	}
	
	unsigned short Barn::readUInt16(std::ifstream& file)
	{
		return readRaw<unsigned short>(file);
	}
	
	unsigned int Barn::readUInt32(std::ifstream& file)
	{
		return readRaw<unsigned int>(file);
	}
	
	std::string Barn::readString(std::ifstream&file, unsigned int length)
	{
		char* cstr = new char[length];
		
		file.read(cstr, length);
		
		std::string str = cstr;
		
		delete[] cstr;
		
		return str;
	}
};

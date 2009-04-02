// Copyright (c) 2008 Brad Farris
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
#include <sstream>
#include <memory>

#ifndef MIN
#define MIN(a,b) (a < b ? a : b)
#endif

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
	
	return brn->GetFileSize(index, false);
}

int brn_GetFileSizeByName(BarnHandle barn, const char* name)
{
	Barn::Barn* brn = static_cast<Barn::Barn*>(barn);

	return brn->GetFileSize(name, false);
}

int brn_GetDecompressedFileSizeByIndex(BarnHandle barn, unsigned int index)
{
	Barn::Barn* brn = static_cast<Barn::Barn*>(barn);

	return brn->GetFileSize(index, true);
}

int brn_GetDecompressedFileSizeByName(BarnHandle barn, const char* name)
{
	Barn::Barn* brn = static_cast<Barn::Barn*>(barn);

	return brn->GetFileSize(name, true);
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

int brn_ReadFile(BarnHandle barn, const char* name, char* buffer, int bufferSize, bool openChildBarns)
{
	assert(name != NULL);
	assert(buffer != NULL);
	assert(bufferSize > 0);

	try
	{
		Barn::Barn* brn = static_cast<Barn::Barn*>(barn);

		std::auto_ptr<Barn::ExtractBuffer> extractbuffer(brn->ReadFile(name, true, openChildBarns));
		
		memcpy(buffer, extractbuffer->GetBuffer(), MIN(extractbuffer->GetSize(), bufferSize));

		return MIN(extractbuffer->GetSize(), bufferSize);
	}
	catch(Barn::BarnException& ex)
	{
		return ex.ErrorNumber;
	}
}

int brn_ExtractFileByIndex(BarnHandle barn, unsigned int index,
	const char* outputPath, bool openChildBarns, bool decompress)
{
	Barn::Barn* brn = static_cast<Barn::Barn*>(barn);
	
	std::string outputPathStr;
	if (outputPath != NULL)
		outputPathStr = outputPath;
	
	return brn->ExtractFileByIndex(index,outputPathStr,
		openChildBarns, decompress);
}

void brn_GetLibInfo(char* buffer, int size)
{
	std::stringstream ss;
	ss << "LibBarn v" << BARN_VERSION_MAJOR << "." << BARN_VERSION_MINOR << "." << BARN_VERSION_REVISION << std::endl
		<< "Compiled on " << __DATE__ << " at " << __TIME__;
	
	strncpy(buffer, ss.str().c_str(), size);
}


namespace Barn
{
	Barn::Barn(const std::string& name, const std::string& path)
	{
		m_name = name;
		m_path = path;

		load(name, path);
	}

	Barn::Barn(const std::string& filename)
	{
		std::string::size_type lastSlash = filename.rfind("\\");
		if (lastSlash == filename.npos)
			lastSlash = filename.rfind("/");

		if (lastSlash == filename.npos)
		{
			m_name = filename;
			m_path = "";
		}
		else if (lastSlash == filename.length()-1)
		{
			throw BarnException("Invalid filename", BARNERR_FILE_NOT_FOUND);
		}
		else
		{
			m_name = filename.substr(lastSlash+1);
			m_path = filename.substr(0, lastSlash+1);
		}

		load(m_name, m_path);
	}

	Barn::~Barn()
	{
		// close any child barns
		while(m_openChildBarns.empty() == false)
		{
			delete m_openChildBarns.back();
			m_openChildBarns.pop_back();
		}

		if (m_file)
		{
			m_file->close();
			delete m_file;
		}
	}

	std::string Barn::GetFileName(unsigned int index) const
	{
		return m_fileList[index].name;
	}

	unsigned int Barn::GetFileSize(unsigned int index, bool decompressedSize)
	{
		if (decompressedSize == false || m_fileList[index].compression == None)
			return m_fileList[index].size;

		// they want the actual decompressed size of the file, which is a little tricky.
		if (m_fileList[index].barn == "")
		{
			unsigned int size;
			
			m_file->seekg(m_fileList[index].offset + m_dataOffset);
			m_file->read((char*)&size, 4);

			return size;
		}
		
		// still here? the file you are looking for is in another barn...
		Barn* barn = openBarn(m_fileList[index].barn);
		return barn->GetFileSize(m_fileList[index].name, true);
	}

	unsigned int Barn::GetFileSize(const std::string& name, bool decompressedSize)
	{
		FileMap::const_iterator itr = m_fileMap.find(name);

		if (itr == m_fileMap.end())
			throw BarnException("The specified file does not exist", BARNERR_FILE_NOT_FOUND);

		if (decompressedSize == false || (*itr).second.compression == None)
			return (*itr).second.size;

		// they want the actual decompressed size of the file, which is a little tricky.
		if ((*itr).second.barn == "")
		{
			unsigned int size;
			
			m_file->seekg((*itr).second.offset + m_dataOffset);
			m_file->read((char*)&size, 4);

			return size;
		}
		
		// still here? the file you are looking for is in another barn...
		Barn* barn = openBarn((*itr).second.barn);
		return barn->GetFileSize((*itr).second.name, true);
	}

	std::string Barn::GetFileBarn(unsigned int index) const
	{
		return m_fileList[index].barn;
	}

	unsigned int Barn::GetFileOffset(unsigned int index) const
	{
		return m_fileList[index].offset;
	}

	Compression Barn::GetFileCompression(unsigned int index) const
	{
		return m_fileList[index].compression;
	}
	int Barn::ExtractFileByIndex(unsigned int index, const std::string& outputPath, 
		bool openChild, bool uncompress)
	{
		if (index >= m_fileList.size())
			return BARNERR_INVALID_INDEX;

		try
		{
			ExtractBuffer* buffer = NULL;

			if (m_fileList[index].barn == "")
			{
				buffer = ReadRaw(m_fileList[index].offset, m_fileList[index].size,
					m_fileList[index].compression, uncompress);
			}
			else
			{
				Barn* barn = openBarn(m_fileList[index].barn);
				
				// extract the file
				buffer = barn->ReadRaw(m_fileList[index].offset, m_fileList[index].size,
					m_fileList[index].compression, uncompress);
			}

			// make sure the path ends with a slash
			std::string tweakedOutputPath = outputPath;
			if (outputPath.length() > 0 && (outputPath[outputPath.length()-1] != '/' && outputPath[outputPath.length()-1] != '\\'))
				tweakedOutputPath = outputPath + '/';

			std::cout << "Writing to " << tweakedOutputPath << m_fileList[index].name <<std::endl;
			buffer->WriteToFile(tweakedOutputPath + m_fileList[index].name);
			delete buffer;
		}
		catch(BarnException& ex)
		{
			return ex.ErrorNumber;
		}
		
		return BARN_SUCCESS;
	}

	ExtractBuffer* Barn::ReadFile(const std::string& filename, bool decompress, bool openChildBarns)
	{
		FileMap::iterator itr = m_fileMap.find(filename);

		if (itr == m_fileMap.end())
			throw BarnException("The specified filename does not exist", BARNERR_FILE_NOT_FOUND);

		BarnFile file = (*itr).second;

		if (file.barn == "")
			return ReadRaw(file.offset, file.size, file.compression, decompress);

		// look for the barn in the open list
		Barn* barn = openBarn(file.barn);

		// extract the file
		return barn->ReadRaw(file.offset, file.size, file.compression, decompress);
	}
	ExtractBuffer* Barn::ReadRaw(unsigned int offset, unsigned int size, Compression compression, bool decompress)
	{
		bool compressed = false;
			
		if (compression == LZO || compression == ZLib)
			compressed = true;

		if (compressed && decompress)
			size += 8;

		std::auto_ptr<ExtractBuffer> buffer(new ExtractBuffer(size));

		buffer->ReadFromFile(*m_file, offset + m_dataOffset + (compressed && !decompress ? 8 : 0));
					
		if (decompress && compressed)
		{
			buffer->Decompress(compression);
		}

		return buffer.release();
	}

	
	// Privates //////////////////////////////////////////////////////////

	void Barn::load(const std::string& filename, const std::string& path)
	{
		std::string fullpath = path + filename;

		// TODO: attempt various case versions (all caps, etc) if this fails
		m_file = new std::ifstream(fullpath.c_str(), std::ios::in | std::ios::binary);
		
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
			else if (type == Data)
			{
				m_dataOffset = headerOffset;
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

	Barn* Barn::openBarn(const std::string& filename)
	{
		try
		{
			// look for the barn in the open list
			Barn* barn = NULL;
			for (std::vector<Barn*>::iterator itr = m_openChildBarns.begin();
				itr != m_openChildBarns.end(); itr++)
			{
				if ((*itr)->GetBarnName() == filename)
				{
					barn = (*itr);
					break;
				}
			}

			// if we didn't find the barn then open it and add it to the 
			// list of open barns
			if (barn == NULL)
			{
				barn = new Barn(filename, m_path);

				m_openChildBarns.push_back(barn);
			}

			return barn;
		}
		catch(BarnException& ex)
		{
			throw BarnException("Unable to open child barn", BARNERR_UNABLE_TO_OPEN_CHILD_BARN);
		}
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

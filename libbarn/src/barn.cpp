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

#ifdef _MSC_VER
#define _CRT_SECURE_NO_WARNINGS
#endif

#include "barn.h"
#include "barn_internal.h"
#include "fs.h"
#include <vector>
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
		return nullptr;
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

int brn_GetFileName(BarnHandle barn, int index, char* buffer, int size)
{
	Barn::Barn* brn = static_cast<Barn::Barn*>(barn);
	
	try
	{
		strncpy(buffer, brn->GetFileName(index), size);
		if (size > 0) buffer[size - 1] = 0;

		return BARN_SUCCESS;
	}
	catch(Barn::BarnException& ex)
	{
		return ex.ErrorNumber;
	}
}

int brn_GetFileIndex(BarnHandle barn, const char* name)
{
	Barn::Barn* brn = static_cast<Barn::Barn*>(barn);

	try
	{
		return brn->GetFileIndex(name);
	}
	catch(Barn::BarnException& ex)
	{
		return ex.ErrorNumber;
	}
}

int brn_GetFileSize(BarnHandle barn, int index)
{
	Barn::Barn* brn = static_cast<Barn::Barn*>(barn);

	try
	{
		return brn->GetFileSize(index);
	}
	catch(Barn::BarnException& ex)
	{
		return ex.ErrorNumber;
	}
}

int brn_GetDecompressedFileSize(BarnHandle barn, int index)
{
	Barn::Barn* brn = static_cast<Barn::Barn*>(barn);

	try
	{
		return brn->GetUncompressedFileSize(index);
	}
	catch(Barn::BarnException& ex)
	{
		return ex.ErrorNumber;
	}
}

int brn_GetFileBarn(BarnHandle barn, int index, char* buffer, int size)
{
	Barn::Barn* brn = static_cast<Barn::Barn*>(barn);
	
	try
	{
		strncpy(buffer, brn->GetFileBarn(index), size);
		if (size > 0) buffer[size - 1] = 0;
	}
	catch(Barn::BarnException& ex)
	{
		return ex.ErrorNumber;
	}
	
	return BARN_SUCCESS;
}

int brn_GetFileCompression(BarnHandle barn, int index)
{
	Barn::Barn* brn = static_cast<Barn::Barn*>(barn);
	
	try
	{
		return (int)brn->GetFileCompression(index);
	}
	catch(Barn::BarnException& ex)
	{
		return ex.ErrorNumber;
	}
}

int brn_ReadFileRaw(BarnHandle barn, int index, char* buffer, int bufferSize)
{
	if (buffer == nullptr)
		return BARNERR_UNKNOWN;
	if (bufferSize <= 0)
		return 0;

	try
	{
		Barn::Barn* brn = static_cast<Barn::Barn*>(barn);

		return brn->ReadRaw(index, buffer, bufferSize);
	}
	catch(Barn::BarnException& ex)
	{
		return ex.ErrorNumber;
	}
}

int brn_ReadFileDecompress(BarnHandle barn, int index, char* buffer, int bufferSize)
{
	if (buffer == nullptr)
		return BARNERR_UNKNOWN;
	if (bufferSize <= 0)
		return 0;

	try
	{
		Barn::Barn* brn = static_cast<Barn::Barn*>(barn);

		return brn->ReadDecompress(index, buffer, bufferSize);
	}
	catch(Barn::BarnException& ex)
	{
		return ex.ErrorNumber;
	}
}

void brn_GetLibInfo(char* buffer, int size)
{
	std::stringstream ss;
	ss << "LibBarn v" << BARN_VERSION_MAJOR << "." << BARN_VERSION_MINOR << "." << BARN_VERSION_REVISION << std::endl
		<< "Compiled on " << __DATE__ << " at " << __TIME__;
	
	strncpy(buffer, ss.str().c_str(), size);
	if (size > 0) buffer[size - 1] = 0;
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
		FileSystem::Close(&m_file);
	}

	int Barn::GetFileIndex(const char* name)
	{
		std::string strname = name;
		auto file = m_fileMap.find(strname);
		if (file == m_fileMap.end())
			return BARNERR_FILE_NOT_FOUND;

		return (*file).second.index;
	}

	const char* Barn::GetFileName(int index) const
	{
		if (index < 0 || (unsigned int)index >= m_fileList.size())
			throw BarnException("Index out of range", BARNERR_INVALID_INDEX);

		return m_fileList[index].name.c_str();
	}

	unsigned int Barn::GetFileSize(int index)
	{
		if (index < 0 || (unsigned int)index >= m_fileList.size())
			throw BarnException("Index out of range", BARNERR_INVALID_INDEX);

		return m_fileList[index].size;
	}

	unsigned int Barn::GetUncompressedFileSize(int index)
	{
		if (index < 0 || (unsigned int)index >= m_fileList.size())
			throw BarnException("Index out of range", BARNERR_INVALID_INDEX);

		BarnFile& barn = m_fileList[index];

		// just use the regular size if the file isn't compressed
		if (barn.compression == Compression::None)
			return barn.size;

		// if we already know the uncompressed size then don't go get it
		if (barn.uncompressedSize != 0)
			return barn.uncompressedSize;

		// don't bother trying if the current barn doesn't contain this file
		if (barn.barn.empty() == false)
			throw BarnException("Function doesn't support opening child barns", BARNERR_UNABLE_TO_OPEN_CHILD_BARN);

		// go get the uncompressed size
		FileReader reader;
		FileReader::Create(&m_file, &reader);
		reader.Seek(m_fileList[index].offset + m_dataOffset, true);

		barn.uncompressedSize = reader.ReadUInt32();

		return barn.uncompressedSize;
	}

	const char* Barn::GetFileBarn(int index) const
	{
		if (index < 0 || (unsigned int)index >= m_fileList.size())
			throw BarnException("Index out of range", BARNERR_INVALID_INDEX);

		return m_fileList[index].barn.c_str();
	}

	unsigned int Barn::GetFileOffset(int index) const
	{
		if (index < 0 || (unsigned int)index >= m_fileList.size())
			throw BarnException("Index out of range", BARNERR_INVALID_INDEX);

		return m_fileList[index].offset;
	}

	Compression Barn::GetFileCompression(int index) const
	{
		if (index < 0 || (unsigned int)index >= m_fileList.size())
			throw BarnException("Index out of range", BARNERR_INVALID_INDEX);

		return m_fileList[index].compression;
	}
	
	unsigned int Barn::ReadRaw(int index, char* buffer, unsigned int bufferSize)
	{
		if (index < 0 || (unsigned int)index >= m_fileList.size())
			throw BarnException("Index out of range", BARNERR_INVALID_INDEX);

		BarnFile file = m_fileList[index];

		if (file.barn.empty())
		{
			size_t offset = file.offset + m_dataOffset + (file.compression != Compression::None ? 8 : 0);
			size_t bytesToRead = MIN(bufferSize, file.size);

			if (offset + bytesToRead > m_file.FileSize)
				bytesToRead = m_file.FileSize - offset;

			memcpy(buffer, (unsigned char*)m_file.Memory + offset, bytesToRead);

			return (unsigned int)bytesToRead;
		}

		// uh oh, apparently the file is in a child barn. We can't read it.
		throw BarnException("The file data is in another barn", BARNERR_UNABLE_TO_OPEN_CHILD_BARN);
	}

	unsigned int Barn::ReadDecompress(int index, char* buffer, unsigned int bufferSize)
	{
		if (index < 0 || (unsigned int)index >= m_fileList.size())
			throw BarnException("Index out of range", BARNERR_INVALID_INDEX);

		BarnFile file = m_fileList[index];

		// just use a regular read if this is uncompressed
		if (file.compression == Compression::None)
			return ReadRaw(index, buffer, bufferSize);

		if (file.barn.empty())
		{
			unsigned int offset = file.offset + m_dataOffset;
			if (offset + 8 + file.size > m_file.FileSize)
				throw BarnException("Invalid barn file", BARNERR_INVALID_BARN);

			FileReader reader;
			FileReader::Create(&m_file, &reader);
			reader.Seek(offset, true);

			unsigned int uncompressedSize = reader.ReadUInt32();
			file.uncompressedSize = uncompressedSize; // don't know why this wouldn't match (or not be set... unless the client doesn't know how to use the API)

			// make sure the output buffer can hold all the uncompressed data
			// (we *could* just create a buffer big enough ourselves and copy a portion of the data, but why?)
			if (file.uncompressedSize > bufferSize)
				throw BarnException("The output buffer must be big enough to hold the entire uncompressed file", BARNERR_DECOMPRESSION_ERROR);

			// skip compressed size (we already know it)
			reader.Seek(4, false);

			ExtractBuffer::Decompress(file.compression, (char*)reader.Position, file.size, buffer, file.uncompressedSize);

			// return the number of *uncompressed* bytes
			return file.uncompressedSize;
		}

		// uh oh, apparently the file is in a child barn. We can't read it.
		throw BarnException("The file data is in another barn", BARNERR_UNABLE_TO_OPEN_CHILD_BARN);
	}

	// Privates //////////////////////////////////////////////////////////

	void Barn::load(const std::string& filename, const std::string& path)
	{
		std::string fullpath = path + filename;

		// TODO: attempt various case versions (all caps, etc) if this fails
		if (FileSystem::OpenRead(fullpath.c_str(), &m_file) == false)
		{
			throw BarnException("Unable to open barn file", BARNERR_FILE_NOT_FOUND);
		}
		
		FileReader reader;
		FileReader::Create(&m_file, &reader);

		if (reader.ReadUInt32() != Magic1 || reader.ReadUInt32() != Magic2)
		{
			FileSystem::Close(&m_file);
			throw BarnException("Barn is not valid", BARNERR_INVALID_BARN);
		}
		
		// read some boring stuff
		reader.ReadUInt16();
		reader.ReadUInt16();
		reader.ReadUInt16();
		reader.ReadUInt16();
		reader.ReadUInt32();
			
		unsigned int dirOffset = reader.ReadUInt32();
		
		// seek to the directory section
		reader.Seek(dirOffset, true);
		
		unsigned int numDirectories = reader.ReadUInt32();

		// load all the DDir offsets
		std::vector<unsigned int> headerOffsets;
		std::vector<unsigned int> dataOffsets;

		headerOffsets.reserve(numDirectories);
		dataOffsets.reserve(numDirectories);
		
		for (unsigned int i = 0; i < numDirectories; i++)
		{
			unsigned int type = reader.ReadUInt32();
			
			reader.ReadUInt16();
			reader.ReadUInt16();
			reader.ReadUInt32();
			reader.ReadUInt32();
			reader.ReadUInt32();
			
			unsigned int headerOffset = reader.ReadUInt32();
			unsigned int dataOffset = reader.ReadUInt32();
			
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
			reader.Seek(headerOffsets[i], true);
			
			char barnName[33];
			reader.Read(barnName, 32);
			barnName[32] = 0;

			reader.ReadUInt32();
			char dummy[40];
			reader.Read(dummy, 40);
			reader.ReadUInt32();
			
			unsigned int numFiles = reader.ReadUInt32();
			
			reader.Seek(dataOffsets[i], true);
			
			for (unsigned int j = 0; j < numFiles; j++)
			{
				BarnFile file;
				file.barn = barnName;
				file.size = reader.ReadUInt32();
				file.offset = reader.ReadUInt32();

				reader.ReadUInt32();
				reader.ReadByte();
				file.compression = (Compression)reader.ReadByte();

				// for some reason there can be files with an invalid compression value (3, to be specific).
				// treat those as uncompressed.
				if ((int)file.compression == 3)
					file.compression = Compression::None;

				unsigned char len = reader.ReadByte();

				char nameBuffer[257];
				reader.Read(nameBuffer, len + 1);
				file.name = nameBuffer;

				file.index = m_fileList.size();
				
				m_fileMap.insert(std::pair<std::string, BarnFile>(file.name, file));
				m_fileList.push_back(file);
				m_numFiles++;
			}
		}
	}
};

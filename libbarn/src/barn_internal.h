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

#ifndef BARN_INTERNAL_H
#define BARN_INTERNAL_H

#include <iostream>
#include <fstream>
#include <string>
#include <map>
#include <vector>
#include <cassert>
#include <memory.h>

namespace Barn
{
	enum Compression
	{
		None = 0,
		ZLib = 1,
		LZO = 2,
	};

	struct BarnFile
	{
		std::string name;
		std::string barn;
		unsigned int size;
		unsigned int uncompressedSize;
		Compression compression;
		int index;

		unsigned int offset;

		BarnFile()
		{
			size = 0;
			uncompressedSize = 0;
			compression = None;
			index = -1;
			offset = 0;
		}
		
		BarnFile(const std::string& name, unsigned int size, Compression compression,
			const std::string& barnName, unsigned int offset)
		{
			this->name = name;
			this->size = size;
			this->compression = compression;
			this->barn = barnName;
			this->offset = offset;

			this->index = -1;
			this->uncompressedSize = 0; // set to 0 until we know
		}
	};
	
	class BarnException
	{
	public:
		BarnException(const std::string& message, int errnum)
		{
			Message = message;
			ErrorNumber = errnum;
		}
	
		std::string Message;
		int ErrorNumber;
	};
	
	const int Magic1 = 0x21334B47;
	const int Magic2 = 0x6E726142;
	const int DDir = 0x44446972;
	const int Data = 0x44617461;
	
	class ExtractBuffer
	{
	public:
		static void Decompress(Compression compressionType, const char* input, unsigned int inputSize, char* output, unsigned int uncompressedSize);
	};
	
	/// The barn archive
	class Barn
	{
	public:
		
		Barn(const std::string& filename);
		Barn(const std::string& name, const std::string& filename);
		~Barn();
	
		unsigned int GetNumberOfFiles() const { return m_numFiles; }
		int GetFileIndex(const char* name);
		const char* GetBarnName() { return m_name.c_str(); }
		const char* GetFileName(int index) const;
		const char* GetFileBarn(int index) const;
	
		unsigned int GetFileSize(int index);
		unsigned int GetUncompressedFileSize(int index);

		Compression GetFileCompression(int index) const;
		unsigned int GetFileOffset(int index) const;

		unsigned int ReadRaw(int index, char* buffer, unsigned int bufferSize);
		unsigned int ReadDecompress(int index, char* buffer, unsigned int bufferSize);
		
	private:
	
		void load(const std::string& filename, const std::string& path);

		static unsigned char readByte(std::ifstream& file);
		static unsigned short readUInt16(std::ifstream& file);
		static unsigned int readUInt32(std::ifstream& file);
		static void readString(std::ifstream& file, unsigned int length, char* output);
		
		template<typename T>
		static T readRaw(std::ifstream& file)
		{
			// TODO: endian switching!
			
			int size = sizeof(T);

			T data;
			
			if (size == 1)
			{
				file.read((char*)&data, 1);
			}
			else if (size == 2)
			{
				file.read((char*)&data, 2);
			}
			else if (size == 4)
			{
				file.read((char*)&data, 4);
			}
			else
			{
				// BAD BAD BAD! No endian switching is being done!
				// This better be throw away data!!
				file.read((char*)&data, size);
			}

			return data;
		}
	
		std::string m_name;
		std::string m_path;
		unsigned int m_numFiles;
		std::vector<BarnFile> m_fileList;

		struct ci_less
		{
			bool operator() (const std::string& s1, const std::string& s2) const
			{
#ifdef WIN32
				return _stricmp(s1.c_str(), s2.c_str()) < 0;
#else
				return strcasecmp(s1.c_str(), s2.c_str()) < 0;
#endif
			}
		};

		typedef std::map<std::string, BarnFile, ci_less> FileMap;
		FileMap m_fileMap;
		unsigned int m_dataOffset;
		
		std::ifstream m_file;
	};

}

#endif // BARN_INTERNAL_H

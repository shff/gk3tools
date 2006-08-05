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

#ifndef BARN_INTERNAL_H
#define BARN_INTERNAL_H

#include <iostream>
#include <fstream>
#include <string>
#include <map>
#include <vector>
#include <cassert>

namespace Barn
{
	enum Compression
	{
		None = 0,
		ZLib = 1,
		LZO = 2
	};

	struct BarnFile
	{
		std::string name;
		std::string barn;
		unsigned int size;
		Compression compression;
		
		unsigned int offset;
		
		BarnFile(const std::string& name, unsigned int size, Compression compression,
			const std::string& barnName, unsigned int offset)
		{
			this->name = name;
			this->size = size;
			this->compression = compression;
			this->barn = barnName;
			this->offset = offset;
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
	
	/// The barn archive
	class Barn
	{
	public:
		
		Barn(const std::string& filename);
		~Barn();
	
		unsigned int GetNumberOfFiles() { return m_numFiles; }
		std::string GetFileName(unsigned int index);
		std::string GetFileBarn(unsigned int index);
	
		unsigned int GetFileSize(unsigned int index);
		Compression GetFileCompression(unsigned int index);
		unsigned int GetFileOffset(unsigned int index);
		
	private:
		
		static unsigned char readByte(std::ifstream& file);
		static unsigned short readUInt16(std::ifstream& file);
		static unsigned int readUInt32(std::ifstream& file);
		static std::string readString(std::ifstream& file, unsigned int length);
		
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
	
		unsigned int m_numFiles;
		std::vector<BarnFile> m_fileList;
		std::map<std::string, BarnFile> m_fileMap;
	};
}

#endif // BARN_INTERNAL_H

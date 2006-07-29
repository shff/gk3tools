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
		BarnException(const std::string& message)
		{
			Message = message;
		}
	
		std::string Message;
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
			
			if (size == 1)
			{
				T data;
				
				file.read((char*)&data, 1);
			}
			else if (size == 2)
			{
				T data;
				
				file.read((char*)&data, 2);
			}
			else if (size == 4)
			{
				T data;
				
				file.read((char*)&data, 4);
			}
			else
			{
				// BAD BAD BAD! No endian switching is being done!
				// This better be throw away data!!
				
				T data;
				
				file.read((char*)&data, size);
			}
		}
	
		unsigned int m_numFiles;
		std::vector<BarnFile> m_fileList;
		std::map<std::string, BarnFile> m_fileMap;
	};
}

#endif // BARN_INTERNAL_H

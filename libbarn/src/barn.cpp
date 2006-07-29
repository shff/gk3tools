#include "barn.h"
#include "barn_internal.h"
#include <vector>

BarnHandle brn_OpenBarn(const char* filename)
{
	try
	{
		Barn::Barn* brn = new Barn::Barn(filename);
		
		return brn;
	}
	catch(Barn::BarnException& ex)
	{
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
		return -1;
	
	std::string name = brn->GetFileName(index);
		
	strncpy(buffer, name.c_str(), size);
	
	return 0;
}

namespace Barn
{
	Barn::Barn(const std::string& filename)
	{
		std::ifstream file(filename.c_str(), std::ios::in | std::ios::binary);
		
		if (file.good() == false)
			throw BarnException("Unable to open barn file");
		
		if (readUInt32(file) != Magic1 || readUInt32(file) != Magic2)
		{
			file.close();
			throw BarnException("Barn is not valid");
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
		for (unsigned int i = 0; i < headerOffsets.size(); i++)
		{
			file.seekg(headerOffsets[i]);
			
			std::string barnName = readString(file, 32);
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

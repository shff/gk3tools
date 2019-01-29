#ifndef LIBBARN_FS_H
#define LIBBARN_FS_H

struct File
{
	size_t FileSize;
	void* Memory;

#ifdef _WIN32
	void* Handle;
	void* Mapping;
#else
	int Handle;
#endif
};

struct FileReader
{
	unsigned char* Start;
	unsigned char* End;
	unsigned char* Position;

	static void Create(const File* file, FileReader* result);
	
	size_t Read(void* destination, size_t length);
	unsigned int ReadUInt32();
	unsigned short ReadUInt16();
	unsigned char ReadByte();

	void Seek(size_t offset, bool fromStart);
};

class FileSystem
{
public:
	static bool OpenRead(const char* path, File* result);
	static void Close(File* result);
};

#endif // LIBBARN_FS_H

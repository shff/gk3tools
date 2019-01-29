#include "fs.h"

#ifdef _WIN32
#define NOMINMAX
#define WIN32_LEAN_AND_MEAN
#include <Windows.h>
#else
#include <sys/types.h>
#include <dirent.h>
#include <unistd.h>
#include <sys/stat.h>
#include <fcntl.h>
#include <sys/mman.h>

// some stupid garbage is #defining "Success", which conflicts with NxnaResult::Success
#undef Success
#endif
#include <cassert>

void FileReader::Create(const File* file, FileReader* result)
{
	assert(file != nullptr);
	assert(result != nullptr);

	result->Start = (unsigned char*)file->Memory;
	result->End = (unsigned char*)file->Memory + file->FileSize;
	result->Position = result->Start;
}

size_t FileReader::Read(void* destination, size_t length)
{
	size_t bytesToRead = length;
	if (Position + bytesToRead > End)
		bytesToRead = End - Position;

	memcpy(destination, Position, bytesToRead);
	Position += bytesToRead;

	return bytesToRead;
}

unsigned int FileReader::ReadUInt32()
{
	unsigned int result = 0;
	Read(&result, sizeof(unsigned int));
	return result;
}

unsigned short FileReader::ReadUInt16()
{
	unsigned short result = 0;
	Read(&result, sizeof(unsigned short));
	return result;
}

unsigned char FileReader::ReadByte()
{
	unsigned char result = 0;
	if (Position < End)
	{
		result = *Position;
		Position++;
	}

	return result;
}

void FileReader::Seek(size_t offset, bool fromStart)
{
	if (fromStart)
	{
		if (Start + offset > End)
			Position = End;
		else
			Position = Start + offset;
	}
	else
	{
		if (Position + offset > End)
			Position = End;
		else
			Position += offset;
	}
}

bool FileSystem::OpenRead(const char* path, File* file)
{
	file->Memory = nullptr;
	file->FileSize = 0;

#ifdef _WIN32
	file->Handle = CreateFile(path, GENERIC_READ, 0, nullptr, OPEN_EXISTING, FILE_ATTRIBUTE_READONLY, nullptr);
	if (file->Handle == INVALID_HANDLE_VALUE)
	{
		file->Handle = nullptr;
		return false;
	}

	file->Mapping = CreateFileMapping(file->Handle, nullptr, PAGE_READONLY, 0, 0, nullptr);
	if (file->Mapping == nullptr)
	{
		CloseHandle(file->Handle);
		file->Handle = nullptr;
		return false;
	}

	file->Memory = (unsigned char*)MapViewOfFile(file->Mapping, FILE_MAP_READ, 0, 0, 0);
	if (file->Memory == nullptr)
	{
		CloseHandle(file->Mapping);
		CloseHandle(file->Handle);
		file->Mapping = nullptr;
		file->Handle = nullptr;

		return false;
	}

	file->FileSize = GetFileSize(file->Handle, nullptr);

	return true;
#else
	file->Handle = open(path, O_RDONLY);
	if (file->Handle == -1)
		return false;

	struct stat sb;
	if (fstat(file->Handle, &sb) == -1)
	{
		close(file->Handle);
		return false;
	}

	file->Memory = (unsigned char*)mmap(nullptr, sb.st_size, PROT_READ, MAP_PRIVATE, file->Handle, 0);
	if (file->Memory == MAP_FAILED)
	{
		file->Memory = nullptr;
		close(file->Handle);
		return false;
	}

	file->FileSize = sb.st_size;

	return true;
#endif
}

void FileSystem::Close(File* file)
{
#ifdef _WIN32
	if (file == nullptr || file->Handle == nullptr)
		return;

	if (file->Memory != nullptr)
	{
		UnmapViewOfFile(file->Memory);
		file->Memory = nullptr;
	}

	if (file->Mapping != nullptr)
	{
		CloseHandle(file->Mapping);
		file->Mapping = nullptr;
	}

	CloseHandle(file->Handle);
	file->Handle = nullptr;

#else
	if (file == nullptr || file->Handle == -1)
		return;

	if (file->Memory != nullptr)
	{
		munmap(file->Memory, file->FileSize);
		file->Memory = nullptr;
	}

	close(file->Handle);
	file->Handle = -1;

#endif

	file->FileSize = 0;
}

#include <cstdio>
#include <cstdlib>
#include <new>

#include "sheepMemoryAllocator.h"
#include "sheepc.h"

static bool g_usingCustomAllocator = false;

void SHP_SetAllocator(SHP_Allocator* allocator)
{
	g_usingCustomAllocator = true;

	g_allocator.Allocator = allocator->Allocator;
	g_allocator.Deallocator = allocator->Deallocator;
}

void* (SHP_CALLBACK defaultAllocator)(size_t size)
{
	return malloc(size);
}

void SHP_CALLBACK defaultDeallocator(void* ptr)
{
	free(ptr);
}

void makeSureAllocatorIsSet()
{
	if (g_usingCustomAllocator == false)
	{
		g_allocator.Allocator = defaultAllocator;
		g_allocator.Deallocator = defaultDeallocator;
	}
}

const char* g_filename;
int g_lineNumber;

struct SheepAllocInfo
{
	size_t ActualSize;
	size_t RequestedSize;

	void* Data;

	char SourceFile[64];
	int LineNumber;

	SheepAllocInfo* Next;
	SheepAllocInfo* Prev;
};


SheepAllocInfo* g_allocRoot = NULL;
size_t g_totalBytesAllocated = 0;
size_t g_totalBytesDeallocated = 0;


void addNewAllocInfo(SheepAllocInfo* blank, size_t size, const char* filename, int lineNumber)
{
	blank->Data = (byte*)blank + sizeof(SheepAllocInfo);
	blank->RequestedSize = size;
	blank->ActualSize = size + sizeof(SheepAllocInfo);
	if (filename)
		strncpy(blank->SourceFile, filename, 64);
	else
		blank->SourceFile[0] = 0;
	blank->LineNumber = lineNumber;
	blank->Next = g_allocRoot;
	blank->Prev = 0;
	
	if (g_allocRoot)
		g_allocRoot->Prev = blank;
	g_allocRoot = blank;

	//printf("Alloced %d bytes in %s:%d\n", size, filename, lineNumber);

	g_totalBytesAllocated += size;
}

void removeAllocInfo(SheepAllocInfo* info)
{
	SheepAllocInfo* ptr = g_allocRoot;
	while(ptr != 0)
	{
		if (ptr == info)
		{
			if (ptr->Next)
				ptr->Next->Prev = ptr->Prev;
			if (ptr->Prev)
				ptr->Prev->Next = ptr->Next;

			if (g_allocRoot == ptr)
				g_allocRoot = ptr->Next;

			g_totalBytesDeallocated += ptr->RequestedSize;

			return;
		}

		ptr = ptr->Next;
	}

	// we should never be here!
	throw "OH NO!";
}

void* operator new(size_t size)
{
	makeSureAllocatorIsSet();
	
	SheepAllocInfo* p = (SheepAllocInfo*)g_allocator.Allocator(sizeof(SheepAllocInfo) + size);
	if (p == 0)
		throw std::bad_alloc();

	addNewAllocInfo(p, size, g_filename, g_lineNumber);

	return p->Data;
}

void* operator new(size_t size, const char* filename, int lineNumber)
{
	makeSureAllocatorIsSet();
	
	SheepAllocInfo* p = (SheepAllocInfo*)g_allocator.Allocator(sizeof(SheepAllocInfo) + size);
	if (p == 0)
		throw std::bad_alloc();

	addNewAllocInfo(p, size, filename, lineNumber);

	return p->Data;
}

void* operator new[](size_t size)
{
	makeSureAllocatorIsSet();
	
	SheepAllocInfo* p = (SheepAllocInfo*)g_allocator.Allocator(sizeof(SheepAllocInfo) + size);
	if (p == 0)
		throw std::bad_alloc();

	addNewAllocInfo(p, size, g_filename, g_lineNumber);

	return p->Data;
}

void* operator new[](size_t size, const char* filename, int lineNumber)
{
	makeSureAllocatorIsSet();
	
	SheepAllocInfo* p = (SheepAllocInfo*)g_allocator.Allocator(sizeof(SheepAllocInfo) + size);
	if (p == 0)
		throw std::bad_alloc();

	addNewAllocInfo(p, size, filename, lineNumber);

	return p->Data;
}

void operator delete(void* p)
{
	SheepAllocInfo* info = (SheepAllocInfo*)((byte*)p - sizeof(SheepAllocInfo));
	removeAllocInfo(info);

	g_allocator.Deallocator(info);
}

void operator delete[](void* p)
{
	SheepAllocInfo* info = (SheepAllocInfo*)((byte*)p - sizeof(SheepAllocInfo));
	removeAllocInfo(info);

	g_allocator.Deallocator(info);
}

void operator delete(void* p, const char*, int)
{
	SheepAllocInfo* info = (SheepAllocInfo*)((byte*)p - sizeof(SheepAllocInfo));
	removeAllocInfo(info);

	g_allocator.Deallocator(info);
}

void operator delete[](void* p, const char*, int)
{
	SheepAllocInfo* info = (SheepAllocInfo*)((byte*)p - sizeof(SheepAllocInfo));
	removeAllocInfo(info);

	g_allocator.Deallocator(info);
}


void SHP_PrintMemoryUsage()
{
	printf("Beginning Memory Report:\n");

	SheepAllocInfo* ptr = g_allocRoot;
	while(ptr != 0)
	{
		printf("%d bytes allocated at %s:%d\n", ptr->RequestedSize, ptr->SourceFile, ptr->LineNumber);

		SheepAllocInfo* tmp = ptr;
		ptr = ptr->Next;
	}

	printf("\n%d bytes allocated, %d bytes freed\n", g_totalBytesAllocated, g_totalBytesDeallocated);

	printf("End of Memory Report\n\n");
}


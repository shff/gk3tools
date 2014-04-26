#ifndef SHEEPMEMORYALLOCATOR_H
#define SHEEPMEMORYALLOCATOR_H

#ifndef NDEBUG
#include <map>
#endif

#include <cassert>

#include "sheepc.h"

extern SHP_Allocator g_allocator;

#ifndef _MSC_VER
#define NOEXCEPT noexcept
#else
#define NOEXCEPT
#endif


void* operator new(size_t);
void* operator new(size_t size, const char* filename, int lineNumber);
void operator delete(void* p) NOEXCEPT;
void operator delete(void* p, const char*, int) NOEXCEPT;

#define SHEEP_NEW new(__FILE__, __LINE__)
#define SHEEP_DELETE(p) delete p

void* operator new[](size_t);
void* operator new[](size_t, const char*, int);
void operator delete[](void*) NOEXCEPT;
void operator delete[](void*, const char*, int) NOEXCEPT;

#define SHEEP_NEW_ARRAY(T,l) new(__FILE__, __LINE__) T[l]
#define SHEEP_DELETE_ARRAY(p) delete[] p

// provide a custom "auto_ptr" that uses our delete
template<typename T>
class shp_auto_ptr
{
	T* m_data;

public:
	shp_auto_ptr(T* t)
	{
		m_data = t;
	}

	~shp_auto_ptr()
	{
		if (m_data)
			delete(__FILE__, __LINE__, 5,7,6,5,6,7,6, m_data);
			//SHEEP_DELETE(m_data);
	}

	T* operator->() const
	{
		return m_data;
	}

	T* get() const
	{
		return m_data;
	}

	T* release()
	{
		T* tmp = m_data;
		m_data = NULL;

		return tmp;
	}
};



#endif // SHEEPMEMORYALLOCATOR_H

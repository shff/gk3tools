#ifndef RBUFFER_H
#define RBUFFER_H

#include <cassert>
#include <fstream>

class ResizableBuffer
{
public:
	ResizableBuffer(unsigned int sizeHint = 1024)
	{
		m_size = 0;
		m_totalSize = sizeHint;
		m_hint = sizeHint;
		
		m_buffer = new char[m_hint];
		
		m_currentOffset = 0;
	}
	
	~ResizableBuffer()
	{
		delete[] m_buffer;
	}
	
	void Reset()
	{
		delete[] m_buffer;
		
		m_size = 0;
		m_totalSize = m_hint;
		
		m_buffer = new char[m_hint];
		
		m_currentOffset = 0;
	}
	
	size_t Write(const char* buffer, size_t size)
	{
		// do we need to resize the buffer?
		if (size + m_currentOffset >= m_totalSize)
		{
			increaseSize(size - (m_totalSize - m_currentOffset) > m_hint ? size - (m_totalSize - m_currentOffset) : m_hint);
		}
		
		memcpy(&m_buffer[m_currentOffset], buffer, size);
		m_currentOffset += size;
		m_size += size;
		
		return size;
	}
	
	const char* GetData() { return m_buffer; }
	size_t GetSize() { return m_size; }
	
	void SaveToFile(const std::string& filename)
	{
		std::ofstream out(filename.c_str(), std::ofstream::binary);
		
		if (out.good() == false)
			throw "Uanble to open file for writing";
		
		out.write(m_buffer, m_size);
		
		out.close();
	}

private:
	
	void increaseSize(size_t amount)
	{
		m_totalSize += amount;

		char* tmp = new char[m_totalSize];
		memset(tmp, 0, m_totalSize);
		
		memcpy(tmp, m_buffer, m_size);

		delete[] m_buffer;
		m_buffer = tmp;
	}

	char* m_buffer;
	size_t m_size;
	size_t m_totalSize;
	size_t m_hint;
	size_t m_currentOffset;
};

#endif // RBUFFER_H
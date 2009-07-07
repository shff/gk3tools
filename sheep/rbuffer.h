#ifndef RBUFFER_H
#define RBUFFER_H

#include <cassert>
#include <fstream>
#include <memory.h>

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

	size_t WriteInt(int i)
	{
		// TODO: do endian conversion on big-endian machines!
		return Write((char*)&i, 4);
	}

	size_t WriteUInt(unsigned int i)
	{
		// TODO: do endian conversion on big-endian machines!
		return Write((char*)&i, 4);
	}

	size_t WriteUIntAt(unsigned int i, size_t offset)
	{
		return WriteAt((char*)&i, 4, offset);
	}

	size_t WriteUShort(unsigned short i)
	{
		// TODO: do endian conversion on big-endian machines!
		return Write((char*)&i, 2);
	}

	void WriteFloat(float value)
	{
		// TODO: adjust for endianness!
		Write((char*)&value, 4);
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

		if (m_currentOffset > m_size)
			m_size = m_currentOffset;
		
		return size;
	}

	size_t WriteAt(const char* buffer, size_t size, size_t offset)
	{
		// don't write past where we've already written
		if (size + offset > m_currentOffset)
			return 0;

		size_t oldOffset = m_currentOffset;
		m_currentOffset = offset;

		size_t bytesWritten = Write(buffer, size);

		m_currentOffset = oldOffset;

		return bytesWritten;
	}
	
	size_t Tell() { return m_currentOffset; }
	const char* GetData() { return m_buffer; }
	size_t GetSize() { return m_size; }
	size_t GetCapacity() { return m_totalSize; }

	void Rewind()
	{
		m_currentOffset = 0;
	}

	void SeekFromStart(size_t offset)
	{
		if (offset > m_size)
			m_currentOffset = m_size;
		else
			m_currentOffset = offset;
	}

	unsigned char ReadByte()
	{
		unsigned char b;
		Read((char*)&b, 1);
		return b;
	}

	int ReadInt()
	{
		int i;
		Read((char*)&i, 4);
		return i;
	}

	float ReadFloat()
	{
		float f;
		Read((char*)&f, 4);
		return f;
	}

	size_t Read(char* buffer, size_t size)
	{
		if (m_currentOffset >= m_size) return 0;

		size_t bytesToRead = std::min(size, m_size - m_currentOffset);

		memcpy(buffer, &m_buffer[m_currentOffset], bytesToRead);

		m_currentOffset += bytesToRead;
		return bytesToRead;
	}
	
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

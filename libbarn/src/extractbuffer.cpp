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

#include <fstream>
#include "barn.h"
#include "barn_internal.h"


#ifndef DISABLE_LZO
#include <lzo1x.h>
#endif
#ifndef DISABLE_ZLIB
#define ZLIB_WINAPI 
#include <zlib.h>
#endif

namespace Barn
{
	void ExtractBuffer::WriteToFile(const std::string& filename, unsigned int startOffset)
	{
		std::ofstream output(filename.c_str(), std::ios_base::binary | std::ios_base::out);
		
		if (output.good() == false)
		{
			std::cout << "Unable to open " << filename << std::endl;
			throw BarnException("Unable to open output file", BARNERR_UNABLE_TO_OPEN_OUTPUT_FILE);
		}
		
		output.write(&m_buffer[startOffset], m_size - startOffset);
		
		output.close();
	}
	
	void ExtractBuffer::Decompress(Compression compressionType)
	{
		if (compressionType == LZO)
		{
#ifdef DISABLE_LZO
			throw BarnException("This version of LibBarn does not have support for LZO", BARNERR_UNABLE_TO_INIT_LZO);
#else
			// init liblzo
			if (lzo_init() != LZO_E_OK)
			{
				throw BarnException("Unable to initialize LZO library", BARNERR_UNABLE_TO_INIT_LZO);
			}
			
			// get the uncompressed file size
			// TODO: make this work for big-endian machines!
			lzo_uint size;
			memcpy(&size, m_buffer, 4);
			
			// create a new buffer
			char* newBuffer = new char[size];
			
			// decompress the data
			if (lzo1x_decompress((unsigned char*)&m_buffer[8], m_size, (unsigned char*)newBuffer, &size, NULL) != LZO_E_OK && false)
			{
				delete[] newBuffer;
				throw BarnException("Error while decompressing LZO-compressed data", BARNERR_DECOMPRESSION_ERROR);
			}
			
			// delete the old buffer
			delete[] m_buffer;
			m_buffer = newBuffer;
			m_size = size;
#endif
		}
		else if (compressionType == ZLib)
		{
#ifdef DISABLE_ZLIB
			throw BarnException("This version of LibBarn does not have support for ZLib", BARNERR_UNABLE_TO_INIT_ZLIB);
#else
			// TODO: make this work for big-endian machines!
			unsigned int size;
			memcpy(&size, m_buffer, 4);

			char* newBuffer = new char[size];

			uLongf s = size;
			if (uncompress((Bytef*)newBuffer, &s, (const Bytef*)&m_buffer[8], m_size) != Z_OK)
			{
				delete[] newBuffer;
				throw BarnException("Error while decompressing ZLib-compressed data", BARNERR_DECOMPRESSION_ERROR);
			}

			// delete the old buffer
			delete[] m_buffer;
			m_buffer = newBuffer;
			m_size = size;
#endif
		}
	}
}

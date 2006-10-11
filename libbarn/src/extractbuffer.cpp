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

#include <fstream>
#include "barn.h"
#include "barn_internal.h"

#include <lzo1x.h>
#include <zlib.h>

namespace Barn
{
	void ExtractBuffer::WriteToFile(const std::string& filename, unsigned int startOffset)
	{
		std::ofstream output(filename.c_str());
		
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
		std::cout << "Decompressiong..." << std::endl;
		
		if (compressionType == LZO)
		{
			// init liblzo
			if (lzo_init() != LZO_E_OK)
			{
				throw BarnException("Unable to initialize LZO library", BARNERR_UNABLE_TO_INIT_LZO);
			}
			
			// get the uncompressed file size
			// TODO: make this work for big-endian machines!
			unsigned int size;
			memcpy(&size, m_buffer, 4);
			
			std::cout << "uncompressed size: " << size << std::endl;
			
			// create a new buffer
			char* newBuffer = new char[size];
			
			// decompress the data
			lzo1x_decompress((unsigned char*)&m_buffer[8], m_size, (unsigned char*)newBuffer, &size, NULL);
			
			// delete the old buffer
			delete[] m_buffer;
			m_buffer = newBuffer;
			m_size = size;
		}
		else if (compressionType == ZLib)
		{
			// TODO: support ZLib decompression!
		}
		
		std::cout << "done decompressing" << std::endl;
	}
}

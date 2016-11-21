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
#include "lzo/minilzo.h"
#endif
#ifndef DISABLE_ZLIB
#define ZLIB_WINAPI 
#include <zlib.h>
#endif

namespace Barn
{
	void ExtractBuffer::Decompress(Compression compressionType, const char* input, unsigned int inputSize, char* output, unsigned int uncompressedSize)
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
			
			lzo_uint outputSize = uncompressedSize;

			// decompress the data
			if (lzo1x_decompress((unsigned char*)input, inputSize, (unsigned char*)output, &outputSize, NULL) != LZO_E_OK && false)
			{
				throw BarnException("Error while decompressing LZO-compressed data", BARNERR_DECOMPRESSION_ERROR);
			}
#endif
		}
		else if (compressionType == ZLib)
		{
#ifdef DISABLE_ZLIB
			throw BarnException("This version of LibBarn does not have support for ZLib", BARNERR_UNABLE_TO_INIT_ZLIB);
#else

			uLongf s = uncompressedSize;
			if (uncompress((Bytef*)output, &s, (const Bytef*)input, inputSize) != Z_OK)
			{
				throw BarnException("Error while decompressing ZLib-compressed data", BARNERR_DECOMPRESSION_ERROR);
			}
#endif
		}
	}
}

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

#ifndef BARN_H
#define BARN_H

#ifdef __cplusplus
extern "C" {
#endif

#ifdef _MSC_VER
#define DECLSPEC __declspec(dllexport)
#define BARN_CALL __cdecl
#else
#define DECLSPEC
#define BARN_CALL
#endif

#define BARN_VERSION_MAJOR 0
#define BARN_VERSION_MINOR 3
#define BARN_VERSION_REVISION 0

#define BARN_SUCCESS            0
#define BARNERR_INVALID_BARN   -1
#define BARNERR_FILE_NOT_FOUND -2
#define BARNERR_INVALID_INDEX  -3
#define BARNERR_UNABLE_TO_OPEN_CHILD_BARN -4
#define BARNERR_UNABLE_TO_OPEN_OUTPUT_FILE -5
#define BARNERR_NOT_YET_IMPLEMENTED -6
#define BARNERR_UNABLE_TO_INIT_LZO -7
#define BARNERR_UNABLE_TO_INIT_ZLIB -8
#define BARNERR_DECOMPRESSION_ERROR -9
#define BARNERR_UNKNOWN        -100

typedef void* BarnHandle;

/// Opens a barn and returns a handle
DECLSPEC BarnHandle BARN_CALL brn_OpenBarn(const char* filename);

/// Closes the barn
DECLSPEC void BARN_CALL brn_CloseBarn(BarnHandle barn);

/// Returns the number of files (including referenced barns) inside the barn
DECLSPEC unsigned int BARN_CALL brn_GetNumFilesInBarn(BarnHandle barn);

/// Fills 'buffer' with the name of the file with the given index. Returns 0
/// on success or -1 on error
DECLSPEC int BARN_CALL brn_GetFileName(BarnHandle barn, int index, char* buffer, int size);

/// Gets the index of the file with the given name. Returns BARNERR_FILE_NOT_FOUND if the file doesn't exist.
DECLSPEC int BARN_CALL brn_GetFileIndex(BarnHandle barn, const char* name);

/// Fills 'buffer' with the name of the barn that the file with the given index is inside.
/// Returns 0 on success or -1 on error
DECLSPEC int BARN_CALL brn_GetFileBarn(BarnHandle barn, int index, char* buffer, int size);

#define BARN_COMPRESSION_NONE 0
#define BARN_COMPRESSION_ZLIB 1
#define BARN_COMPRESSION_LZO 2

/// Returns the size of the file specified by its index (the order inside the .brn)
/// Returns -1 if the index is invalid
DECLSPEC int BARN_CALL brn_GetFileSize(BarnHandle barn, int index);

DECLSPEC int BARN_CALL brn_GetDecompressedFileSize(BarnHandle barn, int index);

/// Returns the compression type of the file with the specified index.
/// 0 = no compression, 1 = ZLib compression, 2 = LZO compression.
DECLSPEC int BARN_CALL brn_GetFileCompression(BarnHandle barn, int index);

/// Reads the file with the specified name into the given buffer. This does NOT decompress any data.
// bufferSize is the maximum size to read. The function returns the number of bytes read, or
/// -1 on error.
DECLSPEC int BARN_CALL brn_ReadFileRaw(BarnHandle barn, int index, char* buffer, int bufferSize);

/// Similar to brn_ReadFileRaw(), except that it decompresses before it reads.
DECLSPEC int BARN_CALL brn_ReadFileDecompress(BarnHandle barn, int index, char* buffer, int bufferSize);

/// Gets information about the library
DECLSPEC void BARN_CALL brn_GetLibInfo(char* buffer, int size);

#ifdef __cplusplus
}
#endif

#endif // BARN_H

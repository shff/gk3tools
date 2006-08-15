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

#ifndef BARN_H
#define BARN_H

#ifdef __cplusplus
extern "C" {
#endif

#define BARN_SUCCESS            0
#define BARNERR_INVALID_BARN   -1
#define BARNERR_FILE_NOT_FOUND -2
#define BARNERR_INVALID_INDEX  -3
#define BARNERR_UNABLE_TO_OPEN_CHILD_BARN -4
#define BARNERR_UNABLE_TO_OPEN_OUTPUT_FILE -5
#define BARNERR_NOT_YET_IMPLEMENTED -6
#define BARNERR_UNKNOWN        -100

typedef void* BarnHandle;

/// Opens a barn and returns a handle
BarnHandle brn_OpenBarn(const char* filename);

/// Closes the barn
void brn_CloseBarn(BarnHandle barn);

/// Returns the number of files (including referenced barns) inside the barn
unsigned int brn_GetNumFilesInBarn(BarnHandle barn);

/// Fills 'buffer' with the name of the file with the given index. Returns 0
/// on success or -1 on error
int brn_GetFileName(BarnHandle barn, unsigned int index, char* buffer, int size);

/// Fills 'buffer' with the name of the barn that the file with the given index is inside.
/// Returns 0 on success or -1 on error
int brn_GetFileBarn(BarnHandle barn, unsigned int index, char* buffer, int size);

#define BARN_COMPRESSION_NONE 0
#define BARN_COMPRESSION_ZLIB 1
#define BARN_COMPRESSION_LZO 2

/// Returns the size of the file specified by its index (the order inside the .brn)
/// Returns -1 if the index is invalid
int brn_GetFileSizeByIndex(BarnHandle barn, unsigned int index);

/// Returns the size of the file specified by its name.
/// Returns -1 if the file does not exist inside the barn
int brn_GetFileSizeByName(BarnHandle barn, const char* name);

/// Returns the compression type of the file with the specified index.
/// 0 = no compression, 1 = ZLib compression, 2 = LZO compression.
int brn_GetFileCompressionByIndex(BarnHandle barn, unsigned int index);
int brn_GetFileCompressionByName(BarnHandle barn, const char* name);

int brn_GetFileOffsetByIndex(BarnHandle barn, unsigned int index);

/// Extracts the file with the specified name. If openChildBarns = true then if the file
/// resides in a barn other than this one then the child barn is opened and the file extracted.
/// The function returns BARN_SUCCESS on success, or an error otherwise.
int brn_ExtractFile(BarnHandle barn, const char* name, bool openChildBarns);


/// Just like brn_ExtractFileByIndex(), except using an index instead of the name
int brn_ExtractFileByIndex(BarnHandle barn, unsigned int index,
	const char* outputPath, bool openChildBarns, bool decompress, bool convertBitmaps);

#ifdef __cplusplus
}
#endif

#endif // BARN_H

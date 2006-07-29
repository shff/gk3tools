#ifndef BARN_H
#define BARN_H

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

#define BARN_COMPRESSION_NONE 0
#define BARN_COMPRESSION_ZLIB 1
#define BARN_COMPRESSION_LZO 2

/// Returns the size of the file specified by its index (the order inside the .brn)
/// Returns -1 if the index is invalid
int brn_GetFileSizeByIndex(BarnHandle barn, unsigned int index);

/// Returns the size of the file specified by its name.
/// Returns -1 if the file does not exist inside the barn
int brn_GetFileSizeByName(BarnHandle barn, const char* name);

int brn_GetFileCompressionByIndex(BarnHandle barn, unsigned int index);
int brn_GetFileCompressionByName(BarnHandle barn, const char* name);

#endif // BARN_H

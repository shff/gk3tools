#ifndef SHEEPC_H
#define SHEEPC_H

#ifdef __cplusplus
extern "C"
{
#endif

#ifdef _MSC_VER
#define DECLSPEC __declspec(dllexport)
#define LIB_CALL __cdecl
#else
#define DECLSPEC
#define LIB_CALL
#endif

#define SHEEP_SUCCESS 0
#define SHEEP_ERROR -1

struct SheepCode
{
	char* code;
	unsigned int size;
};

/// Compiles the sheep script and returns a new SheepCode object.
/// The code returned inside the SheepCode object is suitable for
/// saving to a file as a compiled .shp file. Also, the SheepCode
/// object must be destroyed with shp_DestroySheep().
/// Returns NULL on error.
DECLSPEC SheepCode* LIB_CALL shp_Compile(const char* script);

/// Compiles the "snippet" of sheep. Don't try to save the returned
/// code as a compiled .shp file, because it won't work! Use this
/// function for executing small "snippets" of sheep.
/// Returns NULL on error.
DECLSPEC SheepCode* LIB_CALL shp_CompileSnippet(const char* script);

DECLSPEC void LIB_CALL shp_DestroySheep(SheepCode* sheep);

// TODO: add a way to fetch errors

#ifdef __cplusplus
}
#endif

#endif // SHEEPC_H
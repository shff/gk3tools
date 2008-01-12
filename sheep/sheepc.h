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

typedef unsigned char byte;

enum SHP_SymbolType
{
	Void = 0,
	Int = 1,
	Float = 2,
	String = 3
};

struct SHP_Symbol
{
	char* Name;
	SHP_SymbolType Type;

	int InitialIntValue;
	float InitialFloatValue;
	int InitialStringIndexValue;
};

struct SHP_StringConstant
{
	char* Name;
	char* Value;
};

struct SHP_Import
{
	char* Name;
	SHP_SymbolType ReturnType;
	int NumParameters;
	SHP_SymbolType* ParameterTypes;
};

struct SHP_Function
{
	char* Name;
	byte Reserved1;
	byte Reserved2;

	byte* Code;
	int CodeLength;
};

struct SHP_CompilerOutput
{
	char* Output;
	int LineNumber;
};

struct SHP_IntermediateOutput
{
	int NumImports;
	SHP_Import* Imports;

	int NumConstants;
	SHP_StringConstant* Constants;

	int NumSymbols;
	SHP_Symbol* Symbols;

	int NumFunctions;
	SHP_Function* Functions;

	int NumWarnings;
	SHP_CompilerOutput* Warnings;

	int NumErrors;
	SHP_CompilerOutput* Errors;
};

struct SHP_FullOutput
{
	byte* Code;
	int CodeLength;

	int NumWarnings;
	SHP_CompilerOutput* Warnings;

	int NumErrors;
	SHP_CompilerOutput* Errors;
};

struct SheepVM;

/// Compiles the sheep script and returns a new SHP_FullOutput object.
/// The code returned inside the SHP_FullOutput object is suitable for
/// saving to a file as a compiled .shp file. Also, the SheepCode
/// object must be destroyed with SHP_DestroyFullOutput().
DECLSPEC SHP_FullOutput* LIB_CALL SHP_Compile(const char* script);

/// Compiles the "snippet" of sheep. Don't try to save the returned
/// code as a compiled .shp file, because it won't work! Use this
/// function for executing small "snippets" of sheep.
DECLSPEC SHP_IntermediateOutput* LIB_CALL SHP_CompileSnippet(const char* script);

DECLSPEC void LIB_CALL SHP_DestroyFullOutput(SHP_FullOutput* sheep);
DECLSPEC void LIB_CALL SHP_DestroyIntermediateOutput(SHP_IntermediateOutput* sheep);

// TODO: add a way to fetch errors

DECLSPEC int LIB_CALL SHP_PopIntFromStack(SheepVM* vm);
DECLSPEC float LIB_CALL SHP_PopFloatFromStack(SheepVM* vm);
DECLSPEC const char* LIB_CALL SHP_PopStringFromStack(SheepVM* vm);

#ifdef __cplusplus
}
#endif

#endif // SHEEPC_H
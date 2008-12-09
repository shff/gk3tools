#ifndef SHEEPC_H
#define SHEEPC_H

#ifdef __cplusplus
extern "C"
{
#endif

#ifdef _MSC_VER
#define DECLSPEC __declspec(dllexport)
#define LIB_CALL __cdecl
#define CALLBACK __stdcall
#else
#define DECLSPEC
#define LIB_CALL
#define CALLBACK __attribute__((stdcall))
#endif

#define SHEEP_SUCCESS 0
#define SHEEP_ERROR -1

#define SHEEP_VERSION_MAJOR 0
#define SHEEP_VERSION_MINOR 1
#define SHEEP_VERSION_REVISION 0

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

/*struct SHP_Import
{
	char* Name;
	SHP_SymbolType ReturnType;
	int NumParameters;
	SHP_SymbolType* ParameterTypes;
};*/

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
	const char* Output;
	int LineNumber;
};

struct SHP_Version
{
	byte Major, Minor, Revision;
};

typedef struct {} SheepVM;
typedef struct {} SheepImportFunction;

DECLSPEC SheepVM* LIB_CALL SHP_CreateNewVM();
DECLSPEC void LIB_CALL SHP_DestroyVM(SheepVM* vm);

typedef  void (CALLBACK *SHP_MessageCallback)(int linenumber, const char* message);
DECLSPEC void LIB_CALL SHP_SetOutputCallback(SheepVM* vm, SHP_MessageCallback callback);

DECLSPEC int LIB_CALL SHP_RunSnippet(SheepVM* vm, const char* script, int* result);
DECLSPEC int LIB_CALL SHP_RunScript(SheepVM* vm, const char* script, const char* function);

typedef void (CALLBACK *SHP_ImportCallback)(SheepVM* vm);
DECLSPEC SheepImportFunction* LIB_CALL SHP_AddImport(SheepVM* vm, const char* name, SHP_SymbolType returnType, SHP_ImportCallback callback);
DECLSPEC void LIB_CALL SHP_AddImportParameter(SheepImportFunction* import, SHP_SymbolType parameterType);

DECLSPEC int LIB_CALL SHP_PopIntFromStack(SheepVM* vm);
DECLSPEC float LIB_CALL SHP_PopFloatFromStack(SheepVM* vm);
DECLSPEC const char* LIB_CALL SHP_PopStringFromStack(SheepVM* vm);

DECLSPEC void LIB_CALL SHP_PushIntOntoStack(SheepVM* vm, int i);

DECLSPEC SHP_Version SHP_GetVersion();

#ifdef __cplusplus
}
#endif

#endif // SHEEPC_H

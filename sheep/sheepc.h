#ifndef SHEEPC_H
#define SHEEPC_H

#ifdef __cplusplus
#include <cstring>
extern "C"
{
#else
#include <string.h>	
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

#include "sheepErrorCodes.h"

#define SHEEP_TRUE 1
#define SHEEP_FALSE 0

#define SHEEP_VERSION_MAJOR 0
#define SHEEP_VERSION_MINOR 3
#define SHEEP_VERSION_REVISION 9

#define SHEEP_VERBOSITY_SILENT 0
#define SHEEP_VERBOSITY_POLITE 1
#define SHEEP_VERBOSITY_ANNOYING 2
#define SHEEP_VERBOSITY_EXTREME 3

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
typedef struct {} SheepVMContext;
typedef struct {} SheepImportFunction;
typedef struct {} SheepScript;

DECLSPEC SheepVM* LIB_CALL SHP_CreateNewVM();
DECLSPEC void LIB_CALL SHP_DestroyVM(SheepVM* vm);

/// Sets the "tag" data. A tag can be a pointer whatever you want (including NULL).
/// The VM doesn't use it for anything; it's only for your convenience.
DECLSPEC void LIB_CALL SHP_SetVMTag(SheepVM* vm, void* tag);
/// Gets the "tag" data.
DECLSPEC void* LIB_CALL SHP_GetVMTag(SheepVM* vm);

typedef struct SHP_Allocator
{
	void* (CALLBACK *Allocator)(size_t);
	void (CALLBACK *Deallocator)(void*);
} SHP_Allocator;

/* Sets the allocator. This is optional, but if you want to set the allocator
then this MUST Be called before any calls to SHP_CreateNewVM(). */
DECLSPEC void LIB_CALL SHP_SetAllocator(SHP_Allocator* allocator);


typedef  void (CALLBACK *SHP_MessageCallback)(int linenumber, const char* message);
DECLSPEC void LIB_CALL SHP_SetOutputCallback(SheepVM* vm, SHP_MessageCallback callback);

DECLSPEC int LIB_CALL shp_PrepareScriptForExecution(SheepVM* vm, SheepScript* script, const char* function, SheepVMContext** context);
DECLSPEC int LIB_CALL shp_Execute(SheepVMContext* context);
DECLSPEC int LIB_CALL shp_GetNumVariables(SheepVMContext* context);
DECLSPEC int LIB_CALL shp_GetVariableName(SheepVMContext* context, int index, const char** name);
DECLSPEC int LIB_CALL shp_GetVariableI(SheepVMContext* context, int index, int* value);
DECLSPEC int LIB_CALL shp_GetVariableF(SheepVMContext* context, int index, float* value);
DECLSPEC int LIB_CALL shp_SetVariableI(SheepVMContext* context, int index, int value);
DECLSPEC int LIB_CALL shp_SetVariableF(SheepVMContext* context, int index, float value);

typedef void (CALLBACK *SHP_ImportCallback)(SheepVM* vm);
DECLSPEC void LIB_CALL SHP_SetImportCallback(SheepVM* vm, const char* name, SHP_ImportCallback callback);

DECLSPEC int LIB_CALL SHP_PopIntFromStack(SheepVM* vm, int* result);
DECLSPEC int LIB_CALL SHP_PopFloatFromStack(SheepVM* vm, float* result);
DECLSPEC int LIB_CALL SHP_PopStringFromStack(SheepVM* vm, const char** result);

DECLSPEC void LIB_CALL SHP_PushIntOntoStack(SheepVM* vm, int i);

DECLSPEC SheepVMContext* LIB_CALL SHP_GetCurrentContext(SheepVM* vm);

/* these next few functions are just for debugging the Compiler and VM. They shouldn't
be used for anything else. */
DECLSPEC int LIB_CALL SHP_GetNumContexts(SheepVM* vm);
DECLSPEC int LIB_CALL SHP_GetCurrentContextStackSize(SheepVM* vm);
DECLSPEC void LIB_CALL SHP_SetVerbosity(SheepVM* vm, int verbosity);
DECLSPEC void LIB_CALL SHP_PrintMemoryUsage();
DECLSPEC void LIB_CALL SHP_PrintStackTrace(SheepVM* vm);


/* There are two versions of Sheep, the vanilla version that shipped with GK3,
and an "enhanced" version with our own extras. By default the Sheep compiler and VM
only support GK3's vanilla version of sheep, so if you want the extras you have to explicitly
as for them, using the following function. */
DECLSPEC void LIB_CALL SHP_EnableLanguageEnhancements(SheepVM* vm, bool enabled);

/* You can read more about Waiting in Sheep Engine.doc, which is embedded in the GK3 barns.

As far as this library is concerned, there's one simple rule you should remember:

- ALL import functions return immediately

This means that no functions are allowed to block. They MUST return control to the Sheep VM
immediately. Even if they aren't finished executing. In other words, any import function
that may take a few frames to execute are actually asynchronous. The Wait mechanism
is what allows the VM to wait on functions to finish before continuing with the script.

Here's the typical usage of the wait system:

1) VM encounters a BeginWait instruction. It sets an internal IsWaiting flag.
	(this is the flag that SHP_IsInWaitSection() returns)
2) VM calls an import function. It is up to this import function to determine what to do next.
	If the function can return immediately then everything works as normal. But if
	this is an import function that may take a few frames to execute then the host
	application must check if the VM is in a wait section (using SHP_IsInWaitSection()).
	If so, the host application must somehow *remember that the action is still pending,*
	and then return.
3) The VM encounters an EndWait instruction. This raises the EndWaitCallback (which
	should be set using SHP_SetEndWaitCallback()). The host application should check
	for any pending actions, and call SHP_Suspend() if there is anything still executing.
	Later, once everything finishes, the host application can call SHP_Resume() and
	the script can continue executing where it left off.

NOTE: Though the VM itself doesn't really care, asynchronous import functions should
	never need to return a value.

** THE WAITING STUFF ISN'T WORKING YET! DON'T USE IT! THE API WILL PROBABLY CHANGE! **

*/

DECLSPEC int LIB_CALL SHP_IsInWaitSection(SheepVM* vm);
DECLSPEC int LIB_CALL SHP_Suspend(SheepVMContext* context);
typedef  void (CALLBACK *SHP_EndWaitCallback)(SheepVM* vm, SheepVMContext* context);
DECLSPEC void LIB_CALL SHP_SetEndWaitCallback(SheepVM* vm, SHP_EndWaitCallback callback);


/* To get the disassembly of a sheep file, do this:
1) load the sheep file (you have to do that part yourself)
2) send the entire file contents to SHP_GetDisassembly()
3) call SHP_GetDisassemblyLength() to get the length of the disassembly text
4) allocate a string buffer big enough to hold the disassembly
5) call SHP_GetDisassemblyText() using the allocated string buffer
6) call SHP_FreeDisassembly() to clean up the disassembly info
(since you allocated the buffer yourself you can keep using it even after
the SHP_FreeDisassembly() call) */
typedef struct {} SheepDisassembly;
DECLSPEC SheepDisassembly* SHP_GetDisassembly(const byte* data, int length);
DECLSPEC int SHP_GetDisassemblyLength(const SheepDisassembly* disassembly);
DECLSPEC void SHP_GetDisassemblyText(const SheepDisassembly* disassembly, char* buffer);
DECLSPEC void SHP_FreeDisassembly(const SheepDisassembly* disassembly);

typedef struct {} SheepCompiler;
DECLSPEC SheepCompiler* LIB_CALL shp_CreateNewCompiler(int languageVersion);
DECLSPEC void LIB_CALL shp_DestroyCompiler(SheepCompiler* compiler);
DECLSPEC int LIB_CALL shp_DefineImportFunction(SheepCompiler* compiler, const char* name, SHP_SymbolType returnType, SHP_SymbolType parameters[], int numParameters);
DECLSPEC int LIB_CALL shp_CompileScript(SheepCompiler* compiler, const char* script, SheepScript** result);
DECLSPEC int LIB_CALL shp_LoadScriptFromBytecode(const char* bytecode, int length, SheepScript** result);
DECLSPEC void LIB_CALL shp_ReleaseSheepScript(SheepScript* script);


DECLSPEC SHP_Version SHP_GetVersion();

#ifdef __cplusplus
}
#endif

#endif // SHEEPC_H

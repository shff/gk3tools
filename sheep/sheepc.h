#ifndef SHEEPC_H
#define SHEEPC_H

/// @file

#ifdef __cplusplus
#include <cstring>
extern "C"
{
#else
#include <string.h>	
#endif

#include "sheepCommon.h"

#define SHEEP_TRUE 1
#define SHEEP_FALSE 0

#define SHEEP_VERSION_MAJOR 0
#define SHEEP_VERSION_MINOR 4
#define SHEEP_VERSION_REVISION 0

#define SHEEP_VERBOSITY_SILENT 0
#define SHEEP_VERBOSITY_POLITE 1
#define SHEEP_VERBOSITY_ANNOYING 2
#define SHEEP_VERBOSITY_EXTREME 3

#define SHEEP_CONTEXT_STATE_PREPARED 0
#define SHEEP_CONTEXT_STATE_EXECUTING 1
#define SHEEP_CONTEXT_STATE_SUSPENDED 2
#define SHEEP_CONTEXT_STATE_FINISHED 3


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

/// Handle to a virtual machine object
typedef struct {} SheepVM;

/// Handle to a virtual machine context object
typedef struct {} SheepVMContext;

typedef struct {} SheepImportFunction;

/// Handle to a Sheep script object
typedef struct {} SheepScript;

/// Creates a new virtual machine
SHP_DECLSPEC SheepVM* SHP_LIB_CALL SHP_CreateNewVM(int languageVersion);

/// Destroys an existing virtual machine
SHP_DECLSPEC void SHP_LIB_CALL SHP_DestroyVM(SheepVM* vm);

/// Sets the "tag" data. A tag can be a pointer whatever you want (including NULL).
/// The VM doesn't use it for anything; it's only for your convenience.
SHP_DECLSPEC void SHP_LIB_CALL SHP_SetVMTag(SheepVM* vm, void* tag);
/// Gets the "tag" data.
SHP_DECLSPEC void* SHP_LIB_CALL SHP_GetVMTag(SheepVM* vm);

typedef struct SHP_Allocator
{
	void* (SHP_CALLBACK *Allocator)(size_t);
	void (SHP_CALLBACK *Deallocator)(void*);
} SHP_Allocator;

/* Sets the allocator. This is optional, but if you want to set the allocator
then this MUST Be called before any calls to SHP_CreateNewVM(). */
SHP_DECLSPEC void SHP_LIB_CALL SHP_SetAllocator(SHP_Allocator* allocator);


typedef  void (SHP_CALLBACK *SHP_MessageCallback)(int linenumber, const char* message);
SHP_DECLSPEC void SHP_LIB_CALL SHP_SetOutputCallback(SheepVM* vm, SHP_MessageCallback callback);

SHP_DECLSPEC int SHP_LIB_CALL shp_PrepareScriptForExecution(SheepVM* vm, SheepScript* script, const char* function, SheepVMContext** context);
SHP_DECLSPEC int SHP_LIB_CALL shp_PrepareScriptForExecutionWithParent(SheepVM* vm, SheepScript* script, const char* function, SheepVMContext* parent, SheepVMContext** context);
SHP_DECLSPEC int SHP_LIB_CALL shp_Execute(SheepVMContext* context);
SHP_DECLSPEC void SHP_LIB_CALL shp_ReleaseVMContext(SheepVMContext* context);
SHP_DECLSPEC int SHP_LIB_CALL shp_GetNumVariables(SheepVMContext* context);
SHP_DECLSPEC int SHP_LIB_CALL shp_GetVMContextState(SheepVMContext* context);
SHP_DECLSPEC int SHP_LIB_CALL shp_GetVariableName(SheepVMContext* context, int index, const char** name);
SHP_DECLSPEC int SHP_LIB_CALL shp_GetVariableI(SheepVMContext* context, int index, int* value);
SHP_DECLSPEC int SHP_LIB_CALL shp_GetVariableF(SheepVMContext* context, int index, float* value);
SHP_DECLSPEC int SHP_LIB_CALL shp_SetVariableI(SheepVMContext* context, int index, int value);
SHP_DECLSPEC int SHP_LIB_CALL shp_SetVariableF(SheepVMContext* context, int index, float value);

typedef void (SHP_CALLBACK *SHP_ImportCallback)(SheepVM* vm);
SHP_DECLSPEC void SHP_LIB_CALL SHP_SetImportCallback(SheepVM* vm, const char* name, SHP_ImportCallback callback);

SHP_DECLSPEC int SHP_LIB_CALL SHP_PopIntFromStack(SheepVMContext* vm, int* result);
SHP_DECLSPEC int SHP_LIB_CALL SHP_PopFloatFromStack(SheepVMContext* vm, float* result);
SHP_DECLSPEC int SHP_LIB_CALL SHP_PopStringFromStack(SheepVMContext* vm, const char** result);

SHP_DECLSPEC int SHP_LIB_CALL SHP_PushIntOntoStack(SheepVMContext* vm, int i);

/* these next few functions are just for debugging the Compiler and VM. They shouldn't
be used for anything else. */
SHP_DECLSPEC int SHP_LIB_CALL SHP_GetNumContexts(SheepVM* vm);
SHP_DECLSPEC void SHP_LIB_CALL SHP_SetVerbosity(SheepVM* vm, int verbosity);
SHP_DECLSPEC void SHP_LIB_CALL SHP_PrintMemoryUsage();
SHP_DECLSPEC void SHP_LIB_CALL SHP_PrintStackTrace(SheepVM* vm);

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

SHP_DECLSPEC int SHP_LIB_CALL SHP_IsInWaitSection(SheepVMContext* context);
SHP_DECLSPEC int SHP_LIB_CALL SHP_Suspend(SheepVMContext* context);
typedef  void (SHP_CALLBACK *SHP_EndWaitCallback)(SheepVM* vm, SheepVMContext* context);
SHP_DECLSPEC void SHP_LIB_CALL SHP_SetEndWaitCallback(SheepVM* vm, SHP_EndWaitCallback callback);


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
SHP_DECLSPEC SheepDisassembly* SHP_GetDisassembly(SheepScript* script);
SHP_DECLSPEC const char* SHP_GetDisassemblyText(const SheepDisassembly* disassembly);
SHP_DECLSPEC void SHP_FreeDisassembly(const SheepDisassembly* disassembly);

typedef struct {} SheepCompiler;
SHP_DECLSPEC SheepCompiler* SHP_LIB_CALL shp_CreateNewCompiler(int languageVersion);
SHP_DECLSPEC void SHP_LIB_CALL shp_DestroyCompiler(SheepCompiler* compiler);
SHP_DECLSPEC int SHP_LIB_CALL shp_DefineImportFunction(SheepCompiler* compiler, const char* name, SHP_SymbolType returnType, SHP_SymbolType parameters[], int numParameters);
SHP_DECLSPEC int SHP_LIB_CALL shp_CompileScript(SheepCompiler* compiler, const char* script, SheepScript** result);
SHP_DECLSPEC int SHP_LIB_CALL shp_LoadScriptFromBytecode(const char* bytecode, int length, SheepScript** result);
SHP_DECLSPEC void SHP_LIB_CALL shp_ReleaseSheepScript(SheepScript* script);


#ifdef __cplusplus
}
#endif

#endif // SHEEPC_H

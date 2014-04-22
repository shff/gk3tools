#include <iostream>
#include "sheepc.h"
#include "sheepCodeTree.h"
#include "sheepCodeGenerator.h"
#include "sheepImportTable.h"
#include "sheepCodeBuffer.h"
#include "sheepMachine.h"
#include "sheepFileReader.h"
#include "sheepFileWriter.h"
#include "sheepDisassembler.h"
#include "Internal/script.h"
#include "Internal/compiler.h"

#define SM(v) ((SheepMachine*)(v))

SHP_Allocator g_allocator;


SheepVM* SHP_CreateNewVM()
{
	return (SheepVM*)(SHEEP_NEW SheepMachine());
}

void SHP_DestroyVM(SheepVM* vm)
{
	assert(vm != NULL);
	
	SHEEP_DELETE(SM(vm));
}

void SHP_SetVMTag(SheepVM* vm, void* tag)
{
	assert(vm != NULL);

	SM(vm)->SetTag(tag);
}

void* SHP_GetVMTag(SheepVM* vm)
{
	assert(vm != NULL);

	return SM(vm)->GetTag();
}

void SHP_SetOutputCallback(SheepVM* vm, SHP_MessageCallback callback)
{
	assert(vm != NULL);

	SM(vm)->SetCompileOutputCallback(callback);
}

void SHP_SetImportCallback(SheepVM* vm, const char* name, SHP_ImportCallback callback)
{
	SM(vm)->SetImportCallback(name, (Sheep::ImportCallback)callback);
}

int shp_PrepareScriptForExecution(SheepVM* vm, SheepScript* script, const char* function, SheepVMContext** context)
{
	if (vm == nullptr || script == nullptr || function == nullptr)
		return SHEEP_ERR_INVALID_ARGUMENT;

	return SM(vm)->PrepareScriptForExecution((Sheep::IScript*)script, function, (Sheep::IExecutionContext**)context);
}

int shp_GetNumVariables(SheepVMContext* context)
{
	if (context == nullptr)
		return 0;

	return ((Sheep::IExecutionContext*)context)->GetNumVariables();
}

int shp_GetVariableName(SheepVMContext* context, int index, const char** name)
{
	if (context == nullptr || name == nullptr)
		return SHEEP_ERR_INVALID_ARGUMENT;

	*name = ((Sheep::IExecutionContext*)context)->GetVariableName(index);

	return SHEEP_SUCCESS;
}

int shp_GetVariableI(SheepVMContext* context, int index, int* value)
{
	if (context == nullptr || value == nullptr)
		return SHEEP_ERR_INVALID_ARGUMENT;

	return ((SheepContext*)context)->GetVariableInt(index, value);
}

int shp_GetVariableF(SheepVMContext* context, int index, float* value)
{
	if (context == nullptr || value == nullptr)
		return SHEEP_ERR_INVALID_ARGUMENT;

	return ((Sheep::IExecutionContext*)context)->GetVariableFloat(index, value);
}

int shp_SetVariableI(SheepVMContext* context, int index, int value)
{
	if (context == nullptr)
		return SHEEP_ERR_INVALID_ARGUMENT;

	return ((Sheep::IExecutionContext*)context)->SetVariableInt(index, value);
}

int shp_SetVariableF(SheepVMContext* context, int index, float value)
{
	if (context == nullptr)
		return SHEEP_ERR_INVALID_ARGUMENT;

	return ((Sheep::IExecutionContext*)context)->SetVariableFloat(index, value);
}

int SHP_PopIntFromStack(SheepVM* vm, int* result)
{
	if (vm == nullptr)
		return SHEEP_ERR_INVALID_ARGUMENT;

	return SM(vm)->PopIntFromStack(result);
}

int SHP_PopFloatFromStack(SheepVM* vm, float* result)
{
	if (vm == nullptr)
		return SHEEP_ERR_INVALID_ARGUMENT;

	return SM(vm)->PopFloatFromStack(result);
}

int SHP_PopStringFromStack(SheepVM* vm, const char** result)
{
	if (vm == nullptr)
		return SHEEP_ERR_INVALID_ARGUMENT;

	return SM(vm)->PopStringFromStack(result);
}


void SHP_PushIntOntoStack(SheepVM* vm, int i)
{
	assert(vm != NULL);

	SM(vm)->PushIntOntoStack(i);
}

int SHP_IsInWaitSection(SheepVM* vm)
{
	if (SM(vm)->IsInWaitSection())
		return SHEEP_TRUE;

	return SHEEP_FALSE;
}

int SHP_Suspend(SheepVMContext* context)
{
	if (context == nullptr)
		return SHEEP_ERR_INVALID_ARGUMENT;

	return ((SheepContext*)context)->Suspend();
}

int shp_Execute(SheepVMContext* context)
{
	if (context == nullptr)
		return SHEEP_ERR_INVALID_ARGUMENT;

	return ((SheepContext*)context)->Execute();
}

void SHP_SetEndWaitCallback(SheepVM* vm, SHP_EndWaitCallback callback)
{
	assert(vm != NULL);

	SM(vm)->SetEndWaitCallback((Sheep::EndWaitCallback)callback);
}

SheepVMContext* SHP_GetCurrentContext(SheepVM* vm)
{
	return (SheepVMContext*)SM(vm)->GetCurrentContext();
}

SHP_Version SHP_GetVersion()
{
	SHP_Version v;
	v.Major = SHEEP_VERSION_MAJOR;
	v.Minor = SHEEP_VERSION_MINOR;
	v.Revision = SHEEP_VERSION_REVISION;

	return v;
}


void SHP_PrintStackTrace(SheepVM* vm)
{
	assert(vm != NULL);

	return SM(vm)->PrintStackTrace();
}

void SHP_EnableLanguageEnhancements(SheepVM* vm, bool enabled)
{
	assert(vm != NULL);

	SM(vm)->SetLanguageEnhancementsEnabled(enabled);
}

int SHP_GetNumContexts(SheepVM* vm)
{
	assert(vm != NULL);

	return SM(vm)->GetNumContexts();
}

int SHP_GetCurrentContextStackSize(SheepVM* vm)
{
	assert(vm != NULL);

	return SM(vm)->GetCurrentContextStackSize();
}

void SHP_SetVerbosity(SheepVM* vm, int verbosity)
{
	assert(vm != NULL);

	return SM(vm)->SetVerbosity((SheepMachine::Verbosity)verbosity);
}

struct Disassembly : SheepDisassembly
{
    std::string Text;
};

SheepDisassembly* SHP_GetDisassembly(const byte* data, int length)
{
    std::string disassembly = Sheep::Disassembler::GetDisassembly(data, length);

    Disassembly* d = SHEEP_NEW(Disassembly);
    d->Text = disassembly;

    return d;
}

int SHP_GetDisassemblyLength(const SheepDisassembly* disassembly)
{
    return static_cast<const Disassembly*>(disassembly)->Text.length();
}

void SHP_GetDisassemblyText(const SheepDisassembly* disassembly, char* buffer)
{
    strcpy(buffer, static_cast<const Disassembly*>(disassembly)->Text.c_str());
}

void SHP_FreeDisassembly(const SheepDisassembly* disassembly)
{
    SHEEP_DELETE(disassembly);
}



SheepCompiler* shp_CreateNewCompiler(int languageVersion)
{
	return (SheepCompiler*)CreateSheepCompiler((Sheep::SheepLanguageVersion)languageVersion);
}

void shp_DestroyCompiler(SheepCompiler* compiler)
{
	((Sheep::ICompiler*)(compiler))->Release();
}

int shp_DefineImportFunction(SheepCompiler* compiler, const char* name, SHP_SymbolType returnType, SHP_SymbolType parameters[], int numParameters)
{
	return ((Sheep::ICompiler*)(compiler))->DefineImportFunction(name, (Sheep::SymbolType)returnType, (Sheep::SymbolType*)parameters, numParameters);
}

int shp_CompileScript(SheepCompiler* compiler, const char* script, SheepScript** result)
{
	Sheep::IScript* s = ((Sheep::ICompiler*)(compiler))->CompileScript(script);

	if (s->GetStatus() == Sheep::ScriptStatus::Success)
	{
		*result = (SheepScript*)s;
		return SHEEP_SUCCESS;
	}

	s->Release();
	return SHEEP_ERROR;
}

int shp_LoadScriptFromBytecode(const char* bytecode, int length, SheepScript** result)
{
	return CreateScriptFromBytecode(bytecode, length, (Sheep::IScript**)result);
}

void shp_ReleaseSheepScript(SheepScript* script)
{
	((Sheep::IScript*)(script))->Release();
}

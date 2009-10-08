#include "sheepc.h"
#include "sheepCodeTree.h"
#include "sheepCodeGenerator.h"
#include "sheepImportTable.h"
#include "sheepCodeBuffer.h"
#include "sheepMachine.h"
#include "sheepFileReader.h"

#define SM(v) static_cast<SheepMachine*>(v)

SHP_Allocator g_allocator;


SheepVM* SHP_CreateNewVM()
{
	return SHEEP_NEW SheepMachine();
}

void SHP_DestroyVM(SheepVM* vm)
{
	assert(vm != NULL);
	
	SHEEP_DELETE(SM(vm));
}

void SHP_SetOutputCallback(SheepVM* vm, SHP_MessageCallback callback)
{
	assert(vm != NULL);

	SM(vm)->SetCompileOutputCallback(callback);
}

SheepImportFunction* SHP_AddImport(SheepVM* vm, const char* name, SHP_SymbolType returnType, SHP_ImportCallback callback)
{
	return SM(vm)->GetImports().NewImport(name, (SheepSymbolType)returnType, callback);
}

void SHP_AddImportParameter(SheepImportFunction* import, SHP_SymbolType parameterType)
{
	assert(import != NULL);
	
	static_cast<SheepImport*>(import)->Parameters.push_back((SheepSymbolType)parameterType);
}

int SHP_RunScript(SheepVM* vm, const char* script, const char* function)
{
	assert(vm != NULL);
	
	try
	{
		IntermediateOutput* output = SM(vm)->Compile(script);
		if (output->Errors.empty())
		{
			SM(vm)->Run(output, function);

			return SHEEP_SUCCESS;
		}

		// delete the output (there's no need to delete it if it runs since
		// the VM will take care of it for us)
		SHEEP_DELETE(output);

		return SHEEP_GENERIC_COMPILER_ERROR;
	}
	catch(SheepException& ex)
	{
		return ex.GetErrorNum();
	}
}

int SHP_RunCode(SheepVM* vm, const byte* code, int length, const char* function)
{
	assert(vm != NULL);

	try
	{
		SheepFileReader* reader = SHEEP_NEW SheepFileReader(code, length);
		reader->WireImportCallbacks(SM(vm)->GetImports());
		IntermediateOutput* output = reader->GetIntermediateOutput();
		SM(vm)->Run(output, function);
		
		SHEEP_DELETE(reader);

		return SHEEP_SUCCESS;
	}
	catch(SheepException& ex)
	{
		return ex.GetErrorNum();
	}
}

int SHP_RunSnippet(SheepVM* vm, const char* script, int* result)
{
	assert(vm != NULL);

	try
	{
		return SM(vm)->RunSnippet(script, result);
	}
	catch(SheepException& ex)
	{
		return ex.GetErrorNum();
	}
}

int SHP_PopIntFromStack(SheepVM* vm)
{
	assert(vm != NULL);

	return SM(vm)->PopIntFromStack();
}

float SHP_PopFloatFromStack(SheepVM* vm)
{
	assert(vm != NULL);

	return SM(vm)->PopFloatFromStack();
}

const char* SHP_PopStringFromStack(SheepVM* vm)
{
	assert(vm != NULL);

	return SM(vm)->PopStringFromStack().c_str();
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

int SHP_Suspend(SheepVM* vm)
{
	assert(vm != NULL);

	return SM(vm)->Suspend();
}

int SHP_Resume(SheepVM* vm)
{
	assert(vm != NULL);

	return SM(vm)->Resume();
}

void SHP_SetEndWaitCallback(SheepVM* vm, SHP_EndWaitCallback callback)
{
	assert(vm != NULL);

	return SM(vm)->SetEndWaitCallback(callback);
}

SHP_Version SHP_GetVersion()
{
	SHP_Version v;
	v.Major = SHEEP_VERSION_MAJOR;
	v.Minor = SHEEP_VERSION_MINOR;
	v.Revision = SHEEP_VERSION_REVISION;

	return v;
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

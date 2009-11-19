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
		if (SM(vm)->GetVerbosity() >= SheepMachine::Verbosity_Polite)
			printf("%s\n", ex.GetMessage().c_str());

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

int SHP_PopIntFromStack(SheepVM* vm, int* result)
{
	assert(vm != NULL);

	try
	{
		if (result)
			*result = SM(vm)->PopIntFromStack();
		else
			SM(vm)->PopIntFromStack();

		return SHEEP_SUCCESS;
	}
	catch(SheepException& ex)
	{
		return ex.GetErrorNum();
	}
}

int SHP_PopFloatFromStack(SheepVM* vm, float* result)
{
	assert(vm != NULL);

	try
	{
		if (result)
			*result = SM(vm)->PopFloatFromStack();
		else
			SM(vm)->PopFloatFromStack();

		return SHEEP_SUCCESS;
	}
	catch(SheepException& ex)
	{
		return ex.GetErrorNum();
	}
}

int SHP_PopStringFromStack(SheepVM* vm, const char** result)
{
	assert(vm != NULL);

	try
	{
		if (result)
			*result = SM(vm)->PopStringFromStack().c_str();
		else
			SM(vm)->PopStringFromStack();

		return SHEEP_SUCCESS;
	}
	catch(SheepException& ex)
	{
		return ex.GetErrorNum();
	}
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

SheepVMContext* SHP_Suspend(SheepVM* vm)
{
	assert(vm != NULL);

	try
	{
		return (SheepVMContext*)SM(vm)->Suspend();
	}
	catch(SheepException& ex)
	{
		return NULL;
	}
}

int SHP_Resume(SheepVM* vm, SheepVMContext* context)
{
	assert(vm != NULL);

	try
	{
		return SM(vm)->Resume((SheepContext*)context);
	}
	catch(SheepException& ex)
	{
		return ex.GetErrorNum();
	}
}

void SHP_SetEndWaitCallback(SheepVM* vm, SHP_EndWaitCallback callback)
{
	assert(vm != NULL);

	return SM(vm)->SetEndWaitCallback(callback);
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

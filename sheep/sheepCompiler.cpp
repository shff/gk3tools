#include "sheepc.h"
#include "sheepCodeTree.h"
#include "sheepCodeGenerator.h"
#include "sheepImportTable.h"
#include "sheepCodeBuffer.h"
#include "sheepMachine.h"

#define SM(v) static_cast<SheepMachine*>(v)

SheepVM* SHP_CreateNewVM()
{
	return new SheepMachine();
}

void SHP_DestroyVM(SheepVM* vm)
{
	assert(vm != NULL);
	
	delete SM(vm);
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
		SM(vm)->Prepare(script);
		SM(vm)->Run(function);
		
		return SHEEP_SUCCESS;
	}
	catch(NoSuchFunctionException& ex)
	{
		return SHEEP_ERR_NO_SUCH_FUNCTION;
	}
	catch(SheepException& ex)
	{
		return SHEEP_ERROR;
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
		return SHEEP_ERROR;
	}
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

SHP_Version SHP_GetVersion()
{
	SHP_Version v;
	v.Major = SHEEP_VERSION_MAJOR;
	v.Minor = SHEEP_VERSION_MINOR;
	v.Revision = SHEEP_VERSION_REVISION;

	return v;
}

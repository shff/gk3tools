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

SheepImportFunction* SHP_AddImport(SheepVM* vm, const char* name, SHP_SymbolType returnType, void (*callback)(SheepVM*))
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
	catch(SheepException& ex)
	{
		return SHEEP_ERROR;
	}
}

int SHP_RunSnippet(SheepVM* vm, const char* script)
{
	assert(vm != NULL);

	try
	{
		return SM(vm)->RunSnippet(script);
	}
	catch(SheepException& ex)
	{
		return 0;
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

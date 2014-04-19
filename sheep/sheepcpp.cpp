#include "sheepcpp.h"
#include "sheepFileReader.h"
#include "sheepException.h"
#include "Internal/compiler.h"
#include "Internal/virtualmachine.h"
#include "Internal/script.h"

extern "C" Sheep::ICompiler* SHP_APIENTRY CreateSheepCompiler(Sheep::SheepLanguageVersion version)
{
	return SHEEP_NEW Sheep::Internal::Compiler(version);
}

Sheep::IVirtualMachine* SHP_APIENTRY CreateSheepVirtualMachine()
{
	return SHEEP_NEW Sheep::Internal::VirtualMachine();
}

int SHP_APIENTRY CreateScriptFromBytecode(const char* bytecode, int length, Sheep::IScript** result)
{
	if (bytecode == nullptr || result == nullptr)
		return SHEEP_ERR_INVALID_ARGUMENT;

	try
	{
		SheepFileReader reader((byte*)bytecode, length);
		*result = SHEEP_NEW Sheep::Internal::Script(reader.GetIntermediateOutput());
		static_cast<Sheep::Internal::Script*>(*result)->SetStatus(Sheep::ScriptStatus::Success);

		return SHEEP_SUCCESS;
	}
	catch(SheepException& ex)
	{
		return ex.GetErrorNum();
	}
}
#include "sheepcpp.h"
#include "Internal/compiler.h"
#include "Internal/virtualmachine.h"

extern "C" Sheep::ICompiler* SHP_APIENTRY CreateSheepCompiler(Sheep::SheepLanguageVersion version)
{
	return new Sheep::Internal::Compiler(version);
}

Sheep::IVirtualMachine* SHP_APIENTRY CreateSheepVirtualMachine()
{
	return new Sheep::Internal::VirtualMachine();
}
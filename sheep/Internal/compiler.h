#ifndef SHEEP_INTERNAL_COMPILER_H
#define SHEEP_INTERNAL_COMPILER_H

#include "../sheepcpp.h"
#include "../sheepStringDictionary.h"
#include "../sheepImportTable.h"

namespace Sheep
{
namespace Internal
{
	class Compiler : public ICompiler
	{
		static const int MAX_PARAMETERS = 128;

		struct ImportFunctionDefinition
		{
			SymbolType ReturnType;
			SymbolType Parameters[MAX_PARAMETERS];
			int NumParameters;
		};

		int m_refCount;
		SheepLanguageVersion m_version;
		SheepImportTable m_imports;

	public:
		Compiler(SheepLanguageVersion version);
		virtual ~Compiler();

		void Release() override;

		int DefineImportFunction(const char* name, SymbolType returnType, SymbolType parameters[], int numParameters) override;

		IScript* CompileScript(const char* script) override;
	};
}
}

#endif // SHEEP_INTERNAL_COMPILER_H
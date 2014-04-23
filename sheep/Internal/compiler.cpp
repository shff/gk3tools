#include "compiler.h"
#include "script.h"
#include "../sheepCommon.h"
#include "../sheepCodeGenerator.h"
#include "../sheepLog.h"

namespace Sheep
{
namespace Internal
{
	Compiler::Compiler(SheepLanguageVersion version)
	{
		m_refCount = 0;
		m_version = version;

		// define the built-in "call" function
		SymbolType params[] = { SymbolType::String };
		DefineImportFunction("call", SymbolType::Void, params, 1);
	}

	Compiler::~Compiler()
	{
	}

	void Compiler::Release()
	{
		m_refCount--;

		if (m_refCount <= 0)
			delete this;
	}

	int Compiler::DefineImportFunction(const char* name, SymbolType returnType, SymbolType parameters[], int numParameters)
	{
		if (numParameters > MAX_PARAMETERS || numParameters < 0)
			return SHEEP_ERR_INVALID_ARGUMENT;
		if (name == nullptr)
			return SHEEP_ERR_INVALID_ARGUMENT;
		
		SheepImport* import = m_imports.NewImport(name, (SheepSymbolType)returnType);
		if (import == nullptr)
		{
			// this import function is already defined
			return SHEEP_ERR_INVALID_ARGUMENT;
		}

		for (int i = 0; i < numParameters; i++)
		{
			import->Parameters.push_back((SheepSymbolType)parameters[i]);
		}

		return SHEEP_SUCCESS;
	}

	IScript* Compiler::CompileScript(const char* script)
	{
		SheepCodeTree tree;
		SheepLog log;
		tree.Lock(script, &log);

		SheepCodeGenerator generator(&tree, &m_imports, m_version);
		IntermediateOutput* output = generator.BuildIntermediateOutput();

		tree.Unlock();

		Script* result = new Script(output);
		result->SetStatus(ScriptStatus::Success);

		auto& entries = log.GetEntries();
		for (unsigned int i = 0; i < entries.size(); i++)
		{
			if (entries[i].Type == LOG_ERROR)
				result->SetStatus(ScriptStatus::Error);

			result->AddMessage(entries[i].LineNumber, entries[i].Text.c_str());
		}

		if (output->Errors.empty() == false)
		{
			result->SetStatus(ScriptStatus::Error);

			for (unsigned int i = 0; i < output->Errors.size(); i++)
				result->AddMessage(output->Errors[i].LineNumber, output->Errors[i].Output.c_str());
		}

		return result;
	}
}
}
#include "script.h"
#include "CompiledScriptOutput.h"
#include "../sheepCodeGenerator.h"
#include "../sheepDisassembler.h"
#include "../sheepFileWriter.h"

namespace Sheep
{
namespace Internal
{
	Script::Script(IntermediateOutput* output)
	{
		m_refCount = 0;
		m_output = output;
		m_status = ScriptStatus::Invalid;
	}

	Script::~Script()
	{
		delete m_output;
	}

	void Script::Release()
	{
		if (m_refCount <= 0)
			delete this;
	}

	SheepLanguageVersion Script::GetLanguageVersion()
	{
		return m_output->GetLanguageVersion();
	}

	ScriptStatus Script::GetStatus()
	{
		return m_status;
	}

	void Script::SetStatus(ScriptStatus status)
	{
		m_status = status;
	}

	int Script::GetNumMessages()
	{
		return (int)m_messages.size();
	}

	const char* Script::GetMessage(int index)
	{
		if (index < 0 || index >= m_messages.size())
			return nullptr;

		return m_messages[index].MessageText.c_str();
	}

	int Script::GetMessageLineNumber(int index)
	{
		if (index < 0 || index >= m_messages.size())
			return 0;

		return m_messages[index].LineNumber;
	}

	void Script::AddMessage(int lineNumber, const char* message)
	{
		Message m;
		m.LineNumber = lineNumber;
		m.MessageText = message;
		m_messages.push_back(m);
	}

	IDisassembly* Script::GenerateDisassembly()
	{
		if (m_status != ScriptStatus::Success || m_output == nullptr)
			return nullptr;

		return Disassembler::GetDisassembly(m_output);
	}

	ICompiledScriptOutput* Script::GenerateCompiledOutput()
	{
		if (m_status != ScriptStatus::Success || m_output == nullptr)
			return nullptr;

		SheepFileWriter writer(m_output);
		ResizableBuffer* buffer = writer.GetBuffer();

		return new CompiledScriptOutput(buffer);
	}
}
}

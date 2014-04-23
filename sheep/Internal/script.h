#ifndef SHEEP_INTERNAL_SCRIPT_H
#define SHEEP_INTERNAL_SCRIPT_H

#include <vector>
#include <string>
#include "../sheepcpp.h"

class IntermediateOutput;

namespace Sheep
{
namespace Internal
{
	class Script : public IScript
	{
		int m_refCount;
		ScriptStatus m_status;
		IntermediateOutput* m_output;

		struct Message
		{
			int LineNumber;
			std::string MessageText;
		};

		std::vector<Message> m_messages;

	public:
		Script(IntermediateOutput* script);
		virtual ~Script();

		void Release() override;

		Sheep::SheepLanguageVersion GetLanguageVersion() override;

		ScriptStatus GetStatus() override;
		void SetStatus(ScriptStatus status);

		int GetNumMessages() override;
		const char* GetMessage(int index) override;
		int GetMessageLineNumber(int index) override;

		void AddMessage(int lineNumber, const char* message);

		IntermediateOutput* GetIntermediateOutput() { return m_output; }

		IDisassembly* GenerateDisassembly() override;

		ICompiledScriptOutput* GenerateCompiledOutput() override;
	};
}
}

#endif // SHEEP_INTERNAL_SCRIPT_H

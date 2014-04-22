#ifndef SHEEP_INTERNAL_COMPILEDSCRIPTOUTPUT_H
#define SHEEP_INTERNAL_COMPILEDSCRIPTOUTPUT_H

#include "../sheepcpp.h"
#include "../rbuffer.h"

namespace Sheep
{
namespace Internal
{
	class CompiledScriptOutput : public ICompiledScriptOutput
	{
		ResizableBuffer* m_output;

		virtual ~CompiledScriptOutput() 
		{
			if (m_output != nullptr)
				delete m_output;
		}

	public:

		CompiledScriptOutput(ResizableBuffer* output)
		{
			m_output = output;
		}

		void Release() override
		{
			// don't other with reference counting
			delete this;
		}

		int GetSize() override
		{
			if (m_output != nullptr)
				return (int)m_output->GetSize();

			return 0;
		}

		const char* GetData() override
		{
			if (m_output != nullptr)
				return m_output->GetData();

			return nullptr;
		}
	};
}
}

#endif // SHEEP_INTERNAL_COMPILEDSCRIPTOUTPUT_H

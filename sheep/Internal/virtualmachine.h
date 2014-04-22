#ifndef SHEEP_INTERNAL_VIRTUALMACHINE_H
#define SHEEP_INTERNAL_VIRTUALMACHINE_H

#include "../sheepcpp.h"

namespace Sheep
{
namespace Internal
{
	class VirtualMachine : public IVirtualMachine
	{
		int m_refCount;
		void* m_tag;

	public:
		VirtualMachine();
		virtual ~VirtualMachine();

		void Release() override;

		void SetTag(void* tag) override { m_tag = tag; }
		void* GetTag() override { return m_tag; }

		int SetEndWaitCallback(EndWaitCallback callback) override;
		int SetImportCallback(const char* importName, ImportCallback callback) override;

		int PrepareScriptForExecution(IScript* script, const char* function, IExecutionContext** context) override;

		int PopIntFromStack(int* result) override;
		int PopFloatFromStack(float* result) override;
		int PopStringFromStack(const char** result) override;
		int PushIntOntoStack(int value) override;
		int PushFloatOntoStack(float value) override;
	};
}
}

#endif // SHEEP_INTERNAL_VIRTUALMACHINE_H

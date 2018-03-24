#include "virtualmachine.h"
#include "../sheepMachine.h"

namespace Sheep
{
namespace Internal
{
	VirtualMachine::VirtualMachine()
	{
		m_refCount = 0;
		m_tag = nullptr;
	}

	VirtualMachine::~VirtualMachine()
	{
	}

	void VirtualMachine::Release()
	{
		m_refCount--;

		if (m_refCount <= 0)
			delete this;
	}

	int VirtualMachine::SetEndWaitCallback(EndWaitCallback callback)
	{
		// TODO
		return SHEEP_ERROR;
	}

	int VirtualMachine::SetImportCallback(const char* importName, ImportCallback callback)
	{
		// TODO
		return SHEEP_ERROR;
	}

	int VirtualMachine::PrepareScriptForExecution(IScript* script, const char* function, IExecutionContext** context)
	{
		// TODO
		return SHEEP_ERROR;
	}
}
}

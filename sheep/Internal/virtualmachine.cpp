#include "virtualmachine.h"
#include "..\sheepMachine.h"

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

	int VirtualMachine::SetImportCallback(const char* importName, ImportCallback callback)
	{
		return 0;
	}

	int VirtualMachine::ExecuteScript(IScript* script)
	{
		return 0;
	}

	int VirtualMachine::PopIntFromStack(int* result)
	{
		return 0;
	}

	int VirtualMachine::PopFloatFromStack(float* result)
	{
		return 0;
	}

	int VirtualMachine::PopStringFromStack(const char** result)
	{
		return 0;
	}

	int VirtualMachine::PushIntOntoStack(int value)
	{
		return 0;
	}

	int VirtualMachine::PushFloatOntoStack(float value)
	{
		return 0;
	}
}
}
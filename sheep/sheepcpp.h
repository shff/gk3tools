#ifndef SHEEP_SHEEPCPP_H
#define SHEEP_SHEEPCPP_H

#ifdef _MSC_VER
#define SHP_DECLSPEC __declspec(dllexport)
#define SHP_LIB_CALL __cdecl
#define SHP_CALLBACK __stdcall
#define SHP_APIENTRY __stdcall
#else
#define SHP_DECLSPEC
#define SHP_LIB_CALL
#define SHP_CALLBACK __attribute__((stdcall))
#define SHP_APIENTRY __attribute__((stdcall))
#endif

#include "sheepErrorCodes.h"

namespace Sheep
{
	enum class SymbolType
	{
		Void = 0,
		Int = 1,
		Float = 2,
		String = 3
	};

	class IScript;
	class IVirtualMachine;

	class IExecutionContext
	{
	public:
		virtual void Release() = 0;

		virtual int GetNumVariables() = 0;
		virtual const char* GetVariableName(int index) = 0;
		virtual SymbolType GetVariableType(int index) = 0;
		virtual int SetVariableInt(int index, int value) = 0;
		virtual int SetVariableFloat(int index, float value) = 0;
		virtual int SetVariableString(int index, const char* value) = 0;
		virtual int GetVariableInt(int index, int* result) = 0;
		virtual int GetVariableFloat(int index, float* result) = 0;
		virtual int GetVariableString(int index, const char** result) = 0;
	};

	typedef void (SHP_CALLBACK *ImportCallback)(IVirtualMachine* vm);

	class IVirtualMachine
	{
	public:
		virtual void Release() = 0;

		virtual void SetTag(void* tag) = 0;
		virtual void* GetTag() = 0;

		virtual int SetImportCallback(const char* importName, ImportCallback callback) = 0;

		virtual int PrepareScriptForExecution(IScript* script, const char* function, IExecutionContext** context) = 0;
		virtual int Execute(IExecutionContext* context) = 0;

		virtual int PopIntFromStack(int* result) = 0;
		virtual int PopFloatFromStack(float* result) = 0;
		virtual int PopStringFromStack(const char** result) = 0;
		virtual int PushIntOntoStack(int value) = 0;
		virtual int PushFloatOntoStack(float value) = 0;
	};

	enum class ScriptStatus
	{
		Success,
		Error,
		Invalid
	};

	class IScript
	{
	protected:
		virtual ~IScript() {}

	public:
		virtual void Release() = 0;

		virtual ScriptStatus GetStatus() = 0;
		virtual int GetNumMessages() = 0;
		virtual const char* GetMessage(int index) = 0;
	};

	enum class SheepLanguageVersion
	{
		V100,
		V200
	};

	class ICompiler
	{
	protected:
		virtual ~ICompiler() {}

	public:
		virtual void Release() = 0;

		virtual int DefineImportFunction(const char* name, SymbolType returnType, SymbolType parameters[], int numParameters) = 0;

		virtual IScript* CompileScript(const char* script) = 0;
	};
}

extern "C" 
{
	SHP_DECLSPEC Sheep::ICompiler* SHP_APIENTRY CreateSheepCompiler(Sheep::SheepLanguageVersion version);
	SHP_DECLSPEC Sheep::IVirtualMachine* SHP_APIENTRY CreateSheepVirtualMachine();
}

#endif // SHEEP_SHEEPCPP_H

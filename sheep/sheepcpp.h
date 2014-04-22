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

/** @file */

/// test
namespace Sheep
{
	/// Represents a Sheep symbol type
	enum class SymbolType
	{
		/// "Void" symbol type
		Void = 0,
		
		/// 32-bit integer symbol type
		Int = 1,
		
		/// 32-bit floating point symbol type
		Float = 2,
		
		/// Variable length string variable type
		String = 3
	};

	class IScript;
	class IVirtualMachine;

	enum class ExecutionContextState
	{
		Prepared,
		Executing,
		Suspended,
		Finished
	};

	class IExecutionContext
	{
	public:
		/// Releases the Execution Context, decreasing its reference count by 1.
		///
		/// Once the reference count reaches 0 the Context is destroyed.
		virtual void Release() = 0;

		virtual int Execute() = 0;
		virtual int Suspend() = 0;

		virtual ExecutionContextState GetState() = 0;

		/// Gets the number of global variables within the script associated with the Context.
		virtual int GetNumVariables() = 0;
	
		/// Gets the name of the given variable, or null if the index is invalid.
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
	typedef void (SHP_CALLBACK* EndWaitCallback)(IVirtualMachine* vm, IExecutionContext* context);

	/// Represents a Sheep Virtual Machine which executes compiled Sheep scripts
	class IVirtualMachine
	{
	public:
		/// Releases the Virtual Machine, decreasing its reference count by 1.
		///
		/// Once the reference count reaches 0, the Virtual Machine and any IExecutionContexts that
		/// belong to it are destroyed. Be very careful when accessing IExecutionContext after the 
		/// parent IVirtualMachine has been released. If you don't know what you're doing you could easily cause a crash.
		virtual void Release() = 0;

		/// Sets a "tag," which is just a pointer to whatever you want. The Virtual Machine doesn't use it.
		/// It's just for convenience.
		virtual void SetTag(void* tag) = 0;
		/// Gets the tag that was set earlier, or null if no tag has been set.
		virtual void* GetTag() = 0;

		virtual int SetEndWaitCallback(EndWaitCallback callback) = 0;
		virtual int SetImportCallback(const char* importName, ImportCallback callback) = 0;

		virtual int PrepareScriptForExecution(IScript* script, const char* function, IExecutionContext** context) = 0;

		virtual int PopIntFromStack(int* result) = 0;
		virtual int PopFloatFromStack(float* result) = 0;
		virtual int PopStringFromStack(const char** result) = 0;
		virtual int PushIntOntoStack(int value) = 0;
		virtual int PushFloatOntoStack(float value) = 0;
	};

	/// Represents the status of an IScript object
	enum class ScriptStatus
	{
		/// The script was successfully compiled and is ready to run
		Success,
		
		/// There was an error compiling the script
		Error,
		
		/// The script is not valid and cannot run
		Invalid
	};

	/// Interface to a Script object
	class IScript
	{
	protected:
		virtual ~IScript() {}

	public:
		/// Releases the Script, decreasing its reference count by 1.
		///
		/// This is safe to call even if the Virtual Machine is still executing the script. Each IExecutionContext that is executing
		/// a particular IScript holds its own reference to the IScript.
		virtual void Release() = 0;

		/// Gets the status of the script.
		virtual ScriptStatus GetStatus() = 0;
	
		/// Gets the number of messages that were generated during compilation
		virtual int GetNumMessages() = 0;
	
		/// Gets the message text of the particular message generated during compilation
		virtual const char* GetMessage(int index) = 0;
	};

	/// Represents a specific version of Sheep
	enum class SheepLanguageVersion
	{
		/// Vanilla version of Sheep with no enhancements (Gabriel Knight 3-level features only)
		V100 = 0,
		
		/// New enhancements
		V200
	};

	/// Interface to a Compiler object that compiles sheep scripts
	class ICompiler
	{
	protected:
		virtual ~ICompiler() {}

	public:
		/// Releases the Compiler, decreasing its reference count by 1.
		///
		/// When all references to the compiler have been released the compiler
		/// is deleted. Any active instances of IScript that have been created by this compiler
		/// are still alive.
		virtual void Release() = 0;

		/// Defines an import function.
		/// @param name The name of the import function to define
		/// @param returnType The return type of the import function
		/// @param parameters An array with a list of parameter types, or null if there are no parameters
		/// @param numParameters The number of parameters. This should match the array length of "parameters"
		/// @return #SHEEP_SUCCESS if successful, #SHEEP_ERR_INVALID_ARGUMENT if "name" is null or the import function has already been defined.
		virtual int DefineImportFunction(const char* name, SymbolType returnType, SymbolType parameters[], int numParameters) = 0;

		/// Compiles the given sheep script.
		///
		/// This function always returns an instance if IScript, even if the script wasn't successfully compiled.
		/// You must examine the IScript result to find out the results of the compilation. Be sure to call IScript::Release()
		/// when done with the IScript result.
		/// @param script 0-terminated string containing sheep script
		/// @return A new instance of IScript.
		virtual IScript* CompileScript(const char* script) = 0;
	};
}


extern "C" 
{
	/// Creates an ICompiler object for compiling Sheep scripts
	///
	/// Be sure to call Sheep::ICompiler::Release() when you're done with the compiler.
	/// @param version The version of Sheep that the compiler should support
	/// @return A new ICompiler instance
	SHP_DECLSPEC Sheep::ICompiler* SHP_APIENTRY CreateSheepCompiler(Sheep::SheepLanguageVersion version);
	
	/// Creates an IVirtualMachine object for running Sheep scripts
	///
	/// Be sure to call IVirtualMachine::Release() when you're done with the virtual machine.
	/// @return A new IVirtualMachine instance
	SHP_DECLSPEC Sheep::IVirtualMachine* SHP_APIENTRY CreateSheepVirtualMachine();
	
	/// Creates a new IScript object from Sheep bytecode
	///
	/// Be sure to call Sheep::IScript::Release() when you're done with the script.
	/// @param bytecode The bytecode for the script
	/// @param length The length of the bytecode
	/// @param result A pointer to a pointer that will hold the new IScript instance
	/// @return #SHEEP_SUCCESS if successful, or an error otherwise
	SHP_DECLSPEC int SHP_APIENTRY CreateScriptFromBytecode(const char* bytecode, int length, Sheep::IScript** result);
}

#endif // SHEEP_SHEEPCPP_H
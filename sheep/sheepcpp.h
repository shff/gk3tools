#ifndef SHEEP_SHEEPCPP_H
#define SHEEP_SHEEPCPP_H

#include "sheepCommon.h"

/** @file */

/// Namespace that contains the C++ Sheep API
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

	/// Represents a specific version of Sheep
	enum class SheepLanguageVersion
	{
		/// Vanilla version of Sheep with no enhancements (Gabriel Knight 3-level features only)
		V100 = 0,
		
		/// New enhancements
		V200
	};

	class IScript;
	class IVirtualMachine;

	enum class ExecutionContextState
	{
		/// The context has been prepared for execution and is ready to execute
		Prepared,

		/// The context is currently executing
		Executing,

		/// The context has been suspended and is ready to resume
		Suspended,

		/// The context has finished execution
		Finished
	};

	/// Represents a currently executing Sheep script.
	///
	/// It is created using Sheep::IVirtualMachine::PrepareScriptForExecution() and is executed or resumed
	/// using Sheep::IExecutionContext::Execute().
	class IExecutionContext
	{
	protected:
		virtual ~IExecutionContext() {}
	public:
		/// Releases the Execution Context, decreasing its reference count by 1.
		///
		/// Once the reference count reaches 0 the Context is destroyed.
		virtual void Release() = 0;

		/// Executes a prepared Sheep::IExecutionContext (see IVirtualMachine::PrepareScriptForExecution()), or resumes a suspended context.
		/// @return #SHEEP_SUCCESS if the script completed successfully, #SHEEP_SUSPENDED if the script was
		/// suspended (most likely by a "wait"), #SHEEP_ERR_INVALID_OPERATION if context is not in a valid state.
		virtual int Execute() = 0;

		/// Suspends the context
		/// @return #SHEEP_SUCCESS if the context was successfully suspended, or #SHEEP_ERR_INVALID_OPERATION 
		/// if the context is not in the ExecutionContextState::Executing state
		virtual int Suspend() = 0;

		/// Gets the current state of the context
		virtual ExecutionContextState GetState() = 0;

		/// Gets the parent Sheep::IVirtualMachine that owns this context
		virtual IVirtualMachine* GetParentVirtualMachine() = 0;

		/// Gets whether the context is in a wait section
		virtual bool IsInWaitSection() = 0;

		/// Gets the number of global variables within the script associated with the Context.
		virtual int GetNumVariables() = 0;
	
		/// Gets the name of the given variable, or null if the index is invalid.
		virtual const char* GetVariableName(int index) = 0;

		/// Gets the type of variable at the specified index
		virtual SymbolType GetVariableType(int index) = 0;

		/// Sets the value of an integer variable
		/// @param index The index of the variable
		/// @param value The value to assign to the variable
		/// @return #SHEEP_SUCCESS if successful, #SHEEP_ERR_INVALID_ARGUMENT if index is invalid, 
		/// or #SHEEP_ERR_VARIABLE_INCORRECT_TYPE if the specified variable is not an integer
		virtual int SetVariableInt(int index, int value) = 0;

		/// Sets the value of a float variable
		/// @param index The index of the variable
		/// @param value The value to assign to the variable
		/// @return #SHEEP_SUCCESS if successful, #SHEEP_ERR_INVALID_ARGUMENT if index is invalid, 
		/// or #SHEEP_ERR_VARIABLE_INCORRECT_TYPE if the specified variable is not a float
		virtual int SetVariableFloat(int index, float value) = 0;

		/// Sets the value of a string variable
		/// @param index The index of the variable
		/// @param value The value to assign to the variable
		/// @return #SHEEP_SUCCESS if successful, #SHEEP_ERR_INVALID_ARGUMENT if index is invalid, 
		/// or #SHEEP_ERR_VARIABLE_INCORRECT_TYPE if the specified variable is not a string
		virtual int SetVariableString(int index, const char* value) = 0;

		/// Gets the value of an integer variable
		/// @param index The index of the variable
		/// @param result A pointer to an integer to where the value will be written
		/// @return #SHEEP_SUCCESS if successful, #SHEEP_ERR_INVALID_ARGUMENT if index is invalid or result is null, 
		/// or #SHEEP_ERR_VARIABLE_INCORRECT_TYPE if the specified variable is not an integer
		virtual int GetVariableInt(int index, int* result) = 0;

		/// Gets the value of a float variable
		/// @param index The index of the variable
		/// @param result A pointer to a float to where the value will be written
		/// @return #SHEEP_SUCCESS if successful, #SHEEP_ERR_INVALID_ARGUMENT if index is invalid or result is null, 
		/// or #SHEEP_ERR_VARIABLE_INCORRECT_TYPE if the specified variable is not a float
		virtual int GetVariableFloat(int index, float* result) = 0;

		/// Gets the value of a string variable
		/// @param index The index of the variable
		/// @param result A pointer to a character array to where the value will be written
		/// @return #SHEEP_SUCCESS if successful, #SHEEP_ERR_INVALID_ARGUMENT if index is invalid or result is null, 
		/// or #SHEEP_ERR_VARIABLE_INCORRECT_TYPE if the specified variable is not a string
		virtual int GetVariableString(int index, const char** result) = 0;

		/// Pops an integer from the top of the stack.
		/// @param result A pointer to an integer where the value on top of the stack will be written. May be null.
		/// @return #SHEEP_SUCCESS if successful, #SHEEP_ERR_EMPTY_STACK if the stack is empty, or #SHEEP_ERR_WRONG_TYPE_ON_STACK if the item
		/// on top of the stack is not an integer. If anything other than #SHEEP_SUCCESS is returned then the stack is not modified.
		virtual int PopIntFromStack(int* result) = 0;

		/// Pops a float from the top of the stack
		/// @param result A pointer to a float where the value on top of the stack will be written. May be null.
		/// @return #SHEEP_SUCCESS if successful, #SHEEP_ERR_EMPTY_STACK if the stack is empty, or #SHEEP_ERR_WRONG_TYPE_ON_STACK if the item
		/// on top of the stack is not a float. If anything other than #SHEEP_SUCCESS is returned then the stack is not modified.
		virtual int PopFloatFromStack(float* result) = 0;

		/// Pops a string from the top of the stack
		/// @param result A pointer to an character array where the value on top of the stack will be written. May be null.
		/// @return #SHEEP_SUCCESS if successful, #SHEEP_ERR_EMPTY_STACK if the stack is empty, or #SHEEP_ERR_WRONG_TYPE_ON_STACK if the item
		/// on top of the stack is not a string. If anything other than #SHEEP_SUCCESS is returned then the stack is not modified.
		virtual int PopStringFromStack(const char** result) = 0;

		/// Pushes an integer onto the stack
		/// @param value An integer that will be pushed onto the top of the stack
		/// @return #SHEEP_SUCCESS if successful, or #SHEEP_ERR_NO_CONTEXT_AVAILABLE if there is not a currently executing Sheep::IExecutionContext
		virtual int PushIntOntoStack(int value) = 0;

		/// Pushes a float onto the stack
		/// @param value An float that will be pushed onto the top of the stack
		/// @return #SHEEP_SUCCESS if successful, or #SHEEP_ERR_NO_CONTEXT_AVAILABLE if there is not a currently executing Sheep::IExecutionContext
		virtual int PushFloatOntoStack(float value) = 0;
	};

	typedef void (SHP_CALLBACK *ImportCallback)(IExecutionContext* context);
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

		/// Gets the language version with which this virtual machine was created
		virtual SheepLanguageVersion GetLanguageVersion() = 0;

		/// Sets a "tag," which is just a pointer to whatever you want. The Virtual Machine doesn't use it.
		/// It's just for convenience.
		virtual void SetTag(void* tag) = 0;
		/// Gets the tag that was set earlier, or null if no tag has been set.
		virtual void* GetTag() = 0;

		/// Sets the callback that will be called whenever Sheep encounters the end of a wait section
		/// 
		/// Generally, what you should do is check to see if there are any import functions still executing
		/// that were called within this wait section. If so, suspend the context and wait until everything
		/// has finished, then call Execute() to resume.
		/// @param callback The callback to call when the end of a wait section is encountered (can be null, though this is not recommended)
		/// @return #SHEEP_SUCCESS if successful
		virtual int SetEndWaitCallback(EndWaitCallback callback) = 0;

		/// Sets the callback for an import function.
		///
		/// Sheep will call the specified callback whenever it encounters the given import function.
		/// @param importName The name of the import
		/// @param callback A pointer to a function handler that will be called when a script calls the import function
		/// (can be null, though this is not recommended)
		/// @returns #SHEEP_SUCCESS if successful, #SHEEP_ERR_INVALID_ARGUMENT if importName is null
		virtual int SetImportCallback(const char* importName, ImportCallback callback) = 0;

		/// Creates a new Sheep::IExecutionContext ready to run the specified function.
		///
		/// You must call this method before calling Execute(). After you call this method you can read and modify
		/// variable values before the script is executed. Be sure to call Sheep::IExecutionContext::Release()
		/// when you're done with the context.
		/// @param script A pointer to the script that should be executed
		/// @param function The name of the function that should be executed
		/// @param context A pointer to a pointer to a Sheep::IExecutionContext that will store the new context
		/// @return #SHEEP_SUCCESS if successful, #SHEEP_ERR_INVALID_ARGUMENT if any of the parameters are null,
		/// or #SHEEP_ERR_NO_SUCH_FUNCTION if the function doesn't exist. 
		virtual int PrepareScriptForExecution(IScript* script, const char* function, IExecutionContext** context) = 0;

		virtual int PrepareScriptForExecutionWithParent(IScript* script, const char* function, IExecutionContext* parent, IExecutionContext** context) = 0;
	};
	
	/// Represents a disassembly of an IScript object
	class IDisassembly
	{
	protected:
		virtual ~IDisassembly() {}

	public:
		/// Releases the disassembly
		virtual void Release() = 0;

		/// Returns the text of the disassembly.
		///
		/// This string is freed when Release() is called.
		virtual const char* GetDisassemblyText() = 0;
	};

	class ICompiledScriptOutput
	{
	protected:
		virtual ~ICompiledScriptOutput() {}

	public:
		/// Releases the compiled script output object
		virtual void Release() = 0;

		/// Returns the size (in bytes) of the compiled output
		virtual int GetSize() = 0;

		/// Returns the raw compiled output data
		virtual const char* GetData() = 0;
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

		/// Gets the language version with which this script was created
		virtual SheepLanguageVersion GetLanguageVersion() = 0;

		/// Gets the status of the script.
		virtual ScriptStatus GetStatus() = 0;
	
		/// Gets the number of messages that were generated during compilation
		virtual int GetNumMessages() = 0;
	
		/// Gets the message text of the particular message generated during compilation
		virtual const char* GetMessage(int index) = 0;

		/// Gets the line number for which the particular message was generated
		virtual int GetMessageLineNumber(int index) = 0;

		/// Generates a disassembly
		/// @return An IDisassemby object, or null if the script's is not ScriptStatus::Success
		virtual IDisassembly* GenerateDisassembly() = 0;

		/// Generates compiled bytecode that can be written to a file
		/// and read later by CreateScriptFromBytecode()
		virtual ICompiledScriptOutput* GenerateCompiledOutput() = 0;
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

		/// Gets the language version with which this compiler was created
		virtual SheepLanguageVersion GetLanguageVersion() = 0;

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
	/// @return A new Sheep::ICompiler instance
	SHP_DECLSPEC Sheep::ICompiler* SHP_APIENTRY CreateSheepCompiler(Sheep::SheepLanguageVersion version);
	
	/// Creates an IVirtualMachine object for running Sheep scripts
	///
	/// Be sure to call IVirtualMachine::Release() when you're done with the virtual machine.
	/// @param 
	/// @return A new Sheep::IVirtualMachine instance
	SHP_DECLSPEC Sheep::IVirtualMachine* SHP_APIENTRY CreateSheepVirtualMachine(Sheep::SheepLanguageVersion version);
	
	/// Creates a new IScript object from Sheep bytecode
	///
	/// Be sure to call Sheep::IScript::Release() when you're done with the script.
	/// The bytecode can be freed after this call. The Sheep::IScript doesn't need it anymore.
	/// @param bytecode The bytecode for the script
	/// @param length The length of the bytecode
	/// @param result A pointer to a pointer that will hold the new IScript instance
	/// @return #SHEEP_SUCCESS if successful, or an error otherwise
	SHP_DECLSPEC int SHP_APIENTRY CreateScriptFromBytecode(const char* bytecode, int length, Sheep::IScript** result);
}

#endif // SHEEP_SHEEPCPP_H

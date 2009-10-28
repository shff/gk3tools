#ifndef SHEEPMACHINE_H
#define SHEEPMACHINE_H

#include <stack>
#include "sheepCodeGenerator.h"
#include "sheepImportTable.h"
#include "sheepException.h"

class SheepMachineException : public SheepException
{
public:
	SheepMachineException(const std::string& message)
		: SheepException(message, SHEEP_GENERIC_VM_ERROR)
	{
	}

	SheepMachineException(const std::string& message, int errorCode)
		: SheepException(message, errorCode)
	{
	}
};

class SheepMachineInstructionException : public SheepMachineException
{
public:
	SheepMachineInstructionException(const std::string& message)
		: SheepMachineException(message)
	{
	}
};

struct StackItem
{
	StackItem()
	{
		Type = SYM_VOID;
		IValue = 0;
	}

	StackItem(SheepSymbolType type, int value)
	{
		Type = type;
		IValue = value;
	}

	StackItem(SheepSymbolType type, float value)
	{
		Type = type;
		FValue = value;
	}

	SheepSymbolType Type;

	union
	{
		int IValue;
		float FValue;
	};
};

typedef std::stack<StackItem> SheepStack;

struct SheepContext
{
	SheepContext()
	{
		InWaitSection = false;
		FunctionOffset = 0;
		InstructionOffset = 0;
		CodeBuffer = NULL;
	}

	SheepStack Stack;
	std::vector<StackItem> Variables;

	bool InWaitSection;
	unsigned int FunctionOffset;
	unsigned int InstructionOffset;
	SheepCodeBuffer* CodeBuffer;
	IntermediateOutput* FullCode;
};

class SheepMachine : public SheepVM
{
public:

	SheepMachine();
	virtual ~SheepMachine();

	void SetOutputCallback(void (*callback)(const char* message));
	void SetCompileOutputCallback(SHP_MessageCallback callback);
	
	IntermediateOutput* Compile(const std::string& script);

	void Run(IntermediateOutput* code, const std::string& function);

	/// Runs a snippet. Returns SHEEP_SUCCESS on success, and the value
	/// left on the stack (if any) is put into 'result'. Or returns
	/// SHEEP_ERROR on error.
	int RunSnippet(const std::string& snippet, int* result);

	/// Resumes where the code left off.
	/// Returns SHEEP_SUCCESS on success, or SHEEP_ERROR on error.
	int Resume();
	int Suspend();

	int PopIntFromStack()
	{
		if (m_contexts.empty())
			throw SheepMachineException("No contexts available", SHEEP_ERR_NO_CONTEXT_AVAILABLE);

		return getInt(m_contexts.top().Stack);
	}

	float PopFloatFromStack()
	{
		StackItem item = m_contexts.top().Stack.top();
		m_contexts.top().Stack.pop();

		if (item.Type != SYM_FLOAT)
			throw SheepMachineException("Expected float on stack", SHEEP_ERR_WRONG_TYPE_ON_STACK);

		return item.FValue;
	}

	std::string& PopStringFromStack();

	void PushIntOntoStack(int i)
	{
		m_contexts.top().Stack.push(StackItem(SYM_INT, i));
	}
	
	SheepImportTable& GetImports() { return m_imports; }

	bool IsInWaitSection() { return m_contexts.empty() == false && m_contexts.top().InWaitSection; }
	bool IsSuspended() { return m_suspended; }
	void SetEndWaitCallback(SHP_EndWaitCallback callback);

	int GetNumContexts() { return m_contexts.size(); }
	int GetCurrentContextStackSize() { return m_contexts.top().Stack.size(); }
	

	enum Verbosity
	{
		Verbosity_Silent = 0,
		Verbosity_Polite = 1,
		Verbosity_Annoying = 2,
		Verbosity_Extreme = 3
	};

	void SetVerbosity(Verbosity verbosity) { m_verbosityLevel = verbosity; }
	Verbosity GetVerbosity() { return m_verbosityLevel; }

private:

	void prepareVariables(SheepContext& context);
	void execute(SheepContext& context);
	void executeContextsUntilSuspendedOrFinished();
	void executeNextInstruction(SheepContext& context);

	void (*m_callback)(const char* message);
	SHP_MessageCallback m_compilerCallback;

	SheepImportTable m_imports;

	SHP_EndWaitCallback m_endWaitCallback;


	typedef std::stack<SheepContext> SheepContextStack;
	SheepContextStack m_contexts;
	bool m_suspended;

	Verbosity m_verbosityLevel;

	// we consider Call() a built-in function and not technically an import,
	// mostly for performance reasons
	static void CALLBACK s_call(SheepVM* vm);

	static int getInt(SheepStack& stack)
	{
		if (stack.empty())
			throw SheepMachineException("Stack is empty", SHEEP_ERR_EMPTY_STACK);

		StackItem item = stack.top();
		stack.pop();

		if (item.Type != SYM_INT)
			throw SheepMachineException("Expected integer on stack", SHEEP_ERR_WRONG_TYPE_ON_STACK);

		return item.IValue;
	}

	static void get2Ints(SheepStack& stack, int& i1, int& i2)
	{
		if (stack.empty())
			throw SheepMachineException("Stack is empty", SHEEP_ERR_EMPTY_STACK);
		StackItem item2 = stack.top();
		stack.pop();

		if (stack.empty())
			throw SheepMachineException("Stack is empty", SHEEP_ERR_EMPTY_STACK);
		StackItem item1 = stack.top();
		stack.pop();
		
		if (item1.Type != SYM_INT || item2.Type != SYM_INT)
			throw SheepMachineException("Expected integers on stack.", SHEEP_ERR_WRONG_TYPE_ON_STACK);

		i1 = item1.IValue;
		i2 = item2.IValue;
	}

	static void get2Floats(SheepStack& stack, float& f1, float& f2)
	{
		if (stack.empty())
			throw SheepMachineException("Stack is empty", SHEEP_ERR_EMPTY_STACK);
		StackItem item2 = stack.top();
		stack.pop();

		if (stack.empty())
			throw SheepMachineException("Stack is empty", SHEEP_ERR_EMPTY_STACK);
		StackItem item1 = stack.top();
		stack.pop();
		
		if (item1.Type != SYM_FLOAT || item2.Type != SYM_FLOAT)
			throw SheepMachineException("Expected floats on stack.", SHEEP_ERR_WRONG_TYPE_ON_STACK);

		f1 = item1.FValue;
		f2 = item2.FValue;
	}

	static void storeI(SheepStack& stack, std::vector<StackItem>& variables, int variable)
	{
		int value = getInt(stack);

		if (variable >= variables.size() ||
			variables[variable].Type != SYM_INT)
			throw SheepMachineException("Invalid variable");

		variables[variable].IValue = value;
	}

	static void storeF(SheepStack& stack, std::vector<StackItem>& variables, int variable)
	{
		StackItem item = stack.top();
		stack.pop();
		if (item.Type != SYM_FLOAT)
			throw SheepMachineException("Expected float on stack");

		if (variable >= variables.size() ||
			variables[variable].Type != SYM_FLOAT)
			throw SheepMachineException("Invalid variable");

		variables[variable].FValue = item.FValue;
	}

	static void storeS(SheepStack& stack, std::vector<StackItem>& variables, int variable)
	{
		int value = getInt(stack);

		if (variable >= variables.size() ||
			variables[variable].Type != SYM_STRING)
			throw SheepMachineException("Invalid variable");

		variables[variable].IValue = value;
	}

	static void loadI(SheepStack& stack, std::vector<StackItem>& variables, int variable)
	{
		if (variable >= variables.size() ||
			variables[variable].Type != SYM_INT)
			throw SheepMachineException("Invalid variable");

		stack.push(StackItem(SYM_INT, variables[variable].IValue));
	}

	static void loadF(SheepStack& stack, std::vector<StackItem>& variables, int variable)
	{
		if (variable >= variables.size() ||
			variables[variable].Type != SYM_FLOAT)
			throw SheepMachineException("Invalid variable");

		stack.push(StackItem(SYM_FLOAT, variables[variable].FValue));
	}

	static void addI(SheepStack& stack)
	{
		int i1, i2;
		get2Ints(stack, i1, i2);

		stack.push(StackItem(SYM_INT, i1 + i2));
	}

	static void addF(SheepStack& stack)
	{
		float f1, f2;
		get2Floats(stack, f1, f2);

		stack.push(StackItem(SYM_FLOAT, f1 + f2));
	}

	static void subI(SheepStack& stack)
	{
		int i1, i2;
		get2Ints(stack, i1, i2);

		stack.push(StackItem(SYM_INT, i1 - i2));
	}

	static void subF(SheepStack& stack)
	{
		float f1, f2;
		get2Floats(stack, f1, f2);

		stack.push(StackItem(SYM_FLOAT, f1 - f2));
	}

	static void mulI(SheepStack& stack)
	{
		int i1, i2;
		get2Ints(stack, i1, i2);

		stack.push(StackItem(SYM_INT, i1 * i2));
	}

	static void mulF(SheepStack& stack)
	{
		float f1, f2;
		get2Floats(stack, f1, f2);

		stack.push(StackItem(SYM_FLOAT, f1 * f2));
	}

	static void divI(SheepStack& stack)
	{
		int i1, i2;
		get2Ints(stack, i1, i2);

		stack.push(StackItem(SYM_INT, i1 / i2));
	}

	static void divF(SheepStack& stack)
	{
		float f1, f2;
		get2Floats(stack, f1, f2);

		stack.push(StackItem(SYM_FLOAT, f1 / f2));
	}

	static void negI(SheepStack& stack)
	{
		StackItem item = stack.top();
		stack.pop();

		if (item.Type != SYM_INT)
			throw SheepMachineException("Expected integer on stack");

		stack.push(StackItem(SYM_INT, -item.IValue));
	}

	static void negF(SheepStack& stack)
	{
		StackItem item = stack.top();
		stack.pop();

		if (item.Type != SYM_FLOAT)
			throw SheepMachineException("Expected float on stack");

		stack.push(StackItem(SYM_FLOAT, -item.FValue));
	}

	static void itof(SheepStack& stack, int stackoffset)
	{
		SheepStack tempstack;
		for (int i = 0; i < stackoffset; i++)
		{
			tempstack.push(stack.top());
			stack.pop();
		}

		StackItem item = stack.top();
		stack.pop();

		if (item.Type != SYM_INT)
			throw SheepMachineException("Expected integer on stack");

		stack.push(StackItem(SYM_FLOAT, (float)item.IValue));

		while(tempstack.empty() == false)
		{
			stack.push(tempstack.top());
			tempstack.pop();
		}
	}

	static void ftoi(SheepStack& stack, int stackoffset)
	{
		SheepStack tempstack;
		for (int i = 0; i < stackoffset; i++)
		{
			tempstack.push(stack.top());
			stack.pop();
		}

		StackItem item = stack.top();
		stack.pop();

		if (item.Type != SYM_FLOAT)
			throw SheepMachineException("Expected integer on stack");

		stack.push(StackItem(SYM_INT, (int)item.FValue));

		while(tempstack.empty() == false)
		{
			stack.push(tempstack.top());
			tempstack.pop();
		}
	}

	static void andi(SheepStack& stack)
	{
		int i1, i2;
		get2Ints(stack, i1, i2);

		stack.push(StackItem(SYM_INT, i1 && i2 ? 1 : 0));
	}

	static void ori(SheepStack& stack)
	{
		int i1, i2;
		get2Ints(stack, i1, i2);

		stack.push(StackItem(SYM_INT, i1 || i2 ? 1 : 0));
	}

	static void noti(SheepStack& stack)
	{
		StackItem item = stack.top();
		stack.pop();

		if (item.Type != SYM_INT)
			throw SheepMachineException("Expected integer on stack");

		stack.push(StackItem(SYM_INT, item.IValue == 0 ? 1 : 0));
	}

	void callVoidFunction(SheepStack& stack, std::vector<SheepImport>& imports, int index)
	{
		callFunction(stack, imports, index, 0);

		// the GK3 VM seems to expect something on the stack, even after 'void' functions.
		stack.push(StackItem(SYM_INT, 0));
	}

	void callIntFunction(SheepStack& stack, std::vector<SheepImport>& imports, int index)
	{
		callFunction(stack, imports, index, 1);
	}

	void callFunction(SheepStack& stack, std::vector<SheepImport>& imports, int index, int numExpectedReturns)
	{
		int numParams = getInt(stack);

		const int MAX_NUM_PARAMS = 16;
		StackItem params[MAX_NUM_PARAMS];

		if (numParams >= MAX_NUM_PARAMS)
			throw SheepException("More than the maximum number of allowed parameters found", SHEEP_UNKNOWN_ERROR_PROBABLY_BUG);

		size_t numItemsOnStack = stack.size();

		// find the function
		if (index < 0 || index >= imports.size())
			throw SheepMachineException("Invalid import function");
		if (imports[index].Parameters.size() != numParams)
			throw SheepMachineException("Invalid number of parameters to import function");
		if (numParams > stack.size())
		{
			if (m_verbosityLevel > Verbosity_Silent)
				printf("stack size: %d numparams: %d\n", stack.size(), numParams);
			throw SheepMachineException("Stack is not in a valid state for calling this import function");
		}
			

		if (imports[index].Callback != NULL)
		{
			imports[index].Callback(this);
		}

		int paramsLeftOver = numParams - (int)(numItemsOnStack - stack.size());
		if (paramsLeftOver > numExpectedReturns)
		{
			// lazy bums didn't pop everything off!
			for (int i = numExpectedReturns; i < paramsLeftOver; i++)
				stack.pop();
		}
		else if (paramsLeftOver < numExpectedReturns)
		{
			// the idiots popped too much, or didn't put enough stuff on the stack!
			throw SheepMachineException("Incorrect number of items on the stack after function call");
		}
	}
};

#endif // SHEEPMACHINE_H

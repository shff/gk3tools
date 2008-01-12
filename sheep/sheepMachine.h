#ifndef SHEEPMACHINE_H
#define SHEEPMACHINE_H

#include <stack>
#include "sheepCodeGenerator.h"
#include "sheepImportTable.h"
#include "sheepException.h"

class SheepMachine;

struct SheepVM
{
	SheepMachine* Machine;
};

class SheepMachineException : public SheepException
{
public:
	SheepMachineException(const std::string& message)
		: SheepException(message)
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

class SheepMachine
{
public:

	SheepMachine(const SheepImportTable& imports);
	~SheepMachine();

	void SetOutputCallback(void (*callback)(const char* message));
	void SetCompileOutputCallback(void (*callback)(const char* message));
	
	/// Allows the script machine to prepare for execution.
	/// 'script' is assumed to be uncompiled, and will be compiled.
	void Prepare(const std::string& script);

	/// Allows the script machine to prepare for execution.
	/// 'code' is a pointer to binary code that has already been compiled.
	/// This takes over ownership of the code!
	void Prepare(IntermediateOutput* code);

	/// Executes a function. Make sure that Prepare() has been called first!
	void Run(const std::string& function);

	int PopIntFromStack()
	{
		return getInt(m_currentStack);
	}

	float PopFloatFromStack()
	{
		StackItem item = m_currentStack.top();
		m_currentStack.pop();

		if (item.Type != SYM_FLOAT)
			throw SheepMachineException("Expected float on stack");

		return item.FValue;
	}

	std::string& PopStringFromStack()
	{
		StackItem item = m_currentStack.top();
		m_currentStack.pop();

		if (item.Type != SYM_STRING)
			throw SheepMachineException("Expected float on stack");

		for (std::vector<SheepStringConstant>::iterator itr = m_code->Constants.begin();
			itr != m_code->Constants.end(); itr++)
		{
			if ((*itr).Offset == item.IValue)
				return (*itr).Value;
		}

		throw SheepMachineException("Invalid string offset found on stack");
	}

private:

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

	void prepareVariables();
	void execute(SheepCodeBuffer* code, std::vector<SheepImport>& imports, unsigned int offset);

	void (*m_callback)(const char* message);
	void (*m_compilerCallback)(const char* message);

	std::vector<StackItem> m_variables;
	IntermediateOutput* m_code;
	SheepImportTable m_imports;

	SheepStack m_currentStack;

	static int getInt(SheepStack& stack)
	{
		StackItem item = stack.top();
		stack.pop();

		if (item.Type != SYM_INT)
			throw SheepMachineException("Expected integer on stack");

		return item.IValue;
	}

	static void get2Ints(SheepStack& stack, int& i1, int& i2)
	{
		StackItem item1 = stack.top();
		stack.pop();
		StackItem item2 = stack.top();
		stack.pop();
		
		if (item1.Type != SYM_INT || item2.Type != SYM_INT)
			throw SheepMachineException("Expected integers on stack.");

		i1 = item1.IValue;
		i2 = item2.IValue;
	}

	static void get2Floats(SheepStack& stack, float& f1, float& f2)
	{
		StackItem item1 = stack.top();
		stack.pop();
		StackItem item2 = stack.top();
		stack.pop();
		
		if (item1.Type != SYM_FLOAT || item2.Type != SYM_FLOAT)
			throw SheepMachineException("Expected floats on stack.");

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

	static void storeF(SheepStack& stack, std::vector<StackItem>& variables, float variable)
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
		int numParams = getInt(stack);

		const int MAX_NUM_PARAMS = 16;
		StackItem params[MAX_NUM_PARAMS];

		if (numParams >= MAX_NUM_PARAMS)
			throw SheepException("More than the maximum number of allowed parameters found");

		size_t numItemsOnStack = stack.size();

		// find the function
		if (index < 0 || index >= imports.size())
			throw SheepMachineException("Invalid import function");
		if (imports[index].Parameters.size() != numParams)
			throw SheepMachineException("Invalid number of parameters to import function");

		if (imports[index].Callback != NULL)
		{
			SheepVM vm;
			vm.Machine = this;
			imports[index].Callback(&vm);
		}

		int paramsLeftOver = numParams - (numItemsOnStack - stack.size());
		if (paramsLeftOver > 0)
		{
			// lazy bums didn't pop everything off!
			for (int i = 0; i < paramsLeftOver; i++)
				stack.pop();
		}
		else if (paramsLeftOver < 0)
		{
			// the idiots popped too much!
			throw SheepMachineException("Too many items popped from the stack during function call");
		}

		stack.push(StackItem(SYM_INT, 0));
	}
};

#endif // SHEEPMACHINE_H

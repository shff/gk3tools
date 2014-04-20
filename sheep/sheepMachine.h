#ifndef SHEEPMACHINE_H
#define SHEEPMACHINE_H

#include <stack>
#include "sheepContextTree.h"
#include "sheepCodeGenerator.h"
#include "sheepImportTable.h"
#include "sheepCodeBuffer.h"
#include "sheepException.h"
#include "sheepStringDictionary.h"

#ifndef _MSC_VER
#define _snprintf snprintf
#endif

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



class SheepMachine : public Sheep::IVirtualMachine
{
	int m_refCount;
	StringDictionary<Sheep::ImportCallback> m_importCallbacks;

public:

	SheepMachine();
	virtual ~SheepMachine();

	void Release() override;

	void SetOutputCallback(void (*callback)(const char* message));
	void SetCompileOutputCallback(SHP_MessageCallback callback);

	int SetEndWaitCallback(Sheep::EndWaitCallback callback) { m_endWaitCallback = callback; return SHEEP_SUCCESS; }
	int SetImportCallback(const char* importName, Sheep::ImportCallback callback);

	void Run(Sheep::IScript* script, const std::string& function);

	int PrepareScriptForExecution(Sheep::IScript* script, const char* function, Sheep::IExecutionContext** context) override;

	int Execute(Sheep::IExecutionContext* context) override;

	SheepContext* Suspend();

	int PopIntFromStack(int* result) override
	{
		SheepContext* current = m_contextTree->GetCurrent();

		if (current == NULL)
			return SHEEP_ERR_NO_CONTEXT_AVAILABLE;

		if (result != nullptr)
			*result = getInt(current->Stack);

		return SHEEP_SUCCESS;
	}

	int PopFloatFromStack(float* result) override
	{
		SheepContext* current = m_contextTree->GetCurrent();

		if (current == NULL)
			return SHEEP_ERR_NO_CONTEXT_AVAILABLE;

		StackItem item = current->Stack.top();
		current->Stack.pop();

		if (item.Type != SheepSymbolType::Float)
			return SHEEP_ERR_WRONG_TYPE_ON_STACK;

		if (result != nullptr)
			*result = item.FValue;

		return SHEEP_SUCCESS;
	}

	int PopStringFromStack(const char** result) override;

	int PushIntOntoStack(int i) override
	{
		SheepContext* current = m_contextTree->GetCurrent();

		if (current == NULL)
			return SHEEP_ERR_NO_CONTEXT_AVAILABLE;

		current->Stack.push(StackItem(SheepSymbolType::Int, i));

		return SHEEP_SUCCESS;
	}
	
	int PushFloatOntoStack(float f) override
	{
		SheepContext* current = m_contextTree->GetCurrent();

		if (current == NULL)
			return SHEEP_ERR_NO_CONTEXT_AVAILABLE;

		current->Stack.push(StackItem(SheepSymbolType::Float, f));

		return SHEEP_SUCCESS;
	}

	bool IsInWaitSection()
	{ 
		SheepContext* current = m_contextTree->GetCurrent();
		return current != NULL && current->InWaitSection; 
	}

	int GetNumContexts() { return 0; }
	int GetCurrentContextStackSize()
	{ 
		return m_contextTree->GetCurrent()->Stack.size(); 
	}
	SheepContext* GetCurrentContext() { return m_contextTree->GetCurrent(); }
	void PrintStackTrace();

	void SetLanguageEnhancementsEnabled(bool enabled);

	enum Verbosity
	{
		Verbosity_Silent = 0,
		Verbosity_Polite = 1,
		Verbosity_Annoying = 2,
		Verbosity_Extreme = 3
	};

	void SetVerbosity(Verbosity verbosity) { m_verbosityLevel = verbosity; }
	Verbosity GetVerbosity() { return m_verbosityLevel; }

	void SetTag(void* tag) { m_tag = tag; }
	void* GetTag() { return m_tag; }

private:

	void prepareVariables(SheepContext* context);
	void execute(SheepContext* context);
	void executeNextInstruction(SheepContext* context);

	void (*m_callback)(const char* message);
	SHP_MessageCallback m_compilerCallback;

	SheepContextTree* m_contextTree;
	int m_executingDepth;

	Sheep::EndWaitCallback m_endWaitCallback;

	Verbosity m_verbosityLevel;

	void* m_tag;
	bool m_enhancementsEnabled;

	// we consider Call() a built-in function and not technically an import,
	// mostly for performance reasons
	static void CALLBACK s_call(Sheep::IVirtualMachine* vm);

	static int getInt(SheepStack& stack, bool string = false)
	{
		if (stack.empty())
			throw SheepMachineException("Stack is empty", SHEEP_ERR_EMPTY_STACK);

		StackItem item = stack.top();
		stack.pop();

		if (string == false && item.Type != SheepSymbolType::Int)
		{
			char buffer[256];
			_snprintf(buffer, 256, "Expected integer on stack; found %d", item.Type);
			throw SheepMachineException(buffer, SHEEP_ERR_WRONG_TYPE_ON_STACK);
		}
		else if (string == true && item.Type != SheepSymbolType::String)
		{
			char buffer[256];
			_snprintf(buffer, 256, "Expected string on stack; found %d", item.Type);
			throw SheepMachineException(buffer, SHEEP_ERR_WRONG_TYPE_ON_STACK);
		}

		return item.IValue;
	}

	static void get2Ints(SheepStack& stack, int& i1, int& i2, SheepInstruction instruction)
	{
		if (stack.empty())
			throw SheepMachineException("Stack is empty", SHEEP_ERR_EMPTY_STACK);
		StackItem item2 = stack.top();
		stack.pop();

		if (stack.empty())
			throw SheepMachineException("Stack is empty", SHEEP_ERR_EMPTY_STACK);
		StackItem item1 = stack.top();
		stack.pop();
		
		if (item1.Type != SheepSymbolType::Int || item2.Type != SheepSymbolType::Int)
		{
			char buffer[256];
			_snprintf(buffer, 256, "Expected integers on stack during %x instruction. Found: %d, %d\n", instruction, item1.Type, item2.Type);
			throw SheepMachineException(buffer, SHEEP_ERR_WRONG_TYPE_ON_STACK);
		}

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
		
		if (item1.Type != SheepSymbolType::Float || item2.Type != SheepSymbolType::Float)
			throw SheepMachineException("Expected floats on stack.", SHEEP_ERR_WRONG_TYPE_ON_STACK);

		f1 = item1.FValue;
		f2 = item2.FValue;
	}

	static void storeI(SheepStack& stack, SheepContext* context, int variable)
	{
		int value = getInt(stack);

		if (context->SetVariableInt(variable, value) != SHEEP_SUCCESS)
			throw SheepMachineException("Invalid variable");
	}

	static void storeF(SheepStack& stack, SheepContext* context, int variable)
	{
		StackItem item = stack.top();
		stack.pop();
		if (item.Type != SheepSymbolType::Float)
			throw SheepMachineException("Expected float on stack");

		if (context->SetVariableFloat(variable, item.FValue) != SHEEP_SUCCESS)
			throw SheepMachineException("Invalid variable");
	}

	static void storeS(SheepStack& stack, SheepContext* context, int variable)
	{
		int value = getInt(stack, true);

		if (context->SetVariableInt(variable, value) != SHEEP_SUCCESS)
			throw SheepMachineException("Invalid variable");
	}

	static void loadI(SheepStack& stack, SheepContext* context, int variable)
	{
		int value;
		if (context->GetVariableInt(variable, &value) != SHEEP_SUCCESS)
			throw SheepMachineException("Invalid variable");

		stack.push(StackItem(SheepSymbolType::Int, value));
	}

	static void loadF(SheepStack& stack, SheepContext* context, int variable)
	{
		float value;
		if (context->GetVariableFloat(variable, &value) != SHEEP_SUCCESS)
			throw SheepMachineException("Invalid variable");

		stack.push(StackItem(SheepSymbolType::Float, value));
	}

	static void loadS(SheepStack& stack, SheepContext* context, int variable)
	{
		int value;
		if (context->GetVariableStringIndex(variable, &value) != SHEEP_SUCCESS)
			throw SheepMachineException("Invalid variable");

		stack.push(StackItem(SheepSymbolType::String, value));
	}

	static void addI(SheepStack& stack)
	{
		int i1, i2;
		get2Ints(stack, i1, i2, AddI);

		stack.push(StackItem(SheepSymbolType::Int, i1 + i2));
	}

	static void addF(SheepStack& stack)
	{
		float f1, f2;
		get2Floats(stack, f1, f2);

		stack.push(StackItem(SheepSymbolType::Float, f1 + f2));
	}

	static void subI(SheepStack& stack)
	{
		int i1, i2;
		get2Ints(stack, i1, i2, SubtractI);

		stack.push(StackItem(SheepSymbolType::Int, i1 - i2));
	}

	static void subF(SheepStack& stack)
	{
		float f1, f2;
		get2Floats(stack, f1, f2);

		stack.push(StackItem(SheepSymbolType::Float, f1 - f2));
	}

	static void mulI(SheepStack& stack)
	{
		int i1, i2;
		get2Ints(stack, i1, i2, MultiplyI);

		stack.push(StackItem(SheepSymbolType::Int, i1 * i2));
	}

	static void mulF(SheepStack& stack)
	{
		float f1, f2;
		get2Floats(stack, f1, f2);

		stack.push(StackItem(SheepSymbolType::Float, f1 * f2));
	}

	static void divI(SheepStack& stack)
	{
		int i1, i2;
		get2Ints(stack, i1, i2, DivideI);

		stack.push(StackItem(SheepSymbolType::Int, i1 / i2));
	}

	static void divF(SheepStack& stack)
	{
		float f1, f2;
		get2Floats(stack, f1, f2);

		stack.push(StackItem(SheepSymbolType::Float, f1 / f2));
	}

	static void negI(SheepStack& stack)
	{
		StackItem item = stack.top();
		stack.pop();

		if (item.Type != SheepSymbolType::Int)
			throw SheepMachineException("Expected integer on stack");

		stack.push(StackItem(SheepSymbolType::Int, -item.IValue));
	}

	static void negF(SheepStack& stack)
	{
		StackItem item = stack.top();
		stack.pop();

		if (item.Type != SheepSymbolType::Float)
			throw SheepMachineException("Expected float on stack");

		stack.push(StackItem(SheepSymbolType::Float, -item.FValue));
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

		if (item.Type != SheepSymbolType::Int)
			throw SheepMachineException("Expected integer on stack");

		stack.push(StackItem(SheepSymbolType::Float, (float)item.IValue));

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

		if (item.Type != SheepSymbolType::Float)
			throw SheepMachineException("Expected integer on stack");

		stack.push(StackItem(SheepSymbolType::Int, (int)item.FValue));

		while(tempstack.empty() == false)
		{
			stack.push(tempstack.top());
			tempstack.pop();
		}
	}

	static void andi(SheepStack& stack)
	{
		int i1, i2;
		get2Ints(stack, i1, i2, And);

		stack.push(StackItem(SheepSymbolType::Int, i1 && i2 ? 1 : 0));
	}

	static void ori(SheepStack& stack)
	{
		int i1, i2;
		get2Ints(stack, i1, i2, Or);

		stack.push(StackItem(SheepSymbolType::Int, i1 || i2 ? 1 : 0));
	}

	static void noti(SheepStack& stack)
	{
		StackItem item = stack.top();
		stack.pop();

		if (item.Type != SheepSymbolType::Int)
			throw SheepMachineException("Expected integer on stack");

		stack.push(StackItem(SheepSymbolType::Int, item.IValue == 0 ? 1 : 0));
	}

	void callVoidFunction(SheepContext* context, int index)
	{
		callFunction(context, index, 0);

		// the GK3 VM seems to expect something on the stack, even after 'void' functions.
		context->Stack.push(StackItem(SheepSymbolType::Int, 0));
	}

	void callIntFunction(SheepContext* context, int index)
	{
		callFunction(context, index, 1);
	}

	void callFunction(SheepContext* context, int index, int numExpectedReturns)
	{
		int numParams = getInt(context->Stack);

		const int MAX_NUM_PARAMS = 16;
		StackItem params[MAX_NUM_PARAMS];

		if (numParams >= MAX_NUM_PARAMS)
			throw SheepException("More than the maximum number of allowed parameters found", SHEEP_UNKNOWN_ERROR_PROBABLY_BUG);

		size_t numItemsOnStack = context->Stack.size();

		// find the function
		if (index < 0 || index >= context->FullCode->Imports.size())
			throw SheepMachineException("Invalid import function");
		if (context->FullCode->Imports[index].Parameters.size() != numParams)
			throw SheepMachineException("Invalid number of parameters to import function");
		if (numParams > context->Stack.size())
		{
			if (m_verbosityLevel > Verbosity_Silent)
				printf("stack size: %d numparams: %d\n", context->Stack.size(), numParams);
			throw SheepMachineException("Stack is not in a valid state for calling this import function");
		}
			
		Sheep::ImportCallback callback;
		if (m_importCallbacks.TryGetValue(context->FullCode->Imports[index].Name.c_str(), callback) && callback != nullptr)
			callback(this);

		int paramsLeftOver = numParams - (int)(numItemsOnStack - context->Stack.size());
		if (paramsLeftOver > numExpectedReturns)
		{
			// lazy bums didn't pop everything off!
			for (int i = numExpectedReturns; i < paramsLeftOver; i++)
				context->Stack.pop();
		}
		else if (paramsLeftOver < numExpectedReturns)
		{
			// the idiots popped too much, or didn't put enough stuff on the stack!
			throw SheepMachineException("Incorrect number of items on the stack after function call");
		}
	}
};

#endif // SHEEPMACHINE_H

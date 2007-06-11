
#include <vector>

enum Instruction
{
	SitnSpin,
	CallSysFunctionV,
	CallSysFunctionI,
	Branch,
	BranchIfZero,
	
	BeginWait,
	EndWait,
	ReturnV,

	PushI,
	PushS,
	Pop,

	IsEqualI,
	IsLessI,

	IToF,
	And,
	Or,
	GetString
};

class Function
{
public:
	Function(const std::string& name)
	{
		m_name = name;
	}

private:
	std::string m_name;
};

class Compiler
{
public:

	void AddFunction(Function* function);
	
	void SetCurrentFunction(const std::string& name);
	Function* GetCurrentFunction() { return m_currentFunction; }

private:
	Function* m_currentFunction;
	std::vector<Function*> m_functions;
};

#ifndef SHEEPEXCEPTION_H
#define SHEEPEXCEPTION_H

#include "sheepc.h"

class SheepException : public std::exception
{
public:
	SheepException(const std::string& message, int errorNum) throw()
	{
		m_message = message;
		m_errorNum = errorNum;
	}
	
	virtual ~SheepException() throw() {}

	std::string GetMessage() { return m_message; }
	const char* what() const throw() { return m_message.c_str(); }

	int GetErrorNum() { return m_errorNum; }

private:
	std::string m_message;
	int m_errorNum;
};

class NoSuchFunctionException : public SheepException
{
public:
	NoSuchFunctionException(const std::string& name) throw()
		: SheepException("No such function: " + name, SHEEP_ERR_NO_SUCH_FUNCTION)
	{
	}

	virtual ~NoSuchFunctionException() throw() {}
};

class SheepCompilerException : public SheepException
{
public:
	SheepCompilerException(int lineNumber, const std::string& message) throw()
		: SheepException(message, SHEEP_GENERIC_COMPILER_ERROR)
	{
		m_lineNumber = lineNumber;
	}

	int GetLineNumber() { return m_lineNumber; }

private:
	int m_lineNumber;
};


#endif // SHEEPEXCEPTION_H

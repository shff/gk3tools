#ifndef SHEEPEXCEPTION_H
#define SHEEPEXCEPTION_H

class SheepException : public std::exception
{
public:
	SheepException(const std::string& message) throw()
	{
		m_message = message;
	}
	
	virtual ~SheepException() throw() {}

	std::string GetMessage() { return m_message; }
	const char* what() const throw() { return m_message.c_str(); }

private:
	std::string m_message;

};

class SheepCompilerException : public SheepException
{
public:
	SheepCompilerException(int lineNumber, const std::string& message) throw()
		: SheepException(message)
	{
		m_lineNumber = lineNumber;
	}

	int GetLineNumber() { return m_lineNumber; }

private:
	int m_lineNumber;
};

#endif // SHEEPEXCEPTION_H
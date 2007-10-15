#ifndef SHEEPLOG_H
#define SHEEPLOG_H

#include <vector>

enum LogEntryType
{
	LOG_INFO = 0,
	LOG_WARNING = 1,
	LOG_ERROR = 2
};

struct SheepLogEntry
{
	SheepLogEntry(LogEntryType type, int lineNumber, const std::string& text)
	{
		Type = type;
		LineNumber = lineNumber;
		Text = text;
	}
	
	LogEntryType Type;
	int LineNumber;
	std::string Text;
};

class SheepLog
{
public:
	
	void AddEntry(LogEntryType type, int lineNumber, const std::string& text)
	{
		m_entries.push_back(SheepLogEntry(type, lineNumber, text));
	}
	
	const std::vector<SheepLogEntry>& GetEntries() { return m_entries; }
	
private:
	
	std::vector<SheepLogEntry> m_entries;
};

#endif // SHEEPLOG_H

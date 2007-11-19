#ifndef SHEEPIMPORTTABLE_H
#define SHEEPIMPORTTABLE_H

#include <map>
#include <vector>
#include "sheepTypes.h"

/// A list of all the imports registered with the compiler
class SheepImportTable
{
public:

	bool TryAddImport(const std::string& name, SheepSymbolType returnType)
	{
		if (returnType != SYM_VOID && returnType != SYM_INT && returnType != SYM_FLOAT && returnType != SYM_STRING)
			return false;

		SheepImport import;
		import.Name = name;
		import.ReturnType = returnType;

		return m_imports.insert(std::pair<std::string, SheepImport>(name, import)).second;
	}

	bool TryAddImport(const std::string& name, SheepSymbolType returnType, SheepSymbolType parameter)
	{
		if (returnType != SYM_VOID && returnType != SYM_INT && returnType != SYM_FLOAT && returnType != SYM_STRING)
			return false;

		SheepImport import;
		import.Name = name;
		import.ReturnType = returnType;
		import.Parameters.push_back(parameter);

		return m_imports.insert(std::pair<std::string, SheepImport>(name, import)).second;
	}

	bool TryAddImport(const std::string& name, SheepSymbolType returnType, const std::vector<SheepSymbolType>& parameters)
	{
		if (returnType != SYM_VOID && returnType != SYM_INT && returnType != SYM_FLOAT && returnType != SYM_STRING)
			return false;

		SheepImport import;
		import.Name = name;
		import.ReturnType = returnType;
		import.Parameters = parameters;

		return m_imports.insert(std::pair<std::string, SheepImport>(name, import)).second;
	}

	bool TryFindImport(const std::string& name, SheepImport& import)
	{
		std::map<std::string, SheepImport>::iterator itr = m_imports.find(name);

		if (itr == m_imports.end())
			return false;

		import = (*itr).second;

		return true;
	}

private:

	std::map<std::string, SheepImport> m_imports;
};

#endif // SHEEPIMPORTTABLE_H
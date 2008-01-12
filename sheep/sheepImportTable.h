#ifndef SHEEPIMPORTTABLE_H
#define SHEEPIMPORTTABLE_H

#include <map>
#include <vector>
#include "sheepTypes.h"

/// A list of all the imports registered with the compiler
class SheepImportTable
{
public:

	bool TryAddImport(const std::string& name, SheepSymbolType returnType, SheepImportCallback callback)
	{
		if (returnType != SYM_VOID && returnType != SYM_INT && returnType != SYM_FLOAT && returnType != SYM_STRING)
			return false;

		SheepImport import;
		import.Name = name;
		import.ReturnType = returnType;
		import.Callback = callback;

		return m_imports.insert(ImportMap::value_type(name, import)).second;
	}

	bool TryAddImport(const std::string& name, SheepSymbolType returnType, SheepSymbolType parameter, SheepImportCallback callback)
	{
		if (returnType != SYM_VOID && returnType != SYM_INT && returnType != SYM_FLOAT && returnType != SYM_STRING)
			return false;

		SheepImport import;
		import.Name = name;
		import.ReturnType = returnType;
		import.Parameters.push_back(parameter);
		import.Callback = callback;

		return m_imports.insert(ImportMap::value_type(name, import)).second;
	}

	bool TryAddImport(const std::string& name, SheepSymbolType returnType, SheepSymbolType parameter1, SheepSymbolType parameter2, SheepImportCallback callback)
	{
		if (returnType != SYM_VOID && returnType != SYM_INT && returnType != SYM_FLOAT && returnType != SYM_STRING)
			return false;

		SheepImport import;
		import.Name = name;
		import.ReturnType = returnType;
		import.Parameters.push_back(parameter1);
		import.Parameters.push_back(parameter2);
		import.Callback = callback;

		return m_imports.insert(ImportMap::value_type(name, import)).second;
	}

	bool TryAddImport(const std::string& name, SheepSymbolType returnType, const std::vector<SheepSymbolType>& parameters, SheepImportCallback callback)
	{
		if (returnType != SYM_VOID && returnType != SYM_INT && returnType != SYM_FLOAT && returnType != SYM_STRING)
			return false;

		SheepImport import;
		import.Name = name;
		import.ReturnType = returnType;
		import.Parameters = parameters;
		import.Callback = callback;

		return m_imports.insert(ImportMap::value_type(name, import)).second;
	}

	bool TryFindImport(const std::string& name, SheepImport& import)
	{
		ImportMap::iterator itr = m_imports.find(name);

		if (itr == m_imports.end())
			return false;

		import = (*itr).second;

		return true;
	}

private:

	typedef std::map<std::string, SheepImport> ImportMap;
	ImportMap m_imports;
};

#endif // SHEEPIMPORTTABLE_H
#ifndef SHEEPIMPORTTABLE_H
#define SHEEPIMPORTTABLE_H

#include <map>
#include <vector>
#include "sheepTypes.h"

/// A list of all the imports registered with the compiler
class SheepImportTable
{
public:

	~SheepImportTable()
	{
		while(m_imports.empty() == false)
		{
			delete (*m_imports.begin()).second;
			m_imports.erase(m_imports.begin());
		}
	}

	bool TryAddImport(const std::string& name, SheepSymbolType returnType, SheepImportCallback callback)
	{
		SheepImport* import = NewImport(name, returnType, callback);
		
		if (import)
		{
			return true;
		}
		
		return false;
	}

	bool TryAddImport(const std::string& name, SheepSymbolType returnType, SheepSymbolType parameter, SheepImportCallback callback)
	{
		SheepImport* import = NewImport(name, returnType, callback);
		
		if (import)
		{
			import->Parameters.push_back(parameter);
			return true;
		}
		
		return false;
	}

	bool TryAddImport(const std::string& name, SheepSymbolType returnType, SheepSymbolType parameter1, SheepSymbolType parameter2, SheepImportCallback callback)
	{
		SheepImport* import = NewImport(name, returnType, callback);
		
		if (import)
		{
			import->Parameters.push_back(parameter1);
			import->Parameters.push_back(parameter2);
			return true;
		}
		
		return false;
	}

	bool TryAddImport(const std::string& name, SheepSymbolType returnType, const std::vector<SheepSymbolType>& parameters, SheepImportCallback callback)
	{
		SheepImport* import = NewImport(name, returnType, callback);
		
		if (import)
		{
			import->Parameters = parameters;
			return true;
		}
		
		return false;
	}
	
	SheepImport* NewImport(const std::string& name, SheepSymbolType returnType, SheepImportCallback callback)
	{
		if (returnType != SYM_VOID && returnType != SYM_INT && returnType != SYM_FLOAT)
			return NULL;
		
		SheepImport* import = new SheepImport;
		import->Name = name;
		import->ReturnType = returnType;
		import->Callback = callback;
		
		if (m_imports.insert(ImportMap::value_type(name, import)).second == false)
		{
			delete import;
			return NULL;
		}
		
		return import;
	}

	bool TryFindImport(const std::string& name, SheepImport& import)
	{
		ImportMap::iterator itr = m_imports.find(name);

		if (itr == m_imports.end())
			return false;

		import = *(*itr).second;

		return true;
	}

private:

	typedef std::map<std::string, SheepImport*> ImportMap;
	ImportMap m_imports;
};

#endif // SHEEPIMPORTTABLE_H

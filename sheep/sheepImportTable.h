#ifndef SHEEPIMPORTTABLE_H
#define SHEEPIMPORTTABLE_H

#include <map>
#include <vector>
#include "sheepTypes.h"
#include "sheepCaseInsensitiveStringCompare.h"
#include "sheepMemoryAllocator.h"

/// A list of all the imports registered with the compiler
class SheepImportTable
{
public:

	~SheepImportTable()
	{
		while(m_imports.empty() == false)
		{
			SHEEP_DELETE((*m_imports.begin()).second);
			m_imports.erase(m_imports.begin());
		}
	}

	bool TryAddImport(const std::string& name, SheepSymbolType returnType, SHP_ImportCallback callback)
	{
		SheepImport* import = NewImport(name, returnType, callback);
		
		if (import)
		{
			return true;
		}
		
		return false;
	}

	bool TryAddImport(const std::string& name, SheepSymbolType returnType, SheepSymbolType parameter, SHP_ImportCallback callback)
	{
		SheepImport* import = NewImport(name, returnType, callback);
		
		if (import)
		{
			import->Parameters.push_back(parameter);
			return true;
		}
		
		return false;
	}

	bool TryAddImport(const std::string& name, SheepSymbolType returnType, SheepSymbolType parameter1, SheepSymbolType parameter2, SHP_ImportCallback callback)
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

	bool TryAddImport(const std::string& name, SheepSymbolType returnType, const std::vector<SheepSymbolType>& parameters, SHP_ImportCallback callback)
	{
		SheepImport* import = NewImport(name, returnType, callback);
		
		if (import)
		{
			import->Parameters = parameters;
			return true;
		}
		
		return false;
	}
	
	SheepImport* NewImport(const std::string& name, SheepSymbolType returnType, SHP_ImportCallback callback)
	{
		if (returnType != SYM_VOID && returnType != SYM_INT && returnType != SYM_FLOAT && returnType != SYM_STRING)
			return NULL;
		
		SheepImport* import = SHEEP_NEW SheepImport;
		import->Name = name;
		import->ReturnType = returnType;
		import->Callback = callback;
		
		if (m_imports.insert(ImportMap::value_type(name, import)).second == false)
		{
			SHEEP_DELETE(import);
			return NULL;
		}
		
		return import;
	}

	bool TryFindImport(const std::string& name, SheepImport& import) const
	{
		ImportMap::const_iterator itr = m_imports.find(name);

		if (itr == m_imports.end())
			return false;

		import = *(*itr).second;

		return true;
	}

private:

	typedef std::map<std::string, SheepImport*, ci_less> ImportMap;
	ImportMap m_imports;
};

#endif // SHEEPIMPORTTABLE_H

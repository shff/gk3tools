#ifndef SHEEPSTRINGDICTIONARY_H
#define SHEEPSTRINGDICTIONARY_H

#include <memory>
#include <vector>
#include <string>
#include "sheepCaseInsensitiveStringCompare.h"


template <typename T>
class StringDictionary
{
	std::vector<std::pair<std::string, T> > m_data;

	struct comparer
	{
		bool operator()(const std::pair<std::string, T>& a, const std::pair<std::string, T>& b)
		{
			return ci_less::areEqual(a.first, b.first);
		}
	};


public:

	typedef typename std::vector<std::pair<std::string, T> >::iterator StringDictionaryIterator;
	//typedef typename StringDictionaryVector::iterator StringDictionaryIterator2;

	StringDictionaryIterator Begin() { return m_data.begin(); }
	StringDictionaryIterator End() { return m_data.end(); }

	bool IsEmpty()
	{
		return m_data.empty();
	}

	bool Insert(const char* key, const T& value)
	{
		// see if it already exists
		if (indexOf(key) >= 0)
			return false;

		m_data.push_back(std::pair<std::string, T>(key, value));
		std::sort(m_data.begin(), m_data.end(), comparer());

		return true;
	}

	void InsertOrUpdate(const char* key, T& value)
	{
		int index = indexOf(key);
		if (index >= 0)
		{
			m_data[index].second = value;
		}
		else
		{
			m_data.push_back(std::pair<std::string, T>(key, value));
			std::sort(m_data.begin(), m_data.end(), comparer());
		}
	}

	bool TryGetValue(const char* key, T& value) const
	{
		int index = indexOf(key);

		if (index >= 0)
		{
			value = m_data[index].second;
			return true;
		}

		return false;
	}



private:

	int indexOf(const char* key) const
	{
		int searchLength = m_data.size() - 1;
		int i = 0;
		while(i <= searchLength)
		{
			int currentIndex = i + (searchLength - i) / 2;

			int comparison = compare(m_data[currentIndex].first.c_str(), key);
			if (comparison == 0)
			{
				// we found it!
				return currentIndex;
			}

			if (comparison < 0)
				i = currentIndex + 1;
			else
				searchLength = currentIndex - 1;
		}

		return -1;
	}

	static int compare(const char* str1, const char* str2)
	{
#ifdef WIN32
		return stricmp(str1, str2);
#else
		return strcasecmp(str1, str2);
#endif
	}
};

#endif // SHEEPSTRINGDICTIONARY_H

#ifndef SHEEPCASEINSENSITIVESTRINGCOMPARE_H
#define SHEEPCASEINSENSITIVESTRINGCOMPARE_H

#include <algorithm>

// this stuff was "borrowed" from http://www.gammon.com.au/forum/bbshowpost.php?bbsubject_id=2902
struct ci_less : std::binary_function<std::string, std::string, bool>
{
	struct nocase_compare : public std::binary_function<unsigned char, unsigned char, bool>
	{
		bool operator()(const unsigned char& c1, const unsigned char& c2) const
		{
			return tolower(c1) < tolower(c2);
		}
	};

	bool operator()(const std::string& s1, const std::string& s2) const
	{
		return std::lexicographical_compare(s1.begin(), s1.end(), s2.begin(), s2.end(), nocase_compare());
	}

	static bool areEqual(const std::string& s1, const std::string& s2)
	{
		ci_less l;
		return l(s1, s2);
	}
};

static bool CIEqual(const std::string& s1, const std::string& s2)
{
	std::string c1(s1);
	std::string c2(s2);

	std::transform(c1.begin(), c1.end(), c1.begin(), tolower);
	std::transform(c2.begin(), c2.end(), c2.begin(), tolower);

	return c1 == c2;
}

#endif // SHEEPCASEINSENSITIVESTRINGCOMPARE_H

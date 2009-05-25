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
};

#endif // SHEEPCASEINSENSITIVESTRINGCOMPARE_H

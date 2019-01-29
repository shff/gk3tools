#ifndef SHEEPCASEINSENSITIVESTRINGCOMPARE_H
#define SHEEPCASEINSENSITIVESTRINGCOMPARE_H

#include <algorithm>

static int CICompare(const std::string& s1, const std::string& s2);

// this stuff was "borrowed" from http://www.gammon.com.au/forum/bbshowpost.php?bbsubject_id=2902
struct ci_less : std::binary_function<std::string, std::string, bool>
{
	bool operator()(const std::string& s1, const std::string& s2) const
	{
		return CICompare(s1, s2) < 0;
	}
};

static bool CIEqual(const std::string& s1, const std::string& s2)
{
	return CICompare(s1, s2) == 0;
}

static int CICompare(const std::string& s1, const std::string& s2)
{
#ifdef WIN32
	return _stricmp(s1.c_str(), s2.c_str());
#else
	return strcasecmp(s1.c_str(), s2.c_str());
#endif
}

#endif // SHEEPCASEINSENSITIVESTRINGCOMPARE_H

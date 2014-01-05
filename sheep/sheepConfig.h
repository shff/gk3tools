#ifndef SHEEPCONFIG_H
#define SHEEPCONFIG_H

#if _MSC_VER >= 1700
#define SHEEP_ENUM(e) enum class e {
#define END_SHEEP_ENUM(e) };
#else
template<typename def, typename inner = typename def::type>
class safe_enum : public def
{
	typedef typename def::type type;
	inner val;
public:
	safe_enum(int v) : val((type)v) {}
	safe_enum(type v) : val(v) {}
	safe_enum() : val((inner)0) {}
	inner underlying() const { return val; }
 
	//bool operator == (const safe_enum & s) const { return this->val == s.val; }
	//bool operator != (const safe_enum & s) const { return this->val != s.val; }
	bool operator <  (const safe_enum & s) const { return this->val <  s.val; }
	bool operator <= (const safe_enum & s) const { return this->val <= s.val; }
	bool operator >  (const safe_enum & s) const { return this->val >  s.val; }
	bool operator >= (const safe_enum & s) const { return this->val >= s.val; }
	operator int() const { return (int)this->val; }
};

#define SHEEP_ENUM(e) struct e##_def { enum type {
#define END_SHEEP_ENUM(e) }; }; typedef safe_enum<e##_def> e;
#endif

#endif // SHEEPCONFIG_H

#ifndef IDENTIFIERTESTS_H
#define IDENTIFIERTESTS_H

#include <cxxtest/TestSuite.h>
#include "../sheepc.h"


class IdentifierTestSuite : public CxxTest::TestSuite
{
public:

	void TestIdentifierCaseInsensitive()
	{
		SheepVM* vm = SHP_CreateNewVM();

		TS_ASSERT_EQUALS(SHP_RunScript(vm, "symbols { int foo$; } code { dummy$() { } }", "dummy$"), SHEEP_SUCCESS);
		TS_ASSERT_EQUALS(SHP_RunScript(vm, "symbols { int foo$; } code { Dummy$() { } }", "dummy$"), SHEEP_SUCCESS);
		TS_ASSERT_EQUALS(SHP_RunScript(vm, "symbols { int foo$; } code { dummy$() { } }", "Dummy$"), SHEEP_SUCCESS);
		TS_ASSERT_EQUALS(SHP_RunScript(vm, "symbols { int foo$; } code { dummy$() { foo$ = 4; } }", "dummy$"), SHEEP_SUCCESS);
		TS_ASSERT_EQUALS(SHP_RunScript(vm, "symbols { int foo$; } code { dummy$() { Foo$ = 4; } }", "dummy$"), SHEEP_SUCCESS);

		SHP_DestroyVM(vm);
	}

	void TestIdentifierCharacters()
	{
		SheepVM* vm = SHP_CreateNewVM();

		TS_ASSERT_EQUALS(SHP_RunScript(vm, "symbols { int foo5$; } code { dummy$() { } }", "dummy$"), SHEEP_SUCCESS);
		TS_ASSERT_EQUALS(SHP_RunScript(vm, "symbols { int _foo_$; } code { dummy$() { } }", "dummy$"), SHEEP_SUCCESS);
		TS_ASSERT_DIFFERS(SHP_RunScript(vm, "symbols { int foo$$; } code { dummy$() { } }", "Dummy$"), SHEEP_SUCCESS);
		TS_ASSERT_DIFFERS(SHP_RunScript(vm, "symbols { int 5foo$; } code { dummy$() { } }", "Dummy$"), SHEEP_SUCCESS);
		TS_ASSERT_DIFFERS(SHP_RunScript(vm, "symbols { int foo[$; } code { dummy$() { } }", "Dummy$"), SHEEP_SUCCESS);
		TS_ASSERT_DIFFERS(SHP_RunScript(vm, "symbols { int foo; } code { dummy$() { } }", "Dummy$"), SHEEP_SUCCESS);

		TS_ASSERT_EQUALS(SHP_RunScript(vm, "code { dummy$() { } }", "dummy$"), SHEEP_SUCCESS);
		TS_ASSERT_DIFFERS(SHP_RunScript(vm, "code { 5dummy$() { } }", "5dummy$"), SHEEP_SUCCESS);
		TS_ASSERT_DIFFERS(SHP_RunScript(vm, "code { dummy() { } }", "dummy"), SHEEP_SUCCESS);
		TS_ASSERT_DIFFERS(SHP_RunScript(vm, "code { d[ummy$() { } }", "d[ummy$"), SHEEP_SUCCESS);

		SHP_DestroyVM(vm);
	}

};

#endif // IDENTIFIERTESTS_H
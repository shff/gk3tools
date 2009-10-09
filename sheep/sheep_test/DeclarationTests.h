#ifndef DECLARATIONTESTS_H
#define DECLARATIONTESTS_H

#include <cxxtest/TestSuite.h>
#include "../sheepc.h"


class DeclarationTestSuite : public CxxTest::TestSuite
{
public:

	void TestMultipleDeclarations()
	{
		SheepVM* vm = SHP_CreateNewVM();

		TS_ASSERT_EQUALS(SHP_RunScript(vm, "symbols { int foo$; float bar$; } code { dummy$() { } }", "dummy$"), SHEEP_SUCCESS);
		TS_ASSERT_EQUALS(SHP_RunScript(vm, "symbols { int foo$, bar$; } code { dummy$() { } }", "dummy$"), SHEEP_SUCCESS);
		TS_ASSERT_EQUALS(SHP_RunScript(vm, "symbols { int foo$ = 1, bar$; } code { dummy$() { } }", "dummy$"), SHEEP_SUCCESS);
		TS_ASSERT_EQUALS(SHP_RunScript(vm, "symbols { int foo$, bar$ = 1; } code { dummy$() { } }", "dummy$"), SHEEP_SUCCESS);

		SHP_DestroyVM(vm);
	}
};

#endif // DECLARATIONTESTS_H
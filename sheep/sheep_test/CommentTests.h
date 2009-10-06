#ifndef COMMENTTESTS_H
#define COMMENTTESTS_H

#include <cxxtest/TestSuite.h>
#include "../sheepc.h"


class CommentTestSuite : public CxxTest::TestSuite
{
public:

	void TestInlineComment()
	{
		SheepVM* vm = SHP_CreateNewVM();

		const char* script =
			"symbols { "
			" // this is a test\n"
			" int foo$; // and another test\n"
			"} // comment goes here too\n"
			"code // comment /* comment comment \n"
			"{ dummy$() // */ blah\n"
			"{ foo$ = // woah!\n"
			"5; } }";

		TS_ASSERT_EQUALS(SHP_RunScript(vm, script, "dummy$"), SHEEP_SUCCESS);

		SHP_DestroyVM(vm);
	}

	void TestBlockComment()
	{
		SheepVM* vm = SHP_CreateNewVM();

		const char* script =
			"symbols { "
			" /* this is a test */ "
			" int foo$; /* and another test */"
			"} /* comment goes here too */"
			"code /* // comment inside comment */"
			"{ dummy$() /* blah */"
			"{ foo$ = /* woah! */"
			"5; } }";

		TS_ASSERT_EQUALS(SHP_RunScript(vm, script, "dummy$"), SHEEP_SUCCESS);

		SHP_DestroyVM(vm);
	}

};

#endif // COMMENTTESTS_H

#ifndef OPERATORTESTS_H
#define OPERATORTESTS_H

#include <cxxtest/TestSuite.h>
#include "../sheepc.h"

class OperatorTestSuite : public CxxTest::TestSuite
{
public:
	void testIntOperationOrder(void)
	{
		SheepVM* vm = SHP_CreateNewVM();

		int dummy;
		TS_ASSERT_EQUALS(SHP_RunSnippet(vm, "snippet { 1 + 2 * 3 + 4 }", &dummy), SHEEP_SUCCESS);
		TS_ASSERT_EQUALS(SHP_GetNumContexts(vm), 0);
		TS_ASSERT_EQUALS(dummy, 11);

		TS_ASSERT_EQUALS(SHP_RunSnippet(vm, "snippet { 10 + 20 / 2 + 10}", &dummy), SHEEP_SUCCESS);
		TS_ASSERT_EQUALS(SHP_GetNumContexts(vm), 0);
		TS_ASSERT_EQUALS(dummy, 30);

		TS_ASSERT_EQUALS(SHP_RunSnippet(vm, "snippet { 10 - 20 / 2 - 10}", &dummy), SHEEP_SUCCESS);
		TS_ASSERT_EQUALS(SHP_GetNumContexts(vm), 0);
		TS_ASSERT_EQUALS(dummy, -10);

		SHP_DestroyVM(vm);
	}

	void testFloatOperationOrder()
	{
		SheepVM* vm = SHP_CreateNewVM();

		int dummy;
		TS_ASSERT_EQUALS(SHP_RunSnippet(vm, "snippet { 5.5 + 500*2.5 }", &dummy), SHEEP_SUCCESS);
		TS_ASSERT_EQUALS(SHP_GetNumContexts(vm), 0);
		TS_ASSERT_EQUALS(dummy, 1255);

		TS_ASSERT_EQUALS(SHP_RunSnippet(vm, "snippet { 10 + 500 * 0.5 + 10}", &dummy), SHEEP_SUCCESS);
		TS_ASSERT_EQUALS(SHP_GetNumContexts(vm), 0);
		TS_ASSERT_EQUALS(dummy, 270);

		TS_ASSERT_EQUALS(SHP_RunSnippet(vm, "snippet { 10 - 500 / 0.1 - 10}", &dummy), SHEEP_SUCCESS);
		TS_ASSERT_EQUALS(SHP_GetNumContexts(vm), 0);
		TS_ASSERT_EQUALS(dummy, -5000);

		SHP_DestroyVM(vm);
	}

	void testBooleanOperations()
	{
		SheepVM* vm = SHP_CreateNewVM();

		int dummy;
		TS_ASSERT_EQUALS(SHP_RunSnippet(vm, "snippet { 1 == 1 }", &dummy), SHEEP_SUCCESS);
		TS_ASSERT_EQUALS(SHP_GetNumContexts(vm), 0);
		TS_ASSERT_EQUALS(dummy, 1);

		TS_ASSERT_EQUALS(SHP_RunSnippet(vm, "snippet { 1 == 0 }", &dummy), SHEEP_SUCCESS);
		TS_ASSERT_EQUALS(SHP_GetNumContexts(vm), 0);
		TS_ASSERT_EQUALS(dummy, 0);

		TS_ASSERT_EQUALS(SHP_RunSnippet(vm, "snippet { 1 > 0 }", &dummy), SHEEP_SUCCESS);
		TS_ASSERT_EQUALS(SHP_GetNumContexts(vm), 0);
		TS_ASSERT_EQUALS(dummy, 1);

		TS_ASSERT_EQUALS(SHP_RunSnippet(vm, "snippet { 1 < 0 }", &dummy), SHEEP_SUCCESS);
		TS_ASSERT_EQUALS(SHP_GetNumContexts(vm), 0);
		TS_ASSERT_EQUALS(dummy, 0);

		TS_ASSERT_EQUALS(SHP_RunSnippet(vm, "snippet { 1 >= 0 }", &dummy), SHEEP_SUCCESS);
		TS_ASSERT_EQUALS(SHP_GetNumContexts(vm), 0);
		TS_ASSERT_EQUALS(dummy, 1);

		TS_ASSERT_EQUALS(SHP_RunSnippet(vm, "snippet { 1 <= 0 }", &dummy), SHEEP_SUCCESS);
		TS_ASSERT_EQUALS(SHP_GetNumContexts(vm), 0);
		TS_ASSERT_EQUALS(dummy, 0);

		TS_ASSERT_EQUALS(SHP_RunSnippet(vm, "snippet { 5.5 > 5.4 }", &dummy), SHEEP_SUCCESS);
		TS_ASSERT_EQUALS(SHP_GetNumContexts(vm), 0);
		TS_ASSERT_EQUALS(dummy, 1);

		TS_ASSERT_EQUALS(SHP_RunSnippet(vm, "snippet { 5.5 > 5 }", &dummy), SHEEP_SUCCESS);
		TS_ASSERT_EQUALS(SHP_GetNumContexts(vm), 0);
		TS_ASSERT_EQUALS(dummy, 1);

		TS_ASSERT_EQUALS(SHP_RunSnippet(vm, "snippet { 5 < 5.4 }", &dummy), SHEEP_SUCCESS);
		TS_ASSERT_EQUALS(SHP_GetNumContexts(vm), 0);
		TS_ASSERT_EQUALS(dummy, 1);

		TS_ASSERT_EQUALS(SHP_RunSnippet(vm, "snippet { 5 < 4.4 }", &dummy), SHEEP_SUCCESS);
		TS_ASSERT_EQUALS(SHP_GetNumContexts(vm), 0);
		TS_ASSERT_EQUALS(dummy, 0);

		SHP_DestroyVM(vm);
	}


	void testConditionalOperators()
	{
		SheepVM* vm = SHP_CreateNewVM();

		int dummy;
		TS_ASSERT_EQUALS(SHP_RunSnippet(vm, "snippet { 1 && 0 }", &dummy), SHEEP_SUCCESS);
		TS_ASSERT_EQUALS(SHP_GetNumContexts(vm), 0);
		TS_ASSERT_EQUALS(dummy, 0);

		TS_ASSERT_EQUALS(SHP_RunSnippet(vm, "snippet { 1 && 1 }", &dummy), SHEEP_SUCCESS);
		TS_ASSERT_EQUALS(SHP_GetNumContexts(vm), 0);
		TS_ASSERT_EQUALS(dummy, 1);

		TS_ASSERT_EQUALS(SHP_RunSnippet(vm, "snippet { 0 && 0 }", &dummy), SHEEP_SUCCESS);
		TS_ASSERT_EQUALS(SHP_GetNumContexts(vm), 0);
		TS_ASSERT_EQUALS(dummy, 0);

		TS_ASSERT_EQUALS(SHP_RunSnippet(vm, "snippet { 1 || 0 }", &dummy), SHEEP_SUCCESS);
		TS_ASSERT_EQUALS(SHP_GetNumContexts(vm), 0);
		TS_ASSERT_EQUALS(dummy, 1);

		TS_ASSERT_EQUALS(SHP_RunSnippet(vm, "snippet { 1 || 1 }", &dummy), SHEEP_SUCCESS);
		TS_ASSERT_EQUALS(SHP_GetNumContexts(vm), 0);
		TS_ASSERT_EQUALS(dummy, 1);

		TS_ASSERT_EQUALS(SHP_RunSnippet(vm, "snippet { 0 || 0 }", &dummy), SHEEP_SUCCESS);
		TS_ASSERT_EQUALS(SHP_GetNumContexts(vm), 0);
		TS_ASSERT_EQUALS(dummy, 0);

		TS_ASSERT_EQUALS(SHP_RunSnippet(vm, "snippet { 1 && 0 || 1 }", &dummy), SHEEP_SUCCESS);
		TS_ASSERT_EQUALS(SHP_GetNumContexts(vm), 0);
		TS_ASSERT_EQUALS(dummy, 1);

		TS_ASSERT_EQUALS(SHP_RunSnippet(vm, "snippet { 0 && 1 || 0 }", &dummy), SHEEP_SUCCESS);
		TS_ASSERT_EQUALS(SHP_GetNumContexts(vm), 0);
		TS_ASSERT_EQUALS(dummy, 0);

		SHP_DestroyVM(vm);
	}
};


#endif // OPERATORTESTS_H

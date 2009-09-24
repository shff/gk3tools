#include <cassert>
#include <cstdio>
#include "../sheepc.h"

#ifndef NULL
#define NULL 0
#endif

#define BEGIN_TEST(n) void n() { bool this_test_failed = false; const char* this_test_name = #n; printf("Running "); printf(this_test_name); printf("...\n");
#define END_TEST printf(this_test_name); if (this_test_failed) printf(" failed!\n\n"); else printf(" successful\n\n"); }
#define TEST_ASSERT(c) if (!c) { printf(#c); printf(" failed!\n"); this_test_failed = true; }

void CALLBACK intCallback(SheepVM* vm)
{
	SHP_PushIntOntoStack(vm, 0);
}


BEGIN_TEST(basicSnippetTest)
	SheepVM* vm = SHP_CreateNewVM();

	int dummy;
	TEST_ASSERT(SHP_RunSnippet(vm, "snippet { foo() }", &dummy) != SHEEP_SUCCESS);
	TEST_ASSERT(SHP_GetNumContexts(vm) == 0);

	SHP_AddImport(vm, "foo", Int, intCallback);

	TEST_ASSERT(SHP_RunSnippet(vm, "snippet { foo() }", &dummy) == SHEEP_SUCCESS);
	TEST_ASSERT(SHP_GetNumContexts(vm) == 0);
	TEST_ASSERT(dummy == 0);

	SHP_DestroyVM(vm);
END_TEST

BEGIN_TEST(intOperationOrderTest)
	SheepVM* vm = SHP_CreateNewVM();

	int dummy;
	TEST_ASSERT(SHP_RunSnippet(vm, "snippet { 1 + 2 * 3 + 4 }", &dummy) == SHEEP_SUCCESS);
	TEST_ASSERT(SHP_GetNumContexts(vm) == 0);
	TEST_ASSERT(dummy == 11);

	TEST_ASSERT(SHP_RunSnippet(vm, "snippet { 10 + 20 / 2 + 10}", &dummy) == SHEEP_SUCCESS);
	TEST_ASSERT(SHP_GetNumContexts(vm) == 0);
	TEST_ASSERT(dummy == 30);

	TEST_ASSERT(SHP_RunSnippet(vm, "snippet { 10 - 20 / 2 - 10}", &dummy) == SHEEP_SUCCESS);
	TEST_ASSERT(SHP_GetNumContexts(vm) == 0);
	TEST_ASSERT(dummy == -10);

	SHP_DestroyVM(vm);
END_TEST

BEGIN_TEST(floatOperationOrderTest)
	SheepVM* vm = SHP_CreateNewVM();

	int dummy;
	TEST_ASSERT(SHP_RunSnippet(vm, "snippet { 5.5 + 500*2.5 }", &dummy) == SHEEP_SUCCESS);
	TEST_ASSERT(SHP_GetNumContexts(vm) == 0);
	TEST_ASSERT(dummy == 1255);

	TEST_ASSERT(SHP_RunSnippet(vm, "snippet { 10 + 500 * 0.5 + 10}", &dummy) == SHEEP_SUCCESS);
	TEST_ASSERT(SHP_GetNumContexts(vm) == 0);
	TEST_ASSERT(dummy == 270);

	TEST_ASSERT(SHP_RunSnippet(vm, "snippet { 10 - 500 / 0.1 - 10}", &dummy) == SHEEP_SUCCESS);
	TEST_ASSERT(SHP_GetNumContexts(vm) == 0);
	TEST_ASSERT(dummy == -5000);

	SHP_DestroyVM(vm);
END_TEST

BEGIN_TEST(booleanTest)
	SheepVM* vm = SHP_CreateNewVM();

	int dummy;
	TEST_ASSERT(SHP_RunSnippet(vm, "snippet { 1 == 1 }", &dummy) == SHEEP_SUCCESS);
	TEST_ASSERT(SHP_GetNumContexts(vm) == 0);
	TEST_ASSERT(dummy == 1);

	TEST_ASSERT(SHP_RunSnippet(vm, "snippet { 1 == 0 }", &dummy) == SHEEP_SUCCESS);
	TEST_ASSERT(SHP_GetNumContexts(vm) == 0);
	TEST_ASSERT(dummy == 0);

	TEST_ASSERT(SHP_RunSnippet(vm, "snippet { 1 > 0 }", &dummy) == SHEEP_SUCCESS);
	TEST_ASSERT(SHP_GetNumContexts(vm) == 0);
	TEST_ASSERT(dummy == 1);

	TEST_ASSERT(SHP_RunSnippet(vm, "snippet { 1 < 0 }", &dummy) == SHEEP_SUCCESS);
	TEST_ASSERT(SHP_GetNumContexts(vm) == 0);
	TEST_ASSERT(dummy == 0);

	TEST_ASSERT(SHP_RunSnippet(vm, "snippet { 1 >= 0 }", &dummy) == SHEEP_SUCCESS);
	TEST_ASSERT(SHP_GetNumContexts(vm) == 0);
	TEST_ASSERT(dummy == 1);

	TEST_ASSERT(SHP_RunSnippet(vm, "snippet { 1 <= 0 }", &dummy) == SHEEP_SUCCESS);
	TEST_ASSERT(SHP_GetNumContexts(vm) == 0);
	TEST_ASSERT(dummy == 0);

	TEST_ASSERT(SHP_RunSnippet(vm, "snippet { 5.5 > 5.4 }", &dummy) == SHEEP_SUCCESS);
	TEST_ASSERT(SHP_GetNumContexts(vm) == 0);
	TEST_ASSERT(dummy == 1);

	TEST_ASSERT(SHP_RunSnippet(vm, "snippet { 5.5 > 5 }", &dummy) == SHEEP_SUCCESS);
	TEST_ASSERT(SHP_GetNumContexts(vm) == 0);
	TEST_ASSERT(dummy == 1);

	TEST_ASSERT(SHP_RunSnippet(vm, "snippet { 5 < 5.4 }", &dummy) == SHEEP_SUCCESS);
	TEST_ASSERT(SHP_GetNumContexts(vm) == 0);
	TEST_ASSERT(dummy == 1);

	TEST_ASSERT(SHP_RunSnippet(vm, "snippet { 5 < 4.4 }", &dummy) == SHEEP_SUCCESS);
	TEST_ASSERT(SHP_GetNumContexts(vm) == 0);
	TEST_ASSERT(dummy == 0);

	SHP_DestroyVM(vm);
END_TEST

BEGIN_TEST(conditionalTest)

	SheepVM* vm = SHP_CreateNewVM();

	int dummy;
	TEST_ASSERT(SHP_RunSnippet(vm, "snippet { 1 && 0 }", &dummy) == SHEEP_SUCCESS);
	TEST_ASSERT(SHP_GetNumContexts(vm) == 0);
	TEST_ASSERT(dummy == 0);

	TEST_ASSERT(SHP_RunSnippet(vm, "snippet { 1 && 1 }", &dummy) == SHEEP_SUCCESS);
	TEST_ASSERT(SHP_GetNumContexts(vm) == 0);
	TEST_ASSERT(dummy == 1);

	TEST_ASSERT(SHP_RunSnippet(vm, "snippet { 0 && 0 }", &dummy) == SHEEP_SUCCESS);
	TEST_ASSERT(SHP_GetNumContexts(vm) == 0);
	TEST_ASSERT(dummy == 0);

	TEST_ASSERT(SHP_RunSnippet(vm, "snippet { 1 || 0 }", &dummy) == SHEEP_SUCCESS);
	TEST_ASSERT(SHP_GetNumContexts(vm) == 0);
	TEST_ASSERT(dummy == 1);

	TEST_ASSERT(SHP_RunSnippet(vm, "snippet { 1 || 1 }", &dummy) == SHEEP_SUCCESS);
	TEST_ASSERT(SHP_GetNumContexts(vm) == 0);
	TEST_ASSERT(dummy == 1);

	TEST_ASSERT(SHP_RunSnippet(vm, "snippet { 0 || 0 }", &dummy) == SHEEP_SUCCESS);
	TEST_ASSERT(SHP_GetNumContexts(vm) == 0);
	TEST_ASSERT(dummy == 0);

	TEST_ASSERT(SHP_RunSnippet(vm, "snippet { 1 && 0 || 1 }", &dummy) == SHEEP_SUCCESS);
	TEST_ASSERT(SHP_GetNumContexts(vm) == 0);
	TEST_ASSERT(dummy == 1);

	TEST_ASSERT(SHP_RunSnippet(vm, "snippet { 0 && 1 || 0 }", &dummy) == SHEEP_SUCCESS);
	TEST_ASSERT(SHP_GetNumContexts(vm) == 0);
	TEST_ASSERT(dummy == 0);

	SHP_DestroyVM(vm);

END_TEST

int main()
{
	basicSnippetTest();
	intOperationOrderTest();
	floatOperationOrderTest();
	booleanTest();
	conditionalTest();

	// TODO: there needs to me FAR more tests!

	return 0;
}
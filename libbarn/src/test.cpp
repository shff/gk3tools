/* Generated file, do not edit */

#ifndef CXXTEST_RUNNING
#define CXXTEST_RUNNING
#endif

#define _CXXTEST_HAVE_STD
#include <cxxtest/TestListener.h>
#include <cxxtest/TestTracker.h>
#include <cxxtest/TestRunner.h>
#include <cxxtest/RealDescriptions.h>
#include <cxxtest/ErrorPrinter.h>

int main() {
 return CxxTest::ErrorPrinter().run();
}
#include "tests.h"

static BarnTestSuite *suite_BarnTestSuite = 0;

static CxxTest::List Tests_BarnTestSuite = { 0, 0 };
CxxTest::DynamicSuiteDescription<BarnTestSuite> suiteDescription_BarnTestSuite( "tests.h", 27, "BarnTestSuite", Tests_BarnTestSuite, suite_BarnTestSuite, 73, 74 );

static class TestDescription_BarnTestSuite_TestGetNumberOfFiles : public CxxTest::RealTestDescription {
public:
 TestDescription_BarnTestSuite_TestGetNumberOfFiles() : CxxTest::RealTestDescription( Tests_BarnTestSuite, suiteDescription_BarnTestSuite, 50, "TestGetNumberOfFiles" ) {}
 void runTest() { if ( suite_BarnTestSuite ) suite_BarnTestSuite->TestGetNumberOfFiles(); }
} testDescription_BarnTestSuite_TestGetNumberOfFiles;

static class TestDescription_BarnTestSuite_TestExtractByIndex : public CxxTest::RealTestDescription {
public:
 TestDescription_BarnTestSuite_TestExtractByIndex() : CxxTest::RealTestDescription( Tests_BarnTestSuite, suiteDescription_BarnTestSuite, 55, "TestExtractByIndex" ) {}
 void runTest() { if ( suite_BarnTestSuite ) suite_BarnTestSuite->TestExtractByIndex(); }
} testDescription_BarnTestSuite_TestExtractByIndex;

static class TestDescription_BarnTestSuite_TestReadFile : public CxxTest::RealTestDescription {
public:
 TestDescription_BarnTestSuite_TestReadFile() : CxxTest::RealTestDescription( Tests_BarnTestSuite, suiteDescription_BarnTestSuite, 64, "TestReadFile" ) {}
 void runTest() { if ( suite_BarnTestSuite ) suite_BarnTestSuite->TestReadFile(); }
} testDescription_BarnTestSuite_TestReadFile;

#include <cxxtest/Root.cpp>

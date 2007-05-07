// Copyright (c) 2006 Brad Farris
// This file is licensed under the MIT license. You can do whatever you
// want with this file as long as this notice remains at the top.

// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#include <iostream>
#include <cxxtest/TestSuite.h>

#ifdef WIN32
#include <windows.h>
#endif

#include "barn.h"
#include "barn_internal.h"


bool fileExists(const std::string& path)
{
	std::ifstream file;

	file.open(path.c_str(), std::ifstream::in);

	if (file.is_open())
	{
		file.close();
		return true;
	}

	return false;
}

class BarnTestSuite : public CxxTest::TestSuite
{
public:

	BarnTestSuite()
	{
		// make sure the ./teststuff directory exists
#ifdef WIN32
		CreateDirectory("teststuff", NULL);
#endif

	}

	void setUp()
	{
		_barn = new Barn::Barn("core.brn", "core.brn");
	}

	void tearDown()
	{
		delete _barn;
	}

	void TestGetNumberOfFiles()
	{
		TS_ASSERT_EQUALS(_barn->GetNumberOfFiles(), 36957);
	}

	void TestExtractByIndex()
	{
		TS_ASSERT_EQUALS(_barn->ExtractFileByIndex(1, "", false, true), BARN_SUCCESS);
		TS_ASSERT(fileExists("205PEND.YAK"));

		TS_ASSERT_EQUALS(_barn->ExtractFileByIndex(1, "teststuff/", false, true), BARN_SUCCESS);
		TS_ASSERT(fileExists("teststuff/205PEND.YAK"));
	}

	void TestReadFile()
	{
		Barn::ExtractBuffer* buffer = _barn->ReadFile("50FRANC.BMP", true, false);

		TS_ASSERT_EQUALS(buffer->GetSize(), 16392);

		delete buffer;
	}

	static BarnTestSuite *createSuite() { return new BarnTestSuite(); }
    static void destroySuite( BarnTestSuite *suite ) { delete suite; }

private:

	Barn::Barn* _barn;
};
@echo off

set PYTHON=python.exe
set CXXTEST_PATH=c:\users\bfarris\Documents\code\cxxtest\cxxtest

%PYTHON% %CXXTEST_PATH%\cxxtestgen.py --error-printer -o runner.cpp OperatorTests.h IdentifierTests.h DeclarationTests.h CommentTests.h

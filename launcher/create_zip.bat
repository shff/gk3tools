@echo off

set zip="c:\program files\7-zip\7z.exe"
set libs="C:\Program Files (x86)\Microsoft Visual Studio 8\VC\redist\x86\Microsoft.VC80.CRT"
set tempdir="Gk3Launcher"

rem Create a temporary directory
mkdir %tempdir%

rem Create the Source directory
mkdir %tempdir%\Source

rem Copy source files into the Source directory
copy *.cpp %tempdir%\Source
copy *.h %tempdir%\Source
copy *.vcproj %tempdir%\Source
copy *.sln %tempdir%\Source
copy *.rc %tempdir%\Source

rem Copy the binary files
copy Release\Gk3launcher.exe %tempdir%
copy *.txt %tempdir%


rem Zip up the temporary directory
%zip% a Gk3Launcher.zip %tempdir%\*

rem Remove the temporary directory
del /F /Q %tempdir%\Source
del /F /Q %tempdir%
rmdir %tempdir%\Source
rmdir %tempdir%
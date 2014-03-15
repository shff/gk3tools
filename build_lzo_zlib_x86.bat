@echo off
rem msbuild build.xml

set LzoFile=lzo-2.05
set LzoUrl=http://www.oberhumer.com/opensource/lzo/download/%LzoFile%.tar.gz
set ZLibFile=zlib-1.2.8
set ZLibUrl=http://zlib.net/%ZLibFile%.tar.gz

rem create the Lib folder
mkdir Lib

rem build the magic builder tool
msbuild builder\Builder2010.csproj

rem download LZO
echo Downloading LZO...
Builder.exe -d %LzoUrl%

rem download ZLib
echo Downloading ZLib...
Builder.exe -d %ZLibUrl%

rem move the files into the Lib folder
move %LzoFile%.tar.gz Lib\%LzoFile%.tar.gz
move %ZLibFile%.tar.gz Lib\%ZLibFile%.tar.gz

rem extract LZO and ZLib
echo Extracting LZO...
cd Lib
..\Builder.exe -x %LzoFile%.tar.gz

echo Extracting ZLib...
..\Builder.exe -x %ZLibFile%.tar.gz

move %LzoFile% lzo
move %ZLibFile% zlib

rem build LZO and ZLib
echo Building LZO...
cd lzo
call b\win32\vc_dll.bat
cd ..

echo Building ZLib...
cd zlib\contrib\masmx86
call bld_ml32.bat
cd ..\vstudio\vc11
msbuild zlibstat.vcxproj /p:Configuration=Release
cd ..\..\..\..\

mkdir lzo\build\windows\x86
mkdir zlib\build\windows\x86

xcopy /Y lzo\lzo2.lib lzo\build\windows\x86
xcopy /Y lzo\lzo2.dll lzo\build\windows\x86
xcopy /Y zlib\contrib\vstudio\vc11\x86\ZlibStatRelease\zlibstat.lib zlib\build\windows\x86

cd ..
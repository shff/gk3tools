@echo off

rem this assumes build_everything.bat ran successfully and built the x86 versions of LZO and ZLib.
rem this script must be run with the VS2012 x64 Native Tools prompt for it to work correctly.

cd Lib
mkdir lzo\build\windows\x64
mkdir zlib\build\windows\x64

echo Building LZO...
cd lzo
call b\win64\vc_dll.bat
cd ..

echo Building ZLib...
cd zlib\contrib\masmx64
call bld_ml64.bat
cd ..\vstudio\vc11
msbuild zlibstat.vcxproj /p:Configuration=Release /p:Platform=x64
cd ..\..\..\..\

rem copy the built files into the proper place
echo Copying files...
xcopy /Y lzo\lzo2.lib lzo\build\windows\x64
xcopy /Y lzo\lzo2.dll lzo\build\windows\x64
xcopy /Y zlib\contrib\vstudio\vc11\x64\ZlibStatRelease\zlibstat.lib zlib\build\windows\x64


cd ..
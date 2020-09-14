@echo off
set cur_path=%CD%
call "C:\Program Files (x86)\Microsoft Visual Studio 10.0\VC\vcvarsall.bat" x86
rem call "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\VC\Auxiliary\Build\vcvarsall.bat" x86

"C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe" "%cur_path%\QTTabBar Rebirth.sln" /t:Rebuild /property:Configuration=Release /property:Platform="Any CPU"
pause
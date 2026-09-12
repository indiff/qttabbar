@echo off
set cur_path=%CD%
call "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\VC\Auxiliary\Build\vcvarsall.bat" x86

"%cur_path%\sonar-scanner-msbuild-4.9.0.17385-net46\SonarScanner.MSBuild.exe" begin /k:"qttabbar" /d:sonar.host.url="http://192.168.1.146:9000" /d:sonar.login="4e91ad1959c5f60eba7aa47266c0fca9c65e33ed"

"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" "%cur_path%\QTTabBar Rebirth.sln" /t:Rebuild /property:Configuration=Release /property:Platform="Any CPU"

"%cur_path%\sonar-scanner-msbuild-4.9.0.17385-net46\SonarScanner.MSBuild.exe" end /d:sonar.login="4e91ad1959c5f60eba7aa47266c0fca9c65e33ed"

pause
@echo off
set cur_path=%CD%
rem call "C:\Program Files (x86)\Microsoft Visual Studio 10.0\VC\vcvarsall.bat" x86
call "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\VC\Auxiliary\Build\vcvarsall.bat" x86

rem  C:\Program Files (x86)\WiX Toolset v3.11\bin\Light.exe -out "D:\SVN\qttabbar2019\Installer\bin\Release\zh-CN\QTTabBar Setup.msi" -pdbout "D:\SVN\qttabbar2019\Installer\bin\Release\zh-CN\QTTabBar Setup.wixpdb" -sw1076 -sw1118 -cultures:zh-CN -ext "C:\Program Files (x86)\WiX Toolset v3.11\bin\\WixNetFxExtension.dll" -ext "C:\Program Files (x86)\WiX Toolset v3.11\bin\\WixUIExtension.dll" -fv -loc lang.wxl -loc lang_zh_CN.wxl -sice:ICE80 -contentsfile obj\Release\Installer.wixproj.BindContentsFileListzh-CN.txt -outputsfile obj\Release\Installer.wixproj.BindOutputsFileListzh-CN.txt -builtoutputsfile obj\Release\Installer.wixproj.BindBuiltOutputsFileListzh-CN.txt -wixprojectfile D:\SVN\qttabbar2019\Installer\Installer.wixproj obj\Release\CustomWelcomeEulaDlg.wixobj obj\Release\Installer.wixobj obj\Release\CustomWixUI_Minimal.wixobj

"C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe" "%cur_path%\QTTabBar Rebirth.sln" /t:Rebuild /property:Configuration=Release /property:Platform="Any CPU"
pause
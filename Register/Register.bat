echo off

rem call VCVarsQueryRegistry.bat 32bit 64bit
rem call "D:\Visual Studio 2010 Ultimate\Common7\Tools\VCVarsQueryRegistry.bat" 32bit 64bit
call "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\Tools\VsDevCmd.bat" x64
cd ..\QTTabBar\bin\Release
IF EXIST QTTabBar.dll (
    gacutil /if QTTabBar.dll
    call %FrameworkDir32%\%FrameworkVersion32%\regasm.exe QTTabBar.dll
    if not "%FrameworkDir64%"=="" (
        call %FrameworkDir64%\%FrameworkVersion64%\regasm.exe QTTabBar.dll
    )
)
cd ..\..\

cd ..\QTPluginLib\bin\Release
IF EXIST QTPluginLib.dll (
    gacutil /if QTPluginLib.dll
)

cd ..\..\..\BandObjectLib\bin\Release
IF EXIST BandObjectLib.dll (
    gacutil /if BandObjectLib.dll
)
IF EXIST Interop.SHDocVw.dll (
    gacutil /if Interop.SHDocVw.dll
)

cd ..\..\..\QTHookLib\bin\Release
REG ADD HKLM\SOFTWARE\QTTabBar /v InstallPath /t REG_SZ /d "%cd%" /f /reg:32
REG ADD HKLM\SOFTWARE\QTTabBar /v InstallPath /t REG_SZ /d "%cd%" /f /reg:64

cd ..\..\..\Register

rem taskkill /f /im explorer.exe
rem start explorer.exe

rem timeout /nobreak /t 5

rem cmd.exe /c start taskmgr
taskmgr
pause
rem start cmd.exe

rem cmd cmd /k

rem cmd /c start explorer.exe


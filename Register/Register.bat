echo off

rem call VCVarsQueryRegistry.bat 32bit 64bit
call "D:\Visual Studio 2010 Ultimate\Common7\Tools\VCVarsQueryRegistry.bat" 32bit 64bit
cd ..\QTTabBar\bin\%1
IF EXIST QTTabBar.dll (
    gacutil /if QTTabBar.dll
    call %FrameworkDir32%\%FrameworkVersion32%\regasm.exe QTTabBar.dll
    if not "%FrameworkDir64%"=="" (
        call %FrameworkDir64%\%FrameworkVersion64%\regasm.exe QTTabBar.dll
    )
)
cd ..\..\

cd ..\QTPluginLib\bin\%1
IF EXIST QTPluginLib.dll (
    gacutil /if QTPluginLib.dll
)

cd ..\..\..\BandObjectLib\bin\%1
IF EXIST BandObjectLib.dll (
    gacutil /if BandObjectLib.dll
)
IF EXIST Interop.SHDocVw.dll (
    gacutil /if Interop.SHDocVw.dll
)

cd ..\..\..\QTHookLib\bin\%1
REG ADD HKLM\SOFTWARE\QTTabBar /v InstallPath /t REG_SZ /d "%cd%" /f /reg:32
REG ADD HKLM\SOFTWARE\QTTabBar /v InstallPath /t REG_SZ /d "%cd%" /f /reg:64

cd ..\..\..\Register

rem taskkill /f /im explorer.exe
rem start explorer.exe

rem timeout /nobreak /t 5

rem cmd.exe /c start taskmgr
start taskmgr
exit 0
rem start cmd.exe

rem cmd cmd /k

rem cmd /c start explorer.exe


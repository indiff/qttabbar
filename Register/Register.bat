echo off

call VCVarsQueryRegistry.bat 32bit 64bit
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
REG ADD HKLM\SOFTWARE\Quizo\QTTabBar /v InstallPath /t REG_SZ /d "%cd%" /f /reg:32
REG ADD HKLM\SOFTWARE\Quizo\QTTabBar /v InstallPath /t REG_SZ /d "%cd%" /f /reg:64

cd ..\..\..\Register
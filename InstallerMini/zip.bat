@echo off
cd /d %~dp0
set VZ=v1.5.5-beta.8_
set SUFFIX=.2022.zip
rem if exist ".bin\Release\zh-CN\QTTabBar Setup.QTTabBarSetup(No Plugins).msi" (
del /f /q "%cd%\bin\Release\zh-CN\QTTabBar.Setup_%VZ%" 
del /f /q "%cd%\bin\Release\en-US\QTTabBar.Setup_%VZ%" 
del /f /q "%cd%\bin\Release\de-DE\QTTabBar.Setup_%VZ%" 
del /f /q "%cd%\bin\Release\es-ES\QTTabBar.Setup_%VZ%" 
del /f /q "%cd%\bin\Release\pt-BR\QTTabBar.Setup_%VZ%" 
del /f /q "%cd%\bin\Release\ru-RU\QTTabBar.Setup_%VZ%" 
"C:\Program Files\Bandizip\bz.exe" a -aoa -l:9 "%cd%\bin\Release\zh-CN\QTTabBar.Setup(No Plugins)_%VZ%zh%SUFFIX%" "%cd%\bin\Release\zh-CN\QTTabBarSetup(No Plugins).msi" 
"C:\Program Files\Bandizip\bz.exe" a -aoa "%cd%\bin\Release\en-US\QTTabBar.Setup(No Plugins)_%VZ%en%SUFFIX%" "%cd%\bin\Release\en-US\QTTabBarSetup(No Plugins).msi" 
"C:\Program Files\Bandizip\bz.exe" a -aoa "%cd%\bin\Release\de-DE\QTTabBar.Setup(No Plugins)_%VZ%de%SUFFIX%" "%cd%\bin\Release\de-DE\QTTabBarSetup(No Plugins).msi" 
"C:\Program Files\Bandizip\bz.exe" a -aoa "%cd%\bin\Release\es-ES\QTTabBar.Setup(No Plugins)_%VZ%es%SUFFIX%" "%cd%\bin\Release\es-ES\QTTabBarSetup(No Plugins).msi" 
"C:\Program Files\Bandizip\bz.exe" a -aoa "%cd%\bin\Release\pt-BR\QTTabBar.Setup(No Plugins)_%VZ%pt%SUFFIX%" "%cd%\bin\Release\pt-BR\QTTabBarSetup(No Plugins).msi" 
"C:\Program Files\Bandizip\bz.exe" a -aoa "%cd%\bin\Release\tr-TR\QTTabBar.Setup(No Plugins)_%VZ%tr%SUFFIX%" "%cd%\bin\Release\tr-TR\QTTabBarSetup(No Plugins).msi" 
"C:\Program Files\Bandizip\bz.exe" a -aoa "%cd%\bin\Release\ru-RU\QTTabBar.Setup(No Plugins)_%VZ%ru%SUFFIX%" "%cd%\bin\Release\ru-RU\QTTabBarSetup(No Plugins).msi" 
pause
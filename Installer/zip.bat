@echo off
cd /d %~dp0
set VZ=v1.5.5-beta.8_
set SUFFIX=.2022.zip
rem if exist ".bin\Release\zh-CN\QTTabBar Setup.msi" (
del /f /q "%cd%\bin\Release\zh-CN\QTTabBar.Setup_%VZ%" 
del /f /q "%cd%\bin\Release\en-US\QTTabBar.Setup_%VZ%" 
del /f /q "%cd%\bin\Release\de-DE\QTTabBar.Setup_%VZ%" 
del /f /q "%cd%\bin\Release\es-ES\QTTabBar.Setup_%VZ%" 
del /f /q "%cd%\bin\Release\pt-BR\QTTabBar.Setup_%VZ%" 
del /f /q "%cd%\bin\Release\tr-TR\QTTabBar.Setup_%VZ%" 
del /f /q "%cd%\bin\Release\ru-RU\QTTabBar.Setup_%VZ%" 
"C:\Program Files\Bandizip\bz.exe" a -aoa -l:9 "%cd%\bin\Release\zh-CN\QTTabBar.Setup_%VZ%zh%SUFFIX%" "%cd%\bin\Release\zh-CN\QTTabBar Setup.msi" 
"C:\Program Files\Bandizip\bz.exe" a -aoa "%cd%\bin\Release\en-US\QTTabBar.Setup_%VZ%en%SUFFIX%" "%cd%\bin\Release\en-US\QTTabBar Setup.msi" 
"C:\Program Files\Bandizip\bz.exe" a -aoa "%cd%\bin\Release\de-DE\QTTabBar.Setup_%VZ%de%SUFFIX%" "%cd%\bin\Release\de-DE\QTTabBar Setup.msi" 
"C:\Program Files\Bandizip\bz.exe" a -aoa "%cd%\bin\Release\es-ES\QTTabBar.Setup_%VZ%es%SUFFIX%" "%cd%\bin\Release\es-ES\QTTabBar Setup.msi" 
"C:\Program Files\Bandizip\bz.exe" a -aoa "%cd%\bin\Release\pt-BR\QTTabBar.Setup_%VZ%pt%SUFFIX%" "%cd%\bin\Release\pt-BR\QTTabBar Setup.msi" 
"C:\Program Files\Bandizip\bz.exe" a -aoa "%cd%\bin\Release\tr-TR\QTTabBar.Setup_%VZ%tr%SUFFIX%" "%cd%\bin\Release\tr-TR\QTTabBar Setup.msi" 
"C:\Program Files\Bandizip\bz.exe" a -aoa "%cd%\bin\Release\ru-RU\QTTabBar.Setup_%VZ%ru%SUFFIX%" "%cd%\bin\Release\ru-RU\QTTabBar Setup.msi" 
pause
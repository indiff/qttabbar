@echo off
for /f "delims=" %%i in ('dir /ad /b' ) do @if exist "%CD%\%%i\Debug" @rmdir /s /q "%CD%\%%i\Debug"
for /f "delims=" %%i in ('dir /ad /b' ) do @if exist "%CD%\%%i\Release" @rmdir /s /q "%CD%\%%i\Release"
for /f "delims=" %%i in ('dir /ad /b' ) do @if exist "%CD%\%%i\Release64" @rmdir /s /q "%CD%\%%i\Release64"
for /f "delims=" %%i in ('dir /ad /b' ) do @if exist "%CD%\%%i\bin" @rmdir /s /q "%CD%\%%i\bin"
for /f "delims=" %%i in ('dir /ad /b' ) do @if exist "%CD%\%%i\obj" @rmdir /s /q "%CD%\%%i\obj"
del  /f /s /q *.plg
del /f /s /q *.aps
pause
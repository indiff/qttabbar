@echo off
taskkill /F /IM explorer.exe
start explorer.exe

:wait
tasklist /FI "IMAGENAME eq explorer.exe" 2>NUL | find /I "explorer.exe" >NUL
if errorlevel 1 (
    timeout /T 1 /NOBREAK >NUL
    goto wait
)

exit
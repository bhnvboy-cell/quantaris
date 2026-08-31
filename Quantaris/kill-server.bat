@echo off
echo Stopping SixSigma server...
taskkill /F /IM SixSigmaSaaS-Server.exe >nul 2>&1
if %errorlevel%==0 (
    echo Server stopped.
) else (
    echo Server was not running.
)
timeout /t 2 /nobreak >nul
echo Clearing ASP.NET temp cache...
if exist "%~dp0App_Data\Temp\root" (
    rmdir /S /Q "%~dp0App_Data\Temp\root"
    echo Temp cache cleared.
) else (
    echo Temp cache already clean.
)
echo Done.
pause

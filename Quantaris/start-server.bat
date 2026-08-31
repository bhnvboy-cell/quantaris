@echo off
echo Stopping any existing server...
taskkill /F /IM SixSigmaSaaS-Server.exe >nul 2>&1
timeout /t 2 /nobreak >nul
echo Clearing ASP.NET temp cache...
if exist "%~dp0App_Data\Temp\root" (
    rmdir /S /Q "%~dp0App_Data\Temp\root"
    echo Temp cache cleared.
)
echo Starting SixSigma server on port 8082...
start "" "%~dp0SixSigmaSaaS-Server.exe" 8082
timeout /t 3 /nobreak >nul
echo Server started at http://localhost:8082
pause

@echo off
echo Stopping Quantaris Server...
taskkill /f /im SixSigmaSaaS-Server.exe >nul 2>&1
taskkill /f /im SixSigmaSaaS.exe >nul 2>&1
echo Server stopped.
timeout /t 2 >nul

@echo off
title Quantaris Server
echo Starting Quantaris Server...
echo URL: http://localhost:8082
echo.
start "Quantaris" "%~dp0SixSigmaSaaS-Server.exe" 8082
timeout /t 3 >nul
echo Server started. Open http://localhost:8080 in your browser.
echo To stop, run StopServer.bat

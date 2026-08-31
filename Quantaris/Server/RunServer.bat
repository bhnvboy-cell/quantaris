@echo off
title Quantaris Server
cd /d "%~dp0.."
echo Starting Quantaris Server...
echo Application: %CD%
echo URL: http://localhost:8080
echo.
"%~dp0SixSigmaSaaS.exe" 8082 "%CD%"
pause

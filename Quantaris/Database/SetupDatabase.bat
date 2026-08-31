@echo off
title Quantaris - Database Setup (MySQL / MariaDB)
color 0A
setlocal enabledelayedexpansion

echo ================================================
echo   Quantaris - MySQL / MariaDB Database Setup
echo ================================================
echo.

:: --- Find mysql.exe ---
set MYSQL=
where mysql >nul 2>&1
if !errorlevel! equ 0 (
    set MYSQL=mysql
) else (
    for %%p in (
        "C:\xampp\mysql\bin\mysql.exe"
        "%ProgramFiles%\MySQL\*\bin\mysql.exe"
        "%ProgramFiles(x86)%\MySQL\*\bin\mysql.exe"
    ) do (
        if exist %%p (
            set MYSQL=%%p
            goto :found
        )
    )
)
:found

if "%MYSQL%"=="" (
    echo [ERROR] mysql.exe not found!
    echo.
    echo Install MySQL or MariaDB (e.g. XAMPP) and ensure the bin folder
    echo is on PATH, or place it at C:\xampp\mysql\bin\mysql.exe.
    echo.
    pause
    exit /b 1
)

echo Using MySQL client: %MYSQL%
echo.

:: --- Connection details ---
set DB_NAME=quantaris
set /p MYSQL_HOST="MySQL Host [localhost]: "
if "%MYSQL_HOST%"=="" set MYSQL_HOST=localhost

set /p MYSQL_PORT="MySQL Port [3306]: "
if "%MYSQL_PORT%"=="" set MYSQL_PORT=3306

set /p MYSQL_USER="MySQL Username [root]: "
if "%MYSQL_USER%"=="" set MYSQL_USER=root

set /p MYSQL_PWD="MySQL Password (XAMPP default is blank) [blank]: "

echo.
echo ================================================
echo   Step 1: Creating database (if not exists)
echo ================================================
echo.

%MYSQL% -h %MYSQL_HOST% -P %MYSQL_PORT% -u %MYSQL_USER% --password=%MYSQL_PWD% -e "SELECT VERSION();" >nul 2>&1
if !errorlevel! neq 0 (
    echo [ERROR] Could not connect to MySQL server.
    echo   Host: %MYSQL_HOST%:%MYSQL_PORT%
    echo   User: %MYSQL_USER%
    echo.
    echo Possible issues:
    echo   - MySQL service is not running (start mysqld or the XAMPP MySQL service)
    echo   - Host/port/username is incorrect
    echo   - Password is wrong
    echo.
    pause
    exit /b 1
)

%MYSQL% -h %MYSQL_HOST% -P %MYSQL_PORT% -u %MYSQL_USER% --password=%MYSQL_PWD% -e "CREATE DATABASE IF NOT EXISTS %DB_NAME% DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;"
if !errorlevel! equ 0 (
    echo [OK] Database '%DB_NAME%' is ready (created if it did not exist).
) else (
    echo [ERROR] Failed to create database '%DB_NAME%'.
    pause
    exit /b 1
)

echo.
echo ================================================
echo   Step 2: Creating tables and seed data
echo ================================================
echo.

set SCRIPT_DIR=%~dp0

echo Running master schema (CreateMySQL.sql)...
%MYSQL% -h %MYSQL_HOST% -P %MYSQL_PORT% -u %MYSQL_USER% --password=%MYSQL_PWD% %DB_NAME% < "%SCRIPT_DIR%CreateMySQL.sql"
if !errorlevel! neq 0 (
    echo [ERROR] Failed to run CreateMySQL.sql.
    pause
    exit /b 1
)
echo [OK] Master schema applied (Tenants, Users, projects, FMEA, etc.).

echo Running tool schemas...
for %%f in (
    CreateBenefitsTracking.sql
    CreateControlCharts.sql
    CreateCOPQ.sql
    CreateGageRnR.sql
    CreateGantt.sql
    CreateHoshinKanri.sql
    CreateMonteCarlo.sql
    CreateProcessCapability.sql
    CreateQFD.sql
    CreateRegression.sql
    CreateSampleSize.sql
    CreateTrainingLMS.sql
    CreateDOE.sql
) do (
    if exist "%SCRIPT_DIR%%%f" (
        %MYSQL% -h %MYSQL_HOST% -P %MYSQL_PORT% -u %MYSQL_USER% --password=%MYSQL_PWD% %DB_NAME% < "%SCRIPT_DIR%%%f"
        if !errorlevel! neq 0 (
            echo [ERROR] Failed to run %%f.
            pause
            exit /b 1
        )
        echo [OK] %%f
    ) else (
        echo [SKIP] %%f not found
    )
)

echo Running quiz schema + exam seed (CreateQuizzes.sql)...
%MYSQL% -h %MYSQL_HOST% -P %MYSQL_PORT% -u %MYSQL_USER% --password=%MYSQL_PWD% %DB_NAME% < "%SCRIPT_DIR%CreateQuizzes.sql"
if !errorlevel! neq 0 (
    echo [ERROR] Failed to run CreateQuizzes.sql.
    pause
    exit /b 1
)
echo [OK] CreateQuizzes.sql

echo Running demo seed data (SeedFoodCompanyDemo.sql)...
%MYSQL% -h %MYSQL_HOST% -P %MYSQL_PORT% -u %MYSQL_USER% --password=%MYSQL_PWD% %DB_NAME% < "%SCRIPT_DIR%SeedFoodCompanyDemo.sql"
if !errorlevel! neq 0 (
    echo [ERROR] Failed to run SeedFoodCompanyDemo.sql.
    pause
    exit /b 1
)
echo [OK] SeedFoodCompanyDemo.sql

echo.
echo ================================================
echo   Database Setup Complete!
echo ================================================
echo.
echo   Database  : %DB_NAME%
echo   Host      : %MYSQL_HOST%:%MYSQL_PORT%
echo   User      : %MYSQL_USER%
echo.
echo   Next steps:
echo   1. Start the server:     StartServer.bat
echo   2. Open in browser:      http://localhost:8082/Setup.aspx
echo   3. Login with:           admin / Admin@123  (Tenant: DEMO)
echo.
pause

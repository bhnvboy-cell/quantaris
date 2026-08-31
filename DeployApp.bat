@echo off
echo Creating IIS Application...
echo This needs Administrator privileges.

set APP_NAME=Quantaris
set APP_PATH=C:\Users\nagaraj\my projects\quantaris\Quantaris

echo.
echo ================================================
echo To deploy manually:
echo ================================================
echo 1. Open IIS Manager (inetmgr)
echo 2. Right-click "Sites" -^> "Add Website"
echo 3. Site name: Quantaris
echo 4. Physical path: %APP_PATH%
echo 5. Port: 8080 (or any available)
echo 6. OK
echo.
echo Then browse to: http://localhost:8080/Setup.aspx
echo.
echo ================================================
echo Or run this to create the site via command line:
echo ================================================
echo %windir%\system32\inetsrv\appcmd add site /name:%APP_NAME% /physicalPath:"%APP_PATH%" /bindings:http://*:8080
echo.
pause

@echo off
echo Installing IIS Windows Features...
echo This needs Administrator privileges.

dism /online /enable-feature /featurename:IIS-WebServerRole /all /norestart
dism /online /enable-feature /featurename:IIS-WebServer /all /norestart
dism /online /enable-feature /featurename:IIS-CommonHttpFeatures /all /norestart
dism /online /enable-feature /featurename:IIS-DefaultDocument /all /norestart
dism /online /enable-feature /featurename:IIS-DirectoryBrowsing /all /norestart
dism /online /enable-feature /featurename:IIS-HttpErrors /all /norestart
dism /online /enable-feature /featurename:IIS-StaticContent /all /norestart
dism /online /enable-feature /featurename:IIS-ASPNET45 /all /norestart
dism /online /enable-feature /featurename:IIS-NetFxExtensibility45 /all /norestart
dism /online /enable-feature /featurename:IIS-ISAPIExtensions /all /norestart
dism /online /enable-feature /featurename:IIS-ISAPIFilter /all /norestart
dism /online /enable-feature /featurename:WAS-WindowsActivationService /all /norestart
dism /online /enable-feature /featurename:WAS-ProcessModel /all /norestart
dism /online /enable-feature /featurename:WAS-NetFxEnvironment /all /norestart

echo.
echo IIS installation complete!
echo You may need to restart your computer.
pause

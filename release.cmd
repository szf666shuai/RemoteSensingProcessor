@echo off
setlocal

set "PROJECT=%~dp0"
set "REPO_NAME=RemoteSensingProcessor"
set "VERSION=v1.1.0"
set "ZIP=%PROJECT%dist\RemoteSensingProcessor-win-x64.zip"

echo === 1. Publish ===
dotnet publish "%PROJECT%RemoteSensingProcessor.csproj" -c Release -r win-x64 --self-contained true -o "%PROJECT%PUBLISH"
if errorlevel 1 exit /b 1

echo === 2. Create release zip ===
if not exist "%PROJECT%dist" mkdir "%PROJECT%dist"
powershell -NoProfile -Command "Compress-Archive -Path '%PROJECT%PUBLISH\*' -DestinationPath '%ZIP%' -Force"

echo === 3. Create GitHub Release ===
gh release create %VERSION% "%ZIP%" --title "RemoteSensingProcessor %VERSION%" --notes "Windows x64 自包含发布包，解压后运行 RemoteSensingProcessor.exe"

echo.
echo Done: https://github.com/YOUR_USERNAME/%REPO_NAME%/releases/tag/%VERSION%

@echo off
setlocal

set "PROJECT=%~dp0RemoteSensingProcessor.csproj"
set "OUTPUT=%~dp0PUBLISH"

echo Publishing to %OUTPUT%
dotnet publish "%PROJECT%" -c Release -r win-x64 --self-contained true -o "%OUTPUT%"
if errorlevel 1 (
    echo Publish failed.
    exit /b 1
)

echo.
echo Publish completed successfully.
echo Run: %OUTPUT%\RemoteSensingProcessor.exe

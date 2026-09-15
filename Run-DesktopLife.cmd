@echo off
if exist "%~dp0artifacts\publish\DesktopLife.exe" (
    start "" "%~dp0artifacts\publish\DesktopLife.exe"
) else (
    dotnet run --project "%~dp0src\DesktopLife.App" --configuration Release
    if errorlevel 1 pause
)

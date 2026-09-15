@echo off
if exist "%~dp0artifacts\publish\DesktopLife.exe" (
    start "" "%~dp0artifacts\publish\DesktopLife.exe" --settings
) else (
    dotnet run --project "%~dp0src\DesktopLife.App" --configuration Release -- --settings
    if errorlevel 1 pause
)

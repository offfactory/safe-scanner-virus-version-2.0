@echo off
setlocal
cd /d "%~dp0"
if not exist "Desktop.csproj" (
  echo ERROR: Desktop.csproj not found in "%~dp0"
  pause
  exit /b 1
)

echo Building SafeScan Defender...
dotnet build "Desktop.csproj" -c Debug
if errorlevel 1 (
  echo Build failed.
  pause
  exit /b 1
)

echo Building SafeScan Launcher...
dotnet build "Launcher\Launcher.csproj" -c Debug
if errorlevel 1 (
  echo Launcher build failed.
  pause
  exit /b 1
)

echo Launching SafeScan Launcher...
start "" "%~dp0Launcher\bin\Debug\net8.0\Launcher.exe"

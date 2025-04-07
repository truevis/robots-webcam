@echo off
echo Cleaning up processes...
taskkill /f /im "dotnet.exe" >nul 2>&1
taskkill /f /im "WebcamViewer.exe" >nul 2>&1
timeout /t 3 >nul

echo Cleaning build directories...
if exist "bin" rmdir /s /q "bin" 2>nul
if exist "obj" rmdir /s /q "obj" 2>nul

echo Building application...
dotnet publish -c Release -r win-x64 --self-contained ^
    -p:PublishSingleFile=true ^
    -p:IncludeNativeLibrariesForSelfExtract=true ^
    -p:PublishReadyToRun=false ^
    -p:EnableCompressionInSingleFile=true ^
    -o "output"

if %ERRORLEVEL% NEQ 0 (
    echo Build failed!
    pause
    exit /b 1
)

echo Build completed! Executable is in the output folder.
@REM pause 
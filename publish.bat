@echo off
echo Cleaning up processes...
taskkill /f /im "dotnet.exe" >nul 2>&1
taskkill /f /im "WebcamViewer.exe" >nul 2>&1
taskkill /f /im "msbuild.exe" >nul 2>&1
timeout /t 5 >nul

echo Checking output directory...
if exist "output\WebcamViewer.exe" (
    echo Removing locked output executable...
    taskkill /f /im "WebcamViewer.exe" >nul 2>&1
    timeout /t 2 >nul
    del /f "output\WebcamViewer.exe" 2>nul
    if exist "output\WebcamViewer.exe" (
        echo Unable to delete output file. It may be locked by another process.
        echo Please close any applications using WebcamViewer.exe and try again.
        pause
        exit /b 1
    )
)

echo Checking for file locks...
if exist "obj\project.nuget.cache" (
    attrib -r "obj\project.nuget.cache" >nul 2>&1
)

echo Cleaning build directories...
if exist "bin" (
    echo Attempting to remove bin directory...
    rd /s /q "bin" 2>nul
    if exist "bin" (
        echo Retrying with force...
        attrib -r -h -s "bin\*.*" /s /d >nul 2>&1
        rd /s /q "bin" 2>nul
    )
)

if exist "obj" (
    echo Attempting to remove obj directory...
    rd /s /q "obj" 2>nul
    if exist "obj" (
        echo Retrying with force...
        attrib -r -h -s "obj\*.*" /s /d >nul 2>&1
        rd /s /q "obj" 2>nul
    )
)

timeout /t 2 >nul

echo Building application...
set ATTEMPT=1
set MAX_ATTEMPTS=3

:BUILD
echo Build attempt %ATTEMPT% of %MAX_ATTEMPTS%
dotnet clean >nul 2>&1

:: Delete output directory contents
if exist "output" (
    echo Cleaning output directory...
    del /f /q "output\*.*" >nul 2>&1
) else (
    mkdir "output" >nul 2>&1
)

dotnet publish -c Release -r win-x64 --self-contained ^
    -p:PublishSingleFile=true ^
    -p:IncludeNativeLibrariesForSelfExtract=true ^
    -p:PublishReadyToRun=false ^
    -p:EnableCompressionInSingleFile=true ^
    -o "output"

if %ERRORLEVEL% NEQ 0 (
    if %ATTEMPT% LSS %MAX_ATTEMPTS% (
        echo Build failed, retrying after cleanup...
        set /a ATTEMPT+=1
        taskkill /f /im "dotnet.exe" >nul 2>&1
        taskkill /f /im "msbuild.exe" >nul 2>&1
        taskkill /f /im "WebcamViewer.exe" >nul 2>&1
        timeout /t 10 >nul
        if exist "bin" rd /s /q "bin" 2>nul
        if exist "obj" rd /s /q "obj" 2>nul
        if exist "output\WebcamViewer.exe" del /f "output\WebcamViewer.exe" >nul 2>&1
        timeout /t 5 >nul
        goto BUILD
    ) else (
        echo All build attempts failed!
        pause
        exit /b 1
    )
)

echo Cleaning up build directories after successful build...
if exist "bin" rd /s /q "bin" 2>nul
if exist "obj" rd /s /q "obj" 2>nul

echo Build completed! Executable is in the output folder.
@REM pause 
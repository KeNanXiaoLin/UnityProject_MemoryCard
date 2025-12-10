@echo off
chcp 437 >nul 2>&1  :: 使用Windows默认英文编码，避免中文乱码

:: ===================== DO NOT MODIFY ANY PATHS! =====================
:: Note: The script must be placed in the project's Tools directory (same as main)
:: Base path: Force switch to project root directory (When-Sleeping/)
:: =============================================================

:: 1. Get the absolute path of the script itself
set "SCRIPT_PATH=%~dp0"
set "SCRIPT_PATH=%SCRIPT_PATH:~0,-1%"  :: Remove trailing backslash

:: 2. Switch to project root directory (parent of Tools directory)
for %%i in ("%SCRIPT_PATH%") do set "PROJECT_ROOT=%%~dpi"
set "PROJECT_ROOT=%PROJECT_ROOT:~0,-1%"  :: Remove trailing backslash

cd /d "%PROJECT_ROOT%" || (
    echo Error: Failed to switch to project root: %PROJECT_ROOT%
    echo Press Enter to exit...
    pause >nul
    exit /b 1
)

:: 3. Define relative paths
set "TOOL_PATH=.\Tools\main.exe"
set "INPUT_DIR=.\Excel"
set "OUTPUT_DIR=.\Assets\StreamingAssets"

:: ------------------------ NO NEED TO MODIFY BELOW ------------------------
:: Check if packaging program exists
if not exist "%TOOL_PATH%" (
    echo Error: Packaging program not found!
    echo   Expected path: %PROJECT_ROOT%\%TOOL_PATH%
    echo Press Enter to exit...
    pause >nul
    exit /b 1
)

:: Check if input directory exists
if not exist "%INPUT_DIR%\" (
    echo Error: Input directory not found!
    echo   Expected path: %PROJECT_ROOT%\%INPUT_DIR%
    echo Press Enter to exit...
    pause >nul
    exit /b 1
)

:: Ensure output directory exists
if not exist "%OUTPUT_DIR%\" (
    echo Info: Output directory not found, creating...
    mkdir "%OUTPUT_DIR%" || (
        echo Error: Failed to create output directory: %PROJECT_ROOT%\%OUTPUT_DIR%
        echo Press Enter to exit...
        pause >nul
        exit /b 1
    )
)

:: Execute conversion command
echo ======================================
echo Project root: %PROJECT_ROOT%
echo Tool path: %TOOL_PATH%
echo Input directory: %INPUT_DIR%
echo Output directory: %OUTPUT_DIR%
echo ======================================

:: Run the packaging program
"%TOOL_PATH%" -i "%INPUT_DIR%" -o "%OUTPUT_DIR%"

:: Show result
if %errorlevel% equ 0 (
    echo ======================================
    echo Conversion successful!
) else (
    echo ======================================
    echo Conversion failed!
)

:: Pause window
echo Press Enter to exit...
pause >nul
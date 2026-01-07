@echo off
setlocal

echo ========================================================
echo Building sFixer Solution (WixSharp MSI)
echo ========================================================

REM 1. Build and Publish the Main Application
echo.
echo [1/3] Publishing sfixer application...
cd sfixer
dotnet publish -c Release -r win-x64 --self-contained true
if %errorlevel% neq 0 (
    echo [ERROR] Failed to publish sfixer.
    exit /b %errorlevel%
)
cd ..

REM 2. Build the Installer
echo.
echo [2/3] Building MSI Installer...
cd setup
REM Run the setup console app which uses WixSharp to build the MSI
dotnet run
if %errorlevel% neq 0 (
    echo [ERROR] Failed to build installer.
    exit /b %errorlevel%
)

REM 3. Move MSI to Root
echo.
echo [3/3] Copying MSI to root directory...
if exist "sFixerSetup.msi" (
    copy /Y "sFixerSetup.msi" "..\sFixerSetup.msi"
    echo [SUCCESS] sFixerSetup.msi created at root.
) else (
    echo [WARNING] sFixerSetup.msi not found in setup directory.
)
cd ..

echo.
echo ========================================================
echo Build Complete!
echo ========================================================
pause

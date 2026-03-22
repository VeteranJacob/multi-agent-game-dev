@echo off
:: ============================================================
::  Healing Temple Ledger — Build & Package Script
::  Run from the root of the project folder as Administrator
:: ============================================================
setlocal EnableDelayedExpansion
title Healing Temple Ledger Build Script
color 0A

echo.
echo  =====================================================
echo   H E A L I N G   T E M P L E   L E D G E R   B U I L D   S C R I P T
echo  =====================================================
echo.

:: ── Step 1: Check .NET 8 SDK ─────────────────────────────────────────────────
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo  [ERROR] .NET 8 SDK not found.
    echo  Download from: https://dotnet.microsoft.com/download/dotnet/8.0
    echo  Install the "SDK" version (not Runtime only)
    pause & exit /b 1
)
echo  [OK] .NET SDK found.

:: ── Step 2: Restore NuGet packages ──────────────────────────────────────────
echo.
echo  Restoring packages...
dotnet restore HealingTempleLedger\HealingTempleLedger.csproj
if errorlevel 1 ( echo  [ERROR] Restore failed. & pause & exit /b 1 )
echo  [OK] Packages restored.

:: ── Step 3: Build Release ────────────────────────────────────────────────────
echo.
echo  Building Release...
dotnet build HealingTempleLedger\HealingTempleLedger.csproj -c Release --no-restore
if errorlevel 1 ( echo  [ERROR] Build failed. & pause & exit /b 1 )
echo  [OK] Build complete.

:: ── Step 4: Publish self-contained x64 EXE ──────────────────────────────────
echo.
echo  Publishing self-contained Windows x64 EXE...
dotnet publish HealingTempleLedger\HealingTempleLedger.csproj ^
    -c Release ^
    -r win-x64 ^
    --self-contained true ^
    -p:PublishSingleFile=true ^
    -p:IncludeNativeLibrariesForSelfExtract=true ^
    -o publish
if errorlevel 1 ( echo  [ERROR] Publish failed. & pause & exit /b 1 )
echo  [OK] Published to .\publish\

:: ── Step 5: NSIS Installer (optional) ───────────────────────────────────────
echo.
where makensis >nul 2>&1
if errorlevel 1 (
    echo  [INFO] NSIS not found — skipping installer creation.
    echo         To create installer: install NSIS from https://nsis.sourceforge.io
    echo         Then run: makensis HealingTempleLedger_Installer.nsi
) else (
    echo  Building NSIS installer...
    makensis HealingTempleLedger_Installer.nsi
    if errorlevel 1 ( echo  [WARN] NSIS build failed. ) else ( echo  [OK] Installer created. )
)

:: ── Step 6: Create Desktop Shortcut to published EXE ─────────────────────────
echo.
echo  Creating Desktop shortcut...
set "EXE=%CD%\publish\HealingTempleLedger.exe"
set "SHORTCUT=%USERPROFILE%\Desktop\HealingTempleLedger.lnk"
powershell -Command ^
    "$ws = New-Object -ComObject WScript.Shell; ^
     $s = $ws.CreateShortcut('%SHORTCUT%'); ^
     $s.TargetPath = '%EXE%'; ^
     $s.Description = 'Healing Temple Ledger — GAAPCLAW Suite and GAAPCLAW Suite'; ^
     $s.WorkingDirectory = '%CD%\publish'; ^
     $s.Save()"
if exist "%SHORTCUT%" (
    echo  [OK] Desktop shortcut created: %SHORTCUT%
) else (
    echo  [WARN] Shortcut creation may have failed. You can manually create one.
)

echo.
echo  =====================================================
echo   BUILD COMPLETE
echo.
echo   Executable: .\publish\HealingTempleLedger.exe
echo   Installer:  HealingTempleLedger_Setup_v1.0.0.exe (if NSIS)
echo   Shortcut:   %USERPROFILE%\Desktop\HealingTempleLedger.lnk
echo  =====================================================
echo.
pause

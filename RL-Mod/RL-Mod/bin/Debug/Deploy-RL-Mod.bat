@echo off
setlocal EnableExtensions
rem ============================================================
rem  Deploy RL-Mod.dll from this folder (bin\Debug) to both games.
rem  Run this after building the project in Visual Studio.
rem ============================================================

set "SRC=%~dp0RL-Mod.dll"
set "DST1=D:\Rogue.Legacy.2testing\Rogue Legacy 2\BepInEx\plugins"
set "DST2=D:\Rogue.Legacy.2\Rogue Legacy 2\BepInEx\plugins"

echo.
echo [RL-Mod] Source: %SRC%
for %%F in ("%SRC%") do echo [RL-Mod] DLL timestamp: %%~tF
echo.

if not exist "%SRC%" (
    echo [ERROR] RL-Mod.dll not found. Build the project in Visual Studio first.
    goto end
)

set "FAIL=0"

if not exist "%DST1%\" (
    echo [SKIP] Testing plugins folder not found: %DST1%
    set "FAIL=1"
) else (
    copy /Y "%SRC%" "%DST1%\RL-Mod.dll" >nul
    if errorlevel 1 (
        echo [ERROR] Failed to copy to testing folder.
        set "FAIL=1"
    ) else (
        echo [OK] Copied to: %DST1%\RL-Mod.dll
    )
)

if not exist "%DST2%\" (
    echo [SKIP] Main plugins folder not found: %DST2%
    set "FAIL=1"
) else (
    copy /Y "%SRC%" "%DST2%\RL-Mod.dll" >nul
    if errorlevel 1 (
        echo [ERROR] Failed to copy to main folder.
        set "FAIL=1"
    ) else (
        echo [OK] Copied to: %DST2%\RL-Mod.dll
    )
)

echo.
if "%FAIL%"=="0" (
    echo [DONE] Both folders updated. Restart both games and check the START version in logs.
) else (
    echo [WARN] Some targets were not updated. Check messages above.
)

:end
echo.
pause

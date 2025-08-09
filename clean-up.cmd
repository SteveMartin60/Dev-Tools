@echo off
setlocal enabledelayedexpansion

echo Deleting all bin and obj folders...
echo.

for /d /r . %%d in (bin,obj) do (
    if exist "%%d" (
        echo Deleting: %%d
        rd /s /q "%%d"
    )
)

echo.
echo Cleanup complete.
pause

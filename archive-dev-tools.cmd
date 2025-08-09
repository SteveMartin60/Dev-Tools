@echo off

CLS

REM Get the current folder path where the batch file resides
set "current_folder=%~dp0"

REM 7ip exe file
Set SevenZip=C:\Program Files\7-Zip\7z.exe

REM Archive file (.7z) saved to this folder
Set TargetFolder=D:\Snapshots\Archive-Dev-Tools\

REM Top level folder to be archived
Set SourceFolder="N:\OneDrive - Mesheven\Dev-Tools"

REM Folders listed in this file are not archived
Set Exclusions="%current_folder%\exclusions.txt"

REM Generate a filename like: D:\archives\desktop-automation_2021-04-22_08.21.24.7z
for /f "tokens=2 delims==" %%a in ('wmic OS Get localdatetime /value') do Set "dt=%%a"
Set "YY=%dt:~2,2%" & Set "YYYY=%dt:~0,4%" & Set "MM=%dt:~4,2%" & Set "DD=%dt:~6,2%"
Set "HH=%dt:~8,2%" & Set "Min=%dt:~10,2%" & Set "Sec=%dt:~12,2%"
Set DateTime=%YYYY%-%MM%-%DD%.%HH%.%Min%
Set FileName="%TargetFolder%\C#-%DateTime%.dev-tools.7z"

"%SevenZip%" a -mx1 -mmt8 %FileName% %SourceFolder% -xr@%Exclusions%

Explorer %TargetFolder%

Pause



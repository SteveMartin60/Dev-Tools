@echo off
Call :YesNoBox "This will immediately reboot the computer!!"

taskkill /f /fi *

if "%YesNo%"=="0" (
Call :MessageBox "You answered 0" "Heading"
exit /b
)
if "%YesNo%"=="1" (
shutdown /r /f /t 00
exit /b
)
if "%YesNo%"=="2" (
Call :MessageBox "Reboot cancelled" "Reboot"
exit /b
)
if "%YesNo%"=="3" (
Call :MessageBox "You answered 3" "Heading"
exit /b
)
if "%YesNo%"=="4" (
Call :MessageBox "You answered 4" "Heading"
exit /b
)
if "%YesNo%"=="5" (
Call :MessageBox "You answered 5" "Heading"
exit /b
)
if "%YesNo%"=="6" (
Call :MessageBox "You answered 6" "Heading"
exit /b
)
if "%YesNo%"=="7" (
Call :MessageBox "You answered NO" "Heading"
exit /b
)

Pause

exit /b
:YesNoBox
REM returns 6 = Yes, 7 = No. Type=4 = Yes/No
set YesNo=
set MsgType=4113
set heading="Warning"
set message=%~1
echo wscript.echo msgbox(WScript.Arguments(0),%MsgType%,WScript.Arguments(1)) >"%temp%\input.vbs"

REM MsgBox "Hello everyone!",65,"Warning"

for /f "tokens=* delims=" %%a in ('cscript //nologo "%temp%\input.vbs" "%message%" "%heading%"') do set YesNo=%%a
exit /b

:MessageBox
set heading=%~2
set message=%~1
echo msgbox WScript.Arguments(0),0,WScript.Arguments(1) >"%temp%\input.vbs"
cscript //nologo "%temp%\input.vbs" "%message%" "%heading%"
exit /b
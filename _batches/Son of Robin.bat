@echo off
setlocal

rem Set the folder where the app.exe is located
set "appFolder=Son_of_Robin"

rem Set the name of the executable
set "appName=SonOfRobin.WindowsDX.exe"

rem Get the current date and time components
for /f "tokens=1-6 delims=/:. " %%a in ('echo %date% %time%') do (
    set "year=%%c"
    set "month=%%b"
    set "day=%%a"
    set "hours=%%d"
    set "minutes=%%e"
    set "seconds=%%f"
)

rem Pad single digits with leading zeros

if %day% LSS 10 set "day=0%day%"
if %hours% LSS 10 set "hours=0%hours%"

rem Create the filename based on the date and time
set "outputFilename=..\son_of_robin_output_%year%-%month%-%day%__%hours%-%minutes%-%seconds%.txt"

rem Temporary filenames for stdout and stderr
set "stdoutTempFile=stdout_temp.txt"
set "stderrTempFile=stderr_temp.txt"

rem Run the executable, capture stdout and stderr to temporary files
cd /d "%appFolder%"
powershell -command "& {Start-Process '%appName%' -RedirectStandardOutput '%stdoutTempFile%' -RedirectStandardError '%stderrTempFile%' -PassThru | ForEach-Object { $_.WaitForExit(); }}"

rem Combine stdout and stderr into the final output file
type "%stdoutTempFile%" > "%outputFilename%"
type "%stderrTempFile%" >> "%outputFilename%"

rem Clean up temporary files
del "%stdoutTempFile%"
del "%stderrTempFile%"

echo Output written to "%outputFilename%"

endlocal

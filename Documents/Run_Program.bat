@echo off

set ExePath=CIA.exe

if exist %ExePath% goto DoWork
if exist ..\%ExePath% set ExePath=..\%ExePath% && goto DoWork
if exist ..\CIA\bin\x64\Debug\%ExePath% set ExePath=..\CIA\bin\x64\Debug\%ExePath% && goto DoWork

echo Executable not found: %ExePath%
goto Done

:DoWork
echo.
echo Procesing with %ExePath%
echo.

%ExePath% cia data_4_formularity.txt param_4_formularity.xml ..\Data\CIA_DB\WHOI_CIA_DB_2016_11_21.bin

:Done

pause

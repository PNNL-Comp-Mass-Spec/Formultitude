@echo on

xcopy NoExcel\x64\Debug\*.* C:\DMS_Programs\Formularity /D /Y
xcopy NoExcel\x64\Debug\*.* \\pnl\projects\OmicsSW\DMS_Programs\AnalysisToolManagerDistribution\Formularity /D /Y

pause

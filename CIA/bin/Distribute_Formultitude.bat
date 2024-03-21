@echo on

xcopy x64\Debug\*.* C:\DMS_Programs\Formultitude /D /Y
xcopy x64\Debug\*.* \\pnl\projects\OmicsSW\DMS_Programs\AnalysisToolManagerDistribution\Formultitude /D /Y

pause

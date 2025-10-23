@echo on

xcopy x64\Debug\*.* C:\DMS_Programs\Formultitude /D /Y
xcopy x64\Debug\*.* \\Proto-3\DMS_Programs_Dist\AnalysisToolManagerDistribution\Formultitude /D /Y

pause

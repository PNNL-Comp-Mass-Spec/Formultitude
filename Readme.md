## Formularity Overview

Formularity is software for assignment of low weight molecular formulas from high-resolution mass spectra. 
It includes graphical user interface with two search functions: 
* Compound Identification Algorithm (CIA)
* Isotopic Pattern Algorithm (IPA),

It also includes supporting calibration, alignment and inspection functions. 

Supporting databases and test files are provided together with software executable and source files. 

Related Publication(s): 
* Formularity: Software for Automated Formula Assignment of Natural and Derived Organic Matter from Ultra-High Resolution Mass Spectra
  * N Tolić, Y Liu, A Liyu, Y Shen, MM Tfaily, EB Kujawinski, K Longnecker, LJ Kuo, EW Robinson, L Paša-Tolić, and NJ Hess. Analytical Chemistry. 2017 Dec 5; 89(23), 12659-12665. [PMID 29120613](https://pubmed.ncbi.nlm.nih.gov/29120613/)
* Automated analysis of electrospray ionization Fourier transform ion cyclotron resonance mass spectra of natural organic matter.
  * EB Kujawinski and MD Behn. Analytical Chemistry. 2006 Jul 1; 78(13), 4363-4373. [PMID 16808443](https://pubmed.ncbi.nlm.nih.gov/16808443/)

## Downloads and Instructions

Formularity runs on 64-bit version of Windows; no installer is provided.
Extract the contents of Formularity.zip, then run Formularity.exe
Documentation is in file `UserManual.pdf` in the Documents folder [on GitHub](https://github.com/PNNL-Comp-Mass-Spec/Formularity/tree/master/Documents)

To test the program, use the provided test files in the Documents folder.
The test database is available as a separate download.
* See file `CIA_DB_2016_11_21_Database.zip` [on GitHub](https://github.com/PNNL-Comp-Mass-Spec/Formularity/releases)

### Analysis Steps

1) Select tab "CIA formula finding"
* Drag & drop CIA database (file CIA_DB_2016_11_21.bin) to the database box (labeled "Drop DB files")
* Database load time will take 30 to 60 seconds

2) Select tab "IPA formula finding"
* Drag & drop IPA database (IPA_DB_MTW_Cl_8_3.txt) to database box (labeled "Drop DB file")

3) Use "Load parameters" at the bottom to load test_parameters.xml
* Drag & drop test_parameters.xml to the "Drop parameter file" box

4) To use internal calibration, select regression model (auto, linear, or quadratic) then 
   drag/drop the calibration peaks file (Neg_ESI_CalibrationPeaks.ref) to the 
   calibration box (labeled "Drop ref peaks file"). If used, both search functions use calibrated peaks.

5) Check one or both of CIA and IPA check boxes for desired search. 
* The "Drop Spectra Files" area will turn green. 

6) Drag/drop file test_peaks.txt to the "Drop Spectra Files" area at the upper right of the window.
* The area will turn red while it is processing, then green when the search is complete.

CIA search will produce a timestamp-named directory with two CSV files
* For example, directory 20220110_132934 with:
  * Log file log.csv
  * Report file Out.csv

IPA search will produce two files per spectrum: `s_input_file_name` and `p_input_file_name`
* For example: `s_ipdbtest_peaks.csv` and `p_ipdbtest_peaks.csv`
* Also creates a timestamp-named directory with file log.csv

## Contact Info

E-mail any questions, comments, or bug reports to proteomics@pnnl.gov

## Acknowledgments

Formularity was developed by Andrey Liyu and Nikola Tolić at 
Pacific Northwest National Laboratory (PNNL).

The CIA algorithm was developed by Elizabeth Kujawinski and Krista Longnecker 
at the Woods Hole Oceanographic Institution (WHOI).

All publications that utilize this software should provide appropriate acknowledgement to PNNL and Formularity's GitHub website.

## Disclaimer

These programs are primarily designed to run on Windows machines. Please use them at your own risk. 
This material was prepared as an account of work sponsored by an agency of the United States Government. 
Neither the United States Government nor the United States Department of Energy, nor Battelle, 
nor any of their employees, makes any warranty, express or implied, or assumes any legal liability 
or responsibility for the accuracy, completeness, or usefulness or any information, apparatus, product, 
or process disclosed, or represents that its use would not infringe privately owned rights.

Portions of this research were supported by the W.R. Wiley Environmental Molecular Science Laboratory 
(a national scientific user facility sponsored by the U.S. Department of Energy's Office of Biological 
and Environmental Research and located at PNNL). PNNL is operated by Battelle Memorial Institute for the 
U.S. Department of Energy under contract DE-AC05-76RL0 1830.

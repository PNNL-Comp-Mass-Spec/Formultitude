== Formularity Overview ==

Formularity is software for assignment of low weight molecular formulas from high-resolution mass spectra. 
It includes graphical user interface with two search functions: 
* Compound Identification Algorithm (CIA)
* Isotopic Pattern Algorithm (IPA),

It also includes supporting calibration, alignment and inspection functions. 

Supporting databases and test files are provided together with software executable and source files. 

Related Publication(s): 
* Formularity: Software for Automated Formula Assignment of Natural and Derived Organic Matter from Ultra-High Resolution Mass Spectra (in preparation)
* Kujawinski, E. B., & Behn, M. D. (2006). Automated analysis of electrospray ionization Fourier transform ion cyclotron resonance mass spectra of natural organic matter. Analytical chemistry, 78(13), 4363-4373.

== Downloads and Instructions ==

Formularity runs on 64-bit version of Windows; no installer is provided.
Extract the contents of Formularity.zip, then run Formularity.exe
Documentation is in file UserManual.docx in the Documents folder.

To test the program, use the provided test files in the Documents folder.
The test database is available as a separate download.  
Visit https://omics.pnl.gov/software/formularity and download Formularity_CIA_DB.zip

=== Analysis Steps ===

1) Select tab "CIA formula finding"
	- Drag & drop CIA database (file CIA_DB_2016_11_21.bin) to the database box (labeled "Drop DB files")
	- Database load time will take 30 to 60 seconds

2) Select tab "IPA formula finding"
	- Drag & drop IPA database (IPA_DB_MTW_Cl_8_3.txt) to database box (labeled "Drop DB file")

3) Use "Load parameters" at the bottom to load test_parameters.xml

4) To use internal calibration, select regression model (auto, linear, or quadratic) then 
   drag/drop the calibration peaks file (Neg_ESI_CalibrationPeaks.ref) to the 
   calibration box (labeled "Drop calibration file"). If used, both search functions use calibrated peaks.

5) Check one or both of CIA and IPA check boxes for desired search. 
	- The "Drop Spectra Files" area will turn green. 

6) Drag/drop file test_peaks.txt to the "Drop Spectra Files" area at the upper right of the window.
	- The area will turn red while it is processing, then green when the search is complete.

CIA search will produce a Report file with details of internal calibration 
in a time-stamped log file (example name, Report20170530151031.log).

IPA search will produce two files per spectrum: "s_input_file_name" and "p_input_file_name"
For example: s_ipdbtest_peaks.csv and p_ipdbtest_peaks.csv

== Contact Info ==

We would like your feedback about the usefulness of the tools and information provided by the Resource. 
Your suggestions on how to increase their value to you will be appreciated. Please e-mail any 
questions, comments or bug reports to proteomics@pnnl.gov

== Acknowledgments ==

Formularity was developed by Andrey Liyu and Nikola Tolic at 
Pacific Northwest National Laboratory (PNNL).

The CIA algorithm was developed by Elizabeth Kujawinski and Krista Longnecker 
at the Woods Hole Oceanographic Institution (WHOI).

All publications that utilize this software should provide appropriate acknowledgement to PNNL and the OMICS.PNL.GOV website.

== Disclaimer ==

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

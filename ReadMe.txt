Formularity 

Formularity is software for assignment of low weight molecular formula from high-resolution mass spectra. It includes graphical user interface for two search functions - Compound Identification Algorithm (CIA) and Isotopic Pattern Algorithm (IPA) as well as supporting calibration, alignment and inspection functions. Supporting databases and test files are provided together with software excecutable and source files. 

Formularity is developed by Andrey Liyu and Nikola Tolic at Pacific Northwest National Laboratory (PNNL) and is described in manuscript in preparation. CIA algorithm was developed by Elizabeth Kujawinski and Krista Longnecker at Woods Hole Oceanographic Institution (WHOI).



Area of Research: 

Natural Organic Matter



Related Publication(s): 

Formularity: Software for Automated Formula Assignment of Natural and Derived Organic Matter from Ultra-High Resolution Mass Spectra (in preparation)

Kujawinski, E. B., & Behn, M. D. (2006). Automated analysis of electrospray ionization Fourier transform ion cyclotron resonance mass spectra of natural organic matter. Analytical chemistry, 78(13), 4363-4373.



Version: 

v1.0.0; April 18, 2017



Downloads and Instructions: 

Formularity folder contains source files and executable Formularity.exe.
Attachments folder contain databases, test files and user manual.

Formularity runs on 64-bit version of Windows; no installer is provided; copy entire Formularity folder on desired location; program executable is Formularity.exe in subfolder \CiaUi\bin\x64\Debug\.

To test program use provided "test" files and databases

1) activate tab "CIA formula finding"; drag & drop CIA database to database box

2) activate tab "IPA formula finding"; drag & drop IPA database to database box

3) use "Load parameters" to load test_parameters.xml

4) to use internal calibration select regression model and drop calibration peaks file Neg_ESI_CalibrationPeaks.ref on calibration box. If used, both search functions use calibrated peaks

5) check one or both of CIA and IPA check boxes for desired search. Drop test_peaks.txt file with list of peaks on files box in top right corner to start the search  

CIA search will produce Report file with details of internal calibration in time stamped log file.
IPA search will produce two files per spectrum "s_input_file_name" and "p_input_file_name".

Additional details are available in UserManual.doc document; please let us know if you have any questions, comments or bugs to report.



Acknowledgment

All publications that utilize this software should provide appropriate acknowledgement to PNNL and the OMICS.PNL.GOV website.



Disclaimer

These programs are primarily designed to run on Windows machines. Please use them at your own risk. This material was prepared as an account of work sponsored by an agency of the United States Government. Neither the United States Government nor the United States Department of Energy, nor Battelle, nor any of their employees, makes any warranty, express or implied, or assumes any legal liability or responsibility for the accuracy, completeness, or usefulness or any information, apparatus, product, or process disclosed, or represents that its use would not infringe privately owned rights.

Portions of this research were supported by the W.R. Wiley Environmental Molecular Science Laboratory (a national scientific user facility sponsored by the U.S. Department of Energy's Office of Biological and Environmental Research and located at PNNL). PNNL is operated by Battelle Memorial Institute for the U.S. Department of Energy under contract DE-AC05-76RL0 1830.

We would like your feedback about the usefulness of the tools and information provided by the Resource. Your suggestions on how to increase their value to you will be appreciated. Please e-mail any comments to proteomics@pnl.gov

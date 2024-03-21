# __<span style="color:#D57500">Formultitude</span>__
Formultitude is software for assignment of low weight molecular formula from high-resolution mass spectra.

### Description
Formultitude includes graphical user interface for two search functions - Compound Identification Algorithm (CIA) and Isotopic Pattern Algorithm (IPA) as well as supporting calibration, alignment and inspection functions. Supporting databases and test files are provided together with software excecutable and source files.

Formultitude is developed by Andrey Liyu and Nikola Tolic at Pacific Northwest National Laboratory (PNNL) and is described in manuscript "[Formularity: Software for Automated Formula Assignment of Natural and Other Organic Matter from Ultrahigh-Resolution Mass Spectra](https://pubmed.ncbi.nlm.nih.gov/29120613/)". CIA algorithm was developed by Elizabeth Kujawinski and Krista Longnecker at Woods Hole Oceanographic Institution (WHOI).

### Related Publications
* [Formularity: Software for Automated Formula Assignment of Natural and Other Organic Matter from Ultrahigh-Resolution Mass Spectra](https://pubmed.ncbi.nlm.nih.gov/29120613/)
* [Kujawinski, E. B., & Behn, M. D. (2006). Automated analysis of electrospray ionization Fourier transform ion cyclotron resonance mass spectra of natural organic matter. Analytical chemistry, 78(13), 4363-4373.](https://pubmed.ncbi.nlm.nih.gov/16808443/)

### Downloads
* [Latest version](https://github.com/PNNL-Comp-Mass-Spec/Formultitude/releases/latest)
* [Source code on GitHub](https://github.com/PNNL-Comp-Mass-Spec/Formultitude)
* [CIA_DB_2016_11_21.bin database](https://github.com/PNNL-Comp-Mass-Spec/Formultitude/releases/download/v1.0.7947/CIA_DB_2016_11_21_Database.zip)

#### Software Instructions
Formultitude folder contains source files and executable Formultitude.exe. <br>
Attachments folder contain databases, test files and user manual.

Formultitude runs on 64-bit version of Windows; no installer is provided; copy entire Formultitude folder on desired location; program executable is Formultitude.exe in subfolder \CiaUi\bin\x64\Debug\.

To test program use provided "test" files and databases

1. Activate tab "CIA formula finding"; drag & drop CIA database to database box
2. Activate tab "IPA formula finding"; drag & drop IPA database to database box
3. Use "Load parameters" to load test_parameters.xml
4. To use internal calibration select regression model and drop calibration peaks file Neg_ESI_CalibrationPeaks.ref on calibration box. If used, both search functions use calibrated peaks
5. Check one or both of CIA and IPA check boxes for desired search. Drop test_peaks.txt file with list of peaks on files box in top right corner to start the search  

CIA search will produce Report file with details of internal calibration in time stamped log file. <br>
IPA search will produce two files per spectrum "s_input_file_name" and "p_input_file_name".

Additional details are available in UserManual.doc document; please let us know if you have any questions, comments or bugs to report.

### Acknowledgment

All publications that utilize this software should provide appropriate acknowledgement to PNNL and the Formultitude publication. However, if the software is extended or modified, then any subsequent publications should include a more extensive statement, as shown in the Readme file for the given application or on the website that more fully describes the application.

### Disclaimer

These programs are primarily designed to run on Windows machines. Please use them at your own risk. This material was prepared as an account of work sponsored by an agency of the United States Government. Neither the United States Government nor the United States Department of Energy, nor Battelle, nor any of their employees, makes any warranty, express or implied, or assumes any legal liability or responsibility for the accuracy, completeness, or usefulness or any information, apparatus, product, or process disclosed, or represents that its use would not infringe privately owned rights.

Portions of this research were supported by the NIH National Center for Research Resources (Grant RR018522), the W.R. Wiley Environmental Molecular Science Laboratory (a national scientific user facility sponsored by the U.S. Department of Energy's Office of Biological and Environmental Research and located at PNNL), and the National Institute of Allergy and Infectious Diseases (NIH/DHHS through interagency agreement Y1-AI-4894-01). PNNL is operated by Battelle Memorial Institute for the U.S. Department of Energy under contract DE-AC05-76RL0 1830.

We would like your feedback about the usefulness of the tools and information provided by the Resource. Your suggestions on how to increase their value to you will be appreciated. Please e-mail any comments to proteomics@pnl.gov

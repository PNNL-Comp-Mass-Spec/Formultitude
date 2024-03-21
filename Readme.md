## Formultitude Overview

Formultitude is software for assignment of low weight molecular formulas to peaks in high-resolution mass spectra.
It includes a graphical user interface with two search functions: 
* Compound Identification Algorithm (CIA)
* Isotopic Pattern Algorithm (IPA),

It also includes calibration, alignment and inspection functions.

Related Publication(s): 
* Formularity: Software for Automated Formula Assignment of Natural and Derived Organic Matter from Ultra-High Resolution Mass Spectra
  * N Tolić, Y Liu, A Liyu, Y Shen, MM Tfaily, EB Kujawinski, K Longnecker, LJ Kuo, EW Robinson, L Paša-Tolić, and NJ Hess. Analytical Chemistry. 2017 Dec 5; 89(23), 12659-12665. [PMID 29120613](https://pubmed.ncbi.nlm.nih.gov/29120613/)
* Automated analysis of electrospray ionization Fourier transform ion cyclotron resonance mass spectra of natural organic matter
  * EB Kujawinski and MD Behn. Analytical Chemistry. 2006 Jul 1; 78(13), 4363-4373. [PMID 16808443](https://pubmed.ncbi.nlm.nih.gov/16808443/)

## Downloads and Instructions

Formultitude runs on 64-bit Windows 10 or newer; no installer is provided
* It requires [.NET Framework 4.8](https://dotnet.microsoft.com/en-us/download/dotnet-framework/net48)
* Extract the contents of `Formultitude.zip`, then run `Formultitude.exe`
* Documentation is in file `UserManual.pdf` in the Documents folder [on GitHub](https://github.com/PNNL-Comp-Mass-Spec/Formultitude/tree/master/Documents)

To test the program, use the provided test files in the `Documents\TestData_FTICR` folder
* The CIA and IPA databases are available as a separate download
* See file `Databases_CIA_DB_WHOI_2016_and_IPQ_DB_LQ_v5.zip` [on GitHub](https://github.com/PNNL-Comp-Mass-Spec/Formultitude/releases/tag/v1.0.8475), which has two files:
  * `ciadb_WHOI_2016_11_21.bin`
  * `ipadb_LQ_v5_80384_12_4.txt`

### Analysis Steps

1) Select tab "CIA formula finding"
* Drag & drop the CIA database (file `ciadb_WHOI_2016_11_21.bin`) to the database box (labeled "Drop DB files")
* Database load time will take 30 to 60 seconds

2) Optional (since IPA formula finding is an independent function): Select tab "IPA formula finding"
* Drag & drop IPA database (`ipadb_LQ_v5_80384_12_4.txt`) to the database box (labeled "Drop DB file")

3) Use "Load parameters" at the bottom to load `test_parameters.xml`

4) To use internal calibration, select the regression model (none, linear, or quadratic) then 
drag/drop the calibration peaks file (`neg_ESI_NOM_calibration_peaks.ref`) to the 
calibration box (labeled "Drop calibration file")
* If used, both search functions will use calibrated peaks

5) Enable either or both of the CIA and IPA check boxes for the desired search
* The "Drop Spectra Files" area will turn green

6) Drag/drop files `data_fticr_neg_esi_1.txt` and `data_fticr_neg_esi_2.txt` to the "Drop Spectra Files" area at the upper right of the window
* The area will turn red while it is processing, then green when the search is complete

CIA search will produce a Report file with details of internal calibration in a time-stamped log file
* Example name, `Report20230317111902.log`

IPA search will produce two files per spectrum: `s_input_file_name` and `p_input_file_name`
* For example: `s_ipdbdata_fticr_neg_esi_1.csv` and `p_ipdbdata_fticr_neg_esi_1.csv`

## Contact Info

E-mail any questions or comments to proteomics@pnnl.gov

## Acknowledgments

Formultitude was developed by Andrey Liyu and Nikola Tolić at 
Pacific Northwest National Laboratory (PNNL)

The CIA algorithm was developed by Elizabeth Kujawinski and Krista Longnecker 
at the Woods Hole Oceanographic Institution (WHOI)

All publications that utilize this software should provide appropriate acknowledgment to PNNL and Formultitude's GitHub website

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

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.InteropServices;
using System.Xml;

using TestFSDBSearch;
using Support;

namespace CIA
{
    internal static class FormularityProgram
    {
        private static int Main(string[] args)
        {
            try
            {
                System.Console.WriteLine("Started.");
                var oCCia = new CCia();
                if ((args.Length == 1) && ((args[0] == "\\h") || (args[0] == "/h") || (args[0] == "-h")))
                {
                    throw new Exception("Incorrect Help argument.");
                }
                if ((args.Length < 3) || (args.Length > 5))
                {
                    throw new Exception("Application uses 3, 4 or 5 arguments.");
                }

                //args[0] - CIA or IPA method
                var CiaOrIpa = false;
                if (string.Equals(args[0], "cia", StringComparison.OrdinalIgnoreCase))
                {
                    oCCia.SetProcessType(CCia.ProcessType.Cia);
                    //CiaOrIpa = false;
                }
                else if (string.Equals(args[0], "ipa", StringComparison.OrdinalIgnoreCase))
                {
                    oCCia.SetProcessType(CCia.ProcessType.Ipa);
                    //CiaOrIpa = true;
                }
                else
                {
                    throw new Exception("Argument 1 [" + args[0] + " is not CIA ot IPA.");
                }

                //args[1] - data file path (spectrum file path)
                var FileOrDirectory = false;// False if args[1] is a file (or filename with a wildcard); true if args[1] is a directory
                bool wildcardFileSpec;
                try
                {
                    if (args[1].Contains("*") || args[1].Contains("?"))
                    {
                        // Assume it points to a set of files in the given directory
                        FileOrDirectory = false;
                        wildcardFileSpec = true;
                    }
                    else
                    {
                        FileOrDirectory = (File.GetAttributes(args[1]) & FileAttributes.Directory) == FileAttributes.Directory;
                        wildcardFileSpec = false;
                        //File.GetAttributes() also check File.Exists();
                    }
                    //File.GetAttributes() also check File.Exists();
                }
                catch (Exception ex)
                {
                    throw new Exception("Argument 2 exception: " + ex.Message);
                }
                var DatasetsList = new List<string>();
                if (!FileOrDirectory)
                {
                    //if ( ( Path.GetExtension( args [ 1 ] ) == ".csv" )
                    //|| ( Path.GetExtension( args [ 1 ] ) == ".txt" )
                    //|| ( Path.GetExtension( args [ 1 ] ) == ".xml" ) ) {
                    //DatasetsList.Add( args [ 1 ] );
                    //} else {
                    // throw new Exception( "File in argument 2 must be xml, csv or txt type file" );
                    if (wildcardFileSpec)
                    {
                        try
                        {
                            var placeholderFile = new FileInfo(args[1].Replace("*", "_").Replace("?", "_"));
                            if (placeholderFile.Directory == null)
                            {
                                throw new Exception("Unable to determine the parent directory of " + args[1]);
                            }
                            string wildcardMatchSpec;
                            var lastSlash = args[1].LastIndexOf('\\');
                            if (lastSlash >= 0)
                            {
                                wildcardMatchSpec = args[1].Substring(lastSlash + 1);
                            }
                            else
                            {
                                wildcardMatchSpec = args[1];
                            }
                            foreach (var dataFile in placeholderFile.Directory.GetFiles(wildcardMatchSpec))
                            {
                                DatasetsList.Add(dataFile.FullName);
                            }
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("Argument 2 exception: " + ex.Message);
                        }
                        if (DatasetsList.Count == 0)
                        {
                            throw new Exception("No files found matching \n" + args[1] + "\n");
                        }
                    }
                    else
                    {
                        if ((Path.GetExtension(args[1]) == ".csv")
                            || (Path.GetExtension(args[1]) == ".txt")
                            || (Path.GetExtension(args[1]) == ".tsv")
                            || (Path.GetExtension(args[1]) == ".xml"))
                        {
                            DatasetsList.Add(args[1]);
                        }
                        else
                        {
                            throw new Exception("File in argument 2 must be xml, csv, tsv (ThermoPeakDataExporter .tsv), or txt (DeconTools _peaks.txt) file.");
                        }
                    }
                }
                else
                {
                    DatasetsList.AddRange(Directory.GetFiles(args[1], "*.csv"));
                    DatasetsList.AddRange(Directory.GetFiles(args[1], "*.tsv"));
                    DatasetsList.AddRange(Directory.GetFiles(args[1], "*.txt"));
                    DatasetsList.AddRange(Directory.GetFiles(args[1], "*.xml"));
                    if (DatasetsList.Count == 0)
                    {
                        throw new Exception("Folder in argument 2 doesn't have xml, csv or txt files");
                    }
                }

                //args[2] - xml parameter file
                if (!File.Exists(args[2]))
                {
                    throw new Exception("Argument 3 XML parameter file " + args[2] + " does not exist");
                }
                else if (Path.GetExtension(args[2]) != ".xml")
                {
                    throw new Exception("Argument 3 XML parameter file " + args[2] + " doesn't have XML files");
                }
                oCCia.LoadParameters(args[2]);
                System.Console.WriteLine("Loaded parameters.");
                if (!CiaOrIpa)
                {
                    if (oCCia.GetAlignment())
                    {
                        if (oCCia.GetRelationErrorType() == CCia.RelationErrorType.DynamicPPM)
                        {
                            throw new Exception("in parameter file: multiple file alignment can't be used dynamic ppm relationship error.");
                        }
                        else if (oCCia.GetStaticDynamicPpmError())
                        {
                            throw new Exception("in parameter file: multiple file alignment can't be used dynamic ppm formula tolerance.");
                        }
                    }
                }
                if (FileOrDirectory && DatasetsList.Count > 1)
                {
                    // Make sure the parameter file is not in DatasetsList
                    var paramFileInfo = new FileInfo(args[2]);
                    if (DatasetsList.Contains(paramFileInfo.FullName))
                    {
                        DatasetsList.Remove(paramFileInfo.FullName);
                    }
                }

                //optional args[ 3 ] - db file (optional if it is in Parameter file)
                if (args.Length > 3)
                {
                    if (!CiaOrIpa)
                    {
                        if (!File.Exists(args[3]))
                        {
                            throw new Exception("CIA DB file (argument 4: " + args[3] + ") does not exist");
                        }
                        oCCia.LoadCiaDB(args[3]);
                    }
                    else
                    {
                        if (!File.Exists(args[3]))
                        {
                            throw new Exception("IPA DB file (Argument 4: " + args[3] + ") does not exist");
                        }
                        oCCia.Ipa.LoadTabulatedDB(args[3]);
                    }
                }
                else
                {
                    if (!CiaOrIpa)
                    {
                        if (!File.Exists(oCCia.GetCiaDBFilename()))
                        {
                            throw new Exception("CIA DB file (parameter file: " + oCCia.GetCiaDBFilename() + ") does not exist");
                        }
                        oCCia.LoadCiaDB(oCCia.GetCiaDBFilename());
                    }
                    else
                    {
                        if (!File.Exists(oCCia.GetIpaDBFilename()))
                        {
                            throw new Exception("IPA DB file (parameter file: " + args[3] + ") does not exist");
                        }
                        oCCia.Ipa.LoadTabulatedDB(oCCia.GetIpaDBFilename());
                    }
                }
                System.Console.WriteLine("Loaded DB.");

                //args[ 4] - ref calibration file (optional)
                if (oCCia.oTotalCalibration.ttl_cal_regression != TotalCalibration.ttlRegressionType.none)
                {
                    if (args.Length == 5)
                    {
                        if (!File.Exists(args[4]))
                        {
                            throw new Exception("REF calibration file (argument 5: " + args[4] + ") does not exist");
                        }
                        oCCia.SetRefPeakFilename(args[4]);
                        //oCCia.oTotalCalibration.Load( args [ 4 ] );
                        System.Console.WriteLine("Loaded calibration.");
                    }
                    else
                    {
                        if (!File.Exists(oCCia.GetRefPeakFilename()))
                        {
                            throw new Exception("Argument 5 REF calibration file (parameter file: " + oCCia.GetRefPeakFilename() + ") does not exist");
                        }
                        //oCCia.oTotalCalibration.Load( oCCia.GetRefPeakFilename() );
                    }
                }
                else
                {
                    System.Console.WriteLine("Calibration is not required.");
                }

                System.Console.WriteLine("Checked arguments.");

                var Filenames = DatasetsList.ToArray();
                oCCia.Process(Filenames);

                System.Console.WriteLine("Finished.");
                return 0;
            }
            catch (Exception ex)
            {
                //System.Console.WriteLine( "Error: " + ex.Message );
                ReportError(ex, false);
                PrintHelp();
                return -1;
            }
        }

        private static void PrintHelp()
        {
            System.Console.WriteLine("Help:");
            System.Console.WriteLine("Command line: cia.exe arg1 arg2 arg3 arg4 arg5");
            System.Console.WriteLine("where");
            System.Console.WriteLine("   arg1 is CIA or IPA");
            System.Console.WriteLine("   arg2 is dataset or folder with dataset(s)");
            System.Console.WriteLine("   arg3 is xml parameter file");
            System.Console.WriteLine("   arg4 (optional) is bin db file; can be added through parameter file");
            System.Console.WriteLine("   arg5 (optional) is ref calibration file; can be not used or added through parameter file");
        }
        public static void ReportError(Exception ex, bool throwException = true)
        {
            ReportError(ex.Message);
            if (throwException)
                throw ex;

            Console.WriteLine();
            Console.WriteLine(ex.StackTrace);
        }
        public static void ReportError(string errorMessage)
        {
            Console.WriteLine();
            if (errorMessage.StartsWith("Error:"))
                Console.WriteLine(errorMessage);
            else
                Console.WriteLine("Error: " + errorMessage);
        }
        public static void AppendToLog(StreamWriter logWriter, string message)
        {
            if (message.EndsWith("\n"))
            {
                Console.WriteLine(message);
                logWriter.WriteLine(message);
            }
            else
            {
                Console.Write(message);
                logWriter.Write(message);
            }
        }
    }

    internal class Data
    {
        //arrays per file
        //raw
        public int FileCount;
        public string[] Filenames;
        public double[][] Masses;
        public double[][] Abundances;
        public double[][] SNs;
        public double[][] Resolutions;
        public double[][] RelAbundances;

        //processed
        public double[][] CalMasses;
        public double[][] AlignMasses;
        public double[][] NeutralMasses;
        public short[][][] Formulas;
        public double[][] PPMErrors;
        public short[][] Candidates;
    }
    public class AlignData
    {
        public double[] AlignMasses;
        public double[] NeutralMasses;
        public int[][] Indexes;
        public short[][] Formulas;
        public double[] PPMErrors;
        public short[] Candidates;
    }
    public class CCia
    {
        //Elements
        public const int ElementCount = 8;
        public enum EElemIndex { C = 0, H, O, N, C13, S, P, Na };

        private readonly double[] ElementMasses = { CElements.C, CElements.H, CElements.O, CElements.N, CElements.C13, CElements.S, CElements.P, CElements.Na };
        private readonly short[] ElemValences = { 4, 1, 2, 3, 4, 2, 3, 1 };

        //Formula
        private readonly short[] NullFormula = new short[ElementCount];
        public double FormulaToNeutralMass(short[] Formula)
        {
            if (Formula == null)
            {
                throw new Exception("Formula array is null");
            }
            if (Formula.Length != ElementCount)
            {
                throw new Exception("Formula array length (" + Formula.Length + ") must be " + ElementCount);
            }
            double NeutralMass = 0;
            for (var Element = 0; Element < ElementCount; Element++)
            {
                NeutralMass = NeutralMass + Formula[Element] * ElementMasses[Element];
            }
            return NeutralMass;
        }
        public bool IsFormula(short[] Formula)
        {
            foreach (var Element in Formula) { if (Element > 0) { return true; } }
            return false;
        }

        private bool AreFormulasEqual(IList<short> Formula1, IList<short> Formula2)
        {
            if (Formula1.Count != Formula2.Count) { return false; }
            for (var Element = 0; Element < Formula1.Count; Element++)
            {
                if (Formula1[Element] != Formula2[Element]) { return false; }
            }
            return true;
        }
        public string FormulaToName(short[] Formula)
        {
            var FormulaName = string.Empty;
            for (var Element = 0; Element < Formula.Length; Element++)
            {
                if (Formula[Element] == 0) { continue; }
                FormulaName += Enum.GetName(typeof(EElemIndex), Element);
                if (Formula[Element] == 1) { continue; }
                FormulaName += Formula[Element];
            }
            return FormulaName;
        }
        public short[] NameToFormula(string FormulaName)
        {
            var Formula = (short[])NullFormula.Clone();
            if (FormulaName.Length == 0) { return Formula; }

            for (var CurrentSymbol = 0; CurrentSymbol < FormulaName.Length;)
            {
                //Element
                string ElementName;
                if (!Char.IsLetter(FormulaName[CurrentSymbol]))
                {
                    throw new Exception("Formula name (" + FormulaName + "is wrong");
                }
                if ((FormulaName.Length > CurrentSymbol + 1) && Char.IsLetter(FormulaName[CurrentSymbol + 1]))
                {
                    //Maybe Element name consists of 2 letters
                    ElementName = FormulaName.Substring(CurrentSymbol, 2);
                    //check that is not like CH2
                    EElemIndex oEElemNumber;
                    if (!Enum.TryParse<EElemIndex>(ElementName, out oEElemNumber))
                    {
                        ElementName = FormulaName.Substring(CurrentSymbol, 1);
                    }
                }
                else
                {
                    ElementName = FormulaName.Substring(CurrentSymbol, 1);
                }
                CurrentSymbol = CurrentSymbol + ElementName.Length;
                //Element's atoms
                var Negative = false;
                if (FormulaName.Length > CurrentSymbol)
                {
                    if (FormulaName[CurrentSymbol] == '-')
                    {
                        Negative = true;
                        CurrentSymbol = CurrentSymbol + 1;
                    }
                }
                var DigitCount = 0;
                while ((FormulaName.Length > CurrentSymbol + DigitCount) && Char.IsDigit(FormulaName[CurrentSymbol + DigitCount]))
                {
                    DigitCount = DigitCount + 1;
                }
                short ElementNumber = 1;
                if (DigitCount > 0)
                {
                    ElementNumber = Int16.Parse(FormulaName.Substring(CurrentSymbol, DigitCount));
                }
                if (Negative)
                {
                    ElementNumber = (short)-ElementNumber;
                }
                Formula[(int)Enum.Parse(typeof(EElemIndex), ElementName)] = ElementNumber;
                CurrentSymbol = CurrentSymbol + DigitCount;
            }
            return Formula;
        }

        //error
        //const double PPM = 1e6;//parts per million
        //public double PpmToError( double Mass, double ErrorPPM ) { return Mass * ErrorPPM / PPM; }
        //public static double ErrorToPpm( double Mass, Double Error ) { return Error * PPM / Mass; }
        //public double SignedMassErrorPPM( double ReferenceMass, double Mass ) { return ( Mass - ReferenceMass ) / ReferenceMass * PPM; }
        //public double AbsMassErrorPPM( double ReferenceMass, double Mass ) { return Math.Abs( ( Mass - ReferenceMass ) / ReferenceMass * PPM ); }

        //***********
        //Input files
        //***********
        //string [] Filenames;
        //******************************************************************************************
        //Pre-alignment
        //******************************************************************************************
        public bool PreAlignment = true;
        public bool GetPreAlignment() { return PreAlignment; }
        public void SetPreAlignment(bool PreAlignment) { this.PreAlignment = PreAlignment; }
        //******************************************************************************************
        //Calibration
        //******************************************************************************************
        public TestFSDBSearch.TotalCalibration oTotalCalibration = new TotalCalibration();
        public string RefPeakFilename = "";
        public string GetRefPeakFilename() { return RefPeakFilename; }
        public void SetRefPeakFilename(string RefPeakFilename) { this.RefPeakFilename = RefPeakFilename; }
        //******************************************************************************************
        //Alignment
        //******************************************************************************************
        public bool Alignment = true;
        public bool GetAlignment() { return Alignment; }
        public void SetAlignment(bool Alignment) { this.Alignment = Alignment; }
        private double AlignmentPPMTolerance = 1;//default
        public double GetAlignmentPpmTolerance() { return AlignmentPPMTolerance; }
        public void SetAlignmentPpmTolerance(double PPMTolerance)
        {
            AlignmentPPMTolerance = PPMTolerance;
        }

        private void AlignmentByPeak()
        {
            //Group near peaks as one peak.
            //Peak from the same file can't be grouped
            //New mass calculation is linear (doesn't use peak Abundance)
            var IndexesTemplate = new int[oData.FileCount + 1];//extra last row for weight counting
            var TotalPeakCount = 0;
            for (var FileIndex = 0; FileIndex < oData.FileCount; FileIndex++)
            {
                IndexesTemplate[FileIndex] = -1;//Existing index can be 0 and positive
                TotalPeakCount = TotalPeakCount + oData.Masses[FileIndex].Length;
            }
            IndexesTemplate[oData.FileCount] = 1;
            var TotalMasses = new double[TotalPeakCount];
            var TotalIndexes = new int[TotalPeakCount][];
            var FilePeakShift = 0;
            for (var FileIndex = 0; FileIndex < oData.FileCount; FileIndex++)
            {
                var FilePeakCount = oData.Masses[FileIndex].Length;
                var AlignMasses = oData.CalMasses[FileIndex];
                if (AlignMasses == null)
                {
                    throw new Exception(string.Format("oData.CalMasses is null for FileIndex {0}", FileIndex));
                }
                for (var Peak = 0; Peak < FilePeakCount; Peak++)
                {
                    TotalMasses[FilePeakShift + Peak] = AlignMasses[Peak];
                    TotalIndexes[FilePeakShift + Peak] = (int[])IndexesTemplate.Clone();
                    TotalIndexes[FilePeakShift + Peak][FileIndex] = Peak;
                }
                FilePeakShift = FilePeakShift + FilePeakCount;
            }
            Array.Sort(TotalMasses, TotalIndexes);
            var MassesL = TotalMasses.ToList<double>();
            var IndexesL = TotalIndexes.ToList<int[]>();
            TotalMasses = null;
            TotalIndexes = null;

            //remove peaks with the same mass
            for (var Peak = 0; Peak < TotalPeakCount - 1;)
            {
                if (MassesL[Peak] == MassesL[Peak + 1])
                {
                    var CurrentIndexes = IndexesL[Peak];
                    var NextIndexes = IndexesL[Peak + 1];
                    for (var Index = 0; Index < oData.FileCount; Index++)
                    {
                        if (CurrentIndexes[Index] >= 0)
                        {
                            NextIndexes[Index] = CurrentIndexes[Index];//move current valueable peaks to next
                        }
                    }
                    NextIndexes[oData.FileCount] = NextIndexes[oData.FileCount] + CurrentIndexes[oData.FileCount];//for weight
                    MassesL.RemoveAt(Peak);//remove current peak
                    IndexesL.RemoveAt(Peak);
                    TotalPeakCount = TotalPeakCount - 1;
                }
                else
                {
                    Peak = Peak + 1;
                }
            }
            //align peaks
            //look on left and right peaks; group the nearest peak if error is < Alignment
            for (var Peak = 1; Peak < TotalPeakCount - 1;)
            {
                var PeakError = CPpmError.PpmToError(MassesL[Peak], AlignmentPPMTolerance);//it strange but it is correct to Matlab code
                var LeftDistance = MassesL[Peak] - MassesL[Peak - 1];
                var RightDistance = MassesL[Peak + 1] - MassesL[Peak];
                var CurrentIndexes = IndexesL[Peak];
                //left peak
                var IsLeftPeakUnique = false;
                if (LeftDistance >= PeakError) { IsLeftPeakUnique = true; }
                if (!IsLeftPeakUnique)
                {
                    var LeftIndexes = IndexesL[Peak - 1];
                    for (var Index = 0; Index < oData.FileCount; Index++)
                    {
                        if ((LeftIndexes[Index] >= 0) && (CurrentIndexes[Index] >= 0))
                        {
                            IsLeftPeakUnique = true;
                            break;
                        }
                    }
                }
                if ((!IsLeftPeakUnique) && (LeftDistance <= RightDistance))
                {// Left + Current maybe become Left
                    var LeftIndexes = IndexesL[Peak - 1];
                    MassesL[Peak - 1] = (MassesL[Peak] * CurrentIndexes[oData.FileCount] + MassesL[Peak - 1] * LeftIndexes[oData.FileCount]) / (CurrentIndexes[oData.FileCount] + LeftIndexes[oData.FileCount]);
                    for (var Index = 0; Index < oData.FileCount; Index++)
                    {
                        if (CurrentIndexes[Index] >= 0)
                        {
                            LeftIndexes[Index] = CurrentIndexes[Index];//move current peaks to left
                        }
                    }
                    LeftIndexes[oData.FileCount] = LeftIndexes[oData.FileCount] + CurrentIndexes[oData.FileCount];//for weight
                    MassesL.RemoveAt(Peak);//remove current peak
                    IndexesL.RemoveAt(Peak);
                    TotalPeakCount = TotalPeakCount - 1;
                    continue;
                }

                //right peak
                if (RightDistance >= PeakError) { Peak = Peak + 1; continue; }
                var IsRightPeakUnique = false;
                var RightIndexes = IndexesL[Peak + 1];
                for (var Index = 0; Index < oData.FileCount; Index++)
                {
                    if ((RightIndexes[Index] >= 0) && (CurrentIndexes[Index] >= 0))
                    {
                        IsRightPeakUnique = true;
                        break;
                    }
                }
                if (IsRightPeakUnique)
                {
                    Peak = Peak + 1;
                    continue;
                }
                else
                {
                    MassesL[Peak + 1] = (MassesL[Peak] * CurrentIndexes[oData.FileCount] + MassesL[Peak + 1] * RightIndexes[oData.FileCount]) / (CurrentIndexes[oData.FileCount] + RightIndexes[oData.FileCount]);
                    for (var Index = 0; Index < oData.FileCount; Index++)
                    {
                        if (CurrentIndexes[Index] >= 0)
                        {
                            RightIndexes[Index] = CurrentIndexes[Index];//move current peaks to right
                        }
                    }
                    RightIndexes[oData.FileCount] = RightIndexes[oData.FileCount] + CurrentIndexes[oData.FileCount];//for weight
                    MassesL.RemoveAt(Peak);
                    IndexesL.RemoveAt(Peak);
                    TotalPeakCount = TotalPeakCount - 1;
                    continue;
                }
            }
            oAlignData.AlignMasses = MassesL.ToArray();
            oAlignData.NeutralMasses = new double[oAlignData.AlignMasses.Length];
            oAlignData.Indexes = IndexesL.ToArray();
            oAlignData.Formulas = new short[oAlignData.AlignMasses.Length][];
            for (var Peak = 0; Peak < oAlignData.AlignMasses.Length; Peak++)
            {
                oAlignData.Formulas[Peak] = (short[])NullFormula.Clone();
            }
            oAlignData.Candidates = new short[oAlignData.AlignMasses.Length];
            oAlignData.PPMErrors = new double[oAlignData.AlignMasses.Length];
        }
        //class Line {
        //    public short [] Formulas;
        //    public int [] Indexes;
        //}
        /*
        void AlignmentByFormula (){
            int [] IndexesTemplate = new int [ oData.FileCount];//extra last row for weight counting
            for( int FileIndex = 0; FileIndex < oData.FileCount; FileIndex++ ) {
                IndexesTemplate[ FileIndex ] = -1;
            }

            SortedDictionary<double, Line> AlignDict = new SortedDictionary<double, Line>();
            for( int FileIndex = 0; FileIndex < oData.FileCount; FileIndex ++){
                double [] Masses = oData.NeutralMasses[ FileIndex];
                short [] [] Formulas = oData.Formulas[ FileIndex];
                for( int Peak = 0; Peak < oData.NeutralMasses.Length; Peak++ ) {
                    if( IsFormula( Formulas [ Peak ] ) == false ) { continue; }
                    double NeutralMass = FormulaToNeutralMass( Formulas [ Peak ] );
                    if( AlignDict.ContainsKey( NeutralMass ) == true ) {
                        AlignDict [ NeutralMass ].Indexes [ FileIndex ] = Peak;
                    } else {
                        Line TempLine = new Line();
                        TempLine.Formulas = ( short [] ) Formulas [ Peak ].Clone();
                        TempLine.Indexes = (int[]) IndexesTemplate.Clone();
                        TempLine.Indexes [ FileIndex ] = Peak;
                    }
                }
            }
            oAlignData.AlignMasses = new double [ AlignDict.Count ];
            oAlignData.NeutralMasses = new double [ AlignDict.Count ];
            oAlignData.Formulas = new short [ AlignDict.Count ][];
            oAlignData.Indexes = new int [ AlignDict.Count ][];
            oAlignData.PPMErrors = new double [ AlignDict.Count ];
            oAlignData.Candidates = new short [ AlignDict.Count ];

            int PeakIndex = 0;
            foreach( KeyValuePair<double, Line> PeakPair in AlignDict ) {
                oAlignData.AlignMasses [ PeakIndex ] = PeakPair.Key;
                oAlignData.NeutralMasses[ PeakIndex] = PeakPair.Key;
                oAlignData.Formulas [ PeakIndex] = PeakPair.Value.Formulas;
                oAlignData.Indexes [ PeakIndex] = PeakPair.Value.Indexes;
                int ValueableFiles = 0;
                double Error = 0;
                int Candidates = 0;
                for( int Index = 0; Index < oData.FileCount; Index ++){
                    if( oAlignData.Indexes [ PeakIndex ] [ Index ] < 0 ) { continue; }
                    ValueableFiles ++;
                    Error = Error + Math.Abs( oData.PPMErrors[ Index][ oAlignData.Indexes [ PeakIndex][ Index] ] );
                    Candidates = Candidates + oData.Candidates[ Index][ oAlignData.Indexes [ PeakIndex][ Index] ];
                }
                oAlignData.PPMErrors [ PeakIndex] = Error / ValueableFiles;
                oAlignData.Candidates [ PeakIndex] = (short) Math.Round( (double) Candidates / ValueableFiles);
                PeakIndex ++;
            }
        }
        */
        //******************************************************************************************
        //Formula finding
        //******************************************************************************************
        //Formula error
        private bool StaticDynamicPpmError = false;
        public bool GetStaticDynamicPpmError() { return StaticDynamicPpmError; }
        public void SetStaticDynamicPpmError(bool StaticDynamicPpmError) { this.StaticDynamicPpmError = StaticDynamicPpmError; }
        private double FormulaPPMTolerance = 1;//default
        public double GetFormulaPPMTolerance() { return FormulaPPMTolerance; }
        public void SetFormulaPPMTolerance(double PPMTolerance) { FormulaPPMTolerance = PPMTolerance; }
        private double StdDevErrorGain = 1;
        public double GetStdDevErrorGain() { return StdDevErrorGain; }
        public void SetStdDevErrorGain(double StdDevErrorGain)
        {
            this.StdDevErrorGain = StdDevErrorGain;
        }
        public double GetRealFormulaPpmError(double PeakMass)
        {
            if (StaticDynamicPpmError)
            {
                return StdDevErrorGain * InputData.GetErrorStdDev(PeakMass);
            }
            else
            {
                return GetFormulaPPMTolerance();
            }
        }
        public Support.InputData InputData;
        public void SetInputData(Support.InputData InputData) { this.InputData = InputData; }
        private CChainBlocks oCChainBlocks;
        public void SetChainBlocks(CChainBlocks oCChainBlocks) { this.oCChainBlocks = oCChainBlocks; }
        private double[] BlockMasses;
        public void SetBlockMasses(double[] BlockMasses) { this.BlockMasses = BlockMasses; }

        //Relations
        private bool UseRelation = true;//default
        public bool GetUseRelation() { return UseRelation; }
        public void SetUseRelation(bool UseRelation) { this.UseRelation = UseRelation; }
        private int MaxRelationGaps = 5;//default
        public int GetMaxRelationGaps() { return MaxRelationGaps; }
        public void SetMaxRelationGaps(int MaxRelationGaps) { this.MaxRelationGaps = MaxRelationGaps; }
        public enum RelationErrorType { AMU, PPM, GapPPM, DynamicPPM };

        private RelationErrorType oRelationErrorType = RelationErrorType.AMU;
        public RelationErrorType GetRelationErrorType() { return oRelationErrorType; }
        public void SetRelationErrorType(RelationErrorType oRelationErrorType) { this.oRelationErrorType = oRelationErrorType; }
        private double RelationErrorAMU = 0.00002;//default
        public double GetRelationErrorAMU() { return RelationErrorAMU; }
        public void SetRelationErrorAMU(double ErrorAMU) { RelationErrorAMU = ErrorAMU; }
        private bool UseBackward = true;
        public bool GetUseBackward() { return UseBackward; }
        public void SetUseBackward(bool UseBackward) { this.UseBackward = UseBackward; }

        public static short[][] RelationBuildingBlockFormulas = {
                new short [] { 1, 2, 0, 0, 0, 0, 0, 0 },//CH2
                new short [] { 1, 4, -1, 0, 0, 0, 0, 0},//CH4O- or CH4O-1
                new short [] { 0, 2, 0, 0, 0, 0, 0, 0 },//H2
                new short [] { 2, 4, 1, 0, 0, 0, 0, 0 },//C2H4O
                new short [] { 1, 0, 2, 0, 0, 0, 0, 0 },//CO2
                new short [] { 2, 2, 1, 0, 0, 0, 0, 0 },//C2H2O
                new short [] { 0, 0, 1, 0, 0, 0, 0, 0 },//O
                //new short [] { 1, 1, 0, 0, 0, 0, 0, 0 },//CH
                new short [] { 0, 1, 0, 1, 0, 0, 0, 0 },//HN
                //new short [] { 0, 0, 3, 0, 0, 0, 1, 0 }//O3P
                new short [] { 1, 2, 1, 0, 0, 0, 0, 0 }//CH2O
        };

        private bool[] ActiveRelationBuildingBlocks = { true, false, true, false, false, false, true, false, false };
        public bool[] GetActiveRelationFormulaBuildingBlocks() { return ActiveRelationBuildingBlocks; }
        public void SetActiveRelationFormulaBuildingBlocks(bool[] ActiveBlocks)
        {
            ListActiveRelationFormulaBuildingBlocks = new List<short[]>();
            var ActiveRelationFormulaBuildingBlockMassesList = new List<double>();
            //ActiveRelationFormulaBuildingBlockMasses = new double [ ListActiveRelationFormulaBuildingBlocks.Count ];
            for (var ActiveBlock = 0; ActiveBlock < ActiveBlocks.Length; ActiveBlock++)
            {
                if (!ActiveBlocks[ActiveBlock]) { continue; }
                double Mass = 0;
                for (var Element = 0; Element < ElementCount; Element++)
                {
                    Mass = Mass + RelationBuildingBlockFormulas[ActiveBlock][Element] * ElementMasses[Element];
                }
                ActiveRelationFormulaBuildingBlockMassesList.Add(Mass);
                ListActiveRelationFormulaBuildingBlocks.Add(RelationBuildingBlockFormulas[ActiveBlock]);
            }
            ActiveRelationFormulaBuildingBlockMasses = ActiveRelationFormulaBuildingBlockMassesList.ToArray();
            ActiveRelationBuildingBlocks = (bool[])ActiveBlocks.Clone();
        }

        private List<short[]> ListActiveRelationFormulaBuildingBlocks;
        //public short [] [] GetActiveRelationFormulaBuildingBlocks() { return ListActiveRelationFormulaBuildingBlocks.ToArray(); }
        private double[] ActiveRelationFormulaBuildingBlockMasses;
        /*
        public void SetRelationFormulaBuildingBlocks( short [][] ActiveBlocks ) {
            ListActiveRelationFormulaBuildingBlocks = new List<short []>( ActiveBlocks );
            ActiveRelationFormulaBuildingBlockMasses = new double [ ListActiveRelationFormulaBuildingBlocks.Count];
            for( int Relation = 0; Relation < ListActiveRelationFormulaBuildingBlocks.Count; Relation++ ) {
                ActiveRelationFormulaBuildingBlockMasses [ Relation ] = 0;
                for( int Element = 0; Element < ElementCount; Element++ ) {
                    ActiveRelationFormulaBuildingBlockMasses [ Relation ] = ActiveRelationFormulaBuildingBlockMasses [ Relation ] +
                            ListActiveRelationFormulaBuildingBlocks [ Relation ] [ Element ] * ElementMasses [ Element ];
                }
            }
        }
        */
        //Mass limit to search in DB
        private double MassLimit = 500;
        public double GetMassLimit() { return MassLimit; }
        public void SetMassLimit(double MassLimit) { this.MassLimit = MassLimit; }

        //formula score
        private bool UseCIAFormulaScore = false;
        public bool GetUseCIAFormulaScore() { return UseCIAFormulaScore; }
        public void SetUseCIAFormulaScore(bool UseCIAFormulaScore) { this.UseCIAFormulaScore = UseCIAFormulaScore; }

        private readonly string[] FormulaScoreNames = {
                "min(S+P) & The lowest error",
                "The lowest error",
                "min(N+S+P) & The lowest error",
                "min(O+N+S+P) & The lowest error",
                "UserDefined"
        };
        public string[] GetFormulaScoreNames() { return FormulaScoreNames; }
        public enum EFormulaScore { MinSPAndError = 0, lowestError = 1, MinNSPAndError = 2, MinONSPAndError = 3, UserDefined = 4 };
        public EFormulaScore FormulaScore = EFormulaScore.MinNSPAndError;//default
        public EFormulaScore GetFormulaScore() { return FormulaScore; }
        public void SetFormulaScore(EFormulaScore FormulaScore) { this.FormulaScore = FormulaScore; }

        //User-defined score
        private System.Data.DataTable UserDefinedScoreTable;
        private string UserDefinedScore = "";
        public string GetUserDefinedScore() { return UserDefinedScore; }
        private readonly string PrefixFirst = "First";
        private readonly string PrefixSecond = "Second";
        public void SetUserDefinedScore(string UserDefinedScore)
        {
            if (UserDefinedScore.Length == 0)
            {
                UserDefinedScoreTable = null;
                return;
            }
            UserDefinedScoreTable = new System.Data.DataTable();
            //PrefixFirst
            UserDefinedScoreTable.Columns.Add(PrefixFirst + "Mass", typeof(double));
            UserDefinedScoreTable.Columns.Add(PrefixFirst + "Error", typeof(double));
            foreach (var Name in Enum.GetNames(typeof(EElemIndex)))
            {
                UserDefinedScoreTable.Columns.Add(PrefixFirst + Name, typeof(short));
            }
            //PrefixSecond
            UserDefinedScoreTable.Columns.Add(PrefixSecond + "Mass", typeof(double));
            UserDefinedScoreTable.Columns.Add(PrefixSecond + "Error", typeof(double));
            foreach (var Name in Enum.GetNames(typeof(EElemIndex)))
            {
                UserDefinedScoreTable.Columns.Add(PrefixSecond + Name, typeof(short));
            }

            UserDefinedScoreTable.Columns.Add("UserDefinedScore", typeof(bool), UserDefinedScore);
            UserDefinedScoreTable.Rows.Add(UserDefinedScoreTable.NewRow());
            this.UserDefinedScore = UserDefinedScore;
        }

        //Kendrick
        private bool UseKendrick = true;
        public bool GetUseKendrick() { return UseKendrick; }
        public void SetUseKendrick(bool UseKendrick) { this.UseKendrick = UseKendrick; }

        //C13
        private bool UseC13 = true;
        public bool GetUseC13() { return UseC13; }
        public void SetUseC13(bool UseC13) { this.UseC13 = UseC13; }
        private double C13Tolerance = 0;
        public double GetC13Tolerance() { return C13Tolerance; }
        public void SetC13Tolerance(double C13Tolerance) { this.C13Tolerance = C13Tolerance; }
        public double GetRealC13Tolerance(double PeakMass)
        {
            if (StaticDynamicPpmError)
            {
                return StdDevErrorGain * InputData.GetErrorStdDev(PeakMass);
            }
            return C13Tolerance;
        }

        //Golden rule filters
        public string[] GoldenRuleFilterNames = {
                "Elemental counts",
                "Valence rules",
                "Elemental ratios",
                "Heteroatom counts",
                "Positive atoms",
                "Integer DBE"
        };
        public string[] GetGoldenRuleFilterNames() { return GoldenRuleFilterNames; }
        private readonly bool[] GoldenRuleFilters = { true, true, true, true, true, true };
        public bool[] GetGoldenRuleFilterUsage() { return GoldenRuleFilters; }
        public void SetGoldenRuleFilterUsage(bool[] GoldenRuleFilters)
        {
            var Total = this.GoldenRuleFilters.Length;
            if (Total > GoldenRuleFilters.Length) { Total = GoldenRuleFilters.Length; }
            for (var Filter = 0; Filter < Total; Filter++)
            {
                this.GoldenRuleFilters[Filter] = GoldenRuleFilters[Filter];
            }
        }
        //Special filters
        private System.Data.DataTable DataTableSpecialFilter;
        public enum ESpecialFilters { None, Air, Water, Earth };

        private ESpecialFilters oESpecialFilter = ESpecialFilters.None;

        private readonly string[] SpecialFilterRules = {
                "",
                "O>0 AND N<=2 AND S<=1 AND P=0 AND 3*(S+N)<=O",
                "O>0 AND N<=3 AND S<=2 AND P<=2",
                "O>0 AND N<=3 AND P<=2 AND 3*P<=O"};//last is letter O (o) and not digit 0 (zero)
        public string[] GetSpecialFilterRules() { return SpecialFilterRules; }
        public ESpecialFilters GetSpecialFilter() { return oESpecialFilter; }
        public void SetSpecialFilter(ESpecialFilters oESpecialFilter)
        {
            this.oESpecialFilter = oESpecialFilter;
            DataTableSpecialFilter = new System.Data.DataTable();
            DataTableSpecialFilter.Columns.Add("Mass", typeof(double));
            foreach (var Name in Enum.GetNames(typeof(EElemIndex)))
            {
                DataTableSpecialFilter.Columns.Add(Name, typeof(short));
            }
            if (oESpecialFilter != ESpecialFilters.None)
            {
                DataTableSpecialFilter.Columns.Add("SpecialFilter", typeof(bool), SpecialFilterRules[(int)oESpecialFilter]);
            }
            DataTableSpecialFilter.Rows.Add(DataTableSpecialFilter.NewRow());
        }
        //User-defined filters
        private System.Data.DataTable UserDefinedFilterTable;
        private string UserDefinedFilter = "";
        public string GetUserDefinedFilter() { return UserDefinedFilter; }
        public void SetUserDefinedFilter(string UserDefinedFilter)
        {
            if (UserDefinedFilter.Length == 0)
            {
                UserDefinedFilterTable = null;
                return;
            }
            UserDefinedFilterTable = new System.Data.DataTable();
            UserDefinedFilterTable.Columns.Add("Mass", typeof(double));
            foreach (var Name in Enum.GetNames(typeof(EElemIndex)))
            {
                UserDefinedFilterTable.Columns.Add(Name, typeof(short));
            }
            UserDefinedFilterTable.Columns.Add("UserDefinedFilter", typeof(bool), UserDefinedFilter);
            UserDefinedFilterTable.Rows.Add(UserDefinedFilterTable.NewRow());
            this.UserDefinedFilter = UserDefinedFilter;
        }

        private bool UseFormulaFilters = true;
        public bool GetUseFormulaFilter() { return UseFormulaFilters; }
        public void SetUseFormulaFilter(bool UseFormulaFilters) { this.UseFormulaFilters = UseFormulaFilters; }

        //Output error type
        public enum EErrorType { CIA, Signed };

        private EErrorType oEErrorType = EErrorType.CIA;
        public EErrorType GetErrorType() { return oEErrorType; }
        public void SetErrorType(EErrorType oEErrorType) { this.oEErrorType = oEErrorType; }

        //Reports
        private bool GenerateReports = false;
        public bool GetGenerateReports() { return GenerateReports; }
        public void SetGenerateReports(bool GenerateReports) { this.GenerateReports = GenerateReports; }

        private int MinPeaksPerChain = 5;
        public int GetMinPeaksPerChain() { return MinPeaksPerChain; }
        public void SetMinPeaksPerChain(int MinPeaksPerChain) { this.MinPeaksPerChain = MinPeaksPerChain; }

        //bool LogReportStatus = false;
        //public bool GetLogReportStatus() { return LogReportStatus; }
        //public void SetLogReportStatus( bool LogReportStatus ) { this.LogReportStatus = LogReportStatus; }
        //public StreamWriter oStreamLogWriter;

        /*
        bool AddChain = false;
        public bool GetGenerateChainReport() { return AddChain; }
        public void SetAddChains( bool AddChain ) { this.AddChain = AddChain; }
        public bool GetAddChains(){ return AddChain;}
        */

        //Output file delimiter
        private readonly string OutputFileDelimiter = ",";
        /*public enum EDelimiters { Comma, Tab, Space};
        EDelimiters oEOutputFileDelimiter = EDelimiters.Comma;
        public static string OutputFileDelimiterToString( EDelimiters oo){
            switch ( oo ) {
                case EDelimiters.Comma: return ",";
                case EDelimiters.Tab: return "\t";
                case EDelimiters.Space: return " ";
                default: throw new Exception( "Delimeter error. [" + oo.ToString() + "]");
            }
        }
        public static EDelimiters OutputFileDelimiterToEnum( string oo ) {
            switch ( oo ) {
                case ",": return EDelimiters.Comma;
                case "\t": return EDelimiters.Tab;
                case " ": return EDelimiters.Space;
                default: throw new Exception( "Delimeter error. [" + oo + "]");
            }
        }
        public EDelimiters GetOutputFileDelimiterType() { return oEOutputFileDelimiter; }
        public string GetOutputFileDelimiter() { return OutputFileDelimiter; }
        public void SetOutputFileDelimiterType( EDelimiters Delimiter ) {
            this.oEOutputFileDelimiter = Delimiter;
            OutputFileDelimiter = OutputFileDelimiterToString( Delimiter );
        }
        */
        private readonly Data oData = new Data();//log file
        public AlignData oAlignData = new AlignData();
        public TotalIPDBSearch Ipa;
        public CCia()
        {
            Ipa = new TotalIPDBSearch();
            foreach (var Element in NullFormula) { NullFormula[Element] = 0; }
            //SetRelationFormulaBuildingBlocks( RelationBuildingBlockFormulas );
        }
        public enum ProcessType { Cia, Ipa, CiaIpa };

        private ProcessType oProcessType = ProcessType.Cia;
        public ProcessType GetProcessType() { return oProcessType; }
        public void SetProcessType(ProcessType oProcessType) { this.oProcessType = oProcessType; }
        private string OutputSubfolder = "";
        public void Process(string[] Filenames)
        {
            //create subfolder
            var SubfolderName = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            OutputSubfolder = Path.Combine(Path.GetDirectoryName(Filenames[0]), SubfolderName);
            Directory.CreateDirectory(OutputSubfolder);

            var LogFilename = Path.Combine(OutputSubfolder, "log.csv");
            var oStreamLogWriter = new StreamWriter(LogFilename);
            oStreamLogWriter.AutoFlush = true;

            var FileCount = Filenames.Length;
            var Masses = new double[FileCount][];
            var Abundances = new double[FileCount][];
            var SNs = new double[FileCount][];
            var Resolutions = new double[FileCount][];
            var RelAbundances = new double[FileCount][];
            InputData = new InputData();
            var ChainBlocks = new CChainBlocks();
            var StandardBlockNames = new[] { "H2", "C", "CH2", "O" };
            var StandardBlockMasses = new[] { 2 * CElements.H, CElements.C, 2 * CElements.H + CElements.C, CElements.O };

            var MaxAbundances = new double[FileCount];
            var CalMasses = new double[FileCount][];

            if (oTotalCalibration.ttl_cal_regression != TotalCalibration.ttlRegressionType.none)
            {
                oTotalCalibration.Load(RefPeakFilename);
            }

            if (GenerateReports)
            {
                string inputFileDirectoryPath;

                if (Filenames.Length == 0)
                {
                    inputFileDirectoryPath = ".";
                }
                else
                {
                    var firstInputFile = new FileInfo(Filenames[0]);
                    inputFileDirectoryPath = firstInputFile.Directory.FullName;
                }

                var isotopeFile = FindFile("Isotope.inf", inputFileDirectoryPath);

                CIsotope.ConvertIsotopeFileIntoIsotopeDistanceFile(isotopeFile.FullName);

                var dmTransformationsFile = FindFile("dmTransformations_MalakReal.csv", inputFileDirectoryPath);
                ChainBlocks.KnownMassBlocksFromFile(dmTransformationsFile.FullName);
            }

            for (var FileIndex = 0; FileIndex < FileCount; FileIndex++)
            {
                //read files
                Console.WriteLine("Opening " + Filenames[FileIndex]);
                Support.CFileReader.ReadFile(Filenames[FileIndex], out Masses[FileIndex], out Abundances[FileIndex], out SNs[FileIndex], out Resolutions[FileIndex], out RelAbundances[FileIndex]);
                if (Masses[FileIndex].Length == 0)
                {
                    FormularityProgram.AppendToLog(oStreamLogWriter, "Warning: no data points found in " + Path.GetFileName(Filenames[FileIndex]) + "\n");
                    continue;
                }
                MaxAbundances[FileIndex] = Support.CArrayMath.Max(Abundances[FileIndex]);
                //oStreamLogWriter.WriteLine( "\r\n\t\t\t\t\tDataset " + Path.GetFileName( Filenames [ FileIndex ] ) );
                FormularityProgram.AppendToLog(oStreamLogWriter, "\n");
                FormularityProgram.AppendToLog(oStreamLogWriter, "Dataset: " + Path.GetFileName(Filenames[FileIndex]) + "\n");
                //Pre-Aligment
                //oStreamLogWriter.Write( "Pre-alignment:" );
                FormularityProgram.AppendToLog(oStreamLogWriter, "Pre-alignment:\n");
                if (PreAlignment
                        || StaticDynamicPpmError
                        || (oRelationErrorType == RelationErrorType.DynamicPPM))
                {
                    //spectra alignment
                    InputData.Masses = Masses[FileIndex];
                    InputData.Abundances = Abundances[FileIndex];
                    InputData.Init();

                    double StartPpmError = 5;
                    InputData.MaxPpmErrorGain = 1;
                    var MinPeaksPerChain = 5;

                    if (GenerateReports)
                    {
                        //double [] StandardBlockMasses = new double [] { 2* CElements.H, CElements.C, 2* CElements.H + CElements.C, CElements.O };
                        ChainBlocks.CalculateErrorDistribution(InputData, StartPpmError, StandardBlockMasses);
                        InputData.MaxPpmErrorGain = 3;

                        var DistancePeaks = ChainBlocks.GetPairChainIsotopeStatistics(InputData);
                        File.WriteAllText(Path.Combine(OutputSubfolder, Path.GetFileNameWithoutExtension(Filenames[FileIndex]) + "ChainIsotopes.csv"), ChainBlocks.PairChainIsotopeStatisticsToString(DistancePeaks));
                    }

                    ChainBlocks.CalculateErrorDistribution(InputData, StartPpmError, StandardBlockMasses);
                    ChainBlocks.FindChains1(InputData, MinPeaksPerChain, InputData.Masses.Last() + 1, StandardBlockMasses);
                    ChainBlocks.FindClusters(InputData);

                    if (InputData.Clusters.Length > 0)
                    {
                        ChainBlocks.AssignIdealMassesTheBiggestCluster(InputData);
                    }
                    else
                    {
                        FormularityProgram.AppendToLog(oStreamLogWriter, "Warning: No matches were found\n");
                    }

                    //oStreamLogWriter.Write( "\r\nStart ppm error: " + ( FormulaPPMTolerance * 5 ).ToString( "F8" ) );
                    FormularityProgram.AppendToLog(oStreamLogWriter, "Start ppm error: " + (FormulaPPMTolerance * 5).ToString("F8") + "\n");
                    var TempText = string.Empty;
                    foreach (var BlockName in StandardBlockNames)
                    {
                        TempText = TempText + BlockName + ",";
                    }
                    //oStreamLogWriter.Write( "\r\nError StdDev distribution along mass:\r\n" );
                    FormularityProgram.AppendToLog(oStreamLogWriter, "Error StdDev distribution along mass:\n");
                    //oStreamLogWriter.Write( InputData.ErrorDistributionToString() );
                    FormularityProgram.AppendToLog(oStreamLogWriter, InputData.ErrorDistributionToString());
                    if (GenerateReports)
                    {
                        File.WriteAllText(Path.Combine(OutputSubfolder, Path.GetFileNameWithoutExtension(Filenames[FileIndex]) + "ErrorDistribution.csv"), InputData.ErrorDistributionToString(true));
                        File.WriteAllText(Path.Combine(OutputSubfolder, Path.GetFileNameWithoutExtension(Filenames[FileIndex]) + "Chains.csv"), InputData.ChainsToString(true));
                        File.WriteAllText(Path.Combine(OutputSubfolder, Path.GetFileNameWithoutExtension(Filenames[FileIndex]) + "Clusters.csv"), InputData.ClustersToString(true));
                        File.WriteAllText(Path.Combine(OutputSubfolder, Path.GetFileNameWithoutExtension(Filenames[FileIndex]) + "IdealMassesAfterCluster0.csv"), InputData.IdealMassesToString());
                    }
                    ChainBlocks.AssignIdealMassesToRestPeaks(InputData);
                    if (GenerateReports)
                    {
                        File.WriteAllText(Path.Combine(OutputSubfolder, Path.GetFileNameWithoutExtension(Filenames[FileIndex]) + "IdealMasses.csv"), InputData.IdealMassesToString());
                    }
                    Masses[FileIndex] = InputData.IdealMasses;
                }
                else
                {
                    //oStreamLogWriter.Write( "no" );
                    FormularityProgram.AppendToLog(oStreamLogWriter, "no\n");
                }
                //Calibration
                //oStreamLogWriter.Write( "\r\n\r\nCalibration:" );
                FormularityProgram.AppendToLog(oStreamLogWriter, "\nCalibration: ");
                if (oTotalCalibration.ttl_cal_regression == TotalCalibration.ttlRegressionType.none)
                {
                    //oStreamLogWriter.Write( "no" );
                    FormularityProgram.AppendToLog(oStreamLogWriter, "no\n");
                    CalMasses[FileIndex] = new double[Masses[FileIndex].Length];
                    //for ( int PeakIndex = 0; PeakIndex < CalMasses.Length; PeakIndex++ ) {
                    //    CalMasses [ PeakIndex ] = Masses [ PeakIndex ];
                    //}
                    Array.Copy(Masses[FileIndex], CalMasses[FileIndex], Masses[FileIndex].Length);
                }
                else
                {
                    oTotalCalibration.cal_log.Clear();
                    var MaxAbundance = Support.CArrayMath.Max(Abundances[FileIndex]);
                    CalMasses[FileIndex] = oTotalCalibration.ttl_LQ_InternalCalibration(ref Masses[FileIndex], ref Abundances[FileIndex], ref SNs[FileIndex], MaxAbundances[FileIndex]);
                    //oStreamLogWriter.Write( oTotalCalibration.cal_log );
                    FormularityProgram.AppendToLog(oStreamLogWriter, oTotalCalibration.cal_log + "\n");
                    if (CalMasses[FileIndex] == null && Masses[FileIndex].Length > 0)
                    {

                        FormularityProgram.AppendToLog(oStreamLogWriter, "Calibration failed; using uncalibrated masses\n");
                        FormularityProgram.AppendToLog(oStreamLogWriter, "");

                        // Populate the calibrated mass array with the original masses
                        CalMasses[FileIndex] = new double[Masses[FileIndex].Length];
                        Array.Copy(Masses[FileIndex], CalMasses[FileIndex], Masses[FileIndex].Length);
                    }
                }
            }
            if ((oProcessType == ProcessType.Cia) || (oProcessType == ProcessType.CiaIpa))
            {
                Process(Filenames, Masses, Abundances, SNs, Resolutions, RelAbundances, CalMasses, oStreamLogWriter);
            }
            if ((oProcessType == ProcessType.Ipa) || (oProcessType == ProcessType.CiaIpa))
            {
                for (var FileIndex = 0; FileIndex < FileCount; FileIndex++)
                {
                    Ipa.IPDB_log.Clear();
                    Ipa.ttlSearch(ref CalMasses[FileIndex], ref Abundances[FileIndex], ref SNs[FileIndex], ref MaxAbundances[FileIndex], Filenames[FileIndex]);
                    //oStreamLogWriter.Write( Ipa.IPDB_log );
                    FormularityProgram.AppendToLog(oStreamLogWriter, Ipa.IPDB_log + "\n");
                }
            }

            //oStreamLogWriter.WriteLine( "Processed files:" );
            FormularityProgram.AppendToLog(oStreamLogWriter, "Processed files:\n");
            foreach (var Filename in Filenames)
            {
                //oStreamLogWriter.WriteLine( Path.GetFileName( Filename ) );
                FormularityProgram.AppendToLog(oStreamLogWriter, Path.GetFileName(Filename) + "\n");
            }
            //oStreamLogWriter.WriteLine( "Parameters:" );
            FormularityProgram.AppendToLog(oStreamLogWriter, "Parameters:\n");
            //oStreamLogWriter.Write( GetSaveParameterText() );
            FormularityProgram.AppendToLog(oStreamLogWriter, GetSaveParameterText() + "\n");
            oStreamLogWriter.Flush();
            oStreamLogWriter.Close();
            var LogFileInfo = new FileInfo(LogFilename);
            if (LogFileInfo.Length == 0)
            {
                LogFileInfo.Delete();
            }
        }

        public void Process(string[] Filenames, double[][] Masses, double[][] Abundances, double[][] SNs, double[][] Resolutions, double[][] RelAbundances, double[][] CalMasses, StreamWriter oStreamLogWriter)
        {
            try
            {
                //this.Filenames = Filenames;
                oData.Filenames = Filenames;
                oData.FileCount = oData.Filenames.Length;
                oData.Masses = Masses;
                oData.Abundances = Abundances;
                oData.SNs = SNs;
                oData.Resolutions = Resolutions;
                oData.RelAbundances = RelAbundances;
                oData.CalMasses = CalMasses;
                oData.AlignMasses = new double[oData.FileCount][];
                oData.NeutralMasses = new double[oData.FileCount][];
                oData.Formulas = new short[oData.FileCount][][];
                oData.PPMErrors = new double[oData.FileCount][];
                oData.Candidates = new short[oData.FileCount][];
                for (var FileIndex = 0; FileIndex < oData.FileCount; FileIndex++)
                {
                    oData.AlignMasses[FileIndex] = new double[oData.Masses[FileIndex].Length];
                    oData.NeutralMasses[FileIndex] = new double[oData.Masses[FileIndex].Length];
                    oData.Formulas[FileIndex] = new short[oData.Masses[FileIndex].Length][];
                    oData.PPMErrors[FileIndex] = new double[oData.Masses[FileIndex].Length];
                    oData.Candidates[FileIndex] = new short[oData.Masses[FileIndex].Length];
                }

                //Alignment + Formula finding
                if (Alignment)
                {
                    Console.WriteLine("Aligning");
                    AlignmentByPeak();
                    for (var PeakIndex = 0; PeakIndex < oAlignData.AlignMasses.Length; PeakIndex++)
                    {
                        oAlignData.NeutralMasses[PeakIndex] = Ipa.GetNeutralMass(oAlignData.AlignMasses[PeakIndex]);
                    }
                    if (oAlignData.NeutralMasses.Length == 0)
                    {
                        FormularityProgram.ReportError("Nothing to align; aborting");
                        return;
                    }
                    Console.WriteLine("Formula Finding");
                    //FindFormulas( oAlignData.NeutralMasses, oAlignData.Formulas, oAlignData.PPMErrors, oAlignData.Candidates, FormulaPPMTolerance, RelationErrorAMU, MassLimit );
                    FindFormulas(oAlignData.NeutralMasses, oAlignData.Formulas, oAlignData.PPMErrors, oAlignData.Candidates, RelationErrorAMU, MassLimit);
                    ProcessC13(oAlignData.NeutralMasses, oAlignData.Formulas, oAlignData.PPMErrors);
                }
                else
                {
                    for (var FileIndex = 0; FileIndex < oData.FileCount; FileIndex++)
                    {
                        for (var PeakIndex = 0; PeakIndex < oData.CalMasses[FileIndex].Length; PeakIndex++)
                        {
                            oData.AlignMasses[FileIndex][PeakIndex] = oData.CalMasses[FileIndex][PeakIndex];
                            oData.NeutralMasses[FileIndex][PeakIndex] = Ipa.GetNeutralMass(oData.AlignMasses[FileIndex][PeakIndex]);
                            oData.Formulas[FileIndex][PeakIndex] = (short[])NullFormula.Clone();
                        }
                        Console.WriteLine("Formula Finding");
                        //FindFormulas( oData.NeutralMasses [ FileIndex ], oData.Formulas [ FileIndex ], oData.PPMErrors [ FileIndex ], oData.Candidates [ FileIndex ], FormulaPPMTolerance, RelationErrorAMU, MassLimit );
                        FindFormulas(oData.NeutralMasses[FileIndex], oData.Formulas[FileIndex], oData.PPMErrors[FileIndex], oData.Candidates[FileIndex], RelationErrorAMU, MassLimit);
                        ProcessC13(oData.NeutralMasses[FileIndex], oData.Formulas[FileIndex], oData.PPMErrors[FileIndex]);
                    }
                }

                //Report
                //preparation
                if (Alignment)
                {
                    //preparation: peak alignment into file
                    for (var Peak = 0; Peak < oAlignData.NeutralMasses.Length; Peak++)
                    {
                        var PeakIndexes = oAlignData.Indexes[Peak];
                        for (var FileIndex = 0; FileIndex < oData.FileCount; FileIndex++)
                        {
                            if (PeakIndexes[FileIndex] < 0) { continue; }
                            oData.AlignMasses[FileIndex][PeakIndexes[FileIndex]] = oAlignData.AlignMasses[Peak];
                            oData.NeutralMasses[FileIndex][PeakIndexes[FileIndex]] = oAlignData.NeutralMasses[Peak];
                            oData.Formulas[FileIndex][PeakIndexes[FileIndex]] = (short[])oAlignData.Formulas[Peak].Clone();
                            oData.PPMErrors[FileIndex][PeakIndexes[FileIndex]] = oAlignData.PPMErrors[Peak];
                            oData.Candidates[FileIndex][PeakIndexes[FileIndex]] = oAlignData.Candidates[Peak];
                        }
                    }
                }

                if ((!Alignment) || GenerateReports)
                {
                    for (var FileIndex = 0; FileIndex < Filenames.Length; FileIndex++)
                    {
                        //StreamWriter oStreamWriter;
                        //int FileExtensionLength = Path.GetExtension( Filenames [ FileIndex ] ).Length;
                        //if( oEOutputFileDelimiter == EDelimiters.Comma ) {
                        //    oStreamWriter = new StreamWriter( Filenames [ FileIndex ].Substring( 0, Filenames [ FileIndex ].Length - FileExtensionLength ) + "CShOut.csv" );
                        //} else {
                        //    oStreamWriter = new StreamWriter( Filenames [ FileIndex ].Substring( 0, Filenames [ FileIndex ].Length - FileExtensionLength ) + "CShOut.txt" );
                        //}

                        var reportFilePath = Path.Combine(OutputSubfolder, Path.GetFileNameWithoutExtension(Filenames[FileIndex]) + "Result.csv");
                        Console.WriteLine("Writing results to " + reportFilePath);

                        var oStreamWriter = new StreamWriter(reportFilePath);
                        var Masses1 = oData.Masses[FileIndex];
                        var Abundances1 = oData.Abundances[FileIndex];
                        var SNs1 = oData.SNs[FileIndex];
                        var Resolutions1 = oData.Resolutions[FileIndex];
                        var RelAbundances1 = oData.RelAbundances[FileIndex];

                        var CalMasses1 = oData.CalMasses[FileIndex];
                        var AlignMasses = oData.AlignMasses[FileIndex];
                        var NeutralMasses = oData.NeutralMasses[FileIndex];
                        var Formulas = oData.Formulas[FileIndex];
                        var PPMErrors = oData.PPMErrors[FileIndex];
                        var Candidates = oData.Candidates[FileIndex];

                        var HeaderLine = "Mass";
                        for (var Element = 0; Element < ElementCount; Element++)
                        {
                            HeaderLine = HeaderLine + OutputFileDelimiter + Enum.GetName(typeof(EElemIndex), Element);
                        }
                        HeaderLine = HeaderLine + OutputFileDelimiter + "El_comp" + OutputFileDelimiter + "Class";
                        HeaderLine = HeaderLine + OutputFileDelimiter + "NeutralMass" + OutputFileDelimiter + "Error_ppm" + OutputFileDelimiter + "Candidates";
                        HeaderLine = HeaderLine + OutputFileDelimiter + "CalMass" + OutputFileDelimiter + "AlignMasses" + OutputFileDelimiter + "Abundance";
                        if (SNs1[0] > 0)
                        {
                            HeaderLine = HeaderLine + OutputFileDelimiter + "sn" + OutputFileDelimiter + "resolution" + OutputFileDelimiter + "rel_abu" + OutputFileDelimiter + "peak_index";
                        }
                        oStreamWriter.WriteLine(HeaderLine);

                        for (var Peak = 0; Peak < Masses1.Length; Peak++)
                        {
                            var Line = Masses1[Peak].ToString();
                            for (var Element = 0; Element < ElementCount; Element++)
                            {
                                Line = Line + OutputFileDelimiter + Formulas[Peak][Element].ToString();
                            }
                            Line = Line + OutputFileDelimiter + GetFormulaElementComposition(Formulas[Peak]);
                            Line = Line + OutputFileDelimiter + GetFormulaClass(Formulas[Peak]);
                            Line = Line + OutputFileDelimiter + NeutralMasses[Peak].ToString();
                            if (oEErrorType == EErrorType.CIA)
                            {
                                Line = Line + OutputFileDelimiter + PPMErrors[Peak].ToString();
                            }
                            else if (oEErrorType == EErrorType.Signed)
                            {
                                if (!IsFormula(Formulas[Peak]))
                                {
                                    Line = Line + OutputFileDelimiter + "0";
                                }
                                else
                                {
                                    Line = Line + OutputFileDelimiter + CPpmError.SignedPpmPPM(NeutralMasses[Peak], FormulaToNeutralMass(Formulas[Peak])).ToString();
                                }
                            }
                            Line = Line + OutputFileDelimiter + Candidates[Peak].ToString();
                            Line = Line + OutputFileDelimiter + CalMasses1[Peak].ToString();
                            Line = Line + OutputFileDelimiter + AlignMasses[Peak].ToString();
                            Line = Line + OutputFileDelimiter + Abundances1[Peak].ToString();
                            if (SNs1[0] > 0)
                            {
                                Line = Line + OutputFileDelimiter + SNs1[Peak].ToString();
                                Line = Line + OutputFileDelimiter + Resolutions1[Peak].ToString();
                                Line = Line + OutputFileDelimiter + RelAbundances1[Peak].ToString();
                            }
                            Line = Line + OutputFileDelimiter + Peak.ToString();
                            oStreamWriter.WriteLine(Line);
                        }
                        oStreamWriter.Close();
                    }
                }
                //AlignmentReport
                if (Alignment)
                {
                    //StreamWriter oStreamWriter;
                    //if( oEOutputFileDelimiter == EDelimiters.Comma ) {
                    //    oStreamWriter = new StreamWriter( Path.GetDirectoryName( Filenames [ 0 ] ) + "\\Report.csv" );
                    //} else {
                    //    oStreamWriter = new StreamWriter( Path.GetDirectoryName( Filenames [ 0 ] ) + "\\Report.txt" );
                    //}
                    var OutputFilePath = Path.Combine(OutputSubfolder, "Out.csv");
                    Console.WriteLine("Writing results to " + OutputFilePath);
                    var oStreamWriter = new StreamWriter(OutputFilePath);
                    var HeaderLine = "Mass";
                    foreach (var Element in Enum.GetNames(typeof(CCia.EElemIndex)))
                    {
                        HeaderLine = HeaderLine + OutputFileDelimiter + Element;
                    }
                    HeaderLine = HeaderLine + OutputFileDelimiter + "El_comp" + OutputFileDelimiter + "Class";
                    HeaderLine = HeaderLine + OutputFileDelimiter + "NeutralMass" + OutputFileDelimiter + "Error_ppm" + OutputFileDelimiter + "Candidates";
                    foreach (var File in Filenames)
                    {
                        HeaderLine = HeaderLine + OutputFileDelimiter + Path.GetFileNameWithoutExtension(File);
                    }
                    oStreamWriter.WriteLine(HeaderLine);

                    for (var Peak = 0; Peak < oAlignData.NeutralMasses.Length; Peak++)
                    {
                        var Line = Ipa.GetChargedMass(oAlignData.NeutralMasses[Peak]).ToString();
                        var Formula = oAlignData.Formulas[Peak];
                        for (var Element = 0; Element < ElementCount; Element++)
                        {
                            Line = Line + OutputFileDelimiter + Formula[Element].ToString();
                        }
                        Line = Line + OutputFileDelimiter + GetFormulaElementComposition(Formula);
                        /*
                        //class	        O:C(low)	O:C(high)	H:C(low)	H:C(high)
                        //lipid 	    >=0	        <0.3	    >=1.5	    <2.5
                        //unsatHC	    >=0	        <0.125	    >=0.8	    <1.5
                        //condHC	    >=0	        <0.95	    >=0.2	    <0.8
                        //protein	    >=0.3	    <0.55	    >=1.5	    <2.3
                        //aminosugar	>=0.55   	<0.7	    >=1.5	    <2.2
                        //carb	        >=0.7	    <1.05	    >=1.5	    <2.2
                        //lignin	    >=0.125    	<0.65	    >=0.8	    <1.5
                        //tannin	    >=0.65	    <1.1	    >=0.8	    <1.5
                        double TotalC = Formula [ ( int ) EElemIndex.C ] + Formula [ ( int ) EElemIndex.C13 ];//must be double!
                        if( TotalC == 0 ) {
                            Line = Line + OutputFileDelimiter + "None";
                        } else {
                            double HToC = Formula [ ( int ) EElemIndex.H ] / TotalC;
                            double OToC = Formula [ ( int ) EElemIndex.O ] / TotalC;
                            if( ( ( OToC >= 0 ) && ( OToC < 0.3 ) && ( HToC >= 1.5 ) && ( HToC < 2.5 ) ) == true ) {//Lipid
                                Line = Line + OutputFileDelimiter + "Lipid";
                            } else if( ( ( OToC >= 0 ) && ( OToC < 0.125 ) && ( HToC >= 0.8 ) && ( HToC < 1.5 ) ) == true ) {//UnsatHC
                                Line = Line + OutputFileDelimiter + "UnsatHC";
                            } else if( ( ( OToC >= 0 ) && ( OToC < 0.95 ) && ( HToC >= 0.2 ) && ( HToC < 0.8 ) ) == true ) {//CondHC
                                Line = Line + OutputFileDelimiter + "ConHC";
                            } else if( ( ( OToC >= 0.3 ) && ( OToC < 0.55 ) && ( HToC >= 1.5 ) && ( HToC < 2.3 ) ) == true ) {//Protein
                                Line = Line + OutputFileDelimiter + "Protein";
                            } else if( ( ( OToC >= 0.55 ) && ( OToC < 0.7 ) && ( HToC >= 1.5 ) && ( HToC < 2.2 ) ) == true ) {//AminoSugar
                                Line = Line + OutputFileDelimiter + "AminoSugar";
                            } else if( ( ( OToC >= 0.7 ) && ( OToC < 1.05 ) && ( HToC >= 1.5 ) && ( HToC < 2.2 ) ) == true ) {//Carb
                                Line = Line + OutputFileDelimiter + "Carb";
                            } else if( ( ( OToC >= 0.125 ) && ( OToC < 0.65 ) && ( HToC >= 0.8 ) && ( HToC < 1.5 ) ) == true ) {//Lignin
                                Line = Line + OutputFileDelimiter + "Lignin";
                            } else if( ( ( OToC >= 0.65 ) && ( OToC < 1.1 ) && ( HToC >= 0.8 ) && ( HToC < 1.5 ) ) == true ) {//Tannin
                                Line = Line + OutputFileDelimiter + "Tannin";
                            } else {
                                Line = Line + OutputFileDelimiter + "Other";
                            }
                        }
                        */
                        Line = Line + OutputFileDelimiter + GetFormulaClass(Formula);
                        Line = Line + OutputFileDelimiter + oAlignData.NeutralMasses[Peak].ToString();
                        if (oEErrorType == EErrorType.CIA)
                        {
                            Line = Line + OutputFileDelimiter + oAlignData.PPMErrors[Peak].ToString();
                        }
                        else if (oEErrorType == EErrorType.Signed)
                        {
                            if (!IsFormula(Formula))
                            {
                                Line = Line + OutputFileDelimiter + "0";
                            }
                            else
                            {
                                Line = Line + OutputFileDelimiter + CPpmError.SignedPpmPPM(oAlignData.NeutralMasses[Peak], FormulaToNeutralMass(Formula)).ToString();
                            }
                        }
                        Line = Line + OutputFileDelimiter + oAlignData.Candidates[Peak].ToString();
                        var Indexes = oAlignData.Indexes[Peak];
                        for (var FileIndex = 0; FileIndex < Indexes.Length - 1; FileIndex++)
                        {
                            if (Indexes[FileIndex] < 0)
                            {//no peak in file
                                Line = Line + OutputFileDelimiter + "0";
                            }
                            else
                            {
                                Line = Line + OutputFileDelimiter + oData.Abundances[FileIndex][Indexes[FileIndex]];
                            }
                        }
                        oStreamWriter.WriteLine(Line);
                    }
                    oStreamWriter.Close();
                }
            }
            catch (Exception ex)
            {
                if (oStreamLogWriter != null)
                {
                    //oStreamLogWriter.WriteLine( "Exception: " );
                    //oStreamLogWriter.Write( ex.Message );
                    FormularityProgram.AppendToLog(oStreamLogWriter, "Exception: " + ex.Message);
                }
                else
                {
                    FormularityProgram.ReportError("Exception processing: " + ex.Message);
                }
            }
        }

        private string GetFormulaElementComposition(short[] Formula)
        {
            var Answer = string.Empty;
            if (Formula[(int)EElemIndex.C] > 0)
            {
                foreach (EElemIndex Element in Enum.GetValues(typeof(EElemIndex)))
                {
                    if (Element == EElemIndex.C)
                    {
                        if (Formula[(int)EElemIndex.C] + Formula[(int)EElemIndex.C13] > 0)
                        {
                            Answer = Answer + EElemIndex.C.ToString();
                        }
                    }
                    else if (Element == EElemIndex.C13)
                    {
                        continue;
                    }
                    else
                    {
                        if (Formula[(int)Element] > 0)
                        {
                            Answer = Answer + Element.ToString();
                        }
                    }
                }
            }
            return Answer;
        }

        private string GetFormulaClass(short[] Formula)
        {
            //class	        O:C(low)	O:C(high)	H:C(low)	H:C(high)
            //lipid 	    >=0	        <0.3	    >=1.5	    <2.5
            //unsatHC	    >=0	        <0.125	    >=0.8	    <1.5
            //condHC	    >=0	        <0.95	    >=0.2	    <0.8
            //protein	    >=0.3	    <0.55	    >=1.5	    <2.3
            //aminosugar	>=0.55   	<0.7	    >=1.5	    <2.2
            //carb	        >=0.7	    <1.05	    >=1.5	    <2.2
            //lignin	    >=0.125    	<0.65	    >=0.8	    <1.5
            //tannin	    >=0.65	    <1.1	    >=0.8	    <1.5

            double TotalC = Formula[(int)EElemIndex.C] + Formula[(int)EElemIndex.C13];//must be double!
            if (TotalC == 0)
            {
                return "None";
            }
            var HToC = Formula[(int)EElemIndex.H] / TotalC;
            var OToC = Formula[(int)EElemIndex.O] / TotalC;
            if (((OToC >= 0) && (OToC < 0.3) && (HToC >= 1.5) && (HToC < 2.5)))
            {//Lipid
                return "Lipid";
            }
            else if (((OToC >= 0) && (OToC < 0.125) && (HToC >= 0.8) && (HToC < 1.5)))
            {//UnsatHC
                return "UnsatHC";
            }
            else if (((OToC >= 0) && (OToC < 0.95) && (HToC >= 0.2) && (HToC < 0.8)))
            {//CondHC
                return "ConHC";
            }
            else if (((OToC >= 0.3) && (OToC < 0.55) && (HToC >= 1.5) && (HToC < 2.3)))
            {//Protein
                return "Protein";
            }
            else if (((OToC >= 0.55) && (OToC < 0.7) && (HToC >= 1.5) && (HToC < 2.2)))
            {//AminoSugar
                return "AminoSugar";
            }
            else if (((OToC >= 0.7) && (OToC < 1.05) && (HToC >= 1.5) && (HToC < 2.2)))
            {//Carb
                return "Carb";
            }
            else if (((OToC >= 0.125) && (OToC < 0.65) && (HToC >= 0.8) && (HToC < 1.5)))
            {//Lignin
                return "Lignin";
            }
            else if (((OToC >= 0.65) && (OToC < 1.1) && (HToC >= 0.8) && (HToC < 1.5)))
            {//Tannin
                return "Tannin";
            }
            else
            {
                return "Other";
            }
        }
        public void CleanComObject(object o)
        {
            try
            {
                while (System.Runtime.InteropServices.Marshal.ReleaseComObject(o) > 0)
                    ;
            }
            catch { }
            finally
            {
                o = null;
            }
        }

        private class CGrpdiffK
        {
            public bool IsEmpty;
            public List<int>[] Indexes;
        };

        private bool CheckPeakMassByEvenOdd(double[] Masses)
        {
            var Even = 0;
            var Odd = 0;
            foreach (var Mass in Masses)
            {
                if ((Math.Round(Mass) % 2) == 0)
                {
                    Even = Even + 1;
                }
                else
                {
                    Odd = Odd + 1;
                }
            }
            if (Odd > Even)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private FileInfo FindFile(string fileName, string inputFileDirectoryPath)
        {
            if (File.Exists(fileName))
            {
                return new FileInfo(fileName);
            }
            else
            {
                return new FileInfo(Path.Combine(inputFileDirectoryPath, fileName));
            }
        }

        //void FindFormulas( double [] NeutralMasses, short [][] Formulas, double [] PPMErrors, short [] Candidates, double FormulaErrorPPM, double RelationErrorAMU, double MassLimit ) {
        private void FindFormulas(double[] NeutralMasses, short[][] Formulas, double[] PPMErrors, short[] Candidates, double RelationErrorAMU, double MassLimit)
        {
            //DIFFERENCE FROM NON-MODIFIED VERSION IS WHICH FUNCTIONAL GROUPS ARE PROPAGATED (SEE relations) AND DECISION TREE WHEN MULTIPLE FORMULAS ARE
            //FOUND; IDEA IS THAT MODIFIED VERSION SHOULD BE USED WHEN EXPERIMENTING REDUCING THE RISK OF CHANGING ORIGINAL FUNCTION OR CONFUSION WHICH VERSION IS USED
            //AFTER AUTOMATION THIS DILEMA SHOULD BE RESOLVED ON A PARAMETER FILE LEVEL(N.T.)

            //input the following variables:
            //    peak_mass - list of masses
            //    peak_int - intensity of each peak (can currently be set to zeros)
            //    FORMULA_ERROR is the error for the elemental formula determination by the function, chemform, in ppm;
            //    RELATION_ERROR is the window allowed % for identification of a relationship between two peaks, in ppm.
            //    MASS_LIMIT is the maximum mass for the brute force calculation
            //    fullCompoundList - the database of possible compounds calculated by K Longnecker (LongneckerKujawinski_fullCompoundList.mat)
            //    sortType can be one of the following choices:
            //        1) 'lowestSP' select the formula with the lowest # of S and P
            //        2) 'HAcap' sorts based on the lowest number of S,P,N, and then only formulas with P <= 1 or S <= 3 are considered valid
            //        3) 'lowestError' to sort on the formula which has the lowest error from the measured mass
            //    This iteration % of the program uses only chemical relationships that are common to refractory DOM such as humic acids. It does not include many biologically-relevant (i.e., metabolic) reactions.

            //output is
            //    formula : the elemental formulas for the peak list [C H O N C13 S P Na Error].
            //    elementOrder : a reminder of the order of elements by column

            //Original version of this algorithm published as: Kujawinski and Behn. 2006. Automated analysis of electrospray ionization Fourier-transform ion cyclotron resonance mass spectra
            //of natural organic matter. Analytical Chemistry 78:4363-4373.
            //Largest change has been to use a database to find the formulas rather than recalculating all of the possible formulas each time.
            //Elizabeth Kujawinski Behn, May 2005

            //Last updated:
            //November 6, 2005 updated version received by K Longnecker from LK 8/8/08 KL changing relations to structures 9/12/08
            //KL added 9/12/08: put in a check to make sure that the format of the data are as needed for this function. peak_int and peak_mass both need to be multiple rows and one column

            //KL 1/2/09 - change the list sorting to consider both the minimum number of non-oxygen heteroatoms AND the lowest error - get around the case where
            //    can have different formulas with the same number of non-oxygen heteroatoms
            //KL 1/6/09 - convert the results of getMATfile to double precision to get the right answer in the mass calculations! and added a line to Check7GR to
            //    require all elements to be positive (bc can't have negative number of elements in a compound, so why allow it)
            //KL 1/17/09 findformula_uselist_KL7_SPsort - Still finding too much S and P  at the expense of N, try sorting to bias against S and P, but not N this involves changes both
            //    in the useMATfile function embedded and in the loops for building at the higher masses
            //KL 1/20/09 - had a requirement that to keep a new peak, has to be within half of the given formula_error if there already is an old formula IF the
            //    formula is a comparison bw an existing formula found through the relations and a new formula found with the database (previously brute force)
            //KL 4/15/09 - IF there is more than one formula, cap the number of S and P, and then select based on lowest number of N, S, and P (sortType = 'HA_cap')
            //    (will also test an overall cap on the number of S and P). Also, get rid of the relation which switches H for Na (will deal with that later)
            //KL 4/30/09 - cleaning up, and changing the names of the sortTypes KL 5/1/09 - only change is to correct the neutral mass check bc sent out the wrong version yesterday
            //KL 5/11/09 - changing the neutral mass check yet again
            //KL 9/23/09 - change useMATfile to send out startform as double precision
            //KL 9/1/2011 - correcting problem found by Dan Baluha (Univ. of Maryland) where in one of the calculations, the error calculation was multiplied by 1e-6, rather than 1e6,
            //    therefore the error calculations were off by 1e-12.
            //??? log
            CheckPeakMassByEvenOdd(NeutralMasses);
            //Create relations
            var GrpdiffK = new CGrpdiffK[NeutralMasses.Length];
            var MinMass = NeutralMasses[0];
            var MaxMass = NeutralMasses[NeutralMasses.Length - 1];
            for (var Peak = 0; Peak < NeutralMasses.Length; Peak++)
            {
                var CurrentGrpdiffK = new CGrpdiffK();
                CurrentGrpdiffK.IsEmpty = true;
                if (UseRelation)
                {
                    var NeutralMass = NeutralMasses[Peak];
                    var NeutralMassToMin = MinMass - NeutralMass;
                    if (!UseBackward)
                    {
                        NeutralMassToMin = 0;
                    }
                    var NeutralMassToMax = MaxMass - NeutralMass;
                    CurrentGrpdiffK.Indexes = new List<int>[ListActiveRelationFormulaBuildingBlocks.Count];
                    for (var Relation = 0; Relation < ListActiveRelationFormulaBuildingBlocks.Count; Relation++)
                    {
                        //KL note: look for differences, and keep those less than the defined relation_error
                        var CurIndexes = new List<int>();
                        var NeutralMassToMinGrp = (int)Math.Ceiling(NeutralMassToMin / ActiveRelationFormulaBuildingBlockMasses[Relation]);
                        var NeutralMassToMaxGrp = (int)Math.Floor(NeutralMassToMax / ActiveRelationFormulaBuildingBlockMasses[Relation]);
                        for (var CurrentGrp = NeutralMassToMinGrp; CurrentGrp <= NeutralMassToMaxGrp; CurrentGrp++)
                        {
                            double GrpMinNeutralMass;
                            double GrpMaxNeutralMass;
                            switch (oRelationErrorType)
                            {
                                case RelationErrorType.AMU:
                                    GrpMinNeutralMass = NeutralMass + ActiveRelationFormulaBuildingBlockMasses[Relation] * (CurrentGrp - RelationErrorAMU);
                                    GrpMaxNeutralMass = NeutralMass + ActiveRelationFormulaBuildingBlockMasses[Relation] * (CurrentGrp + RelationErrorAMU);
                                    break;
                                case RelationErrorType.GapPPM:
                                    var GapPPMError = CPpmError.PpmToError(ActiveRelationFormulaBuildingBlockMasses[Relation] * CurrentGrp, RelationErrorAMU);
                                    GrpMinNeutralMass = NeutralMass + ActiveRelationFormulaBuildingBlockMasses[Relation] * (CurrentGrp - GapPPMError);
                                    GrpMaxNeutralMass = NeutralMass + ActiveRelationFormulaBuildingBlockMasses[Relation] * (CurrentGrp + GapPPMError);
                                    break;
                                case RelationErrorType.DynamicPPM:
                                    var GapNeutralMass = NeutralMass + ActiveRelationFormulaBuildingBlockMasses[Relation] * CurrentGrp;
                                    var PPMError = StdDevErrorGain * InputData.GetErrorStdDev(GapNeutralMass);
                                    GrpMinNeutralMass = GapNeutralMass - PPMError;
                                    GrpMaxNeutralMass = GapNeutralMass + PPMError;
                                    break;
                                case RelationErrorType.PPM:
                                default:
                                    GapNeutralMass = NeutralMass + ActiveRelationFormulaBuildingBlockMasses[Relation] * CurrentGrp;
                                    PPMError = CPpmError.PpmToError(GapNeutralMass, RelationErrorAMU);
                                    GrpMinNeutralMass = GapNeutralMass - PPMError;
                                    GrpMaxNeutralMass = GapNeutralMass + PPMError;
                                    break;
                            }
                            var GrpPeak = Array.BinarySearch(NeutralMasses, GrpMinNeutralMass);
                            if (GrpPeak < 0) { GrpPeak = ~GrpPeak; }
                            for (; GrpPeak < NeutralMasses.Length; GrpPeak++)
                            {
                                if (GrpPeak == Peak) { continue; }
                                if (NeutralMasses[GrpPeak] > GrpMaxNeutralMass)
                                {
                                    break;
                                }
                                //double diff = ( NeutralMasses [ GrpPeak ] - NeutralMass ) / ActiveRelationFormulaBuildingBlockMasses [ Relation ];
                                //if( Math.Abs( diff - Math.Round( diff ) ) <= RelationErrorAMU ) {
                                CurIndexes.Add(GrpPeak);
                                CurrentGrpdiffK.IsEmpty = false;
                                //} else {
                                //    break;
                                //}
                            }
                        }
                        CurrentGrpdiffK.Indexes[Relation] = CurIndexes;
                    }
                }
                GrpdiffK[Peak] = CurrentGrpdiffK;
            }
            for (var Peak = 0; Peak < NeutralMasses.Length; Peak++)
            {
                if (NeutralMasses[Peak] < MassLimit)
                {
                    var DBFormula = PickDBFormula(NeutralMasses[Peak], out Candidates[Peak]);
                    if (!IsFormula(DBFormula))
                    {
                        continue;
                    }
                    double DBFormulaMass = 0;
                    double DBFormulaMassError = 0;
                    DBFormulaMass = FormulaToNeutralMass(DBFormula);
                    DBFormulaMassError = CPpmError.AbsPpmError(DBFormulaMass, NeutralMasses[Peak]);
                    if (GrpdiffK[Peak].IsEmpty)
                    {
                        //StartFormula = PickDBFormula( NeutralMasses [ Peak ], out Candidates[ Peak ]);
                        //if( IsFormula( DBFormula ) == true ) {
                        //double tempmass = FormulaToNeutralMass( DBFormula );
                        //double Terror = AbsMassErrorPPM( tempmass, NeutralMasses [ Peak ]);
                        Formulas[Peak] = (short[])DBFormula.Clone();
                        PPMErrors[Peak] = DBFormulaMassError/*Terror*/;
                        //}
                    }
                    else
                    {
                        if (!IsFormula(Formulas[Peak]))
                        {
                            //StartFormula = PickDBFormula( NeutralMasses [ Peak ], out Candidates [ Peak ] );
                            //if( IsFormula( DBFormula ) == true ) {
                            //double tempmass = FormulaToNeutralMass( DBFormula );
                            //double Terror = AbsMassErrorPPM( tempmass, NeutralMasses [ Peak ]);
                            Formulas[Peak] = (short[])DBFormula.Clone();
                            PPMErrors[Peak] = DBFormulaMassError/*Terror*/;
                            //}
                        }
                        else
                        {
                            //first get the brute force formula from the table
                            //short [] tempform = PickDBFormula( NeutralMasses [ Peak ], out Candidates [ Peak ] );
                            //double tempmass = FormulaToNeutralMass( tempform );
                            //double tempmass = FormulaToNeutralMass( DBFormula );
                            //double errornew = AbsMassErrorPPM( tempmass, NeutralMasses [ Peak ]);
                            //bool check = CheckFormulaByFilters( Formulas [ Peak ], NeutralMasses [ Peak ]);//this seems redundant...but leave in
                            //attempt to address incorrect propagation without eliminating NH propagation
                            //Nikola Tolic(1/17/14)
                            //improvement on sub-ppm level is not that easy to evaluate; keep brute force formula if heteroatom evaluation makes it better choice
                            //if we have resolved peaks this is where decision by "fine isotopic structure" could come handy
                            //This was original LK decision tree with comments; then decide whether to keep the brute force formula or the formula which is already present (from the relations)
                            //if( check == false ) {
                            //    Formulas [ Peak ] = ( short [] ) DBFormula/*tempform*/.Clone();
                            //    PPMErrors[ Peak ] = DBFormulaMassError/*errornew*/;
                            //} else {
                            if (UseCIAFormulaScore)
                            {
                                //if( tempform [ ( int ) EElemNumber.S ] + tempform [ ( int ) EElemNumber.P ] < Formulas [ Peak ] [ ( int ) EElemNumber.S ] + Formulas [ Peak ] [ ( int ) EElemNumber.P ]
                                if (DBFormula[(int)EElemIndex.S] + DBFormula[(int)EElemIndex.P] < Formulas[Peak][(int)EElemIndex.S] + Formulas[Peak][(int)EElemIndex.P]
                                        //&& Math.Abs( DBFormulaMassError/*errornew*/ ) < FormulaErrorPPM / 2 ) {
                                        && Math.Abs(DBFormulaMassError/*errornew*/ ) < GetRealFormulaPpmError(NeutralMasses[Peak]))
                                {
                                    //KL adding this last line 1/20/09 - only keep brute force if it 'really' improves the formula (much smaller error)
                                    Formulas[Peak] = (short[])DBFormula/*tempform*/.Clone();
                                    PPMErrors[Peak] = DBFormulaMassError/*errornew*/;
                                }
                            }
                            else
                            {
                                if (IsNewFormulaScoreBetter(Formulas[Peak], PPMErrors[Peak], DBFormula, DBFormulaMassError))
                                {
                                    Formulas[Peak] = (short[])DBFormula/*tempform*/.Clone();
                                    PPMErrors[Peak] = DBFormulaMassError/*errornew*/;
                                }
                            }
                            //}
                        }
                        //if( IsFormula( Formulas [ Peak ] ) == false ) { continue; }
                        //DBFormula = ( short [] ) Formulas [ Peak ].Clone();
                        //now go through Grpdiff and assign those formulas...so using the formula decided to be ok - build from that using the relations
                        if (UseRelation)
                        {
                            for (var Relation = 0; Relation < ListActiveRelationFormulaBuildingBlocks.Count; Relation++)
                            {
                                foreach (var relpk in GrpdiffK[Peak].Indexes[Relation])
                                {
                                    var diff1 = NeutralMasses[relpk] - NeutralMasses[Peak];
                                    var numGps = (int)(Math.Round((NeutralMasses[relpk] - NeutralMasses[Peak]) / ActiveRelationFormulaBuildingBlockMasses[Relation]));
                                    if (numGps > 0)
                                    {
                                        var newform = new short[ElementCount];
                                        for (var Element = 0; Element < ElementCount; Element++)
                                        {
                                            newform[Element] = (short)(DBFormula[Element] + numGps * ListActiveRelationFormulaBuildingBlocks[Relation][Element]);
                                        }
                                        var newmass = FormulaToNeutralMass(newform);
                                        var errornew = CPpmError.AbsPpmError(newmass, NeutralMasses[relpk]);
                                        var checknew = CheckFormulaByFilters(newform, NeutralMasses[relpk]);
                                        var checkold = CheckFormulaByFilters(Formulas[relpk], NeutralMasses[relpk]);
                                        if (checknew)
                                        {
                                            if (checkold)
                                            {
                                                if (UseCIAFormulaScore)
                                                {
                                                    if ((newform[(int)EElemIndex.S] + newform[(int)EElemIndex.P] < Formulas[relpk][(int)EElemIndex.S] + Formulas[relpk][(int)EElemIndex.P])
                                                            //&& ( Math.Abs( errornew ) <= FormulaErrorPPM ) ) {
                                                            && (Math.Abs(errornew) <= GetRealFormulaPpmError(NeutralMasses[Peak])))
                                                    {
                                                        Formulas[relpk] = (short[])newform.Clone();
                                                        PPMErrors[relpk] = errornew;
                                                    }
                                                }
                                                else
                                                {
                                                    if (IsNewFormulaScoreBetter(Formulas[relpk], PPMErrors[relpk], newform, errornew))
                                                    {
                                                        Formulas[relpk] = (short[])newform.Clone();
                                                        PPMErrors[relpk] = errornew;
                                                    }
                                                }
                                                //} else if( checkold == false && Math.Abs( errornew ) <= FormulaErrorPPM ) {
                                            }
                                            else if (!checkold && Math.Abs(errornew) <= GetRealFormulaPpmError(NeutralMasses[Peak]))
                                            {
                                                Formulas[relpk] = (short[])newform.Clone();
                                                PPMErrors[relpk] = errornew;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {// > MassLimit
                    Candidates[Peak] = -1;//no search
                    if (!GrpdiffK[Peak].IsEmpty)
                    {
                        if (IsFormula(Formulas[Peak]))
                        {//if the formula is already known
                            var startform = (short[])(Formulas[Peak].Clone());
                            //KL change to only do this for the places where Grpdiff is not empty
                            for (var Relation = 0; Relation < ListActiveRelationFormulaBuildingBlocks.Count; Relation++)
                            {
                                foreach (var relpk in GrpdiffK[Peak].Indexes[Relation])
                                {
                                    //relpk > Peak???
                                    var checkold = CheckFormulaByFilters(Formulas[relpk], NeutralMasses[relpk]);//???
                                    var newform = (short[])startform.Clone();
                                    var RelationGaps = (int)Math.Round((NeutralMasses[relpk] - NeutralMasses[Peak]) / ActiveRelationFormulaBuildingBlockMasses[Relation]);
                                    if (Math.Abs(RelationGaps) > MaxRelationGaps) { continue; }
                                    for (var Element = 0; Element < ElementCount; Element++)
                                    {
                                        newform[Element] = (short)(newform[Element] + RelationGaps * ListActiveRelationFormulaBuildingBlocks[Relation][Element]);
                                    }
                                    var checknew = CheckFormulaByFilters(newform, NeutralMasses[relpk]);
                                    var newmass = FormulaToNeutralMass(newform);
                                    var errornew = CPpmError.AbsPpmError(newmass, NeutralMasses[relpk]);
                                    //if( ( checknew == true ) && ( Math.Abs( errornew ) <= FormulaErrorPPM ) ) {
                                    if (checknew && (Math.Abs(errornew) <= GetRealFormulaPpmError(NeutralMasses[Peak])))
                                    {
                                        if (checkold)
                                        {
                                            if (UseCIAFormulaScore)
                                            {
                                                if (newform[(int)EElemIndex.S] + newform[(int)EElemIndex.P] < Formulas[relpk][(int)EElemIndex.S] + Formulas[relpk][(int)EElemIndex.P])
                                                {
                                                    Formulas[relpk] = (short[])newform.Clone();
                                                    PPMErrors[relpk] = errornew;
                                                }
                                            }
                                            else
                                            {
                                                if (IsNewFormulaScoreBetter(Formulas[relpk], PPMErrors[relpk], newform, errornew))
                                                {
                                                    Formulas[relpk] = (short[])newform.Clone();
                                                    PPMErrors[relpk] = errornew;
                                                }
                                            }
                                        }
                                        else if (!checkold)
                                        {
                                            Formulas[relpk] = (short[])newform.Clone();
                                            PPMErrors[relpk] = errornew;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {//formula is not known - check for formulas in lower masses
                            //KL change to only do this for the places where Grpdiff is not empty
                            if (UseRelation)
                            {
                                for (var Relation = 0; Relation < ListActiveRelationFormulaBuildingBlocks.Count; Relation++)
                                {
                                    foreach (var low_m in GrpdiffK[Peak].Indexes[Relation])
                                    {
                                        if (NeutralMasses[low_m] < NeutralMasses[Peak])
                                        {
                                            //int low_m = GrpdiffK [ Peak ].Indexs [Relation][ t ];
                                            if (IsFormula(Formulas[low_m]))
                                            {
                                                var startform = (short[])Formulas[low_m].Clone();
                                                var RelationGaps = (int)Math.Round(Math.Abs(NeutralMasses[low_m] - NeutralMasses[Peak]) / ActiveRelationFormulaBuildingBlockMasses[Relation]);
                                                if (Math.Abs(RelationGaps) > MaxRelationGaps) { continue; }
                                                var newform = (short[])startform.Clone();
                                                for (var Element = 0; Element < ElementCount; Element++)
                                                {
                                                    newform[Element] = (short)(newform[Element] + RelationGaps * ListActiveRelationFormulaBuildingBlocks[Relation][Element]);
                                                }
                                                var checknew = CheckFormulaByFilters(newform, NeutralMasses[Peak]);
                                                if (!checknew) { continue; }
                                                var newmass = FormulaToNeutralMass(newform);
                                                var errornew = CPpmError.AbsPpmError(newmass, NeutralMasses[Peak]);
                                                //if( Math.Abs( numGps ) <= MaxNumGps && checknew == true && errornew <= FormulaError ) {
                                                var bIsFormula = IsFormula(Formulas[Peak]);
                                                //if( ( ( bIsFormula == false ) && ( Math.Abs( errornew ) <= FormulaErrorPPM ) ) || ( ( bIsFormula == true ) && ( errornew < PPMErrors [ Peak ] ) ) ) {
                                                if (((!bIsFormula) && (Math.Abs(errornew) <= GetRealFormulaPpmError(NeutralMasses[Peak]))) || (bIsFormula && (errornew < PPMErrors[Peak])))
                                                {
                                                    Formulas[Peak] = (short[])newform.Clone();
                                                    PPMErrors[Peak] = errornew;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            //Kendrick mass stuff
            if (UseKendrick)
            {
                //KendrickParameters [] AKendrick_matrix = new KendrickParameters [ NeutralMasses.Length ];
                var KMD = new int[NeutralMasses.Length];
                var ZStar = new int[NeutralMasses.Length];
                for (var Peak = 0; Peak < NeutralMasses.Length; Peak++)
                {
                    //AKendrick_matrix [ Peak ] = new KendrickParameters();
                    //AKendrick_matrix [ Peak ].peak_mass = NeutralMasses [ Peak ];
                    //AKendrick_matrix [ Peak ].KenMass = NeutralMasses [ Peak ] / ( C + 2 * H ) * 14;
                    var KendrickMass = NeutralMasses[Peak] / (CElements.C + 2 * CElements.H) * 14;
                    var NomIUPACMass = (int)Math.Floor(NeutralMasses[Peak]);
                    //AKendrick_matrix [ Peak ].KMD = ( int ) Math.Floor( ( NomIUPACMass - KendrickMass ) * 1000 );
                    KMD[Peak] = (int)Math.Floor((NomIUPACMass - KendrickMass) * 1000);
                    //AKendrick_matrix [ Peak ].Zstar = NomIUPACMass % 14 - 14;
                    ZStar[Peak] = NomIUPACMass % 14 - 14;
                }
                for (var Peak = 0; Peak < NeutralMasses.Length; Peak++)
                {
                    if (!IsFormula(Formulas[Peak])) { continue; }
                    var startform = (short[])Formulas[Peak].Clone();
                    var FirstRelpk = -1;
                    for (var relpk = 0; relpk < NeutralMasses.Length; relpk++)
                    {
                        //if( ( AKendrick_matrix [ relpk ].KMD == AKendrick_matrix [ Peak ].KMD && AKendrick_matrix [ relpk ].Zstar == AKendrick_matrix [ Peak ].Zstar ) == false ) {
                        if ((KMD[relpk] != KMD[Peak] || ZStar[relpk] != ZStar[Peak]))
                        {
                            continue;
                        }
                        if (FirstRelpk == -1)
                        {
                            FirstRelpk = relpk;
                            continue;
                        }
                        //int numCH2 = ( int ) Math.Floor( ( AKendrick_matrix [ relpk ].peak_mass - AKendrick_matrix [ FirstRelpk ].peak_mass ) / 14 );
                        var numCH2 = (int)Math.Floor((NeutralMasses[relpk] - NeutralMasses[FirstRelpk]) / 14);
                        var newform = (short[])startform.Clone();
                        //KL change this to be more general
                        newform[(int)EElemIndex.C] = (short)(newform[(int)EElemIndex.C] + 1 * numCH2);
                        newform[(int)EElemIndex.H] = (short)(newform[(int)EElemIndex.H] + 2 * numCH2);
                        var checknew = CheckFormulaByFilters(newform, NeutralMasses[relpk]);
                        var checkold = CheckFormulaByFilters(Formulas[relpk], NeutralMasses[relpk]);
                        var errornew = CPpmError.AbsPpmError(FormulaToNeutralMass(newform), NeutralMasses[relpk]);
                        //if( ( checkold == true ) && ( checknew == true ) && ( Math.Abs( errornew ) <= FormulaErrorPPM / 2 ) ){
                        if (checkold && checknew && (Math.Abs(errornew) <= GetRealFormulaPpmError(NeutralMasses[Peak])))
                        {
                            if (UseCIAFormulaScore)
                            {
                                if ((newform[(int)EElemIndex.S] + newform[(int)EElemIndex.P]) < (Formulas[relpk][(int)EElemIndex.S] + Formulas[relpk][(int)EElemIndex.P]))
                                {
                                    Formulas[relpk] = (short[])newform.Clone();
                                    PPMErrors[relpk] = errornew;
                                }
                            }
                            else
                            {
                                if (IsNewFormulaScoreBetter(Formulas[relpk], PPMErrors[relpk], newform, errornew))
                                {
                                    Formulas[relpk] = (short[])newform.Clone();
                                    PPMErrors[relpk] = errornew;
                                }
                            }
                            //} else if( ( checkold == false ) && ( checknew == true ) && ( Math.Abs( errornew ) <= FormulaErrorPPM / 2 ) ) {
                        }
                        else if ((!checkold) && checknew && (Math.Abs(errornew) <= GetRealFormulaPpmError(NeutralMasses[Peak]) / 2))
                        {
                            Formulas[relpk] = (short[])newform.Clone();
                            PPMErrors[relpk] = errornew;
                        }
                    }
                }
            }
        }

        private short[] PickDBFormula(double peak_mass, out short Candidates)
        {//useMATfile_KL4
            Candidates = 0;
            var BestFormula = (short[])NullFormula.Clone();
            //using the master list of compounds - can keep them in the array and then use arrayfun to search through the different cells in sData
            //KL 12/15/08
            //1/2/09 - can have the case where multiple compounds have the same number of non-oxygen heteroatoms, so that isn't always the best way to sort.
            //If that happens, from the options with the lowest number of non-oxygen heteroatoms, chose the one with the lowest error
            //KL 1/6/09 - convert elemformula to double to get the precision I need

            int DBLowerIndex;
            int DBUpperIndex;
            if (!GetDBLimitIndexes(peak_mass, out DBLowerIndex, out DBUpperIndex)) { return BestFormula; }
            Candidates = (short)(DBUpperIndex - DBLowerIndex + 1);
            double BestMass = 0;
            double BestErrorPPM = 0;
            for (var DBIndex = DBLowerIndex; DBIndex <= DBUpperIndex; DBIndex++)
            {
                //if( UseFormulaFilters == true ) {
                if (!CheckFormulaByFilters(DBFormulas[DBIndex], DBMasses[DBIndex]))
                {
                    continue;
                }
                //}
                var DBMass = DBMasses[DBIndex];
                var DBFormula = DBFormulas[DBIndex];
                var ErrorPPMToDBMass = Math.Abs(CPpmError.AbsPpmError(DBMass, peak_mass));
                var Change = false;
                if (!IsFormula(BestFormula))
                {
                    Change = true;
                }
                else
                {
                    if (IsNewFormulaScoreBetter(BestFormula, BestErrorPPM, DBFormula, ErrorPPMToDBMass))
                    {
                        Change = true;
                    }
                }
                if (Change)
                {
                    BestMass = DBMass;
                    BestFormula = (short[])DBFormula.Clone();
                    BestErrorPPM = ErrorPPMToDBMass;
                }
            }
            return BestFormula;
        }
        private bool IsNewFormulaScoreBetter(short[] Formula, double FormulaError, short[] NewFormula, double NewFormulaError)
        {
            switch (FormulaScore)
            {
                case EFormulaScore.MinSPAndError:
                    //select the first elemental formula on the list (lowest # of non oxygen heteroatoms
                    //1/20/09: change to test if only sort based on S and P
                    //KL 1/2/09 addition - if multiple with the same, low, # of non-oxygen heteroatoms, sort based on the lowest error AND
                    //lowest number of non-oxygen heteroatoms (S and P only here).
                    var SPSum = Formula[(int)EElemIndex.S] + Formula[(int)EElemIndex.P];
                    var NewSPSum = NewFormula[(int)EElemIndex.S] + NewFormula[(int)EElemIndex.P];
                    if (NewSPSum < SPSum) { return true; }
                    else if (NewSPSum == SPSum)
                    {
                        //then only consider formulas with P <= 1 or S <= 3
                        var Sle3AndPle1 = (Formula[(int)EElemIndex.S] <= 3) & (Formula[(int)EElemIndex.P] <= 1);
                        var NewSle3AndPle1 = (NewFormula[(int)EElemIndex.S] <= 3) & (NewFormula[(int)EElemIndex.P] <= 1);
                        if ((!Sle3AndPle1) && NewSle3AndPle1)
                        {
                            return true;
                        }
                        else if ((Sle3AndPle1 == NewSle3AndPle1) && (FormulaError > NewFormulaError))
                        {
                            return true;
                        }
                    }
                    break;
                case EFormulaScore.lowestError:
                    //sort based on the lowest error - for comparison
                    if (NewFormulaError < FormulaError) { return true; }
                    break;
                case EFormulaScore.MinNSPAndError:
                    //public enum EElemNumber { C = 0, H, O, N, C13, S, P, Na};
                    //added 4/15/09 by KL cap the number of S and P atoms, after selecting based on the lowest number of N, S, and P
                    //4/15/09 by KL cap the number of S and P atoms, after selecting based on the lowest number of N, S, and P added 2/24/14 by NT; for masses under 350 limit number of N to 3 calculate the # of non-oxygen heteroatoms
                    var NSPSum = Formula[(int)EElemIndex.N] + Formula[(int)EElemIndex.S] + Formula[(int)EElemIndex.P];
                    var NewNSPSum = NewFormula[(int)EElemIndex.N] + NewFormula[(int)EElemIndex.S] + NewFormula[(int)EElemIndex.P];
                    if (NewNSPSum < NSPSum)
                    {
                        //only consider the formulas with the low # non-oxy HA
                        return true;
                    }
                    else if (NSPSum == NewNSPSum)
                    {
                        //then only consider formulas with P <= 1 or S <= 3
                        var Sle3AndPle1 = (Formula[(int)EElemIndex.S] <= 3) & (Formula[(int)EElemIndex.P] <= 1);
                        var NewSle3AndPle1 = (NewFormula[(int)EElemIndex.S] <= 3) & (NewFormula[(int)EElemIndex.P] <= 1);
                        if ((!Sle3AndPle1) && NewSle3AndPle1)
                        {
                            return true;
                        }
                        else if ((Sle3AndPle1 == NewSle3AndPle1) && (FormulaError > NewFormulaError))
                        {
                            return true;
                        }
                    }
                    break;
                case EFormulaScore.MinONSPAndError:
                    var ONSPSum = Formula[(int)EElemIndex.O] + Formula[(int)EElemIndex.N] + Formula[(int)EElemIndex.S] + Formula[(int)EElemIndex.P];
                    var NewONSPSum = NewFormula[(int)EElemIndex.O] + NewFormula[(int)EElemIndex.N] + NewFormula[(int)EElemIndex.S] + NewFormula[(int)EElemIndex.P];
                    if (NewONSPSum < ONSPSum)
                    {
                        //only consider the formulas with the low # non-oxy HA
                        return true;
                    }
                    else if (ONSPSum == NewONSPSum)
                    {
                        //then only consider formulas with P <= 1 or S <= 3
                        var Sle3AndPle1 = (Formula[(int)EElemIndex.S] <= 3) & (Formula[(int)EElemIndex.P] <= 1);
                        var NewSle3AndPle1 = (NewFormula[(int)EElemIndex.S] <= 3) & (NewFormula[(int)EElemIndex.P] <= 1);
                        if ((!Sle3AndPle1) && NewSle3AndPle1)
                        {
                            return true;
                        }
                        else if ((Sle3AndPle1 == NewSle3AndPle1) && (FormulaError > NewFormulaError))
                        {
                            return true;
                        }
                    }
                    break;
                case EFormulaScore.UserDefined:
                    if (UserDefinedScoreTable != null)
                    {
                        //PrefixFirst
                        UserDefinedScoreTable.Rows[0][PrefixFirst + "Mass"] = FormulaToNeutralMass(Formula);
                        UserDefinedScoreTable.Rows[0][PrefixFirst + "Error"] = FormulaError;
                        for (var Element = 0; Element < ElementCount; Element++)
                        {
                            UserDefinedScoreTable.Rows[0][PrefixFirst + Enum.GetName(typeof(EElemIndex), Element)] = Formula[Element];
                        }
                        //PrefixSecond
                        UserDefinedScoreTable.Rows[0][PrefixSecond + "Mass"] = FormulaToNeutralMass(NewFormula);
                        UserDefinedScoreTable.Rows[0][PrefixSecond + "Error"] = NewFormulaError;
                        for (var Element = 0; Element < ElementCount; Element++)
                        {
                            UserDefinedScoreTable.Rows[0][PrefixSecond + Enum.GetName(typeof(EElemIndex), Element)] = NewFormula[Element];
                        }
                        if (!(bool)UserDefinedScoreTable.Rows[0]["UserDefinedScore"])
                        {
                            return false;
                        }
                    }
                    break;
                default:
                    throw new Exception("Wrong SortType : " + FormulaScore.ToString());
            }
            return false;
        }
        public string GetSaveParameterText(string Filename = "")
        {
            //StringBuilder XmlString = new StringBuilder();
            //XmlWriterSettings Settings = new XmlWriterSettings( );
            //Settings.Encoding = Encoding.UTF8;
            //XmlWriter xmlWriter = XmlWriter.Create( XmlString, Settings );
            //XmlWriter xmlWriter = XmlWriter.Create( XmlString);
            //xmlWriter.WriteStartDocument();
            var XmlDoc = new XmlDocument();
            //xmlWriter.WriteStartElement( "DefaultParameters" );
            XmlNode RootNode = XmlDoc.CreateElement("DefaultParameters");
            XmlNode FirstLayerNode;
            XmlNode SecondLayerNode;
            XmlNode ThirdLayerNode;
            XmlNode FourthLayerNode;
            XmlDoc.AppendChild(RootNode);
            //xmlWriter.WriteStartElement( "InputFilesTab" );
            FirstLayerNode = XmlDoc.CreateElement("InputFilesTab");
            //xmlWriter.WriteStartElement( "Adduct" );
            //xmlWriter.WriteString( Ipa.Adduct );
            //xmlWriter.WriteEndElement();
            SecondLayerNode = XmlDoc.CreateElement("Adduct");
            SecondLayerNode.InnerText = Ipa.Adduct;
            FirstLayerNode.AppendChild(SecondLayerNode);
            //xmlWriter.WriteStartElement( "Ionization" );
            //xmlWriter.WriteString( Ipa.Ionization.ToString() );
            //xmlWriter.WriteEndElement();
            SecondLayerNode = XmlDoc.CreateElement("Ionization");
            SecondLayerNode.InnerText = Ipa.Ionization.ToString();
            FirstLayerNode.AppendChild(SecondLayerNode);
            //xmlWriter.WriteStartElement( "Charge" );
            //xmlWriter.WriteString( Ipa.CS.ToString() );
            //xmlWriter.WriteEndElement();
            SecondLayerNode = XmlDoc.CreateElement("Charge");
            SecondLayerNode.InnerText = Ipa.CS.ToString();
            FirstLayerNode.AppendChild(SecondLayerNode);
            //xmlWriter.WriteStartElement( "UsePreAlignment" );
            //xmlWriter.WriteString( GetPreAlignment().ToString() );
            //xmlWriter.WriteEndElement();
            SecondLayerNode = XmlDoc.CreateElement("UsePreAlignment");
            SecondLayerNode.InnerText = GetPreAlignment().ToString();
            FirstLayerNode.AppendChild(SecondLayerNode);
            //xmlWriter.WriteStartElement( "Calibration" );
            SecondLayerNode = XmlDoc.CreateElement("Calibration");
            //xmlWriter.WriteStartElement( "RefPeakFileName" );
            //xmlWriter.WriteString( RefPeakFileName.Replace( " ", "\u0020") );
            //xmlWriter.WriteEndElement();
            ThirdLayerNode = XmlDoc.CreateElement("RefPeakFileName");
            ThirdLayerNode.InnerText = RefPeakFilename.Replace(" ", "\u0020");
            SecondLayerNode.AppendChild(ThirdLayerNode);
            //xmlWriter.WriteStartElement( "Regression" );
            //xmlWriter.WriteString( oTotalCalibration.ttl_cal_regression.ToString() );
            //xmlWriter.WriteEndElement();
            ThirdLayerNode = XmlDoc.CreateElement("Regression");
            ThirdLayerNode.InnerText = oTotalCalibration.ttl_cal_regression.ToString();
            SecondLayerNode.AppendChild(ThirdLayerNode);
            //xmlWriter.WriteStartElement( "RelFactor" );
            //xmlWriter.WriteString( oTotalCalibration.ttl_cal_rf.ToString() );
            //xmlWriter.WriteEndElement();
            ThirdLayerNode = XmlDoc.CreateElement("RelFactor");
            ThirdLayerNode.InnerText = oTotalCalibration.ttl_cal_rf.ToString();
            SecondLayerNode.AppendChild(ThirdLayerNode);
            //xmlWriter.WriteStartElement( "StartTolerance" );
            //xmlWriter.WriteString( oTotalCalibration.ttl_cal_start_ppm.ToString() );
            //xmlWriter.WriteEndElement();
            ThirdLayerNode = XmlDoc.CreateElement("StartTolerance");
            ThirdLayerNode.InnerText = oTotalCalibration.ttl_cal_start_ppm.ToString();
            SecondLayerNode.AppendChild(ThirdLayerNode);
            //xmlWriter.WriteStartElement( "EndTolerance" );
            //xmlWriter.WriteString( oTotalCalibration.ttl_cal_target_ppm.ToString() );
            //xmlWriter.WriteEndElement();
            ThirdLayerNode = XmlDoc.CreateElement("EndTolerance");
            ThirdLayerNode.InnerText = oTotalCalibration.ttl_cal_target_ppm.ToString();
            SecondLayerNode.AppendChild(ThirdLayerNode);
            //xmlWriter.WriteStartElement( "PeakFilters" );
            ThirdLayerNode = XmlDoc.CreateElement("PeakFilters");
            //xmlWriter.WriteStartElement( "MinSToN" );
            //xmlWriter.WriteString( oTotalCalibration.ttl_cal_min_sn.ToString() );
            //xmlWriter.WriteEndElement();
            FourthLayerNode = XmlDoc.CreateElement("MinSToN");
            FourthLayerNode.InnerText = oTotalCalibration.ttl_cal_min_sn.ToString();
            ThirdLayerNode.AppendChild(FourthLayerNode);
            //xmlWriter.WriteStartElement( "MinRelAbun" );
            //xmlWriter.WriteString( oTotalCalibration.ttl_cal_min_abu_pct.ToString() );
            //xmlWriter.WriteEndElement();
            FourthLayerNode = XmlDoc.CreateElement("MinRelAbun");
            FourthLayerNode.InnerText = oTotalCalibration.ttl_cal_min_abu_pct.ToString();
            ThirdLayerNode.AppendChild(FourthLayerNode);
            //xmlWriter.WriteStartElement( "MaxRelAbun" );
            //xmlWriter.WriteString( oTotalCalibration.ttl_cal_max_abu_pct.ToString() );
            //xmlWriter.WriteEndElement();
            FourthLayerNode = XmlDoc.CreateElement("MaxRelAbun");
            FourthLayerNode.InnerText = oTotalCalibration.ttl_cal_max_abu_pct.ToString();
            ThirdLayerNode.AppendChild(FourthLayerNode);
            //xmlWriter.WriteEndElement();
            SecondLayerNode.AppendChild(ThirdLayerNode);
            //xmlWriter.WriteEndElement();
            FirstLayerNode.AppendChild(SecondLayerNode);
            //xmlWriter.WriteEndElement();//InputFilesTab
            RootNode.AppendChild(FirstLayerNode);

            //xmlWriter.WriteStartElement( "CiaTab" );
            FirstLayerNode = XmlDoc.CreateElement("CiaTab");
            //xmlWriter.WriteStartElement( "UseAlignment" );
            //xmlWriter.WriteString( GetAlignment().ToString() );
            //xmlWriter.WriteEndElement();
            SecondLayerNode = XmlDoc.CreateElement("UseAlignment");
            SecondLayerNode.InnerText = GetAlignment().ToString();
            FirstLayerNode.AppendChild(SecondLayerNode);
            //xmlWriter.WriteStartElement( "AlignmentTolerance" );
            //xmlWriter.WriteString( GetAlignmentPpmTolerance().ToString() );
            //xmlWriter.WriteEndElement();
            SecondLayerNode = XmlDoc.CreateElement("AlignmentTolerance");
            SecondLayerNode.InnerText = GetAlignmentPpmTolerance().ToString();
            FirstLayerNode.AppendChild(SecondLayerNode);
            //xmlWriter.WriteStartElement( "CiaDBFilename" );
            //xmlWriter.WriteString( GetCiaDBFilename() );
            //xmlWriter.WriteEndElement();
            SecondLayerNode = XmlDoc.CreateElement("CiaDBFilename");
            SecondLayerNode.InnerText = GetCiaDBFilename().Replace(" ", "\u0020");
            FirstLayerNode.AppendChild(SecondLayerNode);
            SecondLayerNode = XmlDoc.CreateElement("StaticPpmDynamicStdDevError");
            SecondLayerNode.InnerText = GetStaticDynamicPpmError().ToString();
            FirstLayerNode.AppendChild(SecondLayerNode);
            SecondLayerNode = XmlDoc.CreateElement("StdDevErrorGain");
            SecondLayerNode.InnerText = GetStdDevErrorGain().ToString();
            FirstLayerNode.AppendChild(SecondLayerNode);
            //xmlWriter.WriteStartElement( "FormulaTolerance" );
            //xmlWriter.WriteString( GetFormulaPPMTolerance().ToString() );
            //xmlWriter.WriteEndElement();
            SecondLayerNode = XmlDoc.CreateElement("FormulaTolerance");
            SecondLayerNode.InnerText = GetFormulaPPMTolerance().ToString();
            FirstLayerNode.AppendChild(SecondLayerNode);
            //xmlWriter.WriteStartElement( "DBMassLimit" );
            //xmlWriter.WriteString( GetMassLimit().ToString() );
            //xmlWriter.WriteEndElement();
            SecondLayerNode = XmlDoc.CreateElement("DBMassLimit");
            SecondLayerNode.InnerText = GetMassLimit().ToString();
            FirstLayerNode.AppendChild(SecondLayerNode);
            //xmlWriter.WriteStartElement( "FormulaScore" );
            //xmlWriter.WriteString( GetFormulaScore().ToString() );
            //xmlWriter.WriteEndElement();
            SecondLayerNode = XmlDoc.CreateElement("FormulaScore");
            SecondLayerNode.InnerText = GetFormulaScore().ToString();
            FirstLayerNode.AppendChild(SecondLayerNode);
            //xmlWriter.WriteStartElement( "UseKendrick" );
            //xmlWriter.WriteString( GetUseKendrick().ToString() );
            //xmlWriter.WriteEndElement();
            SecondLayerNode = XmlDoc.CreateElement("UseKendrick");
            SecondLayerNode.InnerText = GetUseKendrick().ToString();
            FirstLayerNode.AppendChild(SecondLayerNode);
            //xmlWriter.WriteStartElement( "UseC13" );
            //xmlWriter.WriteString( GetUseC13().ToString() );
            //xmlWriter.WriteEndElement();
            SecondLayerNode = XmlDoc.CreateElement("UseC13");
            SecondLayerNode.InnerText = GetUseC13().ToString();
            FirstLayerNode.AppendChild(SecondLayerNode);
            //xmlWriter.WriteStartElement( "C13Tolerance" );
            //xmlWriter.WriteString( GetC13Tolerance().ToString() );
            //xmlWriter.WriteEndElement();
            SecondLayerNode = XmlDoc.CreateElement("C13Tolerance");
            SecondLayerNode.InnerText = GetC13Tolerance().ToString();
            FirstLayerNode.AppendChild(SecondLayerNode);
            //xmlWriter.WriteStartElement( "UseFormulaFilters" );
            //xmlWriter.WriteString( GetUseFormulaFilter().ToString() );
            //xmlWriter.WriteEndElement();
            SecondLayerNode = XmlDoc.CreateElement("UseFormulaFilters");
            SecondLayerNode.InnerText = GetUseFormulaFilter().ToString();
            FirstLayerNode.AppendChild(SecondLayerNode);
            //xmlWriter.WriteStartElement( "ElementalCounts" );
            //xmlWriter.WriteString( GetGoldenRuleFilterUsage() [ 0 ].ToString() );
            //xmlWriter.WriteEndElement();
            SecondLayerNode = XmlDoc.CreateElement("ElementalCounts");
            SecondLayerNode.InnerText = GetGoldenRuleFilterUsage()[0].ToString();
            FirstLayerNode.AppendChild(SecondLayerNode);
            //xmlWriter.WriteStartElement( "ValenceRules" );
            //xmlWriter.WriteString( GetGoldenRuleFilterUsage() [ 1 ].ToString() );
            //xmlWriter.WriteEndElement();
            SecondLayerNode = XmlDoc.CreateElement("ValenceRules");
            SecondLayerNode.InnerText = GetGoldenRuleFilterUsage()[1].ToString();
            FirstLayerNode.AppendChild(SecondLayerNode);
            //xmlWriter.WriteStartElement( "ElementalRatios" );
            //xmlWriter.WriteString( GetGoldenRuleFilterUsage() [ 2 ].ToString() );
            //xmlWriter.WriteEndElement();
            SecondLayerNode = XmlDoc.CreateElement("ElementalRatios");
            SecondLayerNode.InnerText = GetGoldenRuleFilterUsage()[2].ToString();
            FirstLayerNode.AppendChild(SecondLayerNode);
            //xmlWriter.WriteStartElement( "HeteroatomCounts" );
            //xmlWriter.WriteString( GetGoldenRuleFilterUsage() [ 3 ].ToString() );
            //xmlWriter.WriteEndElement();
            SecondLayerNode = XmlDoc.CreateElement("HeteroatomCounts");
            SecondLayerNode.InnerText = GetGoldenRuleFilterUsage()[3].ToString();
            FirstLayerNode.AppendChild(SecondLayerNode);
            //xmlWriter.WriteStartElement( "PositiveAtoms" );
            //xmlWriter.WriteString( GetGoldenRuleFilterUsage() [ 4 ].ToString() );
            //xmlWriter.WriteEndElement();
            SecondLayerNode = XmlDoc.CreateElement("PositiveAtoms");
            SecondLayerNode.InnerText = GetGoldenRuleFilterUsage()[4].ToString();
            FirstLayerNode.AppendChild(SecondLayerNode);
            //xmlWriter.WriteStartElement( "IntegerDBE" );
            //xmlWriter.WriteString( GetGoldenRuleFilterUsage() [ 5 ].ToString() );
            //xmlWriter.WriteEndElement();
            SecondLayerNode = XmlDoc.CreateElement("IntegerDBE");
            SecondLayerNode.InnerText = GetGoldenRuleFilterUsage()[5].ToString();
            FirstLayerNode.AppendChild(SecondLayerNode);
            //xmlWriter.WriteStartElement( "SpecialFilter" );
            //xmlWriter.WriteString( GetSpecialFilter().ToString() );
            //xmlWriter.WriteEndElement();
            SecondLayerNode = XmlDoc.CreateElement("SpecialFilter");
            SecondLayerNode.InnerText = GetSpecialFilter().ToString();
            FirstLayerNode.AppendChild(SecondLayerNode);
            //xmlWriter.WriteStartElement( "UserDefinedFilter" );
            //xmlWriter.WriteString( GetUserDefinedFilter() );
            //xmlWriter.WriteEndElement();
            SecondLayerNode = XmlDoc.CreateElement("UserDefinedFilter");
            SecondLayerNode.InnerText = GetUserDefinedFilter().ToString();
            FirstLayerNode.AppendChild(SecondLayerNode);
            //xmlWriter.WriteStartElement( "UseRelation" );
            //xmlWriter.WriteString( GetUseRelation().ToString() );
            //xmlWriter.WriteEndElement();
            SecondLayerNode = XmlDoc.CreateElement("UseRelation");
            SecondLayerNode.InnerText = GetUseRelation().ToString();
            FirstLayerNode.AppendChild(SecondLayerNode);
            //xmlWriter.WriteStartElement( "MaxRelationGaps" );
            //xmlWriter.WriteString( GetMaxRelationGaps().ToString() );
            //xmlWriter.WriteEndElement();
            SecondLayerNode = XmlDoc.CreateElement("MaxRelationGaps");
            SecondLayerNode.InnerText = GetMaxRelationGaps().ToString();
            FirstLayerNode.AppendChild(SecondLayerNode);
            //xmlWriter.WriteStartElement( "UseBackward" );
            //xmlWriter.WriteString( GetUseBackward().ToString() );
            //xmlWriter.WriteEndElement();
            SecondLayerNode = XmlDoc.CreateElement("UseBackward");
            SecondLayerNode.InnerText = GetUseBackward().ToString();
            FirstLayerNode.AppendChild(SecondLayerNode);
            SecondLayerNode = XmlDoc.CreateElement("RelationErrorType");
            SecondLayerNode.InnerText = GetRelationErrorType().ToString();
            FirstLayerNode.AppendChild(SecondLayerNode);
            //xmlWriter.WriteStartElement( "RelationError" );
            //xmlWriter.WriteString( GetRelationErrorAMU().ToString() );
            //xmlWriter.WriteEndElement();
            SecondLayerNode = XmlDoc.CreateElement("RelationError");
            SecondLayerNode.InnerText = GetRelationErrorAMU().ToString();
            FirstLayerNode.AppendChild(SecondLayerNode);
            //xmlWriter.WriteStartElement( "FormulaBuildingBlocks" );
            SecondLayerNode = XmlDoc.CreateElement("FormulaBuildingBlocks");
            for (var BlockIndex = 0; BlockIndex < RelationBuildingBlockFormulas.Length; BlockIndex++)
            {
                //xmlWriter.WriteStartElement( FormulaToName( RelationBuildingBlockFormulas [ BlockIndex ] ) );
                //xmlWriter.WriteString( ActiveRelationBuildingBlocks [ BlockIndex ].ToString() );
                //xmlWriter.WriteEndElement();
                ThirdLayerNode = XmlDoc.CreateElement(FormulaToName(RelationBuildingBlockFormulas[BlockIndex]));
                ThirdLayerNode.InnerText = ActiveRelationBuildingBlocks[BlockIndex].ToString();
                SecondLayerNode.AppendChild(ThirdLayerNode);
            }
            //xmlWriter.WriteEndElement();
            FirstLayerNode.AppendChild(SecondLayerNode);

            //xmlWriter.WriteStartElement( "Output" );
            SecondLayerNode = XmlDoc.CreateElement("Output");
            //xmlWriter.WriteStartElement( "IndividualFileReports" );
            //xmlWriter.WriteString( GetGenerateIndividualFileReports().ToString() );
            //xmlWriter.WriteEndElement();
            ThirdLayerNode = XmlDoc.CreateElement("GenerateReports");
            ThirdLayerNode.InnerText = GetGenerateReports().ToString();
            SecondLayerNode.AppendChild(ThirdLayerNode);
            //xmlWriter.WriteStartElement( "Delimiters" );
            //xmlWriter.WriteString( OutputFileDelimiter );
            //xmlWriter.WriteEndElement();
            //ThirdLayerNode = XmlDoc.CreateElement( "Delimiters" );
            //ThirdLayerNode.InnerText =  OutputFileDelimiter;
            //SecondLayerNode.AppendChild( ThirdLayerNode );
            //xmlWriter.WriteStartElement( "Error" );
            //xmlWriter.WriteString( oEErrorType.ToString() );
            //xmlWriter.WriteEndElement();
            ThirdLayerNode = XmlDoc.CreateElement("Error");
            ThirdLayerNode.InnerText = oEErrorType.ToString();
            SecondLayerNode.AppendChild(ThirdLayerNode);
            //xmlWriter.WriteEndElement();
            FirstLayerNode.AppendChild(SecondLayerNode);
            //xmlWriter.WriteEndElement();
            RootNode.AppendChild(FirstLayerNode);
            //end CiaTab
            //xmlWriter.WriteStartElement( "IpaTab" );
            FirstLayerNode = XmlDoc.CreateElement("IpaTab");
            //xmlWriter.WriteStartElement( "IpaDBFilename" );
            //xmlWriter.WriteString( IpaDBFilename);
            //xmlWriter.WriteEndElement();
            SecondLayerNode = XmlDoc.CreateElement("IpaDBFilename");
            SecondLayerNode.InnerText = IpaDBFilename.Replace(" ", "\u0020");
            FirstLayerNode.AppendChild(SecondLayerNode);
            //xmlWriter.WriteStartElement( "MassTol" );
            //xmlWriter.WriteString( Ipa.m_ppm_tol.ToString() );
            //xmlWriter.WriteEndElement();
            SecondLayerNode = XmlDoc.CreateElement("MassTol");
            SecondLayerNode.InnerText = Ipa.m_ppm_tol.ToString();
            FirstLayerNode.AppendChild(SecondLayerNode);
            //xmlWriter.WriteStartElement( "MajorPeaksMinSToN" );
            //xmlWriter.WriteString( Ipa.m_min_major_sn.ToString() );
            //xmlWriter.WriteEndElement();
            SecondLayerNode = XmlDoc.CreateElement("MajorPeaksMinSToN");
            SecondLayerNode.InnerText = Ipa.m_min_major_sn.ToString();
            FirstLayerNode.AppendChild(SecondLayerNode);
            //xmlWriter.WriteStartElement( "MinorPeaksMinSToN" );
            //xmlWriter.WriteString( Ipa.m_min_minor_sn.ToString() );
            //xmlWriter.WriteEndElement();
            SecondLayerNode = XmlDoc.CreateElement("MinorPeaksMinSToN");
            SecondLayerNode.InnerText = Ipa.m_min_minor_sn.ToString();
            FirstLayerNode.AppendChild(SecondLayerNode);
            //xmlWriter.WriteStartElement( "MatchedPeakReport" );
            //xmlWriter.WriteString( Ipa.m_matched_peaks_report.ToString() );
            //xmlWriter.WriteEndElement();
            SecondLayerNode = XmlDoc.CreateElement("MatchedPeakReport");
            SecondLayerNode.InnerText = Ipa.m_matched_peaks_report.ToString();
            FirstLayerNode.AppendChild(SecondLayerNode);
            //xmlWriter.WriteStartElement( "MinPeakProbabilityToScore" );
            //xmlWriter.WriteString( Ipa.m_min_p_to_score.ToString() );
            //xmlWriter.WriteEndElement();
            SecondLayerNode = XmlDoc.CreateElement("MinPeakProbabilityToScore");
            SecondLayerNode.InnerText = Ipa.m_min_p_to_score.ToString();
            FirstLayerNode.AppendChild(SecondLayerNode);
            //xmlWriter.WriteStartElement( "DbFilter" );
            //xmlWriter.WriteString( Ipa.m_IPDB_ec_filter );
            //xmlWriter.WriteEndElement();
            SecondLayerNode = XmlDoc.CreateElement("DbFilter");
            SecondLayerNode.InnerText = Ipa.m_IPDB_ec_filter;
            FirstLayerNode.AppendChild(SecondLayerNode);
            //xmlWriter.WriteEndElement();//IpaTab
            RootNode.AppendChild(FirstLayerNode);
            //xmlWriter.WriteEndDocument();
            //xmlWriter.Flush();
            //xmlWriter.Close();
            if (Filename.Length > 0)
            {
                XmlDoc.Save(Filename);
            }
            return XmlDoc.InnerXml;
        }
        public void LoadParameters(string Filename)
        {
            var XmlDoc = new XmlDocument();
            XmlDoc.Load(Filename);
            XmlNode XmlDocRoot = XmlDoc.DocumentElement;
            var XmlNode = XmlDocRoot.SelectSingleNode("//DefaultParameters/InputFilesTab/Adduct");
            if (XmlNode != null)
            {
                Ipa.Adduct = XmlNode.InnerText;
            }
            XmlNode = XmlDocRoot.SelectSingleNode("//DefaultParameters/InputFilesTab/Ionization");
            if (XmlNode != null)
            {
                Ipa.Ionization = (TestFSDBSearch.TotalSupport.IonizationMethod)Enum.Parse(typeof(TestFSDBSearch.TotalSupport.IonizationMethod), XmlNode.InnerText);
            }
            XmlNode = XmlDocRoot.SelectSingleNode("//DefaultParameters/InputFilesTab/Charge");
            if (XmlNode != null)
            {
                Ipa.CS = int.Parse(XmlNode.InnerText);
            }
            XmlNode = XmlDocRoot.SelectSingleNode("//DefaultParameters/InputFilesTab/UsePreAlignment");
            if (XmlNode != null)
            {
                SetPreAlignment(bool.Parse(XmlNode.InnerText));
            }
            XmlNode = XmlDocRoot.SelectSingleNode("//DefaultParameters/InputFilesTab/Calibration/RefPeakFileName");
            if (XmlNode != null)
            {
                RefPeakFilename = XmlNode.InnerText.Replace("\u0020", " ");
            }
            XmlNode = XmlDocRoot.SelectSingleNode("//DefaultParameters/InputFilesTab/Calibration/Regression");
            if (XmlNode != null)
            {
                oTotalCalibration.ttl_cal_regression = (TestFSDBSearch.TotalCalibration.ttlRegressionType)Enum.Parse(typeof(TestFSDBSearch.TotalCalibration.ttlRegressionType), XmlNode.InnerText);
            }
            XmlNode = XmlDocRoot.SelectSingleNode("//DefaultParameters/InputFilesTab/Calibration/RelFactor");
            if (XmlNode != null)
            {
                oTotalCalibration.ttl_cal_rf = double.Parse(XmlNode.InnerText);
            }
            XmlNode = XmlDocRoot.SelectSingleNode("//DefaultParameters/InputFilesTab/Calibration/StartTolerance");
            if (XmlNode != null)
            {
                oTotalCalibration.ttl_cal_start_ppm = double.Parse(XmlNode.InnerText);
            }
            XmlNode = XmlDocRoot.SelectSingleNode("//DefaultParameters/InputFilesTab/Calibration/EndTolerance");
            if (XmlNode != null)
            {
                oTotalCalibration.ttl_cal_target_ppm = double.Parse(XmlNode.InnerText);
            }
            XmlNode = XmlDocRoot.SelectSingleNode("//DefaultParameters/InputFilesTab/Calibration/PeakFilters/MinSToN");
            if (XmlNode != null)
            {
                oTotalCalibration.ttl_cal_min_sn = double.Parse(XmlNode.InnerText);
            }
            XmlNode = XmlDocRoot.SelectSingleNode("//DefaultParameters/InputFilesTab/Calibration/PeakFilters/MinRelAbun");
            if (XmlNode != null)
            {
                oTotalCalibration.ttl_cal_min_abu_pct = double.Parse(XmlNode.InnerText);
            }
            XmlNode = XmlDocRoot.SelectSingleNode("//DefaultParameters/InputFilesTab/Calibration/PeakFilters/MaxRelAbun");
            if (XmlNode != null)
            {
                oTotalCalibration.ttl_cal_max_abu_pct = double.Parse(XmlNode.InnerText);
            }

            XmlNode = XmlDocRoot.SelectSingleNode("//DefaultParameters/CiaTab/UseAlignment");
            if (XmlNode != null)
            {
                SetAlignment(bool.Parse(XmlNode.InnerText));
            }
            XmlNode = XmlDocRoot.SelectSingleNode("//DefaultParameters/CiaTab/AlignmentTolerance");
            if (XmlNode != null)
            {
                SetAlignmentPpmTolerance(double.Parse(XmlNode.InnerText));
            }
            //XmlNode = XmlDocRoot.SelectSingleNode( "//DefaultParameters/CiaTab/AddChains" );
            //if ( XmlNode != null ) {
            //    SetAddChains( bool.Parse( XmlNode.InnerText ) );
            //}
            //XmlNode = XmlDocRoot.SelectSingleNode( "//DefaultParameters/CiaTab/MinPeaksPerChain" );
            //if ( XmlNode != null ) {
            //    SetMinPeaksPerChain( int.Parse( XmlNode.InnerText ) );
            //}
            XmlNode = XmlDocRoot.SelectSingleNode("//DefaultParameters/CiaTab/CiaDBFilename");
            if (XmlNode != null)
            {
                SetCiaDBFilename(XmlNode.InnerText.Replace("\u0020", " "));
            }
            XmlNode = XmlDocRoot.SelectSingleNode("//DefaultParameters/CiaTab/StaticPpmDynamicStdDevError");
            if (XmlNode != null)
            {
                SetStaticDynamicPpmError(bool.Parse(XmlNode.InnerText));
            }
            XmlNode = XmlDocRoot.SelectSingleNode("//DefaultParameters/CiaTab/StdDevErrorGain");
            if (XmlNode != null)
            {
                SetStdDevErrorGain(double.Parse(XmlNode.InnerText));
            }
            XmlNode = XmlDocRoot.SelectSingleNode("//DefaultParameters/CiaTab/FormulaTolerance");
            if (XmlNode != null)
            {
                SetFormulaPPMTolerance(double.Parse(XmlNode.InnerText));
            }
            XmlNode = XmlDocRoot.SelectSingleNode("//DefaultParameters/CiaTab/DBMassLimit");
            if (XmlNode != null)
            {
                SetMassLimit(double.Parse(XmlNode.InnerText));
            }
            XmlNode = XmlDocRoot.SelectSingleNode("//DefaultParameters/CiaTab/FormulaScore");
            if (XmlNode != null)
            {
                SetFormulaScore((CCia.EFormulaScore)Enum.Parse(typeof(CCia.EFormulaScore), XmlNode.InnerText));
            }
            XmlNode = XmlDocRoot.SelectSingleNode("//DefaultParameters/CiaTab/UseKendrick");
            if (XmlNode != null)
            {
                SetUseKendrick(bool.Parse(XmlNode.InnerText));
            }
            XmlNode = XmlDocRoot.SelectSingleNode("//DefaultParameters/CiaTab/UseC13");
            if (XmlNode != null)
            {
                SetUseC13(bool.Parse(XmlNode.InnerText));
            }
            XmlNode = XmlDocRoot.SelectSingleNode("//DefaultParameters/CiaTab/C13Tolerance");
            if (XmlNode != null)
            {
                SetC13Tolerance(double.Parse(XmlNode.InnerText));
            }
            XmlNode = XmlDocRoot.SelectSingleNode("//DefaultParameters/CiaTab/UseFormulaFilters");
            if (XmlNode != null)
            {
                SetUseFormulaFilter(bool.Parse(XmlNode.InnerText));
            }
            XmlNode = XmlDocRoot.SelectSingleNode("//DefaultParameters/CiaTab/ElementalCounts");
            var GoldenRules = new bool[6];
            if (XmlNode != null)
            {
                GoldenRules[0] = bool.Parse(XmlNode.InnerText);
            }
            XmlNode = XmlDocRoot.SelectSingleNode("//DefaultParameters/CiaTab/ValenceRules");
            if (XmlNode != null)
            {
                GoldenRules[1] = bool.Parse(XmlNode.InnerText);
            }
            XmlNode = XmlDocRoot.SelectSingleNode("//DefaultParameters/CiaTab/ElementalRatios");
            if (XmlNode != null)
            {
                GoldenRules[2] = bool.Parse(XmlNode.InnerText);
            }
            XmlNode = XmlDocRoot.SelectSingleNode("//DefaultParameters/CiaTab/HeteroatomCounts");
            if (XmlNode != null)
            {
                GoldenRules[3] = bool.Parse(XmlNode.InnerText);
            }
            XmlNode = XmlDocRoot.SelectSingleNode("//DefaultParameters/CiaTab/PositiveAtoms");
            if (XmlNode != null)
            {
                GoldenRules[4] = bool.Parse(XmlNode.InnerText);
            }
            XmlNode = XmlDocRoot.SelectSingleNode("//DefaultParameters/CiaTab/IntegerDBE");
            if (XmlNode != null)
            {
                GoldenRules[5] = bool.Parse(XmlNode.InnerText);
            }
            SetGoldenRuleFilterUsage(GoldenRules);

            XmlNode = XmlDocRoot.SelectSingleNode("//DefaultParameters/CiaTab/SpecialFilter");
            if (XmlNode != null)
            {
                SetSpecialFilter((CCia.ESpecialFilters)Enum.Parse(typeof(CCia.ESpecialFilters), XmlNode.InnerText));
            }
            XmlNode = XmlDocRoot.SelectSingleNode("//DefaultParameters/CiaTab/UserDefinedFilter");
            if (XmlNode != null)
            {
                SetUserDefinedFilter(XmlNode.InnerText);
            }
            XmlNode = XmlDocRoot.SelectSingleNode("//DefaultParameters/CiaTab/UseRelation");
            if (XmlNode != null)
            {
                SetUseRelation(bool.Parse(XmlNode.InnerText));
            }
            XmlNode = XmlDocRoot.SelectSingleNode("//DefaultParameters/CiaTab/UseBackward");
            if (XmlNode != null)
            {
                SetUseBackward(bool.Parse(XmlNode.InnerText));
            }
            XmlNode = XmlDocRoot.SelectSingleNode("//DefaultParameters/CiaTab/MaxRelationGaps");
            if (XmlNode != null)
            {
                SetMaxRelationGaps(int.Parse(XmlNode.InnerText));
            }
            XmlNode = XmlDocRoot.SelectSingleNode("//DefaultParameters/CiaTab/RelationErrorType");
            if (XmlNode != null)
            {
                SetRelationErrorType((CCia.RelationErrorType)Enum.Parse(typeof(CCia.RelationErrorType), XmlNode.InnerText));
            }
            XmlNode = XmlDocRoot.SelectSingleNode("//DefaultParameters/CiaTab/RelationError");
            if (XmlNode != null)
            {
                SetRelationErrorAMU(double.Parse(XmlNode.InnerText));
            }
            XmlNode = XmlDocRoot.SelectSingleNode("//DefaultParameters/CiaTab/FormulaBuildingBlocks/CH2");
            if (XmlNode != null)
            {
                ActiveRelationBuildingBlocks[0] = bool.Parse(XmlNode.InnerText);
            }
            XmlNode = XmlDocRoot.SelectSingleNode("//DefaultParameters/CiaTab/FormulaBuildingBlocks/CH4O-1");
            if (XmlNode != null)
            {
                ActiveRelationBuildingBlocks[1] = bool.Parse(XmlNode.InnerText);
            }
            XmlNode = XmlDocRoot.SelectSingleNode("//DefaultParameters/CiaTab/FormulaBuildingBlocks/H2");
            if (XmlNode != null)
            {
                ActiveRelationBuildingBlocks[2] = bool.Parse(XmlNode.InnerText);
            }
            XmlNode = XmlDocRoot.SelectSingleNode("//DefaultParameters/CiaTab/FormulaBuildingBlocks/C2H4O");
            if (XmlNode != null)
            {
                ActiveRelationBuildingBlocks[3] = bool.Parse(XmlNode.InnerText);
            }
            XmlNode = XmlDocRoot.SelectSingleNode("//DefaultParameters/CiaTab/FormulaBuildingBlocks/CO2");
            if (XmlNode != null)
            {
                ActiveRelationBuildingBlocks[4] = bool.Parse(XmlNode.InnerText);
            }
            XmlNode = XmlDocRoot.SelectSingleNode("//DefaultParameters/CiaTab/FormulaBuildingBlocks/C2H2O");
            if (XmlNode != null)
            {
                ActiveRelationBuildingBlocks[5] = bool.Parse(XmlNode.InnerText);
            }
            XmlNode = XmlDocRoot.SelectSingleNode("//DefaultParameters/CiaTab/FormulaBuildingBlocks/O");
            if (XmlNode != null)
            {
                ActiveRelationBuildingBlocks[6] = bool.Parse(XmlNode.InnerText);
            }
            XmlNode = XmlDocRoot.SelectSingleNode("//DefaultParameters/CiaTab/FormulaBuildingBlocks/HN");
            if (XmlNode != null)
            {
                ActiveRelationBuildingBlocks[8] = bool.Parse(XmlNode.InnerText);
            }
            XmlNode = XmlDocRoot.SelectSingleNode("//DefaultParameters/CiaTab/FormulaBuildingBlocks/CH2O");
            if (XmlNode != null)
            {
                ActiveRelationBuildingBlocks[8] = bool.Parse(XmlNode.InnerText);
            }
            SetActiveRelationFormulaBuildingBlocks(ActiveRelationBuildingBlocks);
            XmlNode = XmlDocRoot.SelectSingleNode("//DefaultParameters/CiaTab/Output/GenerateReports");
            if (XmlNode != null)
            {
                SetGenerateReports(bool.Parse(XmlNode.InnerText));
            }
            //XmlNode = XmlDocRoot.SelectSingleNode( "//DefaultParameters/CiaTab/Output/Delimiters" );
            //if ( XmlNode != null ) {
            //    SetOutputFileDelimiterType( OutputFileDelimiterToEnum( XmlNode.InnerText ) );
            //}
            XmlNode = XmlDocRoot.SelectSingleNode("//DefaultParameters/CiaTab/Output/Error");
            if (XmlNode != null)
            {
                SetErrorType((CCia.EErrorType)Enum.Parse(typeof(CCia.EErrorType), XmlNode.InnerText));
            }
            XmlNode = XmlDocRoot.SelectSingleNode("//DefaultParameters/IpaTab/IpaDBFilename");
            if (XmlNode != null)
            {
                IpaDBFilename = XmlNode.InnerText.Replace("\u0020", " ");
            }
            XmlNode = XmlDocRoot.SelectSingleNode("//DefaultParameters/IpaTab/MassTol");
            if (XmlNode != null)
            {
                Ipa.m_ppm_tol = double.Parse(XmlNode.InnerText);
            }
            XmlNode = XmlDocRoot.SelectSingleNode("//DefaultParameters/IpaTab/MajorPeaksMinSToN");
            if (XmlNode != null)
            {
                Ipa.m_min_major_sn = double.Parse(XmlNode.InnerText);
            }
            XmlNode = XmlDocRoot.SelectSingleNode("//DefaultParameters/IpaTab/MinorPeaksMinSToN");
            if (XmlNode != null)
            {
                Ipa.m_min_minor_sn = double.Parse(XmlNode.InnerText);
            }
            XmlNode = XmlDocRoot.SelectSingleNode("//DefaultParameters/IpaTab/MatchedPeakReport");
            if (XmlNode != null)
            {
                Ipa.m_matched_peaks_report = bool.Parse(XmlNode.InnerText);
            }
            XmlNode = XmlDocRoot.SelectSingleNode("//DefaultParameters/IpaTab/MinPeakProbabilityToScore");
            if (XmlNode != null)
            {
                Ipa.m_min_p_to_score = double.Parse(XmlNode.InnerText);
            }
            XmlNode = XmlDocRoot.SelectSingleNode("//DefaultParameters/IpaTab/DbFilter");
            if (XmlNode != null)
            {
                Ipa.m_IPDB_ec_filter = XmlNode.InnerText;
            }
        }
        //Filter 1: check number of elements possible within mass range
        //C>=1 AND H>=1 AND IIF(Mass<=500, C+C13<39 AND H<72 AND O<20 AND N< 20 AND S<10 AND P<9, IIF(Mass>1000,C+C13<156 AND H<180 AND O<63 AND N<32 AND S<14 AND P<9, C+C13<78 AND H<126 AND O<27 AND N<25 AND S<14 AND P<9))
        /*function [good goodformulas] = Check7GR_KL2(formulas, mass)
            [GOOD, GOODFORMULAS] = Check7GR(FORMULAS, MASS) where GOOD is 1 if formula adheres to rules and 0 if not;
            FORMULAS is a list of formulas that have been proposed by the relationship algorithm (usually just one) and MASS is the neutral mass (corrected from observed)

            this function should check any formulas assigned by the relationships with the 7-golden rules (BMC Bioinformatics 2007 8:105)

            written August 2008 LK
            KL modify 9/18/08 to go back to presenting good and goodformula
            KL 1/8/09 - adding a check to make sure all the elements are positive avoid having negative number of elements
        */
        private bool CheckFormulaByFilters(short[] Formula, double Mass)
        {
            if (Formula == null) { return false; }
            if (!UseFormulaFilters) { return true; }
            //Golden rule 1 "Elemental counts" within mass range
            if (GoldenRuleFilters[0])
            {
                short[] LowLimitFormula = { 1, 1, 0, 0, 0, 0, 0, 0 };
                short[] UpLimit500Formula = { 39, 72, 20, 20, 0, 10, 9, 0 };
                short[] UpLimit500_1000Formula = { 78, 126, 27, 25, 0, 14, 9, 0 };
                short[] UpLimit_1000Formula = { 156, 180, 63, 32, 0, 14, 9, 0 };
                if (Formula[(int)EElemIndex.C] < LowLimitFormula[(int)EElemIndex.C] || Formula[(int)EElemIndex.H] < LowLimitFormula[(int)EElemIndex.H])
                {
                    return false;
                }
                short[] UpLimitFormula;
                if (Mass <= 500)
                {
                    UpLimitFormula = UpLimit500Formula;
                }
                else if (Mass > 500 && Mass <= 1000)
                {
                    UpLimitFormula = UpLimit500_1000Formula;
                }
                else
                {
                    UpLimitFormula = UpLimit_1000Formula;
                }
                var result = (Formula[(int)EElemIndex.C] + Formula[(int)EElemIndex.C13] < UpLimitFormula[(int)EElemIndex.C])
                             & (Formula[(int)EElemIndex.H] < UpLimitFormula[(int)EElemIndex.H])
                             & (Formula[(int)EElemIndex.O] < UpLimitFormula[(int)EElemIndex.O])
                             & (Formula[(int)EElemIndex.N] < UpLimitFormula[(int)EElemIndex.N])
                             & (Formula[(int)EElemIndex.S] < UpLimitFormula[(int)EElemIndex.S])
                             & (Formula[(int)EElemIndex.P] < UpLimitFormula[(int)EElemIndex.P]);

                if (!result) { return false; }
            }

            //Golden rule 2 "Valence rule"
            //odd elements with odd valency must have event total valency sum
            //check only H (valency 1) and N (valency 3) ???
            //Filter 2. Valence rules
            //H+N%2=0 AND C*4+H*1+O*2+N*3+C13*4+S*2+P*3+Na*1
            //public enum EElemNumber { C, H, O, N, C13, S, P, Na};
            //short [] ElemValences = { 4, 1, 2, 3, 4, 2, 3, 1 };
            if (GoldenRuleFilters[1])
            {
                if (((Formula[(int)EElemIndex.H] + Formula[(int)EElemIndex.N] + Formula[(int)EElemIndex.P]) % 2) != 0)
                {
                    return false;
                }
                //sum of valences greater than or equal to twice the maximum valence of one element
                var FormulaValences = 0;
                for (var Element = 0; Element < ElementCount; Element++)
                {
                    FormulaValences = FormulaValences + Formula[Element] * ElemValences[Element];
                }
                for (var Element = 0; Element < ElementCount; Element++)
                {
                    if (Formula[Element] > 0)
                    {
                        if (ElemValences[Element] * 2 > FormulaValences)
                        {
                            return false;
                        }
                    }
                }
                //sum of valences greater than or equal to 2 * atom # - 1 (incorrect. Nikola)
                //sum of valences greater than or equal to 2 * (atom # - 1)
                var TotalAtoms = 0;
                for (var Element = 0; Element < ElementCount; Element++)
                {
                    TotalAtoms = TotalAtoms + Formula[Element];
                }
                //if( FormulaValences < 2 * TotalAtoms - 1 ) {
                if (FormulaValences < 2 * (TotalAtoms - 1))
                {
                    return false;
                }
            }
            //Godlen rule 3 "Elemental ratios"
            if (GoldenRuleFilters[2])
            {
                double TotalC = Formula[(int)EElemIndex.C] + Formula[(int)EElemIndex.C13];
                var HC = 1.0 * Formula[(int)EElemIndex.H] / TotalC;
                var NC = 1.0 * Formula[(int)EElemIndex.N] / TotalC;
                var OC = 1.0 * Formula[(int)EElemIndex.O] / TotalC;
                var PC = 1.0 * Formula[(int)EElemIndex.P] / TotalC;
                var SC = 1.0 * Formula[(int)EElemIndex.S] / TotalC;
                var GoldenRule3 = (HC >= 0.2) & (HC <= 3.1) & (NC >= 0) & (NC <= 1.3) & (OC >= 0) & (OC <= 1.2) & (PC >= 0) & (PC <= 0.3) & (SC >= 0) & (SC <= 0.8);
                if (!GoldenRule3)
                {
                    return false;
                }
            }
            //Golden rule 4 "Heteroatom counts"
            if (GoldenRuleFilters[4])
            {
                //manuscript has "<" instead of "<=" for ***Min calculation
                var checkNOPSMin = (Formula[(int)EElemIndex.O] > 1) & (Formula[(int)EElemIndex.N] > 1) & (Formula[(int)EElemIndex.S] > 1) & (Formula[(int)EElemIndex.P] > 1);
                var checkNOPSMax = (Formula[(int)EElemIndex.O] < 20) & (Formula[(int)EElemIndex.N] < 10) & (Formula[(int)EElemIndex.S] < 3) & (Formula[(int)EElemIndex.P] < 4);
                if (checkNOPSMin && !checkNOPSMax)
                {
                    return false;
                }

                //bool checkNOPMin = ( Formula [ ( int ) EElemNumber.O ] > 3 ) & ( Formula [ ( int ) EElemNumber.N ] > 3 ) & ( Formula [ ( int ) EElemNumber.S ] == 0 ) & ( Formula [ ( int ) EElemNumber.P ] >= 3 );
                //manuscript doesn't use "S == 0"
                var checkNOPMin = (Formula[(int)EElemIndex.O] > 3) & (Formula[(int)EElemIndex.N] > 3) & (Formula[(int)EElemIndex.P] > 3);
                var checkNOPMax = (Formula[(int)EElemIndex.O] < 22) & (Formula[(int)EElemIndex.N] < 11) & (Formula[(int)EElemIndex.P] < 6);
                if (checkNOPMin && !checkNOPMax)
                {
                    return false;
                }

                //bool checkOPSMin = ( Formula [ ( int ) EElemNumber.O ] >= 1 ) & ( Formula [ ( int ) EElemNumber.N ] == 0 ) & ( Formula [ ( int ) EElemNumber.S ] >= 1 ) & ( Formula [ ( int ) EElemNumber.P ] >= 1 );
                //manuscript doesn't use "N == 0"
                var checkOPSMin = (Formula[(int)EElemIndex.O] > 1) & (Formula[(int)EElemIndex.S] > 1) & (Formula[(int)EElemIndex.P] > 1);
                var checkOPSMax = (Formula[(int)EElemIndex.O] < 14) & (Formula[(int)EElemIndex.S] < 3) & (Formula[(int)EElemIndex.P] < 3);
                if (checkOPSMin && !checkOPSMax)
                {
                    return false;
                }

                //bool checkNPSMin = ( Formula [ ( int ) EElemNumber.O ] == 0 ) & ( Formula [ ( int ) EElemNumber.N ] >= 1 ) & ( Formula [ ( int ) EElemNumber.S ] >= 1 ) & ( Formula [ ( int ) EElemNumber.P ] >= 1 );
                //manuscript doesn't use "O == 0"
                var checkNPSMin = (Formula[(int)EElemIndex.N] > 1) & (Formula[(int)EElemIndex.S] > 1) & (Formula[(int)EElemIndex.P] > 1);
                var checkNPSMax = (Formula[(int)EElemIndex.N] < 4) & (Formula[(int)EElemIndex.S] < 3) & (Formula[(int)EElemIndex.P] < 3);
                if (checkNPSMin && !checkNPSMax)
                {
                    return false;
                }

                //bool checkNOSMin = ( Formula [ ( int ) EElemNumber.O ] >= 6 ) & ( Formula [ ( int ) EElemNumber.N ] >= 6 ) & ( Formula [ ( int ) EElemNumber.S ] >= 6 ) & ( Formula [ ( int ) EElemNumber.P ] == 0 );
                //manuscript doesn't use "P == 0"
                var checkNOSMin = (Formula[(int)EElemIndex.O] > 6) & (Formula[(int)EElemIndex.N] > 6) & (Formula[(int)EElemIndex.S] > 6);
                var checkNOSMax = (Formula[(int)EElemIndex.O] < 14) & (Formula[(int)EElemIndex.N] < 19) & (Formula[(int)EElemIndex.S] < 8);
                if (checkNOSMin && !checkNOSMax)
                {
                    return false;
                }
            }
            //Golden rule 5 "Positive atoms"
            //KL 1/8/09 - checking to make sure everything is a positive number (i.e. can't have a negative number of elements in a formula)
            if (GoldenRuleFilters[4])
            {
                foreach (var Element in Formula)
                {
                    if (Element < 0)
                    {
                        return false;
                    }
                }
            }
            //Golden rule 6 "Integer DBE"
            if (GoldenRuleFilters[5])
            {
                //int DBECount = ( Formula [ ( int ) EElemNumber.C ] * 2 + Formula [ ( int ) EElemNumber.N ] + Formula [ ( int ) EElemNumber.P ] - Formula [ ( int ) EElemNumber.H ] + 2 ) % 2;
                var DBEResudence = (Formula[(int)EElemIndex.N] + Formula[(int)EElemIndex.P] - Formula[(int)EElemIndex.H]) % 2;
                if (DBEResudence != 0) { return false; }
            }

            //Special filter
            if (oESpecialFilter != ESpecialFilters.None)
            {
                DataTableSpecialFilter.Rows[0]["Mass"] = Mass;
                for (var Element = 0; Element < ElementCount; Element++)
                {
                    DataTableSpecialFilter.Rows[0][Enum.GetName(typeof(EElemIndex), Element)] = Formula[Element];
                }
                if (!(bool)DataTableSpecialFilter.Rows[0]["SpecialFilter"])
                {
                    return false;
                }
            }
            //User-defined filters
            if (UserDefinedFilterTable != null)
            {
                UserDefinedFilterTable.Rows[0]["Mass"] = Mass;
                for (var Element = 0; Element < ElementCount; Element++)
                {
                    UserDefinedFilterTable.Rows[0][Enum.GetName(typeof(EElemIndex), Element)] = Formula[Element];
                }
                if (!(bool)UserDefinedFilterTable.Rows[0]["UserDefinedFilter"])
                {
                    return false;
                }
            }
            return true;
        }

        private void ProcessC13(double[] NeutralMasses, short[][] Formulas, double[] PPMErrors)
        {
            if (!UseC13) { return; }
            var CDiff = CElements.C13 - CElements.C;
            for (var Peak = 0; Peak < NeutralMasses.Length - 1; Peak++)
            {
                var PeakFormula = Formulas[Peak];
                if (!IsFormula(PeakFormula))
                {
                    continue;
                }
                if ((PeakFormula[(int)EElemIndex.C13] > 0) || (PeakFormula[(int)EElemIndex.C] <= 0))
                {
                    continue;
                }
                var PeakMass = NeutralMasses[Peak];
                var C13PeakMass = PeakMass + CDiff;
                var CurC13Tolerance = GetRealC13Tolerance(PeakMass);
                var MinPeakMass = C13PeakMass - CPpmError.PpmToError(C13PeakMass, CurC13Tolerance);
                var MaxPeakMass = C13PeakMass + CPpmError.PpmToError(C13PeakMass, CurC13Tolerance);
                for (var C13Peak = Peak + 1; C13Peak < NeutralMasses.Length; C13Peak++)
                {
                    if (NeutralMasses[C13Peak] < MinPeakMass)
                    {
                        continue;
                    }
                    if (NeutralMasses[C13Peak] > MaxPeakMass)
                    {
                        break;
                    }
                    if (!IsFormula(Formulas[C13Peak]))
                    {
                        Formulas[C13Peak] = (short[])PeakFormula.Clone();
                        Formulas[C13Peak][(int)EElemIndex.C13] = (short)(Formulas[C13Peak][(int)EElemIndex.C13] + 1);
                        Formulas[C13Peak][(int)EElemIndex.C] = (short)(Formulas[C13Peak][(int)EElemIndex.C] - 1);
                        PPMErrors[C13Peak] = NeutralMasses[C13Peak] - FormulaToNeutralMass(Formulas[C13Peak]);
                    }
                    else
                    {
                        var Formula = (short[])PeakFormula.Clone();
                        Formula[(int)EElemIndex.C13] = (short)(Formula[(int)EElemIndex.C13] + 1);
                        Formula[(int)EElemIndex.C] = (short)(Formula[(int)EElemIndex.C] - 1);
                        var PPMError = NeutralMasses[C13Peak] - FormulaToNeutralMass(Formula);
                        if (Math.Abs(PPMError) < Math.Abs(PPMErrors[C13Peak]))
                        {
                            Formulas[C13Peak] = Formula;
                            PPMErrors[C13Peak] = PPMError;
                        }
                    }
                }
            }
        }

        private int FindC13ParentPeak(short[][] Formulas, int C13Peak)
        {
            var ParentFormula = (short[])Formulas[C13Peak].Clone();
            ParentFormula[(int)EElemIndex.C13] = (short)(ParentFormula[(int)EElemIndex.C13] - 1);
            ParentFormula[(int)EElemIndex.C] = (short)(ParentFormula[(int)EElemIndex.C] + 1);
            for (var ParentPeak = C13Peak - 1; ParentPeak >= 0; ParentPeak--)
            {
                if (AreFormulasEqual(Formulas[C13Peak], ParentFormula))
                {
                    return ParentPeak;
                }
            }
            return -1;
        }
        //*******************************************************************
        //DB
        //*******************************************************************
        //List<string> DBFilenames = new List<string>();
        private string CiaDBFilename = "";
        public string GetCiaDBFilename() { return CiaDBFilename; }
        public void SetCiaDBFilename(string CiaDBFilename) { this.CiaDBFilename = CiaDBFilename; }
        public double[] DBMasses = null;
        public double GetDBMass(int Index) { return DBMasses[Index]; }
        public short[][] DBFormulas = null;
        public short[] GetDBFormula(int Index) { return DBFormulas[Index]; }
        public string GetDBFormulaName(int Index) { return FormulaToName(DBFormulas[Index]); }
        private static readonly int DBBytesPerRecord = sizeof(double) + ElementCount * sizeof(short);
        private static readonly int DBRecordPerBlock = 1000;
        private readonly int DBBlockBytes = DBRecordPerBlock * DBBytesPerRecord;
        [StructLayout(LayoutKind.Explicit)]
        public struct DoubleAndBytes
        {
            [FieldOffset(0)]
            public double Double;
            [FieldOffset(0)]
            public byte Byte0;
            [FieldOffset(1)]
            public byte Byte1;
            [FieldOffset(2)]
            public byte Byte2;
            [FieldOffset(3)]
            public byte Byte3;
            [FieldOffset(4)]
            public byte Byte4;
            [FieldOffset(5)]
            public byte Byte5;
            [FieldOffset(6)]
            public byte Byte6;
            [FieldOffset(7)]
            public byte Byte7;
            [FieldOffset(8)]
            public byte Byte8;
        }

        private DoubleAndBytes oDoubleLongUnion = new DoubleAndBytes();

        private double BytesToDouble(byte[] Bytes, long StartIndex)
        {
            oDoubleLongUnion.Byte0 = Bytes[StartIndex];
            oDoubleLongUnion.Byte1 = Bytes[StartIndex + 1];
            oDoubleLongUnion.Byte2 = Bytes[StartIndex + 2];
            oDoubleLongUnion.Byte3 = Bytes[StartIndex + 3];
            oDoubleLongUnion.Byte4 = Bytes[StartIndex + 4];
            oDoubleLongUnion.Byte5 = Bytes[StartIndex + 5];
            oDoubleLongUnion.Byte6 = Bytes[StartIndex + 6];
            oDoubleLongUnion.Byte7 = Bytes[StartIndex + 7];
            return oDoubleLongUnion.Double;
        }

        private void DoubleToBytes(double dd, byte[] Bytes, long StartIndex)
        {
            oDoubleLongUnion.Double = dd;
            Bytes[StartIndex] = oDoubleLongUnion.Byte0;
            Bytes[StartIndex + 1] = oDoubleLongUnion.Byte1;
            Bytes[StartIndex + 2] = oDoubleLongUnion.Byte2;
            Bytes[StartIndex + 3] = oDoubleLongUnion.Byte3;
            Bytes[StartIndex + 4] = oDoubleLongUnion.Byte4;
            Bytes[StartIndex + 5] = oDoubleLongUnion.Byte5;
            Bytes[StartIndex + 6] = oDoubleLongUnion.Byte6;
            Bytes[StartIndex + 7] = oDoubleLongUnion.Byte7;
        }

        private void ShortToBytes(short ss, byte[] Bytes, long StartIndex)
        {
            Bytes[StartIndex] = (byte)ss;
            Bytes[StartIndex + 1] = (byte)(ss >> 8);
        }

        private short BytesToShort(byte[] Bytes, long StartIndex)
        {
            return (short)((Bytes[StartIndex + 1] << 8) + Bytes[StartIndex]);
        }

        private short[] FormulaCovertFromBinary(byte[] TempBytes, int ArrayPointer)
        {
            var Formula = new short[ElementCount];
            for (var Element = 0; Element < ElementCount; Element++)
            {
                Formula[Element] = BytesToShort(TempBytes, ArrayPointer);
                ArrayPointer = ArrayPointer + sizeof(short);
            }
            return Formula;
        }
        public int GetDBRecords()
        {
            if (DBMasses == null) { return 0; }
            return DBMasses.Length;
        }
        public double GetDBMinMass()
        {
            if (DBMasses == null) { return 0; }
            return DBMasses[0];
        }
        public double GetDBMaxMass()
        {
            if (DBMasses == null) { return 0; }
            return DBMasses[DBMasses.Length - 1];
        }

        private double DBMinError;
        private double DBMaxError;
        public double GetDBMinError() { return DBMinError; }
        public double GetDBMaxError() { return DBMaxError; }
        public bool GetDBLimitIndexes(double Mass, out int LowerIndex, out int UpperIndex)
        {
            //double FormulaError = Mass * FormulaErrorPPM / PPM;
            var LowerMZ = Mass - CPpmError.PpmToError(Mass, FormulaPPMTolerance);
            //double LowerMZ = Mass - CPpmError.PpmToError( Mass, GetRealFormulaPpmError( Mass) );
            LowerIndex = Array.BinarySearch(DBMasses, LowerMZ);
            UpperIndex = -1;//can't return without assignment
            if (LowerIndex < 0)
            {
                LowerIndex = ~LowerIndex;
            }
            else
            {
                LowerIndex = LowerIndex + 1;//must be more
            }
            if (LowerIndex >= DBMasses.Length)
            {
                return false;
            }
            var UpperMZ = Mass + CPpmError.PpmToError(Mass, FormulaPPMTolerance);
            //double UpperMZ = Mass + CPpmError.PpmToError( Mass, GetRealFormulaPpmError( Mass ) );
            UpperIndex = Array.BinarySearch(DBMasses, UpperMZ);
            if (UpperIndex < 0)
            {
                UpperIndex = ~UpperIndex;
            }
            UpperIndex = UpperIndex - 1;//must be less

            if (UpperIndex >= DBMasses.Length)
            {
                UpperIndex = DBMasses.Length - 1;
            }
            if (LowerIndex > UpperIndex)
            {
                return false;
            }
            return true;
        }
        public void LoadCiaDB(string CiaDBFilename)
        {
            if ((Path.GetFileName(CiaDBFilename) != Path.GetFileName(this.CiaDBFilename))
                    || (GetDBRecords() == 0))
            {
                if (File.Exists(CiaDBFilename))
                {
                    SetCiaDBFilename(CiaDBFilename);
                    LoadCiaDB();
                }
            }
        }
        public bool LoadCiaDB()
        {
            if (CiaDBFilename.Length == 0) { return false; }
            if (!File.Exists(CiaDBFilename)) { return false; }

            if (DBMasses != null) { DBMasses = null; }
            if (DBFormulas != null)
            {
                for (var Formula = 0; Formula < DBFormulas.Length; Formula++)
                {
                    if (DBFormulas[Formula] != null)
                    {
                        DBFormulas[Formula] = null;
                    }
                }
                DBFormulas = null;
            }
            GC.Collect();
            Console.WriteLine("Reading database {0}", CiaDBFilename);
            var RecordCount = (new System.IO.FileInfo(CiaDBFilename)).Length / DBBytesPerRecord;
            DBMasses = new double[RecordCount];
            DBFormulas = new short[RecordCount][];
            var NextRecord = 0;
            var TempBytes = new byte[DBBlockBytes];
            var oBinaryReader = new BinaryReader(File.Open(CiaDBFilename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
            int RealBytes;
            do
            {
                RealBytes = oBinaryReader.Read(TempBytes, 0, DBBlockBytes);
                var RealRecords = RealBytes / DBBytesPerRecord;
                for (var RecordIndex = 0; RecordIndex < RealRecords; RecordIndex++)
                {
                    var ArrayShift = RecordIndex * DBBytesPerRecord;
                    DBMasses[NextRecord] = BytesToDouble(TempBytes, ArrayShift);
                    ArrayShift = ArrayShift + sizeof(double);
                    DBFormulas[NextRecord] = FormulaCovertFromBinary(TempBytes, ArrayShift);
                    NextRecord = NextRecord + 1;
                }
            } while (RealBytes == DBBlockBytes);
            oBinaryReader.Close();

            // Sort, but do not check for duplicates
            // DBSortAndClean( ref DBMasses, ref DBFormulas );
            Array.Sort(DBMasses, DBFormulas);

            // Determine the minimum and maximum errors
            DBMassError(DBMasses, DBFormulas, ref DBMinError, ref DBMaxError);
            return true;
        }

        private void DBSortAndClean(ref double[] Masses, ref short[][] Formulas)
        {
            Array.Sort(Masses, Formulas);
            var RemovedFormulas = 0;
            var MaxRecords = Masses.Length;
            for (var Record = 0; Record < MaxRecords - 1; Record++)
            {
                var Mass = Masses[Record];
                if (Mass < 0) { continue; }
                var MassPlusPpmError = Support.CPpmError.MassPlusPpmError(Mass, FormulaPPMTolerance);
                for (var TempRecord = Record + 1; TempRecord < MaxRecords; TempRecord++)
                {
                    if (Masses[TempRecord] < 0)
                    {
                        continue;
                    }
                    if (Masses[TempRecord] > MassPlusPpmError)
                    {
                        break;
                    }
                    if (AreFormulasEqual(Formulas[Record], Formulas[TempRecord]))
                    {
                        Masses[TempRecord] = -1;
                        RemovedFormulas = RemovedFormulas + 1;
                    }
                }
            }
            if (RemovedFormulas == 0) { return; }
            var RealRecords = MaxRecords - RemovedFormulas;
            var TempDBMasses = new double[RealRecords];
            var TempDBFormulas = new short[RealRecords][];
            var RealRecord = 0;
            for (var Record = 0; Record < MaxRecords; Record++)
            {
                if (Masses[Record] > 0)
                {
                    TempDBMasses[RealRecord] = Masses[Record];
                    TempDBFormulas[RealRecord] = Formulas[Record];
                    RealRecord = RealRecord + 1;
                }
            }
            Masses = TempDBMasses;
            TempDBMasses = null;
            Formulas = TempDBFormulas;
            TempDBFormulas = null;
            GC.Collect();
        }

        private void DBMassError(double[] Masses, short[][] Formulas, ref double MinError, ref double MaxError)
        {
            MinError = 0;
            MaxError = 0;
            for (var Record = 0; Record < Masses.Length; Record++)
            {
                var DBError = Masses[Record] - FormulaToNeutralMass(Formulas[Record]);
                if (MinError > DBError) { MinError = DBError; }
                else if (MaxError < DBError) { MaxError = DBError; }
            }
        }
        public char[] WordSeparators = { '\t', ',', ' ' };
        private char[] LineSeparators = { '\r', '\n' };
        private bool DBCalculateMassFromFormula = true;
        public bool GetDBCalculateMassFromFormula() { return DBCalculateMassFromFormula; }
        public void SetDBCalculateMassFromFormula(bool DBCalculateMassFromFormula) { this.DBCalculateMassFromFormula = DBCalculateMassFromFormula; }
        private bool DBSort = true;
        public bool GetDBSort() { return DBSort; }
        public void SetDBSort(bool DBSort) { this.DBSort = DBSort; }
        private bool DBMassRangePerCsvFile = false;
        public bool GetDBMassRangePerCsvFile() { return DBMassRangePerCsvFile; }
        public void SetDBMassRangePerCsvFile(bool DBMassRangePerCsvFile) { this.DBMassRangePerCsvFile = DBMassRangePerCsvFile; }
        private double DBMassRange = 100;
        public double GetDBMassRange() { return DBMassRange; }
        public void SetDBMassRange(double DBMassRange) { this.DBMassRange = DBMassRange; }

        private void ReadDBAsciiFile(string Filename, out double[] Masses, out short[][] Formulas)
        {
            //file types: csv, txt, xls, xlsx
            //first line/row could have headers
            //line formats:
            //1. column 1 = index as integer; column 2 = mass as double; column 3-10 = 8 elements as short
            //2. column 1 = mass as double; column 2 = formula like C1H1O8P1 or CH1O8P
            //also checks last empty line
            //read file of text and csv files
            if (!File.Exists(Filename)) { throw new Exception("File is not exist. " + Filename); }
            if (new FileInfo(Filename).Length == 0) { throw new Exception("File is empty. " + Filename); }

            var FileExtension = Path.GetExtension(Filename);
            var ListMasses = new List<double>();
            var ListFormulas = new List<short[]>();
            var FirstLine = true;
            if (FileExtension == ".csv" || FileExtension == ".txt")
            {
                var oStreamReader = new StreamReader(Filename);
                while (oStreamReader.Peek() >= 0)
                {
                    var Line = oStreamReader.ReadLine();
                    if (Line.Length == 0) { break; }
                    var Words = Line.Split(WordSeparators, StringSplitOptions.RemoveEmptyEntries);
                    double Mass;
                    if (FirstLine)
                    {
                        FirstLine = false;
                        try
                        {
                            Mass = double.Parse(Words[0]);
                        }
                        catch
                        {
                            continue;
                        }
                    }
                    if (Words.Length == 2)
                    {
                        ListMasses.Add(double.Parse(Words[0]));
                        ListFormulas.Add(NameToFormula(Words[1]));
                    }
                    else if (Words.Length == 10)
                    {
                        ListMasses.Add(double.Parse(Words[1]));
                        var Formula = new short[ElementCount];
                        for (var Element = 0; Element < ElementCount; Element++)
                        {
                            Formula[Element] = Int16.Parse(Words[Element + 2]);
                        }
                        ListFormulas.Add(Formula);
                    }
                    else
                    {
                        throw new Exception("File format is wrong. " + Filename);
                    }
                }
                oStreamReader.Close();
            } /*else if( FileExtension == ".xlsx" || FileExtension == ".xls" ) {
                Microsoft.Office.Interop.Excel.Application MyApp = new Microsoft.Office.Interop.Excel.Application();
                MyApp.Visible = false;
                Microsoft.Office.Interop.Excel.Workbook MyBook = MyApp.Workbooks.Open( Filename );
                Microsoft.Office.Interop.Excel.Worksheet MySheet = MyBook.Sheets [ 1 ];
                Microsoft.Office.Interop.Excel.Range MyRange = MySheet.UsedRange;
                int FormulaCount = MyRange.Rows.Count;
                int Columns = MyRange.Columns.Count;
                object RangeArray = MyRange.Value;
                string ExceptionMesssage = string.Empty;
                for( int FormulaIndex = 0; FormulaIndex < FormulaCount; FormulaIndex++ ) {
                    double Mass;
                    if( FirstLine == true ) {
                        FirstLine = false;
                        try {
                            Mass = double.Parse( ( string ) ( ( Array ) RangeArray ).GetValue( FormulaIndex + 1, 2 ) );
                        } catch {
                            continue;
                        }
                    }
                    if( Columns == 10 ) {
                        ListMasses.Add( ( double ) ( ( Array ) RangeArray ).GetValue( FormulaIndex + 1, 2 ) );
                        short [] Formula = new short [ ElementCount ];
                        for( int Element = 0; Element < ElementCount; Element++ ) {
                            Formula [ Element ] = ( short ) ( ( Array ) RangeArray ).GetValue( FormulaIndex + 1, Element + 2 );
                        }
                        ListFormulas.Add( Formula );
                    } else if( Columns >= 2 ) {
                        short [] Formula = NameToFormula( ( string ) ( ( Array ) RangeArray ).GetValue( FormulaIndex + 1, 2 ) );
                        ListFormulas.Add( NameToFormula( ( string ) ( ( Array ) RangeArray ).GetValue( FormulaIndex + 1, 2 ) ) );
                        //Masses.Add( FormulaToMass( Formula) );//???
                        ListMasses.Add( ( double ) ( ( Array ) RangeArray ).GetValue( FormulaIndex + 1, 1 ) );
                    } else {
                        ExceptionMesssage = "File format is wrong. " + Filename;
                        break;
                    }
                }
                CleanComObject( MyRange );
                MyRange = null;
                CleanComObject( MySheet );
                MySheet = null;
                MyBook.Close( null, null, null );
                CleanComObject( MyBook );
                MyBook = null;
                MyApp.Quit();
                CleanComObject( MyApp );
                MyApp = null;
                GC.Collect();
                if( ExceptionMesssage.Length > 0 ) {
                    throw new Exception( ExceptionMesssage );
                }
            } */
            else
            {
                throw new Exception("File type is not supported. " + Filename);
            }
            Masses = ListMasses.ToArray();
            Formulas = ListFormulas.ToArray();
        }
        public void DBConvertAsciiToBin(string DBAsciiFilename)
        {
            double[] Masses;
            short[][] Formulas;
            ReadDBAsciiFile(DBAsciiFilename, out Masses, out Formulas);
            if (DBCalculateMassFromFormula)
            {
                for (var Record = 0; Record < Masses.Length; Record++)
                {
                    Masses[Record] = FormulaToNeutralMass(Formulas[Record]);
                }
            }
            if (DBSort)
            {
                DBSortAndClean(ref Masses, ref Formulas);
            }
            double MinError = 0;
            double MaxError = 0;
            if (!DBCalculateMassFromFormula)
            {
                DBMassError(Masses, Formulas, ref MinError, ref MaxError);
            }
            //write
            var DBBinaryFilename = Path.GetFullPath(DBAsciiFilename).Substring(0, DBAsciiFilename.Length - Path.GetExtension(DBAsciiFilename).Length) + ".bin";
            var oBinaryWriter = new BinaryWriter(File.Open(DBBinaryFilename, FileMode.Create));
            for (var Record = 0; Record < Masses.Length; Record++)
            {
                oBinaryWriter.Write(Masses[Record]);
                for (var Element = 0; Element < ElementCount; Element++)
                {
                    oBinaryWriter.Write(Formulas[Record][Element]);
                }
            }
            oBinaryWriter.Close();
        }
        public void DBConvertAsciisToBin(string[] AsciiFilenames)
        {
            var ListMasses = new List<double>();
            var ListFormulas = new List<short[]>();
            foreach (var Filename in AsciiFilenames)
            {
                double[] TempMasses;
                short[][] TempFormulas;
                ReadDBAsciiFile(Filename, out TempMasses, out TempFormulas);
                ListMasses.AddRange(TempMasses);
                ListFormulas.AddRange(TempFormulas.ToList<short[]>());
            }
            var Masses = ListMasses.ToArray();
            var Formulas = ListFormulas.ToArray();
            if (DBCalculateMassFromFormula)
            {
                for (var Record = 0; Record < Masses.Length; Record++)
                {
                    Masses[Record] = FormulaToNeutralMass(Formulas[Record]);
                }
            }
            if (DBSort)
            {
                DBSortAndClean(ref Masses, ref Formulas);
            }
            double MinError = 0;
            double MaxError = 0;
            if (!DBCalculateMassFromFormula)
            {
                DBMassError(Masses, Formulas, ref MinError, ref MaxError);
            }

            //write
            var DBBinaryFilename = Path.GetFullPath(AsciiFilenames[0]).Substring(0, AsciiFilenames[0].Length - Path.GetExtension(AsciiFilenames[0]).Length) + ".bin";
            var oBinaryWriter = new BinaryWriter(File.Open(DBBinaryFilename, FileMode.Create));
            for (var Record = 0; Record < Masses.Length; Record++)
            {
                oBinaryWriter.Write(Masses[Record]);
                for (var Element = 0; Element < ElementCount; Element++)
                {
                    oBinaryWriter.Write(Formulas[Record][Element]);
                }
            }
            oBinaryWriter.Close();
        }
        public void DBConvertBinToCsv(string DBBinaryFile)
        {
            var Formulas = new FileInfo(DBBinaryFile).Length / DBBytesPerRecord;
            var oBinaryReader = new BinaryReader(File.Open(DBBinaryFile, FileMode.Open, FileAccess.Read));
            StreamWriter oStreamWriter = null;
            if (!DBMassRangePerCsvFile)
            {
                oStreamWriter = new StreamWriter(DBBinaryFile.Substring(0, DBBinaryFile.Length - 4) + ".csv");
            }
            var RangeIndex = -1;
            for (var Formula = 0; Formula < Formulas; Formula++)
            {
                var Mass = oBinaryReader.ReadDouble();
                var Line = (Formula + 1).ToString() + ',' + Mass.ToString();
                for (var Element = 0; Element < ElementCount; Element++)
                {
                    var gg = (short)oBinaryReader.ReadInt16();
                    Line = Line + ',' + gg.ToString();
                }
                if (DBMassRangePerCsvFile)
                {
                    var NewRangeIndex = Convert.ToInt32(Math.Floor(Mass / DBMassRange) * DBMassRange);
                    if (NewRangeIndex != RangeIndex)
                    {
                        RangeIndex = NewRangeIndex;
                        if (oStreamWriter != null)
                        {
                            oStreamWriter.Flush();
                            oStreamWriter.Close();
                        }
                        oStreamWriter = new StreamWriter(DBBinaryFile.Substring(0, DBBinaryFile.Length - 4) + RangeIndex + ".csv");
                    }
                }
                oStreamWriter.WriteLine(Line);
            }
            oStreamWriter.Flush();
            oStreamWriter.Close();
            oBinaryReader.Close();
        }

        private string IpaDBFilename = "";
        public string GetIpaDBFilename() { return IpaDBFilename; }
        public void SetIpaDBFilename(string IpaDBFilename) { this.IpaDBFilename = IpaDBFilename; }
        public void LoadIpaDB()
        {
            if (IpaDBFilename.Length <= 0) { return; }
            if (!File.Exists(IpaDBFilename)) { return; }
            Ipa.LoadTabulatedDB(IpaDBFilename);
        }
    }
}


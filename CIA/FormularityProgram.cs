using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Collections;

using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Data;
using System.Runtime.InteropServices;
using Microsoft.Office.Interop.Excel;
using System.Xml;
using System.Globalization;

using TestFSDBSearch;
using FileReader;

namespace CIA {
    class FormularityProgram {
        static void Main( string [] args ) {
            //CCia oCCia = new CCia( null);
            //string [] Files = Directory.GetFiles( "C:\\Nikola\\DOM_Working_Individual_Spectra_Analysis" );
            //foreach( string Filename in Files ) {
            //    if( Path.GetExtension( Filename ) != ".csv" ) { continue; }
            //    oCCia.Process( Filename );
            //}
            //oCCia.Process( Files );
        }
    }
    class Data {
        //arrays per file
        //raw
        public int FileCount;
        public string [] Filenames;
        public double [] [] Masses;
        public double [] [] Abundances;
        public double [] [] SNs;
        public double [] [] Resolutions;
        public double [] [] RelAbundances;

        //processed
        public double [] [] CalMasses;
        public double [] [] AlignMasses;
        public double [] [] NeutralMasses;
        public short [] [] [] Formulas;
        public double [] [] PPMErrors;
        public short [] [] Candidates;
    }
    class AlignData{
        public double [] AlignMasses;
        public double [] NeutralMasses;
        public int [] [] Indexes;
        public short [] [] Formulas;
        public double [] PPMErrors;
        public short [] Candidates;
    }
    public class CCia{        
        //Elements
        public const double elec = 0.00054857990945;
        public const double proton = 1.00727646688;
        public const double H = 1.0078250321;
        public const double C = 12;
        public const double C13 = 13.0033548378;
        public const double N = 14.0030740052;
        public const double N15 = 15.0001088984;
        public const double O = 15.9949146221;
        public const double Na = 22.98976967;
        public const double P = 30.97376151;
        public const double S = 31.97207069;
        public const double Cl = 34.96885271;
        public const double Br = 78.9183376;
        public const double K = 38.9637069;
        public const int Elements = 8;
        public enum EElemNumber { C = 0, H, O, N, C13, S, P, Na};
        double [] ElementMasses = new double [ Elements ] { C, H, O, N, C13, S, P, Na };
        short [] ElemValences = { 4, 1, 2, 3, 4, 2, 3, 1};

        //Formula
        short [] NullFormula = new short[ Elements];
        public double FormulaToNeutralMass( short [] Formula ) {
            if( Formula == null ) {
                throw new Exception( "Formula array is null" );
            }
            if( Formula.Length != Elements ) {
                throw new Exception( "Formula array length (" + Formula.Length + ") must be " + Elements );
            }
            double NeutralMass = 0;
            for( int Element = 0; Element < Elements; Element++ ) {
                NeutralMass = NeutralMass + Formula [ Element ] * ElementMasses [ Element ];
            }
            return NeutralMass;
        }
        public bool IsFormula( short [] Formula ) {
            foreach( short Element in Formula ) { if( Element > 0 ) { return true; } }
            return false;
        }
        bool AreFormulasEqual( short [] Formula1, short [] Formula2 ) {
            if( Formula1.Length != Formula2.Length ) { return false; }
            for( int Element = 0; Element < Formula1.Length; Element++ ) {
                if( Formula1 [ Element ] != Formula2 [ Element ] ) { return false; }
            }
            return true;
        }
        public string FormulaToName( short [] Formula ) {
            string FormulaName = string.Empty;
            for( int Element = 0; Element < Formula.Length; Element++ ) {
                if( Formula [ Element ] == 0 ) { continue; }
                FormulaName += Enum.GetName( typeof( CCia.EElemNumber ), Element );
                if( Formula [ Element ] == 1 ) { continue; }
                FormulaName += Formula [ Element ];
            }
            return FormulaName;
        }
        public short [] NameToFormula( string FormulaName) {                   
            short [] Formula = (short []) NullFormula.Clone();
            if( FormulaName.Length == 0){ return Formula;}

            for( int CurrentSymbol = 0; CurrentSymbol < FormulaName.Length; ) {
                //Element
                string ElementName = string.Empty;
                if( Char.IsLetter( FormulaName [ CurrentSymbol ] ) == false ) {
                    throw new Exception( "Formula name (" + FormulaName + "is wrong" );
                }
                if( ( FormulaName.Length > CurrentSymbol + 1) && (Char.IsLetter( FormulaName [ CurrentSymbol + 1 ] ) == true ) ) {
                    //Maybe Element name consists of 2 letters
                    ElementName = FormulaName.Substring( CurrentSymbol, 2 );
                    //check that is not like CH2
                    EElemNumber oEElemNumber;
                    if( Enum.TryParse<EElemNumber>( ElementName, out oEElemNumber ) == false ) {
                        ElementName = FormulaName.Substring( CurrentSymbol, 1 );
                    }
                } else {
                    ElementName = FormulaName.Substring( CurrentSymbol, 1 );
                }
                CurrentSymbol = CurrentSymbol + ElementName.Length;
                //Element's atoms
                bool Negative = false;
                if( FormulaName.Length > CurrentSymbol ) {
                    if( FormulaName [ CurrentSymbol ] == '-' ) {
                        Negative = true;
                        CurrentSymbol = CurrentSymbol + 1;
                    }
                }
                int DigitCount = 0;
                while( ( FormulaName.Length > CurrentSymbol + DigitCount) && (Char.IsDigit( FormulaName [ CurrentSymbol + DigitCount] ) == true ) ) {
                    DigitCount = DigitCount + 1;
                }
                short ElementNumber = 1;
                if( DigitCount > 0 ) {
                    ElementNumber = Int16.Parse( FormulaName.Substring( CurrentSymbol, DigitCount ) );
                }
                if( Negative == true ) {
                    ElementNumber = (short) -ElementNumber;
                }
                Formula [ ( int) Enum.Parse( typeof( EElemNumber ), ElementName ) ] = ElementNumber;
                CurrentSymbol = CurrentSymbol + DigitCount;
            }
            return Formula;
        }

        //error
        const double PPM = 1e6;//parts per million
        public double PpmToError( double Mass, double ErrorPPM ) { return Mass * ErrorPPM / PPM; }
        public double SignedMassErrorPPM( double ReferenceMass, double Mass ) { return ( Mass - ReferenceMass ) / ReferenceMass * PPM; }
        public double AbsMassErrorPPM( double ReferenceMass, double Mass ) { return Math.Abs( ( Mass - ReferenceMass ) / ReferenceMass * PPM ); }

        //***********
        //Input files
        //***********
        //string [] Filenames;
    
        //******************************************************************************************
        //Calibration
        //******************************************************************************************
        public TestFSDBSearch.TotalCalibration oTotalCalibration = new TotalCalibration();
        /*
        void Calibrate() {            
            for( int FileIndex = 0; FileIndex < oData.FileCount; FileIndex++ ) {
                double [] Abundances = oData.Abundances[ FileIndex];
                double MaxAbundance = Abundances[ 0];
                foreach( double Abundabce in Abundances){ if( MaxAbundance < Abundabce){  MaxAbundance = Abundabce;}}
                oTotalCalibration.cal_log.Clear();
                oData.CalMasses [ FileIndex ] = oTotalCalibration.ttl_LQ_InternalCalibration( ref oData.Masses [ FileIndex ], ref Abundances, ref oData.SNs [ FileIndex ], MaxAbundance );                
                oStreamLogWriter.WriteLine();
                oStreamLogWriter.WriteLine( "Calibration of " + Path.GetFileName( Filenames [ FileIndex]) );
                oStreamLogWriter.WriteLine();
                oStreamLogWriter.Write( oTotalCalibration.cal_log );
            }
        }
        */
        //******************************************************************************************
        //Alignment
        //******************************************************************************************
        public bool Alignment = true;
        public bool GetAlignment() { return Alignment; }
        public void SetAlignment( bool Alignment ) { this.Alignment = Alignment; }
        double AlignmentPPMTolerance = 1;//default
        public double GetAlignmentPpmTolerance() { return AlignmentPPMTolerance; }
        public void SetAlignmentPpmTolerance( double PPMTolerance ) {
            AlignmentPPMTolerance = PPMTolerance;
        }
        void AlignmentByPeak() {
            //Group near peaks as one peak.
            //Peak from the same file can't be gropped
            //New mass calculation is linear (doesn't use peak Abundance)
            int [] IndexesTemplate = new int [ oData.FileCount + 1 ];//extra last row for weight counting
            int TotalPeakCount = 0;
            for( int FileIndex = 0; FileIndex < oData.FileCount; FileIndex++ ) {
                IndexesTemplate [ FileIndex ] = -1;//Existing index can be 0 and positive
                TotalPeakCount = TotalPeakCount + oData.Masses [ FileIndex ].Length;
            }
            IndexesTemplate [ oData.FileCount ] = 1;
            double [] TotalMasses = new double [ TotalPeakCount ];
            int [] [] TotalIndexes = new int [ TotalPeakCount ] [];
            int FilePeakShift = 0;
            for( int FileIndex = 0; FileIndex < oData.FileCount; FileIndex++ ) {
                int FilePeakCount = oData.Masses [ FileIndex ].Length;
                double [] AlignMasses = oData.CalMasses[ FileIndex ];
                for( int Peak = 0; Peak < FilePeakCount; Peak++ ) {
                    TotalMasses [ FilePeakShift + Peak ] = AlignMasses[ Peak ];
                    TotalIndexes [ FilePeakShift + Peak ] = ( int [] ) IndexesTemplate.Clone();
                    TotalIndexes [ FilePeakShift + Peak ] [ FileIndex ] = Peak;
                }
                FilePeakShift = FilePeakShift + FilePeakCount;
            }
            Array.Sort( TotalMasses, TotalIndexes );
            List<double> MassesL = TotalMasses.ToList<double>();
            List<int []> IndexesL = TotalIndexes.ToList<int []>();
            TotalMasses = null;
            TotalIndexes = null;

            //remove peaks with the same mass
            for( int Peak = 0; Peak < TotalPeakCount - 1; ) {
                if( MassesL [ Peak ] == MassesL [ Peak + 1 ] ) {
                    int [] CurrentIndexes = IndexesL [ Peak ];
                    int [] NextIndexes = IndexesL [ Peak + 1 ];
                    for( int Index = 0; Index < oData.FileCount; Index++ ) {
                        if( CurrentIndexes [ Index ] >= 0 ) {
                            NextIndexes [ Index ] = CurrentIndexes [ Index ];//move current valueable peaks to next
                        }
                    }
                    NextIndexes [ oData.FileCount ] = NextIndexes [ oData.FileCount ] + CurrentIndexes [ oData.FileCount ];//for weight
                    MassesL.RemoveAt( Peak );//remove current peak
                    IndexesL.RemoveAt( Peak );
                    TotalPeakCount = TotalPeakCount - 1;
                } else {
                    Peak = Peak + 1;
                }
            }
            //align peaks
            //look on left and right peaks; group the nearest peak if error is < Alignment
            for( int Peak = 1; Peak < TotalPeakCount - 1; ) {
                double PeakError = PpmToError( MassesL [ Peak ], AlignmentPPMTolerance );//it strange but it is correct to Matlab code
                double LeftDistance = MassesL [ Peak ] - MassesL [ Peak - 1 ];
                double RightDistance = MassesL [ Peak + 1 ] - MassesL [ Peak ];
                int [] CurrentIndexes = IndexesL [ Peak ];
                //left peak
                bool IsLeftPeakUnique = false;
                if( LeftDistance >= PeakError ) { IsLeftPeakUnique = true; }
                if( IsLeftPeakUnique == false ) {
                    int [] LeftIndexes = IndexesL [ Peak - 1 ];
                    for( int Index = 0; Index < oData.FileCount; Index++ ) {
                        if( ( LeftIndexes [ Index ] >= 0 ) && ( CurrentIndexes [ Index ] >= 0 ) ) {
                            IsLeftPeakUnique = true;
                            break;
                        }
                    }
                }
                if( ( IsLeftPeakUnique == false ) && ( LeftDistance <= RightDistance ) ) {// Left + Current maybe become Left
                    int [] LeftIndexes = IndexesL [ Peak - 1 ];
                    MassesL [ Peak - 1 ] = ( MassesL [ Peak ] * CurrentIndexes [ oData.FileCount ] + MassesL [ Peak - 1 ] * LeftIndexes [ oData.FileCount ] ) / ( CurrentIndexes [ oData.FileCount ] + LeftIndexes [ oData.FileCount ] );
                    for( int Index = 0; Index < oData.FileCount; Index++ ) {
                        if( CurrentIndexes [ Index ] >= 0 ) {
                            LeftIndexes [ Index ] = CurrentIndexes [ Index ];//move current peaks to left
                        }
                    }
                    LeftIndexes [ oData.FileCount ] = LeftIndexes [ oData.FileCount ] + CurrentIndexes [ oData.FileCount ];//for weight
                    MassesL.RemoveAt( Peak );//remove current peak
                    IndexesL.RemoveAt( Peak );
                    TotalPeakCount = TotalPeakCount - 1;
                    continue;
                }

                //right peak
                if( RightDistance >= PeakError ) { Peak = Peak + 1; continue; }
                bool IsRightPeakUnique = false;
                int [] RightIndexes = IndexesL [ Peak + 1 ];
                for( int Index = 0; Index < oData.FileCount; Index++ ) {
                    if( ( RightIndexes [ Index ] >= 0 ) && ( CurrentIndexes [ Index ] >= 0 ) ) {
                        IsRightPeakUnique = true;
                        break;
                    }
                }
                if( IsRightPeakUnique == true ) {
                    Peak = Peak + 1;
                    continue;
                } else {
                    MassesL [ Peak + 1 ] = ( MassesL [ Peak ] * CurrentIndexes [ oData.FileCount ] + MassesL [ Peak + 1 ] * RightIndexes [ oData.FileCount ] ) / ( CurrentIndexes [ oData.FileCount ] + RightIndexes [ oData.FileCount ] );
                    for( int Index = 0; Index < oData.FileCount; Index++ ) {
                        if( CurrentIndexes [ Index ] >= 0 ) {
                            RightIndexes [ Index ] = CurrentIndexes [ Index ];//move current peaks to right
                        }
                    }
                    RightIndexes [ oData.FileCount ] = RightIndexes [ oData.FileCount ] + CurrentIndexes [ oData.FileCount ];//for weight
                    MassesL.RemoveAt( Peak );
                    IndexesL.RemoveAt( Peak );
                    TotalPeakCount = TotalPeakCount - 1;
                    continue;
                }
            }
            oAlignData.AlignMasses = MassesL.ToArray();
            oAlignData.NeutralMasses = new double [ oAlignData.AlignMasses.Length ];
            oAlignData.Indexes = IndexesL.ToArray();
            oAlignData.Formulas = new short [ oAlignData.AlignMasses.Length ] [];
            for( int Peak = 0; Peak < oAlignData.AlignMasses.Length; Peak++ ) {
                oAlignData.Formulas[ Peak] = (short [] ) NullFormula.Clone();
            }
            oAlignData.Candidates = new short [ oAlignData.AlignMasses.Length ];
            oAlignData.PPMErrors = new double [ oAlignData.AlignMasses.Length ];
        }
        class Line {
            public short [] Formulas;
            public int [] Indexes;
        }
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
        double FormulaPPMTolerance = 1;//default
        public double GetFormulaPPMTolerance() { return FormulaPPMTolerance; }
        public void SetFormulaPPMTolerance( double PPMTolerance ) { FormulaPPMTolerance = PPMTolerance;}

        //Relations
        bool UseRelation = true;//default
        public bool GetUseRelation() { return UseRelation; }
        public void SetUseRelation( bool UseRelation ) { this.UseRelation = UseRelation; }
        int MaxRelationGaps = 5;//default
        public int GetMaxRelationGaps() { return MaxRelationGaps; }
        public void SetMaxRelationGaps( int MaxRelationGaps ) { this.MaxRelationGaps = MaxRelationGaps; }
        public enum RelationshipErrorType { AMU, PPM, GapPPM };
        RelationshipErrorType oRelationshipErrorType = RelationshipErrorType.AMU;
        public RelationshipErrorType GetRelationshipErrorType() { return oRelationshipErrorType; }
        public void SetRelationshipErrorType( RelationshipErrorType oRelationshipErrorType ) { this.oRelationshipErrorType = oRelationshipErrorType; }
        double RelationErrorAMU = 0.00002;//default
        public double GetRelationErrorAMU() { return RelationErrorAMU; }
        public void SetRelationErrorAMU( double ErrorAMU ) { RelationErrorAMU = ErrorAMU; }
        bool UseBackward = true;
        public bool GetUseBackward() { return UseBackward; }
        public void SetUseBackward( bool UseBackward ) { this.UseBackward = UseBackward; }

        short [] [] RelationBuildingFormulaBlocks = { 
                new short [] { 1, 2, 0, 0, 0, 0, 0, 0 },//CH2
                new short [] { 1, 4, -1, 0, 0, 0, 0, 0},//CH4O- or CH4O-1
                new short [] { 0, 2, 0, 0, 0, 0, 0, 0 },//H2
                new short [] { 2, 4, 1, 0, 0, 0, 0, 0 },//C2H4O
                new short [] { 1, 0, 2, 0, 0, 0, 0, 0 },//CO2
                new short [] { 2, 2, 1, 0, 0, 0, 0, 0 },//C2H2O
                new short [] { 0, 0, 1, 0, 0, 0, 0, 0 },//O
                new short [] { 1, 1, 0, 0, 0, 0, 0, 0 },//CH
                new short [] { 0, 1, 0, 1, 0, 0, 0, 0 },//HN
                new short [] { 0, 0, 3, 0, 0, 0, 1, 0 }//O3P
        };
        public short [][] GetRelationFormulaBuildingBlocks() { return RelationBuildingFormulaBlocks; }
        List<short []> ListActiveRelationFormulaBuildingBlocks;
        public short [] [] GetActiveRelationFormulaBuildingBlocks() { return ListActiveRelationFormulaBuildingBlocks.ToArray(); }
        double [] ActiveRelationFormulaBuildingBlockMasses;
        public void SetRelationFormulaBuildingBlocks( short [][] ActiveBlocks ) {
            ListActiveRelationFormulaBuildingBlocks = new List<short []>( ActiveBlocks );
            ActiveRelationFormulaBuildingBlockMasses = new double [ ListActiveRelationFormulaBuildingBlocks.Count];
            for( int Relation = 0; Relation < ListActiveRelationFormulaBuildingBlocks.Count; Relation++ ) {
                ActiveRelationFormulaBuildingBlockMasses [ Relation ] = 0;
                for( int Element = 0; Element < Elements; Element++ ) {
                    ActiveRelationFormulaBuildingBlockMasses [ Relation ] = ActiveRelationFormulaBuildingBlockMasses [ Relation ] + ListActiveRelationFormulaBuildingBlocks [ Relation ] [ Element ] * ElementMasses [ Element ];
                }
            }
        }

        //Mass limit to search in DB
        double MassLimit = 500;
        public double GetMassLimit() { return MassLimit; }
        public void SetMassLimit( double MassLimit ) { this.MassLimit = MassLimit; }

        //formula score
        bool UseCIAFormulaScore = false;
        public bool GetUseCIAFormulaScore() { return UseCIAFormulaScore; }
        public void SetUseCIAFormulaScore( bool UseCIAFormulaScore ) { this.UseCIAFormulaScore = UseCIAFormulaScore; }

        string [] FormulaScoreNames = new string []{
                "min(S+P) & The lowest error",
                "The lowest error",
                "min(N+S+P) & The lowest error"};
        public string [] GetFormulaScoreNames() { return FormulaScoreNames; }
        public enum EFormulaScore { lowestSP = 0, lowestError = 1, HAcap = 2};
        public EFormulaScore FormulaScore = EFormulaScore.HAcap;//default
        public EFormulaScore GetFormulaScore() { return FormulaScore; }
        public void SetFormulaScore( EFormulaScore FormulaScore ) { this.FormulaScore = FormulaScore; }

        //Kendrick
        bool UseKendrick = true;
        public bool GetUseKendrick() { return UseKendrick; }
        public void SetUseKendrick( bool UseKendrick ) { this.UseKendrick = UseKendrick; }

        //C13
        bool UseC13 = true;
        public bool GetUseC13() { return UseC13; }
        public void SetUseC13( bool UseC13 ) { this.UseC13 = UseC13; }
        double C13Tolerance = 0;
        public double GetC13Tolerance() { return C13Tolerance; }
        public void SetC13Tolerance( double C13Tolerance ) { this.C13Tolerance = C13Tolerance; }

        //Golden rule filters
        public string [] GoldenRuleFilterNames = new string []{ 
                "Elemental counts",
                "Valence rules",
                "Elemental ratios",
                "Heteroatom counts",
                "Positive atoms",
                "Integer DBE"
        };
        public string [] GetGoldenRuleFilterNames() { return GoldenRuleFilterNames; }
        bool [] GoldenRuleFilters = new bool[]{ true, true, true, true, true, true};
        public bool [] GetGoldenRuleFilterUsage() { return GoldenRuleFilters; }
        public void SetGoldenRuleFilterUsage( bool [] GoldenRuleFilters ) {
            int Total = this.GoldenRuleFilters.Length;
            if( Total > GoldenRuleFilters.Length ) { Total = GoldenRuleFilters.Length; }
            for( int Filter = 0; Filter < Total; Filter++ ) {
                this.GoldenRuleFilters [ Filter ] = GoldenRuleFilters [ Filter ];
            }
        }

        //Special filters
        System.Data.DataTable DataTableSpecialFilter;
        public enum ESpecialFilters { None, Air, Water, Earth};
        ESpecialFilters oESpecialFilter = ESpecialFilters.None;
        string [] SpecialFilterRules = new string []{
                "",
                "O>0 AND N<=2 AND S<=1 AND P=0 AND 3*(S+N)<=O",
                "O>0 AND N<=3 AND S<=2 AND P<=2",
                "O>0 AND N<=3 AND P<=2 AND 3*P<=O"};//last is letter O (o) and not digit 0 (zero)
        public string [] GetSpecialFilterRules() { return SpecialFilterRules; }
        public ESpecialFilters GetSpecialFilter() { return oESpecialFilter; }
        public void SetSpecialFilter( ESpecialFilters oESpecialFilter ) {
            this.oESpecialFilter = oESpecialFilter;
            DataTableSpecialFilter = new System.Data.DataTable();
            DataTableSpecialFilter.Columns.Add( "Mass", typeof( double ) );
            foreach( string Name in Enum.GetNames( typeof( EElemNumber ) ) ) {
                DataTableSpecialFilter.Columns.Add( Name, typeof( short ) );
            }
            if( oESpecialFilter != ESpecialFilters.None ) {
                DataTableSpecialFilter.Columns.Add( "SpecialFilter", typeof( bool ), SpecialFilterRules [ ( int ) oESpecialFilter ] );
            }
            DataTableSpecialFilter.Rows.Add( DataTableSpecialFilter.NewRow() );           
        }
        //User-defined filters
        System.Data.DataTable UserDefinedFilter;
        public void SetUserDefinedFilter( string NewUserDefinedFilter ) {
            if( NewUserDefinedFilter.Length == 0 ) {
                UserDefinedFilter = null;
                return;
            }
            UserDefinedFilter = new System.Data.DataTable();
            UserDefinedFilter.Columns.Add( "Mass", typeof( double ) );
            foreach( string Name in Enum.GetNames( typeof( EElemNumber ) ) ) {
                UserDefinedFilter.Columns.Add( Name, typeof( short ) );
            }
            UserDefinedFilter.Columns.Add( "UserDefinedFilter", typeof( bool ), NewUserDefinedFilter );
            UserDefinedFilter.Rows.Add( UserDefinedFilter.NewRow() );
        }

        bool UseFormulaFilters = true;
        public bool GetUseFormulaFilter() { return UseFormulaFilters; }
        public void SetUseFormulaFilter( bool UseFormulaFilters ) { this.UseFormulaFilters = UseFormulaFilters; }

        //Output error type
        public enum EErrorType { CIA, Signed };
        EErrorType oEErrorType = EErrorType.CIA;
        public EErrorType GetErrorType() { return oEErrorType; }
        public void SetErrorType( EErrorType oEErrorType ) { this.oEErrorType = oEErrorType; }

        //Reports
        bool GenerateIndividualFileReports = false;
        public bool GetGenerateIndividualFileReports(){ return GenerateIndividualFileReports;}
        public void SetGenerateIndividualFileReports( bool GenerateIndividualFileReports ) { this.GenerateIndividualFileReports = GenerateIndividualFileReports; }

        //bool LogReportStatus = false;
        //public bool GetLogReportStatus() { return LogReportStatus; }
        //public void SetLogReportStatus( bool LogReportStatus ) { this.LogReportStatus = LogReportStatus; }
        //public StreamWriter oStreamLogWriter;

        bool GenerateChainReport = false;
        public bool GetGenerateChainReport() { return GenerateChainReport; }
        public void SetGenerateChainReport( bool GenerateChainReport ) { this.GenerateChainReport = GenerateChainReport; }

        //Output file delimiter
        public enum EDelimiters { Comma, Tab, Space};
        EDelimiters oEOutputFileDelimiter = EDelimiters.Comma;
        string OutputFileDelimiter = ",";
        public EDelimiters GetOutputFileDelimiterType() { return oEOutputFileDelimiter; }
        public string GetOutputFileDelimiter() { return OutputFileDelimiter; }
        public void SetOutputFileDelimiterType( EDelimiters Delimiter ) {
            this.oEOutputFileDelimiter = Delimiter;
            switch( this.oEOutputFileDelimiter ) {
                case EDelimiters.Comma: OutputFileDelimiter = ","; break;
                case EDelimiters.Tab: OutputFileDelimiter = "\t"; break;
                case EDelimiters.Space: OutputFileDelimiter = " "; break;
                //default: new 
            }
        }

        Data oData = new Data();
        AlignData oAlignData = new AlignData();
        public TotalIPDBSearch Ipa;
        public CCia() {
            Ipa = new TotalIPDBSearch();
            foreach( short Element in NullFormula ) { NullFormula [ Element ] = 0; }
            SetRelationFormulaBuildingBlocks( RelationBuildingFormulaBlocks );           
            //LoadDB( "C:\\Nikola\\Table\\lokufuco_db.bin" );
        }
        public void Process( string [] Filenames, double [] [] Masses, double [] [] Abundances, double [] [] SNs, double [] [] Resolutions, double [] [] RelAbundances, double [] [] CalMasses, StreamWriter oStreamLogWriter ) {
            try {
                //this.Filenames = Filenames;
                oData.Filenames = Filenames;
                oData.FileCount = oData.Filenames.Length;
                oData.Masses = Masses;
                oData.Abundances = Abundances;
                oData.SNs = SNs;
                oData.Resolutions = Resolutions;
                oData.RelAbundances = RelAbundances;
                oData.CalMasses = CalMasses;
                oData.AlignMasses = new double [ oData.FileCount ] [];
                oData.NeutralMasses = new double [ oData.FileCount ] [];
                oData.Formulas = new short [ oData.FileCount ] [] [];
                oData.PPMErrors = new double [ oData.FileCount ] [];
                oData.Candidates = new short [ oData.FileCount ] [];
                for( int FileIndex = 0; FileIndex < oData.FileCount; FileIndex++ ) {
                    oData.AlignMasses [ FileIndex ] = new double [ oData.Masses[FileIndex].Length ];
                    oData.NeutralMasses [ FileIndex ] = new double [ oData.Masses[ FileIndex ].Length ];
                    oData.Formulas [ FileIndex ] = new short [ oData.Masses [ FileIndex ].Length ] [];
                    oData.PPMErrors [ FileIndex ] = new double [ oData.Masses [ FileIndex ].Length ];
                    oData.Candidates [ FileIndex ] = new short [ oData.Masses [ FileIndex ].Length ];
                }

                //Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture( "ja-JP" );
                //string LogFileName = DateTime.Now.ToString();
                //LogFileName = Path.GetDirectoryName( Filenames [ 0 ] ) + "\\" + "Report" + LogFileName.Replace( "/", "" ).Replace( ":", "" ).Replace( " ", "" ) + ".log";
                //oStreamLogWriter = new StreamWriter( LogFileName );

                //Alignment + Formula finding
                if( Alignment == true ) {
                    AlignmentByPeak();
                    for( int PeakIndex = 0; PeakIndex < oAlignData.AlignMasses.Length; PeakIndex++ ) {
                        oAlignData.NeutralMasses [ PeakIndex ] = Ipa.GetNeutralMass( oAlignData.AlignMasses [ PeakIndex ] );
                    }
                    FindFormulas( oAlignData.NeutralMasses, oAlignData.Formulas, oAlignData.PPMErrors, oAlignData.Candidates, FormulaPPMTolerance, RelationErrorAMU, MassLimit );
                    ProcessC13( oAlignData.NeutralMasses, oAlignData.Formulas, oAlignData.PPMErrors );
                } else {
                    for( int FileIndex = 0; FileIndex < oData.FileCount; FileIndex++ ) {
                        for( int PeakIndex = 0; PeakIndex < oData.CalMasses [ FileIndex ].Length; PeakIndex++ ) {
                            oData.AlignMasses [ FileIndex ] [ PeakIndex ] = oData.CalMasses [ FileIndex ] [ PeakIndex ];
                            oData.NeutralMasses [ FileIndex ] [ PeakIndex ] = Ipa.GetNeutralMass( oData.AlignMasses [ FileIndex ] [ PeakIndex ] );
                            oData.Formulas [ FileIndex ] [ PeakIndex ] = ( short [] ) NullFormula.Clone();
                        }
                        FindFormulas( oData.NeutralMasses [ FileIndex ], oData.Formulas [ FileIndex ], oData.PPMErrors [ FileIndex ], oData.Candidates [ FileIndex ], FormulaPPMTolerance, RelationErrorAMU, MassLimit );
                        ProcessC13( oData.NeutralMasses [ FileIndex ], oData.Formulas [ FileIndex ], oData.PPMErrors [ FileIndex ] );
                    }
                }

                //Save
                //preparation
                if( Alignment == true ) {
                    //preparation: peak alignment into file
                    for( int Peak = 0; Peak < oAlignData.NeutralMasses.Length; Peak++ ) {
                        int [] PeakIndexes = oAlignData.Indexes [ Peak ];
                        for( int FileIndex = 0; FileIndex < oData.FileCount; FileIndex++ ) {
                            if( PeakIndexes [ FileIndex ] < 0 ) { continue; }
                            oData.AlignMasses [ FileIndex ] [ PeakIndexes [ FileIndex ] ] = oAlignData.AlignMasses [ Peak ];
                            oData.NeutralMasses [ FileIndex ] [ PeakIndexes [ FileIndex ] ] = oAlignData.NeutralMasses [ Peak ];
                            oData.Formulas [ FileIndex ] [ PeakIndexes [ FileIndex ] ] = ( short [] ) oAlignData.Formulas [ Peak ].Clone();
                            oData.PPMErrors [ FileIndex ] [ PeakIndexes [ FileIndex ] ] = oAlignData.PPMErrors [ Peak ];
                            oData.Candidates [ FileIndex ] [ PeakIndexes [ FileIndex ] ] = oAlignData.Candidates [ Peak ];
                        }
                    }
                }

                //IndividualFileReports
                if( GenerateIndividualFileReports == true ) {
                    for( int FileIndex = 0; FileIndex < Filenames.Length; FileIndex++ ) {
                        StreamWriter oStreamWriter;
                        int FileExtentionLength = Path.GetExtension( Filenames [ FileIndex ] ).Length;
                        if( oEOutputFileDelimiter == EDelimiters.Comma ) {
                            oStreamWriter = new StreamWriter( Filenames [ FileIndex ].Substring( 0, Filenames [ FileIndex ].Length - FileExtentionLength ) + "CShOut.csv" );
                        } else {
                            oStreamWriter = new StreamWriter( Filenames [ FileIndex ].Substring( 0, Filenames [ FileIndex ].Length - FileExtentionLength ) + "CShOut.txt" );
                        }
                        double [] Masses1 = oData.Masses [ FileIndex ];
                        double [] Abundances1 = oData.Abundances [ FileIndex ];
                        double [] SNs1 = oData.SNs [ FileIndex ];
                        double [] Resolutions1 = oData.Resolutions [ FileIndex ];
                        double [] RelAbundances1 = oData.RelAbundances [ FileIndex ];

                        double [] CalMasses1 = oData.CalMasses [ FileIndex ];
                        double [] AlignMasses = oData.AlignMasses [ FileIndex ];
                        double [] NeutralMasses = oData.NeutralMasses [ FileIndex ];
                        short [] [] Formulas = oData.Formulas [ FileIndex ];
                        double [] PPMErrors = oData.PPMErrors [ FileIndex ];
                        short [] Candidates = oData.Candidates [ FileIndex ];

                        string HeaderLine = "Mass" + OutputFileDelimiter + "Abundance";
                        for( int Element = 0; Element < Elements; Element++ ) {
                            HeaderLine = HeaderLine + OutputFileDelimiter + Enum.GetName( typeof( EElemNumber ), Element );
                        }
                        HeaderLine = HeaderLine + OutputFileDelimiter + "Error_ppm" + OutputFileDelimiter + "Candidates";
                        HeaderLine = HeaderLine + OutputFileDelimiter + "CalMass" + OutputFileDelimiter + "AlignMasses" + OutputFileDelimiter + "NeutralMass";
                        if( SNs1 [ 0 ] > 0 ) {
                            HeaderLine = HeaderLine + OutputFileDelimiter + "sn" + OutputFileDelimiter + "resolution" + OutputFileDelimiter + "rel_abu" + OutputFileDelimiter + "peak_index";
                        }
                        oStreamWriter.WriteLine( HeaderLine );

                        for( int Peak = 0; Peak < Masses1.Length; Peak++ ) {
                            string Line = Masses1 [ Peak ].ToString();
                            Line = Line + OutputFileDelimiter + Abundances1 [ Peak ].ToString();
                            for( int Element = 0; Element < Elements; Element++ ) {
                                Line = Line + OutputFileDelimiter + Formulas [ Peak ] [ Element ].ToString();
                            }
                            if( oEErrorType == EErrorType.CIA ) {
                                Line = Line + OutputFileDelimiter + PPMErrors [ Peak ].ToString();
                            } else if( oEErrorType == EErrorType.Signed ) {
                                if( IsFormula( Formulas [ Peak ] ) == false ) {
                                    Line = Line + OutputFileDelimiter + "0";
                                } else {
                                    Line = Line + OutputFileDelimiter + SignedMassErrorPPM( NeutralMasses [ Peak ], FormulaToNeutralMass( Formulas [ Peak ] ) ).ToString();
                                }
                            }
                            Line = Line + OutputFileDelimiter + Candidates [ Peak ].ToString();
                            Line = Line + OutputFileDelimiter + CalMasses1 [ Peak ].ToString();
                            Line = Line + OutputFileDelimiter + AlignMasses [ Peak ].ToString();
                            Line = Line + OutputFileDelimiter + NeutralMasses [ Peak ].ToString();
                            if( SNs1 [ 0 ] > 0 ) {
                                Line = Line + OutputFileDelimiter + SNs1 [ Peak ].ToString();
                                Line = Line + OutputFileDelimiter + Resolutions1 [ Peak ].ToString();
                                Line = Line + OutputFileDelimiter + RelAbundances1 [ Peak ].ToString();
                            }
                            Line = Line + OutputFileDelimiter + Peak.ToString();
                            oStreamWriter.WriteLine( Line );
                        }
                        oStreamWriter.Close();
                    }
                }
                //AlignmentReport
                if( Alignment == true ) {
                    StreamWriter oStreamWriter;
                    if( oEOutputFileDelimiter == EDelimiters.Comma ) {
                        oStreamWriter = new StreamWriter( Path.GetDirectoryName( Filenames [ 0 ] ) + "\\Report.csv" );
                    } else {
                        oStreamWriter = new StreamWriter( Path.GetDirectoryName( Filenames [ 0 ] ) + "\\Report.txt" );
                    }
                    string HeaderLine = "Mass";
                    foreach( string Element in Enum.GetNames( typeof( CCia.EElemNumber ) ) ) {
                        HeaderLine = HeaderLine + OutputFileDelimiter + Element;
                    }
                    HeaderLine = HeaderLine + OutputFileDelimiter + "El_comp" + OutputFileDelimiter + "Class";
                    HeaderLine = HeaderLine + OutputFileDelimiter + "NeutralMass" + OutputFileDelimiter + "Error_ppm" + OutputFileDelimiter + "Candidates";
                    foreach( string File in Filenames ) {
                        HeaderLine = HeaderLine + OutputFileDelimiter + Path.GetFileNameWithoutExtension( File );
                    }
                    oStreamWriter.WriteLine( HeaderLine );

                    for( int Peak = 0; Peak < oAlignData.NeutralMasses.Length; Peak++ ) {
                        string Line = Ipa.GetChargedMass( oAlignData.NeutralMasses [ Peak ] ).ToString();
                        short [] Formula = oAlignData.Formulas [ Peak ];
                        for( int Element = 0; Element < Elements; Element++ ) {
                            Line = Line + OutputFileDelimiter + Formula [ Element ].ToString();
                        }
                        Line = Line + OutputFileDelimiter;
                        if( Formula [ ( int ) EElemNumber.C ] > 0 ) {
                            foreach( EElemNumber Element in Enum.GetValues( typeof( EElemNumber ) ) ) {
                                if( Element == EElemNumber.C ) {
                                    if( Formula [ ( int ) EElemNumber.C ] + Formula [ ( int ) EElemNumber.C13 ] > 0 ) {
                                        Line = Line + EElemNumber.C.ToString();
                                    }
                                } else if( Element == EElemNumber.C13 ) {
                                    continue;
                                } else {
                                    if( Formula [ ( int ) Element ] > 0 ) {
                                        Line = Line + Element.ToString();
                                    }
                                }
                            }
                        }

                        //class	        O:C(low)	O:C(high)	H:C(low)	H:C(high)
                        //lipid 	    >=0	        <0.3	    >=1.5	    <2.5
                        //unsatHC	    >=0	        <0.125	    >=0.8	    <1.5
                        //condHC	    >=0	        <0.95	    >=0.2	    <0.8
                        //protein	    >=0.3	    <0.55	    >=1.5	    <2.3
                        //aminosugar	>=0.55   	<0.7	    >=1.5	    <2.2
                        //carb	        >=0.7	    <1.05	    >=1.5	    <2.2
                        //lignin	    >=0.125    	<0.65	    >=0.8	    <1.5
                        //tannin	    >=0.65	    <1.1	    >=0.8	    <1.5
                        double TotalC = Formula [ ( int ) EElemNumber.C ] + Formula [ ( int ) EElemNumber.C13 ];//must be double!
                        if( TotalC == 0 ) {
                            Line = Line + OutputFileDelimiter + "None";
                        } else {
                            double HToC = Formula [ ( int ) EElemNumber.H ] / TotalC;
                            double OToC = Formula [ ( int ) EElemNumber.O ] / TotalC;
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

                        Line = Line + OutputFileDelimiter + oAlignData.NeutralMasses [ Peak ].ToString();
                        if( oEErrorType == EErrorType.CIA ) {
                            Line = Line + OutputFileDelimiter + oAlignData.PPMErrors [ Peak ].ToString();
                        } else if( oEErrorType == EErrorType.Signed ) {
                            if( IsFormula( Formula ) == false ) {
                                Line = Line + OutputFileDelimiter + "0";
                            } else {
                                //Line = Line + OutputFileDelimiter + ( oAlignData.NeutralMasses [ Peak ] - FormulaToNeutralMass( Formula ) ).ToString();
                                Line = Line + OutputFileDelimiter + SignedMassErrorPPM( oAlignData.NeutralMasses [ Peak ], FormulaToNeutralMass( Formula ) ).ToString();
                            }
                        }
                        Line = Line + OutputFileDelimiter + oAlignData.Candidates [ Peak ].ToString();
                        int [] Indexes = oAlignData.Indexes [ Peak ];
                        for( int FileIndex = 0; FileIndex < Indexes.Length - 1; FileIndex++ ) {
                            if( Indexes [ FileIndex ] < 0 ) {//no peak in file
                                Line = Line + OutputFileDelimiter + "0";
                            } else {
                                Line = Line + OutputFileDelimiter + oData.Abundances [ FileIndex ] [ Indexes [ FileIndex ] ];
                            }
                        }
                        oStreamWriter.WriteLine( Line );
                    }
                    oStreamWriter.Close();
                }
            } catch( Exception ex ) {
                if( oStreamLogWriter != null ) {
                    oStreamLogWriter.WriteLine( "Exception: " );
                    oStreamLogWriter.Write( ex.Message );
                }
            }
            //if( oStreamLogWriter != null ) {
            //    oStreamLogWriter.Flush();
            //    oStreamLogWriter.Close();
            //}
        }
        public void CleanComObject( object o ) {
            try {
                while( System.Runtime.InteropServices.Marshal.ReleaseComObject( o ) > 0 )
                    ;
            } catch { } finally {
                o = null;
            }
        }        
        class CGrpdiffK {
            public bool IsEmpty;
            public List<int> [] Indexes;
        }; 
        bool CheckPeakMassByEvenOdd( double [] Masses) {
            int Even = 0;
            int Odd = 0;
            foreach( double Mass in Masses ) {
                if( ( Math.Round( Mass ) % 2 ) == 0 ) {
                    Even = Even + 1;
                } else {
                    Odd = Odd + 1;
                }
            }
            if( Odd > Even ) { 
                return false; 
            } else { 
                return true;
            }
        }
        void FindFormulas( double [] NeutralMasses, short [][] Formulas, double [] PPMErrors, short [] Candidates, double FormulaErrorPPM, double RelationErrorAMU, double MassLimit ) {
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
            CheckPeakMassByEvenOdd( NeutralMasses );
            //Create relations
            CGrpdiffK [] GrpdiffK = new CGrpdiffK [ NeutralMasses.Length ];
            double MinMass = NeutralMasses [ 0 ];
            double MaxMass = NeutralMasses [ NeutralMasses.Length - 1 ];
            for( int Peak = 0; Peak < NeutralMasses.Length; Peak++ ) {
                CGrpdiffK CurrentGrpdiffK = new CGrpdiffK();
                CurrentGrpdiffK.IsEmpty = true;
                if( UseRelation == true ) {
                    double NeutralMass = NeutralMasses [ Peak ];
                    double NeutralMassToMin = MinMass - NeutralMass;
                    if( UseBackward == false) {
                        NeutralMassToMin = 0;
                    }
                    double NeutralMassToMax = MaxMass - NeutralMass;
                    CurrentGrpdiffK.Indexes = new List<int> [ ListActiveRelationFormulaBuildingBlocks.Count ];
                    for( int Relation = 0; Relation < ListActiveRelationFormulaBuildingBlocks.Count; Relation++ ) {
                        //KL note: look for differences, and keep those less than the defined relation_error                 
                        List<int> CurIndexes = new List<int>();
                        int NeutralMassToMinGrp = ( int ) Math.Ceiling( NeutralMassToMin / ActiveRelationFormulaBuildingBlockMasses [ Relation ] );
                        int NeutralMassToMaxGrp = ( int ) Math.Floor( NeutralMassToMax / ActiveRelationFormulaBuildingBlockMasses [ Relation ] );
                        for( int CurrentGrp = NeutralMassToMinGrp; CurrentGrp <= NeutralMassToMaxGrp; CurrentGrp++ ) {
                            double GrpMinNeutralMass;
                            double GrpMaxNeutralMass;
                            if( oRelationshipErrorType == RelationshipErrorType.AMU ) {
                                GrpMinNeutralMass = NeutralMass + ActiveRelationFormulaBuildingBlockMasses [ Relation ] * (CurrentGrp - RelationErrorAMU);
                                GrpMaxNeutralMass = NeutralMass + ActiveRelationFormulaBuildingBlockMasses [ Relation ] * (CurrentGrp + RelationErrorAMU);                              
                            } else if( oRelationshipErrorType == RelationshipErrorType.GapPPM ) {
                                double GapPPMError = PpmToError( ActiveRelationFormulaBuildingBlockMasses [ Relation ] * CurrentGrp, RelationErrorAMU);                                
                                GrpMinNeutralMass = NeutralMass + ActiveRelationFormulaBuildingBlockMasses [ Relation ] * (CurrentGrp - GapPPMError);
                                GrpMaxNeutralMass = NeutralMass + ActiveRelationFormulaBuildingBlockMasses [ Relation ] * (CurrentGrp + GapPPMError);
                            } else {
                                double GapNeutralMass =  NeutralMass + ActiveRelationFormulaBuildingBlockMasses [ Relation ] * CurrentGrp;
                                double PPMError = PpmToError( GapNeutralMass, RelationErrorAMU); 
                                GrpMinNeutralMass = GapNeutralMass - PPMError;
                                GrpMaxNeutralMass = GapNeutralMass + PPMError;
                            }
                            int GrpPeak = Array.BinarySearch( NeutralMasses, GrpMinNeutralMass );
                            if( GrpPeak < 0 ) { GrpPeak = ~GrpPeak; }
                            for( ; GrpPeak < NeutralMasses.Length; GrpPeak++ ) {
                                if( GrpPeak == Peak ) { continue; }
                                if( NeutralMasses [ GrpPeak ] > GrpMaxNeutralMass ) {
                                    break;}
                                //double diff = ( NeutralMasses [ GrpPeak ] - NeutralMass ) / ActiveRelationFormulaBuildingBlockMasses [ Relation ];
                                //if( Math.Abs( diff - Math.Round( diff ) ) <= RelationErrorAMU ) {
                                    CurIndexes.Add( GrpPeak );
                                    CurrentGrpdiffK.IsEmpty = false;
                                //} else {
                                //    break;
                                //}
                            }
                        }
                        CurrentGrpdiffK.Indexes [ Relation ] = CurIndexes;
                    }
                }
                GrpdiffK [ Peak ] = CurrentGrpdiffK;
            }
            for( int Peak = 0; Peak < NeutralMasses.Length; Peak++ ) {
                if( NeutralMasses [ Peak ] < MassLimit ) {
                    short [] DBFormula = PickDBFormula( NeutralMasses [ Peak ], out Candidates [ Peak ] );
                    if( IsFormula( DBFormula ) == false ) {
                        continue;
                    }
                    double DBFormulaMass = 0;
                    double DBFormulaMassError = 0;
                    DBFormulaMass = FormulaToNeutralMass( DBFormula );
                    DBFormulaMassError = AbsMassErrorPPM( DBFormulaMass, NeutralMasses [ Peak ] );
                    if( GrpdiffK [ Peak ].IsEmpty == true ) {
                        //StartFormula = PickDBFormula( NeutralMasses [ Peak ], out Candidates[ Peak ]);
                        //if( IsFormula( DBFormula ) == true ) {
                            //double tempmass = FormulaToNeutralMass( DBFormula );
                            //double Terror = AbsMassErrorPPM( tempmass, NeutralMasses [ Peak ]);
                            Formulas [ Peak ] = ( short [] ) DBFormula.Clone();
                            PPMErrors [ Peak ] = DBFormulaMassError/*Terror*/;
                        //}
                    } else {
                        if( IsFormula( Formulas [ Peak ]) == false ) {
                            //StartFormula = PickDBFormula( NeutralMasses [ Peak ], out Candidates [ Peak ] );
                            //if( IsFormula( DBFormula ) == true ) {
                                //double tempmass = FormulaToNeutralMass( DBFormula );
                                //double Terror = AbsMassErrorPPM( tempmass, NeutralMasses [ Peak ]);
                                Formulas [ Peak ] = ( short [] ) DBFormula.Clone();
                                PPMErrors [ Peak ] = DBFormulaMassError/*Terror*/;
                            //}
                        } else {
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
                            if( UseCIAFormulaScore == true ) {
                                //if( tempform [ ( int ) EElemNumber.S ] + tempform [ ( int ) EElemNumber.P ] < Formulas [ Peak ] [ ( int ) EElemNumber.S ] + Formulas [ Peak ] [ ( int ) EElemNumber.P ]
                                if( DBFormula [ ( int ) EElemNumber.S ] + DBFormula [ ( int ) EElemNumber.P ] < Formulas [ Peak ] [ ( int ) EElemNumber.S ] + Formulas [ Peak ] [ ( int ) EElemNumber.P ]
                                        && Math.Abs( DBFormulaMassError/*errornew*/ ) < FormulaErrorPPM / 2 ) {
                                    //KL adding this last line 1/20/09 - only keep brute force if it 'really' improves the formula (much smaller error)                                                                         
                                    Formulas [ Peak ] = ( short [] ) DBFormula/*tempform*/.Clone();
                                    PPMErrors [ Peak ] = DBFormulaMassError/*errornew*/;
                                }
                            } else {
                                if( IsNewFormulaScoreBetter( Formulas [ Peak ], PPMErrors [ Peak ], DBFormula, DBFormulaMassError ) == true ) {
                                    Formulas [ Peak ] = ( short [] ) DBFormula/*tempform*/.Clone();
                                    PPMErrors [ Peak ] = DBFormulaMassError/*errornew*/;
                                }
                            }
                            //}
                        }
                        //if( IsFormula( Formulas [ Peak ] ) == false ) { continue; }
                        //DBFormula = ( short [] ) Formulas [ Peak ].Clone();
                        //now go through Grpdiff and assign those formulas...so using the formula decided to be ok - build from that using the relations
                        if( UseRelation == true ) {
                            for( int Relation = 0; Relation < ListActiveRelationFormulaBuildingBlocks.Count; Relation++ ) {
                                foreach( int relpk in GrpdiffK [ Peak ].Indexes [ Relation ] ) {
                                    double diff1 = NeutralMasses [ relpk ] - NeutralMasses [ Peak ];
                                    int numGps = ( int ) ( Math.Round( ( NeutralMasses [ relpk ] - NeutralMasses [ Peak ] ) / ActiveRelationFormulaBuildingBlockMasses [ Relation ] ) );
                                    if( numGps > 0 ) {
                                        short [] newform = new short [ Elements ];
                                        for( int Element = 0; Element < Elements; Element++ ) {
                                            newform [ Element ] = ( short ) ( DBFormula [ Element ] + numGps * ListActiveRelationFormulaBuildingBlocks [ Relation ] [ Element ] );
                                        }
                                        double newmass = FormulaToNeutralMass( newform );
                                        double errornew = AbsMassErrorPPM( newmass, NeutralMasses [ relpk ] );
                                        bool checknew = CheckFormulaByFilters( newform, NeutralMasses [ relpk ] );
                                        bool checkold = CheckFormulaByFilters( Formulas [ relpk ], NeutralMasses [ relpk ] );
                                        if( checknew == true ) {
                                            if( checkold == true ){
                                                if( UseCIAFormulaScore == true ) {
                                                    if( ( newform [ ( int ) EElemNumber.S ] + newform [ ( int ) EElemNumber.P ] < Formulas [ relpk ] [ ( int ) EElemNumber.S ] + Formulas [ relpk ] [ ( int ) EElemNumber.P ] )
                                                            && ( Math.Abs( errornew ) <= FormulaErrorPPM ) ) {
                                                        Formulas [ relpk ] = ( short [] ) newform.Clone();
                                                        PPMErrors [ relpk ] = errornew;
                                                    }
                                                } else {
                                                    if( IsNewFormulaScoreBetter( Formulas [ relpk ] , PPMErrors [ relpk ], newform, errornew) == true){
                                                        Formulas [ relpk ] = ( short [] ) newform.Clone();
                                                        PPMErrors [ relpk ] = errornew;
                                                    }
                                                }
                                            } else if( checkold == false && Math.Abs( errornew ) <= FormulaErrorPPM ) {
                                                Formulas [ relpk ] = ( short [] ) newform.Clone();
                                                PPMErrors [ relpk ] = errornew;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                } else {// > MassLimit
                    Candidates [ Peak ] = -1;//no search
                    if( GrpdiffK [ Peak ].IsEmpty == false) {
                        if( IsFormula( Formulas[ Peak ] ) == true ) {//if the formula is already known
                            short [] startform = ( short [] ) ( Formulas [ Peak ].Clone() );
                            //KL change to only do this for the places where Grpdiff is not empty                                 
                            for( int Relation = 0; Relation < ListActiveRelationFormulaBuildingBlocks.Count; Relation++ ) {
                                foreach( int relpk in GrpdiffK [ Peak ].Indexes [ Relation ] ) {
                                    //relpk > Peak???
                                    bool checkold = CheckFormulaByFilters( Formulas [ relpk ], NeutralMasses [ relpk ]);//???
                                    short [] newform = ( short [] ) startform.Clone();
                                    int RelationshipGaps = ( int ) Math.Round( ( NeutralMasses [ relpk ] - NeutralMasses [ Peak ]) / ActiveRelationFormulaBuildingBlockMasses [ Relation ] );
                                    if( Math.Abs( RelationshipGaps ) > MaxRelationGaps ) { continue; }
                                    for( int Element = 0; Element < Elements; Element++ ) {
                                        newform [ Element ] = ( short ) ( newform [ Element ] + RelationshipGaps * ListActiveRelationFormulaBuildingBlocks [ Relation ] [ Element ] );
                                    }
                                    bool checknew = CheckFormulaByFilters( newform, NeutralMasses [ relpk ]);
                                    double newmass = FormulaToNeutralMass( newform );
                                    double errornew = AbsMassErrorPPM( newmass, NeutralMasses [ relpk ] );
                                    if( ( checknew == true ) && ( Math.Abs( errornew ) <= FormulaErrorPPM ) ) {
                                        if( checkold == true ) {
                                            if( UseCIAFormulaScore == true) {
                                                if( newform [ ( int ) EElemNumber.S ] + newform [ ( int ) EElemNumber.P ] < Formulas [ relpk ] [ ( int ) EElemNumber.S ] + Formulas [ relpk ] [ ( int ) EElemNumber.P ] ) {
                                                    Formulas [ relpk ] = ( short [] ) newform.Clone();
                                                    PPMErrors [ relpk ] = errornew;
                                                }
                                            } else {
                                                if( IsNewFormulaScoreBetter( Formulas [ relpk ], PPMErrors [ relpk ], newform, errornew) == true){
                                                    Formulas [ relpk ] = ( short [] ) newform.Clone();
                                                    PPMErrors [ relpk ] = errornew;
                                                }
                                            }
                                        } else if( checkold == false ) {
                                            Formulas [ relpk ] = ( short [] ) newform.Clone();
                                            PPMErrors [ relpk ] = errornew;
                                        }
                                    }
                                }
                            }
                        } else {//formula is not known - check for formulas in lower masses                            
                            //KL change to only do this for the places where Grpdiff is not empty       
                            if( UseRelation == true ) {
                                for( int Relation = 0; Relation < ListActiveRelationFormulaBuildingBlocks.Count; Relation++ ) {
                                    foreach( int low_m in GrpdiffK [ Peak ].Indexes [ Relation ] ) {
                                        if( NeutralMasses [ low_m ] < NeutralMasses [ Peak ] ) {
                                            //int low_m = GrpdiffK [ Peak ].Indexs [Relation][ t ];
                                            if( IsFormula( Formulas [ low_m ] ) == true ) {
                                                short [] startform = ( short [] ) Formulas [ low_m ].Clone();
                                                int RelationshipGaps = ( int ) Math.Round( Math.Abs( NeutralMasses [ low_m ] - NeutralMasses [ Peak ] ) / ActiveRelationFormulaBuildingBlockMasses [ Relation ] );
                                                if( Math.Abs( RelationshipGaps ) > MaxRelationGaps ) { continue; }
                                                short [] newform = ( short [] ) startform.Clone();
                                                for( int Element = 0; Element < Elements; Element++ ) {
                                                    newform [ Element ] = ( short ) ( newform [ Element ] + RelationshipGaps * ListActiveRelationFormulaBuildingBlocks [ Relation ] [ Element ] );
                                                }
                                                bool checknew = CheckFormulaByFilters( newform, NeutralMasses [ Peak ] );
                                                if( checknew == false ) { continue; }
                                                double newmass = FormulaToNeutralMass( newform );
                                                double errornew = AbsMassErrorPPM( newmass, NeutralMasses [ Peak ] );
                                                //if( Math.Abs( numGps ) <= MaxNumGps && checknew == true && errornew <= FormulaError ) {
                                                bool bIsFormula = IsFormula( Formulas [ Peak ] );
                                                if( ( ( bIsFormula == false ) && ( Math.Abs( errornew ) <= FormulaErrorPPM ) ) || ( ( bIsFormula == true ) && ( errornew < PPMErrors [ Peak ] ) ) ) {
                                                    Formulas [ Peak ] = ( short [] ) newform.Clone();
                                                    PPMErrors [ Peak ] = errornew;
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
            if( UseKendrick == true ) {
                //KendrickParameters [] AKendrick_matrix = new KendrickParameters [ NeutralMasses.Length ];
                int [] KMD = new int [ NeutralMasses.Length ];
                int [] ZStar = new int [ NeutralMasses.Length ];
                for( int Peak = 0; Peak < NeutralMasses.Length; Peak++ ) {
                    //AKendrick_matrix [ Peak ] = new KendrickParameters();
                    //AKendrick_matrix [ Peak ].peak_mass = NeutralMasses [ Peak ];
                    //AKendrick_matrix [ Peak ].KenMass = NeutralMasses [ Peak ] / ( C + 2 * H ) * 14;
                    double KendrickMass = NeutralMasses [ Peak ] / ( C + 2 * H ) * 14;
                    int NomIUPACMass = ( int ) Math.Floor( NeutralMasses [ Peak ] );
                    //AKendrick_matrix [ Peak ].KMD = ( int ) Math.Floor( ( NomIUPACMass - KendrickMass ) * 1000 );
                    KMD[ Peak ] = ( int ) Math.Floor( ( NomIUPACMass - KendrickMass ) * 1000 );
                    //AKendrick_matrix [ Peak ].Zstar = NomIUPACMass % 14 - 14;
                    ZStar[ Peak ] = NomIUPACMass % 14 - 14;
                }
                for( int Peak = 0; Peak < NeutralMasses.Length; Peak++ ) {
                    if( IsFormula( Formulas [ Peak ] ) == false ) { continue; }
                    short [] startform = ( short [] ) Formulas [ Peak ].Clone();
                    int FirstRelpk = -1;
                    for( int relpk = 0; relpk < NeutralMasses.Length; relpk++ ) {
                        //if( ( AKendrick_matrix [ relpk ].KMD == AKendrick_matrix [ Peak ].KMD && AKendrick_matrix [ relpk ].Zstar == AKendrick_matrix [ Peak ].Zstar ) == false ) {
                        if( ( KMD [ relpk ] == KMD [ Peak ] && ZStar [ relpk ] == ZStar [ Peak ]) == false ) {
                            continue;
                        }
                        if( FirstRelpk == -1 ) {
                            FirstRelpk = relpk;
                            continue;
                        }
                        //int numCH2 = ( int ) Math.Floor( ( AKendrick_matrix [ relpk ].peak_mass - AKendrick_matrix [ FirstRelpk ].peak_mass ) / 14 );
                        int numCH2 = ( int ) Math.Floor( ( NeutralMasses [ relpk ] - NeutralMasses[ FirstRelpk ]) / 14 );                        
                        short [] newform = ( short [] ) startform.Clone();
                        //KL change this to be more general
                        newform [ ( int ) EElemNumber.C ] = ( short ) ( newform [ ( int ) EElemNumber.C ] + 1 * numCH2 );
                        newform [ ( int ) EElemNumber.H ] = ( short ) ( newform [ ( int ) EElemNumber.H ] + 2 * numCH2 );
                        bool checknew = CheckFormulaByFilters( newform, NeutralMasses [ relpk ] );
                        bool checkold = CheckFormulaByFilters( Formulas [ relpk ], NeutralMasses [ relpk ] );
                        double errornew = AbsMassErrorPPM( FormulaToNeutralMass( newform ), NeutralMasses [ relpk ] );
                        if( ( checkold == true ) && ( checknew == true ) && ( Math.Abs( errornew ) <= FormulaErrorPPM / 2 ) ){
                            if( UseCIAFormulaScore == true ) {
                                if( ( newform [ ( int ) EElemNumber.S ] + newform [ ( int ) EElemNumber.P ] ) < ( Formulas [ relpk ] [ ( int ) EElemNumber.S ] + Formulas [ relpk ] [ ( int ) EElemNumber.P ] ) ) {
                                    Formulas [ relpk ] = ( short [] ) newform.Clone();
                                    PPMErrors [ relpk ] = errornew;
                                }
                            } else {
                                if( IsNewFormulaScoreBetter(  Formulas [ relpk ], PPMErrors [ relpk ], newform, errornew) ){
                                    Formulas [ relpk ] = ( short [] ) newform.Clone();
                                    PPMErrors [ relpk ] = errornew;
                                }
                            }
                        } else if( ( checkold == false ) && ( checknew == true ) && ( Math.Abs( errornew ) <= FormulaErrorPPM / 2 ) ) {
                            Formulas [ relpk ] = ( short [] ) newform.Clone();
                            PPMErrors [ relpk ] = errornew;
                        }
                    }
                }
            }
        }

        short [] PickDBFormula( double peak_mass, out short Candidates ) {//useMATfile_KL4
            Candidates = 0;
            short [] BestFormula = ( short[]) NullFormula.Clone();
            //using the master list of compounds - can keep them in the array and then use arrayfun to search through the different cells in sData
            //KL 12/15/08
            //1/2/09 - can have the case where multiple compounds have the same number of non-oxygen heteroatoms, so that isn't always the best way to sort. 
            //If that happens, from the options with the lowest number of non-oxygen heteroatoms, chose the one with the lowest error
            //KL 1/6/09 - convert elemformula to double to get the precision I need

            int DBLowerIndex;
            int DBUpperIndex;
            if( GetDBLimitIndexes( peak_mass, out DBLowerIndex, out DBUpperIndex ) == false ) { return BestFormula; }
            Candidates = (short) ( DBUpperIndex - DBLowerIndex + 1);
            double BestMass = 0;
            double BestErrorPPM = 0;
            for( int DBIndex = DBLowerIndex; DBIndex <= DBUpperIndex; DBIndex++ ) {
                //if( UseFormulaFilters == true ) {
                if( CheckFormulaByFilters( DBFormulas [ DBIndex ], DBMasses [ DBIndex ] ) == false ) {
                    continue;
                }
                //}
                double DBMass = DBMasses [ DBIndex ];
                short [] DBFormula = DBFormulas [ DBIndex ];
                double ErrorPPMToDBMass = Math.Abs( AbsMassErrorPPM( DBMass, peak_mass ) );
                bool Change = false;
                if( IsFormula( BestFormula) == false ) {
                    Change = true;
                }else{
                    if( IsNewFormulaScoreBetter( BestFormula, BestErrorPPM, DBFormula, ErrorPPMToDBMass) == true){
                        Change = true;
                    }
                }
                if( Change == true) {
                    BestMass = DBMass;
                    BestFormula = ( short [] ) DBFormula.Clone();
                    BestErrorPPM = ErrorPPMToDBMass;
                }
            }        
            return BestFormula;
        }
        private bool IsNewFormulaScoreBetter( short [] Formula, double FormulaError, short [] NewFormula, double NewFormulaError ) {
            switch( FormulaScore ) {
                case EFormulaScore.lowestSP:
                    //select the first elemental formula on the list (lowest # of non oxygen heteroatoms
                    //1/20/09: change to test if only sort based on S and P 
                    //KL 1/2/09 addition - if multiple with the same, low, # of non-oxygen heteroatoms, sort based on the lowest error AND
                    //lowest number of non-oyxgen heteroatoms (S and P only here).
                    int SPlusP = Formula [ ( int ) EElemNumber.S ] + Formula [ ( int ) EElemNumber.P ];
                    int NewSPlusP = NewFormula [ ( int ) EElemNumber.S ] + NewFormula [ ( int ) EElemNumber.P ];
                    if( NewSPlusP < SPlusP) { return true;}
                    break;
                case EFormulaScore.lowestError:
                    //sort based on the lowest error - for comparison
                    if( NewFormulaError < FormulaError ) { return true;}
                    break;
                case EFormulaScore.HAcap:
                    //public enum EElemNumber { C = 0, H, O, N, C13, S, P, Na};
                    //added 4/15/09 by KL cap the number of S and P atoms, after selecting based on the lowest number of N, S, and P
                    //4/15/09 by KL cap the number of S and P atoms, after selecting based on the lowest number of N, S, and P added 2/24/14 by NT; for masses under 350 limit number of N to 3 calculate the # of non-oxygen heteroatoms
                    int NonOxyHeteroAtoms = Formula [ ( int ) EElemNumber.N ] + Formula [ ( int ) EElemNumber.S ] + Formula [ ( int ) EElemNumber.P ];
                    int NewNonOxyHeteroAtoms = NewFormula [ ( int ) EElemNumber.N ] + NewFormula [ ( int ) EElemNumber.S ] + NewFormula [ ( int ) EElemNumber.P ];
                    if( NewNonOxyHeteroAtoms < NonOxyHeteroAtoms ) {
                        //only consider the formulas with the low # non-oxy HA
                        return true;
                    } else if( NonOxyHeteroAtoms == NewNonOxyHeteroAtoms ) {
                        //then only consider formulas with P <= 1 or S <= 3
                        bool Sle3AndPle1 = ( Formula [ ( int ) EElemNumber.S ] <= 3 ) & ( Formula [ ( int ) EElemNumber.P ] <= 1 );
                        bool NewSle3AndPle1 = ( NewFormula [ ( int ) EElemNumber.S ] <= 3 ) & ( NewFormula [ ( int ) EElemNumber.P ] <= 1 );
                        if( ( Sle3AndPle1 == false ) && ( NewSle3AndPle1 == true ) ) {
                            return true;
                        } else if( ( Sle3AndPle1 == NewSle3AndPle1 ) && ( FormulaError > NewFormulaError ) ) {
                            return true;
                        }
                    }
                    break;
                default:
                    throw new Exception( "Wrong SortType : " + FormulaScore.ToString() );
            }
            return false;
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
        bool CheckFormulaByFilters( short [] Formula, double Mass){
            if( Formula == null ) { return false; }
            if( UseFormulaFilters == false ) { return true; }
            //Golden rule 1 "Elemental counts" within mass range
            if( GoldenRuleFilters [ 0 ] == true ) {
                short [] LowLimitFormula = { 1, 1, 0, 0, 0, 0, 0, 0 };
                short [] UpLimit500Formula = { 39, 72, 20, 20, 0, 10, 9, 0 };
                short [] UpLimit500_1000Formula = { 78, 126, 27, 25, 0, 14, 9, 0 };
                short [] UpLimit_1000Formula = { 156, 180, 63, 32, 0, 14, 9, 0 };
                if( Formula [ ( int ) EElemNumber.C ] < LowLimitFormula [ ( int ) EElemNumber.C ] || Formula [ ( int ) EElemNumber.H ] < LowLimitFormula [ ( int ) EElemNumber.H ] ) {
                    return false;
                }
                short [] UpLimitFormula;
                if( Mass <= 500 ) {
                    UpLimitFormula = UpLimit500Formula;
                } else if( Mass > 500 && Mass <= 1000 ) {
                    UpLimitFormula = UpLimit500_1000Formula;
                } else {
                    UpLimitFormula = UpLimit_1000Formula;
                }
                bool result = ( Formula [ ( int ) EElemNumber.C ] + Formula [ ( int ) EElemNumber.C13 ] < UpLimitFormula [ ( int ) EElemNumber.C ] )
                        & ( Formula [ ( int ) EElemNumber.H ] < UpLimitFormula [ ( int ) EElemNumber.H ] )
                        & ( Formula [ ( int ) EElemNumber.O ] < UpLimitFormula [ ( int ) EElemNumber.O ] )
                        & ( Formula [ ( int ) EElemNumber.N ] < UpLimitFormula [ ( int ) EElemNumber.N ] )
                        & ( Formula [ ( int ) EElemNumber.S ] < UpLimitFormula [ ( int ) EElemNumber.S ] )
                        & ( Formula [ ( int ) EElemNumber.P ] < UpLimitFormula [ ( int ) EElemNumber.P ] );

                if( result == false ) { return false; } 
            }

            //Golden rule 2 "Valence rule"
            //odd elements with odd valency must have event total valency sum
            //check only H (valency 1) and N (valency 3) ???
            //Filter 2. Valence rules
            //H+N%2=0 AND C*4+H*1+O*2+N*3+C13*4+S*2+P*3+Na*1
            //public enum EElemNumber { C, H, O, N, C13, S, P, Na};
            //short [] ElemValences = { 4, 1, 2, 3, 4, 2, 3, 1 };
            if( GoldenRuleFilters [ 1 ] == true ) {
                if( ( ( Formula [ ( int ) EElemNumber.H ] + Formula [ ( int ) EElemNumber.N ] + Formula [ ( int ) EElemNumber.P ] ) % 2 ) != 0 ) {
                    return false;
                }
                //sum of valences greater than or equal to twice the maximum valence of one element
                int FormulaValences = 0;
                for( int Element = 0; Element < Elements; Element++ ) {
                    FormulaValences = FormulaValences + Formula [ Element ] * ElemValences [ Element ];
                }
                for( int Element = 0; Element < Elements; Element++ ) {
                    if( Formula [ Element ] > 0 ) {
                        if( ElemValences [ Element ] * 2 > FormulaValences ) {
                            return false;
                        }
                    }
                }
                //sum of valences greater than or equal to 2 * atom # - 1 (incorrect. Nikola)
                //sum of valences greater than or equal to 2 * (atom # - 1)
                int TotalAtoms = 0;
                for( int Element = 0; Element < Elements; Element++ ) {
                    TotalAtoms = TotalAtoms + Formula [ Element ];
                }
                //if( FormulaValences < 2 * TotalAtoms - 1 ) {
                if( FormulaValences < 2 * ( TotalAtoms - 1 ) ){
                    return false;
                }
            }
            //Godlen rule 3 "Elemental ratios"
            if( GoldenRuleFilters [ 2 ] == true ) {
                double TotalC = Formula [ ( int ) EElemNumber.C ] + Formula [ ( int ) EElemNumber.C13 ];
                double HC = 1.0 * Formula [ ( int ) EElemNumber.H ] / TotalC;
                double NC = 1.0 * Formula [ ( int ) EElemNumber.N ] / TotalC;
                double OC = 1.0 * Formula [ ( int ) EElemNumber.O ] / TotalC;
                double PC = 1.0 * Formula [ ( int ) EElemNumber.P ] / TotalC;
                double SC = 1.0 * Formula [ ( int ) EElemNumber.S ] / TotalC;
                bool GoldenRule3 = ( HC >= 0.2 ) & ( HC <= 3.1 ) & ( NC >= 0 ) & ( NC <= 1.3 ) & ( OC >= 0 ) & ( OC <= 1.2 ) & ( PC >= 0 ) & ( PC <= 0.3 ) & ( SC >= 0 ) & ( SC <= 0.8 );
                if( GoldenRule3 == false ) {
                    return false;
                }
            }
            //Golden rule 4 "Heteroatom counts"
            if( GoldenRuleFilters [ 4 ] == true ) {
                //manuscript has "<" instead of "<=" for ***Min calculation
                bool checkNOPSMin = ( Formula [ ( int ) EElemNumber.O ] > 1 ) & ( Formula [ ( int ) EElemNumber.N ] > 1 ) & ( Formula [ ( int ) EElemNumber.S ] > 1 ) & ( Formula [ ( int ) EElemNumber.P ] > 1 );
                bool checkNOPSMax = ( Formula [ ( int ) EElemNumber.O ] < 20 ) & ( Formula [ ( int ) EElemNumber.N ] < 10 ) & ( Formula [ ( int ) EElemNumber.S ] < 3 ) & ( Formula [ ( int ) EElemNumber.P ] < 4 );
                if( checkNOPSMin == true && checkNOPSMax == false ) {
                    return false;
                }

                //bool checkNOPMin = ( Formula [ ( int ) EElemNumber.O ] > 3 ) & ( Formula [ ( int ) EElemNumber.N ] > 3 ) & ( Formula [ ( int ) EElemNumber.S ] == 0 ) & ( Formula [ ( int ) EElemNumber.P ] >= 3 );
                //manuscript doesn't use "S == 0"
                bool checkNOPMin = ( Formula [ ( int ) EElemNumber.O ] > 3 ) & ( Formula [ ( int ) EElemNumber.N ] > 3 ) & ( Formula [ ( int ) EElemNumber.P ] > 3 );
                bool checkNOPMax = ( Formula [ ( int ) EElemNumber.O ] < 22 ) & ( Formula [ ( int ) EElemNumber.N ] < 11 ) & ( Formula [ ( int ) EElemNumber.P ] < 6 );
                if( checkNOPMin == true && checkNOPMax == false ) {
                    return false;
                }

                //bool checkOPSMin = ( Formula [ ( int ) EElemNumber.O ] >= 1 ) & ( Formula [ ( int ) EElemNumber.N ] == 0 ) & ( Formula [ ( int ) EElemNumber.S ] >= 1 ) & ( Formula [ ( int ) EElemNumber.P ] >= 1 );
                //manuscript doesn't use "N == 0"
                bool checkOPSMin = ( Formula [ ( int ) EElemNumber.O ] > 1 ) & ( Formula [ ( int ) EElemNumber.S ] > 1 ) & ( Formula [ ( int ) EElemNumber.P ] > 1 );
                bool checkOPSMax = ( Formula [ ( int ) EElemNumber.O ] < 14 ) & ( Formula [ ( int ) EElemNumber.S ] < 3 ) & ( Formula [ ( int ) EElemNumber.P ] < 3 );
                if( checkOPSMin == true && checkOPSMax == false ) {
                    return false;
                }

                //bool checkNPSMin = ( Formula [ ( int ) EElemNumber.O ] == 0 ) & ( Formula [ ( int ) EElemNumber.N ] >= 1 ) & ( Formula [ ( int ) EElemNumber.S ] >= 1 ) & ( Formula [ ( int ) EElemNumber.P ] >= 1 );
                //manuscript doesn't use "O == 0"
                bool checkNPSMin = ( Formula [ ( int ) EElemNumber.N ] > 1 ) & ( Formula [ ( int ) EElemNumber.S ] > 1 ) & ( Formula [ ( int ) EElemNumber.P ] > 1 );
                bool checkNPSMax = ( Formula [ ( int ) EElemNumber.N ] < 4 ) & ( Formula [ ( int ) EElemNumber.S ] < 3 ) & ( Formula [ ( int ) EElemNumber.P ] < 3 );
                if( checkNPSMin == true && checkNPSMax == false ) {
                    return false;
                }

                //bool checkNOSMin = ( Formula [ ( int ) EElemNumber.O ] >= 6 ) & ( Formula [ ( int ) EElemNumber.N ] >= 6 ) & ( Formula [ ( int ) EElemNumber.S ] >= 6 ) & ( Formula [ ( int ) EElemNumber.P ] == 0 );
                //manuscript doesn't use "P == 0"
                bool checkNOSMin = ( Formula [ ( int ) EElemNumber.O ] > 6 ) & ( Formula [ ( int ) EElemNumber.N ] > 6 ) & ( Formula [ ( int ) EElemNumber.S ] > 6 );
                bool checkNOSMax = ( Formula [ ( int ) EElemNumber.O ] < 14 ) & ( Formula [ ( int ) EElemNumber.N ] < 19 ) & ( Formula [ ( int ) EElemNumber.S ] < 8 );
                if( checkNOSMin == true && checkNOSMax == false ) {
                    return false;
                }
            }
            //Golden rule 5 "Positive atoms"
            //KL 1/8/09 - checking to make sure everything is a positive number (i.e. can't have a negative number of elements in a formula)
            if( GoldenRuleFilters [ 4 ] == true ) {
                foreach( short Element in Formula ) {
                    if( Element < 0 ) {
                        return false;
                    }
                }
            }
            //Golden rule 6 "Integer DBE"
            if( GoldenRuleFilters [ 5 ] == true ) {
                //int DBECount = ( Formula [ ( int ) EElemNumber.C ] * 2 + Formula [ ( int ) EElemNumber.N ] + Formula [ ( int ) EElemNumber.P ] - Formula [ ( int ) EElemNumber.H ] + 2 ) % 2;
                int DBEResudence = ( Formula [ ( int ) EElemNumber.N ] + Formula [ ( int ) EElemNumber.P ] - Formula [ ( int ) EElemNumber.H ] ) % 2;
                if( DBEResudence != 0 ) { return false; }
            }

            //Special filter
            if( oESpecialFilter != ESpecialFilters.None ) {
                DataTableSpecialFilter.Rows [ 0 ] [ "Mass" ] = Mass;
                for( int Element = 0; Element < Elements; Element++ ) {
                    DataTableSpecialFilter.Rows [ 0 ] [ Enum.GetName( typeof( EElemNumber ), Element ) ] = Formula [ Element ];
                }
                if( ( bool ) DataTableSpecialFilter.Rows [ 0 ] [ "SpecialFilter" ] == false) {
                    return false;
                }
            }
            //User-defined filters
            if( UserDefinedFilter != null ) {
                UserDefinedFilter.Rows [ 0 ] [ "Mass" ] = Mass;
                for( int Element = 0; Element < Elements; Element++ ) {
                    UserDefinedFilter.Rows [ 0 ] [ Enum.GetName( typeof( EElemNumber ), Element ) ] = Formula [ Element ];
                }
                if( ( bool ) UserDefinedFilter.Rows [ 0 ] [ "UserDefinedFilter" ] == false ) {
                    return false;
                }
            }
            return true;
        } 
        void ProcessC13( double [] NeutralMasses, short [][] Formulas, double [] PPMErrors) {
            if( UseC13 == false ) { return; }
            double CDiff = C13 - C;
            for( int Peak = 0; Peak < NeutralMasses.Length - 1; Peak++ ) {
                short [] PeakFormula = Formulas [ Peak ];
                if( IsFormula( PeakFormula ) == false ) {
                    continue;
                }
                if( ( PeakFormula [ ( int ) EElemNumber.C13 ] > 0) || (PeakFormula [ ( int ) EElemNumber.C] <= 0) ) {
                    continue;
                }
                double PeakMass = NeutralMasses [ Peak ];
                double C13PeakMass = PeakMass + CDiff;
                double MinPeakMass = C13PeakMass - PpmToError( C13PeakMass, C13Tolerance );
                double MaxPeakMass = C13PeakMass + PpmToError( C13PeakMass, C13Tolerance );
                for( int C13Peak = Peak + 1; C13Peak < NeutralMasses.Length; C13Peak++ ) {
                    if( NeutralMasses [ C13Peak] < MinPeakMass ) {
                        continue;
                    }
                    if( NeutralMasses [ C13Peak] > MaxPeakMass ) {
                        break;
                    }
                    if( IsFormula( Formulas [ C13Peak ] ) == false ) {
                        Formulas [ C13Peak ] = ( short [] ) PeakFormula.Clone();
                        Formulas [ C13Peak ] [ ( int ) EElemNumber.C13 ] = ( short ) ( Formulas [ C13Peak ] [ ( int ) EElemNumber.C13 ] + 1 );
                        Formulas [ C13Peak ] [ ( int ) EElemNumber.C ] = ( short ) ( Formulas [ C13Peak ] [ ( int ) EElemNumber.C ] - 1 );
                        PPMErrors [ C13Peak ] = NeutralMasses [ C13Peak ] - FormulaToNeutralMass( Formulas [ C13Peak ] );
                    } else {
                        short [] Formula = ( short [] ) PeakFormula.Clone();
                        Formula [ ( int ) EElemNumber.C13 ] = ( short ) ( Formula [ ( int ) EElemNumber.C13 ] + 1 );
                        Formula [ ( int ) EElemNumber.C ] = ( short ) ( Formula [ ( int ) EElemNumber.C ] - 1 );
                        double PPMError = NeutralMasses [ C13Peak ] - FormulaToNeutralMass( Formula );
                        if( Math.Abs( PPMError ) < Math.Abs( PPMErrors [ C13Peak ] ) ) {
                            Formulas [ C13Peak ] = Formula;
                            PPMErrors [ C13Peak ] = PPMError;
                        }
                    }
                }
            }
        }
        int FindC13ParentPeak( short [] [] Formulas, int C13Peak ) {
            short [] ParentFormula = ( short [] ) Formulas [ C13Peak ].Clone();
            ParentFormula [ ( int ) EElemNumber.C13 ] = ( short ) ( ParentFormula [ ( int ) EElemNumber.C13 ] - 1 );
            ParentFormula [ ( int ) EElemNumber.C ] = ( short ) ( ParentFormula [ ( int ) EElemNumber.C ] + 1 );
            for( int ParentPeak = C13Peak - 1; ParentPeak >= 0; ParentPeak-- ) {
                if( AreFormulasEqual( Formulas [ C13Peak ], ParentFormula ) == true ) {
                    return ParentPeak;
                }
            }
            return -1;
        }
        //*******************************************************************
        //DB
        //*******************************************************************
        List<string> DBFilenames = new List<string>();
        double [] DBMasses = null;
        public double GetDBMass( int Index ) { return DBMasses [ Index ]; }
        short [] [] DBFormulas = null;
        public short [] GetDBFormula( int Index ) { return DBFormulas [ Index ]; }
        public string GetDBFormulaName( int Index ) { return FormulaToName( DBFormulas [ Index ] ); }
        public string [] GetDBFilenames() { return DBFilenames.ToArray(); }
        static int DBBytesPerRecord = sizeof( double ) + Elements * sizeof( short );
        static int DBRecordPerBlock = 1000;
        int DBBlockBytes = DBRecordPerBlock * DBBytesPerRecord;
        [StructLayout( LayoutKind.Explicit )]
        public struct DoubleAndBytes {
            [FieldOffset( 0 )]
            public double Double;
            [FieldOffset( 0 )]
            public byte Byte0;
            [FieldOffset( 1 )]
            public byte Byte1;
            [FieldOffset( 2 )]
            public byte Byte2;
            [FieldOffset( 3 )]
            public byte Byte3;
            [FieldOffset( 4 )]
            public byte Byte4;
            [FieldOffset( 5 )]
            public byte Byte5;
            [FieldOffset( 6 )]
            public byte Byte6;
            [FieldOffset( 7 )]
            public byte Byte7;
            [FieldOffset( 8 )]
            public byte Byte8;
        }
        DoubleAndBytes oDoubleLongUnion = new DoubleAndBytes();
        double BytesToDouble( byte [] Bytes, long StartIndex ) {
            oDoubleLongUnion.Byte0 = Bytes [ StartIndex ];
            oDoubleLongUnion.Byte1 = Bytes [ StartIndex + 1 ];
            oDoubleLongUnion.Byte2 = Bytes [ StartIndex + 2 ];
            oDoubleLongUnion.Byte3 = Bytes [ StartIndex + 3 ];
            oDoubleLongUnion.Byte4 = Bytes [ StartIndex + 4 ];
            oDoubleLongUnion.Byte5 = Bytes [ StartIndex + 5 ];
            oDoubleLongUnion.Byte6 = Bytes [ StartIndex + 6 ];
            oDoubleLongUnion.Byte7 = Bytes [ StartIndex + 7 ];
            return oDoubleLongUnion.Double;
        }
        void DoubleToBytes( double dd, byte [] Bytes, long StartIndex ) {
            oDoubleLongUnion.Double = dd;
            Bytes [ StartIndex ] = oDoubleLongUnion.Byte0;
            Bytes [ StartIndex + 1 ] = oDoubleLongUnion.Byte1;
            Bytes [ StartIndex + 2 ] = oDoubleLongUnion.Byte2;
            Bytes [ StartIndex + 3 ] = oDoubleLongUnion.Byte3;
            Bytes [ StartIndex + 4 ] = oDoubleLongUnion.Byte4;
            Bytes [ StartIndex + 5 ] = oDoubleLongUnion.Byte5;
            Bytes [ StartIndex + 6 ] = oDoubleLongUnion.Byte6;
            Bytes [ StartIndex + 7 ] = oDoubleLongUnion.Byte7;
        }
        void ShortToBytes( short ss, byte [] Bytes, long StartIndex ) {
            Bytes [ StartIndex ] = ( byte ) ss;
            Bytes [ StartIndex + 1 ] = ( byte ) ( ss >> 8 );
        }
        short BytesToShort( byte [] Bytes, long StartIndex ) {
            return ( short ) ( ( Bytes [ StartIndex + 1 ] << 8 ) + Bytes [ StartIndex ] );
        }
        short [] FormulaCovertFromBinary( byte [] TempBytes, int ArrayPointer ) {
            short [] Formula = new short [ Elements ];
            for( int Element = 0; Element < Elements; Element++ ) {
                Formula[ Element ] = BytesToShort( TempBytes, ArrayPointer );
                ArrayPointer = ArrayPointer + sizeof( short );
            }
            return Formula;
        }
        public int GetDBRecords() { return DBMasses.Length; }
        public double GetDBMinMass() {
            if( DBMasses == null ) { return 0; }
            return DBMasses [ 0 ];
        }
        public double GetDBMaxMass() {
            if( DBMasses == null ) { return 0; }
            return DBMasses [ DBMasses.Length - 1 ];
        }
        double DBMinError;
        double DBMaxError;
        public double GetDBMinError() { return DBMinError; }
        public double GetDBMaxError() { return DBMaxError; }
        public bool GetDBLimitIndexes( double Mass, out int LowerIndex, out int UpperIndex ) {
            //double FormulaError = Mass * FormulaErrorPPM / PPM;
            double LowerMZ = Mass - PpmToError( Mass, FormulaPPMTolerance );
            LowerIndex = Array.BinarySearch( DBMasses, LowerMZ );
            UpperIndex = -1;//can't return without assigment
            if( LowerIndex < 0 ) {
                LowerIndex = ~LowerIndex;
            } else {
                LowerIndex = LowerIndex + 1;//must be more
            }
            if( LowerIndex >= DBMasses.Length ) {
                return false;
            }
            double UpperMZ = Mass + PpmToError( Mass, FormulaPPMTolerance );
            UpperIndex = Array.BinarySearch( DBMasses, UpperMZ );
            if( UpperIndex < 0 ) {
                UpperIndex = ~UpperIndex;
            }
            UpperIndex = UpperIndex - 1;//must be less     

            if( UpperIndex >= DBMasses.Length ) {
                UpperIndex = DBMasses.Length - 1;
            }
            if( LowerIndex > UpperIndex ) {
                return false;
            }
            return true;
        }

        public void LoadDBs( string [] NewDBFilenames ) {
            if( ( NewDBFilenames == null ) || ( NewDBFilenames.Length == 0 ) ) { return; }
            DBFilenames.Clear();
            if( DBMasses != null ) { DBMasses = null; }
            if( DBFormulas != null ) {
                for( int Formula = 0; Formula < DBFormulas.Length; Formula++ ) {
                    if( DBFormulas [ Formula ] != null ) {
                        DBFormulas [ Formula ] = null;
                    }
                }
                DBFormulas = null;
            }
            GC.Collect();

            int [] DBRecords = new int [ NewDBFilenames.Length ];
            int MaxRecords = 0;
            for( int DBFilename = 0; DBFilename < NewDBFilenames.Length; DBFilename++ ) {
                DBRecords [ DBFilename ] = ( int ) ( new FileInfo( NewDBFilenames [ DBFilename ] ).Length / DBBytesPerRecord );
                MaxRecords = MaxRecords + DBRecords [ DBFilename ];
            }
            DBMasses = new double [ MaxRecords ];
            DBFormulas = new short [ MaxRecords ] [];
            int NextRecord = 0;
            byte [] TempBytes = new byte [ DBBlockBytes ];
            for( int DBFilename = 0; DBFilename < NewDBFilenames.Length; DBFilename++ ) {
                BinaryReader oBinaryReader = new BinaryReader( File.Open( NewDBFilenames [ DBFilename ], FileMode.Open ) );
                for( int FileRecord = 0; FileRecord < DBRecords [ DBFilename ]; FileRecord = FileRecord + DBRecordPerBlock ) {
                    int RealBytes = oBinaryReader.Read( TempBytes, 0, DBBlockBytes );
                    int RealBlockRecords = RealBytes / DBBytesPerRecord;
                    for( int RecordInBlock = 0; RecordInBlock < RealBlockRecords; RecordInBlock++ ) {
                        int ArrayShift = RecordInBlock * DBBytesPerRecord;
                        DBMasses [ NextRecord ] = BytesToDouble( TempBytes, ArrayShift );
                        ArrayShift = ArrayShift + sizeof( double );
                        DBFormulas [ NextRecord ] = FormulaCovertFromBinary( TempBytes, ArrayShift );
                        NextRecord = NextRecord + 1;
                    }
                }
                oBinaryReader.Close();
            }
            TempBytes = null;

            DBSortAndClean( ref DBMasses, ref DBFormulas);
            DBMassError( DBMasses, DBFormulas, ref DBMinError, ref DBMaxError );
            DBFilenames.AddRange( NewDBFilenames );
        }
        void DBSortAndClean( ref double [] Masses, ref short [][] Formulas){
            Array.Sort( Masses, Formulas );
            int RemovedFormulas = 0;
            int MaxRecords = Masses.Length;
            for( int Record = 0; Record < MaxRecords - 1; Record++ ) {
                double Mass = Masses [ Record ];
                if( Mass < 0 ) { continue; }
                double MassPlusPpmError = Mass + PpmToError( Mass, FormulaPPMTolerance );
                for( int TempRecord = Record + 1; TempRecord < MaxRecords; TempRecord++ ) {
                    if( Masses [ TempRecord ] < 0 ) {
                        continue;
                    }
                    if( Masses [ TempRecord ] > MassPlusPpmError ) {
                        break;
                    }
                    if( AreFormulasEqual( Formulas [ Record ], Formulas [ TempRecord ] ) == true ) {
                        Masses [ TempRecord ] = -1;
                        RemovedFormulas = RemovedFormulas + 1;
                    }
                }
            }
            int RealRecords = MaxRecords - RemovedFormulas;
            double [] TempDBMasses = new double [ RealRecords ];
            short [][] TempDBFormulas = new short [ RealRecords ] [];
            int RealRecord = 0;
            for( int Record = 0; Record < MaxRecords; Record++ ) {
                if( Masses [ Record ] > 0 ) {
                    TempDBMasses [ RealRecord ] = Masses [ Record ];
                    TempDBFormulas [ RealRecord ] = Formulas [ Record ];
                    RealRecord = RealRecord + 1;
                }
            }
            Masses = TempDBMasses;
            TempDBMasses = null;
            Formulas = TempDBFormulas;
            TempDBFormulas = null;
            GC.Collect();
        }
        void DBMassError( double [] Masses, short [] [] Formulas, ref double MinError, ref double MaxError ) {
            MinError = 0;
            MaxError = 0;
            for( int Record = 0; Record < Masses.Length; Record++ ) {
                double DBError = Masses[ Record] - FormulaToNeutralMass( Formulas [ Record ] );
                if( MinError > DBError ) { MinError = DBError; } 
                else if( MaxError < DBError ) { MaxError = DBError; }
            }
        }
        public char [] WordSeparators = new char [] { '\t', ',', ' ' };
        char [] LineSeparators = new char [] { '\r', '\n' };
        bool DBCalculateMassFromFormula = true;
        public bool GetDBCalculateMassFromFormula() { return DBCalculateMassFromFormula; }
        public void SetDBCalculateMassFromFormula( bool DBCalculateMassFromFormula ) { this.DBCalculateMassFromFormula = DBCalculateMassFromFormula; }
        bool DBSort = true;
        public bool GetDBSort() { return DBSort; }
        public void SetDBSort( bool DBSort ) { this.DBSort = DBSort; }
        bool DBMassRangePerCsvFile = false;
        public bool GetDBMassRangePerCsvFile() { return DBMassRangePerCsvFile; }
        public void SetDBMassRangePerCsvFile( bool DBMassRangePerCsvFile ) { this.DBMassRangePerCsvFile = DBMassRangePerCsvFile; }
        double DBMassRange = 100;
        public double GetDBMassRange() { return DBMassRange; }
        public void SetDBMassRange( double DBMassRange ) { this.DBMassRange = DBMassRange; }
        void ReadDBAsciiFile( string Filename, out double [] Masses, out short [] [] Formulas ) {
            //file types: csv, txt, xls, xlsx
            //first line/row could have headers
            //line formats:
            //1. column 1 = index as integer; column 2 = mass as double; column 3-10 = 8 elements as short
            //2. column 1 = mass as double; column 2 = formula like C1H1O8P1 or CH1O8P
            //also checks last empty line
            //read file of text and csv files
            if( File.Exists( Filename ) == false ) { throw new Exception( "File is not exist. " + Filename ); }
            if( new FileInfo( Filename ).Length == 0 ) { throw new Exception( "File is empty. " + Filename ); }

            string FileExtension = Path.GetExtension( Filename );
            List<double> ListMasses = new List<double>();
            List<short []> ListFormulas = new List<short []>();
            bool FirstLine = true;
            if( FileExtension == ".csv" || FileExtension == ".txt" ) {
                StreamReader oStreamReader = new StreamReader( Filename );
                while( oStreamReader.Peek() >= 0 ) {
                    string Line = oStreamReader.ReadLine();
                    if( Line.Length == 0 ) { break; }
                    string [] Words = Line.Split( WordSeparators, StringSplitOptions.RemoveEmptyEntries );
                    double Mass;
                    if( FirstLine == true ) {
                        FirstLine = false;
                        try {
                            Mass = double.Parse( Words [ 0 ] );
                        } catch {
                            continue;
                        }
                    }
                    if( Words.Length == 2 ) {
                        ListMasses.Add( double.Parse( Words [ 0 ] ) );
                        ListFormulas.Add( NameToFormula( Words [ 1 ] ) );
                    } else if( Words.Length == 10 ) {
                        ListMasses.Add( double.Parse( Words [ 1 ] ) );
                        short [] Formula = new short [ Elements ];
                        for( int Element = 0; Element < Elements; Element++ ) {
                            Formula [ Element ] = Int16.Parse( Words [ Element + 2 ] );
                        }
                        ListFormulas.Add( Formula );
                    } else {
                        throw new Exception( "File format is wrong. " + Filename );
                    }
                }
                oStreamReader.Close();
            } else if( FileExtension == ".xlsx" || FileExtension == ".xls" ) {
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
                        short [] Formula = new short [ Elements ];
                        for( int Element = 0; Element < Elements; Element++ ) {
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
            } else {
                throw new Exception( "File type is not supported. " + Filename );
            }
            Masses = ListMasses.ToArray();
            Formulas = ListFormulas.ToArray();
        }
        public void DBConvertAsciiToBin( string DBAsciiFilename ) {
            double [] Masses;
            short [] [] Formulas;
            ReadDBAsciiFile( DBAsciiFilename, out Masses, out Formulas );
            if( DBCalculateMassFromFormula == true ) {
                for( int Record = 0; Record < Masses.Length; Record++ ) {
                    Masses [ Record ] = FormulaToNeutralMass( Formulas [ Record ] );
                }
            }
            if( DBSort == true ) {
                DBSortAndClean( ref Masses, ref Formulas);
            }
            double MinError = 0;
            double MaxError = 0;
            if( DBCalculateMassFromFormula == false) {
                DBMassError( Masses, Formulas, ref MinError, ref MaxError );
            }
            //write
            string DBBinaryFilename = Path.GetFullPath( DBAsciiFilename ).Substring( 0, DBAsciiFilename.Length - Path.GetExtension( DBAsciiFilename ).Length ) + ".bin";
            BinaryWriter oBinaryWriter = new BinaryWriter( File.Open( DBBinaryFilename, FileMode.Create ) );
            for( int Record = 0; Record < Masses.Length; Record++ ) {
                oBinaryWriter.Write( Masses[ Record] );
                for( int Element = 0; Element < Elements; Element++ ) {
                    oBinaryWriter.Write( Formulas[ Record][ Element]);
                }
            }
            oBinaryWriter.Close();
        }
        public void DBConvertAsciisToBin( string [] AsciiFilenames ) {
            List<double> ListMasses = new List<double>();
            List<short []>  ListFormulas = new List<short []>();
            foreach( string Filename in AsciiFilenames ) {
                double [] TempMasses;
                short [] [] TempFormulas;
                ReadDBAsciiFile( Filename, out TempMasses, out TempFormulas );
                ListMasses.AddRange( TempMasses );
                ListFormulas.AddRange( TempFormulas.ToList<short []>() );
            }
            double [] Masses = ListMasses.ToArray();
            short [] [] Formulas = ListFormulas.ToArray();
            if( DBCalculateMassFromFormula == true ) {
                for( int Record = 0; Record < Masses.Length; Record++ ) {
                    Masses [ Record ] = FormulaToNeutralMass( Formulas [ Record ] );
                }
            }
            if( DBSort == true ) {
                DBSortAndClean( ref Masses, ref Formulas);
            }
            double MinError = 0;
            double MaxError = 0;
            if( DBCalculateMassFromFormula == false ) {
                DBMassError( Masses, Formulas, ref MinError, ref MaxError );
            }            

            //write
            string DBBinaryFilename = Path.GetFullPath( AsciiFilenames [ 0 ] ).Substring( 0, AsciiFilenames [ 0 ].Length - Path.GetExtension( AsciiFilenames [ 0 ] ).Length ) + ".bin";
            BinaryWriter oBinaryWriter = new BinaryWriter( File.Open( DBBinaryFilename, FileMode.Create ) );
            for( int Record = 0; Record < Masses.Length; Record++ ) {
                oBinaryWriter.Write( Masses [ Record ] );
                for( int Element = 0; Element < Elements; Element++ ) {
                    oBinaryWriter.Write( Formulas [ Record ] [ Element ] );
                }
            }
            oBinaryWriter.Close();
        }
        public void DBConvertBinToCsv( string DBBinaryFile) {
            long Formulas = new FileInfo( DBBinaryFile ).Length / DBBytesPerRecord;
            BinaryReader oBinaryReader = new BinaryReader( File.Open( DBBinaryFile, FileMode.Open, FileAccess.Read ) );
            StreamWriter oStreamWriter = null;
            if( DBMassRangePerCsvFile == false ) {
                oStreamWriter = new StreamWriter( DBBinaryFile.Substring( 0, DBBinaryFile.Length - 4 ) + ".csv" );
            }
            int RangeIndex = -1;
            for( int Formula = 0; Formula < Formulas; Formula++ ) {
                double Mass = oBinaryReader.ReadDouble();
                string Line = ( Formula + 1 ).ToString() + ',' + Mass.ToString();
                for( int Element = 0; Element < Elements; Element++ ) {
                    short ElementCount = ( short ) oBinaryReader.ReadInt16();
                    Line = Line + ',' + ElementCount.ToString();
                }
                if( DBMassRangePerCsvFile == true ){
                    int NewRangeIndex = Convert.ToInt32( Math.Floor( Mass / DBMassRange) * DBMassRange );
                    if( NewRangeIndex != RangeIndex){
                        RangeIndex = NewRangeIndex;
                        if( oStreamWriter != null ) {
                            oStreamWriter.Flush();
                            oStreamWriter.Close();
                        }
                        oStreamWriter = new StreamWriter( DBBinaryFile.Substring( 0, DBBinaryFile.Length - 4 ) + RangeIndex + ".csv" );
                    }
                }
                oStreamWriter.WriteLine( Line );
            }
            oStreamWriter.Flush();
            oStreamWriter.Close();
            oBinaryReader.Close();
        }
        public void ReportFormulas() {
            for( int FileIndex = 0; FileIndex < oData.FileCount; FileIndex++ ) {
                StreamWriter oStreamWriter;
                string Filename = oData.Filenames [ FileIndex ];
                int FileExtentionLength = Path.GetExtension( Filename ).Length;
                if( oEOutputFileDelimiter == EDelimiters.Comma ) {
                    oStreamWriter = new StreamWriter( Filename.Substring( 0, Filename.Length - FileExtentionLength ) + "FormulaReport.csv" );
                } else {
                    oStreamWriter = new StreamWriter( Filename.Substring( 0, Filename.Length - FileExtentionLength ) + "FormulaReport.txt" );
                }

                string HeaderLine = "Mass" + OutputFileDelimiter + "Abundance";
                for( int Element = 0; Element < Elements; Element++ ) {
                    HeaderLine = HeaderLine + OutputFileDelimiter + Enum.GetName( typeof( EElemNumber ), Element );
                }
                HeaderLine = HeaderLine + OutputFileDelimiter + "Error_ppm"/* + OutputFileDelimiter + "Candidates";
                HeaderLine = HeaderLine + OutputFileDelimiter + "CalMass" + OutputFileDelimiter + "AlignMasses"*/ + OutputFileDelimiter + "NeutralMass";
                if( oData.SNs [ FileIndex ] [ 0 ] > 0 ) {
                    HeaderLine = HeaderLine + OutputFileDelimiter + "sn" + OutputFileDelimiter + "resolution" + OutputFileDelimiter + "rel_abu";
                }
                oStreamWriter.WriteLine( HeaderLine );

                double [] Masses = oData.Masses [ FileIndex ];
                double [] Abundances = oData.Abundances [ FileIndex ];
                double [] SNs = oData.SNs [ FileIndex ];
                double [] Resolutions = oData.Resolutions [ FileIndex ];
                double [] RelAbundances = oData.RelAbundances [ FileIndex ];

                for( int Peak = 0; Peak < Masses.Length; Peak++ ) {
                    string LineStart = Masses [ Peak ].ToString() + OutputFileDelimiter + Abundances [ Peak ].ToString();
                    double NeutralMass = Ipa.GetNeutralMass( Masses [ Peak ] );
                    double Error = PpmToError( NeutralMass, GetFormulaPPMTolerance() );
                    int LowerIndex, UpperIndex;
                    if( GetDBLimitIndexes( NeutralMass, out LowerIndex, out UpperIndex ) == false ) {
                        string Line = LineStart;
                        for( int Element = 0; Element < Elements; Element++ ) {
                            Line = Line + OutputFileDelimiter + "0";
                        }
                        Line = Line + OutputFileDelimiter + "0";
                        Line = Line + OutputFileDelimiter + NeutralMass.ToString();
                        if( SNs [ 0 ] > 0 ) {
                            Line = Line + OutputFileDelimiter + SNs [ Peak ].ToString();
                            Line = Line + OutputFileDelimiter + Resolutions [ Peak ].ToString();
                            Line = Line + OutputFileDelimiter + RelAbundances [ Peak ].ToString();
                        }
                        oStreamWriter.WriteLine( Line );
                    } else {
                        for( int Index = LowerIndex; Index <= UpperIndex; Index++ ) {
                            string Line = LineStart;
                            short [] Formula = DBFormulas [ Index ];
                            for( int Element = 0; Element < Elements; Element++ ) {
                                Line = Line + OutputFileDelimiter + Formula [ Element ].ToString();
                            }
                            Line = Line + OutputFileDelimiter + ( NeutralMass - FormulaToNeutralMass( Formula ) ).ToString();
                            Line = Line + OutputFileDelimiter + NeutralMass.ToString();
                            if( SNs [ 0 ] > 0 ) {
                                Line = Line + OutputFileDelimiter + SNs [ Peak ].ToString();
                                Line = Line + OutputFileDelimiter + Resolutions [ Peak ].ToString();
                                Line = Line + OutputFileDelimiter + RelAbundances [ Peak ].ToString();
                            }
                            oStreamWriter.WriteLine( Line );
                        }
                    }
                }
                oStreamWriter.Close();
            }
        }
    }
}


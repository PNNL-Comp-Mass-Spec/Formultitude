using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using Microsoft.Office.Interop.Excel;
using System.Xml;

namespace Support {
    public class Chain {
        public double BlockMass;
        public double BlockMassStdDev;
        public double [] PeakMasses;
        public int [] PeakIndexes;
        public string Formula;
        public double IdealBlockMass;
    }
    public class InputData {
        public double [] Masses;
        public double [] Abundances;
        public double [] S2Ns;
        public double [] Resolutions;
        public double [] RelAbundances;
        public int [] [] ChainIndexes;

        public Chain [] Chains;
    };
    public static class CFileReader {
        static char [] WordSeparators = new char [] { '\t', ',', ' ' };
        public static void ReadFile( string Filename, out double [] Masses, out double [] Abundances, out double [] S2Ns, out double [] Resolutions, out double [] RelAbundances ) {
            int Peak = -1;
            Masses = new double [ 0 ];
            Abundances = new double [ 0 ];
            S2Ns = new double [ 0 ];
            Resolutions = new double [ 0 ];
            RelAbundances = new double [ 0 ];
            try {
                string FileExtension = Path.GetExtension( Filename );
                double MaxAbundance = 0;
                if( FileExtension == ".xlsx" || FileExtension == ".xls" ) {
                    Microsoft.Office.Interop.Excel.Application ExcelApp = null;
                    Microsoft.Office.Interop.Excel.Workbook ExcelBook = null;
                    Microsoft.Office.Interop.Excel.Worksheet ExcelSheet = null;
                    Microsoft.Office.Interop.Excel.Range ExcelRange = null;
                    string ExceptionMessage = string.Empty;

                    try {
                        ExcelApp = new Microsoft.Office.Interop.Excel.Application();
                        ExcelApp.Visible = false;
                        ExcelBook = ExcelApp.Workbooks.Open( Filename );
                        ExcelSheet = ExcelBook.Sheets [ 1 ];
                        ExcelRange = ExcelSheet.UsedRange;
                        object RangeArray = ExcelRange.Value;
                        int PeakCount = ExcelRange.Rows.Count - 1;
                        Masses = new double [ PeakCount ];
                        Abundances = new double [ PeakCount ];
                        S2Ns = new double [ PeakCount ];
                        Resolutions = new double [ PeakCount ];
                        RelAbundances = new double [ PeakCount ];
                        for( Peak = 0; Peak < PeakCount; Peak++ ) {
                            int Row = Peak + 2;
                            ( ( Array ) RangeArray ).GetLength( 0 );
                            Masses [ Peak ] = ( double ) ( ( Array ) RangeArray ).GetValue( Row, 1 );
                            Abundances [ Peak ] = ( double ) ( ( Array ) RangeArray ).GetValue( Row, 2 );
                            if( MaxAbundance < Abundances [ Peak ] ) { MaxAbundance = Abundances [ Peak ]; }
                            S2Ns [ Peak ] = ( double ) ( ( Array ) RangeArray ).GetValue( Row, 3 );
                            Resolutions [ Peak ] = ( double ) ( ( Array ) RangeArray ).GetValue( Row, 4 );
                        }
                    } catch( Exception ex ) {
                        ExceptionMessage = ex.Message;
                    }
                    if( ExcelRange != null ) { CleanComObject( ExcelRange ); ExcelRange = null; }
                    if( ExcelSheet != null ) { CleanComObject( ExcelSheet ); ExcelSheet = null; }
                    if( ExcelBook != null ) { ExcelBook.Close( null, null, null ); CleanComObject( ExcelBook ); ExcelBook = null; }
                    if( ExcelApp != null ) { ExcelApp.Quit(); CleanComObject( ExcelApp ); ExcelApp = null; }
                    GC.Collect();
                    if( ExceptionMessage.Length != 0 ) {
                        throw new Exception( ExceptionMessage );
                    }
                } else if( FileExtension == ".xml" ) {
                    XmlDocument XmlDoc = new XmlDocument();
                    XmlDoc.Load( Filename );
                    //check Bruker instrument
                    XmlNodeList Nodes = XmlDoc.GetElementsByTagName( "fileinfo" );
                    if( Nodes.Count != 1 ) { throw new Exception( "fileinfo" ); }
                    if( Nodes [ 0 ].Attributes [ "appname" ].Value != "Bruker Compass DataAnalysis" ) { throw new Exception( "Bruker Compass DataAnalysis" ); }
                    //read peaks
                    XmlNodeList MsPeakNodes = XmlDoc.GetElementsByTagName( "ms_peaks" );
                    if( MsPeakNodes.Count != 1 ) { throw new Exception( "ms_peaks" ); }
                    XmlNode MsPeakNode = MsPeakNodes [ 0 ];
                    int PeakCount = MsPeakNode.ChildNodes.Count;
                    Masses = new double [ PeakCount ];
                    Abundances = new double [ PeakCount ];
                    S2Ns = new double [ PeakCount ];
                    Resolutions = new double [ PeakCount ];
                    RelAbundances = new double [ PeakCount ];
                    for( Peak = 0; Peak < PeakCount; Peak++ ) {
                        //<pk res="930674.5" algo="FTMS" fwhm="0.000218" a="102.53" sn="7.15" i="646225.1" mz="203.034719"/>
                        XmlAttributeCollection Attributes = MsPeakNode.ChildNodes [ Peak ].Attributes;
                        Masses [ Peak ] = Double.Parse( Attributes [ "mz" ].Value );
                        Abundances [ Peak ] = Double.Parse( Attributes [ "i" ].Value );
                        if( MaxAbundance < Abundances [ Peak ] ) { MaxAbundance = Abundances [ Peak ]; }
                        S2Ns [ Peak ] = Double.Parse( Attributes [ "sn" ].Value );
                        Resolutions [ Peak ] = Double.Parse( Attributes [ "res" ].Value );
                    }
                    XmlDoc = null;
                } else { //if( FileExtension == ".csv" || FileExtension == ".txt" ) { and unsupported
                    string [] Lines = File.ReadAllLines( Filename );
                    int PeakCount = Lines.Length - 1;
                    Masses = new double [ PeakCount ];
                    Abundances = new double [ PeakCount ];
                    S2Ns = new double [ PeakCount ];
                    Resolutions = new double [ PeakCount ];
                    RelAbundances = new double [ PeakCount ];
                    for( Peak = 0; Peak < Lines.Length - 1; Peak++ ) {
                        int Line = Peak + 1;
                        string [] LineParts = Lines [ Line ].Split( WordSeparators );
                        Masses [ Peak ] = Double.Parse( LineParts [ 0 ] );
                        Abundances [ Peak ] = Double.Parse( LineParts [ 1 ] );
                        if( MaxAbundance < Abundances [ Peak ] ) { MaxAbundance = Abundances [ Peak ]; }
                        if( LineParts.Length >= 3 ) {
                            S2Ns [ Peak ] = Double.Parse( LineParts [ 2 ] );
                        }
                        if( LineParts.Length >= 4 ) {
                            Resolutions [ Peak ] = Double.Parse( LineParts [ 3 ] );
                        }
                    }
                }
                if( MaxAbundance <= 0 ) { throw new Exception( "incorrect Abundances" ); }
                for( Peak = 0; Peak < RelAbundances.Length; Peak++ ) {
                    RelAbundances [ Peak ] = Abundances [ Peak ] / MaxAbundance;
                }
            } catch( Exception ex ) {
                if( Peak == -1 ) {
                    throw new Exception( "Error in File [" + Filename + "] is " + ex.Message );
                } else {
                    throw new Exception( "Error at Peak [" + Peak + "] in File [" + Filename + "] is " + ex.Message );
                }
            }
        }
        public static void ReadFile( string Filename, out InputData Data ) {
            Data = new InputData();
            ReadFile( Filename, out Data.Masses, out Data.Abundances, out Data.S2Ns, out Data.Resolutions, out Data.RelAbundances );
            Data.Chains = new Chain[ 0];
        }
        public enum ECutType{ MZ, Abundance, S2N, Resolution, RelAbundance};
        public class CutSettings{
            public ECutType CutType;
            public double Min;
            public double Max;
        };
        public static void CutData( InputData Data, out InputData OutData, CutSettings[] CutSettings){
            if ( Data.Masses.Length <= 0 ){ throw new Exception( "Input data is not correct" );}
            CutSettings MZCutSettings = null;
            CutSettings AbundanceCutSettings = null;
            CutSettings S2NCutSettings = null;
            CutSettings ResolutionCutSettings = null;
            CutSettings RelAbundanceSettings = null;
            foreach ( CutSettings TempCutSettings in CutSettings ) {
                switch ( TempCutSettings.CutType ) {
                    case ECutType.MZ:
                        if ( MZCutSettings == null ) {
                            MZCutSettings = TempCutSettings;
                        } else {
                            if ( MZCutSettings.Min < TempCutSettings.Min ) { MZCutSettings.Min = TempCutSettings.Min; }
                            if ( MZCutSettings.Max < TempCutSettings.Max ) { MZCutSettings.Max = TempCutSettings.Max; }
                        }
                        break;
                    case ECutType.Abundance:
                        if ( AbundanceCutSettings == null ) {
                            AbundanceCutSettings = TempCutSettings;
                        } else {
                            if ( AbundanceCutSettings.Min < TempCutSettings.Min ) { AbundanceCutSettings.Min = TempCutSettings.Min; }
                            if ( AbundanceCutSettings.Max < TempCutSettings.Max ) { AbundanceCutSettings.Max = TempCutSettings.Max; }
                        }
                        break;
                    case ECutType.S2N:
                        if ( S2NCutSettings == null ) {
                            S2NCutSettings = TempCutSettings;
                        } else {
                            if ( S2NCutSettings.Min < TempCutSettings.Min ) { S2NCutSettings.Min = TempCutSettings.Min; }
                            if ( S2NCutSettings.Max < TempCutSettings.Max ) { S2NCutSettings.Max = TempCutSettings.Max; }
                        }
                        break;
                    case ECutType.Resolution:
                        if ( ResolutionCutSettings == null ) {
                            ResolutionCutSettings = TempCutSettings;
                        } else {
                            if ( ResolutionCutSettings.Min < TempCutSettings.Min ) { ResolutionCutSettings.Min = TempCutSettings.Min; }
                            if ( ResolutionCutSettings.Max < TempCutSettings.Max ) { ResolutionCutSettings.Max = TempCutSettings.Max; }
                        }
                        break;
                    case ECutType.RelAbundance:
                        if ( RelAbundanceSettings == null ) {
                            RelAbundanceSettings = TempCutSettings;
                        } else {
                            if ( RelAbundanceSettings.Min < TempCutSettings.Min ) { RelAbundanceSettings.Min = TempCutSettings.Min; }
                            if ( RelAbundanceSettings.Max < TempCutSettings.Max ) { RelAbundanceSettings.Max = TempCutSettings.Max; }
                        }
                        break;
                }
            }
            if ( Data.S2Ns [ 0 ] <= 0 ) { S2NCutSettings = null; }
            if ( Data.Resolutions [ 0 ] <= 0 ) { ResolutionCutSettings = null; }
            bool [] NewData = new bool [ Data.Masses.Length ];
            int NewDataCount = 0;
            for ( int PeakIndex = 0; PeakIndex < Data.Masses.Length; PeakIndex++ ) {
                for ( ; ; ) {
                    if ( MZCutSettings != null ) {
                        if ( ( MZCutSettings.Min >= 0 ) && ( MZCutSettings.Min > Data.Masses [ PeakIndex ] ) ) break;
                        if ( ( MZCutSettings.Max >= 0 ) && ( MZCutSettings.Max < Data.Masses [ PeakIndex ] ) ) break;
                    }
                    if ( AbundanceCutSettings != null ) {
                        if ( ( AbundanceCutSettings.Min >= 0 ) && ( AbundanceCutSettings.Min > Data.Abundances [ PeakIndex ] ) ) break;
                        if ( ( AbundanceCutSettings.Max >= 0 ) && ( AbundanceCutSettings.Max < Data.Abundances [ PeakIndex ] ) ) break;
                    }
                    if ( S2NCutSettings != null ) {
                        if ( ( S2NCutSettings.Min >= 0 ) && ( S2NCutSettings.Min > Data.Abundances [ PeakIndex ] ) ) break;
                        if ( ( S2NCutSettings.Max >= 0 ) && ( S2NCutSettings.Max < Data.Abundances [ PeakIndex ] ) ) break;
                    }
                    if ( ResolutionCutSettings != null ) {
                        if ( ( ResolutionCutSettings.Min >= 0 ) && ( ResolutionCutSettings.Min > Data.Abundances [ PeakIndex ] ) ) break;
                        if ( ( ResolutionCutSettings.Max >= 0 ) && ( ResolutionCutSettings.Max < Data.Abundances [ PeakIndex ] ) ) break;
                    }
                    if ( RelAbundanceSettings != null ) {
                        if ( ( RelAbundanceSettings.Min >= 0 ) && ( RelAbundanceSettings.Min > Data.Abundances [ PeakIndex ] ) ) break;
                        if ( ( RelAbundanceSettings.Max >= 0 ) && ( RelAbundanceSettings.Max < Data.Abundances [ PeakIndex ] ) ) break;
                    }
                    NewData [ PeakIndex ] = true;
                    NewDataCount++;
                    break;
                }
            }
            OutData = new InputData();
            OutData.Masses = new double [ NewDataCount ];
            OutData.Abundances = new double [ NewDataCount ];
            OutData.S2Ns = new double [ NewDataCount ];
            OutData.Resolutions = new double [ NewDataCount ];
            OutData.RelAbundances = new double [ NewDataCount ];
            for ( int PeakIndex = 0, RealPeakIndex = 0; PeakIndex < Data.Masses.Length; PeakIndex++ ) {
                if ( NewData [ PeakIndex ] == true ) {
                    OutData.Masses [ RealPeakIndex ] = Data.Masses [ PeakIndex ];
                    OutData.Abundances [ RealPeakIndex ]= Data.Abundances [ PeakIndex ];
                    OutData.S2Ns [ RealPeakIndex ]= Data.S2Ns [ PeakIndex ];
                    OutData.Resolutions [ RealPeakIndex ]= Data.Resolutions [ PeakIndex ];
                    OutData.RelAbundances [ RealPeakIndex ] = Data.RelAbundances [ PeakIndex ];
                    RealPeakIndex++;
                }
            }
        }
        static void CleanComObject( object o ) {
            try {
                while( System.Runtime.InteropServices.Marshal.ReleaseComObject( o ) > 0 )
                    ;
            } catch { } finally {
                o = null;
            }
        }
    }
}

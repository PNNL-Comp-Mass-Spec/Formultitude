using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using Microsoft.Office.Interop.Excel;
using System.Xml;

namespace FileReader {
    public class FileReader {
        static char [] WordSeparators = new char [] { '\t', ',', ' ' };
        public static void ReadFile( string Filename, out double [] Masses, out double [] Abundances, out double [] SNs, out double [] Resolutions, out double [] RelAbundances ) {
            int Peak = -1;
            Masses = new double [ 0 ];
            Abundances = new double [ 0 ];
            SNs = new double [ 0 ];
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
                        // ExcelSheet = ExcelBook.Sheets [ 1 ];
                        ExcelSheet = (Worksheet)ExcelBook.Worksheets[1];
                        ExcelRange = ExcelSheet.UsedRange;
                        object RangeArray = ExcelRange.Value;
                        int PeakCount = ExcelRange.Rows.Count - 1;
                        Masses = new double [ PeakCount ];
                        Abundances = new double [ PeakCount ];
                        SNs = new double [ PeakCount ];
                        Resolutions = new double [ PeakCount ];
                        RelAbundances = new double [ PeakCount ];
                        for( Peak = 0; Peak < PeakCount; Peak++ ) {
                            int Row = Peak + 2;
                            ( ( Array ) RangeArray ).GetLength( 0 );
                            Masses [ Peak ] = ( double ) ( ( Array ) RangeArray ).GetValue( Row, 1 );
                            Abundances [ Peak ] = ( double ) ( ( Array ) RangeArray ).GetValue( Row, 2 );
                            if( MaxAbundance < Abundances [ Peak ] ) { MaxAbundance = Abundances [ Peak ]; }
                            SNs [ Peak ] = ( double ) ( ( Array ) RangeArray ).GetValue( Row, 3 );
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
                    SNs = new double [ PeakCount ];
                    Resolutions = new double [ PeakCount ];
                    RelAbundances = new double [ PeakCount ];
                    for( Peak = 0; Peak < PeakCount; Peak++ ) {
                        //<pk res="930674.5" algo="FTMS" fwhm="0.000218" a="102.53" sn="7.15" i="646225.1" mz="203.034719"/>
                        XmlAttributeCollection Attributes = MsPeakNode.ChildNodes [ Peak ].Attributes;
                        Masses [ Peak ] = Double.Parse( Attributes [ "mz" ].Value );
                        Abundances [ Peak ] = Double.Parse( Attributes [ "i" ].Value );
                        if( MaxAbundance < Abundances [ Peak ] ) { MaxAbundance = Abundances [ Peak ]; }
                        SNs [ Peak ] = Double.Parse( Attributes [ "sn" ].Value );
                        Resolutions [ Peak ] = Double.Parse( Attributes [ "res" ].Value );
                    }
                    XmlDoc = null;
                } else { //if( FileExtension == ".csv" || FileExtension == ".txt" ) { and unsupported
                    string [] Lines = File.ReadAllLines( Filename );
                    int PeakCount = Lines.Length - 1;
                    Masses = new double [ PeakCount ];
                    Abundances = new double [ PeakCount ];
                    SNs = new double [ PeakCount ];
                    Resolutions = new double [ PeakCount ];
                    RelAbundances = new double [ PeakCount ];
                    for( Peak = 0; Peak < Lines.Length - 1; Peak++ ) {
                        int Line = Peak + 1;
                        string [] LineParts = Lines [ Line ].Split( WordSeparators );
                        Masses [ Peak ] = Double.Parse( LineParts [ 0 ] );
                        Abundances [ Peak ] = Double.Parse( LineParts [ 1 ] );
                        if( MaxAbundance < Abundances [ Peak ] ) { MaxAbundance = Abundances [ Peak ]; }
                        if( LineParts.Length >= 3 ) {
                            SNs [ Peak ] = Double.Parse( LineParts [ 2 ] );
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

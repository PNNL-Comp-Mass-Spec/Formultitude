using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.IO;
using System.Collections;

using System.Collections.Concurrent;
//using System.Threading;
//using System.Threading.Tasks;
//using System.Data;
using System.Runtime.InteropServices;
using Microsoft.Office.Interop.Excel;
using System.Xml;

namespace FindChains {
    public partial class FindChainsForm : Form {
        public char [] WordSeparators = new char [] { '\t', ',', ' ' };
        public FindChainsForm() {
            InitializeComponent();
            numericUpDownFrequencyError.Enabled = checkBoxFrequency.Checked;
        }
        private void FindChainsForm_DragEnter( object sender, DragEventArgs e ) {
            if( e.Data.GetDataPresent( DataFormats.FileDrop ) == true ) {
                e.Effect = DragDropEffects.Copy;
            }
        }

        //PeakIndex
        double [] Masses;
        double [] Abundances;
        double [] SNs;
        double [] Resolutions;
        double [] RelAbundances;

        class PeakLink {
            public int BinIndex;
            public int PairIndex;
        }
        //BinIndex + PairIndex + LinkIndex
        List<int> [] BinLeftPeaks;
        List<int> [] BinRightPeaks;
        List<PeakLink> [] [] BinLeftLinks;
        List<PeakLink> [] [] BinRightLinks;

        List<int> CreateChain( PeakLink oPeakLink ) {
            int BinIndex = oPeakLink.BinIndex;
            int PairIndex = oPeakLink.PairIndex;
            if( (BinRightLinks[ BinIndex].Length == 0) || ( BinRightLinks[ BinIndex] [ PairIndex].Count == 0) ){
                List<int> StartChain = new List<int>();
                StartChain.Add( BinRightPeaks [ BinIndex ] [ PairIndex ] );
                return StartChain;
            }
            PeakLink NextLink = BinRightLinks[ BinIndex] [ PairIndex][ 0];
            List<int> TempChain = CreateChain( NextLink );
            TempChain.Insert( 0, BinRightPeaks [ BinIndex ] [ PairIndex ] );
            return TempChain;
        }
        const double PPM = 1e6;//parts per million
        public double PpmToError( double Mass, double ErrorPPM ) { return Mass * ErrorPPM / PPM; }
        public double SignedMassErrorPPM( double ReferenceMass, double Mass ) { return ( Mass - ReferenceMass ) / ReferenceMass * PPM; }
        public double AbsMassErrorPPM( double ReferenceMass, double Mass ) { return Math.Abs( ( Mass - ReferenceMass ) / ReferenceMass * PPM ); }
        public double LeftPpmMass( double Mass, double PpmError ) { return Mass / ( 1 + PpmError / PPM ); }
        public double RightPpmMass( double Mass, double PpmError ) { return Mass * ( 1 + PpmError / PPM ); }

        private void checkBoxFrequency_CheckedChanged( object sender, EventArgs e ) {
            numericUpDownFrequencyError.Enabled = checkBoxFrequency.Checked;
        }
        private void FindChainsForm_DragDrop( object sender, DragEventArgs e ) {
            string [] Filenames = ( string [] ) e.Data.GetData( DataFormats.FileDrop );
            //read from files
            for( int FileIndex = 0; FileIndex < Filenames.Length; FileIndex++ ) {
                string FileExtension = Path.GetExtension( Filenames [ FileIndex ] );
                if( FileExtension == ".csv" || FileExtension == ".txt" ) {
                    string [] Lines = File.ReadAllLines( Filenames [ FileIndex ] );
                    int PeakCount = Lines.Length - 1;
                    Masses = new double [ PeakCount ];
                    Abundances = new double [ PeakCount ];
                    SNs = new double [ PeakCount ];
                    Resolutions = new double [ PeakCount ];
                    RelAbundances = new double [ PeakCount ];
                    for( int Line = 1; Line < Lines.Length; Line++ ) {
                        string [] LineParts = Lines [ Line ].Split( WordSeparators, StringSplitOptions.RemoveEmptyEntries );
                        int Peak = Line - 1;
                        Masses [ Peak ] = Double.Parse( LineParts [ 0 ] );
                        Abundances [ Peak ] = Double.Parse( LineParts [ 1 ] );
                        if( LineParts.Length == 5 ) {
                            SNs [ Peak ] = Double.Parse( LineParts [ 2 ] );
                            Resolutions [ Peak ] = Double.Parse( LineParts [ 3 ] );
                            RelAbundances [ Peak ] = Double.Parse( LineParts [ 4 ] );
                        }
                    }
                } else if( FileExtension == ".xlsx" || FileExtension == ".xls" ) {
                    Microsoft.Office.Interop.Excel.Application MyApp = new Microsoft.Office.Interop.Excel.Application();
                    MyApp.Visible = false;
                    Microsoft.Office.Interop.Excel.Workbook MyBook = MyApp.Workbooks.Open( Filenames [ FileIndex ] );
                    Microsoft.Office.Interop.Excel.Worksheet MySheet = MyBook.Sheets [ 1 ];
                    Microsoft.Office.Interop.Excel.Range MyRange = MySheet.UsedRange;
                    object RangeArray = MyRange.Value;
                    int PeakCount = MyRange.Rows.Count - 1;
                    Masses = new double [ PeakCount ];
                    Abundances = new double [ PeakCount ];
                    SNs = new double [ PeakCount ];
                    Resolutions = new double [ PeakCount ];
                    RelAbundances = new double [ PeakCount ];
                    for( int Peak = 0; Peak < PeakCount; Peak++ ) {
                        int Row = Peak + 2;
                        Masses [ Peak ] = ( double ) ( ( Array ) RangeArray ).GetValue( Row, 1 );
                        Abundances [ Peak ] = ( double ) ( ( Array ) RangeArray ).GetValue( Row, 2 );
                        SNs [ Peak ] = ( double ) ( ( Array ) RangeArray ).GetValue( Row, 3 );
                        Resolutions [ Peak ] = ( double ) ( ( Array ) RangeArray ).GetValue( Row, 4 );
                        RelAbundances [ Peak ] = ( double ) ( ( Array ) RangeArray ).GetValue( Row, 5 );
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
                } else if( FileExtension == ".xml" ) {
                    XmlDocument XmlDoc = new XmlDocument();
                    XmlDoc.Load( Filenames [ FileIndex ] );
                    //check Bruker instrument
                    XmlNodeList Nodes = XmlDoc.GetElementsByTagName( "fileinfo" );
                    if( Nodes.Count != 1 ) { continue; }
                    if( Nodes [ 0 ].Attributes [ "appname" ].Value != "Bruker Compass DataAnalysis" ) { continue; }
                    //read peaks
                    XmlNodeList MsPeakNodes = XmlDoc.GetElementsByTagName( "ms_peaks" );
                    if( MsPeakNodes.Count != 1 ) { continue; }
                    XmlNode MsPeakNode = MsPeakNodes [ 0 ];
                    int PeakCount = MsPeakNode.ChildNodes.Count;
                    Masses = new double [ PeakCount ];
                    Abundances = new double [ PeakCount ];
                    SNs = new double [ PeakCount ];
                    Resolutions = new double [ PeakCount ];
                    RelAbundances = new double [ PeakCount ];
                    double MaxAbundance = 0;
                    for( int Peak = 0; Peak < PeakCount; Peak++ ) {
                        //<pk res="930674.5" algo="FTMS" fwhm="0.000218" a="102.53" sn="7.15" i="646225.1" mz="203.034719"/>
                        XmlAttributeCollection Attributes = MsPeakNode.ChildNodes [ Peak ].Attributes;
                        Masses [ Peak ] = Double.Parse( Attributes [ "mz" ].Value );
                        Abundances [ Peak ] = Double.Parse( Attributes [ "i" ].Value );
                        if( MaxAbundance < Abundances [ Peak ] ) { MaxAbundance = Abundances [ Peak ]; }
                        SNs [ Peak ] = Double.Parse( Attributes [ "sn" ].Value );
                        Resolutions [ Peak ] = Double.Parse( Attributes [ "res" ].Value );
                    }
                    for( int Peak = 0; Peak < PeakCount; Peak++ ) {
                        RelAbundances [ Peak ] = Abundances [ Peak ] / MaxAbundance;
                    }
                    XmlDoc = null;
                } else {
                    //???unsupported format
                    continue;
                }

                string Filename = Path.GetDirectoryName( Filenames [ FileIndex ] ) + "\\" + Path.GetFileNameWithoutExtension( Filenames [ FileIndex ] );
                if( checkBoxPPMProcess.Checked == true ) {
                    List<double> LChainDistances = new List<double>( Masses.Length * ( Masses.Length - 1 ) );
                    List<List<int>> LChains = new List<List<int>>( Masses.Length * ( Masses.Length - 1 ) );

                    double MaxChainStartMass = ( double ) numericUpDownMaxPeakToStartChain.Value;
                    int MinPeaksInChain = ( int ) numericUpDownMinPeaksInChain.Value;
                    double PpmError = ( double ) numericUpDownPpmError.Value;

                    for( int PeakIndex = 0; PeakIndex < Masses.Length - 1; PeakIndex++ ) {
                        if( Masses [ PeakIndex ] > MaxChainStartMass ) { break; }
                        double MaxDistance = ( Masses [ Masses.Length - 1 ] - Masses [ PeakIndex ] ) / ( MinPeaksInChain - 1 );
                        double MaxMass = Masses [ PeakIndex ] + MaxDistance + PpmToError( Masses [ PeakIndex ], PpmError );

                        for( int NextPeakIndex = PeakIndex + 1; NextPeakIndex < Masses.Length; NextPeakIndex++ ) {
                            if( Masses [ NextPeakIndex ] > MaxMass ) { break; }
                            List<int> Chain = new List<int>();
                            Chain.Add( PeakIndex );
                            Chain.Add( NextPeakIndex );
                            double Distance = Masses [ NextPeakIndex ] - Masses [ PeakIndex ];
                            double ChainLastPeakMass = Masses [ NextPeakIndex ];
                            for( ; ; ) {
                                double NextMass = ChainLastPeakMass + Distance;
                                double LeftMass = LeftPpmMass( NextMass, PpmError );
                                int Index = Array.BinarySearch( Masses, LeftMass );
                                //int Index = Array.BinarySearch( Masses, PeakIndex, Masses.Length - PeakIndex - 1, LeftMass );
                                if( Index < 0 ) { Index = ~Index; }
                                if( Index >= Masses.Length ) { break; }
                                double RightMass = RightPpmMass( NextMass, PpmError );
                                if( Masses [ Index ] <= RightMass ) {
                                    Chain.Add( Index );
                                    ChainLastPeakMass = Masses [ Index ];
                                } else {
                                    break;
                                }
                            }
                            if( Chain.Count >= MinPeaksInChain ) {
                                LChainDistances.Add( Distance );
                                int ChainIndex = LChainDistances.Count - 1;
                                LChains.Add( Chain );
                            }
                        }
                    }

                    double [] ChainDistances = LChainDistances.ToArray();
                    List<int> [] Chains = LChains.ToArray();
                    Array.Sort( ChainDistances, Chains );

                    StreamWriter oStreamWriterChains = new StreamWriter( Filename + "PpmChains.csv" );
                    oStreamWriterChains.WriteLine( "Distance,Count,Mass" );
                    for( int LineIndex = 0; LineIndex < ChainDistances.Length; LineIndex++ ) {
                        string Line = ChainDistances [ LineIndex ].ToString() + "," + Chains [ LineIndex ].Count;
                        foreach( int PeakIndex in Chains [ LineIndex ] ) {
                            Line = Line + "," + Masses [ PeakIndex ];
                        }
                        oStreamWriterChains.WriteLine( Line );
                    }
                    oStreamWriterChains.Close();

                    //remove secondary chains
                    bool [] DublicatedChains = new bool [ Chains.Length ];
                    for( int ChainIndex = 0; ChainIndex < Chains.Length - 1; ChainIndex++ ) {
                        if( DublicatedChains [ ChainIndex ] == true ) { continue; }
                        double Distance = ChainDistances [ ChainIndex ];
                        List<int> Chain = Chains [ ChainIndex ];
                        for( int DistanceGap = 1; ; DistanceGap++ ) {
                            //check chain availibity on min peaks in chain
                            double MinPeaksMaxMass = Masses [ Chain [ 0 ] ] + Distance * DistanceGap * ( MinPeaksInChain - 1 );
                            if( Masses [ Masses.Length - 1 ] < LeftPpmMass( MinPeaksMaxMass, 2 * PpmError ) ) {
                                break;
                            }
                            //find distance error max based on mass of last peak in chain
                            double MaxMassInChain = ( Masses [ Chain [ 0 ] ] + Masses [ Chain [ Chain.Count - 1 ] ] - Masses [ Chain [ 0 ] ] ) * DistanceGap;
                            double MaxError = PpmToError( MaxMassInChain, 2 * PpmError );
                            double LeftDistance = Distance * DistanceGap - MaxError;
                            //calculate min distance
                            int LeftDistanceIndex = Array.BinarySearch( ChainDistances, LeftDistance );
                            //int LeftDistanceIndex = Array.BinarySearch( ChainDistances, ChainIndex + 1, Chains.Length - ChainIndex - 1, LeftDistance );
                            if( LeftDistanceIndex < 0 ) { LeftDistanceIndex = ~LeftDistanceIndex; }
                            if( LeftDistanceIndex <= ChainIndex ) { LeftDistanceIndex = ChainIndex + 1; }//in case DistanceGap == 1
                            if( LeftDistanceIndex >= ChainDistances.Length ) { break; }
                            //search
                            double RightDistance = Distance * DistanceGap + MaxError;
                            for( int CompareChainIndex = LeftDistanceIndex; CompareChainIndex < ChainDistances.Length; CompareChainIndex++ ) {
                                if( ChainDistances [ CompareChainIndex ] > RightDistance ) { break; }
                                if( DublicatedChains [ CompareChainIndex ] == true ) { continue; }
                                List<int> CompareChain = Chains [ CompareChainIndex ];

                                //compare chains
                                for( int Index = 0; Index < Chain.Count; Index++ ) {
                                    int PeakIndex = Chain [ Index ];
                                    int CompareIndex = Array.BinarySearch( CompareChain.ToArray(), PeakIndex );
                                    if( CompareIndex >= 0 ) {
                                        int NextIndex = Index + DistanceGap;
                                        int NextCompareIndex = CompareIndex + 1;
                                        if( ( ( NextIndex < Chain.Count ) && ( NextCompareIndex < CompareChain.Count ) ) == false ) {
                                            break;
                                        }
                                        bool TheSameChains = ( Chain [ NextIndex ] == CompareChain [ NextCompareIndex ] );
                                        if( TheSameChains == false ) {
                                            //second try
                                            NextIndex = Index + DistanceGap * 2;
                                            NextCompareIndex = CompareIndex + 2;
                                            if( ( ( NextIndex < Chain.Count ) && ( NextCompareIndex < CompareChain.Count ) ) == false ) {
                                                break;
                                            }
                                            TheSameChains = ( Chain [ NextIndex ] == CompareChain [ NextCompareIndex ] );
                                        }
                                        if( TheSameChains == false ) { break; }
                                        //mark secondary chain
                                        if( DistanceGap > 1 ) {
                                            DublicatedChains [ CompareChainIndex ] = true;
                                            //include peaks?
                                        } else {
                                            if( Chain.Count >= CompareChain.Count ) {
                                                DublicatedChains [ CompareChainIndex ] = true;
                                            } else {
                                                DublicatedChains [ ChainIndex ] = true;
                                            }
                                        }
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    //remove secondary chains
                    LChains.Clear();
                    LChainDistances.Clear();
                    for( int ChainIndex = 0; ChainIndex < Chains.Length; ChainIndex++ ) {
                        if( DublicatedChains [ ChainIndex ] == false ) {
                            LChains.Add( Chains [ ChainIndex ] );
                            LChainDistances.Add( ChainDistances [ ChainIndex ] );
                        }
                    }
                    //save result
                    if( checkBoxFileFormatPeakIndex.Checked == true ) {
                        StreamWriter oStreamWriterPrimaryChains = new StreamWriter( Filename + "PrimaryChainsIndex.csv" );
                        oStreamWriterPrimaryChains.WriteLine( "Distance,Count,Index" );
                        for( int LineIndex = 0; LineIndex < LChainDistances.Count; LineIndex++ ) {
                            string Line = LChainDistances [ LineIndex ].ToString() + "," + LChains [ LineIndex ].Count;
                            foreach( int PeakIndex in LChains [ LineIndex ] ) {
                                Line = Line + "," + PeakIndex;
                            }
                            oStreamWriterPrimaryChains.WriteLine( Line );
                        }
                        oStreamWriterPrimaryChains.Close();
                    }
                    if( checkBoxFileFormatPeakMass.Checked == true ) {
                        StreamWriter oStreamWriterPrimaryChains = new StreamWriter( Filename + "PrimaryChainsMass.csv" );
                        oStreamWriterPrimaryChains.WriteLine( "Distance,Count,Mass" );
                        for( int LineIndex = 0; LineIndex < LChainDistances.Count; LineIndex++ ) {
                            string Line = LChainDistances [ LineIndex ].ToString() + "," + LChains [ LineIndex ].Count;
                            foreach( int PeakIndex in LChains [ LineIndex ] ) {
                                Line = Line + "," + Masses [ PeakIndex ];
                            }
                            oStreamWriterPrimaryChains.WriteLine( Line );
                        }
                        oStreamWriterPrimaryChains.Close();
                    }
                    //Combine distances                
                    if( checkBoxFrequency.Checked == true){
                        List <int> LDistancePeaks = new List<int> ( LChains.Count);
                        for( int ChainIndex = 0; ChainIndex < LChains.Count; ChainIndex++ ) {
                            LDistancePeaks.Add( LChains [ ChainIndex ].Count);
                        }
                        int TotalChains = LChains.Count;
                        double DistanceError = ( double ) numericUpDownFrequencyError.Value;
                        for( int ChainIndex = 1; ChainIndex < TotalChains - 2; ) {
                            double PreviousDistance = LChainDistances [ ChainIndex - 1 ];
                            double Distance = LChainDistances [ ChainIndex ];
                            double LeftError = Distance - PreviousDistance;
                            if( LeftError > DistanceError ) { ChainIndex++; }
                            double NextDistance = LChainDistances [ ChainIndex + 1 ];
                            double RightError = NextDistance - Distance;
                            if( LeftError <= RightError ) {
                                //combine PreviousChain and Chain
                                LDistancePeaks [ ChainIndex - 1 ] = LDistancePeaks [ ChainIndex - 1 ] + LDistancePeaks [ ChainIndex ];
                                LChainDistances [ ChainIndex - 1 ] = PreviousDistance + ( Distance - PreviousDistance ) * LDistancePeaks [ ChainIndex ] / LDistancePeaks [ ChainIndex - 1 ];
                                LDistancePeaks.RemoveAt( ChainIndex );
                                LChainDistances.RemoveAt( ChainIndex );
                                TotalChains--;
                            } else {//LeftError > RightError
                                //combine Chain and NextChain
                                LDistancePeaks [ ChainIndex ] = LDistancePeaks [ ChainIndex ] + LDistancePeaks [ ChainIndex + 1 ];
                                LChainDistances [ ChainIndex ] = Distance + ( NextDistance - Distance ) * LDistancePeaks [ ChainIndex + 1 ] / LDistancePeaks [ ChainIndex ];
                                LDistancePeaks.RemoveAt( ChainIndex + 1 );
                                LChainDistances.RemoveAt( ChainIndex + 1 );
                                TotalChains--;
                            }
                        }
                        StreamWriter oStreamWriterPrimaryChains = new StreamWriter( Filename + "AlignedDistances.csv" );
                        oStreamWriterPrimaryChains.WriteLine( "Distance,Peaks" );
                        for( int LineIndex = 0; LineIndex < LDistancePeaks.Count; LineIndex++ ) {
                            string Line = LChainDistances [ LineIndex ].ToString() + "," + LDistancePeaks [ LineIndex ];
                            oStreamWriterPrimaryChains.WriteLine( Line );
                        }
                        oStreamWriterPrimaryChains.Close();
                    }                    
                }
                if( checkBoxAmuProcess.Checked == true ) {
                    CalculatePpmError( Masses );
                    double Error = ( double ) numericUpDownAbsError.Value;
                    double RangeMin = ( double ) numericUpDownRangeMin.Value;
                    double RangeMax = ( double ) numericUpDownRangeMax.Value;

                    int BinsPerErrorRange = ( int ) numericUpDownBinsPerErrorRange.Value;
                    int MinPeaksInChain = ( int ) numericUpDownMinPeaksInChain.Value;

                    //create Bin distances ( = left and right peaks)           
                    double BinSize = Error / BinsPerErrorRange;
                    int BinCount = ( int ) Math.Ceiling( ( RangeMax - RangeMin ) / BinSize );
                    BinLeftPeaks = new List<int> [ BinCount ];
                    BinRightPeaks = new List<int> [ BinCount ];
                    for( int BinIndex = 0; BinIndex < BinCount; BinIndex++ ) {
                        BinLeftPeaks [ BinIndex ] = new List<int>();
                        BinRightPeaks [ BinIndex ] = new List<int>();
                    }
                    for( int Row = 0; Row < Masses.Length; Row++ ) {
                        double RowMass = Masses [ Row ];
                        for( int Column = Row + 1; Column < Masses.Length; Column++ ) {
                            double Distance = Masses [ Column ] - RowMass;
                            if( Distance < RangeMin ) { continue; }
                            if( Distance >= RangeMax ) { break; }
                            int BinIndex = ( int ) Math.Floor( ( Distance - RangeMin ) / BinSize );
                            BinLeftPeaks [ BinIndex ].Add( Row );
                            BinRightPeaks [ BinIndex ].Add( Column );
                        }
                    }

                    //create Bin links
                    BinLeftLinks = new List<PeakLink> [ BinCount ] [];
                    BinRightLinks = new List<PeakLink> [ BinCount ] [];
                    for( int BinIndex = 0; BinIndex < BinCount; BinIndex++ ) {
                        BinLeftLinks [ BinIndex ] = new List<PeakLink> [ BinLeftPeaks [ BinIndex ].Count ];
                        BinRightLinks [ BinIndex ] = new List<PeakLink> [ BinLeftPeaks [ BinIndex ].Count ];
                        for( int PairIndex = 0; PairIndex < BinLeftPeaks [ BinIndex ].Count; PairIndex++ ) {
                            BinRightLinks [ BinIndex ] [ PairIndex ] = new List<PeakLink>();
                            BinLeftLinks [ BinIndex ] [ PairIndex ] = new List<PeakLink>();
                        }
                    }
                    for( int BinIndex = 0; BinIndex < BinCount; BinIndex++ ) {
                        if( BinLeftPeaks [ BinIndex ].Count == 0 ) { continue; }//BinRightPeaks[ BinIndex ].Count == 0 must

                        int MaxBinIndex = BinIndex + 2 * BinsPerErrorRange + 1;//??? 2 *
                        if( MaxBinIndex >= BinCount ) { MaxBinIndex = BinCount - 1; }

                        for( int PairIndex = 0; PairIndex < BinLeftPeaks [ BinIndex ].Count; PairIndex++ ) {
                            int PairLeftPeak = BinLeftPeaks [ BinIndex ] [ PairIndex ];
                            int PairRightPeak = BinRightPeaks [ BinIndex ] [ PairIndex ];
                            for( int LinkBinIndex = BinIndex; LinkBinIndex < MaxBinIndex; LinkBinIndex++ ) {
                                int LinkPairIndex = 0;
                                if( BinIndex == LinkBinIndex ) {
                                    LinkPairIndex = PairIndex + 1;
                                }
                                for( ; LinkPairIndex < BinLeftPeaks [ LinkBinIndex ].Count; LinkPairIndex++ ) {
                                    if( PairLeftPeak == BinRightPeaks [ LinkBinIndex ] [ LinkPairIndex ] ) {
                                        PeakLink ToLeftLink = new PeakLink();
                                        ToLeftLink.BinIndex = LinkBinIndex;
                                        ToLeftLink.PairIndex = LinkPairIndex;
                                        BinLeftLinks [ BinIndex ] [ PairIndex ].Add( ToLeftLink );

                                        PeakLink ToRightLink = new PeakLink();
                                        ToRightLink.BinIndex = BinIndex;
                                        ToRightLink.PairIndex = PairIndex;
                                        BinRightLinks [ LinkBinIndex ] [ LinkPairIndex ].Add( ToRightLink );
                                    }

                                    if( PairRightPeak == BinLeftPeaks [ LinkBinIndex ] [ LinkPairIndex ] ) {
                                        PeakLink ToRightLink = new PeakLink();
                                        ToRightLink.BinIndex = LinkBinIndex;
                                        ToRightLink.PairIndex = LinkPairIndex;
                                        BinRightLinks [ BinIndex ] [ PairIndex ].Add( ToRightLink );

                                        PeakLink ToLeftLink = new PeakLink();
                                        ToLeftLink.BinIndex = BinIndex;
                                        ToLeftLink.PairIndex = PairIndex;
                                        BinLeftLinks [ LinkBinIndex ] [ LinkPairIndex ].Add( ToLeftLink );
                                    }
                                }
                            }
                        }
                    }
                    if( checkBoxBinLinks.Checked == true ) {
                        StreamWriter oStreamWriterBinLinks = new StreamWriter( Filename + "BinLinks.csv" );
                        oStreamWriterBinLinks.WriteLine( "Bin,Min,PairPeaks" );
                        for( int BinIndex = 0; BinIndex < BinCount; BinIndex++ ) {
                            string Line = BinIndex.ToString() + "," + ( RangeMin + BinSize * BinIndex );
                            for( int PairIndex = 0; PairIndex < BinRightLinks [ BinIndex ].Length; PairIndex++ ) {
                                Line = Line + "," + BinLeftPeaks [ BinIndex ] [ PairIndex ] + "_" + BinRightPeaks [ BinIndex ] [ PairIndex ];
                            }
                            oStreamWriterBinLinks.WriteLine( Line );
                        }
                        oStreamWriterBinLinks.Close();
                    }

                    List<List<int>> Chains = new List<List<int>>();
                    for( int BinIndex = 0; BinIndex < BinCount; BinIndex++ ) {
                        //double BinDistance = RangeMin + BinIndex * BinSize;
                        for( int PairIndex = 0; PairIndex < BinLeftPeaks [ BinIndex ].Count; PairIndex++ ) {
                            if( BinLeftLinks [ BinIndex ] [ PairIndex ].Count != 0 ) { continue; }//chain starts when left peak doesn't have link    
                            if( BinRightLinks [ BinIndex ] [ PairIndex ].Count == 0 ) { continue; }//chain doesn't start when right peak doesn't have link   
                            for( int LinkIndex = 0; LinkIndex < BinRightLinks [ BinIndex ] [ PairIndex ].Count; LinkIndex++ ) {
                                if( LinkIndex > 0 ) { break; }//??? take only first
                                PeakLink temp = BinRightLinks [ BinIndex ] [ PairIndex ] [ LinkIndex ];
                                List<int> Chain = CreateChain( temp );
                                Chain.Insert( 0, BinLeftPeaks [ BinIndex ] [ PairIndex ] );
                                Chain.Insert( 1, BinRightPeaks [ BinIndex ] [ PairIndex ] );
                                if( Chain.Count < MinPeaksInChain ) { continue; }
                                //bool Good = true;
                                //for( int Index = 1; Index < Chain.Count; Index++ ) {
                                //    if( Math.Abs( Masses[ Chain [ Index ] ] - Masses[ Chain [ Index - 1 ] ] - BinDistance) > Error) {
                                //        Good = false;
                                //        break;
                                //    }
                                //}
                                //if( Good == false ) { continue; }
                                Chains.Add( Chain );
                            }
                        }
                    }

                    if( checkBoxFileFormatPeakIndex.Checked == true ) {
                        StreamWriter oStreamWriterChains = new StreamWriter( Filename + "ChainsAmuIndex.csv" );
                        oStreamWriterChains.WriteLine( "Distance,MaxError,Indexes" );
                        for( int ChainIndex = 0; ChainIndex < Chains.Count; ChainIndex++ ) {
                            List<int> Chain = Chains [ ChainIndex ];
                            double Distance = Masses [ Chain [ 1 ] ] - Masses [ Chain [ 0 ] ];
                            string Line = Distance.ToString();
                            foreach( int Index in Chain ) {
                                Line = Line + "," + Index;
                            }
                            oStreamWriterChains.WriteLine( Line );
                        }
                        oStreamWriterChains.Close();
                    }
                    if( checkBoxFileFormatPeakMass.Checked == true ) {
                        StreamWriter oStreamWriterChains = new StreamWriter( Filename + "ChainsAmuMass.csv" );
                        oStreamWriterChains.WriteLine( "Distance,MaxError,Mass" );
                        for( int ChainIndex = 0; ChainIndex < Chains.Count; ChainIndex++ ) {
                            List<int> Chain = Chains [ ChainIndex ];
                            double Distance =  Masses [ Chain [ 1 ] ] - Masses [ Chain [ 0 ] ];
                            string Line = Distance.ToString();
                            foreach( int Index in Chain ) {
                                Line = Line + "," + Masses [ Index ];
                            }
                            oStreamWriterChains.WriteLine( Line );
                        }
                        oStreamWriterChains.Close();
                    }
                    if( checkBoxFileFormatPeakAbundance.Checked == true ) {
                        StreamWriter oStreamWriterChains = new StreamWriter( Filename + "ChainsAmuAbun.csv" );
                        oStreamWriterChains.WriteLine( "Distance,MaxError,Abundance" );
                        for( int ChainIndex = 0; ChainIndex < Chains.Count; ChainIndex++ ) {
                            List<int> Chain = Chains [ ChainIndex ];
                            double Distance = Masses [ Chain [ 1 ] ] - Masses [ Chain [ 0 ] ];
                            string Line = Distance.ToString();
                            foreach( int Peak in Chain ) {
                                Line = Line + "," + Abundances [ Peak ];
                            }
                            oStreamWriterChains.WriteLine( Line );
                        }
                        oStreamWriterChains.Close();
                    }
                }
            }
        }
        public double CalculatePpmError( double [] Masses ) {
            int MassesCount = Masses.Length;
            double RangeMin = 0;
            int MinBlockPeaksInSpectrum = 5;
            double RangeMax = Masses [ MassesCount - 1 ] / MinBlockPeaksInSpectrum;
            double Error = 0.0002;
            int BinsPerErrorRange = 5;
            double BinSize = Error / BinsPerErrorRange;
            int BinCount = ( int ) Math.Ceiling( ( RangeMax - RangeMin ) / BinSize );
            BinLeftPeaks = new List<int> [ BinCount ];
            BinRightPeaks = new List<int> [ BinCount ];
            for( int BinIndex = 0; BinIndex < BinCount; BinIndex++ ) {
                BinLeftPeaks [ BinIndex ] = new List<int>();
                BinRightPeaks [ BinIndex ] = new List<int>();
            }
            for( int Row = 0; Row < Masses.Length; Row++ ) {
                double RowMass = Masses [ Row ];
                for( int Column = Row + 1; Column < Masses.Length; Column++ ) {
                    double Distance = Masses [ Column ] - RowMass;
                    if( Distance < RangeMin ) { continue; }
                    if( Distance >= RangeMax ) { break; }
                    int BinIndex = ( int ) Math.Floor( ( Distance - RangeMin ) / BinSize );
                    BinLeftPeaks [ BinIndex ].Add( Row );
                    BinRightPeaks [ BinIndex ].Add( Column );
                }
            }

            //create Bin links
            BinLeftLinks = new List<PeakLink> [ BinCount ] [];
            BinRightLinks = new List<PeakLink> [ BinCount ] [];
            for( int BinIndex = 0; BinIndex < BinCount; BinIndex++ ) {
                BinLeftLinks [ BinIndex ] = new List<PeakLink> [ BinLeftPeaks [ BinIndex ].Count ];
                BinRightLinks [ BinIndex ] = new List<PeakLink> [ BinLeftPeaks [ BinIndex ].Count ];
                for( int PairIndex = 0; PairIndex < BinLeftPeaks [ BinIndex ].Count; PairIndex++ ) {
                    BinRightLinks [ BinIndex ] [ PairIndex ] = new List<PeakLink>();
                    BinLeftLinks [ BinIndex ] [ PairIndex ] = new List<PeakLink>();
                }
            }
            for( int BinIndex = 0; BinIndex < BinCount; BinIndex++ ) {
                if( BinLeftPeaks [ BinIndex ].Count == 0 ) { continue; }//BinRightPeaks[ BinIndex ].Count == 0 must

                int MaxBinIndex = BinIndex + 2 * BinsPerErrorRange + 1;//??? 2 *
                if( MaxBinIndex >= BinCount ) { MaxBinIndex = BinCount - 1; }

                for( int PairIndex = 0; PairIndex < BinLeftPeaks [ BinIndex ].Count; PairIndex++ ) {
                    int PairLeftPeak = BinLeftPeaks [ BinIndex ] [ PairIndex ];
                    int PairRightPeak = BinRightPeaks [ BinIndex ] [ PairIndex ];
                    for( int LinkBinIndex = BinIndex; LinkBinIndex < MaxBinIndex; LinkBinIndex++ ) {
                        int LinkPairIndex = 0;
                        if( BinIndex == LinkBinIndex ) {
                            LinkPairIndex = PairIndex + 1;
                        }
                        for( ; LinkPairIndex < BinLeftPeaks [ LinkBinIndex ].Count; LinkPairIndex++ ) {
                            if( PairLeftPeak == BinRightPeaks [ LinkBinIndex ] [ LinkPairIndex ] ) {
                                PeakLink ToLeftLink = new PeakLink();
                                ToLeftLink.BinIndex = LinkBinIndex;
                                ToLeftLink.PairIndex = LinkPairIndex;
                                BinLeftLinks [ BinIndex ] [ PairIndex ].Add( ToLeftLink );

                                PeakLink ToRightLink = new PeakLink();
                                ToRightLink.BinIndex = BinIndex;
                                ToRightLink.PairIndex = PairIndex;
                                BinRightLinks [ LinkBinIndex ] [ LinkPairIndex ].Add( ToRightLink );
                            }

                            if( PairRightPeak == BinLeftPeaks [ LinkBinIndex ] [ LinkPairIndex ] ) {
                                PeakLink ToRightLink = new PeakLink();
                                ToRightLink.BinIndex = LinkBinIndex;
                                ToRightLink.PairIndex = LinkPairIndex;
                                BinRightLinks [ BinIndex ] [ PairIndex ].Add( ToRightLink );

                                PeakLink ToLeftLink = new PeakLink();
                                ToLeftLink.BinIndex = BinIndex;
                                ToLeftLink.PairIndex = PairIndex;
                                BinLeftLinks [ LinkBinIndex ] [ LinkPairIndex ].Add( ToLeftLink );
                            }
                        }
                    }
                }
            }

            //Save BinLinks to file
            string FilenameBinLinks =  "c:\\temp\\BinLinks.csv";
            StreamWriter oStreamWriterBinLinks = new StreamWriter( FilenameBinLinks );
            oStreamWriterBinLinks.WriteLine( "Block size,Counts" );                        
            for( int BinIndex = 0; BinIndex < BinCount; BinIndex++ ) {
                oStreamWriterBinLinks.WriteLine( ( RangeMin + BinSize * BinIndex + BinSize / 2 ).ToString() + "," +   BinLeftLinks [ BinIndex ].Length  );
            }
            oStreamWriterBinLinks.Close();

            //Max peak
            int MaxBinCount = BinLeftPeaks [ 0].Count;
            int MaxBinIndex1 = 0;
            for( int BinIndex = 1; BinIndex < BinCount; BinIndex++ ) {
                if( MaxBinCount < BinLeftPeaks [ BinIndex ].Count ) {
                    MaxBinCount = BinLeftPeaks [ BinIndex ].Count;
                    MaxBinIndex1 = BinIndex;
                }  
            }
            //find wigth on 10% MaxBinCount
            int Level10MaxBinCount = MaxBinCount / 10;
            int LeftBinIndex = MaxBinIndex1;
            for( int BinIndexShift = 1; ; BinIndexShift++ ) {
                int NextLeftBinIndex = MaxBinIndex1 - BinIndexShift;
                if( NextLeftBinIndex < 0 ) { break; }
                if( BinLeftPeaks [NextLeftBinIndex ].Count < Level10MaxBinCount ) {
                    break;
                }
                LeftBinIndex = NextLeftBinIndex;
            }
            int RightBinIndex = MaxBinIndex1;
            for( int BinIndexShift = 1; ; BinIndexShift++ ) {
                int NextRightBinIndex = MaxBinIndex1 + BinIndexShift;
                if( NextRightBinIndex >= BinCount ) { break; }
                if( BinLeftPeaks [ NextRightBinIndex ].Count < Level10MaxBinCount ) {
                    break;
                }
                RightBinIndex = NextRightBinIndex;
            }
            //find error
            List<double> LinkErrors = new List<double>();
            double BestDistance = Masses [ BinRightPeaks [ MaxBinIndex1 ] [ 0 ] ] - Masses [ BinLeftPeaks[ MaxBinIndex1 ] [ 0 ] ];
            for( int BinIndex = LeftBinIndex; BinIndex <= RightBinIndex; BinIndex++ ) {
                for( int PairIndex = 0; PairIndex < BinLeftPeaks [ BinIndex ].Count; PairIndex++ ) {
                    double RightMass = Masses [ BinRightPeaks [ BinIndex ] [ PairIndex ] ];
                    double LeftMass= Masses [ BinLeftPeaks [ BinIndex ] [ PairIndex ] ];
                    double CurDistance = RightMass - LeftMass;
                    double LinkError = AbsMassErrorPPM( BestDistance, CurDistance );
                    LinkErrors.Add( LinkError );
                }
            }
            double MeanError = 0;
            foreach( double Error1 in LinkErrors){
                MeanError = MeanError + Error1;
            }
            MeanError = MeanError / LinkErrors.Count;
            textBoxError.Text = MeanError.ToString();

            double MaxError = 0;
            foreach( double Error1 in LinkErrors ) {
                double dd = Math.Abs( Error1 - MeanError);
                if( MaxError < dd ) { MaxError = dd; }
            }
            textBoxMaxError.Text = MaxError.ToString();

            return MeanError;
        }

        public void CleanComObject( object o ) {
            try {
                while( System.Runtime.InteropServices.Marshal.ReleaseComObject( o ) > 0 )
                    ;
            } catch { } finally {
                o = null;
            }
        }
    }
    class DistanceSupport {
        public int PeakIndex;
        public int NextPeakIndex;
        public List<int> ChaninIndexes = new List<int>();
        public DistanceSupport( int PeakIndex, int NextPeakIndex ) {
            this.PeakIndex = PeakIndex;
            this.NextPeakIndex = NextPeakIndex;
        }
    }
}

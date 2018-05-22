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
using Support;

namespace FindChains {
    public partial class FindChainsForm : Form {
        public FindChainsForm() {
            InitializeComponent();
            numericUpDownFrequencyError.Enabled = checkBoxFrequency.Checked;
            oCChainBlocks = new CChainBlocks();
        }
        CChainBlocks oCChainBlocks;

        //PeakIndex
        double [] Masses;
        double [] Abundances;
        double [] S2Ns;
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
        private void checkBoxFrequency_CheckedChanged( object sender, EventArgs e ) {
            numericUpDownFrequencyError.Enabled = checkBoxFrequency.Checked;
        }
        class PeakMzError {
            public int PeakIndex;
            public double Mz;
            public double NewMz;
            public double PpmError;
            public int Chain;
        }

        private void textBoxChainBlockMasses_DragEnter( object sender, DragEventArgs e ) {
            if ( e.Data.GetDataPresent( DataFormats.FileDrop ) == true ) {
                e.Effect = DragDropEffects.Copy;
            }
        }
        private void textBoxChainBlockMasses_DragDrop( object sender, DragEventArgs e ) {
            string Filename = ( ( string [] ) e.Data.GetData( DataFormats.FileDrop ))[ 0];
            oCChainBlocks.ReadFile( Filename );
        }

        private void textBoxSpectraFile_DragEnter( object sender, DragEventArgs e ) {
            if ( e.Data.GetDataPresent( DataFormats.FileDrop ) == true ) {
                e.Effect = DragDropEffects.Copy;
            }
        }
        private void textBoxSpectraFile_DragDrop( object sender, DragEventArgs e ) {
            string [] Filenames = ( string [] ) e.Data.GetData( DataFormats.FileDrop );
            
            for( int FileIndex = 0; FileIndex < Filenames.Length; FileIndex++ ) {
                string FileExtension = Path.GetExtension( Filenames [ FileIndex ] );
                //read from files        

                Support.CFileReader.ReadFile( Filenames [ FileIndex ], out Masses, out Abundances, out S2Ns, out Resolutions, out RelAbundances );
                Support.InputData RawData = new Support.InputData();
                Support.CFileReader.ReadFile( Filenames [ FileIndex ], out RawData );

                //cut data
                int SettingCount = 0;
                if ( checkBoxS2N.Checked == true ) { SettingCount++;}
                if( checkBoxUseRelAbundance.Checked == true){ SettingCount ++;}
                Support.CFileReader.CutSettings [] CurSettings = new Support.CFileReader.CutSettings [ SettingCount ];
                int SettingIndex = 0;
                if ( checkBoxS2N.Checked == true ) {
                    CurSettings [ SettingIndex ] = new CFileReader.CutSettings();
                    CurSettings [ SettingIndex ].CutType = Support.CFileReader.ECutType.S2N;
                    CurSettings [ SettingIndex ].Min = ( double ) numericUpDownS2N.Value;
                    CurSettings [ SettingIndex ].Max = -1;
                    SettingIndex++;
                }
                if ( checkBoxUseRelAbundance.Checked == true ) {
                    CurSettings [ SettingIndex ] = new CFileReader.CutSettings();
                    CurSettings [ SettingIndex ].CutType = Support.CFileReader.ECutType.RelAbundance;
                    CurSettings [ SettingIndex ].Min = ( double ) numericUpDownMinRelAbundance.Value;
                    CurSettings [ SettingIndex ].Max = -1;
                }
                
                string Filename = Path.GetDirectoryName( Filenames [ FileIndex ] ) + "\\" + Path.GetFileNameWithoutExtension( Filenames [ FileIndex ] );
                if ( checkBoxPPMProcess.Checked == true ) {
                    double MaxChainStartMass = ( double ) numericUpDownMaxPeakToStartChain.Value;
                    int MinPeaksInChain = ( int ) numericUpDownMinPeaksInChain.Value;
                    double PeakPpmError = ( double ) numericUpDownPpmError.Value;

                    Support.InputData Data;
                    Support.CFileReader.CutData( RawData, out Data, CurSettings );
                    oCChainBlocks.FindChains( RawData, MinPeaksInChain, PeakPpmError, 3 * PeakPpmError, MaxChainStartMass, 0, RawData.Masses [ RawData.Masses.Length - 1 ], checkBoxUseKnownChainBlocks.Checked );
                    //CFindChains.ChainsToFile( RawData, Filename + "RawChains.csv" );

                    oCChainBlocks.CreateUniqueChains( RawData, PeakPpmError );
                    oCChainBlocks.ChainsToFile( RawData, Filename + "UniqueChains.csv" );

                    //find clusters based on chains
                    bool [] IsInChainCluster = new bool [ RawData.Chains.Length ];
                    List<List<int>> ChainClusters = new List<List<int>>();

                    for ( int LeftChainIndex = 0; LeftChainIndex < RawData.Chains.Length; LeftChainIndex++ ) {
                        if ( IsInChainCluster [ LeftChainIndex ] == true ) { continue; }
                        List<int> ChainCluster = new List<int>();
                        ChainCluster.Add( LeftChainIndex );
                        IsInChainCluster [ LeftChainIndex ] = true;

                        Dictionary<int, int> ClusterPeaksD = new Dictionary<int, int>();
                        foreach ( int PeakIndex in RawData.Chains [ LeftChainIndex ].PeakIndexes ) {
                            ClusterPeaksD.Add( PeakIndex, PeakIndex );
                        }
                        bool New = false;
                        for ( int RightChainIndex = LeftChainIndex + 1; RightChainIndex < RawData.Chains.Length; RightChainIndex++ ) {
                            if ( IsInChainCluster [ RightChainIndex ] == true ) { continue; }
                            foreach ( int ComparingIndex in RawData.Chains [ RightChainIndex ].PeakIndexes ) {
                                if ( ClusterPeaksD.Contains( new KeyValuePair<int, int>( ComparingIndex, ComparingIndex ) ) == true ) {
                                    ChainCluster.Add( RightChainIndex );
                                    IsInChainCluster [ RightChainIndex ] = true;
                                    foreach ( int PeakIndex in RawData.Chains [ RightChainIndex ].PeakIndexes ) {
                                        KeyValuePair<int, int> qq = new KeyValuePair<int, int>( PeakIndex, PeakIndex );
                                        if ( ClusterPeaksD.Contains( qq ) == false ) {
                                            ClusterPeaksD.Add( PeakIndex, PeakIndex );
                                        }
                                    }
                                    New = true;
                                    break;
                                }
                            }
                            if ( New == true ) {
                                if ( RightChainIndex == RawData.Chains.Length - 1 ) {
                                    RightChainIndex = LeftChainIndex + 1;
                                    New = false;
                                }
                            }
                        }
                        ChainClusters.Add( ChainCluster );
                    }
                    {
                        StreamWriter oStreamWriterClusters = new StreamWriter( Filename + "Clusters.csv" );
                        string HeadLine = "Chain,Mass,PeakIndexes";
                        oStreamWriterClusters.WriteLine( HeadLine );

                        for ( int ClusterIndex = 0; ClusterIndex < ChainClusters.Count; ClusterIndex++ ) {
                            oStreamWriterClusters.WriteLine( "Cluster " + ( ClusterIndex + 1 ).ToString() );
                            for ( int ChainIndex = 0; ChainIndex < ChainClusters [ ClusterIndex ].Count; ChainIndex++ ) {
                                int ChainNumber = ChainClusters [ ClusterIndex ] [ ChainIndex ];
                                string Line = ChainNumber.ToString() + ',' + RawData.Chains [ ChainNumber ].BlockMass.ToString( "F6" );
                                foreach ( int PeakIndex in RawData.Chains [ ChainNumber ].PeakIndexes ) {
                                    Line = Line + ',' + PeakIndex;
                                }
                                oStreamWriterClusters.WriteLine( Line );
                            }
                        }
                        oStreamWriterClusters.Close();
                    }

                    //find the biggest cluster (very simple)
                    int [] ClusterIndexes = new int [ ChainClusters.Count ];
                    int [] ClusterCounts = new int [ ChainClusters.Count ];
                    for ( int Index = 0; Index < ClusterCounts.Length; Index++ ) {
                        ClusterIndexes [ Index ] = Index;
                        ClusterCounts [ Index ] = ChainClusters [ Index ].Count;
                    }
                    Array.Sort( ClusterCounts, ClusterIndexes );
                    List<int> BiggestChainCluster = ChainClusters [ ClusterIndexes [ ClusterIndexes.Length - 1 ] ];

                    //error
                    int ClusterPeaks = 0;
                    int MinClsuterChainIndex = -1;
                    for ( int ClusterChainIndex = 0; ClusterChainIndex < BiggestChainCluster.Count; ClusterChainIndex++ ) {
                        int ChainIndex = BiggestChainCluster [ ClusterChainIndex ];
                        ClusterPeaks = ClusterPeaks + RawData.Chains [ ChainIndex ].PeakIndexes.Length;
                        if ( MinClsuterChainIndex == -1 ) {
                            MinClsuterChainIndex = ClusterChainIndex;
                        } else if ( RawData.Chains [ BiggestChainCluster [ MinClsuterChainIndex ] ].PeakIndexes [ 0 ] > RawData.Chains [ BiggestChainCluster [ ClusterChainIndex ] ].PeakIndexes [ 0 ] ) {
                            MinClsuterChainIndex = ClusterChainIndex;
                        }
                    }
                    int SwapClusterIndex = BiggestChainCluster [ 0 ];
                    BiggestChainCluster [ 0 ] = BiggestChainCluster [ MinClsuterChainIndex ];
                    BiggestChainCluster [ MinClsuterChainIndex ] = SwapClusterIndex;

                    bool [] ClusterChains = new bool [ BiggestChainCluster.Count];
                    int [] ClusterPeakIndexes = new int [ ClusterPeaks ];
                    for( int ii = 0; ii < ClusterPeaks; ii++){
                        ClusterPeakIndexes[ ii] = -1;
                    }
                    PeakMzError [] PeakMzErrorArray = new PeakMzError[ ClusterPeaks];
                    int StartPeakMzErrorArrayIndex = 0;
                    int EndPeakMzErrorArrayIndex = 0;
                    int NewEndPeakMzErrorArrayIndex = 0;
                    do {
                        if ( EndPeakMzErrorArrayIndex == 0 ) {
                            //add Chain 0
                            int [] PeakIndexes = RawData.Chains [ BiggestChainCluster [ 0 ] ].PeakIndexes;
                            double FirstPeakMz = RawData.Masses [ PeakIndexes [ 0 ] ];
                            for ( int PeakIndex = 0; PeakIndex < PeakIndexes.Length; PeakIndex++ ) {
                                PeakMzError NewPeakMzError = new PeakMzError();
                                NewPeakMzError.PeakIndex = PeakIndexes [ PeakIndex ];
                                NewPeakMzError.Mz = RawData.Masses [ NewPeakMzError.PeakIndex ];
                                NewPeakMzError.NewMz = FirstPeakMz + PeakIndex *  RawData.Chains [ BiggestChainCluster [ 0 ] ].IdealBlockMass;
                                NewPeakMzError.Chain = BiggestChainCluster [ 0 ];
                                NewPeakMzError.PpmError = CPpmError.ErrorToPpm( NewPeakMzError.Mz, NewPeakMzError.NewMz - NewPeakMzError.Mz );
                                PeakMzErrorArray [ EndPeakMzErrorArrayIndex ] = NewPeakMzError;
                                EndPeakMzErrorArrayIndex++;
                            }
                            NewEndPeakMzErrorArrayIndex = EndPeakMzErrorArrayIndex;
                        } else {
                            StartPeakMzErrorArrayIndex = EndPeakMzErrorArrayIndex;
                            EndPeakMzErrorArrayIndex = NewEndPeakMzErrorArrayIndex;
                        }
                        for ( int CurIndex = StartPeakMzErrorArrayIndex; CurIndex < EndPeakMzErrorArrayIndex; CurIndex++ ) {
                            for ( int ClusterChainIndex = 1; ClusterChainIndex < BiggestChainCluster.Count; ClusterChainIndex++ ) {
                                if ( ClusterChains [ ClusterChainIndex ] == true ) { continue; }
                                int SearchPeakIndex = Array.BinarySearch( RawData.Chains [ BiggestChainCluster [ ClusterChainIndex ] ].PeakIndexes, PeakMzErrorArray [ CurIndex ].PeakIndex );
                                if ( SearchPeakIndex < 0 ) { continue; }
                                int [] PeakIndexes = RawData.Chains [ BiggestChainCluster [ ClusterChainIndex ] ].PeakIndexes;
                                double FirstPeakMz = PeakMzErrorArray [ CurIndex ].NewMz - SearchPeakIndex * RawData.Chains [ BiggestChainCluster [ ClusterChainIndex ] ].IdealBlockMass;
                                for ( int PeakIndex = 0; PeakIndex < PeakIndexes.Length; PeakIndex++ ) {
                                    PeakMzError NewPeakMzError = new PeakMzError();
                                    NewPeakMzError.PeakIndex = PeakIndexes [ PeakIndex ];
                                    NewPeakMzError.Mz = RawData.Masses [ NewPeakMzError.PeakIndex ];
                                    NewPeakMzError.NewMz = FirstPeakMz + PeakIndex *  RawData.Chains [ BiggestChainCluster [ ClusterChainIndex ] ].IdealBlockMass;
                                    NewPeakMzError.Chain = BiggestChainCluster [ ClusterChainIndex];
                                    NewPeakMzError.PpmError = CPpmError.ErrorToPpm( NewPeakMzError.Mz, NewPeakMzError.NewMz - NewPeakMzError.Mz );
                                    PeakMzErrorArray [ NewEndPeakMzErrorArrayIndex ] = NewPeakMzError;
                                    NewEndPeakMzErrorArrayIndex++;
                                }
                                ClusterChains [ ClusterChainIndex ] = true;
                            }
                        }
                    } while ( NewEndPeakMzErrorArrayIndex > EndPeakMzErrorArrayIndex );
                    {
                        StreamWriter oStreamWriterErrors = new StreamWriter( Filename + "Errors.csv" );
                        string HeadLine = "PeakIndex,Mass,Abundance,NewMass,Chain,ChainBlockMass,PpmError";
                        oStreamWriterErrors.WriteLine( HeadLine );
                        for ( int PeakIndex = 0; PeakIndex < PeakMzErrorArray.Length; PeakIndex++ ) {
                            PeakMzError CurPeakMzError = PeakMzErrorArray [ PeakIndex ];
                            oStreamWriterErrors.WriteLine( CurPeakMzError.PeakIndex.ToString() + "," + CurPeakMzError.Mz.ToString( "F6" ) + "," + RawData.Abundances[CurPeakMzError.PeakIndex]
                                    + "," + CurPeakMzError.NewMz.ToString( "F6" ) + "," + CurPeakMzError.Chain + 
                                    "," + RawData.Chains[CurPeakMzError.Chain].IdealBlockMass.ToString( "F6") + "," + CurPeakMzError.PpmError.ToString( "F6" ) );
                        }
                        oStreamWriterErrors.Close();
                    }

                    //                    bool [] DublicatedChains = new bool [ Chains.Length ];
                    //                    for( int ChainIndex = 0; ChainIndex < Chains.Length - 1; ChainIndex++ ) {
                    //                        if( DublicatedChains [ ChainIndex ] == true ) { continue; }
                    //                        double Distance = ChainDistances [ ChainIndex ];
                    //                        List<int> Chain = Chains [ ChainIndex ];
                    //                        for( int DistanceGap = 1; ; DistanceGap++ ) {
                    //                            //check chain availibity on min peaks in chain
                    //                            double MinPeaksMaxMass = Masses [ Chain [ 0 ] ] + Distance * DistanceGap * ( MinPeaksInChain - 1 );
                    //                            if( Masses [ Masses.Length - 1 ] < LeftPpmMass( MinPeaksMaxMass, 2 * PpmError ) ) {
                    //                                break;
                    //                            }
                    //                            //find distance error max based on mass of last peak in chain
                    //                            double MaxMassInChain = ( Masses [ Chain [ 0 ] ] + Masses [ Chain [ Chain.Count - 1 ] ] - Masses [ Chain [ 0 ] ] ) * DistanceGap;
                    //                            double MaxError = PpmToError( MaxMassInChain, 2 * PpmError );
                    //                            double LeftDistance = Distance * DistanceGap - MaxError;
                    //                            //calculate min distance
                    //                            int LeftDistanceIndex = Array.BinarySearch( ChainDistances, LeftDistance );
                    //                            //int LeftDistanceIndex = Array.BinarySearch( ChainDistances, ChainIndex + 1, Chains.Length - ChainIndex - 1, LeftDistance );
                    //                            if( LeftDistanceIndex < 0 ) { LeftDistanceIndex = ~LeftDistanceIndex; }
                    //                            if( LeftDistanceIndex <= ChainIndex ) { LeftDistanceIndex = ChainIndex + 1; }//in case DistanceGap == 1
                    //                            if( LeftDistanceIndex >= ChainDistances.Length ) { break; }
                    //                            //search
                    //                            double RightDistance = Distance * DistanceGap + MaxError;
                    //                            for( int CompareChainIndex = LeftDistanceIndex; CompareChainIndex < ChainDistances.Length; CompareChainIndex++ ) {
                    //                                if( ChainDistances [ CompareChainIndex ] > RightDistance ) { break; }
                    //                                if( DublicatedChains [ CompareChainIndex ] == true ) { continue; }
                    //                                List<int> CompareChain = Chains [ CompareChainIndex ];
                    //
                    //                                //compare chains
                    //                                for( int Index = 0; Index < Chain.Count; Index++ ) {
                    //                                    int PeakIndex = Chain [ Index ];
                    //                                    int CompareIndex = Array.BinarySearch( CompareChain.ToArray(), PeakIndex );
                    //                                    if( CompareIndex >= 0 ) {
                    //                                        int NextIndex = Index + DistanceGap;
                    //                                        int NextCompareIndex = CompareIndex + 1;
                    //                                        if( ( ( NextIndex < Chain.Count ) && ( NextCompareIndex < CompareChain.Count ) ) == false ) {
                    //                                           break;
                    //                                        }
                    //                                        bool TheSameChains = ( Chain [ NextIndex ] == CompareChain [ NextCompareIndex ] );
                    //                                        if( TheSameChains == false ) {
                    //                                            //second try
                    //                                            NextIndex = Index + DistanceGap * 2;
                    //                                            NextCompareIndex = CompareIndex + 2;
                    //                                            if( ( ( NextIndex < Chain.Count ) && ( NextCompareIndex < CompareChain.Count ) ) == false ) {
                    //                                                break;
                    //                                            }
                    //                                            TheSameChains = ( Chain [ NextIndex ] == CompareChain [ NextCompareIndex ] );
                    //                                        }
                    //                                        if( TheSameChains == false ) { break; }
                    //                                        //mark secondary chain
                    //                                        if( DistanceGap > 1 ) {
                    //                                            DublicatedChains [ CompareChainIndex ] = true;
                    //                                            //include peaks?
                    //                                        } else {
                    //                                            if( Chain.Count >= CompareChain.Count ) {
                    //                                                DublicatedChains [ CompareChainIndex ] = true;
                    //                                            } else {
                    //                                                DublicatedChains [ ChainIndex ] = true;
                    //                                           }
                    //                                        }
                    //                                        break;
                    //                                    }
                    //                                }
                    //                            }
                    //                        }
                    //                    }
                    //remove n*derived chains
                    //                    LChains.Clear();
                    //                    LChainDistances.Clear();
                    //                    for( int ChainIndex = 0; ChainIndex < Chains.Length; ChainIndex++ ) {
                    //                        if( DublicatedChains [ ChainIndex ] == false ) {
                    //                            LChains.Add( Chains [ ChainIndex ] );
                    //                            LChainDistances.Add( ChainDistances [ ChainIndex ] );
                    //                        }
                    //                    }
                    //                    //save result
                    //                    if( checkBoxFileFormatPeakIndex.Checked == true ) {
                    //                        StreamWriter oStreamWriterPrimaryChains = new StreamWriter( Filename + "PrimaryChainsIndex.csv" );
                    //                        oStreamWriterPrimaryChains.WriteLine( "Distance,Count,Index" );
                    //                        for( int LineIndex = 0; LineIndex < LChainDistances.Count; LineIndex++ ) {
                    //                            string Line = LChainDistances [ LineIndex ].ToString() + "," + LChains [ LineIndex ].Count;
                    //                            foreach( int PeakIndex in LChains [ LineIndex ] ) {
                    //                                Line = Line + "," + PeakIndex;
                    //                            }
                    //                            oStreamWriterPrimaryChains.WriteLine( Line );
                    //                        }
                    //                        oStreamWriterPrimaryChains.Close();
                    //                    }
                    //                    if( checkBoxFileFormatPeakMass.Checked == true ) {
                    //                        StreamWriter oStreamWriterPrimaryChains = new StreamWriter( Filename + "PrimaryChainsMass.csv" );
                    //                        oStreamWriterPrimaryChains.WriteLine( "Distance,Count,Mass" );
                    //                        for( int LineIndex = 0; LineIndex < LChainDistances.Count; LineIndex++ ) {
                    //                            string Line = LChainDistances [ LineIndex ].ToString() + "," + LChains [ LineIndex ].Count;
                    //                            foreach( int PeakIndex in LChains [ LineIndex ] ) {
                    //                                Line = Line + "," + Masses [ PeakIndex ];
                    //                            }
                    //                            oStreamWriterPrimaryChains.WriteLine( Line );
                    //                        }
                    //                        oStreamWriterPrimaryChains.Close();
                    //                    }
                    //Combine distances                
                    //                    if( checkBoxFrequency.Checked == true){
                    //                        List <int> LDistancePeaks = new List<int> ( LChains.Count);
                    //                        for( int ChainIndex = 0; ChainIndex < LChains.Count; ChainIndex++ ) {
                    //                            LDistancePeaks.Add( LChains [ ChainIndex ].Count);
                    //                        }
                    //                        int TotalChains = LChains.Count;
                    //                        double DistanceError = ( double ) numericUpDownFrequencyError.Value;
                    //                        for( int ChainIndex = 1; ChainIndex < TotalChains - 2; ) {
                    //                            double PreviousDistance = LChainDistances [ ChainIndex - 1 ];
                    //                            double Distance = LChainDistances [ ChainIndex ];
                    //                            double LeftError = Distance - PreviousDistance;
                    //                            if( LeftError > DistanceError ) { ChainIndex++; }
                    //                            double NextDistance = LChainDistances [ ChainIndex + 1 ];
                    //                            double RightError = NextDistance - Distance;
                    //                            if( LeftError <= RightError ) {
                    //                                //combine PreviousChain and Chain
                    //                                LDistancePeaks [ ChainIndex - 1 ] = LDistancePeaks [ ChainIndex - 1 ] + LDistancePeaks [ ChainIndex ];
                    //                                LChainDistances [ ChainIndex - 1 ] = PreviousDistance + ( Distance - PreviousDistance ) * LDistancePeaks [ ChainIndex ] / LDistancePeaks [ ChainIndex - 1 ];
                    //                                LDistancePeaks.RemoveAt( ChainIndex );
                    //                                LChainDistances.RemoveAt( ChainIndex );
                    //                                TotalChains--;
                    //                            } else {//LeftError > RightError
                    //                                //combine Chain and NextChain
                    //                                LDistancePeaks [ ChainIndex ] = LDistancePeaks [ ChainIndex ] + LDistancePeaks [ ChainIndex + 1 ];
                    //                                LChainDistances [ ChainIndex ] = Distance + ( NextDistance - Distance ) * LDistancePeaks [ ChainIndex + 1 ] / LDistancePeaks [ ChainIndex ];
                    //                                LDistancePeaks.RemoveAt( ChainIndex + 1 );
                    //                                LChainDistances.RemoveAt( ChainIndex + 1 );
                    //                                TotalChains--;
                    //                            }
                    //                        }
                    //                        StreamWriter oStreamWriterPrimaryChains = new StreamWriter( Filename + "AlignedDistances.csv" );
                    //                        oStreamWriterPrimaryChains.WriteLine( "Distance,Peaks" );
                    //                        for( int LineIndex = 0; LineIndex < LDistancePeaks.Count; LineIndex++ ) {
                    //                            string Line = LChainDistances [ LineIndex ].ToString() + "," + LDistancePeaks [ LineIndex ];
                    //                            oStreamWriterPrimaryChains.WriteLine( Line );
                    //                        }
                    //                        oStreamWriterPrimaryChains.Close();
                    //                    }
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
                    double LinkError = CPpmError.AbsMassErrorPPM( BestDistance, CurDistance );
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

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Support;

namespace FindChains {
    public static class CFindChains {
        public static void FindChains( Support.InputData Data, int MinPeaksInChain, double PpmError, double ChainPpmError,
                double MaxStartPeakMass, double MinChainMass, double MaxChainMass ) {
            //ChainPpmError argument is not used
            double [] Masses = Data.Masses;
            List<List<int>> ChainPeakListList = new List<List<int>>();
            for ( int FirstPeakIndex = 0; FirstPeakIndex < Masses.Length - 1; FirstPeakIndex++ ) {
                if ( Masses [ FirstPeakIndex ] > MaxStartPeakMass ) { break; }
                double SpectraMaxChainMass = ( Masses [ Masses.Length - 1 ] - Masses [ FirstPeakIndex ] ) / ( MinPeaksInChain - 1 );
                if ( MinChainMass > SpectraMaxChainMass ) { continue; }
                if ( MaxChainMass > SpectraMaxChainMass ) { MaxChainMass = SpectraMaxChainMass; }

                int StartSecondPeakIndex = FirstPeakIndex + 1;
                if ( MinChainMass > 0 ) {
                    double StartSecondPeakLeftMass = Masses [ FirstPeakIndex ] + MinChainMass;
                    StartSecondPeakLeftMass = CPpmError.LeftPpmMass( StartSecondPeakLeftMass, PpmError );
                    StartSecondPeakIndex = Array.BinarySearch( Masses, FirstPeakIndex + 1, Masses.Length - FirstPeakIndex - 1, StartSecondPeakLeftMass );
                    if ( StartSecondPeakIndex < 0 ) { StartSecondPeakIndex = ~StartSecondPeakIndex; }
                    if ( StartSecondPeakIndex >= Masses.Length ) { continue; }
                }

                for ( int SecondPeakIndex = StartSecondPeakIndex; SecondPeakIndex < Masses.Length; SecondPeakIndex++ ) {
                    double ChainMass = Masses [ SecondPeakIndex ] - Masses [ FirstPeakIndex ];
                    if ( ChainMass > MaxChainMass ) { break; }
                    List<int> TempPeakList = new List<int>();
                    TempPeakList.Add( FirstPeakIndex );
                    TempPeakList.Add( SecondPeakIndex );

                    double NextPeakMass = Masses [ SecondPeakIndex ];
                    int NextPeakIndex = SecondPeakIndex + 1;
                    for ( ; ; ) {
                        NextPeakMass = NextPeakMass + ChainMass;
                        int Index = Array.BinarySearch( Masses, NextPeakIndex, Masses.Length - NextPeakIndex, NextPeakMass );
                        if ( Index < 0 ) { Index = ~Index; }
                        if ( Index >= Masses.Length ) { break; }
                        if ( NextPeakMass - Masses [ Index - 1 ] < Masses [ Index ] - NextPeakMass ) {//left peak
                            if ( Index -1 == TempPeakList [ TempPeakList.Count - 1 ] ) {
                                break;
                            }//left peak can't be previous peak
                            double PeakLeftMass = CPpmError.LeftPpmMass( NextPeakMass, PpmError );
                            if ( PeakLeftMass > Masses [ Index - 1 ] ) { break; }
                            TempPeakList.Add( Index - 1 );
                            NextPeakMass = Masses [ Index - 1 ];
                            NextPeakIndex = Index;
                        } else {//right peak
                            double PeakRightMass = Support.CPpmError.RightPpmMass( NextPeakMass, PpmError );
                            if ( Masses [ Index ] > PeakRightMass ) { break; }
                            TempPeakList.Add( Index );
                            NextPeakMass = Masses [ Index ];
                            NextPeakIndex = Index + 1;
                            if ( NextPeakIndex >= Masses.Length ) { break; }
                        }
                    }
                    if ( TempPeakList.Count >= MinPeaksInChain ) {
                        ChainPeakListList.Add( TempPeakList );
                    }
                }
            }
            List<int> [] ChainPeakList = ChainPeakListList.ToArray();
            double [] ChainMasses = new double [ ChainPeakList.Length ];//for sort
            Support.Chain [] Chains = new Support.Chain [ ChainPeakList.Length ];
            for ( int ChainIndex = 0; ChainIndex < Chains.Length; ChainIndex++ ) {
                Support.Chain TempChain = new Support.Chain();
                TempChain.PeakIndexes = ChainPeakList [ ChainIndex ].ToArray();
                TempChain.PeakMasses = new double [ TempChain.PeakIndexes.Length ];
                for ( int PeakIndex = 0; PeakIndex < TempChain.PeakMasses.Length; PeakIndex++ ) {
                    TempChain.PeakMasses [ PeakIndex ] = Masses [ TempChain.PeakIndexes [ PeakIndex ] ];
                }
                double [] BlockMasses = new double [ TempChain.PeakIndexes.Length - 1 ];
                for ( int BlockIndex = 0; BlockIndex < BlockMasses.Length; BlockIndex++ ) {
                    BlockMasses [ BlockIndex ] = TempChain.PeakMasses [ BlockIndex + 1 ] - TempChain.PeakMasses [ BlockIndex ];
                }
                TempChain.Mass = CArrayMath.Mean( BlockMasses );
                ChainMasses [ ChainIndex ] = TempChain.Mass;//for sort
                TempChain.MassStdDev = CArrayMath.StandardDeviation( BlockMasses, TempChain.Mass );
                Chains [ ChainIndex ] = TempChain;
            }
            Array.Sort( ChainMasses, Chains );
            Data.Chains = Chains;
            CreateChainPeaks( Data );
        }
        public static void CreateChainPeaks( Support.InputData Data ) {
            Support.Chain [] Chains = Data.Chains;
            int PeakCount = Data.Masses.Length;
            List<int> [] PeakChainsList = new List<int> [ PeakCount ];
            for ( int PeakIndex = 0; PeakIndex < PeakCount; PeakIndex++ ) {
                PeakChainsList [ PeakIndex ] = new List<int>();
            }
            for ( int ChainIndex = 0; ChainIndex < Chains.Length; ChainIndex++ ) {
                int [] TempPeakIndexes = Chains [ ChainIndex ].PeakIndexes;
                for ( int ChainPeakIndex = 0; ChainPeakIndex < TempPeakIndexes.Length; ChainPeakIndex++ ) {
                    int PeakIndex = TempPeakIndexes [ ChainPeakIndex ];
                    PeakChainsList [ PeakIndex ].Add( ChainIndex );
                }
            }
            int [] [] PeakChains = new int [ PeakCount ] [];
            for ( int PeakIndex = 0; PeakIndex < PeakChains.Length; PeakIndex++ ) {
                PeakChains [ PeakIndex ] =  PeakChainsList [ PeakIndex ].ToArray();
            }
            Data.ChainIndexes = PeakChains;
        }
        public static int GetMaxChainsPeak( Support.InputData Data ) {
            int MaxChainsPeak = 0;
            int [] [] ChainIndexes = Data.ChainIndexes;
            for ( int PeakIndex = 0; PeakIndex < ChainIndexes.Length; PeakIndex++ ) {
                if ( MaxChainsPeak < ChainIndexes [ PeakIndex ].Length ) { MaxChainsPeak = ChainIndexes [ PeakIndex ].Length; }
            }
            return MaxChainsPeak;
        }
        public static void CreateUniqueChains( Support.InputData Data, double PpmError ) {
            Support.Chain [] Chains = Data.Chains;
            int [] [] ChainPeaks = Data.ChainIndexes;
            //search duplicate chain in each peak
            bool [] IsDuplicateChains = new bool [ Chains.Length ];
            for ( int PeakIndex = 0; PeakIndex < Data.Masses.Length; PeakIndex++ ) {
                if ( ChainPeaks [ PeakIndex ] == null ) continue;
                if ( ChainPeaks [ PeakIndex ].Length == 1 ) continue;
                for ( int LeftPeakChainIndex = 0; LeftPeakChainIndex < ChainPeaks [ PeakIndex ].Length - 1; LeftPeakChainIndex++ ) {
                    if ( IsDuplicateChains [ ChainPeaks [ PeakIndex ] [ LeftPeakChainIndex ] ] == true ) continue;
                    int [] LeftChainPeakIndexes = Chains [ ChainPeaks [ PeakIndex ] [ LeftPeakChainIndex ] ].PeakIndexes;
                    for ( int RightPeakChainIndex = LeftPeakChainIndex + 1; RightPeakChainIndex <  ChainPeaks [ PeakIndex ].Length; RightPeakChainIndex++ ) {
                        if ( IsDuplicateChains [ ChainPeaks [ PeakIndex ] [ RightPeakChainIndex ] ] == true ) continue;
                        int [] RightChainPeakIndexes = Chains [ ChainPeaks [ PeakIndex ] [ RightPeakChainIndex ] ].PeakIndexes;
                        int Coincidences = 0;
                        foreach ( int LeftPeak in LeftChainPeakIndexes ) {
                            foreach ( int RightPeak in RightChainPeakIndexes ) {
                                if ( LeftPeak == RightPeak ) Coincidences++;
                            }
                        }
                        if ( Coincidences > 1 ) { IsDuplicateChains [ ChainPeaks [ PeakIndex ] [ RightPeakChainIndex ] ] = true; }
                    }
                }
            }

            int NonDuplicatedChainCount = 0;
            foreach ( bool IsDuplicated in IsDuplicateChains ) {
                if ( IsDuplicated == true ) { continue; }
                NonDuplicatedChainCount++;
            }

            Support.Chain [] NonDuplicatedChains = new Support.Chain [ NonDuplicatedChainCount ];
            int NonDuplicatedChainIndex = 0;
            for ( int ChainIndex = 0; ChainIndex < Chains.Length; ChainIndex++ ) {
                if ( IsDuplicateChains [ ChainIndex ] == true ) { continue; }
                NonDuplicatedChains [ NonDuplicatedChainIndex ] = Chains [ ChainIndex ];
                NonDuplicatedChainIndex++;
            }
            Data.Chains = NonDuplicatedChains;
            CreateChainPeaks( Data );
        }
        public static void ChainsToFile( Support.InputData Data, string Filename ) {
            StreamWriter oStreamWriterChains = new StreamWriter( Filename );
            int MaxChainPeaks = 0;
            foreach ( Support.Chain CurChain in Data.Chains ) {
                if ( MaxChainPeaks < CurChain.PeakIndexes.Length ) { MaxChainPeaks = CurChain.PeakIndexes.Length; }
            }
            string HeadLine = string.Empty;
            for ( int Index = 0; Index < MaxChainPeaks; Index++ ) {
                HeadLine = HeadLine + "Index,";
            }
            HeadLine = HeadLine + "AvMass,MassStDev,Count";
            for ( int Index = 0; Index < MaxChainPeaks; Index++ ) {
                HeadLine = HeadLine + ",Mass";
            }
            oStreamWriterChains.WriteLine( HeadLine );
            string OutputAccuracity ="F6";
            for ( int LineIndex = 0; LineIndex < Data.Chains.Length; LineIndex++ ) {
                Support.Chain CurChain = Data.Chains [ LineIndex ];
                //add peak indexes
                string Line = new string( ',', MaxChainPeaks - CurChain.PeakIndexes.Length );
                for ( int PeakIndex = 0; PeakIndex < CurChain.PeakIndexes.Length; PeakIndex++ ) {
                    Line = Line + CurChain.PeakIndexes [ PeakIndex ].ToString( OutputAccuracity ) + ",";
                }
                //add middle part
                Line = Line + CurChain.Mass.ToString( OutputAccuracity ) +  "," + CurChain.MassStdDev.ToString( OutputAccuracity )  + "," + CurChain.PeakIndexes.Length;
                //add peak masses
                for ( int PeakIndex = 0; PeakIndex < CurChain.PeakIndexes.Length; PeakIndex++ ) {
                    Line = Line  + "," + CurChain.PeakMasses [ PeakIndex ].ToString( OutputAccuracity );
                }
                oStreamWriterChains.WriteLine( Line );
            }
            oStreamWriterChains.Close();
        }
        public static void PeakChainsToFile( Support.InputData Data, string Filename ) {
            StreamWriter oStreamWriterChains = new StreamWriter( Filename );
            string HeadLine = "Mass,ChainsCount,Chain";
            oStreamWriterChains.WriteLine( HeadLine );
            string OutputAccuracity ="F6";
            for ( int PeakIndex = 0; PeakIndex < Data.Masses.Length; PeakIndex++ ) {
                int [] ChainIndexes =  Data.ChainIndexes [ PeakIndex ];
                string Line = Data.Masses [ PeakIndex ].ToString( OutputAccuracity ) + ',' + ChainIndexes.Length;
                for ( int ChainIndex = 0; ChainIndex < ChainIndexes.Length; ChainIndex++ ) {
                    Line = Line + ',' + ChainIndexes [ ChainIndex ].ToString( OutputAccuracity );
                }
                oStreamWriterChains.WriteLine( Line );
            }
            oStreamWriterChains.Close();
        }
    }
}

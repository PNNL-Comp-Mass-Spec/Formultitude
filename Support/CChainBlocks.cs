using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

namespace Support {
    public class CChainBlocks {
        string [] ElementNames = new string [] {
                "H", "C", "N", "O", "Na",
                "P", "S", "Cl", "K", "Br",
                "I"
        };
        double [] ElementMasses = new double [] {
                1.0078250321, 12, 14.0030740052, 15.9949146221, 22.98976967,
                30.97376151, 31.97207069, 34.96885271, 38.9637069, 78.9183376,
                126.904473
        };

        public string [] KnownChainBlockNames = new string [] {
                "H2",
                "C",
                "CH2",
                "O"
        };
        public double [] KnownChainBlockMasses = new double [] {
                CElements.H *2,
                CElements.C,
                CElements.C + 2 * CElements.H,
                CElements.O,
        };

        public CChainBlocks() {
            Array.Sort( ElementNames, ElementMasses );
            //double dd = CalculateFormulaMass( "N1H_3O" );
            //double dd1 = CalculateFormulaMass( "N1H_30O" );
            //double dd2 = CalculateFormulaMass( "NH_30O" );
            //double dd3 = CalculateFormulaMass( "NH_O" );
        }
        static char [] WordSeparators = new char [] { '\t', ',', ' ' };
        public void ReadFile( string Filename){
            //Chain block formula is first word in line
            string [] Lines = File.ReadAllLines( Filename );
            List<string> FormulaNameList = new List<string>( Lines.Length);
            List<double> FormulaMassList = new List<double>( Lines.Length);
            foreach ( string Line in Lines ) {
                string Formula = Line.Split( WordSeparators )[0];
                try{
                    FormulaMassList.Add( CalculateFormulaMass( Formula) );
                    FormulaNameList.Add( Formula );
                }catch{}
            }
            KnownChainBlockNames = FormulaNameList.ToArray();
            KnownChainBlockMasses = FormulaMassList.ToArray();
            Array.Sort( KnownChainBlockMasses, KnownChainBlockNames );
            WriteFile( Filename );
        }
        public void WriteFile( string Filename ) {
            string [] Lines = new string [ KnownChainBlockMasses.Length ];
            for ( int Index = 0; Index < KnownChainBlockMasses.Length; Index++ ) {
                Lines [ Index ] = KnownChainBlockNames [ Index ] + "," + KnownChainBlockMasses [ Index ].ToString( "F6" );
            }
            int FileExtentionLength = Path.GetExtension( Filename).Length;
            Filename = Filename.Substring( 0, Filename.Length - FileExtentionLength ) + "Real.csv";
            //File.WriteAllLines( Filename, Lines );
        }
        double CalculateFormulaMass( string Formula ) {
            //formula example N1H_3O
            double Mass = 0;
            for ( int SymbolIndex = 0; SymbolIndex < Formula.Length; SymbolIndex++ ) {
                string ElementName;
                int NegPos = 1;
                int Atoms = 1;
                //start from Capital letter
                if ( char.IsUpper( Formula [ SymbolIndex ] ) == false ) { throw new Exception( "" ); }
                if( SymbolIndex + 1 >= Formula.Length){
                    //last symbol
                    ElementName = Formula [ SymbolIndex ].ToString();
                    //NegPos = 1;
                    //Atoms = 1;
                }else{
                    ElementName = Formula [ SymbolIndex ].ToString();
                    if ( char.IsUpper( Formula [ SymbolIndex + 1 ] ) == true ) {
                        //next element
                        //NegPos = 1;
                        //Atoms = 1;
                    } else {
                        if ( char.IsLower( Formula [ SymbolIndex + 1 ] ) == true ) {
                            //Check second small letter
                            ElementName = ElementName + Formula [ SymbolIndex + 1 ];
                            SymbolIndex = SymbolIndex + 1;
                        }
                        if ( SymbolIndex + 1 >= Formula.Length ) {
                            //last symbol
                            //NegPos = 1;
                            //Atoms = 1;
                        } else {
                            //Check negative
                            if ( Formula [ SymbolIndex + 1 ] == '_' ) {
                                NegPos = -1;
                                SymbolIndex = SymbolIndex + 1;
                            }
                            //Check atom number
                            if ( SymbolIndex + 1 >= Formula.Length ) {
                                //Atoms = 1;
                            } else if ( char.IsDigit( Formula [ SymbolIndex + 1 ] ) == true ) {
                                string Number = Formula [ SymbolIndex + 1 ].ToString();
                                SymbolIndex = SymbolIndex + 1;
                                for ( ; SymbolIndex + 1 < Formula.Length; ) {
                                    if ( char.IsDigit( Formula [ SymbolIndex + 1 ] ) == false ) { break;}
                                    SymbolIndex++;
                                    Number = Number + Formula [ SymbolIndex ];
                                }
                                if ( int.TryParse( Number, out Atoms ) == false ) { throw new Exception( "" ); }
                            } else {
                                //Atoms = 1;
                            }
                        }
                    }
                }

                int Index = Array.BinarySearch( ElementNames, ElementName);
                if ( Index < 0 ) { throw new Exception( "" ); }
                Mass = Mass + NegPos * ElementMasses[ Index] * Atoms;
            }
            return Mass;
        }
        public int FindKnownChainBlockIndex( double ChainBlockMass, double PpmError ) {
            return CPpmError.SearchIndex( this.KnownChainBlockMasses, ChainBlockMass, PpmError );
        }
        public void FindChains( Support.InputData Data, int MinPeaksInChain, double PeakPpmError, double ChainPpmError,
                double MaxStartPeakMass, double MinChainDistance, double MaxChainDistance, bool OnlyKnowsChains ) {
            //if MinChainDistance <= 0 - parameter is not used
            double [] Masses = Data.Masses;
            List<List<int>> ChainPeakListList = new List<List<int>>();
            for ( int FirstPeakIndex = 0; FirstPeakIndex < Masses.Length - 1; FirstPeakIndex++ ) {
                if ( Masses [ FirstPeakIndex ] > MaxStartPeakMass ) { break; }//???
                double SpectraMaxChainDistance = ( Masses [ Masses.Length - 1 ] - Masses [ FirstPeakIndex ] ) / ( MinPeaksInChain - 1 );
                if ( MinChainDistance > SpectraMaxChainDistance ) { break; }
                int SecondPeakIndex = FirstPeakIndex + 1;
                if ( MinChainDistance > 0 ) {
                    double SecondPeakMass = Masses [ FirstPeakIndex ] + MinChainDistance;
                    SecondPeakMass = CPpmError.LeftPpmMass( SecondPeakMass, PeakPpmError );
                    SecondPeakIndex = CPpmError.SearchNextIndex( Masses, FirstPeakIndex + 1, SecondPeakMass );
                    if ( SecondPeakIndex == -1 ) { continue; }
                }
                for ( ; SecondPeakIndex < Masses.Length; SecondPeakIndex++ ) {
                    double ChainDistance = Masses [ SecondPeakIndex ] - Masses [ FirstPeakIndex ];
                    if ( CPpmError.LeftPpmMass( ChainDistance, PeakPpmError ) > MaxChainDistance ) { break; }
                    int ChainBlockIndex = FindKnownChainBlockIndex( ChainDistance, ChainPpmError );
                    if ( ( OnlyKnowsChains == true ) && ( ChainBlockIndex < 0 ) ) {
                        continue;
                    }
                    List<int> TempPeakList = new List<int>();
                    TempPeakList.Add( FirstPeakIndex );
                    TempPeakList.Add( SecondPeakIndex );

                    double NextPeakMass = Masses [ SecondPeakIndex ];
                    int NextPeakIndex = SecondPeakIndex + 1;
                    for ( ; NextPeakIndex < Masses.Length; ) {
                        NextPeakMass = NextPeakMass + ChainDistance;
                        int Index = CPpmError.SearchIndex( Masses, NextPeakIndex, NextPeakMass, PeakPpmError );
                        if ( Index < 0 ) { break; }
                        TempPeakList.Add( Index );
                        NextPeakMass = Masses [ Index ];
                        NextPeakIndex = Index + 1;
                    }
                    if ( TempPeakList.Count >= MinPeaksInChain ) {
                        ChainPeakListList.Add( TempPeakList );
                    }
                }
            }
            List<int> [] ChainPeakList = ChainPeakListList.ToArray();
            double [] ChainMasses = new double [ ChainPeakList.Length ];//for sort
            Chain [] Chains = new Chain [ ChainPeakList.Length ];
            for ( int ChainIndex = 0; ChainIndex < Chains.Length; ChainIndex++ ) {
                Chain NewChain = new Chain();
                NewChain.PeakIndexes = ChainPeakList [ ChainIndex ].ToArray();
                NewChain.PeakMasses = new double [ NewChain.PeakIndexes.Length ];
                for ( int PeakIndex = 0; PeakIndex < NewChain.PeakMasses.Length; PeakIndex++ ) {
                    NewChain.PeakMasses [ PeakIndex ] = Masses [ NewChain.PeakIndexes [ PeakIndex ] ];
                }
                double [] BlockMasses = new double [ NewChain.PeakIndexes.Length - 1 ];
                for ( int BlockIndex = 0; BlockIndex < BlockMasses.Length; BlockIndex++ ) {
                    BlockMasses [ BlockIndex ] = NewChain.PeakMasses [ BlockIndex + 1 ] - NewChain.PeakMasses [ BlockIndex ];
                }
                NewChain.BlockMass = CArrayMath.Mean( BlockMasses );
                ChainMasses [ ChainIndex ] = NewChain.BlockMass;//for sort
                NewChain.BlockMassStdDev = CArrayMath.StandardDeviation( BlockMasses, NewChain.BlockMass );
                int ChainBlockIndex = FindKnownChainBlockIndex( NewChain.PeakMasses [ 1 ] - NewChain.PeakMasses [ 0 ], ChainPpmError );
                if ( ChainBlockIndex >= 0 ) {
                    NewChain.IdealBlockMass = KnownChainBlockMasses [ ChainBlockIndex ];
                    NewChain.Formula = KnownChainBlockNames[ ChainBlockIndex];
                }else{
                    NewChain.IdealBlockMass = 0;
                    NewChain.Formula = "N/A";
                }
                Chains [ ChainIndex ] = NewChain;
            }
            Array.Sort( ChainMasses, Chains );
            Data.Chains = Chains;
            CreateChainPeaks( Data );
        }
        public void CreateChainPeaks( Support.InputData Data ) {
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
        public int GetMaxChainsPeak( Support.InputData Data ) {
            int MaxChainsPeak = 0;
            int [] [] ChainIndexes = Data.ChainIndexes;
            for ( int PeakIndex = 0; PeakIndex < ChainIndexes.Length; PeakIndex++ ) {
                if ( MaxChainsPeak < ChainIndexes [ PeakIndex ].Length ) { MaxChainsPeak = ChainIndexes [ PeakIndex ].Length; }
            }
            return MaxChainsPeak;
        }
        public void CreateUniqueChains( Support.InputData Data, double PpmError ) {
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
        public void ChainsToFile( Support.InputData Data, string Filename ) {
            StreamWriter oStreamWriterChains = new StreamWriter( Filename );
            int MaxChainPeaks = 0;
            foreach ( Support.Chain CurChain in Data.Chains ) {
                if ( MaxChainPeaks < CurChain.PeakIndexes.Length ) { MaxChainPeaks = CurChain.PeakIndexes.Length; }
            }
            string HeadLine = string.Empty;
            for ( int Index = 0; Index < MaxChainPeaks; Index++ ) {
                HeadLine = HeadLine + "Index,";
            }
            HeadLine = HeadLine + "ExactMass,Name,";
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
                Line = Line + CurChain.IdealBlockMass.ToString( OutputAccuracity ) + ',' + CurChain.Formula + ',';
                Line = Line + CurChain.BlockMass.ToString( OutputAccuracity ) +  "," + CurChain.BlockMassStdDev.ToString( OutputAccuracity )  + "," + CurChain.PeakIndexes.Length;
                //add peak masses
                for ( int PeakIndex = 0; PeakIndex < CurChain.PeakIndexes.Length; PeakIndex++ ) {
                    Line = Line  + "," + CurChain.PeakMasses [ PeakIndex ].ToString( OutputAccuracity );
                }
                oStreamWriterChains.WriteLine( Line );
            }
            oStreamWriterChains.Close();
        }
        public void PeakChainsToFile( Support.InputData Data, string Filename ) {
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

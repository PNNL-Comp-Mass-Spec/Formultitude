using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using NationalInstruments.Analysis;
using NationalInstruments.Analysis.Monitoring;

using FileReader;

namespace TestSpectrumNoiseLevel {
    public partial class TestSpectrumNoiseLevelForm : Form {

        const double PPM = 1e6;//parts per million
        public double PpmToError( double Mass, double ErrorPPM ) { return Mass * ErrorPPM / PPM; }
        public double PPMMassError( double Mass1, double Mass2 ) {
            if( Mass1 < Mass2 ) { return ( Mass2 - Mass1 ) / Mass1 * PPM; } else { return ( Mass1 - Mass2 ) / Mass2 * PPM; }
        }
        public double LeftPPMErrorEdge( double Mass, double PPMError ) {
            return Mass * PPM / ( PPM + PPMError);
        }
        public double RightPPMErrorEdge( double Mass, double PPMError ) {
            return Mass * ( PPM + PPMError) / PPM;
        }
        public TestSpectrumNoiseLevelForm() {
            InitializeComponent();
        }

        private void textBoxDropRawData_DragEnter( object sender, DragEventArgs e ) {
            if( e.Data.GetDataPresent( DataFormats.FileDrop ) == true ) {
                e.Effect = DragDropEffects.Copy;
            }
        }
        private void textBoxDropRawData_DragDrop( object sender, DragEventArgs e ) {
            string [] Filenames = ( string [] ) e.Data.GetData( DataFormats.FileDrop );
            double NoiseGain = ( double ) numericUpDownNoiseGain.Value;
            bool SubstractNoiseLevel = checkBoxSubstractNoiseLevel.Checked;

            double [] MZs;
            double [] Abundances;
            double [] SNs;
            double [] Resolutions;
            double [] RelAbundances;
            FileReader.FileReader.ReadFile( Filenames [ 0 ], out MZs, out Abundances, out SNs, out Resolutions, out RelAbundances );
            RawSignal.LineStyle = NationalInstruments.UI.LineStyle.Solid;
            RawSignal.PointStyle = NationalInstruments.UI.PointStyle.None;
            RawSignal.PlotXY( MZs, Abundances );

            double [] NoiseMZs;
            double [] MinNoiseAbundances;
            double [] MeanNoiseAbundances;
            Spectrum.ConvertProfileToCentroid( MZs, Abundances, out NoiseMZs, out MinNoiseAbundances, out MeanNoiseAbundances );
            double [] MaxNoiseAbundances = new double [ MeanNoiseAbundances.Length ];

            for( int Index = 0; Index < MeanNoiseAbundances.Length - 1; Index++ ) {
                MaxNoiseAbundances [ Index ] = MeanNoiseAbundances [ Index ] + NoiseGain * ( MeanNoiseAbundances [ Index ] - MinNoiseAbundances [ Index ] );
            }
            MinNoise.LineStyle = NationalInstruments.UI.LineStyle.Solid;
            MinNoise.PointStyle = NationalInstruments.UI.PointStyle.SolidTriangleDown;
            MinNoise.PlotXY( NoiseMZs, MinNoiseAbundances );
            MaxNoise.LineStyle = NationalInstruments.UI.LineStyle.Solid;
            MaxNoise.PointStyle = NationalInstruments.UI.PointStyle.SolidCircle;
            MaxNoise.PlotXY( NoiseMZs, MaxNoiseAbundances );

            int Windows = NoiseMZs.Length;
            double WindowMZRange = ( MZs [ MZs.Length - 1 ] - MZs [ 0 ] ) / Windows;
            int WindowMZEndIndex = -1;

            List<string> FileOutput = new List<string>();
            for( int Window = 0; Window < Windows; Window++ ) {
                int WindowMZStartIndex = WindowMZEndIndex + 1;
                if( Window < Windows - 1 ) {
                    WindowMZEndIndex = Array.BinarySearch( MZs, MZs [ 0 ] + WindowMZRange * ( Window + 1 ) );
                    if( WindowMZEndIndex < 0 ) { WindowMZEndIndex = ~WindowMZEndIndex - 1; }
                } else {
                    WindowMZEndIndex = MZs.Length - 1;
                }

                for( int Index = WindowMZStartIndex; Index <= WindowMZEndIndex; Index++ ) {
                    if( Abundances [ Index ] <= MaxNoiseAbundances [ Window ] ) {
                        Abundances [ Index ] = 0;
                    } else {
                        if( SubstractNoiseLevel == true ) {
                            Abundances [ Index ] = Abundances [ Index ] - MeanNoiseAbundances [ Window ];
                        }
                    }
                }
            }

            /*
            PeakDetector PeakDetection = new PeakDetector( 0, 3, PeakPolarity.Peaks );
            double [] PeakMZs;
            double [] PeakAbundances;
            double [] TempPeakDerivations;
            PeakDetection.Detect( Abundances [ 0 ], true, out PeakAbundances, out PeakMZs, out TempPeakDerivations );
            for( int Index = 0; Index < PeakMZs.Length; Index++ ) {
                int LeftMZIndex = ( int ) Math.Floor( PeakMZs [ Index ] );
                PeakMZs [ Index ] = MZs [ 0 ] [ LeftMZIndex ] + ( MZs [ 0 ] [ LeftMZIndex + 1 ] - MZs [ 0 ] [ LeftMZIndex ] ) * ( PeakMZs [ Index ] - LeftMZIndex );
                FileOutput.Add( PeakMZs [ Index ].ToString() + ',' + PeakAbundances [ Index ] );
            }
            */
            for( int Index = 0; Index < Abundances.Length; Index++ ) {
                FileOutput.Add( MZs [ Index ].ToString() + ',' + Abundances [ Index ] );
            }
            System.IO.File.WriteAllLines( Filenames [ 0 ] + "0", FileOutput );
        }

        private void textBoxDropOurPeakFile_DragEnter( object sender, DragEventArgs e ) {
            if( e.Data.GetDataPresent( DataFormats.FileDrop ) == true ) {
                e.Effect = DragDropEffects.Copy;
            }
        }
        double [] OurMZs;
        double [] OurAbundances;
        private void textBoxDropOurPeakFile_DragDrop( object sender, DragEventArgs e ) {
            string [] Filenames = ( string [] ) e.Data.GetData( DataFormats.FileDrop );
            double [] SNs;
            double [] Resolutions;
            double [] RelAbundances;
            FileReader.FileReader.ReadFile( Filenames [ 0 ], out OurMZs, out OurAbundances, out SNs, out Resolutions, out RelAbundances );
        }

        private void textBoxDropBrukerPeakData_DragEnter( object sender, DragEventArgs e ) {
            if( e.Data.GetDataPresent( DataFormats.FileDrop ) == true ) {
                e.Effect = DragDropEffects.Copy;
            }
        }
        double [] BrukerMZs;
        double [] BrukerAbundances;
        private void textBoxDropBrukerPeakData_DragDrop( object sender, DragEventArgs e ) {
            string [] Filenames = ( string [] ) e.Data.GetData( DataFormats.FileDrop );
            double [] SNs;
            double [] Resolutions;
            double [] RelAbundances;
            FileReader.FileReader.ReadFile( Filenames [ 0 ], out BrukerMZs, out BrukerAbundances, out SNs, out Resolutions, out RelAbundances );
        }

        class MZData {
            public double Abundance;
            public int Type;
            public int Link;
            public double PPMError;
        }
        private void buttonCompare_Click( object sender, EventArgs e ) {
            double [] MZs = new double [ OurMZs.Length + BrukerMZs.Length ];
            OurMZs.CopyTo( MZs, 0 );
            Array.Copy( BrukerMZs, 0, MZs, OurMZs.Length, BrukerMZs.Length );

            MZData [] MZData = new MZData [ OurMZs.Length + BrukerMZs.Length ];

            for( int Index = 0; Index < OurMZs.Length; Index++ ) {
                MZData [ Index ] = new MZData();
                MZData [ Index ].Abundance = OurAbundances [ Index ];
                MZData [ Index ].Type = 1;
                MZData [ Index ].Link = -1;
                MZData [ Index ].PPMError = -1;
            }
            for( int Index = 0; Index < BrukerMZs.Length; Index++ ) {
                MZData [ OurMZs.Length + Index ] = new MZData();
                MZData [ OurMZs.Length + Index ].Abundance = OurAbundances [ Index ];
                MZData [ OurMZs.Length + Index ].Type = 2;
                MZData [ OurMZs.Length + Index ].Link = -1;
                MZData [ OurMZs.Length + Index ].PPMError = -1;
            }

            Array.Sort( MZs, MZData );
            List<string> SortedData = new List<string>();
            double PPMError = 1;
            SortedData.Add( MZs [ 0 ].ToString() + ',' + MZData [ 0 ].Abundance + ',' + MZData [ 0 ].Type + ',' + LeftPPMErrorEdge( MZs [ 0 ], PPMError ) + ',' + RightPPMErrorEdge( MZs [ 0 ], PPMError ) );
            for( int Index = 1; Index < MZs.Length; Index++ ) {
                double Mass = MZs [ Index ];
                SortedData.Add( Mass.ToString() + ',' + MZData [ Index ].Abundance + ',' + MZData [ Index ].Type + ',' + LeftPPMErrorEdge( Mass, PPMError ) + ',' + RightPPMErrorEdge( Mass, PPMError ) );
            }
            System.IO.File.WriteAllLines( "\\\\pnl\\projects\\MSSHARE\\Nikola\\For_Andrey\\21T_SRFA_UltimateDatasets\\SortedData.csv", SortedData );

            bool Repeat;
            do {
                Repeat = false;
                for( int Index = 0; Index < MZs.Length - 2; Index++ ) {
                    if( MZData [ Index ].Link >= 0 ) { continue; }//or Error >= 0
                    if( MZData [ Index + 1 ].Link >= 0 ) { continue; }//or Error >= 0

                    if( MZData [ Index ].Type == MZData [ Index + 1 ].Type ) { continue; }
                    PPMError = PPMMassError( MZs [ Index ], MZs [ Index + 1 ] );
                    //if( PPMError > PPMErrors[ ErrorIndex]){ continue;}

                    double NextPPMError = PPMMassError( MZs [ Index + 2 ], MZs [ Index + 1 ] );
                    if( ( MZData [ Index + 2 ].Link < 0 )
                            && ( MZData [ Index + 1 ].Type != MZData [ Index + 2 ].Type )
                            && ( PPMError > NextPPMError ) ) {
                        Repeat = true;
                        continue;
                    };
                    MZData [ Index ].Link = Index + 1;
                    MZData [ Index ].PPMError = PPMError;
                    MZData [ Index + 1 ].Link = Index;
                    MZData [ Index + 1 ].PPMError = PPMError;
                }
            } while( Repeat == true );
            List<string> FileOutput = new List<string>();
            //string format
            //OurMZ,OurAbundance,BrukerMZ,BrukerAbundance,Error
            FileOutput.Add( "OurMZ,OurAbundance,BrukerMZ,BrukerAbundance,PPPError" );
            for( int Index = 0; Index < MZs.Length - 1; Index++ ) {
                if( MZData [ Index ].Link < 0 ) {
                    if( MZData [ Index ].Type == 1 ) {
                        FileOutput.Add( MZs [ Index ].ToString() + ',' + MZData [ Index ].Abundance + ",-1,-1,-1" );
                    } else {
                        FileOutput.Add( "-1,-1," + MZs [ Index ] + ',' + MZData [ Index ].Abundance + ",-1" );
                    }
                } else {
                    if( MZData [ Index ].Type == 1 ) {
                        FileOutput.Add( MZs [ Index ].ToString() + ',' + MZData [ Index ].Abundance + "," + MZs [ Index + 1 ] + ',' + MZData [ Index + 1 ].Abundance + "," + MZData [ Index ].PPMError );
                    } else {
                        FileOutput.Add( MZs [ Index + 1 ].ToString() + ',' + MZData [ Index + 1 ].Abundance + "," + MZs [ Index ] + ',' + MZData [ Index ].Abundance + "," + MZData [ Index ].PPMError );
                    }
                    Index++;
                }
            }
            System.IO.File.WriteAllLines( "\\\\pnl\\projects\\MSSHARE\\Nikola\\For_Andrey\\21T_SRFA_UltimateDatasets\\OutputTTT.csv", FileOutput );
        }
    }
    class Spectrum {
        enum Datatype { Profile, Centroid };
        enum Type { LTQ, LTQFT};
        int MSN = 1;

        //double [] MZs;
        //double [] Abundances;

        class ExtremumMZSup {
            public bool ValleyPeak;
            public double Abundance;
        }

        public static void ConvertProfileToCentroid( double [] MZs, double [] Abundances, out double [] NoiseMZs, out double [] MinNoiseAbundances, out double [] MeanNoiseAbundances) {
            if( MZs == null || MZs.Length == 0 || Abundances == null || Abundances.Length == 0 || MZs.Length != Abundances.Length ) { throw new Exception( "Input arrays are incoorrect" ); }

            List<double> ListNoiseMZs1 = new List<double>();
            List<double> ListMinNoiseAbundances1 = new List<double>();
            List<double> ListMeanNoiseAbundances1 = new List<double>();

            int Windows = 1000;
            double WindowMZRange = ( MZs [ MZs.Length - 1 ] - MZs [ 0 ] ) / Windows;
            int WindowMZEndIndex = -1;

            for( int Window = 0; Window < Windows; Window++ ) {
                int WindowMZStartIndex = WindowMZEndIndex + 1;
                if( Window < Windows - 1 ) {
                    WindowMZEndIndex = Array.BinarySearch( MZs, MZs[ 0] + WindowMZRange * ( Window + 1 ) );
                    if( WindowMZEndIndex < 0 ) { WindowMZEndIndex = ~WindowMZEndIndex - 1; }
                } else {
                    WindowMZEndIndex = MZs.Length - 1;
                }
                double [] WindowAbundances = new double [ WindowMZEndIndex - WindowMZStartIndex + 1 ];
                
                double WeightAbundanceSum = 0;
                for( int Index = WindowMZStartIndex; Index <= WindowMZEndIndex; Index++ ) {
                    WindowAbundances [ Index - WindowMZStartIndex ] = Abundances [ Index ];
                    WeightAbundanceSum = WeightAbundanceSum + WindowAbundances [ Index - WindowMZStartIndex ];                    
                }
                Array.Sort( WindowAbundances);

                int FrequencyQuantity = ( int ) Math.Round( 1 + 3.3 * Math.Log( Windows ) );
                int WindowIndex = 0;
                for( ; ; ) {
                    int [] Frequencies = new int [ FrequencyQuantity ];
                    double FrequencyRange = ( WindowAbundances [ WindowAbundances.Length - 1 ] - WindowAbundances [ 0 ] ) / FrequencyQuantity;
                    for( int Index = 0; Index < WindowAbundances.Length; Index++ ) {
                        int Frequency = ( int ) Math.Floor( ( WindowAbundances [ Index ] - WindowAbundances [ 0 ] ) / FrequencyRange );
                        if( Frequency >= FrequencyQuantity ) {
                            Frequency = FrequencyQuantity - 1;
                        }
                        Frequencies [ Frequency ]++;
                    }

                    int MaxIndex = 0;
                    for( int Index = 1; Index < Frequencies.Length - 1; Index++ ) {
                        if( Frequencies [ MaxIndex ] < Frequencies [ Index ] ) { MaxIndex = Index; }
                    }
                    double Half = Frequencies [ MaxIndex ] / 2;
                    int LeftIndex = 0;
                    for( ; LeftIndex < Frequencies.Length - 1; LeftIndex++ ) {
                        if( Frequencies [ LeftIndex ] > Half ) { break; }
                    }
                    int RightIndex = Frequencies.Length - 1;
                    for( ; RightIndex >= 0; RightIndex-- ) {
                        if( Frequencies [ RightIndex ] > Half ) { break; }
                    }
                    if( RightIndex - LeftIndex < 5 ) {
                        FrequencyQuantity = 2 * FrequencyQuantity;
                        continue;
                    }
                    int GoodIndex = ( int ) Math.Ceiling( ( double ) ( LeftIndex + RightIndex ) / 2 );
                    WindowIndex = 0;
                    for( int Index = 0; Index <= GoodIndex; Index++ ) {
                        WindowIndex = WindowIndex + Frequencies [ Index ];
                    }
                    break;
                }
                double MeanWindowMoiseAbundance = WindowAbundances [ WindowIndex ];

                /*
                int MaxWeightIndex = WindowAbundances.Length - 1;
                for(;;){
                    double WeightAbundance = WeightAbundanceSum / ( MaxWeightIndex + 1 );
                    int NewMaxAbundanceIndex = Array.BinarySearch( WindowAbundances, ( WeightAbundance + WindowAbundances [ 0] ) / 2);
                    if( NewMaxAbundanceIndex < 0 ) { NewMaxAbundanceIndex = ~NewMaxAbundanceIndex - 1; }
                    for( int Index = MaxWeightIndex; Index > NewMaxAbundanceIndex; Index-- ) {
                        WeightAbundanceSum = WeightAbundanceSum - WindowAbundances [ Index ];
                    }    



                    if( Math.Abs( MaxWeightIndex / 2 - NewMaxAbundanceIndex ) < 0.05 * NewMaxAbundanceIndex ) { break;}

                    int NextGoodMaxIndex = Array.BinarySearch( WindowAbundances, WeightAbundance * 2 );
                    if( NextGoodMaxIndex < 0 ) { NextGoodMaxIndex = ~NextGoodMaxIndex - 1; }
                    if( MaxWeightIndex == NextGoodMaxIndex ) { break; }
                    for( int Index = MaxWeightIndex; Index > NextGoodMaxIndex; Index--){
                        WeightAbundanceSum = WeightAbundanceSum - WindowAbundances [ Index];
                    }
                    MaxWeightIndex = NextGoodMaxIndex;
                }
                if( MaxWeightIndex < WindowAbundances.Length / 2 ) {
                    MaxWeightIndex = MaxWeightIndex;
                }
                */

                ListNoiseMZs1.Add( ( MZs [ WindowMZStartIndex ] + MZs [ WindowMZEndIndex ] ) / 2 );
                ListMinNoiseAbundances1.Add( WindowAbundances [ 0 ] );
                ListMeanNoiseAbundances1.Add( MeanWindowMoiseAbundance );
            }
            NoiseMZs = ListNoiseMZs1.ToArray();
            MinNoiseAbundances = ListMinNoiseAbundances1.ToArray();
            MeanNoiseAbundances = ListMeanNoiseAbundances1.ToArray();

            /*
            //find peaks and valleys
            double MinAbundance = Abundances [ 0 ];
            double MaxAbundance = Abundances [ 0 ];
            foreach( double Abundance in Abundances ) {
                if( MinAbundance > Abundance ) { MinAbundance = Abundance; }
                if( MaxAbundance < Abundance ) { MaxAbundance = Abundance; }
            }
            PeakDetector PeakDetection = new PeakDetector( MinAbundance, 3, PeakPolarity.Peaks );            
            double [] TempPeakMZs;
            double [] TempPeakAbundances;
            double [] TempPeakDerivations;
            PeakDetection.Detect( Abundances, true, out TempPeakAbundances, out TempPeakMZs, out TempPeakDerivations );

            for( int Peak = 0; Peak < TempPeakAbundances.Length; Peak++ ) {
                if( TempPeakAbundances [ Peak ] < 0 ) {
                    TempPeakAbundances [ Peak ] = TempPeakAbundances [ Peak ];
                }
            }
            PeakDetector ValleyDetection = new PeakDetector( MaxAbundance, 3, PeakPolarity.Valleys );
            double [] TempValleyMZs;
            double [] TempValleyAbundances;
            double [] TempValleyDerivations;            
            ValleyDetection.Detect( Abundances, true, out TempValleyAbundances, out TempValleyMZs, out TempValleyDerivations );

            for( int Peak = 0; Peak < TempValleyAbundances.Length; Peak++ ) {
                if( TempValleyAbundances [ Peak ] < 0 ) {
                    TempValleyAbundances [ Peak ] = TempValleyAbundances [ Peak ];
                }
            }

            //create extremum data
            int Extremus = TempPeakMZs.Length + TempValleyMZs.Length;
            double [] ExtremumMZs = new double [ Extremus ];
            ExtremumMZSup [] ExtremumMZSups = new ExtremumMZSup [ Extremus ];

            for( int Index = 0; Index < Extremus; Index++ ) {
                ExtremumMZSup TempExtremumMZSup = new ExtremumMZSup();
                ExtremumMZSups [ Index ] = TempExtremumMZSup;
                if( Index < TempPeakMZs.Length ) {
                    ExtremumMZs [ Index ] = TempPeakMZs [ Index ];
                    TempExtremumMZSup.ValleyPeak = true;
                    TempExtremumMZSup.Abundance = TempPeakAbundances [ Index ];
                } else {
                    int ValleyIndex = Index - TempPeakMZs.Length;
                    ExtremumMZs [ Index ] = TempValleyMZs [ ValleyIndex ];
                    TempExtremumMZSup.ValleyPeak = false;
                    TempExtremumMZSup.Abundance = TempValleyAbundances [ ValleyIndex ];
                }              
            }
            Array.Sort( ExtremumMZs, ExtremumMZSups );

            int ExtremusPerStep = ( int ) Math.Round( ( double) ExtremumMZs.Length / 10);
            double [] StepAbundances = new double [ ExtremusPerStep ];
            int [] StepMZLinks = new int [ ExtremusPerStep ];

            List<double> ListNoiseMZs = new List<double>();
            List<double> ListMinNoiseAbundances = new List<double>();
            List<double> ListMeanNoiseAbundances = new List<double>();
            List<double> ListGoodPeakMZs = new List<double>();
            List<double> ListPeakAbundances = new List<double>();
            for( int StartExtremum = 0; StartExtremum < Extremus; StartExtremum = StartExtremum + ExtremusPerStep ) {
                if( ExtremusPerStep > ( Extremus - StartExtremum ) ) { ExtremusPerStep = Extremus - StartExtremum; }

                //prepare arrays for this step
                //Calculate abundance weighted arithmetic mean           
                double AbundanceSum = 0;// (AbundanceSum/Count) is weighted arithmetic mean!!!
                for( int Index = 0; Index < ExtremusPerStep; Index ++ ) {
                    StepAbundances [ Index ] = ExtremumMZSups [ StartExtremum + Index ].Abundance;
                    StepMZLinks [ Index ] = StartExtremum + Index;
                    AbundanceSum += StepAbundances [ Index ];
                }
                Array.Sort( StepAbundances, StepMZLinks );

                int GoodExtremum = ExtremusPerStep - 1;
                double NoiseMean = -1;
                for( ; GoodExtremum > 0; GoodExtremum-- ) {
                    NoiseMean = ( StepAbundances [ 0 ] + StepAbundances [ GoodExtremum ] ) / 2;
                    double NoiseWeightedArithmeticMean = AbundanceSum / ( GoodExtremum + 1 );
                    if( NoiseMean <= NoiseWeightedArithmeticMean ) { break; }
                    AbundanceSum = AbundanceSum - StepAbundances [ GoodExtremum ];
                    if( ExtremumMZSups[ StepMZLinks[ GoodExtremum] ].ValleyPeak == true){     
                        double ExtremumMZ = ExtremumMZs[ StepMZLinks [ GoodExtremum ] ];
                        int ExtremumMZIndex = (int) Math.Floor( ExtremumMZ);
                        ExtremumMZ = MZs[ ExtremumMZIndex] + (  MZs[ ExtremumMZIndex + 1] -  MZs[ ExtremumMZIndex]) * (ExtremumMZ - ExtremumMZIndex);
                        ListGoodPeakMZs.Add( ExtremumMZ );
                        ListPeakAbundances.Add( StepAbundances [ GoodExtremum ] );
                    }
                }
                if( GoodExtremum < ExtremusPerStep / 2 ) {
                    GoodExtremum = GoodExtremum;
                }
                double MidStepMZ = ( MZs[ ( int ) Math.Round( ExtremumMZs [ StartExtremum + ExtremusPerStep - 1 ])] + MZs[ ( int ) Math.Round( ExtremumMZs [ StartExtremum ] ) ] ) / 2;
                ListNoiseMZs.Add( MidStepMZ );
                ListMinNoiseAbundances.Add( StepAbundances[ 0]);
                ListMeanNoiseAbundances.Add( NoiseMean );
            }
            PeakMZs = ListGoodPeakMZs.ToArray();
            PeakAbundances = ListPeakAbundances.ToArray();
            NoiseMZs = ListNoiseMZs.ToArray();
            MinNoiseAbundances = ListMinNoiseAbundances.ToArray();
            MeanNoiseAbundances = ListMeanNoiseAbundances.ToArray();
            */
            /*
            //find noise mean
            double NoiseMean = 0;
            for( ; ; ) {
                for( int Extremum = StartExtremum; Extremum < StartExtremum + ExtremusPerBase; Extremum++ ) {
                    if( ExtremumMZSups [ Extremum ].Good == false ) { continue; }
                    double Abundance = ExtremumMZSups [ Extremum ].Abundance;                    
                    NoiseMean = NoiseMean + Abundance;
                }
                NoiseMean = NoiseMean / ExtremusPerBase;
                double BaseMax = NoiseMin + ( NoiseMean - NoiseMin ) * 2;

                double ExtremumBinRange = ( BaseMax - NoiseMin ) / ExtremumBins;
                int [] Bins = new int [ ExtremumBins ];
                for( int Extremum = 0; Extremum < ExtremusPerBase; Extremum++ ) {
                    int Bin = ( int ) Math.Floor( ( ExtremumMZSups [ Extremum ].Abundance - NoiseMin ) / ExtremumBinRange );
                    if( Bin < 0 ) { Bin = 0; } else if( Bin >= ExtremumBins ) {
                        ExtremumMZSups [ Extremum ].Good = false;
                        continue;
                    }
                    Bins [ Bin ]++;
                }

                //Weighted arithmetic mean
                double Sum = 0;
                double WeightedArithmeticMean = 0;
                for( int Bin = 0; Bin < ExtremumBins; Bin++ ) {
                    Sum += Bins [ Bin ];
                    WeightedArithmeticMean += ( Bin - 1 ) * Bins [ Bin ];
                }
                WeightedArithmeticMean = WeightedArithmeticMean / Sum;
                if( Math.Abs( ExtremumBins / 2 - WeightedArithmeticMean) < 0.5 ) {
                    break;
                }
                //high abundance peaks are not good
                for( int Extremum = 0; Extremum < ExtremusPerBase; Extremum++ ) {
                    if( ){ ExtremumMZSups [ Extremum ].Good == false;}
                }
            }
            */

            /*
            double MinPeakAbundance = PeakAbundances[ 0];
            double MedianPeakAbundance = 0;
            foreach( double PeakAbundance in PeakAbundances ) {
                if( MinPeakAbundance > PeakAbundance ) { MinPeakAbundance = PeakAbundance; }
                MedianPeakAbundance = MedianPeakAbundance + PeakAbundance;
            }
            foreach( double ValleyAbundance in ValleyAbundances ) {
                if( MinPeakAbundance > ValleyAbundance ) { MinPeakAbundance = ValleyAbundance; }
                MedianPeakAbundance = MedianPeakAbundance + ValleyAbundance;
            }
            MedianPeakAbundance = MedianPeakAbundance / ( PeakAbundances.Length + ValleyAbundances.Length );
             */ 
        }

        /*
        const short NoiseThresholdBins = 100;
        const double NoiseThresholdGain = 1.5;
        public double CalculateNoiseThreshold(){
            double YMin = YPeaks[ 0];
    		double YMean = 0;
		    foreach( double YPeak in YPeaks){
			    YMean += YPeak;
                if( YMin > YPeak) { YMin = YPeak;}
            }
		    YMean = YMean / Peaks;
		
		    double BinSize = ( YMean - YMin) / NoiseThresholdBins / 2;
            double YMax = YMin + BinSize * NoiseThresholdBins;
		    int [] Bins = new int [ NoiseThresholdBins];
            int NonInBinsPeaks = 0;
		    foreach( double YPeak in YPeaks){
                int BinIndex = ( int) Math.Round( ( YPeak - YMin) / BinSize);
                if( BinIndex >= 0 && BinIndex < NoiseThresholdBins){
			        Bins[ BinIndex] ++;
                }else{
                    NonInBinsPeaks++;
                }
            }
            if( NonInBinsPeaks * 10 > Peaks){
                NonInBinsPeaks = NonInBinsPeaks;//error?
            }
            int MaxBinIndex = 0;
            for( int BinIndex = 1; BinIndex < NoiseThresholdBins; BinIndex ++){
                if( Bins[ MaxBinIndex] < Bins[ BinIndex]){ MaxBinIndex = BinIndex;}
            }
            if( MaxBinIndex >= NoiseThresholdBins / 2){
                MaxBinIndex = MaxBinIndex;//error?
            }
            return MaxBinIndex * BinSize * NoiseThresholdGain;
        }
         */ 

    }
}

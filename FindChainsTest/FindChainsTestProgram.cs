using System;
using System.Collections.Generic;
using System.Linq;
using Support;
using System.IO;

namespace FindChainsTest
{
    class FindChainsTestProgram
    {
        /*
        class Bin {
            public double LowMass;
            public double UpperMass;
            public int Count;
            public int PeakCount;//after removing peaks in 5*BinSize range
            public int TwoBinCount;
            public int ChainCount;
            public double BlockMass;
            public int BlockCount;
            public double ErrorMean;
            public double ErrorStdDev;
            public string Formula;
            public double FormulaMass;
        }
         * */
        /*
        public enum DistancePeakType { Pair, MPair, DM, MDM, Chain, MChain, UChain, MUChain, Isotope, MIsotope, Att };
        class DistancePeak {
            public int LowBinIndex;
            public int UpperBinIndex;
            public int PairCount;
            public double Mean;
            public double StdDev;
            public List<DistancePeakType> DistancePeakTypeList;
            public List<string> FormulaList;
            public List<double> FormulaDistanceList;
            public int ChainCount;
            public double ChainMean;
            public double ChainStdDev;
        }
         * */
        static void Main(string[] args)
        {
            Support.InputData InputData;
            var ChainBlocks = new CChainBlocks();
            //ChainBlocks.KnownMassBlocksFromFile( "\\\\picfs\\Research\\Projects\\TolicNikola\\AFAI_Manuscript\\Formularity area\\Improvements_Tests\\dmTransformations_MalakReal.csv" );
            ChainBlocks.KnownMassBlocksFromFile("dmTransformations_MalakReal.csv");
            //21 T
            //string Filename = "\\\\picfs\\Research\\Projects\\TolicNikola\\AFAI_Manuscript\\Ultimate_SRFA_Spectra\\21T_FT-ICR_Neg_Repro_2017_03_20_SRFA\\AverageSpectraProcessing\\sn2_SRFAI_01_21T_HESI_14Mar17_Leopard_100us-qb.txt";
            //string Filename = "C:\\Projects\\Proteomics\\Formularity\\FindChainsTest\\testdata\\21T\\Old\\21T_sn2_nocal_SRFAI_04_21T_HESI_14Mar17_Leopard_100us-qb.txt";
            //string Filename = "\\\\picfs\\Research\\Projects\\TolicNikola\\AFAI_Manuscript\\Formularity area\\Testing Dynamic Error\\Andrey\\testdata\\21T\\Nikola\\sn3_peaks.txt";
            //string Filename = "C:\\Projects\\Proteomics\\Formularity\\FindChainsTest\\testdata\\21T\\Nikola\\sn3_peaks.txt";
            //for Error vs SN
            //string Filename = "\\\\picfs\\Research\\Projects\\TolicNikola\\AFAI_Manuscript\\Formularity area\\Testing Dynamic Error\\Testing_SN_Cutoff\\21T_allpeaks_2016-02-11_20ppm_SRFAI_AGC-2E6_400K_NSI_1000avg.txt";

            //12T
            var Filename = "C:\\Temp\\textGarbage\\12T_NotCalibrated.txt";
            //for Error vs SN
            //string Filename = "\\\\picfs\\Research\\Projects\\TolicNikola\\AFAI_Manuscript\\Formularity area\\Testing Dynamic Error\\Testing_SN_Cutoff\\12T_sn1_Cates_H2O_SRFAII_12Apr18_Alder_Infuse_p05_1_01_28394.xml";
            //string Filename = "\\\\picfs\\Research\\Projects\\TolicNikola\\AFAI_Manuscript\\Formularity area\\Testing Dynamic Error\\Testing_SN_Cutoff\\12T_NotCalibrated.txt";


            Support.CFileReader.ReadFile(Filename, out InputData);
            //Approach with high start ppm error
            const double MaxOffsetPpmError = 5;
            if (true)
            {
                var IsotopeFilename = "Isotope.inf";
                CIsotope.ConvertIsotopeFileIntoIsotopeDistanceFile(IsotopeFilename);
                var DistancePeaks = ChainBlocks.GetPairChainIsotopeStatistics(InputData);
                File.WriteAllText(Filename + "PairIsotopes.csv", ChainBlocks.PairChainIsotopeStatisticsToString(DistancePeaks));
            }
            if (false)
            {
                //test sn vs peak pair error
                var SNs = new double[] { 1, 2, 3, 5, 10, 15, 20 };
                var BlockMasses1 = new double[] { 2 * CElements.H, CElements.C, 2 * CElements.H + CElements.C, CElements.O };
                var Text = "";
                foreach (var SN in SNs)
                {
                    var CutMasses = new List<double>();
                    var CutSNs = new List<double>();
                    for (var PeakIndex = 0; PeakIndex < InputData.Masses.Length; PeakIndex++)
                    {
                        if (InputData.S2Ns[PeakIndex] > SN)
                        {
                            CutMasses.Add(InputData.Masses[PeakIndex]);
                            CutSNs.Add(InputData.S2Ns[PeakIndex]);
                        }
                    }
                    InputData.Masses = CutMasses.ToArray();
                    InputData.S2Ns = CutSNs.ToArray();
                    ChainBlocks.CalculateErrorDistribution(InputData, MaxOffsetPpmError, BlockMasses1);
                    Text = Text + "\r\nSN," + SN.ToString("F2") + ",Peaks," + InputData.Masses.Length + "\r\n";
                    Text = Text + InputData.ErrorDistributionToString();
                }
                File.WriteAllText(Filename + "ErrorDistVsSN.csv", Text);
            }

            if (false)
            {
                //peak pair distribution
                var Masses = InputData.Masses;
                var DistanceList = new List<double>();
                double MaxDistanceMass = 100;
                for (var LowPeakIndex = 0; LowPeakIndex < Masses.Length - 1; LowPeakIndex++)
                {
                    for (var UpperPeakIndex = LowPeakIndex + 1; UpperPeakIndex < Masses.Length; UpperPeakIndex++)
                    {
                        var CurDistance = Masses[UpperPeakIndex] - Masses[LowPeakIndex];
                        if (MaxDistanceMass < CurDistance) { break; }
                        DistanceList.Add(CurDistance);
                    }
                }
                DistanceList.Sort();

                ChainBlocks.CalculateErrorDistribution(InputData, MaxOffsetPpmError, new double[] { 12.00 });
                var BinSize = CPpmError.PpmToError(InputData.ErrDisMassMedians[0], InputData.ErrDisStdDevs[0]) * 3;
                var BinCount = (int)Math.Ceiling(MaxDistanceMass / BinSize) + 1;
                var BinCounts = new int[BinCount];
                for (var Index = 0; Index < DistanceList.Count - 1; Index++)
                {
                    var Bin = (int)Math.Round(DistanceList[Index] / BinSize);
                    BinCounts[Bin]++;
                }

                var MaxCount = CArrayMath.Max(BinCounts);
                var MinCount = MaxCount / 100;

                var TextDistance = "BlockMass,Counts";
                for (var Index = 1; Index < BinCounts.Length - 1; Index++)
                {
                    if ((BinCounts[Index - 1] > MinCount)
                    || (BinCounts[Index] > MinCount)
                    || (BinCounts[Index + 1] > MinCount))
                    {
                        TextDistance = TextDistance + "\r\n" + (Index * BinSize).ToString("F8") + "," + BinCounts[Index];
                    }
                }
                File.WriteAllText(Filename + "BlockMassDistribution.csv", TextDistance);

                var Distances = DistanceList.ToArray();
            }
            //Array.Sort( Distances );
            /*
            double MinDistance = 0.01;
            double MaxDistance = 50;
            List<double> WindowStartPeakIndexList = new List<double>();
            List<int> WindowPeakList = new List<int>();
            for ( int DistanceIndex = 0; DistanceIndex < Distances.Length;) {
                double UpperDistance = Distances [ DistanceIndex ] + MinDistance;
                if ( UpperDistance > MaxDistance ) { break; }
                if ( UpperDistance > Distances.Last() ) { break; }
                int Index = Array.BinarySearch( Distances, DistanceIndex + 1, Distances.Length - DistanceIndex - 1, UpperDistance);
                if ( Index < 0 ) { Index = ~Index; Index--; }
                WindowStartPeakIndexList.Add( Distances [ DistanceIndex ] );
                WindowPeakList.Add( Index - DistanceIndex + 1 );
                UpperDistance = Distances [ DistanceIndex ] + MinDistance / 3;
                int NextDistanceIndex = Array.BinarySearch( Distances, DistanceIndex + 1, Distances.Length - DistanceIndex - 1, UpperDistance );
                if ( NextDistanceIndex < 0 ) { NextDistanceIndex = ~NextDistanceIndex; }
                DistanceIndex = NextDistanceIndex;
            }

            string TextDistance = "BlockMass,Counts";
            for( int Index = 0; Index < WindowPeakList.Count; Index ++ ) {
                TextDistance = TextDistance + "\r\n" + WindowStartPeakIndexList [ Index ].ToString( "F8") + "," + WindowPeakList [ Index ];
            }
            File.WriteAllText( Filename + "BlockMassDistribution.csv", TextDistance );
            */

            if (false)
            {
                //special test C13 error, CH4O-1 error
                //string [] Names = new string [] { "C13", "H2", "C", "CH2", "O" };
                //double [] Sizes = new double [] { CElements.C13 - CElements.C, 2* CElements.H, CElements.C, 2* CElements.H + CElements.C, CElements.O };
                var Names = new string[] { "CH4O-1" };
                var Sizes = new double[] { 2 * CElements.H, CElements.C + 4 * CElements.H - CElements.O };
                var BlockMassData = new Tuple<int[], int[], double[]>[Sizes.Length];

                InputData.MaxPpmErrorGain = 1;
                ChainBlocks.CalculateErrorDistribution(InputData, MaxOffsetPpmError, new double[] { 2 * CElements.H });
                File.WriteAllText(Filename + "ErrorDistributionH2.csv", InputData.ErrorDistributionToString());
                ChainBlocks.FindChains1(InputData, 5, InputData.Masses.Last() + 1, new double[] { 2 * CElements.H });
                File.WriteAllText(Filename + "ChainsH2.csv", InputData.ChainsToString());

                ChainBlocks.CalculateErrorDistribution(InputData, MaxOffsetPpmError, new double[] { CElements.C + 4 * CElements.H - CElements.O });
                File.WriteAllText(Filename + "ErrorDistributionCH4O_.csv", InputData.ErrorDistributionToString());
                ChainBlocks.FindChains1(InputData, 5, InputData.Masses.Last() + 1, new double[] { CElements.C + 4 * CElements.H - CElements.O });
                File.WriteAllText(Filename + "ChainsCH4O_.csv", InputData.ChainsToString());

            }

            //string [] BlockNames = new string [] { "H2", "C", "CH2", "O", "NH" };
            //double [] BlockMasses = new double [] { 2 * CElements.H, CElements.C, 2 * CElements.H + CElements.C, CElements.O, CElements.H + CElements.N };
            var BlockNames = new string[] { "H2", "C", "CH2", "O" };
            var BlockMasses = new double[] { 2 * CElements.H, CElements.C, 2 * CElements.H + CElements.C, CElements.O };

            ChainBlocks.CalculateErrorDistribution(InputData, MaxOffsetPpmError, BlockMasses);
            File.WriteAllText(Filename + "ErrorDistribution.csv", InputData.ErrorDistributionToString(true));

            InputData.MaxPpmErrorGain = 1;
            ChainBlocks.FindChains1(InputData, 5, InputData.Masses.Last() + 1, BlockMasses);

            ChainBlocks.FindClusters(InputData);
            ChainBlocks.AssignIdealMassesTheBiggestCluster(InputData);
            File.WriteAllText(Filename + "Chains.csv", InputData.ChainsToString());
            File.WriteAllText(Filename + "Clusters.csv", InputData.ClustersToString());
            File.WriteAllText(Filename + "IdealMassesAfterCluster0.csv", InputData.IdealMassesToString());

            var NoTrendMasses = ChainBlocks.GetNoTrendErrorMasses(InputData.Masses, InputData.IdealMasses, InputData);
            InputData.Masses = NoTrendMasses;
            File.WriteAllText(Filename + "ErrorTrend.csv", InputData.ErrorTrendToString());

            ChainBlocks.CalculateErrorDistribution(InputData, MaxOffsetPpmError, BlockMasses);
            File.WriteAllText(Filename + "ErrorDistributionNoTrend.csv", InputData.ErrorDistributionToString(true));

            InputData.MaxPpmErrorGain = 1;
            ChainBlocks.FindChains1(InputData, 5, InputData.Masses.Last() + 1, BlockMasses);

            ChainBlocks.FindClusters(InputData);
            ChainBlocks.AssignIdealMassesTheBiggestCluster(InputData);
            File.WriteAllText(Filename + "ChainsNoTrend.csv", InputData.ChainsToString());
            File.WriteAllText(Filename + "ClustersNoTrend.csv", InputData.ClustersToString());
            File.WriteAllText(Filename + "IdealMassesAfterCluster0NoTrend.csv", InputData.IdealMassesToString());


            //ChainBlocks.AssignC13IdealMasses( InputData, 1 );
            //File.WriteAllText( Filename + "IdealMassesAfterCluster0C13.csv", InputData.IdealMassesToString() );

            //double C13BlockMass = CElements.C13 - CElements.C;
            //string IsotopicText = "ParentPeakIndex,ParentPeakMass,C13PeakIndex,C13PeakMass,PpmError";
            //for ( int IsotopicPeakIndex = 0; IsotopicPeakIndex < InputData.IsotopicParentPeaks.Length; IsotopicPeakIndex++ ) {
            //    int ParentPeakIndex = InputData.IsotopicParentPeaks[ IsotopicPeakIndex];
            //    double ParentPeakMass = InputData.Masses [ ParentPeakIndex ];
            //    int C13PeakIndex = InputData.IsotopicChildPeaks [ IsotopicPeakIndex ];
            //    double C13PeakMass = InputData.Masses [ C13PeakIndex ];
            //    double PpmError = CPpmError.SignedMassErrorPPM( ParentPeakMass + C13BlockMass, C13PeakMass );
            //    IsotopicText = IsotopicText + "\r\n" + ParentPeakIndex + "," + ParentPeakMass.ToString( "F8" ) + "," + C13PeakIndex + "," + C13PeakMass.ToString( "F8" ) + "," + PpmError.ToString( "F8" );
            //}
            //File.WriteAllText( Filename + "C13.csv.", IsotopicText );
            ChainBlocks.AssignIdealMassesToRestPeaks(InputData);
            File.WriteAllText(Filename + "IdealMassesNoTrend.csv.", InputData.IdealMassesToString());
            /*
            int [] SecondClusterPeakIndexes = InputData.GetClusterPeakIndexes( 1 );
            string Text = "Index,MassC13,Chain(ChainIndex/BlockMass/BlockMassStdDev/StartChainPeakIndex/PeakIndexes)";
            for ( int PeakIndex = 0; PeakIndex < InputData.Masses.Length; PeakIndex++ ) {
                Text = Text + "\r\n" + PeakIndex + ",";
                if( SecondClusterPeakIndexes.Contains( PeakIndex) == true){
                    Text = Text + InputData.Masses [ PeakIndex ].ToString( "F8" );
                    for ( int ChainIndex = 0; ChainIndex <InputData.Chains.Length; ChainIndex ++ ) {
                        Chain CurChain = InputData.Chains [ ChainIndex ];
                        if ( CurChain.ContainPeak( PeakIndex ) == false ) { continue; }
                        //if ( CurChain.StartChainPeakIndex == PeakIndex ) { continue; }
                        Text = Text + "," + ChainIndex + ";" + CurChain.BlockMassMean.ToString( "F8" ) + ";" + CurChain.PpmErrorStdDev.ToString( "F8") + ";"
                                + CurChain.ClusteringPeakIndex;
                        foreach ( int CurPeakIndex in CurChain.PeakIndexes ) {
                            Text = Text + ";" + CurPeakIndex;
                        }
                    }
                }else{
                    Text = Text + "0,0";
                }
            }
            File.WriteAllText( Filename + "Cluster1.csv.", Text );
            */
            //double dd = InputData.Masses[ 0];
            /*
            double PpmError = 1;
            int [] ParentPeakIndexes;
            int [] ChildPeakIndexes;
            double [] PpmErrors;
            double Distance = 5 * ( CElements.C + 2* CElements.H);
            ChainBlocks.FindPeaksByMassDistance( InputData, Distance, PpmError, out ParentPeakIndexes, out ChildPeakIndexes, out PpmErrors );
            string Text = "Index,Mass,Abundance,ParentIndex,ParentMass,ParentAbundance,PpmError";
            for ( int PeakIndex = 0; PeakIndex < ParentPeakIndexes.Length; PeakIndex++ ) {
                int ParentPeak =  ParentPeakIndexes [ PeakIndex ];
                int ChildPeak =  ChildPeakIndexes [ PeakIndex ];
                Text = Text + "\r\n" + ParentPeak + "," + InputData.Masses [ ParentPeak ].ToString( "F8" ) + "," + InputData.Abundances [ ParentPeak ].ToString( "F8" )
                        + "," + ChildPeak + "," + InputData.Masses [ ChildPeak ].ToString( "F8" ) + "," + InputData.Abundances [ ChildPeak ].ToString( "F8" )
                        + "," + PpmErrors[ PeakIndex ];
            }
            File.WriteAllText( Filename + "5CH2.csv.", Text );

            int PpmErrorCount = 1000;
            double MaxPpmError = 1;
            double MinPpmError = MaxPpmError / PpmErrorCount;
            //double [] PpmErrors = new double [ PpmErrorCount ];
            int [] IsotopicPeaks = new int [ PpmErrorCount ];
            string IsotopicText = "Index,Mass,Abundance,ParentIndex,ParentMass,ParentAbundance,PpmError";
            ChainBlocks.FindIsotopicPeaks( InputData, PpmError );
            double Charge1Distance = CElements.C13 - CElements.C;
            for ( int PeakIndex = 0; PeakIndex < InputData.IsotopicChildPeaks.Length; PeakIndex++ ) {
                int ParentPeakIndex = InputData.IsotopicChildPeaks [ PeakIndex ];
                if ( ParentPeakIndex > 0 ){
                    IsotopicText = IsotopicText + "\r\n" + PeakIndex + "," + InputData.Masses [ PeakIndex ].ToString( "F8" ) + "," + InputData.Abundances [ PeakIndex ].ToString( "F8" )
                            + "," + ParentPeakIndex + "," + InputData.Masses[ ParentPeakIndex].ToString("F8") + "," + InputData.Abundances[ParentPeakIndex].ToString( "F8")
                            + "," + CPpmError.CalculateRangePpmError( InputData.Masses [ PeakIndex ] + Charge1Distance, InputData.Masses [ ParentPeakIndex ] );
                }
            }
            File.WriteAllText( Filename + "Isotopics.csv.", IsotopicText );

            for ( int PpmErrorIndex = 0; PpmErrorIndex < PpmErrorCount; PpmErrorIndex++ ) {
                PpmErrors [ PpmErrorIndex ] = MinPpmError * ( PpmErrorIndex + 1 );
                ChainBlocks.FindIsotopicPeaks( InputData, PpmErrors [ PpmErrorIndex ] );
                IsotopicPeaks[ PpmErrorIndex] = InputData.IsotopicPeaks;
            }

            ChainBlocks.FindChains( InputData, 5, PpmError, PpmError, InputData.Masses.Last(), 0.5, InputData.Masses.Last(), true );
            */
        }
    }
}

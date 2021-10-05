using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

using System.IO;

using Support;

namespace FindChains
{
    public partial class FindChainsForm : Form
    {
        public FindChainsForm()
        {
            InitializeComponent();
            var Error = CPpmError.ErrorToPpm(100, 1);
            numericUpDownFrequencyError.Enabled = checkBoxFrequency.Checked;
            oCChainBlocks = new CChainBlocks();
        }

        private readonly CChainBlocks oCChainBlocks;

        //PeakIndex
        private double[] Masses;
        private double[] Abundances;
        private double[] S2Ns;
        private double[] Resolutions;
        private double[] RelAbundances;

        private class PeakLink
        {
            public int BinIndex;
            public int PairIndex;
        }
        //BinIndex + PairIndex + LinkIndex
        private List<int>[] BinLeftPeaks;
        private List<int>[] BinRightPeaks;
        private List<PeakLink>[][] BinLeftLinks;
        private List<PeakLink>[][] BinRightLinks;

        private List<int> CreateChain(PeakLink oPeakLink)
        {
            var BinIndex = oPeakLink.BinIndex;
            var PairIndex = oPeakLink.PairIndex;

            if (BinRightLinks[BinIndex].Length == 0 || BinRightLinks[BinIndex][PairIndex].Count == 0)
            {
                var StartChain = new List<int>();
                StartChain.Add(BinRightPeaks[BinIndex][PairIndex]);
                return StartChain;
            }
            var NextLink = BinRightLinks[BinIndex][PairIndex][0];
            var TempChain = CreateChain(NextLink);
            TempChain.Insert(0, BinRightPeaks[BinIndex][PairIndex]);
            return TempChain;
        }

        private void checkBoxFrequency_CheckedChanged(object sender, EventArgs e)
        {
            numericUpDownFrequencyError.Enabled = checkBoxFrequency.Checked;
        }

        private class PeakMzError
        {
            public int PeakIndex;
            public double Mz;
            public double NewMz;
            public double PpmError;
            public int Chain;
        }

        private void textBoxChainBlockMasses_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void textBoxChainBlockMasses_DragDrop(object sender, DragEventArgs e)
        {
            var Filename = ((string[])e.Data.GetData(DataFormats.FileDrop))[0];
            oCChainBlocks.KnownMassBlocksFromFile(Filename);
        }

        private void textBoxSpectraFile_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void textBoxSpectraFile_DragDrop(object sender, DragEventArgs e)
        {
            var Filenames = (string[])e.Data.GetData(DataFormats.FileDrop);

            for (var FileIndex = 0; FileIndex < Filenames.Length; FileIndex++)
            {
                var FileExtension = Path.GetExtension(Filenames[FileIndex]);
                //read from files

                Support.CFileReader.ReadFile(Filenames[FileIndex], out Masses, out Abundances, out S2Ns, out Resolutions, out RelAbundances);
                var RawData = new Support.InputData();
                Support.CFileReader.ReadFile(Filenames[FileIndex], out RawData);

                //cut data
                var SettingCount = 0;

                if (checkBoxS2N.Checked) { SettingCount++; }
                if (checkBoxUseRelAbundance.Checked) { SettingCount++; }
                var CurSettings = new Support.CFileReader.CutSettings[SettingCount];
                var SettingIndex = 0;

                if (checkBoxS2N.Checked)
                {
                    CurSettings[SettingIndex] = new CFileReader.CutSettings();
                    CurSettings[SettingIndex].CutType = Support.CFileReader.ECutType.S2N;
                    CurSettings[SettingIndex].Min = (double)numericUpDownS2N.Value;
                    CurSettings[SettingIndex].Max = -1;
                    SettingIndex++;
                }
                if (checkBoxUseRelAbundance.Checked)
                {
                    CurSettings[SettingIndex] = new CFileReader.CutSettings();
                    CurSettings[SettingIndex].CutType = Support.CFileReader.ECutType.RelAbundance;
                    CurSettings[SettingIndex].Min = (double)numericUpDownMinRelAbundance.Value;
                    CurSettings[SettingIndex].Max = -1;
                }

                var Filename = Path.GetDirectoryName(Filenames[FileIndex]) + "\\" + Path.GetFileNameWithoutExtension(Filenames[FileIndex]);

                if (checkBoxPPMProcess.Checked)
                {
                    var MaxChainStartMass = (double)numericUpDownMaxPeakToStartChain.Value;
                    var MinPeaksInChain = (int)numericUpDownMinPeaksInChain.Value;
                    var PeakPpmError = (double)numericUpDownPpmError.Value;

                    Support.CFileReader.CutData(RawData, out var Data, CurSettings);
                    oCChainBlocks.FindChains(RawData, MinPeaksInChain, PeakPpmError, 3 * PeakPpmError, MaxChainStartMass, 0, RawData.Masses[RawData.Masses.Length - 1], checkBoxUseKnownChainBlocks.Checked);

                    oCChainBlocks.CreateUniqueChains(RawData, PeakPpmError);
                    File.WriteAllText(Filename + "UniqueChains.csv", RawData.ChainsToString());

                    //find clusters based on chains
                    oCChainBlocks.FindClusters(RawData);
                    var IsInChainCluster = new bool[RawData.Chains.Length];

                    var ChainClusters = new List<List<int>>();

                    for (var LeftChainIndex = 0; LeftChainIndex < RawData.Chains.Length; LeftChainIndex++)
                    {
                        if (IsInChainCluster[LeftChainIndex]) { continue; }
                        var ChainCluster = new List<int>();
                        ChainCluster.Add(LeftChainIndex);
                        IsInChainCluster[LeftChainIndex] = true;

                        var ClusterPeaksD = new Dictionary<int, int>();

                        foreach (var PeakIndex in RawData.Chains[LeftChainIndex].PeakIndexes)
                        {
                            ClusterPeaksD.Add(PeakIndex, PeakIndex);
                        }
                        var New = false;

                        for (var RightChainIndex = LeftChainIndex + 1; RightChainIndex < RawData.Chains.Length; RightChainIndex++)
                        {
                            if (IsInChainCluster[RightChainIndex]) { continue; }
                            foreach (var ComparingIndex in RawData.Chains[RightChainIndex].PeakIndexes)
                            {
                                if (ClusterPeaksD.Contains(new KeyValuePair<int, int>(ComparingIndex, ComparingIndex)))
                                {
                                    ChainCluster.Add(RightChainIndex);
                                    IsInChainCluster[RightChainIndex] = true;

                                    foreach (var PeakIndex in RawData.Chains[RightChainIndex].PeakIndexes)
                                    {
                                        var qq = new KeyValuePair<int, int>(PeakIndex, PeakIndex);

                                        if (!ClusterPeaksD.Contains(qq))
                                        {
                                            ClusterPeaksD.Add(PeakIndex, PeakIndex);
                                        }
                                    }
                                    New = true;
                                    break;
                                }
                            }
                            if (New)
                            {
                                if (RightChainIndex == RawData.Chains.Length - 1)
                                {
                                    RightChainIndex = LeftChainIndex + 1;
                                    New = false;
                                }
                            }
                        }
                        ChainClusters.Add(ChainCluster);
                    }
                    {
                        var oStreamWriterClusters = new StreamWriter(Filename + "Clusters.csv");
                        var HeadLine = "Chain,Mass,PeakIndexes";
                        oStreamWriterClusters.WriteLine(HeadLine);

                        for (var ClusterIndex = 0; ClusterIndex < ChainClusters.Count; ClusterIndex++)
                        {
                            oStreamWriterClusters.WriteLine("Cluster " + (ClusterIndex + 1).ToString());

                            for (var ChainIndex = 0; ChainIndex < ChainClusters[ClusterIndex].Count; ChainIndex++)
                            {
                                var ChainNumber = ChainClusters[ClusterIndex][ChainIndex];
                                var Line = ChainNumber.ToString() + ',' + RawData.Chains[ChainNumber].BlockMassMean.ToString("F8");

                                foreach (var PeakIndex in RawData.Chains[ChainNumber].PeakIndexes)
                                {
                                    Line = Line + ',' + PeakIndex;
                                }
                                oStreamWriterClusters.WriteLine(Line);
                            }
                        }
                        oStreamWriterClusters.Close();
                    }

                    //find the biggest cluster (very simple)
                    var ClusterIndexes = new int[ChainClusters.Count];
                    var ClusterCounts = new int[ChainClusters.Count];

                    for (var Index = 0; Index < ClusterCounts.Length; Index++)
                    {
                        ClusterIndexes[Index] = Index;
                        ClusterCounts[Index] = ChainClusters[Index].Count;
                    }
                    Array.Sort(ClusterCounts, ClusterIndexes);
                    var BiggestChainCluster = ChainClusters[ClusterIndexes[ClusterIndexes.Length - 1]];

                    //error
                    var ClusterPeaks = 0;
                    var MinClsuterChainIndex = -1;

                    for (var ClusterChainIndex = 0; ClusterChainIndex < BiggestChainCluster.Count; ClusterChainIndex++)
                    {
                        var ChainIndex = BiggestChainCluster[ClusterChainIndex];
                        ClusterPeaks += RawData.Chains[ChainIndex].PeakIndexes.Length;

                        if (MinClsuterChainIndex == -1)
                        {
                            MinClsuterChainIndex = ClusterChainIndex;
                        }
                        else if (RawData.Chains[BiggestChainCluster[MinClsuterChainIndex]].PeakIndexes[0] > RawData.Chains[BiggestChainCluster[ClusterChainIndex]].PeakIndexes[0])
                        {
                            MinClsuterChainIndex = ClusterChainIndex;
                        }
                    }
                    var SwapClusterIndex = BiggestChainCluster[0];
                    BiggestChainCluster[0] = BiggestChainCluster[MinClsuterChainIndex];
                    BiggestChainCluster[MinClsuterChainIndex] = SwapClusterIndex;

                    var ClusterChains = new bool[BiggestChainCluster.Count];
                    var ClusterPeakIndexes = new int[ClusterPeaks];

                    for (var ii = 0; ii < ClusterPeaks; ii++)
                    {
                        ClusterPeakIndexes[ii] = -1;
                    }
                    var PeakMzErrorArray = new PeakMzError[ClusterPeaks];
                    var StartPeakMzErrorArrayIndex = 0;
                    var EndPeakMzErrorArrayIndex = 0;
                    var NewEndPeakMzErrorArrayIndex = 0;
                    do
                    {
                        if (EndPeakMzErrorArrayIndex == 0)
                        {
                            //add Chain 0
                            var PeakIndexes = RawData.Chains[BiggestChainCluster[0]].PeakIndexes;
                            var FirstPeakMz = RawData.Masses[PeakIndexes[0]];

                            for (var PeakIndex = 0; PeakIndex < PeakIndexes.Length; PeakIndex++)
                            {
                                var NewPeakMzError = new PeakMzError();
                                NewPeakMzError.PeakIndex = PeakIndexes[PeakIndex];
                                NewPeakMzError.Mz = RawData.Masses[NewPeakMzError.PeakIndex];
                                NewPeakMzError.NewMz = FirstPeakMz + PeakIndex * RawData.Chains[BiggestChainCluster[0]].IdealBlockMass;
                                NewPeakMzError.Chain = BiggestChainCluster[0];
                                NewPeakMzError.PpmError = CPpmError.ErrorToPpm(NewPeakMzError.Mz, NewPeakMzError.NewMz - NewPeakMzError.Mz);
                                PeakMzErrorArray[EndPeakMzErrorArrayIndex] = NewPeakMzError;
                                EndPeakMzErrorArrayIndex++;
                            }
                            NewEndPeakMzErrorArrayIndex = EndPeakMzErrorArrayIndex;
                        }
                        else
                        {
                            StartPeakMzErrorArrayIndex = EndPeakMzErrorArrayIndex;
                            EndPeakMzErrorArrayIndex = NewEndPeakMzErrorArrayIndex;
                        }
                        for (var CurIndex = StartPeakMzErrorArrayIndex; CurIndex < EndPeakMzErrorArrayIndex; CurIndex++)
                        {
                            for (var ClusterChainIndex = 1; ClusterChainIndex < BiggestChainCluster.Count; ClusterChainIndex++)
                            {
                                if (ClusterChains[ClusterChainIndex]) { continue; }
                                var SearchPeakIndex1 = Array.BinarySearch(RawData.Chains[BiggestChainCluster[ClusterChainIndex]].PeakIndexes, PeakMzErrorArray[CurIndex].PeakIndex);

                                if (SearchPeakIndex1 < 0) { continue; }
                                var PeakIndexes = RawData.Chains[BiggestChainCluster[ClusterChainIndex]].PeakIndexes;
                                var FirstPeakMz = PeakMzErrorArray[CurIndex].NewMz - SearchPeakIndex1 * RawData.Chains[BiggestChainCluster[ClusterChainIndex]].IdealBlockMass;

                                for (var PeakIndex = 0; PeakIndex < PeakIndexes.Length; PeakIndex++)
                                {
                                    var NewPeakMzError = new PeakMzError();
                                    NewPeakMzError.PeakIndex = PeakIndexes[PeakIndex];
                                    NewPeakMzError.Mz = RawData.Masses[NewPeakMzError.PeakIndex];
                                    NewPeakMzError.NewMz = FirstPeakMz + PeakIndex * RawData.Chains[BiggestChainCluster[ClusterChainIndex]].IdealBlockMass;
                                    NewPeakMzError.Chain = BiggestChainCluster[ClusterChainIndex];
                                    NewPeakMzError.PpmError = CPpmError.ErrorToPpm(NewPeakMzError.Mz, NewPeakMzError.NewMz - NewPeakMzError.Mz);
                                    PeakMzErrorArray[NewEndPeakMzErrorArrayIndex] = NewPeakMzError;
                                    NewEndPeakMzErrorArrayIndex++;
                                }
                                ClusterChains[ClusterChainIndex] = true;
                            }
                        }
                    } while (NewEndPeakMzErrorArrayIndex > EndPeakMzErrorArrayIndex);
                    {
                        var oStreamWriterErrors = new StreamWriter(Filename + "Errors.csv");
                        var HeadLine = "PeakIndex,Mass,Abundance,NewMass,Chain,ChainBlockMass,PpmError";
                        oStreamWriterErrors.WriteLine(HeadLine);

                        for (var PeakIndex = 0; PeakIndex < PeakMzErrorArray.Length; PeakIndex++)
                        {
                            var CurPeakMzError = PeakMzErrorArray[PeakIndex];
                            oStreamWriterErrors.WriteLine(CurPeakMzError.PeakIndex.ToString() + "," + CurPeakMzError.Mz.ToString("F8") + "," + RawData.Abundances[CurPeakMzError.PeakIndex]
                                    + "," + CurPeakMzError.NewMz.ToString("F8") + "," + CurPeakMzError.Chain +
                                    "," + RawData.Chains[CurPeakMzError.Chain].IdealBlockMass.ToString("F8") + "," + CurPeakMzError.PpmError.ToString("F8"));
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

                if (checkBoxAmuProcess.Checked)
                {
                    CalculatePpmError(Masses);
                    var Error = (double)numericUpDownAbsError.Value;
                    var RangeMin = (double)numericUpDownRangeMin.Value;
                    var RangeMax = (double)numericUpDownRangeMax.Value;

                    var BinsPerErrorRange = (int)numericUpDownBinsPerErrorRange.Value;
                    var MinPeaksInChain = (int)numericUpDownMinPeaksInChain.Value;

                    //create Bin distances ( = left and right peaks)
                    var BinSize = Error / BinsPerErrorRange;
                    var BinCount = (int)Math.Ceiling((RangeMax - RangeMin) / BinSize);
                    BinLeftPeaks = new List<int>[BinCount];
                    BinRightPeaks = new List<int>[BinCount];

                    for (var BinIndex = 0; BinIndex < BinCount; BinIndex++)
                    {
                        BinLeftPeaks[BinIndex] = new List<int>();
                        BinRightPeaks[BinIndex] = new List<int>();
                    }
                    for (var Row = 0; Row < Masses.Length; Row++)
                    {
                        var RowMass = Masses[Row];

                        for (var Column = Row + 1; Column < Masses.Length; Column++)
                        {
                            var Distance = Masses[Column] - RowMass;

                            if (Distance < RangeMin) { continue; }
                            if (Distance >= RangeMax) { break; }
                            var BinIndex = (int)Math.Floor((Distance - RangeMin) / BinSize);
                            BinLeftPeaks[BinIndex].Add(Row);
                            BinRightPeaks[BinIndex].Add(Column);
                        }
                    }

                    //create Bin links
                    BinLeftLinks = new List<PeakLink>[BinCount][];
                    BinRightLinks = new List<PeakLink>[BinCount][];

                    for (var BinIndex = 0; BinIndex < BinCount; BinIndex++)
                    {
                        BinLeftLinks[BinIndex] = new List<PeakLink>[BinLeftPeaks[BinIndex].Count];
                        BinRightLinks[BinIndex] = new List<PeakLink>[BinLeftPeaks[BinIndex].Count];

                        for (var PairIndex = 0; PairIndex < BinLeftPeaks[BinIndex].Count; PairIndex++)
                        {
                            BinRightLinks[BinIndex][PairIndex] = new List<PeakLink>();
                            BinLeftLinks[BinIndex][PairIndex] = new List<PeakLink>();
                        }
                    }
                    for (var BinIndex = 0; BinIndex < BinCount; BinIndex++)
                    {
                        if (BinLeftPeaks[BinIndex].Count == 0) { continue; }//BinRightPeaks[ BinIndex ].Count == 0 must

                        var MaxBinIndex = BinIndex + 2 * BinsPerErrorRange + 1;//??? 2 *
                        if (MaxBinIndex >= BinCount) { MaxBinIndex = BinCount - 1; }

                        for (var PairIndex = 0; PairIndex < BinLeftPeaks[BinIndex].Count; PairIndex++)
                        {
                            var PairLeftPeak = BinLeftPeaks[BinIndex][PairIndex];
                            var PairRightPeak = BinRightPeaks[BinIndex][PairIndex];

                            for (var LinkBinIndex = BinIndex; LinkBinIndex < MaxBinIndex; LinkBinIndex++)
                            {
                                var LinkPairIndex = 0;

                                if (BinIndex == LinkBinIndex)
                                {
                                    LinkPairIndex = PairIndex + 1;
                                }
                                for (; LinkPairIndex < BinLeftPeaks[LinkBinIndex].Count; LinkPairIndex++)
                                {
                                    if (PairLeftPeak == BinRightPeaks[LinkBinIndex][LinkPairIndex])
                                    {
                                        var ToLeftLink = new PeakLink();
                                        ToLeftLink.BinIndex = LinkBinIndex;
                                        ToLeftLink.PairIndex = LinkPairIndex;
                                        BinLeftLinks[BinIndex][PairIndex].Add(ToLeftLink);

                                        var ToRightLink = new PeakLink();
                                        ToRightLink.BinIndex = BinIndex;
                                        ToRightLink.PairIndex = PairIndex;
                                        BinRightLinks[LinkBinIndex][LinkPairIndex].Add(ToRightLink);
                                    }

                                    if (PairRightPeak == BinLeftPeaks[LinkBinIndex][LinkPairIndex])
                                    {
                                        var ToRightLink = new PeakLink();
                                        ToRightLink.BinIndex = LinkBinIndex;
                                        ToRightLink.PairIndex = LinkPairIndex;
                                        BinRightLinks[BinIndex][PairIndex].Add(ToRightLink);

                                        var ToLeftLink = new PeakLink();
                                        ToLeftLink.BinIndex = BinIndex;
                                        ToLeftLink.PairIndex = PairIndex;
                                        BinLeftLinks[LinkBinIndex][LinkPairIndex].Add(ToLeftLink);
                                    }
                                }
                            }
                        }
                    }
                    if (checkBoxBinLinks.Checked)
                    {
                        var oStreamWriterBinLinks = new StreamWriter(Filename + "BinLinks.csv");
                        oStreamWriterBinLinks.WriteLine("Bin,Min,PairPeaks");

                        for (var BinIndex = 0; BinIndex < BinCount; BinIndex++)
                        {
                            var Line = BinIndex.ToString() + "," + (RangeMin + BinSize * BinIndex);

                            for (var PairIndex = 0; PairIndex < BinRightLinks[BinIndex].Length; PairIndex++)
                            {
                                Line = Line + "," + BinLeftPeaks[BinIndex][PairIndex] + "_" + BinRightPeaks[BinIndex][PairIndex];
                            }
                            oStreamWriterBinLinks.WriteLine(Line);
                        }
                        oStreamWriterBinLinks.Close();
                    }

                    var Chains = new List<List<int>>();

                    for (var BinIndex = 0; BinIndex < BinCount; BinIndex++)
                    {
                        //double BinDistance = RangeMin + BinIndex * BinSize;

                        for (var PairIndex = 0; PairIndex < BinLeftPeaks[BinIndex].Count; PairIndex++)
                        {
                            if (BinLeftLinks[BinIndex][PairIndex].Count != 0) { continue; }//chain starts when left peak doesn't have link
                            if (BinRightLinks[BinIndex][PairIndex].Count == 0) { continue; }//chain doesn't start when right peak doesn't have link
                            for (var LinkIndex = 0; LinkIndex < BinRightLinks[BinIndex][PairIndex].Count; LinkIndex++)
                            {
                                if (LinkIndex > 0) { break; }//??? take only first
                                var temp = BinRightLinks[BinIndex][PairIndex][LinkIndex];
                                var Chain = CreateChain(temp);
                                Chain.Insert(0, BinLeftPeaks[BinIndex][PairIndex]);
                                Chain.Insert(1, BinRightPeaks[BinIndex][PairIndex]);

                                if (Chain.Count < MinPeaksInChain) { continue; }
                                //bool Good = true;
                                //for( int Index = 1; Index < Chain.Count; Index++ ) {
                                //    if( Math.Abs( Masses[ Chain [ Index ] ] - Masses[ Chain [ Index - 1 ] ] - BinDistance) > Error) {
                                //        Good = false;
                                //        break;
                                //    }
                                //}
                                //if( Good == false ) { continue; }
                                Chains.Add(Chain);
                            }
                        }
                    }

                    if (checkBoxFileFormatPeakIndex.Checked)
                    {
                        var oStreamWriterChains = new StreamWriter(Filename + "ChainsAmuIndex.csv");
                        oStreamWriterChains.WriteLine("Distance,MaxError,Indexes");

                        for (var ChainIndex = 0; ChainIndex < Chains.Count; ChainIndex++)
                        {
                            var Chain = Chains[ChainIndex];
                            var Distance = Masses[Chain[1]] - Masses[Chain[0]];
                            var Line = Distance.ToString();

                            foreach (var Index in Chain)
                            {
                                Line = Line + "," + Index;
                            }
                            oStreamWriterChains.WriteLine(Line);
                        }
                        oStreamWriterChains.Close();
                    }
                    if (checkBoxFileFormatPeakMass.Checked)
                    {
                        var oStreamWriterChains = new StreamWriter(Filename + "ChainsAmuMass.csv");
                        oStreamWriterChains.WriteLine("Distance,MaxError,Mass");

                        for (var ChainIndex = 0; ChainIndex < Chains.Count; ChainIndex++)
                        {
                            var Chain = Chains[ChainIndex];
                            var Distance = Masses[Chain[1]] - Masses[Chain[0]];
                            var Line = Distance.ToString();

                            foreach (var Index in Chain)
                            {
                                Line = Line + "," + Masses[Index];
                            }
                            oStreamWriterChains.WriteLine(Line);
                        }
                        oStreamWriterChains.Close();
                    }
                    if (checkBoxFileFormatPeakAbundance.Checked)
                    {
                        var oStreamWriterChains = new StreamWriter(Filename + "ChainsAmuAbun.csv");
                        oStreamWriterChains.WriteLine("Distance,MaxError,Abundance");

                        for (var ChainIndex = 0; ChainIndex < Chains.Count; ChainIndex++)
                        {
                            var Chain = Chains[ChainIndex];
                            var Distance = Masses[Chain[1]] - Masses[Chain[0]];
                            var Line = Distance.ToString();

                            foreach (var Peak in Chain)
                            {
                                Line = Line + "," + Abundances[Peak];
                            }
                            oStreamWriterChains.WriteLine(Line);
                        }
                        oStreamWriterChains.Close();
                    }
                }
            }
        }

        public double CalculatePpmError(double[] Masses)
        {
            var MassesCount = Masses.Length;
            double RangeMin = 0;
            var MinBlockPeaksInSpectrum = 5;
            var RangeMax = Masses[MassesCount - 1] / MinBlockPeaksInSpectrum;
            var Error = 0.0002;
            var BinsPerErrorRange = 5;
            var BinSize = Error / BinsPerErrorRange;
            var BinCount = (int)Math.Ceiling((RangeMax - RangeMin) / BinSize);
            BinLeftPeaks = new List<int>[BinCount];
            BinRightPeaks = new List<int>[BinCount];

            for (var BinIndex = 0; BinIndex < BinCount; BinIndex++)
            {
                BinLeftPeaks[BinIndex] = new List<int>();
                BinRightPeaks[BinIndex] = new List<int>();
            }
            for (var Row = 0; Row < Masses.Length; Row++)
            {
                var RowMass = Masses[Row];

                for (var Column = Row + 1; Column < Masses.Length; Column++)
                {
                    var Distance = Masses[Column] - RowMass;

                    if (Distance < RangeMin) { continue; }
                    if (Distance >= RangeMax) { break; }
                    var BinIndex = (int)Math.Floor((Distance - RangeMin) / BinSize);
                    BinLeftPeaks[BinIndex].Add(Row);
                    BinRightPeaks[BinIndex].Add(Column);
                }
            }

            //create Bin links
            BinLeftLinks = new List<PeakLink>[BinCount][];
            BinRightLinks = new List<PeakLink>[BinCount][];

            for (var BinIndex = 0; BinIndex < BinCount; BinIndex++)
            {
                BinLeftLinks[BinIndex] = new List<PeakLink>[BinLeftPeaks[BinIndex].Count];
                BinRightLinks[BinIndex] = new List<PeakLink>[BinLeftPeaks[BinIndex].Count];

                for (var PairIndex = 0; PairIndex < BinLeftPeaks[BinIndex].Count; PairIndex++)
                {
                    BinRightLinks[BinIndex][PairIndex] = new List<PeakLink>();
                    BinLeftLinks[BinIndex][PairIndex] = new List<PeakLink>();
                }
            }
            for (var BinIndex = 0; BinIndex < BinCount; BinIndex++)
            {
                if (BinLeftPeaks[BinIndex].Count == 0) { continue; }//BinRightPeaks[ BinIndex ].Count == 0 must

                var MaxBinIndex = BinIndex + 2 * BinsPerErrorRange + 1;//??? 2 *
                if (MaxBinIndex >= BinCount) { MaxBinIndex = BinCount - 1; }

                for (var PairIndex = 0; PairIndex < BinLeftPeaks[BinIndex].Count; PairIndex++)
                {
                    var PairLeftPeak = BinLeftPeaks[BinIndex][PairIndex];
                    var PairRightPeak = BinRightPeaks[BinIndex][PairIndex];

                    for (var LinkBinIndex = BinIndex; LinkBinIndex < MaxBinIndex; LinkBinIndex++)
                    {
                        var LinkPairIndex = 0;

                        if (BinIndex == LinkBinIndex)
                        {
                            LinkPairIndex = PairIndex + 1;
                        }
                        for (; LinkPairIndex < BinLeftPeaks[LinkBinIndex].Count; LinkPairIndex++)
                        {
                            if (PairLeftPeak == BinRightPeaks[LinkBinIndex][LinkPairIndex])
                            {
                                var ToLeftLink = new PeakLink();
                                ToLeftLink.BinIndex = LinkBinIndex;
                                ToLeftLink.PairIndex = LinkPairIndex;
                                BinLeftLinks[BinIndex][PairIndex].Add(ToLeftLink);

                                var ToRightLink = new PeakLink();
                                ToRightLink.BinIndex = BinIndex;
                                ToRightLink.PairIndex = PairIndex;
                                BinRightLinks[LinkBinIndex][LinkPairIndex].Add(ToRightLink);
                            }

                            if (PairRightPeak == BinLeftPeaks[LinkBinIndex][LinkPairIndex])
                            {
                                var ToRightLink = new PeakLink();
                                ToRightLink.BinIndex = LinkBinIndex;
                                ToRightLink.PairIndex = LinkPairIndex;
                                BinRightLinks[BinIndex][PairIndex].Add(ToRightLink);

                                var ToLeftLink = new PeakLink();
                                ToLeftLink.BinIndex = BinIndex;
                                ToLeftLink.PairIndex = PairIndex;
                                BinLeftLinks[LinkBinIndex][LinkPairIndex].Add(ToLeftLink);
                            }
                        }
                    }
                }
            }

            //Save BinLinks to file
            var FilenameBinLinks = "c:\\temp\\BinLinks.csv";
            var oStreamWriterBinLinks = new StreamWriter(FilenameBinLinks);
            oStreamWriterBinLinks.WriteLine("Block size,Counts");

            for (var BinIndex = 0; BinIndex < BinCount; BinIndex++)
            {
                oStreamWriterBinLinks.WriteLine((RangeMin + BinSize * BinIndex + BinSize / 2).ToString() + "," + BinLeftLinks[BinIndex].Length);
            }
            oStreamWriterBinLinks.Close();

            //Max peak
            var MaxBinCount = BinLeftPeaks[0].Count;
            var MaxBinIndex1 = 0;

            for (var BinIndex = 1; BinIndex < BinCount; BinIndex++)
            {
                if (MaxBinCount < BinLeftPeaks[BinIndex].Count)
                {
                    MaxBinCount = BinLeftPeaks[BinIndex].Count;
                    MaxBinIndex1 = BinIndex;
                }
            }
            //find wigth on 10% MaxBinCount
            var Level10MaxBinCount = MaxBinCount / 10;
            var LeftBinIndex = MaxBinIndex1;

            for (var BinIndexShift = 1; ; BinIndexShift++)
            {
                var NextLeftBinIndex = MaxBinIndex1 - BinIndexShift;

                if (NextLeftBinIndex < 0) { break; }
                if (BinLeftPeaks[NextLeftBinIndex].Count < Level10MaxBinCount)
                {
                    break;
                }
                LeftBinIndex = NextLeftBinIndex;
            }
            var RightBinIndex = MaxBinIndex1;

            for (var BinIndexShift = 1; ; BinIndexShift++)
            {
                var NextRightBinIndex = MaxBinIndex1 + BinIndexShift;

                if (NextRightBinIndex >= BinCount) { break; }
                if (BinLeftPeaks[NextRightBinIndex].Count < Level10MaxBinCount)
                {
                    break;
                }
                RightBinIndex = NextRightBinIndex;
            }
            //find error
            var LinkErrors = new List<double>();
            var BestDistance = Masses[BinRightPeaks[MaxBinIndex1][0]] - Masses[BinLeftPeaks[MaxBinIndex1][0]];

            for (var BinIndex = LeftBinIndex; BinIndex <= RightBinIndex; BinIndex++)
            {
                for (var PairIndex = 0; PairIndex < BinLeftPeaks[BinIndex].Count; PairIndex++)
                {
                    var RightMass = Masses[BinRightPeaks[BinIndex][PairIndex]];
                    var LeftMass = Masses[BinLeftPeaks[BinIndex][PairIndex]];
                    var CurDistance = RightMass - LeftMass;
                    var LinkError = CPpmError.AbsPpmError(BestDistance, CurDistance);
                    LinkErrors.Add(LinkError);
                }
            }
            double MeanError = 0;

            foreach (var Error1 in LinkErrors)
            {
                MeanError += Error1;
            }
            MeanError /= LinkErrors.Count;
            textBoxError.Text = MeanError.ToString();

            double MaxError = 0;

            foreach (var Error1 in LinkErrors)
            {
                var dd = Math.Abs(Error1 - MeanError);

                if (MaxError < dd) { MaxError = dd; }
            }
            textBoxMaxError.Text = MaxError.ToString();

            return MeanError;
        }

        private void buttonCalLogParser_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void buttonCalLogParser_DragDrop(object sender, DragEventArgs e)
        {
            var Filenames = (string[])e.Data.GetData(DataFormats.FileDrop);
            var StartFile = "Calibration of ";

            for (var FileIndex = 0; FileIndex < Filenames.Length; FileIndex++)
            {
                var Lines = File.ReadAllLines(Filenames[FileIndex]);
                var Text = "Filename,MatchedPeaks,BeforeErrorAverage,BeforeErrorStdDev,AfterErrorAverage,AfterStdDev";

                for (var LineIndex = 0; LineIndex < Lines.Length; LineIndex++)
                {
                    if (!Lines[LineIndex].StartsWith(StartFile)) { continue; }
                    var HeaderParts = Lines[LineIndex].Split(new[] { ' ' });
                    Text = Text + "\r\n" + HeaderParts[2];
                    var MatchedPeaksParts = Lines[LineIndex + 2].Split(new[] { ' ', '(' });
                    Text = Text + ',' + MatchedPeaksParts[5];
                    LineIndex += 12;
                    var ErrorsBeforeCal = new List<double>();
                    var ErrorsAfterCal = new List<double>();

                    for (; LineIndex < Lines.Length; LineIndex++)
                    {
                        if (Lines[LineIndex].StartsWith("quadratic_calibration")) { break; }
                        var CalibrantParts = Lines[LineIndex].Split(new[] { '\t' });
                        ErrorsBeforeCal.Add(double.Parse(CalibrantParts[3]));
                        ErrorsAfterCal.Add(double.Parse(CalibrantParts[5]));
                    }
                    var AverageErrorBeforeCal = Support.CArrayMath.Mean(ErrorsBeforeCal.ToArray());
                    var StdDevErrorBeforeCal = Support.CArrayMath.StandardDeviation(ErrorsBeforeCal.ToArray(), AverageErrorBeforeCal);
                    var AverageErrorAfterCal = Support.CArrayMath.Mean(ErrorsAfterCal.ToArray());
                    var StdDevErrorAfterCal = Support.CArrayMath.StandardDeviation(ErrorsAfterCal.ToArray(), AverageErrorAfterCal);
                    Text = Text + ',' + AverageErrorBeforeCal.ToString("F8") + ',' + StdDevErrorBeforeCal.ToString("F8")
                            + ',' + AverageErrorAfterCal.ToString("F8") + ',' + StdDevErrorAfterCal.ToString("F8");
                }
                File.WriteAllText(Filenames[FileIndex] + ".csv", Text);
            }
        }
    }
}

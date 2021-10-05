using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml;

namespace Support
{
    public class Chain
    {
        public string Formula;
        public double IdealBlockMass;
        public int[] PeakIndexes;
        public bool ContainPeak(int PeakIndex)
        {
            if (PeakIndexes == null) { return false; }
            foreach (var CurPeakIndex in PeakIndexes)
            {
                if (CurPeakIndex == PeakIndex)
                {
                    return true;
                }
            }
            return false;
        }

        public double[] PeakMasses;
        public double BlockMassMean;
        public double PpmErrorStdDev;

        //during clustering
        public int ClusteringChainIndex;
        public int ClusteringPeakIndex;
        //during ideal mass assignment
        public double ClusteringMassError;

        public static string HeaderToString()
        {
            return "Formula,Ideal block mass,Peaks,Block mass mean,StdDev,ClusteringChainIndex, ClusteringPeakIndex,ClusteringMassError";
        }

        public string ToString(bool FullInfo = false)
        {
            var Text = Formula + "," + IdealBlockMass.ToString("F8") + "," + PeakIndexes.Length + "," + BlockMassMean.ToString("F8") + "," + PpmErrorStdDev.ToString("F8")
                       + "," + ClusteringChainIndex + "," + ClusteringPeakIndex + "," + ClusteringMassError.ToString("F8");

            if (!FullInfo) { return Text; }
            Text = Text + "\r\nPeak,Index,Mass";

            for (var Index = 0; Index < PeakIndexes.Length; Index++)
            {
                Text = Text + "\r\n" + Index + "," + PeakIndexes[Index] + "," + PeakMasses[Index].ToString("F8");
            }
            return Text;
        }
    }

    public class Cluster
    {
        public int[] ChainIndexes;
        public Cluster(int[] ChainIndexes)
        {
            this.ChainIndexes = ChainIndexes;
        }

        public static string HeaderString()
        {
            return "Chains,Peaks,MinMass,MaxMass,Even peaks,Odd peaks,Best chain,Best peak,Best score,Max error gain,Peaks with higher error,Max outlier error";
        }

        public string ToString(bool FullInfo = false)
        {
            var Text = ChainIndexes.Length + "," + PeakCount + "," + MinMass.ToString("F8") + "," + MaxMass.ToString("F8")
                       + "," + EvenPeakCount + ", " + OddPeakCount + "," + TheBestChainIndex + "," + TheBestPeakIndex + "," + TheBestScore.ToString("F3")
                       + "," + MaxPpmErrorGain.ToString("F8") + "," + HigherPpmErrorCount + "," + MaxOutlierPpmError;

            if (!FullInfo) { return Text; }
            Text = Text + "\r\nChain indexes:\r\n";

            foreach (var ChainIndex in ChainIndexes)
            {
                Text = Text + ChainIndex + ",";
            }
            return Text;
        }

        public int PeakCount;//unique peaks
        public int OddPeakCount;
        public int EvenPeakCount;
        public int TheBestChainIndex;//chain index
        public int TheBestPeakIndex;//peak index in chain
        public double TheBestScore;
        public double MinMass;
        public double MaxMass;
        public double MaxPpmErrorGain;
        public int HigherPpmErrorCount;
        public double MaxOutlierPpmError;
    }

    public enum IsotopicInteger { NotProcessed, Even, MostEven, Odd, MostOdd, None };
    public class InputData
    {
        public double[] Masses;
        public double[] Abundances;
        public double[] S2Ns;
        public double[] Resolutions;
        public double[] RelAbundances;

        public void Init()
        {
            IsotopicParentPeaks = Enumerable.Repeat<int>(-1, Masses.Length).ToArray();
            IsotopicChildPeaks = Enumerable.Repeat<int>(-1, Masses.Length).ToArray();
            ParentPeakIndexes = Enumerable.Repeat<int>(-1, Masses.Length).ToArray();
            Chains = new Chain[0];
            Clusters = new Cluster[0];
            IdealMasses = new double[Masses.Length];
        }

        //error distribution (ED)
        public int[][] ErrDisParentPeakIndexes;
        public int[][] ErrDisChildPeakIndexes;
        public double[][] ErrDisPpmErrors;
        public double[][] ErrDisBlockMasses;
        public double[] ErrDisLowPpmErrors;
        public double[] ErrDisUpperPpmErrors;

        public double[] ErrDisMassMedians;
        public double[] ErrDisMeans;
        public double[] ErrDisStdDevs;
        public int[] ErrDisPairCount;
        public double GetErrorStdDev(double Mass)
        {
            if (ErrDisMassMedians.Length == 0)
            {
                return 0;
            }
            if (Mass <= ErrDisMassMedians.First())
            {
                return ErrDisStdDevs.First();
            }
            else if (Mass >= ErrDisMassMedians.Last())
            {
                return ErrDisStdDevs.Last();
            }
            var RangeIndex = Array.BinarySearch(ErrDisMassMedians, Mass);

            if (RangeIndex < 0) { RangeIndex = ~RangeIndex; }
            return CArrayMath.LinearValue(Mass, ErrDisMassMedians[RangeIndex - 1], ErrDisMassMedians[RangeIndex], ErrDisStdDevs[RangeIndex - 1], ErrDisStdDevs[RangeIndex]);
        }

        public string ErrorDistributionToString(bool Full = false)
        {
            var Text = "MassMedian,LowError,HighError,ErrorMean,ErrorStdDev,Count";

            for (var RangeIndex = 0; RangeIndex < ErrDisMassMedians.Length; RangeIndex++)
            {
                Text = Text + "\r\n" + ErrDisMassMedians[RangeIndex].ToString("F8") + "," + ErrDisLowPpmErrors[RangeIndex] + "," + ErrDisUpperPpmErrors[RangeIndex]
                        + "," + ErrDisMeans[RangeIndex].ToString("F8") + "," + ErrDisStdDevs[RangeIndex].ToString("F8") + "," + ErrDisPairCount[RangeIndex];
            }
            if (!Full) { return Text; }

            var MaxPeakIndex = 0;
            Text = Text + "\r\n\r\n";

            foreach (var ErrDisParentPeakIndexesInRange in ErrDisParentPeakIndexes)
            {
                if (MaxPeakIndex < ErrDisParentPeakIndexesInRange.Length) { MaxPeakIndex = ErrDisParentPeakIndexesInRange.Length; }
                Text = Text + "ParentPeakIndex,ParentPeakMass,ChildPeakIndex,ChildPeakMass,PpmError,MassBlock,,";
            }
            for (var PeakIndex = 0; PeakIndex < MaxPeakIndex; PeakIndex++)
            {
                Text = Text + "\r\n";

                for (var RangeIndex = 0; RangeIndex < ErrDisParentPeakIndexes.Length; RangeIndex++)
                {
                    if (ErrDisParentPeakIndexes[RangeIndex].Length > PeakIndex)
                    {
                        Text = Text + ErrDisParentPeakIndexes[RangeIndex][PeakIndex] + "," + Masses[ErrDisParentPeakIndexes[RangeIndex][PeakIndex]].ToString("F8")
                                + "," + ErrDisChildPeakIndexes[RangeIndex][PeakIndex] + "," + Masses[ErrDisChildPeakIndexes[RangeIndex][PeakIndex]].ToString("F8")
                                + "," + ErrDisPpmErrors[RangeIndex][PeakIndex].ToString("F8") + "," + ErrDisBlockMasses[RangeIndex][PeakIndex].ToString("F8") + ",,";
                    }
                    else
                    {
                        Text = Text + ",,,,,,,";
                    }
                }
            }
            return Text;
        }

        //error trend
        public double[] ErrorTrend;
        public string ErrorTrendToString()
        {
            var Text = "MassInteger,Error(da)";
            var Write = false;

            for (var Index = 0; Index < ErrorTrend.Length; Index++)
            {
                if ((!Write) && double.IsNaN(ErrorTrend[Index])) { continue; }
                Write = true;
                Text = Text + "\r\n" + Index + "," + ErrorTrend[Index].ToString("F8");
            }
            return Text;
        }

        //isotopic peaks
        public int[] IsotopicParentPeaks;
        public int[] IsotopicChildPeaks;
        public int IsotopicPeaks;

        //chains
        public Chain[] Chains;
        public int[] ParentPeakIndexes;
        public int GetMaxChainLength()
        {
            var MaxChainLength = 0;

            foreach (var oChain in Chains)
            {
                if (MaxChainLength < oChain.PeakIndexes.Length) { MaxChainLength = oChain.PeakIndexes.Length; }
            }
            return MaxChainLength;
        }

        public string ChainsToString(bool FullInfo = false)
        {
            var Text = "Index," + Chain.HeaderToString();

            for (var ChainIndex = 0; ChainIndex < Chains.Length; ChainIndex++)
            {
                Text = Text + "\r\n" + ChainIndex + "," + Chains[ChainIndex].ToString(FullInfo);
            }
            return Text;
        }

        //clusters
        public Cluster[] Clusters;
        public int[] GetClusterPeakIndexes(int ClusterIndex)
        {
            if ((Clusters == null) || (Clusters.Length <= ClusterIndex))
            {
                throw new Exception("Clusters doesn't have ClusterIndex in GetClusterPeakIndexes function");
            }
            var PeakIndexSortedList = new SortedList<int, int>();

            foreach (var ChainIndex in Clusters[ClusterIndex].ChainIndexes)
            {
                var CurChain = Chains[ChainIndex];

                foreach (var PeakIndex in CurChain.PeakIndexes)
                {
                    if (!PeakIndexSortedList.ContainsKey(PeakIndex))
                    {
                        PeakIndexSortedList.Add(PeakIndex, PeakIndex);
                    }
                }
            }
            return PeakIndexSortedList.Keys.ToArray();
        }

        public int[] GetClusterChains(int ClusterIndex, int PeakIndex)
        {
            if ((Clusters == null) || (Clusters.Length <= ClusterIndex))
            {
                throw new Exception("Clusters doesn't have ClusterIndex in GetClusterPeakIndexes function");
            }
            var TempChainList = new List<int>();

            foreach (var ChainIndex in Clusters[ClusterIndex].ChainIndexes)
            {
                var CurChain = Chains[ChainIndex];

                if (CurChain.PeakIndexes.Contains(PeakIndex))
                {
                    TempChainList.Add(ChainIndex);
                }
            }
            return TempChainList.ToArray();
        }

        public double MaxPpmErrorGain;
        public string ClustersToString(bool FullInfo = false)
        {
            var Text = "Index," + Cluster.HeaderString();

            for (var ClusterIndex = 0; ClusterIndex < Clusters.Length; ClusterIndex++)
            {
                var CurCluster = Clusters[ClusterIndex];
                Text = Text + "\r\n" + ClusterIndex + "," + CurCluster.ToString(FullInfo);
            }
            return Text;
        }

        public double[] IdealMasses;
        public string IdealMassesToString()
        {
            var Text = "Index,Mass,IdealMass";

            for (var PeakIndex = 0; PeakIndex < Masses.Length; PeakIndex++)
            {
                Text = Text + "\r\n" + PeakIndex + "," + Masses[PeakIndex].ToString("F8") + "," + IdealMasses[PeakIndex].ToString("F8");
            }
            return Text;
        }
    };
    public static class CFileReader
    {
        private static readonly char[] WordSeparators = { '\t', ',', ' ' };
        public static void ReadFile(string Filename, out double[] Masses, out double[] Abundances, out double[] S2Ns, out double[] Resolutions, out double[] RelAbundances)
        {
            var Peak = -1;
            Masses = new double[0];
            Abundances = new double[0];
            S2Ns = new double[0];
            Resolutions = new double[0];
            RelAbundances = new double[0];
            try
            {
                var FileExtension = Path.GetExtension(Filename);
                double MaxAbundance = 0;
                /*
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
                } else */
                if (FileExtension == ".xml")
                {
                    var XmlDoc = new XmlDocument();
                    XmlDoc.Load(Filename);
                    //check Bruker instrument
                    var Nodes = XmlDoc.GetElementsByTagName("fileinfo");

                    if (Nodes.Count != 1) { throw new Exception("fileinfo"); }
                    if (Nodes[0].Attributes["appname"].Value != "Bruker Compass DataAnalysis") { throw new Exception("Bruker Compass DataAnalysis"); }
                    //read peaks
                    var MsPeakNodes = XmlDoc.GetElementsByTagName("ms_peaks");

                    if (MsPeakNodes.Count != 1) { throw new Exception("ms_peaks"); }
                    var MsPeakNode = MsPeakNodes[0];
                    var PeakCount = MsPeakNode.ChildNodes.Count;
                    Masses = new double[PeakCount];
                    Abundances = new double[PeakCount];
                    S2Ns = new double[PeakCount];
                    Resolutions = new double[PeakCount];
                    RelAbundances = new double[PeakCount];

                    for (Peak = 0; Peak < PeakCount; Peak++)
                    {
                        //<pk res="930674.5" algo="FTMS" fwhm="0.000218" a="102.53" sn="7.15" i="646225.1" mz="203.034719"/>
                        var Attributes = MsPeakNode.ChildNodes[Peak].Attributes;
                        Masses[Peak] = Double.Parse(Attributes["mz"].Value);
                        Abundances[Peak] = Double.Parse(Attributes["i"].Value);

                        if (MaxAbundance < Abundances[Peak]) { MaxAbundance = Abundances[Peak]; }
                        S2Ns[Peak] = Double.Parse(Attributes["sn"].Value);
                        Resolutions[Peak] = Double.Parse(Attributes["res"].Value);
                    }
                    XmlDoc = null;
                }
                else if (FileExtension == ".tsv")
                {
                    var Lines = File.ReadAllLines(Filename);
                    var PeakCount = Lines.Length - 1;
                    Masses = new double[PeakCount];
                    Abundances = new double[PeakCount];
                    S2Ns = new double[PeakCount];
                    Resolutions = new double[PeakCount];
                    RelAbundances = new double[PeakCount];

                    for (Peak = 0; Peak < Lines.Length - 1; Peak++)
                    {
                        var Line = Peak + 1;
                        var LineParts = Lines[Line].Split(WordSeparators);
                        Masses[Peak] = Double.Parse(LineParts[2]);
                        Abundances[Peak] = Double.Parse(LineParts[3]);

                        if (MaxAbundance < Abundances[Peak]) { MaxAbundance = Abundances[Peak]; }
                        S2Ns[Peak] = Double.Parse(LineParts[8]);
                        Resolutions[Peak] = Double.Parse(LineParts[4]);
                        RelAbundances[Peak] = Double.Parse(LineParts[9]);
                    }
                }
                else
                { //if( FileExtension == ".csv" || FileExtension == ".txt" ) { and unsupported
                    var Lines = File.ReadAllLines(Filename);
                    var PeakCount = Lines.Length - 1;
                    Masses = new double[PeakCount];
                    Abundances = new double[PeakCount];
                    S2Ns = new double[PeakCount];
                    Resolutions = new double[PeakCount];
                    RelAbundances = new double[PeakCount];
                    var ColumnCount = Lines[0].Split(WordSeparators).Length;

                    if (ColumnCount == 7)
                    {
                        for (Peak = 0; Peak < Lines.Length - 1; Peak++)
                        {
                            var Line = Peak + 1;
                            var LineParts = Lines[Line].Split(WordSeparators);
                            Masses[Peak] = Double.Parse(LineParts[2]);
                            Abundances[Peak] = Double.Parse(LineParts[3]);

                            if (MaxAbundance < Abundances[Peak]) { MaxAbundance = Abundances[Peak]; }
                            S2Ns[Peak] = Double.Parse(LineParts[5]);
                            Resolutions[Peak] = Masses[Peak] / Double.Parse(LineParts[4]);
                        }
                    }
                    else
                    {
                        for (Peak = 0; Peak < Lines.Length - 1; Peak++)
                        {
                            var Line = Peak + 1;
                            var LineParts = Lines[Line].Split(WordSeparators);
                            Masses[Peak] = Double.Parse(LineParts[0]);
                            Abundances[Peak] = Double.Parse(LineParts[1]);

                            if (MaxAbundance < Abundances[Peak]) { MaxAbundance = Abundances[Peak]; }
                            if (LineParts.Length >= 3)
                            {
                                S2Ns[Peak] = Double.Parse(LineParts[2]);
                            }
                            if (LineParts.Length >= 4)
                            {
                                Resolutions[Peak] = Double.Parse(LineParts[3]);
                            }
                        }
                    }
                }
                if (MaxAbundance <= 0) { throw new Exception("incorrect Abundances"); }
                for (Peak = 0; Peak < RelAbundances.Length; Peak++)
                {
                    RelAbundances[Peak] = Abundances[Peak] / MaxAbundance;
                }
            }
            catch (Exception ex)
            {
                if (Peak == -1)
                {
                    throw new Exception("Error in File [" + Filename + "] is " + ex.Message);
                }
                else
                {
                    throw new Exception("Error at Peak [" + Peak + "] in File [" + Filename + "] is " + ex.Message);
                }
            }
        }

        public static void ReadFile(string Filename, out InputData Data)
        {
            Data = new InputData();
            ReadFile(Filename, out Data.Masses, out Data.Abundances, out Data.S2Ns, out Data.Resolutions, out Data.RelAbundances);
            Data.Init();
        }

        public enum ECutType { MZ, Abundance, S2N, Resolution, RelAbundance };
        public class CutSettings
        {
            public ECutType CutType;
            public double Min;
            public double Max;
        };
        public static void CutData(InputData Data, out InputData OutData, CutSettings[] CutSettings)
        {
            if (Data.Masses.Length <= 0) { throw new Exception("Input data is not correct"); }
            CutSettings MZCutSettings = null;
            CutSettings AbundanceCutSettings = null;
            CutSettings S2NCutSettings = null;
            CutSettings ResolutionCutSettings = null;
            CutSettings RelAbundanceSettings = null;

            foreach (var TempCutSettings in CutSettings)
            {
                switch (TempCutSettings.CutType)
                {
                    case ECutType.MZ:
                        if (MZCutSettings == null)
                        {
                            MZCutSettings = TempCutSettings;
                        }
                        else
                        {
                            if (MZCutSettings.Min < TempCutSettings.Min) { MZCutSettings.Min = TempCutSettings.Min; }
                            if (MZCutSettings.Max < TempCutSettings.Max) { MZCutSettings.Max = TempCutSettings.Max; }
                        }
                        break;
                    case ECutType.Abundance:
                        if (AbundanceCutSettings == null)
                        {
                            AbundanceCutSettings = TempCutSettings;
                        }
                        else
                        {
                            if (AbundanceCutSettings.Min < TempCutSettings.Min) { AbundanceCutSettings.Min = TempCutSettings.Min; }
                            if (AbundanceCutSettings.Max < TempCutSettings.Max) { AbundanceCutSettings.Max = TempCutSettings.Max; }
                        }
                        break;
                    case ECutType.S2N:
                        if (S2NCutSettings == null)
                        {
                            S2NCutSettings = TempCutSettings;
                        }
                        else
                        {
                            if (S2NCutSettings.Min < TempCutSettings.Min) { S2NCutSettings.Min = TempCutSettings.Min; }
                            if (S2NCutSettings.Max < TempCutSettings.Max) { S2NCutSettings.Max = TempCutSettings.Max; }
                        }
                        break;
                    case ECutType.Resolution:
                        if (ResolutionCutSettings == null)
                        {
                            ResolutionCutSettings = TempCutSettings;
                        }
                        else
                        {
                            if (ResolutionCutSettings.Min < TempCutSettings.Min) { ResolutionCutSettings.Min = TempCutSettings.Min; }
                            if (ResolutionCutSettings.Max < TempCutSettings.Max) { ResolutionCutSettings.Max = TempCutSettings.Max; }
                        }
                        break;
                    case ECutType.RelAbundance:
                        if (RelAbundanceSettings == null)
                        {
                            RelAbundanceSettings = TempCutSettings;
                        }
                        else
                        {
                            if (RelAbundanceSettings.Min < TempCutSettings.Min) { RelAbundanceSettings.Min = TempCutSettings.Min; }
                            if (RelAbundanceSettings.Max < TempCutSettings.Max) { RelAbundanceSettings.Max = TempCutSettings.Max; }
                        }
                        break;
                }
            }
            if (Data.S2Ns[0] <= 0) { S2NCutSettings = null; }
            if (Data.Resolutions[0] <= 0) { ResolutionCutSettings = null; }
            var NewData = new bool[Data.Masses.Length];
            var NewDataCount = 0;

            for (var PeakIndex = 0; PeakIndex < Data.Masses.Length; PeakIndex++)
            {
                for (; ; )
                {
                    if (MZCutSettings != null)
                    {
                        if ((MZCutSettings.Min >= 0) && (MZCutSettings.Min > Data.Masses[PeakIndex])) break;

                        if ((MZCutSettings.Max >= 0) && (MZCutSettings.Max < Data.Masses[PeakIndex])) break;
                    }
                    if (AbundanceCutSettings != null)
                    {
                        if ((AbundanceCutSettings.Min >= 0) && (AbundanceCutSettings.Min > Data.Abundances[PeakIndex])) break;

                        if ((AbundanceCutSettings.Max >= 0) && (AbundanceCutSettings.Max < Data.Abundances[PeakIndex])) break;
                    }
                    if (S2NCutSettings != null)
                    {
                        if ((S2NCutSettings.Min >= 0) && (S2NCutSettings.Min > Data.Abundances[PeakIndex])) break;

                        if ((S2NCutSettings.Max >= 0) && (S2NCutSettings.Max < Data.Abundances[PeakIndex])) break;
                    }
                    if (ResolutionCutSettings != null)
                    {
                        if ((ResolutionCutSettings.Min >= 0) && (ResolutionCutSettings.Min > Data.Abundances[PeakIndex])) break;

                        if ((ResolutionCutSettings.Max >= 0) && (ResolutionCutSettings.Max < Data.Abundances[PeakIndex])) break;
                    }
                    if (RelAbundanceSettings != null)
                    {
                        if ((RelAbundanceSettings.Min >= 0) && (RelAbundanceSettings.Min > Data.Abundances[PeakIndex])) break;

                        if ((RelAbundanceSettings.Max >= 0) && (RelAbundanceSettings.Max < Data.Abundances[PeakIndex])) break;
                    }
                    NewData[PeakIndex] = true;
                    NewDataCount++;
                    break;
                }
            }
            OutData = new InputData();
            OutData.Masses = new double[NewDataCount];
            OutData.Abundances = new double[NewDataCount];
            OutData.S2Ns = new double[NewDataCount];
            OutData.Resolutions = new double[NewDataCount];
            OutData.RelAbundances = new double[NewDataCount];

            for (int PeakIndex = 0, RealPeakIndex = 0; PeakIndex < Data.Masses.Length; PeakIndex++)
            {
                if (NewData[PeakIndex])
                {
                    OutData.Masses[RealPeakIndex] = Data.Masses[PeakIndex];
                    OutData.Abundances[RealPeakIndex] = Data.Abundances[PeakIndex];
                    OutData.S2Ns[RealPeakIndex] = Data.S2Ns[PeakIndex];
                    OutData.Resolutions[RealPeakIndex] = Data.Resolutions[PeakIndex];
                    OutData.RelAbundances[RealPeakIndex] = Data.RelAbundances[PeakIndex];
                    RealPeakIndex++;
                }
            }
        }

        private static void CleanComObject(object o)
        {
            try
            {
                while (System.Runtime.InteropServices.Marshal.ReleaseComObject(o) > 0)
                    ;
            }
            catch { }
            finally
            {
                o = null;
            }
        }
    }
}

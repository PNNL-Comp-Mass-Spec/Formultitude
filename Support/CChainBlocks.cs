using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using MathNet.Numerics.Statistics;

namespace Support
{
    public class CChainBlocks
    {
        public string[] ElementNames = {
                "H", "C", "N", "O", "Na",
                "P", "S", "Cl", "K", "Br",
                "I"
        };
        public double[] ElementMasses = {
                CElements.H, CElements.C, 14.0030740052, CElements.O, 22.98976967,
                30.97376151, 31.97207069, 34.96885271, 38.9637069, 78.9183376,
                126.904473
        };

        public string[] Known4BlockNames = {
                "H2",
                "C",
                "CH2",
                "O"
        };
        public double[] Known4BlockMasses = {
                CElements.H *2,
                CElements.C,
                CElements.C + 2 * CElements.H,
                CElements.O,
        };
        public string[] KnownBlockNames = new string[0];
        public double[] KnownBlockMasses = new double[0];
        public int FindKnownFormulaIndex(Support.InputData Data, double Mass)
        {
            if ((KnownBlockNames.Length == 0) || (KnownBlockMasses.Length == 0)) { return -1; }
            //block mass is very low when peak mass is much higher. so block mass ppm error can't be used.
            //way: find the nearest known block mass; mass dif moves into first range median mass; find ppm error and compare - still is not good
            //next way: use middle range median mass
            var Index = CPpmError.SearchNearPeakIndex(KnownBlockMasses, Mass);

            if (Index < 0) { return -1; }
            var MassDif = Mass - KnownBlockMasses[Index];
            var Subrange = Data.ErrDisMassMedians.Length / 2;
            var NewMass = Data.ErrDisMassMedians[Subrange] + MassDif;
            var NewAbsMassPpmError = Math.Abs(CPpmError.CalculateRangePpmError(Data.ErrDisMassMedians[Subrange], NewMass));

            if (NewAbsMassPpmError < Data.ErrDisStdDevs[Subrange] * Data.MaxPpmErrorGain) { return Index; }
            return -1;
        }

        public CChainBlocks()
        {
            Array.Sort(ElementNames, ElementMasses);
        }

        private static readonly char[] WordSeparators = { '\t', ',', ' ' };
        public void KnownMassBlocksFromFile(string Filename)
        {
            //Chain block formula is first word in line
            var Lines = File.ReadAllLines(Filename);
            var FormulaNameList = new List<string>(Lines.Length);
            var FormulaMassList = new List<double>(Lines.Length);

            foreach (var Line in Lines)
            {
                var Formula = Line.Split(WordSeparators)[0];
                FormulaNameList.Add(Formula);
                try
                {
                    FormulaMassList.Add(Math.Abs(CalculateFormulaMass(Formula)));
                }
                catch { }
            }
            KnownBlockNames = FormulaNameList.ToArray();
            KnownBlockMasses = FormulaMassList.ToArray();
            Array.Sort(KnownBlockMasses, KnownBlockNames);
            //KnownMassBlocksToFile( Filename );
        }

        public void KnownMassBlocksToFile(string Filename)
        {
            var Lines = new string[KnownBlockMasses.Length];

            for (var Index = 0; Index < KnownBlockMasses.Length; Index++)
            {
                Lines[Index] = KnownBlockNames[Index] + "," + KnownBlockMasses[Index].ToString("F8");
            }
            var FileExtensionLength = Path.GetExtension(Filename).Length;
            Filename = Filename.Substring(0, Filename.Length - FileExtensionLength) + "Real.csv";
            //File.WriteAllLines( Filename, Lines );
        }

        private double CalculateFormulaMass(string Formula)
        {
            //formula example N1H_3O
            double Mass = 0;

            for (var SymbolIndex = 0; SymbolIndex < Formula.Length; SymbolIndex++)
            {
                string ElementName;
                var NegPos = 1;
                var Atoms = 1;
                //start from Capital letter
                if (!char.IsUpper(Formula[SymbolIndex])) { throw new Exception(""); }
                if (SymbolIndex + 1 >= Formula.Length)
                {
                    //last symbol
                    ElementName = Formula[SymbolIndex].ToString();
                    //NegPos = 1;
                    //Atoms = 1;
                }
                else
                {
                    ElementName = Formula[SymbolIndex].ToString();

                    if (char.IsUpper(Formula[SymbolIndex + 1]))
                    {
                        //next element
                        //NegPos = 1;
                        //Atoms = 1;
                    }
                    else
                    {
                        if (char.IsLower(Formula[SymbolIndex + 1]))
                        {
                            //Check second small letter
                            ElementName = ElementName + Formula[SymbolIndex + 1];
                            SymbolIndex = SymbolIndex + 1;
                        }
                        if (SymbolIndex + 1 >= Formula.Length)
                        {
                            //last symbol
                            //NegPos = 1;
                            //Atoms = 1;
                        }
                        else
                        {
                            //Check negative
                            if (Formula[SymbolIndex + 1] == '_')
                            {
                                NegPos = -1;
                                SymbolIndex = SymbolIndex + 1;
                            }
                            //Check atom number
                            if (SymbolIndex + 1 >= Formula.Length)
                            {
                                //Atoms = 1;
                            }
                            else if (char.IsDigit(Formula[SymbolIndex + 1]))
                            {
                                var Number = Formula[SymbolIndex + 1].ToString();
                                SymbolIndex = SymbolIndex + 1;

                                for (; SymbolIndex + 1 < Formula.Length;)
                                {
                                    if (!char.IsDigit(Formula[SymbolIndex + 1])) { break; }
                                    SymbolIndex++;
                                    Number = Number + Formula[SymbolIndex];
                                }
                                if (!int.TryParse(Number, out Atoms)) { throw new Exception(""); }
                            }
                            else
                            {
                                //Atoms = 1;
                            }
                        }
                    }
                }

                var Index = Array.BinarySearch(ElementNames, ElementName);

                if (Index < 0) { throw new Exception(""); }
                Mass = Mass + NegPos * ElementMasses[Index] * Atoms;
            }
            return Mass;
        }

        public int FindKnownChainBlockIndex(double ChainBlockMass, double PpmError)
        {
            return CPpmError.SearchPeakIndex(this.Known4BlockMasses, ChainBlockMass, PpmError);
        }
        //chain with fixed error
        public void FindChains(Support.InputData Data, int MinPeaksInChain, double PeakPpmError, double ChainPpmError, double MaxStartPeakMass, double MinChainDistance, double MaxChainDistance, bool OnlyKnowsChains)
        {
            //if MinChainDistance <= 0 - parameter is not used
            if (MinPeaksInChain < 2) { throw new Exception("MinPeaksInChain is less 2 : " + MinPeaksInChain); }
            var Masses = Data.Masses;
            var ChainPeakListList = new List<List<int>>();
            //find FirstPeakIndex by binary search
            for (var FirstPeakIndex = 0; FirstPeakIndex < Masses.Length - MinPeaksInChain; FirstPeakIndex++)
            {
                if (Masses[FirstPeakIndex] > MaxStartPeakMass) { break; }
                var MaxPossibleChainDistance = (Masses[Masses.Length - 1] - Masses[FirstPeakIndex]) / (MinPeaksInChain - 1);

                if (MinChainDistance > MaxPossibleChainDistance) { continue; }
                var SecondPeakIndex = FirstPeakIndex + 1;

                if (MinChainDistance > 0)
                {
                    var SecondPeakMass = Masses[FirstPeakIndex] + MinChainDistance;
                    SecondPeakMass = CPpmError.MassMinusPpmError(SecondPeakMass, PeakPpmError);
                    SecondPeakIndex = CPpmError.SearchNearPeakIndex(Masses, SecondPeakMass, FirstPeakIndex + 1);//???
                    //???SecondPeakIndex = CPpmError.SearchPeakIndex( Masses, SecondPeakMass, PeakPpmError, FirstPeakIndex + 1, CPpmError.EMassType.MinMass );

                    if (SecondPeakIndex == -1) { continue; }
                }
                for (; SecondPeakIndex < Masses.Length - MinPeaksInChain + 1; SecondPeakIndex++)
                {
                    var ChainDistance = Masses[SecondPeakIndex] - Masses[FirstPeakIndex];

                    if (CPpmError.MassMinusPpmError(ChainDistance, PeakPpmError) > MaxChainDistance) { break; }
                    var ChainBlockIndex = FindKnownChainBlockIndex(ChainDistance, ChainPpmError);

                    if (OnlyKnowsChains && (ChainBlockIndex < 0))
                    {
                        continue;
                    }
                    var TempPeakList = new List<int>();
                    TempPeakList.Add(FirstPeakIndex);
                    TempPeakList.Add(SecondPeakIndex);

                    var NextPeakMass = Masses[SecondPeakIndex];
                    //int NextPeakIndex = SecondPeakIndex + 1;

                    for (var NextPeakIndex = SecondPeakIndex + 1; NextPeakIndex < Masses.Length; NextPeakIndex++)
                    {
                        NextPeakMass = NextPeakMass + ChainDistance;
                        var Index = CPpmError.SearchPeakIndex(Masses, NextPeakMass, PeakPpmError, NextPeakIndex);

                        if (Index < 0) { break; }
                        TempPeakList.Add(Index);
                        NextPeakMass = Masses[Index];
                        //NextPeakIndex = Index + 1;
                    }
                    if (TempPeakList.Count >= MinPeaksInChain)
                    {
                        ChainPeakListList.Add(TempPeakList);
                    }
                }
            }
            var ChainPeakList = ChainPeakListList.ToArray();
            var ChainMasses = new double[ChainPeakList.Length];//for sort
            var Chains = new Chain[ChainPeakList.Length];

            for (var ChainIndex = 0; ChainIndex < Chains.Length; ChainIndex++)
            {
                var NewChain = new Chain();
                NewChain.PeakIndexes = ChainPeakList[ChainIndex].ToArray();
                NewChain.PeakMasses = new double[NewChain.PeakIndexes.Length];

                for (var ChainPeakIndex = 0; ChainPeakIndex < NewChain.PeakMasses.Length; ChainPeakIndex++)
                {
                    NewChain.PeakMasses[ChainPeakIndex] = Masses[NewChain.PeakIndexes[ChainPeakIndex]];
                }
                var BlockMasses = new double[NewChain.PeakIndexes.Length - 1];

                for (var BlockIndex = 0; BlockIndex < BlockMasses.Length; BlockIndex++)
                {
                    BlockMasses[BlockIndex] = NewChain.PeakMasses[BlockIndex + 1] - NewChain.PeakMasses[BlockIndex];
                }
                NewChain.BlockMassMean = CArrayMath.Mean(BlockMasses);
                ChainMasses[ChainIndex] = NewChain.BlockMassMean;//for sort
                NewChain.PpmErrorStdDev = CArrayMath.StandardDeviation(BlockMasses, NewChain.BlockMassMean);
                var ChainBlockIndex = FindKnownChainBlockIndex(NewChain.PeakMasses[1] - NewChain.PeakMasses[0], ChainPpmError);

                if (ChainBlockIndex >= 0)
                {
                    NewChain.IdealBlockMass = Known4BlockMasses[ChainBlockIndex];
                    NewChain.Formula = Known4BlockNames[ChainBlockIndex];
                }
                else
                {
                    NewChain.IdealBlockMass = 0;
                    //NewChain.Formula = "N/A";
                }
                Chains[ChainIndex] = NewChain;
            }
            Data.Chains = Chains;
            CreateUniqueChains(Data, ChainPpmError);
        }

        public int AreChainsDuplicated(Support.InputData Data, Chain FirstChain, Chain SecondChain, double PeakPpmError)
        {
            var FirstTheSamePeakIndex = false;
            var AreBlockSizesTheSame = false;

            for (var FirstIndex = 0; FirstIndex < FirstChain.PeakIndexes.Length; FirstIndex++)
            {
                for (var SecondIndex = 0; SecondIndex < SecondChain.PeakIndexes.Length; SecondIndex++)
                {
                    if (FirstChain.PeakIndexes[FirstIndex] != SecondChain.PeakIndexes[SecondIndex]) { continue; }
                    if (!FirstTheSamePeakIndex)
                    {
                        FirstTheSamePeakIndex = true;

                        if ((FirstChain.Formula.Length != 0) && (SecondChain.Formula.Length != 0) && (FirstChain.Formula != "N/A") && (SecondChain.Formula != "N/A"))
                        {
                            if (FirstChain.Formula == SecondChain.Formula)
                            {
                                AreBlockSizesTheSame = true;
                            }
                        }
                        else
                        {
                            var BlockMassDif = FirstChain.BlockMassMean - SecondChain.BlockMassMean;
                            var MassError = CPpmError.PpmToError(Data.Masses[FirstChain.PeakIndexes[FirstIndex]], PeakPpmError);

                            if (Math.Abs(BlockMassDif) <= MassError)
                            {
                                AreBlockSizesTheSame = true;
                            }
                        }
                        continue;
                    }
                    if (AreBlockSizesTheSame)
                    {
                        if (FirstChain.PeakIndexes.Length >= SecondChain.PeakIndexes.Length)
                        {
                            return 1;
                        }
                        else
                        {
                            return -1;
                        }
                    }
                    else
                    {
                        if (FirstChain.BlockMassMean <= SecondChain.BlockMassMean)
                        {
                            return 1;
                        }
                        else
                        {
                            return -1;
                        }
                    }
                }
            }
            return 0;
        }

        public void CreateUniqueChains(Support.InputData Data, double PeakPpmError)
        {
            var Chains = Data.Chains;
            var NotUniqueChains = new bool[Chains.Length];
            //search duplicate chain in each peak
            for (var FirstChainIndex = 0; FirstChainIndex < Chains.Length - 1; FirstChainIndex++)
            {
                if (NotUniqueChains[FirstChainIndex]) { continue; }
                for (var SecondChainIndex = FirstChainIndex + 1; SecondChainIndex < Chains.Length; SecondChainIndex++)
                {
                    if (NotUniqueChains[SecondChainIndex]) { continue; }
                    if (AreChainsDuplicated(Data, Chains[FirstChainIndex], Chains[SecondChainIndex], PeakPpmError) > 0)
                    {
                        NotUniqueChains[SecondChainIndex] = true;
                    }
                    else if (AreChainsDuplicated(Data, Chains[FirstChainIndex], Chains[SecondChainIndex], PeakPpmError) < 0)
                    {
                        NotUniqueChains[FirstChainIndex] = true;
                        //break;???
                    }
                }
            }
            var UniqueChains = new List<Support.Chain>();

            for (var Index = 0; Index < NotUniqueChains.Length; Index++)
            {
                if (!NotUniqueChains[Index]) { UniqueChains.Add(Chains[Index]); }
            }
            Data.Chains = UniqueChains.ToArray();
        }

        //StdDev error
        private const int UniquePeaksInRange = 200;
        //return Tuple where
        //  Item1 is array of parent peak indexes
        //  Item2 is array of child peak indexes
        //  Item3 is array of PpmErrors
        public Tuple<int[], int[], double[]> FindPeaksByBlockMass(double[] Masses, double BlockMass, double MaxPpmError, double MaxMass = 500)
        {
            var ParentPeakIndexList = new List<int>();
            var ChildPeakIndexList = new List<int>();
            var PpmErrorList = new List<double>();

            for (var PeakIndex = 0; PeakIndex < Masses.Length - 1; PeakIndex++)
            {
                if (Masses[PeakIndex] > MaxMass) { break; }
                var SearchPeakIndex = CPpmError.SearchPeakIndex(Masses, Masses[PeakIndex] + BlockMass, MaxPpmError, PeakIndex + 1);

                if (SearchPeakIndex < 0) { continue; }
                ParentPeakIndexList.Add(PeakIndex);
                ChildPeakIndexList.Add(SearchPeakIndex);
                PpmErrorList.Add(CPpmError.CalculateRangePpmError(Masses[PeakIndex], Masses[SearchPeakIndex] - BlockMass));
            }
            return new Tuple<int[], int[], double[]>(ParentPeakIndexList.ToArray(), ChildPeakIndexList.ToArray(), PpmErrorList.ToArray());
        }

        //return Tuple where
        //  Item1 is low mass
        //  Item2 is upper mass
        //  Item3 is block quantity
        //  Item4 is block mass average( median)
        //  Item4 is error mean
        //  Item5 is error StdDev
        public Tuple<double, double, int, double, double, double> CalculateBlockPpmErrorParameters(double[] Masses, double StartPpmError, double BlockMass, double MaxMass = 500)
        {
            var CurData = FindPeaksByBlockMass(Masses, BlockMass, StartPpmError);
            var ParentPeaks = CurData.Item1;
            var ChildPeaks = CurData.Item2;
            var PpmErrors = CurData.Item3;
            //remove outliers
            var Five = Statistics.FiveNumberSummary(PpmErrors);
            var LowPpmError = Five[1] - 1.5 * (Five[3] - Five[1]);
            var UpperPpmError = Five[3] + 1.5 * (Five[3] - Five[1]);
            var RealErrorList = new List<double>();
            var RealBlockMasses = new List<double>();

            for (var Index = 0; Index < PpmErrors.Length; Index++)
            {
                if ((PpmErrors[Index] >= LowPpmError) & (PpmErrors[Index] <= UpperPpmError))
                {
                    RealErrorList.Add(PpmErrors[Index]);
                    RealBlockMasses.Add(Masses[ChildPeaks[Index]] - Masses[ParentPeaks[Index]]);
                }
            }
            return new Tuple<double, double, int, double, double, double>(
                    Masses[ParentPeaks.First()],
                    Masses[ParentPeaks.Last()],
                    RealErrorList.Count,
                    Statistics.Median(RealBlockMasses),
                    Statistics.Mean(RealErrorList),
                    Statistics.StandardDeviation(RealErrorList));
        }

        public void CalculateErrorDistribution(Support.InputData Data, double StartPpmError, double[] BlockMasses)
        {
            var GlobalIndexList = new List<int>();
            var ParentPeakIndexList = new List<int>();
            var ChildPeakIndexList = new List<int>();
            var PpmErrorList = new List<double>();
            var BlockMassList = new List<double>();

            for (var BlockMassIndex = 0; BlockMassIndex < BlockMasses.Length; BlockMassIndex++)
            {
                var BlockMass = BlockMasses[BlockMassIndex];
                var CurData = FindPeaksByBlockMass(Data.Masses, BlockMass, StartPpmError, Data.Masses.Last() + 1);
                var Indexes = new int[CurData.Item1.Length];

                for (var Index = 0; Index < Indexes.Length; Index++)
                {
                    Indexes[Index] = CurData.Item1[Index] * BlockMasses.Length + BlockMassIndex;
                }
                GlobalIndexList.AddRange(Indexes);
                ParentPeakIndexList.AddRange(CurData.Item1);
                ChildPeakIndexList.AddRange(CurData.Item2);
                PpmErrorList.AddRange(CurData.Item3);
                BlockMassList.AddRange(Enumerable.Repeat(BlockMass, CurData.Item3.Length));
            }
            //sort all data by mass
            var GlobalIndexes = GlobalIndexList.ToArray();
            Array.Sort(GlobalIndexes);

            var ParentPeakIndexes = ParentPeakIndexList.ToArray();
            Array.Sort(GlobalIndexList.ToArray(), ParentPeakIndexes);
            var ChildPeakIndexes = ChildPeakIndexList.ToArray();
            Array.Sort(GlobalIndexList.ToArray(), ChildPeakIndexes);
            var PpmErrors = PpmErrorList.ToArray();
            Array.Sort(GlobalIndexList.ToArray(), PpmErrors);
            var PeakBlockMasses = BlockMassList.ToArray();
            Array.Sort(GlobalIndexList.ToArray(), PeakBlockMasses);

            //find error vs mass
            var PeaksInRange = (int)(UniquePeaksInRange * (1 + BlockMasses.Length / 1.5));
            var RangeCount = (int)Math.Floor(1.0 * ParentPeakIndexList.Count / PeaksInRange);

            var MassMedians = new double[RangeCount];
            var ErrorMeans = new double[RangeCount];
            var ErrorStdDevs = new double[RangeCount];
            var Pairs = new int[RangeCount];
            var ErrorRanges = new double[RangeCount];
            Data.ErrDisParentPeakIndexes = new int[RangeCount][];
            Data.ErrDisChildPeakIndexes = new int[RangeCount][];
            Data.ErrDisPpmErrors = new double[RangeCount][];
            Data.ErrDisBlockMasses = new double[RangeCount][];
            Data.ErrDisLowPpmErrors = new double[RangeCount];
            Data.ErrDisUpperPpmErrors = new double[RangeCount];

            for (var RangeIndex = 0; RangeIndex < RangeCount; RangeIndex++)
            {
                //create subarray
                var LowerIndex = (int)Math.Round(1.0 * RangeIndex * ParentPeakIndexes.Length / RangeCount);
                var LowerMass = Data.Masses[ParentPeakIndexes[LowerIndex]];
                var UpperIndex = (int)Math.Round(1.0 * (RangeIndex + 1) * ParentPeakIndexes.Length / RangeCount) - 1;
                var UpperMass = Data.Masses[ParentPeakIndexes[UpperIndex]];
                MassMedians[RangeIndex] = LowerMass + (UpperMass - LowerMass) / 2;

                var RangePeakCount = UpperIndex - LowerIndex + 1;
                var MedianSubsetPpmErrors = new double[RangePeakCount];
                Data.ErrDisParentPeakIndexes[RangeIndex] = new int[RangePeakCount];
                Data.ErrDisChildPeakIndexes[RangeIndex] = new int[RangePeakCount];
                Data.ErrDisPpmErrors[RangeIndex] = new double[RangePeakCount];
                Data.ErrDisBlockMasses[RangeIndex] = new double[RangePeakCount];

                for (var PeakIndex = 0; PeakIndex < RangePeakCount; PeakIndex++)
                {
                    MedianSubsetPpmErrors[PeakIndex] = PpmErrors[LowerIndex + PeakIndex];
                    Data.ErrDisParentPeakIndexes[RangeIndex][PeakIndex] = ParentPeakIndexes[LowerIndex + PeakIndex];
                    Data.ErrDisChildPeakIndexes[RangeIndex][PeakIndex] = ChildPeakIndexes[LowerIndex + PeakIndex];
                    Data.ErrDisPpmErrors[RangeIndex][PeakIndex] = PpmErrors[LowerIndex + PeakIndex];
                    Data.ErrDisBlockMasses[RangeIndex][PeakIndex] = PeakBlockMasses[LowerIndex + PeakIndex];
                }
                //remove outliers
                var Five = Statistics.FiveNumberSummary(MedianSubsetPpmErrors);
                var LowPpmError = Five[1] - 1.5 * (Five[3] - Five[1]);
                var UpperPpmError = Five[3] + 1.5 * (Five[3] - Five[1]);
                Data.ErrDisLowPpmErrors[RangeIndex] = LowPpmError;
                Data.ErrDisUpperPpmErrors[RangeIndex] = UpperPpmError;

                if ((LowPpmError < -StartPpmError) || (UpperPpmError > StartPpmError))
                {
                    LowPpmError = LowPpmError;
                }
                var RealErrorList = new List<double>();
                var RealBlockList = new List<double>();

                for (var Index = 0; Index < RangePeakCount; Index++)
                {
                    if ((PpmErrors[LowerIndex + Index] >= LowPpmError) & (PpmErrors[LowerIndex + Index] <= UpperPpmError))
                    {
                        RealErrorList.Add(PpmErrors[LowerIndex + Index]);
                        RealBlockList.Add(PeakBlockMasses[LowerIndex + Index]);
                    }
                }
                //calculate Mean and StdDev
                ErrorMeans[RangeIndex] = Statistics.Mean(RealErrorList);
                ErrorStdDevs[RangeIndex] = Statistics.StandardDeviation(RealErrorList);
                Pairs[RangeIndex] = RealErrorList.Count;
            }
            Data.ErrDisMassMedians = MassMedians;
            Data.ErrDisMeans = ErrorMeans;
            Data.ErrDisStdDevs = ErrorStdDevs;
            Data.ErrDisPairCount = Pairs;
        }
        //isotopic peaks
        private const double IsotopicPeakGain = 5;
        private const double Charge1Distance = CElements.C13 - CElements.C;
        public void FindIsotopicPeaks(Support.InputData Data)
        {
            //clean
            Data.IsotopicParentPeaks = Enumerable.Repeat<int>(-1, Data.Masses.Length).ToArray();
            Data.IsotopicChildPeaks = Enumerable.Repeat<int>(-1, Data.Masses.Length).ToArray();
            Data.IsotopicPeaks = -1;
            //calculation
            var IsotopicPeaks = 0;

            for (var ParentPeakIndex = 0; ParentPeakIndex < Data.Masses.Length - 1; ParentPeakIndex++)
            {
                var IsotopicPeakIndex = CPpmError.SearchPeakIndexBasedOnErrorDistribution(Data, Data.Masses[ParentPeakIndex] + Charge1Distance, ParentPeakIndex + 1);

                if (IsotopicPeakIndex < 0) { continue; }
                if (Data.Abundances[ParentPeakIndex] / Data.Abundances[IsotopicPeakIndex] > IsotopicPeakGain)
                {
                    Data.IsotopicChildPeaks[ParentPeakIndex] = IsotopicPeakIndex;
                    Data.IsotopicParentPeaks[IsotopicPeakIndex] = ParentPeakIndex;
                    IsotopicPeaks++;
                }
            }
            Data.IsotopicPeaks = IsotopicPeaks;
        }
        //chain with StdDev error
        public Chain FindChainBasedOnPeakAndMassDistance(Support.InputData Data, int StartPeakIndex, double BlockMass, int MinPeaksInChain)
        {
            var PeakIndexList = new List<int>();
            PeakIndexList.Add(StartPeakIndex);

            for (var NextPeakIndex = StartPeakIndex; NextPeakIndex < Data.Masses.Length - 1;)
            {
                var SearchPeakIndex = CPpmError.SearchPeakIndexBasedOnErrorDistribution(Data, Data.Masses[NextPeakIndex] + BlockMass, NextPeakIndex + 1);

                if (SearchPeakIndex < 0) { break; }
                PeakIndexList.Add(SearchPeakIndex);
                NextPeakIndex = SearchPeakIndex;
            }
            if (PeakIndexList.Count < MinPeaksInChain) { return null; }
            var CurChain = new Chain();
            CurChain.IdealBlockMass = BlockMass;
            CurChain.Formula = "";//can't use "N/A" because AreChainsDuplicated use Formula for comparision
            CurChain.PeakIndexes = PeakIndexList.ToArray();
            var PeakMasses = new double[CurChain.PeakIndexes.Length];
            var BlockMasses = new double[CurChain.PeakIndexes.Length - 1];
            var PpmErrors = new double[CurChain.PeakIndexes.Length - 1];

            for (var PeakIndex = 0; PeakIndex < CurChain.PeakIndexes.Length; PeakIndex++)
            {
                PeakMasses[PeakIndex] = Data.Masses[CurChain.PeakIndexes[PeakIndex]];

                if (PeakIndex > 0)
                {
                    BlockMasses[PeakIndex - 1] = PeakMasses[PeakIndex] - PeakMasses[PeakIndex - 1];
                    PpmErrors[PeakIndex - 1] = CPpmError.CalculateAbsRangePpmError(PeakMasses[PeakIndex - 1] + BlockMass, PeakMasses[PeakIndex]);
                }
                if (Data.ParentPeakIndexes[CurChain.PeakIndexes[PeakIndex]] == -1)
                {
                    if (Data.ParentPeakIndexes[StartPeakIndex] == -1)
                    {
                        Data.ParentPeakIndexes[CurChain.PeakIndexes[PeakIndex]] = -2;
                    }
                    else
                    {
                        Data.ParentPeakIndexes[CurChain.PeakIndexes[PeakIndex]] = CurChain.PeakIndexes[PeakIndex - 1];
                    }
                }
            }
            CurChain.PeakMasses = PeakMasses;
            CurChain.BlockMassMean = Statistics.Mean(BlockMasses);
            CurChain.PpmErrorStdDev = Statistics.StandardDeviation(PpmErrors);
            return CurChain;
        }

        public void FindChains1(Support.InputData Data, int MinPeaksInChain, double MaxStartPeakMass, double[] BlockMasses)
        {
            if (MinPeaksInChain < 2) { throw new Exception("MinPeaksInChain is less 2 : " + MinPeaksInChain); }
            var Masses = Data.Masses;
            var ChainList = new List<Chain>();

            for (var PeakIndex = 0; PeakIndex < Masses.Length - MinPeaksInChain; PeakIndex++)
            {
                if (MaxStartPeakMass < Masses[PeakIndex]) { break; }
                foreach (var BlockMass in BlockMasses)
                {
                    var TempChain = FindChainBasedOnPeakAndMassDistance(Data, PeakIndex, BlockMass, MinPeaksInChain);

                    if (TempChain != null)
                    {
                        ChainList.Add(TempChain);
                    }
                }
            }
            Data.Chains = ChainList.ToArray();

            CreateUniqueChains1(Data);
        }

        public int AreChainsTheSame(Chain FirstChain, Chain SecondChain)
        {
            //return 1 - chains are the same; first chain is better
            //return 0 - chains are not the same
            //return -1 - chains are the same; second chain is better
            var TheSameIndexCount = 0;

            for (var FirstIndex = 0; FirstIndex < FirstChain.PeakIndexes.Length; FirstIndex++)
            {
                for (var SecondIndex = 0; SecondIndex < SecondChain.PeakIndexes.Length; SecondIndex++)
                {
                    if (FirstChain.PeakIndexes[FirstIndex] == SecondChain.PeakIndexes[SecondIndex])
                    {
                        TheSameIndexCount++;

                        if (TheSameIndexCount >= 2)
                        {
                            break;
                        }
                    }
                }
                if (TheSameIndexCount >= 2)
                {
                    break;
                }
            }
            const double MaxGainOfTheSameChains = 1.01;
            double BlockMassGain;

            if ((FirstChain.IdealBlockMass > 0) && (SecondChain.IdealBlockMass > 0))
            {
                if (FirstChain.IdealBlockMass > SecondChain.IdealBlockMass)
                {
                    BlockMassGain = FirstChain.IdealBlockMass / SecondChain.IdealBlockMass;
                }
                else
                {
                    BlockMassGain = SecondChain.IdealBlockMass / FirstChain.IdealBlockMass;
                }
            }
            else
            {
                if (FirstChain.BlockMassMean > SecondChain.BlockMassMean)
                {
                    BlockMassGain = FirstChain.BlockMassMean / SecondChain.BlockMassMean;
                }
                else
                {
                    BlockMassGain = SecondChain.BlockMassMean / FirstChain.BlockMassMean;
                }
            }
            if (TheSameIndexCount < 2)
            {
                if (TheSameIndexCount == 1)
                {
                    if (BlockMassGain < MaxGainOfTheSameChains)
                    {
                        BlockMassGain = BlockMassGain;
                    }
                }
                return 0;
            }
            if (BlockMassGain < MaxGainOfTheSameChains)
            {
                //the same BlockMass => must be the same chain but first peaks are different
                if (FirstChain.PeakIndexes.Last() != SecondChain.PeakIndexes.Last())
                {
                    //shouldn't be
                    TheSameIndexCount = TheSameIndexCount;
                }
                if (FirstChain.PeakIndexes.Length >= SecondChain.PeakIndexes.Length)
                {
                    return 1;
                }
                else
                {
                    return -1;
                }
            }
            else
            {
                //must be part of chain
                if (FirstChain.BlockMassMean <= SecondChain.BlockMassMean)
                {
                    return 1;
                }
                else
                {
                    return -1;
                }
            }
        }

        public void CreateUniqueChains1(Support.InputData Data)
        {
            var Chains = Data.Chains;
            var NotUniqueChains = new bool[Chains.Length];
            //search duplicate chain in each peak
            for (var ChainIndex = 0; ChainIndex < Chains.Length - 1; ChainIndex++)
            {
                if (NotUniqueChains[ChainIndex]) { continue; }
                for (var NextChainIndex = ChainIndex + 1; NextChainIndex < Chains.Length; NextChainIndex++)
                {
                    if (NotUniqueChains[NextChainIndex]) { continue; }
                    var Result = AreChainsTheSame(Chains[ChainIndex], Chains[NextChainIndex]);

                    if (Result > 0)
                    {
                        NotUniqueChains[NextChainIndex] = true;
                    }
                    else if (Result < 0)
                    {
                        NotUniqueChains[ChainIndex] = true;
                    }
                }
            }
            var UniqueChains = new List<Support.Chain>();

            for (var Index = 0; Index < NotUniqueChains.Length; Index++)
            {
                if (!NotUniqueChains[Index]) { UniqueChains.Add(Chains[Index]); }
            }
            Data.Chains = UniqueChains.ToArray();
            var FirstPeakIndexes = new int[Data.Chains.Length];

            for (var ChainIndex = 0; ChainIndex < Data.Chains.Length; ChainIndex++)
            {
                FirstPeakIndexes[ChainIndex] = Data.Chains[ChainIndex].PeakIndexes[0];
            }
            Array.Sort(FirstPeakIndexes, Data.Chains);
        }

        //cluster
        public void FindClusters(Support.InputData Data)
        {
            //find clusters based on chains
            var IsChainInCluster = new bool[Data.Chains.Length];
            var ClusterList = new List<Cluster>();
            var ClusterPeakCountList = new List<int>();//purpose - stort based on amount of peaks
            var TotalPeakCount = 0;

            for (var ChainIndex = 0; ChainIndex < Data.Chains.Length; ChainIndex++)
            {
                if (IsChainInCluster[ChainIndex]) { continue; }
                IsChainInCluster[ChainIndex] = true;

                var ClusterChains = new List<int>();
                ClusterChains.Add(ChainIndex);
                Data.Chains[ChainIndex].ClusteringChainIndex = -1;
                Data.Chains[ChainIndex].ClusteringPeakIndex = -1;
                var ClusterPeakSortedList = new SortedList<int, int>();
                var ClusterEvenPeakSortedList = new SortedList<int, int>();

                foreach (var PeakIndex in Data.Chains[ChainIndex].PeakIndexes)
                {
                    ClusterPeakSortedList.Add(PeakIndex, ChainIndex);

                    if (((int)Math.Round(Data.Masses[PeakIndex])) % 2 == 0)
                    {
                        ClusterEvenPeakSortedList.Add(PeakIndex, ChainIndex);
                    }
                }
                TotalPeakCount = TotalPeakCount + Data.Chains[ChainIndex].PeakIndexes.Length;

                for (; ; )
                {
                    var WasAddedChain = false;

                    for (var NextChainIndex = ChainIndex + 1; NextChainIndex < Data.Chains.Length; NextChainIndex++)
                    {
                        if (IsChainInCluster[NextChainIndex]) { continue; }
                        foreach (var PeakIndex in Data.Chains[NextChainIndex].PeakIndexes)
                        {
                            int ParentChainIndex;

                            if (ClusterPeakSortedList.TryGetValue(PeakIndex, out ParentChainIndex))
                            {
                                IsChainInCluster[NextChainIndex] = true;
                                ClusterChains.Add(NextChainIndex);
                                Data.Chains[NextChainIndex].ClusteringChainIndex = ParentChainIndex;
                                Data.Chains[NextChainIndex].ClusteringPeakIndex = PeakIndex;

                                foreach (var CurPeakIndex in Data.Chains[NextChainIndex].PeakIndexes)
                                {
                                    if (!ClusterPeakSortedList.ContainsKey(CurPeakIndex))
                                    {
                                        ClusterPeakSortedList.Add(CurPeakIndex, NextChainIndex);

                                        if (((int)Math.Round(Data.Masses[CurPeakIndex])) % 2 == 0)
                                        {
                                            ClusterEvenPeakSortedList.Add(CurPeakIndex, NextChainIndex);
                                        }
                                    }
                                }
                                WasAddedChain = true;
                                break;
                            }
                        }
                    }
                    if (!WasAddedChain)
                    {
                        break;
                    }
                }
                ClusterPeakCountList.Add(ClusterPeakSortedList.Count);
                var CurCluster = new Cluster(ClusterChains.ToArray());
                Array.Sort(CurCluster.ChainIndexes);
                //add cluster statistics
                var ClusterChainIndexes = CurCluster.ChainIndexes;
                var TheBestClusterChainIndex = -1;
                var TheBestChainPeakIndex = -1;
                var TheBestScore = double.NaN;
                var MinMass = double.NaN;
                var MaxMass = double.NaN;

                for (var ClusterChainIndex = 0; ClusterChainIndex < ClusterChainIndexes.Length; ClusterChainIndex++)
                {
                    var CurChainIndex = ClusterChainIndexes[ClusterChainIndex];
                    var CurChain = Data.Chains[CurChainIndex];

                    for (var ChainPeakIndex = 0; ChainPeakIndex < CurChain.PeakIndexes.Length; ChainPeakIndex++)
                    {
                        var CurPeakIndex = CurChain.PeakIndexes[ChainPeakIndex];
                        var CurPeakScore = GetPeakScore(Data, CurPeakIndex);

                        if ((ClusterChainIndex == 0) && (ChainPeakIndex == 0))
                        {
                            TheBestClusterChainIndex = ClusterChainIndex;
                            TheBestChainPeakIndex = ChainPeakIndex;
                            TheBestScore = CurPeakScore;
                        }
                        else if (TheBestScore < CurPeakScore)
                        {
                            TheBestClusterChainIndex = ClusterChainIndex;
                            TheBestChainPeakIndex = ChainPeakIndex;
                            TheBestScore = CurPeakScore;
                        }
                    }
                    if (ClusterChainIndex == 0)
                    {
                        MinMass = Data.Masses[CurChain.PeakIndexes.First()];
                        MaxMass = Data.Masses[CurChain.PeakIndexes.Last()];
                    }
                    else
                    {
                        if (MinMass > Data.Masses[CurChain.PeakIndexes.First()]) { MinMass = Data.Masses[CurChain.PeakIndexes.First()]; }
                        if (MaxMass < Data.Masses[CurChain.PeakIndexes.Last()]) { MaxMass = Data.Masses[CurChain.PeakIndexes.Last()]; }
                    }
                }
                CurCluster.MinMass = MinMass;
                CurCluster.MaxMass = MaxMass;
                CurCluster.TheBestChainIndex = TheBestClusterChainIndex;
                CurCluster.TheBestPeakIndex = TheBestChainPeakIndex;
                CurCluster.TheBestScore = TheBestScore;
                CurCluster.PeakCount = ClusterPeakSortedList.Count;
                CurCluster.EvenPeakCount = ClusterEvenPeakSortedList.Count;
                CurCluster.OddPeakCount = ClusterPeakSortedList.Count - ClusterEvenPeakSortedList.Count;
                ClusterList.Add(CurCluster);
            }

            var Clusters = ClusterList.ToArray();
            //sort by high peak amount
            Array.Sort(ClusterPeakCountList.ToArray(), Clusters);
            Array.Reverse(Clusters);
            Data.Clusters = Clusters;
        }

        public double[] GetNoTrendErrorMasses(double[] Masses, double[] IdealMasses, Support.InputData Data)
        {
            var MinMassInteger = (int)Math.Round(Masses.First());
            var MassIntegerCount = (int)Math.Round(Masses.Last()) + 1;
            var MeanAbsErrorAtInteger = Enumerable.Repeat(double.NaN, MassIntegerCount).ToArray();

            for (var PeakIndex = 0; PeakIndex < Masses.Length;)
            {
                if (IdealMasses[PeakIndex] < 0.1) { PeakIndex++; continue; }
                //List<int> PeakIndexList = new List<int>();
                var AbsErrorList = new List<double>();
                var CurInteger = (int)Math.Round(IdealMasses[PeakIndex]);

                for (; PeakIndex < Masses.Length; PeakIndex++)
                {//to fill Lists specially not "NextPeakIndex = PeakIndex + 1"
                    if (IdealMasses[PeakIndex] < 0.1) { continue; }
                    if ((int)Math.Round(IdealMasses[PeakIndex]) != CurInteger)
                    {
                        break;
                    }
                    //PeakIndexList.Add( PeakIndex );
                    //PpmErrorList.Add( CPpmError.SignedMassErrorPPM( Masses[ PeakIndex], IdealMasses[ PeakIndex]) );
                    AbsErrorList.Add(IdealMasses[PeakIndex] - Masses[PeakIndex]);
                }
                MeanAbsErrorAtInteger[CurInteger] = FindMeanWithoutOutlilers(AbsErrorList.ToArray());
            }
            Data.ErrorTrend = MeanAbsErrorAtInteger.ToArray();
            //smoothing by filter ??? MathNet.Numerics.Fit.

            var MinAssignedMassInteger = -1;

            for (var IntegerIndex = MinMassInteger; IntegerIndex < MassIntegerCount; IntegerIndex++)
            {
                if (!double.IsNaN(MeanAbsErrorAtInteger[IntegerIndex]))
                {
                    MinAssignedMassInteger = IntegerIndex;
                    break;
                }
            }
            var MaxAssignedMassInteger = -1;

            for (var IntegerIndex = MassIntegerCount - 1; IntegerIndex >= MinMassInteger; IntegerIndex--)
            {
                if (!double.IsNaN(MeanAbsErrorAtInteger[IntegerIndex]))
                {
                    MaxAssignedMassInteger = IntegerIndex;
                    break;
                }
            }
            var NoTrendMasses = new double[Masses.Length];

            for (var PeakIndex = 0; PeakIndex < Masses.Length; PeakIndex++)
            {
                var CurInteger = (int)Math.Round(Masses[PeakIndex]);
                double AbsError;

                if (CurInteger < MinAssignedMassInteger)
                {
                    AbsError = MeanAbsErrorAtInteger[MinAssignedMassInteger];
                }
                else if (CurInteger > MaxAssignedMassInteger)
                {
                    AbsError = MeanAbsErrorAtInteger[MaxAssignedMassInteger];
                }
                else
                {
                    if (!double.IsNaN(MeanAbsErrorAtInteger[CurInteger]))
                    {
                        AbsError = MeanAbsErrorAtInteger[CurInteger];
                    }
                    else
                    {
                        var LowIntegerIndex = -1;

                        for (var IntegerIndex = CurInteger - 1; ; IntegerIndex--)
                        {
                            if (!double.IsNaN(MeanAbsErrorAtInteger[IntegerIndex]))
                            {
                                LowIntegerIndex = IntegerIndex;
                                break;
                            }
                        }
                        var UpperIntegerIndex = -1;

                        for (var IntegerIndex = CurInteger + 1; ; IntegerIndex++)
                        {
                            if (!double.IsNaN(MeanAbsErrorAtInteger[IntegerIndex]))
                            {
                                UpperIntegerIndex = IntegerIndex;
                                break;
                            }
                        }
                        AbsError = CArrayMath.LinearValue(CurInteger, LowIntegerIndex, UpperIntegerIndex, MeanAbsErrorAtInteger[LowIntegerIndex], MeanAbsErrorAtInteger[UpperIntegerIndex]);
                    }
                }
                NoTrendMasses[PeakIndex] = Masses[PeakIndex] + AbsError;
            }
            return NoTrendMasses;
        }

        public double FindMeanWithoutOutlilers(double[] Values)
        {
            //int [] Indexes = new int [ Values.Length ];
            //for ( int Index = 0; Index < Values.Length; Index++ ) { Indexes [ Index ] = Index; }
            var FiveNumbers = Statistics.FiveNumberSummary(Values);
            var MinValue = FiveNumbers[1] - 1.5 * (FiveNumbers[3] - FiveNumbers[1]);
            var MaxValue = FiveNumbers[3] + 1.5 * (FiveNumbers[3] - FiveNumbers[1]);
            var CorrectDataset = new List<double>();

            foreach (var Value in Values)
            {
                if ((Value >= MinValue) && (Value <= MaxValue))
                {
                    CorrectDataset.Add(Value);
                }
            }
            return Statistics.Mean(CorrectDataset);
        }

        //assign ideal masses
        /*
        public void AssignIdealMassesInClusterOld( Support.InputData Data, int ClusterIndex ) {
            if( ( Data.Clusters == null) || (Data.Clusters.Length <= ClusterIndex) ){
                throw new Exception( "Cluster index " + ClusterIndex + " is out ot range");
            }
            int [] ClusterChainIndexes = Data.Clusters[ ClusterIndex].ChainIndexes;
            bool [] UsedChains = new bool [ ClusterChainIndexes.Length];
            List<double> MassErrors = new List<double>();
            UsedChains[ 0] = true;
            int UsedChainCount = ClusterChainIndexes.Length - 1;
            int TestDoublePeakCount = 0;

            for( ;;){
                for ( int ChainIndexInCluster = 0; ChainIndexInCluster < ClusterChainIndexes.Length; ChainIndexInCluster++ ) {
                    int [] CurChainPeakIndexes = Data.Chains [ ClusterChainIndexes [ ChainIndexInCluster ] ].PeakIndexes;
                    int SearchPeakIndexInChain = 0;
                    double SearchMass = Data.Masses [ CurChainPeakIndexes [ SearchPeakIndexInChain ] ];

                    if ( ChainIndexInCluster != 0 ) {
                        if ( UsedChains [ ChainIndexInCluster ] == true ) { continue; }
                        for ( ; SearchPeakIndexInChain < CurChainPeakIndexes.Length; SearchPeakIndexInChain++ ){
                            if ( Data.IdealMasses[ CurChainPeakIndexes[ SearchPeakIndexInChain] ] > 0.1){
                                SearchMass = Data.IdealMasses [ CurChainPeakIndexes [ SearchPeakIndexInChain ] ];
                                UsedChains [ ChainIndexInCluster ] = true;
                                UsedChainCount--;
                                break;
                            }
                        }
                        if ( UsedChains [ ChainIndexInCluster ] == false ) { continue; }
                    }
                    //add chain peaks
                    double CurChainIdealBlockMass = Data.Chains [ ClusterChainIndexes [ ChainIndexInCluster ] ].IdealBlockMass;
                    double StartMass = SearchMass - SearchPeakIndexInChain * CurChainIdealBlockMass;

                    for ( int CurPeakIndexInChain = 0; CurPeakIndexInChain < CurChainPeakIndexes.Length; CurPeakIndexInChain++ ) {
                        if ( CurPeakIndexInChain == SearchPeakIndexInChain ) { continue; }
                        double IdealMass = StartMass + CurPeakIndexInChain * CurChainIdealBlockMass;

                        if( Data.IdealMasses [ CurChainPeakIndexes[ CurPeakIndexInChain ] ] <= 0.1){
                            Data.IdealMasses [ CurChainPeakIndexes [ CurPeakIndexInChain ] ] = IdealMass;
                            MassErrors.Add( Support.CPpmError.CalculateRangePpmError( IdealMass, Data.Masses [ CurChainPeakIndexes [ CurPeakIndexInChain ] ] ) );
                        } else {
                            TestDoublePeakCount++;
                            //check Ppm error
                            if ( Support.CPpmError.CalculateAbsRangePpmError( IdealMass, Data.Masses [ CurChainPeakIndexes [ CurPeakIndexInChain ] ] ) > 1 ) {
                                TestDoublePeakCount = TestDoublePeakCount;
                            }
                        }
                    }
                }
                if( UsedChainCount <= 0){ break;}
            }
            TestDoublePeakCount = TestDoublePeakCount;
        }
        */
        public void AssignIdealMassesTheBiggestCluster(Support.InputData Data)
        {//new
            if ((Data.Clusters == null) || (Data.Clusters.Length <= 1))
            {
                throw new Exception("Cluster index 0 is out ot range");
            }
            double MaxOutlierPpmError = 0;
            var oCluster = Data.Clusters[0];
            var UsedChains = new bool[oCluster.ChainIndexes.Length];
            var UsedChainCount = 0;
            double MaxPpmErrorGain = 0;
            var HigherPpmErrorCount = 0;
            {
                //assign masses in first chain in cluster
                var StartChainIndex = 0;
                var StartChain = Data.Chains[oCluster.ChainIndexes[StartChainIndex]];
                StartChain.ClusteringPeakIndex = StartChain.PeakIndexes[0];
                StartChain.ClusteringMassError = 0;
                var StartChainFirstPeak = 0;
                var BlockMass = StartChain.IdealBlockMass;
                var FirstPeakMass = Data.Masses[StartChain.PeakIndexes[StartChainFirstPeak]] - StartChainFirstPeak * BlockMass;

                for (var ChainPeakIndex = 0; ChainPeakIndex < StartChain.PeakIndexes.Length; ChainPeakIndex++)
                {
                    var IdealMass = FirstPeakMass + ChainPeakIndex * BlockMass;

                    if (Data.IdealMasses[StartChain.PeakIndexes[ChainPeakIndex]] < 0.1)
                    {
                        Data.IdealMasses[StartChain.PeakIndexes[ChainPeakIndex]] = IdealMass;
                        var PpmError = CPpmError.CalculateAbsRangePpmError(Data.Masses[StartChain.PeakIndexes[ChainPeakIndex]], IdealMass);
                        var PpmErrorGain = PpmError / Data.GetErrorStdDev(IdealMass);

                        if (Data.MaxPpmErrorGain < PpmErrorGain)
                        {
                            HigherPpmErrorCount++;

                            if (Data.MaxPpmErrorGain < PpmErrorGain) { Data.MaxPpmErrorGain = PpmErrorGain; }
                        }
                    }
                }
                UsedChains[StartChainIndex] = true;
                UsedChainCount = 1;
            }
            //assign masses in rest chains in cluster
            for (var Iteration = 1; ; Iteration++)
            {
                for (var ChainIndex = 0; ChainIndex < oCluster.ChainIndexes.Length; ChainIndex++)
                {
                    if (UsedChains[ChainIndex]) { continue; }
                    var CurChain = Data.Chains[oCluster.ChainIndexes[ChainIndex]];
                    var ConnectedChainPeakIndex = -1;

                    for (var ChainPeakIndex = 0; ChainPeakIndex < CurChain.PeakIndexes.Length; ChainPeakIndex++)
                    {
                        if (Data.IdealMasses[CurChain.PeakIndexes[ChainPeakIndex]] > 0.1)
                        {
                            ConnectedChainPeakIndex = ChainPeakIndex;
                            break;
                        }
                    }
                    if (ConnectedChainPeakIndex == -1) { continue; }
                    CurChain.ClusteringPeakIndex = CurChain.PeakIndexes[ConnectedChainPeakIndex];

                    if (Data.IdealMasses[CurChain.ClusteringPeakIndex] < 0.1)
                    {
                    }
                    CurChain.ClusteringMassError = Data.Masses[CurChain.ClusteringPeakIndex] - Data.IdealMasses[CurChain.ClusteringPeakIndex];
                    var BlockMass = CurChain.IdealBlockMass;
                    var FirstPeakMass = Data.IdealMasses[CurChain.PeakIndexes[ConnectedChainPeakIndex]] - ConnectedChainPeakIndex * BlockMass;//Masses -> IdealMasses
                    for (var ChainPeakIndex = 0; ChainPeakIndex < CurChain.PeakIndexes.Length; ChainPeakIndex++)
                    {
                        var IdealMass = FirstPeakMass + ChainPeakIndex * BlockMass;

                        if (Data.IdealMasses[CurChain.PeakIndexes[ChainPeakIndex]] < 0.1)
                        {
                            Data.IdealMasses[CurChain.PeakIndexes[ChainPeakIndex]] = IdealMass;
                            //check error
                            var PpmError = CPpmError.CalculateAbsRangePpmError(Data.Masses[CurChain.PeakIndexes[ChainPeakIndex]], IdealMass);
                            var PpmErrorGain = PpmError / Data.GetErrorStdDev(IdealMass) * Iteration;

                            if (Data.MaxPpmErrorGain < PpmErrorGain)
                            {
                                HigherPpmErrorCount++;

                                if (MaxPpmErrorGain < PpmErrorGain) { MaxPpmErrorGain = PpmErrorGain; }
                            }
                        }
                        else
                        {
                            //check
                            var PpmError = CPpmError.CalculateAbsRangePpmError(Data.IdealMasses[CurChain.PeakIndexes[ChainPeakIndex]], IdealMass);

                            if (MaxOutlierPpmError < PpmError)
                            {
                                MaxOutlierPpmError = PpmError;
                            }
                        }
                    }
                    UsedChains[ChainIndex] = true;
                    UsedChainCount++;
                }
                if (UsedChainCount >= oCluster.ChainIndexes.Length) { break; }
            }
            oCluster.HigherPpmErrorCount = HigherPpmErrorCount;
            oCluster.MaxPpmErrorGain = MaxPpmErrorGain;
            oCluster.MaxOutlierPpmError = MaxOutlierPpmError;
        }

        public double GetPeakScore(Support.InputData Data, int PeakIndex)
        {
            return Data.Abundances[PeakIndex] / Data.Masses[PeakIndex];
        }

        public void AssignIdealMassesInClusterBasedTheBestPeak(Support.InputData Data, int ClusterIndex)
        {
            if ((Data.Clusters == null) || (Data.Clusters.Length <= ClusterIndex))
            {
                throw new Exception("Cluster index " + ClusterIndex + " is out ot range");
            }
            var ClusterChainIndexes = Data.Clusters[ClusterIndex].ChainIndexes;

            //assign the best chain
            var TheBestChainIndex = Data.Clusters[0].TheBestChainIndex;
            var TheBestChainPeakIndex = Data.Clusters[0].TheBestPeakIndex;
            var TheBestChainPeakIndexes = Data.Chains[ClusterChainIndexes[TheBestChainIndex]].PeakIndexes;
            var TheBestChainIdealBlockMass = Data.Chains[ClusterChainIndexes[TheBestChainIndex]].IdealBlockMass;
            var TheBestChainStartPeakMass = Data.Masses[TheBestChainPeakIndexes[TheBestChainPeakIndex]] - TheBestChainIdealBlockMass * TheBestChainPeakIndex;

            for (var CurPeakIndexInChain = 0; CurPeakIndexInChain < TheBestChainPeakIndexes.Length; CurPeakIndexInChain++)
            {
                var PeakIndex = TheBestChainPeakIndexes[CurPeakIndexInChain];

                if (Data.IdealMasses[PeakIndex] > 0.1) { throw new Exception("Algorithm error"); }
                Data.IdealMasses[PeakIndex] = TheBestChainStartPeakMass + CurPeakIndexInChain * TheBestChainIdealBlockMass;
            }
            //align rest chains
            var UsedChains = new bool[ClusterChainIndexes.Length];
            var MassErrors = new List<double>();
            UsedChains[TheBestChainPeakIndex] = true;
            var UsedChainCount = ClusterChainIndexes.Length - 1;
            var TestDoublePeakCount = 0;

            for (; ; )
            {
                for (var ChainIndexInCluster = 0; ChainIndexInCluster < ClusterChainIndexes.Length; ChainIndexInCluster++)
                {
                    if (UsedChains[ChainIndexInCluster]) { continue; }
                    var CurChainPeakIndexes = Data.Chains[ClusterChainIndexes[ChainIndexInCluster]].PeakIndexes;
                    var SearchPeakIndexInChain = 0;

                    for (; SearchPeakIndexInChain < CurChainPeakIndexes.Length; SearchPeakIndexInChain++)
                    {
                        if (Data.IdealMasses[CurChainPeakIndexes[SearchPeakIndexInChain]] > 0.1)
                        {
                            UsedChains[ChainIndexInCluster] = true;
                            UsedChainCount--;
                            break;
                        }
                    }
                    if (!UsedChains[ChainIndexInCluster]) { continue; }

                    //add chain peaks
                    var CurChainIdealBlockMass = Data.Chains[ClusterChainIndexes[ChainIndexInCluster]].IdealBlockMass;
                    var StartMass = Data.IdealMasses[CurChainPeakIndexes[SearchPeakIndexInChain]] - SearchPeakIndexInChain * CurChainIdealBlockMass;

                    for (var CurPeakIndexInChain = 0; CurPeakIndexInChain < CurChainPeakIndexes.Length; CurPeakIndexInChain++)
                    {
                        if (CurPeakIndexInChain == SearchPeakIndexInChain) { continue; }
                        var IdealMass = StartMass + CurPeakIndexInChain * CurChainIdealBlockMass;

                        if (Data.IdealMasses[CurChainPeakIndexes[CurPeakIndexInChain]] <= 0.1)
                        {
                            Data.IdealMasses[CurChainPeakIndexes[CurPeakIndexInChain]] = IdealMass;
                            MassErrors.Add(IdealMass - Data.Masses[CurChainPeakIndexes[CurPeakIndexInChain]]);
                            //MassErrors.Add( Support.CPpmError.CalculateAbsRangePpmError( IdealMass, Data.Masses [ CurChainPeakIndexes [ CurPeakIndexInChain ] ] ) );
                        }
                        else
                        {
                            TestDoublePeakCount++;
                            //check Ppm error
                            if (Support.CPpmError.CalculateAbsRangePpmError(IdealMass, Data.Masses[CurChainPeakIndexes[CurPeakIndexInChain]]) > 1)
                            {
                                TestDoublePeakCount = TestDoublePeakCount;
                            }
                        }
                    }
                }
                if (UsedChainCount <= 0) { break; }
            }
        }

        public void AssignC13IdealMasses(Support.InputData Data, int ClusterIndex)
        {
            var FirstClusterPeakIndexes = Data.GetClusterPeakIndexes(0);
            var SecondClusterPeakIndexes = Data.GetClusterPeakIndexes(ClusterIndex);
            var C13BlockMass = CElements.C13 - CElements.C;
            var ParentPeakList = new List<int>();
            var C13PeakList = new List<int>();
            int TestInt;

            foreach (var PeakIndex in FirstClusterPeakIndexes)
            {
                if (Data.IdealMasses[PeakIndex] < 0.1)
                {
                    TestInt = PeakIndex;
                    //throw new Exception( "There is not ideal mass of parent mass peak; function AssignC13IdealMasses" );
                }
                var PeakMass = Data.Masses[PeakIndex];
                var SearchPeakIndex = CPpmError.SearchNearPeakIndex(Data.Masses, PeakMass + C13BlockMass);
                /*
                int SearchPeakIndex = CPpmError.SearchPeakIndex( Data, PeakMass + C13BlockMass, PeakIndex + 1 );

                if ( SearchPeakIndex < 0 ) { continue; }
                if ( Data.IdealMasses [ SearchPeakIndex ] > 0.1 ) {
                    SearchPeakIndex = SearchPeakIndex;
                    //throw new Exception( "There is ideal mass of Search C13 peak; function AssignC13IdealMasses" );
                }
                Data.IdealMasses [ SearchPeakIndex ] = Data.IdealMasses [ PeakIndex ] + C13BlockMass;
                 */
                if (SecondClusterPeakIndexes.Contains(SearchPeakIndex))
                {
                    ParentPeakList.Add(PeakIndex);
                    C13PeakList.Add(SearchPeakIndex);
                }
            }
            Data.IsotopicParentPeaks = ParentPeakList.ToArray();
            Data.IsotopicChildPeaks = C13PeakList.ToArray();
        }

        public void AssignIdealMassesToRestPeaks(Support.InputData Data)
        {
            var MinMassInteger = (int)Math.Round(Data.Masses.Min());
            var MaxMassInteger = (int)Math.Round(Data.Masses.Max());
            var MinPeakIndexesAtInteger = new int[MaxMassInteger + 1];
            var MaxPeakIndexesAtInteger = new int[MaxMassInteger + 1];
            var PeakIndex = 0;

            for (var MassInteger = MinMassInteger; MassInteger <= MaxMassInteger; MassInteger++)
            {
                var FirstPeakIndex = PeakIndex;

                for (; PeakIndex < Data.Masses.Length;)
                {
                    var CurMassInteger = (int)Math.Round(Data.Masses[PeakIndex]);

                    if (CurMassInteger != MassInteger)
                    {
                        if (FirstPeakIndex == PeakIndex)
                        {
                            MinPeakIndexesAtInteger[MassInteger] = -1;
                            MaxPeakIndexesAtInteger[MassInteger] = -1;
                        }
                        else
                        {
                            MinPeakIndexesAtInteger[MassInteger] = FirstPeakIndex;
                            MaxPeakIndexesAtInteger[MassInteger] = PeakIndex - 1;
                        }
                        break;
                    }
                    PeakIndex++;
                }
            }
            var IdealPeakIndexesAtIntegers = new int[MaxMassInteger + 1][];
            var IdealMassMeansAtIntegers = new double[MaxMassInteger + 1];
            var IdealMassErrorMeansAtIntegers = new double[MaxMassInteger + 1];
            var FirstIdealMassInteger = -1;
            var LastIdealMassInteger = -1;

            for (var MassInteger = MinMassInteger; MassInteger <= MaxMassInteger; MassInteger++)
            {
                if (MinPeakIndexesAtInteger[MassInteger] < 0) { continue; }
                var IdealPeakIndexes = new List<int>();
                var IdealMasses = new List<double>();
                var IdealMassErrors = new List<double>();

                for (PeakIndex = MinPeakIndexesAtInteger[MassInteger]; PeakIndex <= MaxPeakIndexesAtInteger[MassInteger]; PeakIndex++)
                {
                    if (Data.IdealMasses[PeakIndex] > 0.1)
                    {
                        IdealPeakIndexes.Add(PeakIndex);
                        IdealMasses.Add(Data.IdealMasses[PeakIndex]);
                        IdealMassErrors.Add(Data.Masses[PeakIndex] - Data.IdealMasses[PeakIndex]);
                    }
                }
                if (IdealPeakIndexes.Count == 0) { continue; }
                if (FirstIdealMassInteger == -1) { FirstIdealMassInteger = MassInteger; }
                LastIdealMassInteger = MassInteger;
                IdealPeakIndexesAtIntegers[MassInteger] = IdealPeakIndexes.ToArray();
                IdealMassMeansAtIntegers[MassInteger] = Support.CArrayMath.Mean(IdealMasses.ToArray());
                IdealMassErrorMeansAtIntegers[MassInteger] = Support.CArrayMath.Mean(IdealMassErrors.ToArray());
            }

            for (PeakIndex = 0; PeakIndex < Data.Masses.Length; PeakIndex++)
            {
                if (Data.IdealMasses[PeakIndex] > 0.1) { continue; }
                var MassInteger = (int)Math.Round(Data.Masses[PeakIndex]);

                if (IdealMassMeansAtIntegers[MassInteger] > 0.1)
                {
                    //Integer has ideal peaks
                    if (PeakIndex < IdealPeakIndexesAtIntegers[MassInteger].First())
                    {
                        Data.IdealMasses[PeakIndex] = Data.Masses[PeakIndex] - (Data.Masses[IdealPeakIndexesAtIntegers[MassInteger].First()] - Data.IdealMasses[IdealPeakIndexesAtIntegers[MassInteger].First()]);
                    }
                    else if (PeakIndex > IdealPeakIndexesAtIntegers[MassInteger].Last())
                    {
                        Data.IdealMasses[PeakIndex] = Data.Masses[PeakIndex] - (Data.Masses[IdealPeakIndexesAtIntegers[MassInteger].Last()] - Data.IdealMasses[IdealPeakIndexesAtIntegers[MassInteger].Last()]);
                    }
                    else
                    {
                        var IdealPeakIndexAtInteger = Array.BinarySearch(IdealPeakIndexesAtIntegers[MassInteger], PeakIndex);

                        if (IdealPeakIndexAtInteger < 0)
                        {
                            IdealPeakIndexAtInteger = ~IdealPeakIndexAtInteger;
                        }
                        else
                        {
                            throw new Exception("Algorithm error");
                        }
                        var LeftIdealPeak = IdealPeakIndexesAtIntegers[MassInteger][IdealPeakIndexAtInteger - 1];
                        var RightIdealPeak = IdealPeakIndexesAtIntegers[MassInteger][IdealPeakIndexAtInteger];
                        Data.IdealMasses[PeakIndex] = Data.Masses[PeakIndex]
                                + Support.CArrayMath.LinearValue(Data.Masses[PeakIndex], Data.Masses[LeftIdealPeak], Data.Masses[RightIdealPeak],
                                Data.IdealMasses[LeftIdealPeak] - Data.Masses[LeftIdealPeak],
                                Data.IdealMasses[RightIdealPeak] - Data.Masses[RightIdealPeak]);
                    }
                }
                else
                {
                    //Integer doesn't have ideal peaks
                    if (MassInteger < FirstIdealMassInteger)
                    {
                        if (FirstIdealMassInteger >= 0)
                        {
                            Data.IdealMasses[PeakIndex] = Data.Masses[PeakIndex] - IdealMassErrorMeansAtIntegers[FirstIdealMassInteger];
                        }
                    }
                    else if (MassInteger > LastIdealMassInteger)
                    {
                        if (LastIdealMassInteger >= 0)
                        {
                            Data.IdealMasses[PeakIndex] = Data.Masses[PeakIndex] - IdealMassErrorMeansAtIntegers[LastIdealMassInteger];
                        }
                    }
                    else
                    {
                        var LeftMassInteger = MassInteger - 1;

                        for (; LeftMassInteger >= FirstIdealMassInteger; LeftMassInteger--)
                        {
                            if (IdealMassMeansAtIntegers[LeftMassInteger] > 0.1) { break; }
                        }
                        var RightMassInteger = MassInteger + 1;

                        for (; RightMassInteger <= LastIdealMassInteger; RightMassInteger++)
                        {
                            if (IdealMassMeansAtIntegers[RightMassInteger] > 0.1) { break; }
                        }
                        Data.IdealMasses[PeakIndex] = Data.Masses[PeakIndex]
                                - Support.CArrayMath.LinearValue(Data.Masses[PeakIndex], IdealMassMeansAtIntegers[LeftMassInteger], IdealMassMeansAtIntegers[RightMassInteger],
                                IdealMassErrorMeansAtIntegers[LeftMassInteger], IdealMassErrorMeansAtIntegers[RightMassInteger]);
                    }
                }
            }
        }

        public enum DistancePeakType { Pair, MPair, DM, MDM, Chain, MChain, UChain, MUChain, Isotope, MIsotope, Att };
        public class PairDistance
        {
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

        public PairDistance[] GetPairChainIsotopeStatistics(Support.InputData InputData, double MaxPairDistance = 50, int UnknownCount = 10)
        {
            const double MaxOffsetPpmError = 5;
            //find error along MZ
            var StandardBlockMasses = new[] { 2 * CElements.H, CElements.C, 2 * CElements.H + CElements.C, CElements.O };
            CalculateErrorDistribution(InputData, MaxOffsetPpmError, StandardBlockMasses);
            InputData.MaxPpmErrorGain = 3;

            //Bin zise is Min StdDev of Max Peak Mass / 2
            var Masses = InputData.Masses;
            var BinMass = Masses[Masses.Length - 1];
            var BinSize = (CPpmError.CalculateMass(BinMass, InputData.ErrDisStdDevs[0]) - BinMass) / 2;

            var BinCount = (int)(MaxPairDistance / BinSize + 2);
            var LowPeakIndexInBins = new List<int>[BinCount];
            var UpperPeakIndexInBins = new List<int>[BinCount];

            for (var LowIndex = 0; LowIndex < Masses.Length - 1; LowIndex++)
            {
                for (var UpperIndex = LowIndex + 1; UpperIndex < Masses.Length; UpperIndex++)
                {
                    var Distance = Masses[UpperIndex] - Masses[LowIndex];

                    if (Distance > MaxPairDistance) { break; }
                    var BinIndex = (int)(Distance / BinSize);

                    if (LowPeakIndexInBins[BinIndex] == null)
                    {
                        LowPeakIndexInBins[BinIndex] = new List<int>();
                        UpperPeakIndexInBins[BinIndex] = new List<int>();
                    }
                    LowPeakIndexInBins[BinIndex].Add(LowIndex);
                    UpperPeakIndexInBins[BinIndex].Add(UpperIndex);
                }
            }

            var DistancePeakList = new List<PairDistance>();
            var NoiseBinPairs = 2;
            var MinPairs = 50;

            for (var BinIndex = 0; BinIndex < BinCount; BinIndex++)
            {
                //find next peak
                //???more than 1 peak
                if (LowPeakIndexInBins[BinIndex] == null) { continue; }
                if (LowPeakIndexInBins[BinIndex].Count < NoiseBinPairs) { continue; }
                var StartBinIndex = BinIndex;

                for (BinIndex++; BinIndex < BinCount; BinIndex++)
                {
                    if (LowPeakIndexInBins[BinIndex] == null) { break; }
                    if (LowPeakIndexInBins[BinIndex].Count < NoiseBinPairs)
                    {
                        break;
                    }
                }
                var EndBinIndex = BinIndex - 1;

                if (EndBinIndex - StartBinIndex < 2) { continue; }

                var Distances = new List<double>();

                for (var CurBinIndex = StartBinIndex; CurBinIndex <= EndBinIndex; CurBinIndex++)
                {
                    for (var PairIndex = 0; PairIndex < LowPeakIndexInBins[CurBinIndex].Count; PairIndex++)
                    {
                        //???check that ppm error < range ppm error
                        Distances.Add(Masses[UpperPeakIndexInBins[CurBinIndex][PairIndex]] - Masses[LowPeakIndexInBins[CurBinIndex][PairIndex]]);
                    }
                }
                if (Distances.Count < MinPairs) { continue; }
                var PeakMean = MathNet.Numerics.Statistics.Statistics.Mean(Distances);
                //???linear adjustment of StdDev ppm error to range 0 ppm error
                //simple StdDev
                var PeakStdDev = MathNet.Numerics.Statistics.Statistics.StandardDeviation(Distances);
                var CurDistancePeak = new PairDistance();
                CurDistancePeak.LowBinIndex = StartBinIndex;
                CurDistancePeak.UpperBinIndex = EndBinIndex;
                CurDistancePeak.PairCount = Distances.Count;
                CurDistancePeak.Mean = PeakMean;
                CurDistancePeak.StdDev = PeakStdDev;
                CurDistancePeak.DistancePeakTypeList = new List<DistancePeakType>();
                CurDistancePeak.FormulaList = new List<string>();
                CurDistancePeak.FormulaDistanceList = new List<double>();
                DistancePeakList.Add(CurDistancePeak);
            }
            const int MinChainPeakCount = 5;
            const int MinChainCount = 5;

            if (CIsotope.GetNames() == null)
            {
                var IsotopeFilename = "Isotope.inf";
                CIsotope.ConvertIsotopeFileIntoIsotopeDistanceFile(IsotopeFilename);
            }
            for (var DistancePeakIndex = 0; DistancePeakIndex < DistancePeakList.Count; DistancePeakIndex++)
            {
                var CurDistancePeak = DistancePeakList[DistancePeakIndex];
                //search dm, chains and isotope
                var DmIndex = FindKnownFormulaIndex(InputData, CurDistancePeak.Mean);
                FindChains1(InputData, MinChainPeakCount, InputData.Masses.Last() + 1, new[] { CurDistancePeak.Mean });

                if (InputData.Chains.Length >= MinChainCount)
                {
                    CurDistancePeak.ChainCount = InputData.Chains.Length;
                    var Distances = new List<double>();

                    for (var ChainIndex = 0; ChainIndex < InputData.Chains.Length; ChainIndex++)
                    {
                        var PeakMasses = InputData.Chains[ChainIndex].PeakMasses;

                        for (var PeakMassIndex = 0; PeakMassIndex < PeakMasses.Length - 2; PeakMassIndex++)
                        {
                            Distances.Add(PeakMasses[PeakMassIndex + 1] - PeakMasses[PeakMassIndex]);
                        }
                    }
                    CurDistancePeak.ChainMean = MathNet.Numerics.Statistics.Statistics.Mean(Distances);
                    CurDistancePeak.ChainStdDev = MathNet.Numerics.Statistics.Statistics.StandardDeviation(Distances);
                }
                //int IsotopeIndex = CIsotope.FindIsotopeIndex( InputData, IsotopeDistances, CurDistancePeak.Mean, CurDistancePeak.StdDev );
                var IsotopeIndex = CIsotope.FindIsotopeIndex(InputData, CurDistancePeak.Mean, CurDistancePeak.StdDev);

                if (InputData.Chains.Length >= MinChainCount)
                {
                    if (DmIndex >= 0)
                    {
                        CurDistancePeak.DistancePeakTypeList.Add(DistancePeakType.Chain);
                        CurDistancePeak.FormulaList.Add(KnownBlockNames[DmIndex]);
                        CurDistancePeak.FormulaDistanceList.Add(KnownBlockMasses[DmIndex]);
                    }
                }
                if (IsotopeIndex >= 0)
                {
                    CurDistancePeak.DistancePeakTypeList.Add(DistancePeakType.Isotope);
                    CurDistancePeak.FormulaList.Add(CIsotope.GetNames()[IsotopeIndex]);
                    CurDistancePeak.FormulaDistanceList.Add(CIsotope.GetDistances()[IsotopeIndex]);
                }
                if ((DmIndex >= 0) && (IsotopeIndex < 0) && (InputData.Chains.Length < MinChainCount))
                {
                    CurDistancePeak.DistancePeakTypeList.Add(DistancePeakType.DM);
                    CurDistancePeak.FormulaList.Add(KnownBlockNames[DmIndex]);
                    CurDistancePeak.FormulaDistanceList.Add(KnownBlockMasses[DmIndex]);
                }
            }
            //mark n*Distance from Chain, Isotopes, MD
            for (var DistancePeakIndex = 0; DistancePeakIndex < DistancePeakList.Count; DistancePeakIndex++)
            {
                var CurDistancePeak = DistancePeakList[DistancePeakIndex];

                if (CurDistancePeak.DistancePeakTypeList.Count == 0) { continue; }
                if (((CurDistancePeak.DistancePeakTypeList[0] != DistancePeakType.DM)
                            && (CurDistancePeak.DistancePeakTypeList[0] != DistancePeakType.Chain)
                            && (CurDistancePeak.DistancePeakTypeList[0] != DistancePeakType.UChain)
                            && (CurDistancePeak.DistancePeakTypeList[0] != DistancePeakType.Isotope)))
                {
                    continue;
                }
                var LowMultiplyDistancePeakIndex = DistancePeakIndex - 1;
                var UpperMultiplyDistancePeakIndex = DistancePeakIndex + 1;

                for (var Multiply = 2; Multiply < 10; Multiply++)
                {
                    var LowMultiplyDistance = CurDistancePeak.Mean / Multiply;

                    for (; LowMultiplyDistancePeakIndex >= 0; LowMultiplyDistancePeakIndex--)
                    {
                        var TempDistancePeak = DistancePeakList[LowMultiplyDistancePeakIndex];

                        if (LowMultiplyDistance < TempDistancePeak.Mean - 3 * TempDistancePeak.StdDev) { continue; }
                        if (LowMultiplyDistance > TempDistancePeak.Mean + 3 * TempDistancePeak.StdDev)
                        {
                            LowMultiplyDistancePeakIndex++;
                            break;
                        }
                        for (var Index = 0; Index < CurDistancePeak.FormulaList.Count; Index++)
                        {
                            switch (CurDistancePeak.DistancePeakTypeList[Index])
                            {
                                case DistancePeakType.DM:
                                    if (TempDistancePeak.ChainCount == 0)
                                    {
                                        TempDistancePeak.DistancePeakTypeList.Add(DistancePeakType.MDM);
                                    }
                                    break;
                                case DistancePeakType.Chain:
                                    if (TempDistancePeak.ChainCount > 0)
                                    {
                                        TempDistancePeak.DistancePeakTypeList.Add(DistancePeakType.MChain);
                                    }
                                    break;
                                case DistancePeakType.Isotope:
                                    TempDistancePeak.DistancePeakTypeList.Add(DistancePeakType.MIsotope);
                                    break;
                                default:
                                    //TempDistancePeak.DistancePeakTypeList.Add( DistancePeakType.Unknown );
                                    break;
                            }
                            if (TempDistancePeak.DistancePeakTypeList.Count > TempDistancePeak.FormulaList.Count)
                            {
                                TempDistancePeak.FormulaList.Add("1 / " + Multiply.ToString() + " * " + CurDistancePeak.FormulaList[Index]);
                                TempDistancePeak.FormulaDistanceList.Add(1.0 / Multiply * CurDistancePeak.FormulaDistanceList[Index]);
                            }
                        }
                    }
                    var UpperMultiplyDistance = CurDistancePeak.Mean * Multiply;

                    if (UpperMultiplyDistance > MaxPairDistance) { break; }
                    for (; UpperMultiplyDistancePeakIndex < DistancePeakList.Count; UpperMultiplyDistancePeakIndex++)
                    {
                        var TempDistancePeak = DistancePeakList[UpperMultiplyDistancePeakIndex];

                        if (UpperMultiplyDistance > TempDistancePeak.Mean + 3 * TempDistancePeak.StdDev) { continue; }
                        if (UpperMultiplyDistance < TempDistancePeak.Mean - 3 * TempDistancePeak.StdDev)
                        {
                            UpperMultiplyDistancePeakIndex--;
                            break;
                        }
                        for (var Index = 0; Index < CurDistancePeak.DistancePeakTypeList.Count; Index++)
                        {
                            switch (CurDistancePeak.DistancePeakTypeList[Index])
                            {
                                case DistancePeakType.DM:
                                    if (TempDistancePeak.ChainCount == 0)
                                    {
                                        TempDistancePeak.DistancePeakTypeList.Add(DistancePeakType.MDM);
                                    }
                                    break;
                                case DistancePeakType.Chain:
                                    if (TempDistancePeak.ChainCount > 0)
                                    {
                                        TempDistancePeak.DistancePeakTypeList.Add(DistancePeakType.MChain);
                                    }
                                    break;
                                case DistancePeakType.Isotope:
                                    TempDistancePeak.DistancePeakTypeList.Add(DistancePeakType.MIsotope);
                                    break;
                                default:
                                    //TempDistancePeak.DistancePeakTypeList.Add( DistancePeakType.Unknown );
                                    break;
                            }
                            if (TempDistancePeak.DistancePeakTypeList.Count > TempDistancePeak.FormulaList.Count)
                            {
                                TempDistancePeak.FormulaList.Add(Multiply.ToString() + " * " + CurDistancePeak.FormulaList[Index]);
                                TempDistancePeak.FormulaDistanceList.Add(Multiply * CurDistancePeak.FormulaDistanceList[Index]);
                            }
                        }
                    }
                }
            }
            //mark n*Distance from UChain and Pair
            for (var DistancePeakIndex = 0; DistancePeakIndex < DistancePeakList.Count; DistancePeakIndex++)
            {
                var CurDistancePeak = DistancePeakList[DistancePeakIndex];

                if (CurDistancePeak.DistancePeakTypeList.Count > 0) { continue; }
                if (CurDistancePeak.ChainCount > 0)
                {
                    CurDistancePeak.DistancePeakTypeList.Add(DistancePeakType.UChain);
                }
                else
                {
                    CurDistancePeak.DistancePeakTypeList.Add(DistancePeakType.Pair);
                }
                CurDistancePeak.FormulaList.Add("Unknown");
                CurDistancePeak.FormulaDistanceList.Add(CurDistancePeak.Mean);
                var UpperMultiplyDistancePeakIndex = DistancePeakIndex + 1;

                for (var Multiply = 2; Multiply < 10; Multiply++)
                {
                    var UpperMultiplyDistance = CurDistancePeak.Mean * Multiply;

                    if (UpperMultiplyDistance > MaxPairDistance) { break; }
                    for (; UpperMultiplyDistancePeakIndex < DistancePeakList.Count; UpperMultiplyDistancePeakIndex++)
                    {
                        var TempDistancePeak = DistancePeakList[UpperMultiplyDistancePeakIndex];

                        if (UpperMultiplyDistance > TempDistancePeak.Mean + 3 * TempDistancePeak.StdDev) { continue; }
                        if (UpperMultiplyDistance < TempDistancePeak.Mean - 3 * TempDistancePeak.StdDev)
                        {
                            UpperMultiplyDistancePeakIndex--;
                            break;
                        }
                        for (var Index = 0; Index < CurDistancePeak.DistancePeakTypeList.Count; Index++)
                        {
                            switch (CurDistancePeak.DistancePeakTypeList[Index])
                            {
                                case DistancePeakType.UChain:
                                    if (TempDistancePeak.ChainCount > 0)
                                    {
                                        TempDistancePeak.DistancePeakTypeList.Add(DistancePeakType.MUChain);
                                    }
                                    break;
                                case DistancePeakType.Pair:
                                    TempDistancePeak.DistancePeakTypeList.Add(DistancePeakType.MPair);
                                    break;
                                default:
                                    //TempDistancePeak.DistancePeakTypeList.Add( DistancePeakType.Unknown );
                                    break;
                            }
                            if (TempDistancePeak.DistancePeakTypeList.Count > TempDistancePeak.FormulaList.Count)
                            {
                                TempDistancePeak.FormulaList.Add(Multiply.ToString() + " * Unknown");
                                TempDistancePeak.FormulaDistanceList.Add(Multiply * CurDistancePeak.Mean);
                            }
                        }
                    }
                }
            }
            //sort by PairCount
            {
                var Pairs = new int[DistancePeakList.Count];

                for (var DistancePeakIndex = 0; DistancePeakIndex < DistancePeakList.Count; DistancePeakIndex++)
                {
                    Pairs[DistancePeakIndex] = DistancePeakList[DistancePeakIndex].PairCount;
                }
                var PairDistances = DistancePeakList.ToArray();
                Array.Sort(Pairs, PairDistances);
                Array.Reverse(PairDistances);
                DistancePeakList = PairDistances.ToList();
            }
            //remove small Unknown
            var RemovePairDistanceList = new List<int>();
            var UChainIndex = 0;
            var UPairIndex = 0;

            for (var PairDistanceIndex = 0; PairDistanceIndex < DistancePeakList.Count; PairDistanceIndex++)
            {
                var Cur = DistancePeakList[PairDistanceIndex];
                var RemoveIndexList = new List<int>();

                for (var ListIndex = 0; ListIndex < Cur.FormulaList.Count; ListIndex++)
                {
                    var CurDistancePeakType = Cur.DistancePeakTypeList[ListIndex];

                    if (((CurDistancePeakType == DistancePeakType.UChain) || (CurDistancePeakType == DistancePeakType.MUChain)))
                    {
                        if (UChainIndex >= UnknownCount)
                        {
                            RemoveIndexList.Add(ListIndex); ;
                        }
                        else
                        {
                            UChainIndex++;
                        }
                    }
                    else if (((CurDistancePeakType == DistancePeakType.Pair) || (CurDistancePeakType == DistancePeakType.MPair)))
                    {
                        if (UPairIndex >= UnknownCount)
                        {
                            RemoveIndexList.Add(ListIndex);
                        }
                        else
                        {
                            UPairIndex++;
                        }
                    }
                }
                RemoveIndexList.Reverse();

                foreach (var ListIndex in RemoveIndexList)
                {
                    Cur.DistancePeakTypeList.RemoveAt(ListIndex);
                    Cur.FormulaDistanceList.RemoveAt(ListIndex);
                    Cur.FormulaList.RemoveAt(ListIndex);
                }
                if (Cur.DistancePeakTypeList.Count == 0)
                {
                    RemovePairDistanceList.Add(PairDistanceIndex);
                }
            }
            RemovePairDistanceList.Reverse();

            foreach (var PairDistanceIndex in RemovePairDistanceList)
            {
                DistancePeakList.RemoveAt(PairDistanceIndex);
            }
            return DistancePeakList.ToArray();
        }

        public string PairChainIsotopeStatisticsToString(PairDistance[] PairDistances, int UnknownCount = 10)
        {
            var TextHeader = "LowBinIndex,UpperBinIndex,PairCount,DistanceMean,DistanceStdDev,Type,Formula,FormulaDistance,ChainCount,ChainMean,ChainStdDev";
            var ChainText = new StringBuilder();
            var IsotopeText = new StringBuilder();
            var DMText = new StringBuilder();
            var UChainText = new StringBuilder();
            var UChainIndex = 0;
            var UPairIndex = 0;
            var PairText = new StringBuilder();

            for (var Index = 0; Index < PairDistances.Length; Index++)
            {
                var Cur = PairDistances[Index];

                for (var ListIndex = 0; ListIndex < Cur.FormulaList.Count; ListIndex++)
                {
                    var CurDistancePeakType = Cur.DistancePeakTypeList[ListIndex];

                    if (((CurDistancePeakType == DistancePeakType.DM) || (CurDistancePeakType == DistancePeakType.MDM)))
                    {
                        DMText.Append("\r\n" + Cur.LowBinIndex + "," + Cur.UpperBinIndex + "," + Cur.PairCount + "," + Cur.Mean.ToString("F8") + "," + Cur.StdDev.ToString("F8")
                                + "," + Cur.DistancePeakTypeList[ListIndex].ToString() + ',' + Cur.FormulaList[ListIndex] + "," + Cur.FormulaDistanceList[ListIndex].ToString("F8")
                                + "," + Cur.ChainCount + "," + Cur.ChainMean + "," + Cur.ChainStdDev.ToString("F8"));
                    }
                    else if (((CurDistancePeakType == DistancePeakType.Chain) || (CurDistancePeakType == DistancePeakType.MChain)))
                    {
                        ChainText.Append("\r\n" + Cur.LowBinIndex + "," + Cur.UpperBinIndex + "," + Cur.PairCount + "," + Cur.Mean.ToString("F8") + "," + Cur.StdDev.ToString("F8")
                                + "," + Cur.DistancePeakTypeList[ListIndex].ToString() + ',' + Cur.FormulaList[ListIndex] + "," + Cur.FormulaDistanceList[ListIndex].ToString("F8")
                                + "," + Cur.ChainCount + "," + Cur.ChainMean + "," + Cur.ChainStdDev.ToString("F8"));
                    }
                    else if (((CurDistancePeakType == DistancePeakType.Isotope) || (CurDistancePeakType == DistancePeakType.MIsotope)))
                    {
                        IsotopeText.Append("\r\n" + Cur.LowBinIndex + "," + Cur.UpperBinIndex + "," + Cur.PairCount + "," + Cur.Mean.ToString("F8") + "," + Cur.StdDev.ToString("F8")
                                + "," + Cur.DistancePeakTypeList[ListIndex].ToString() + ',' + Cur.FormulaList[ListIndex] + "," + Cur.FormulaDistanceList[ListIndex].ToString("F8")
                                + "," + Cur.ChainCount + "," + Cur.ChainMean + "," + Cur.ChainStdDev.ToString("F8"));
                    }
                    else if (((CurDistancePeakType == DistancePeakType.UChain) || (CurDistancePeakType == DistancePeakType.MUChain)))
                    {
                        if (UChainIndex >= UnknownCount) { continue; }
                        UChainIndex++;
                        UChainText.Append("\r\n" + Cur.LowBinIndex + "," + Cur.UpperBinIndex + "," + Cur.PairCount + "," + Cur.Mean.ToString("F8") + "," + Cur.StdDev.ToString("F8")
                                + "," + Cur.DistancePeakTypeList[ListIndex].ToString() + ',' + Cur.FormulaList[ListIndex] + "," + Cur.FormulaDistanceList[ListIndex].ToString("F8")
                                + "," + Cur.ChainCount + "," + Cur.ChainMean + "," + Cur.ChainStdDev.ToString("F8"));
                    }
                    else if (((CurDistancePeakType == DistancePeakType.Pair) || (CurDistancePeakType == DistancePeakType.MPair)))
                    {
                        if (UPairIndex >= UnknownCount) { continue; }
                        UPairIndex++;
                        PairText.Append("\r\n" + Cur.LowBinIndex + "," + Cur.UpperBinIndex + "," + Cur.PairCount + "," + Cur.Mean.ToString("F8") + "," + Cur.StdDev.ToString("F8")
                                + "," + Cur.DistancePeakTypeList[ListIndex].ToString() + ',' + Cur.FormulaList[ListIndex] + "," + Cur.FormulaDistanceList[ListIndex].ToString("F8")
                                + "," + Cur.ChainCount + "," + Cur.ChainMean + "," + Cur.ChainStdDev.ToString("F8"));
                    }
                }
            }
            return TextHeader + DMText + ChainText + IsotopeText + UChainText + PairText;
            //File.WriteAllText( ParentFilename + "ChainsIsotopes.csv", Text.ToString() );
        }
    }
}

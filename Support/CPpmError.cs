using System;
using System.Linq;

namespace Support
{
    public class CPpmError
    {
        const double PPM = 1000000.0;//parts per million

        public static double PpmToError(double Mass, double PpmError) { return Mass * PpmError / PPM; }
        public static double ErrorToPpm(double Mass, double Error) { return Error * PPM / Mass; }

        public static double SignedPpmPPM(double ReferenceMass, double Mass) { return (Mass - ReferenceMass) / ReferenceMass * PPM; }
        public static double AbsPpmError(double ReferenceMass, double Mass) { return Math.Abs(SignedPpmPPM(ReferenceMass, Mass)); }

        public static double MassMinusPpmError(double Mass, double PpmError) { return Mass / (1.0 + PpmError / PPM); }
        public static double MassPlusPpmError(double Mass, double PpmError) { return Mass * (1.0 + PpmError / PPM); }

        //===================================================
        public enum EMassType { MinMass, BestMass, MaxMass };
        public static double CalculateRangePpmError(double Mass1, double Mass2)
        {
            if (Mass2 >= Mass1)
            {
                return (Mass2 - Mass1) / Mass1 * PPM;
            }
            else
            {
                return (Mass2 - Mass1) / Mass2 * PPM;
            }
        }
        public static double CalculateAbsRangePpmError(double Mass1, double Mass2)
        {
            return Math.Abs(CalculateRangePpmError(Mass1, Mass2));
        }
        public enum EMassArgument { None, First, Second };
        public static EMassArgument FindBetterMass(double ReferenceMass, double FirstMass, double SecondMass, double MaxPpmError)
        {
            var FirstMassPpmError = CalculateAbsRangePpmError(ReferenceMass, FirstMass);
            var SecondMassPpmError = CalculateAbsRangePpmError(ReferenceMass, SecondMass);
            if ((FirstMassPpmError <= SecondMassPpmError) && (FirstMassPpmError <= MaxPpmError))
            {
                return EMassArgument.First;
            }
            else if ((FirstMassPpmError > SecondMassPpmError) && (SecondMassPpmError <= MaxPpmError))
            {
                return EMassArgument.Second;
            }
            return EMassArgument.None;
        }
        public static double CalculateMass(double Mass, double PpmError, EMassType MassType = EMassType.MaxMass)
        {
            switch (MassType)
            {
                case EMassType.BestMass: return Mass;
                case EMassType.MaxMass: return Mass * (1.0 + PpmError / PPM);
                case EMassType.MinMass: return Mass / (1.0 + PpmError / PPM);
                default: throw new Exception(EMassType.MaxMass.GetType().ToString() + " has incorrect value: " + MassType.ToString());
            }
        }
        public static bool IsMassInRange(double Mass1, double Mass2, double PpmError)
        {
            if (CalculateAbsRangePpmError(Mass1, Mass2) <= PpmError) { return true; }
            return false;
        }
        public static int SearchPeakIndex(double[] Masses, double PeakMass, double MaxPpmError, int StartIndex = 0)
        {
            var Index = Array.BinarySearch(Masses, StartIndex, Masses.Length - StartIndex, PeakMass);
            if (Index >= 0) { return Index; }
            else { Index = ~Index; }
            if (Index >= Masses.Length)
            {
                if (IsMassInRange(Masses[Masses.Length - 1], PeakMass, MaxPpmError) == true)
                {
                    return Masses.Length - 1;
                }
            }
            else if (Index == StartIndex)
            {
                if (StartIndex == 0)
                {
                    if (IsMassInRange(Masses[StartIndex], PeakMass, MaxPpmError) == true)
                    {
                        return Index;
                    }
                }
                else
                {
                    if (FindBetterMass(PeakMass, Masses[StartIndex - 1], Masses[StartIndex], MaxPpmError) == EMassArgument.Second)
                    {
                        return Index;
                    }
                }
            }
            else
            {
                var MassArgument = FindBetterMass(PeakMass, Masses[Index - 1], Masses[Index], MaxPpmError);
                if (MassArgument == EMassArgument.First)
                {
                    return Index - 1;
                }
                else if (MassArgument == EMassArgument.Second)
                {
                    return Index;
                }
            }
            return -1;
        }
        public static int SearchPeakIndexBasedOnErrorDistribution(Support.InputData Data, double PeakMass, int StartIndex = 0)
        {
            var InputArray = Data.Masses;
            var MaxPpmError = Data.GetErrorStdDev(PeakMass) * Data.MaxPpmErrorGain;
            return SearchPeakIndex(Data.Masses, PeakMass, MaxPpmError, StartIndex);//add StartIndex
        }
        public static int SearchNearPeakIndex(double[] InputArray, double Mass, int StartIndex = 0)
        {
            var Index = Array.BinarySearch(InputArray, StartIndex, InputArray.Length - StartIndex - 1, Mass);
            if (Index < 0)
            {
                Index = ~Index;
                if (Index >= InputArray.Length)
                {
                    return -1;
                }
            }
            return Index;
        }
        public static int SearchNearPeakIndex(double[] InputArray, double Mass)
        {
            if (Mass <= InputArray.First()) { return 0; } else if (Mass >= InputArray.Last()) { return InputArray.Length - 1; }
            var Index = Array.BinarySearch(InputArray, Mass);
            if (Index >= 0) { return Index; }
            Index = ~Index;
            if (AbsPpmError(Mass, InputArray[Index - 1]) <= AbsPpmError(Mass, InputArray[Index])) { return Index - 1; }
            return Index;
        }
    }
}

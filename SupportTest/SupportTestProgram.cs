using Support;

namespace TestSupport
{
    internal static class TestSupportProgram
    {
        private static void Main(string[] args)
        {
            //double CalculateAbsRangePpmError( double ReferenceMass, double Mass )
            var PpmError1 = CPpmError.CalculateAbsRangePpmError(100, 99);
            var PpmError2 = CPpmError.CalculateAbsRangePpmError(100, 100);
            var PpmError3 = CPpmError.CalculateAbsRangePpmError(100, 101);

            //EMassArgument FindBetterMass( double ReferenceMass, double FirstMass, double SecondMass, double PpmError )
            var MassArgument1 = CPpmError.FindBetterMass(100, 99, 101, 5000);
            var MassArgument2 = CPpmError.FindBetterMass(100, 99, 101, 15000);
            var MassArgument3 = CPpmError.FindBetterMass(100, 101, 99, 5000);
            var MassArgument4 = CPpmError.FindBetterMass(100, 101, 99, 15000);
            var MassArgument5 = CPpmError.FindBetterMass(100, 100, 99, 0.000001);

            //double CalculateMass( double Mass, double PpmError, EMassType MassType = EMassType.MaxMass )
            var MinCalculatedMass = CPpmError.CalculateMass(100, 10000, CPpmError.EMassType.MinMass);
            var BestCalculatedMass = CPpmError.CalculateMass(100, 10000, CPpmError.EMassType.BestMass);
            var MaxCalculatedMass = CPpmError.CalculateMass(100, 10000, CPpmError.EMassType.MaxMass);

            //bool IsMassInRange( double ReferenceMass, double Mass, double PpmError )
            var IsMassInRange1 = CPpmError.IsMassInRange(100, 99, 10000);
            var IsMassInRange2 = CPpmError.IsMassInRange(100, 99.5, 10000);
            var IsMassInRange3 = CPpmError.IsMassInRange(100, 100, 10000);
            var IsMassInRange4 = CPpmError.IsMassInRange(100, 101, 10000);
            var IsMassInRange5 = CPpmError.IsMassInRange(100, 102, 10000);

            var TestArrayLength = 100;
            var TestArray = new double[TestArrayLength];

            for (var Index = 0; Index < TestArray.Length; Index++)
            {
                TestArray[Index] = 50 + Index;
            }
            double PpmError = 40000;
            //at min
            var StartValue = CPpmError.CalculateMass(TestArray[0], PpmError, CPpmError.EMassType.MinMass);// = 50
            var MinIndexAtMinNo = CPpmError.SearchPeakIndex(TestArray, StartValue, PpmError, 0);
            var MinIndexAtMinYes = CPpmError.SearchPeakIndex(TestArray, StartValue + 1, PpmError, 0);
            StartValue = CPpmError.CalculateMass(TestArray[0], PpmError, CPpmError.EMassType.BestMass);
            var BestIndexAtMinNo = CPpmError.SearchPeakIndex(TestArray, StartValue - 2, PpmError, 0);
            var BestIndexAtMinYes = CPpmError.SearchPeakIndex(TestArray, StartValue - 1, PpmError, 0);
            StartValue = CPpmError.CalculateMass(TestArray[0], PpmError, CPpmError.EMassType.MaxMass);
            var MaxIndexAtMinNo = CPpmError.SearchPeakIndex(TestArray, StartValue - 3, PpmError, 0);
            var MaxIndexAtMinYes = CPpmError.SearchPeakIndex(TestArray, StartValue - 2, 40000, 0);

            //middle
            var MiddleValue = CPpmError.CalculateMass(TestArray[50], PpmError, CPpmError.EMassType.MinMass);// = 100
            var MinIndex46 = CPpmError.SearchPeakIndex(TestArray, MiddleValue, PpmError, 0);
            MiddleValue = CPpmError.CalculateMass(TestArray[50], PpmError, CPpmError.EMassType.BestMass);// = 100
            var BestIndex50 = CPpmError.SearchPeakIndex(TestArray, MiddleValue, PpmError, 0);
            var BestIndex51 = CPpmError.SearchPeakIndex(TestArray, MiddleValue + 0.5, PpmError, 0);
            MiddleValue = CPpmError.CalculateMass(TestArray[50], 4000, CPpmError.EMassType.MaxMass);// = 100
            var MaxIndex54 = CPpmError.SearchPeakIndex(TestArray, MiddleValue, 40000, 0);

            //at max
            var MaxValue = CPpmError.CalculateMass(TestArray[TestArray.Length - 1], PpmError, CPpmError.EMassType.MinMass);// = 149
            var MinIndexAtMaxYes = CPpmError.SearchPeakIndex(TestArray, MaxValue + 12, 40000, 0);
            var MinIndexAtMaxNo = CPpmError.SearchPeakIndex(TestArray, MaxValue + 13, 40000, 0);
            MaxValue = CPpmError.CalculateMass(TestArray[TestArray.Length - 1], PpmError, CPpmError.EMassType.BestMass);
            var BestIndexAtMaxYes = CPpmError.SearchPeakIndex(TestArray, MaxValue + 5, 40000, 0);
            var BestIndexAtMaxNo = CPpmError.SearchPeakIndex(TestArray, MaxValue + 6, 40000, 0);
            MaxValue = CPpmError.CalculateMass(TestArray[TestArray.Length - 1], PpmError, CPpmError.EMassType.MaxMass);
            var MaxIndexAtMaxYes = CPpmError.SearchPeakIndex(TestArray, MaxValue - 1, 40000, 0);
            var MaxIndexAtMaxNo = CPpmError.SearchPeakIndex(TestArray, MaxValue, 40000, 0);

            //StartIndex
            var IndexStartIndex50 = CPpmError.SearchPeakIndex(TestArray, MiddleValue, 40000, 20);
            var IndexStartIndexNoBefore = CPpmError.SearchPeakIndex(TestArray, MiddleValue, 40000, 60);
            var IndexStartIndexNoClose = CPpmError.SearchPeakIndex(TestArray, MiddleValue + 0.1, 10, 51);

            MaxIndexAtMaxNo++;
        }
    }

}

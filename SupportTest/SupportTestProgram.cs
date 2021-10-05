using Support;

namespace TestSupport {
    class TestSupportProgram {
        static void Main( string [] args ) {
            //double CalculateAbsRangePpmError( double ReferenceMass, double Mass ) 
            double PpmError1 = CPpmError.CalculateAbsRangePpmError( 100, 99 );
            double PpmError2 = CPpmError.CalculateAbsRangePpmError( 100, 100 );
            double PpmError3 = CPpmError.CalculateAbsRangePpmError( 100, 101 );

            //EMassArgument FindBetterMass( double ReferenceMass, double FirstMass, double SecondMass, double PpmError )
            CPpmError.EMassArgument MassArgument1 = CPpmError.FindBetterMass( 100, 99, 101, 5000 );
            CPpmError.EMassArgument MassArgument2 = CPpmError.FindBetterMass( 100, 99, 101, 15000 );
            CPpmError.EMassArgument MassArgument3 = CPpmError.FindBetterMass( 100, 101, 99, 5000 );
            CPpmError.EMassArgument MassArgument4 = CPpmError.FindBetterMass( 100, 101, 99, 15000 );
            CPpmError.EMassArgument MassArgument5 = CPpmError.FindBetterMass( 100, 100, 99, 0.000001 );

            //double CalculateMass( double Mass, double PpmError, EMassType MassType = EMassType.MaxMass ) 
            double MinCalculatedMass = CPpmError.CalculateMass( 100, 10000, CPpmError.EMassType.MinMass );
            double BestCalculatedMass = CPpmError.CalculateMass( 100, 10000, CPpmError.EMassType.BestMass );
            double MaxCalculatedMass = CPpmError.CalculateMass( 100, 10000, CPpmError.EMassType.MaxMass );

            //bool IsMassInRange( double ReferenceMass, double Mass, double PpmError )
            bool IsMassInRange1 = CPpmError.IsMassInRange( 100, 99, 10000 );
            bool IsMassInRange2 = CPpmError.IsMassInRange( 100, 99.5, 10000 );
            bool IsMassInRange3 = CPpmError.IsMassInRange( 100, 100, 10000 );
            bool IsMassInRange4 = CPpmError.IsMassInRange( 100, 101, 10000 );
            bool IsMassInRange5 = CPpmError.IsMassInRange( 100, 102, 10000 );

            int TestArrayLength = 100;
            double [] TestArray = new double [ TestArrayLength ];
            for ( int Index = 0; Index < TestArray.Length; Index++ ) {
                TestArray [ Index ] = 50 + Index;
            }
            double PpmError = 40000;
            //at min
            double StartValue = CPpmError.CalculateMass( TestArray [ 0 ], PpmError, CPpmError.EMassType.MinMass );// = 50
            int MinIndexAtMinNo = CPpmError.SearchPeakIndex( TestArray, StartValue, PpmError, 0);
            int MinIndexAtMinYes = CPpmError.SearchPeakIndex( TestArray, StartValue + 1, PpmError, 0);
            StartValue = CPpmError.CalculateMass( TestArray [ 0 ], PpmError, CPpmError.EMassType.BestMass );
            int BestIndexAtMinNo = CPpmError.SearchPeakIndex( TestArray, StartValue - 2, PpmError, 0);
            int BestIndexAtMinYes = CPpmError.SearchPeakIndex( TestArray, StartValue - 1, PpmError, 0);
            StartValue = CPpmError.CalculateMass( TestArray [ 0 ], PpmError, CPpmError.EMassType.MaxMass );
            int MaxIndexAtMinNo = CPpmError.SearchPeakIndex( TestArray, StartValue - 3, PpmError, 0);
            int MaxIndexAtMinYes = CPpmError.SearchPeakIndex( TestArray, StartValue - 2, 40000, 0);

            //middle
            double MiddleValue =  CPpmError.CalculateMass( TestArray [ 50 ], PpmError, CPpmError.EMassType.MinMass );// = 100
            int MinIndex46 = CPpmError.SearchPeakIndex( TestArray, MiddleValue, PpmError, 0);
            MiddleValue =  CPpmError.CalculateMass( TestArray [ 50 ], PpmError, CPpmError.EMassType.BestMass );// = 100
            int BestIndex50 = CPpmError.SearchPeakIndex( TestArray, MiddleValue, PpmError, 0);
            int BestIndex51 = CPpmError.SearchPeakIndex( TestArray, MiddleValue + 0.5, PpmError, 0);
            MiddleValue =  CPpmError.CalculateMass( TestArray [ 50 ], 4000, CPpmError.EMassType.MaxMass );// = 100
            int MaxIndex54 = CPpmError.SearchPeakIndex( TestArray, MiddleValue, 40000, 0);

            //at max
            double MaxValue =  CPpmError.CalculateMass( TestArray [ TestArray.Length -1 ], PpmError, CPpmError.EMassType.MinMass );// = 149
            int MinIndexAtMaxYes = CPpmError.SearchPeakIndex( TestArray, MaxValue + 12, 40000, 0);
            int MinIndexAtMaxNo = CPpmError.SearchPeakIndex( TestArray, MaxValue + 13, 40000, 0);
            MaxValue =  CPpmError.CalculateMass( TestArray [ TestArray.Length -1 ], PpmError, CPpmError.EMassType.BestMass );
            int BestIndexAtMaxYes = CPpmError.SearchPeakIndex( TestArray, MaxValue + 5, 40000, 0);
            int BestIndexAtMaxNo = CPpmError.SearchPeakIndex( TestArray, MaxValue + 6, 40000, 0);
            MaxValue =  CPpmError.CalculateMass( TestArray [ TestArray.Length -1 ], PpmError, CPpmError.EMassType.MaxMass );
            int MaxIndexAtMaxYes = CPpmError.SearchPeakIndex( TestArray, MaxValue - 1, 40000, 0);
            int MaxIndexAtMaxNo = CPpmError.SearchPeakIndex( TestArray, MaxValue, 40000, 0);

            //StartIndex
            int IndexStartIndex50 = CPpmError.SearchPeakIndex( TestArray, MiddleValue, 40000, 20 );
            int IndexStartIndexNoBefore = CPpmError.SearchPeakIndex( TestArray, MiddleValue, 40000, 60);
            int IndexStartIndexNoClose = CPpmError.SearchPeakIndex( TestArray, MiddleValue + 0.1, 10, 51 );

            MaxIndexAtMaxNo = MaxIndexAtMaxNo + 1;
        }
    }

}

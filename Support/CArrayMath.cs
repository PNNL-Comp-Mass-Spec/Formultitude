using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Support {
    public class CArrayMath{
        public static double Max( double [] Data ) {
            if ( Data == null || Data.Length <= 0 ) { throw new Exception( "Array is empty" ); }
            double Max = Data[ 0];
            foreach( double Value in Data ) {
                if ( Max < Value ) { Max = Value; }
            }
            return Max;
        }
        public static double Mean( double [] Data ) {
            if ( Data == null || Data.Length <= 0 ) { throw new Exception( "Array is empty" ); }
            double Sum = 0;
            foreach ( double Value in Data ) {
                Sum = Sum + Value;
            }
            return Sum / Data.Length;
        }
        public static double StandardDeviation( double [] Data, double Mean ) {
            if ( Data == null || Data.Length <= 0 ) { throw new Exception( "Array is empty" ); }
            double Sum = 0;
            foreach ( double Value in Data ) {
                Sum = Sum + Math.Pow(Value - Mean, 2);
            }
            return Math.Pow( Sum / ( Data.Length - 1 ), 0.5 );
        }
    }
}

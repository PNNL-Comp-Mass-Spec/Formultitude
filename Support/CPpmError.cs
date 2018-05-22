using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Support {
    public class CPpmError {
        const double PPM = 1e6;//parts per million
        public static double PpmToError( double Mass, double ErrorPPM ) { return Mass * ErrorPPM / PPM; }
        public static double ErrorToPpm( double Mass, double Error ) { return Error * PPM / Mass; }
        public static double SignedMassErrorPPM( double ReferenceMass, double Mass ) { return ( Mass - ReferenceMass ) / ReferenceMass * PPM; }
        public static double AbsMassErrorPPM( double ReferenceMass, double Mass ) { return Math.Abs( SignedMassErrorPPM( ReferenceMass, Mass)); }
        public static double LeftPpmMass( double Mass, double PpmError ) { return Mass / ( 1 + PpmError / PPM ); }
        public static double RightPpmMass( double Mass, double PpmError ) { return Mass * ( 1 + PpmError / PPM ); }
        public static int SearchIndex( double [] InputArray, double Value, double PpmError ) {
            int Index = Array.BinarySearch( InputArray, Value );
            if ( Index < 0 ) {
                Index = ~Index;
                if ( Index >= InputArray.Length ) {
                    if ( CPpmError.AbsMassErrorPPM( Value, InputArray [ InputArray.Length - 1 ] ) >  PpmError ) {
                        return -1;
                    } else {
                        Index = InputArray.Length - 1;
                    }
                } else if ( Index == 0 ) {
                    if ( CPpmError.AbsMassErrorPPM( Value, InputArray [ 0 ]) >  PpmError ) {
                        return -1;
                    }
                } else {
                    if ( ( Value - InputArray [ Index - 1 ] ) < ( InputArray [ Index ] - Value ) ) {
                        Index = Index - 1;
                        if ( CPpmError.AbsMassErrorPPM( InputArray [ Index ], Value) >  PpmError ) {
                            return -1;
                        }
                    } else {
                        if ( CPpmError.AbsMassErrorPPM( Value, InputArray [ Index ] ) >  PpmError ) {
                            return -1;
                        }
                    }
                }
            }
            return Index;
        }
        public static int SearchIndex( double [] InputArray, int StartIndex, double Value, double PpmError ) {
            int Index = Array.BinarySearch( InputArray, StartIndex, InputArray.Length - StartIndex - 1, Value );
            if ( Index < 0 ) {
                Index = ~Index;
                if ( Index >= InputArray.Length ) {
                    if ( CPpmError.AbsMassErrorPPM( InputArray [ Index], Value ) >  PpmError ) {
                        return -1;
                    } else {
                        Index = Index - 1;
                    }
                } else if ( Index == StartIndex ) {
                    if ( CPpmError.AbsMassErrorPPM( Value, InputArray [ StartIndex ] ) >  PpmError ) {
                        return -1;
                    }
                } else {
                    double LeftError = CPpmError.AbsMassErrorPPM( InputArray [ Index - 1], Value );
                    double RightError = CPpmError.AbsMassErrorPPM( Value, InputArray [ Index ] );
                    if( LeftError < RightError){
                        if( LeftError > PpmError){ return -1;}
                        Index = Index - 1;
                    } else {
                        if ( RightError > PpmError ) { return -1; }
                    }
                }
            }
            return Index;
        }
        public static int SearchNextIndex( double [] InputArray, int StartIndex, double Value) {
            int Index = Array.BinarySearch( InputArray, StartIndex, InputArray.Length - StartIndex - 1, Value );
            if ( Index < 0 ) {
                Index = ~Index;
                if ( Index >= InputArray.Length ) {
                    return -1;
                }
            }
            return Index;
        }
    }
}

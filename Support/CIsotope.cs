using System;
using System.Collections.Generic;
using System.IO;

namespace Support
{
    public class CIsotope
    {
        private static string[] Names = null;
        public static string[] GetNames() { return Names; }
        private static double[] Distances = null;
        public static double[] GetDistances() { return Distances; }

        //public static Tuple<string[],double[]> ConvertIsotopeFileIntoIsotopeDistanceFile( string IsotopeFilename) {
        public static void ConvertIsotopeFileIntoIsotopeDistanceFile(string IsotopeFilename)
        {
            /* Isotope.inf file format:
Atomic Weights and Isotopic Compositions for All ElementsAtomic Weights and
Isotopic Compositions for All Elements
Description of Quantities and Notes

name=1_H Hydrogen
Atomic Number = 1
Atomic Symbol = H
Mass Number = 1
Relative Atomic Mass = 1.0078250321(4)
Isotopic Composition = 99.9885(70)
Standard Atomic Weight = 1.00794(7)
Notes = g,m,r,c,w
Valence = 1
            */
            /* Isotope.inf.csv file format:
Mame,Distance
1_B,0.996368
             */
            var IsotopeFileLines = File.ReadAllLines(IsotopeFilename);
            var IsotopeNameList = new List<string>();
            var IsotopeDistanceList = new List<double>();
            for (var LineIndex = 0; LineIndex < IsotopeFileLines.Length; LineIndex++)
            {
                //first isotipe
                if (IsotopeFileLines[LineIndex].StartsWith("name=") == false) { continue; }
                var AtomicNumber = IsotopeFileLines[LineIndex + 1];
                //Relative Atomic Mass
                double AtomicMass = 0;
                try
                {
                    AtomicMass = double.Parse(IsotopeFileLines[LineIndex + 4].Split(new char[] { '=' })[1].Split(new char[] { '(' })[0]);
                }
                catch
                {
                    LineIndex = LineIndex + 10;
                    continue;
                }
                LineIndex = LineIndex + 10;
                for (; LineIndex < IsotopeFileLines.Length; LineIndex++)
                {
                    if (IsotopeFileLines[LineIndex].StartsWith("name=") == false) { continue; }
                    var IsotopeAtomicNumber = IsotopeFileLines[LineIndex + 1];
                    if (AtomicNumber != IsotopeAtomicNumber)
                    {
                        //new element
                        LineIndex = LineIndex - 1;
                        break;
                    }
                    var IsotopeName = IsotopeFileLines[LineIndex].Split(new char[] { '=' })[1];
                    IsotopeNameList.Add(IsotopeName);
                    //Relative Atomic Mass
                    var IsotopeAtomicMass = double.Parse(IsotopeFileLines[LineIndex + 4].Split(new char[] { '=' })[1].Split(new char[] { '(' })[0]);
                    IsotopeDistanceList.Add(IsotopeAtomicMass - AtomicMass);
                }
            }
            Names = IsotopeNameList.ToArray();
            Distances = IsotopeDistanceList.ToArray();
            Array.Sort(Distances, Names);
            /*
            //convert Isotope.inf into Isotope.inf.csv
            string [] IsotopeOutput = new string [ Distances.Length + 1 ];
            IsotopeOutput [ 0 ] = "Name,Distance";
            for ( int IsotopeIndex = 1; IsotopeIndex < Distances.Length; IsotopeIndex++ ) {
                IsotopeOutput [ IsotopeIndex ] = Names [ IsotopeIndex ] + ',' + Distances [ IsotopeIndex ].ToString( "F6" );
            }
            File.WriteAllLines( IsotopeFilename + ".csv", IsotopeOutput );
            */
            //return new Tuple<string [], double []>( Names, Distances );
        }

        public static int FindIsotopeIndex(Support.InputData Data, double Distance, double StdDev)
        {
            //public static int FindIsotopeIndex( Support.InputData Data, double [] Distances, double Distance, double StdDev ) {
            var Index = CPpmError.SearchNearPeakIndex(Distances, Distance);
            if (Index < 0) { return -1; }
            /*if ( Index > 0 ) {
                if ( Distance - Distances [ Index - 1 ] < Distances [ Index ] - Distance ) {
                    Index = Index--;
                }
            }*/
            if (Index == 0)
            {
                if (Distances[Index] <= Distance + 3 * StdDev)
                {
                    return Index;
                }
            }
            else
            {
                if ((Distances[Index] >= Distance - 3 * StdDev) && (Distances[Index] <= Distance + 3 * StdDev))
                {
                    return Index;
                }
            }
            return -1;
        }
    }
}

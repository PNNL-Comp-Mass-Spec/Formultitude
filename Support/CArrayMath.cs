using System;

namespace Support
{
    public class CArrayMath
    {
        public static double Max(double[] Data)
        {
            if (Data == null || Data.Length <= 0) { throw new Exception("Array is empty"); }
            double Max = Data[0];
            foreach (double Value in Data)
            {
                if (Max < Value) { Max = Value; }
            }
            return Max;
        }
        public static int Max(int[] Data)
        {
            if (Data == null || Data.Length <= 0) { throw new Exception("Array is empty"); }
            int Max = Data[0];
            foreach (int Value in Data)
            {
                if (Max < Value) { Max = Value; }
            }
            return Max;
        }
        public static double Mean(double[] Data)
        {
            if (Data == null || Data.Length <= 0) { throw new Exception("Array is empty"); }
            double Sum = 0;
            foreach (double Value in Data)
            {
                Sum = Sum + Value;
            }
            return Sum / Data.Length;
        }
        public static double AbsMean(double[] Data)
        {
            if (Data == null || Data.Length <= 0) { throw new Exception("Array is empty"); }
            double Sum = 0;
            foreach (double Value in Data)
            {
                Sum = Sum + Math.Abs(Value);
            }
            return Sum / Data.Length;
        }
        public static double StandardDeviation(double[] Data, double Mean)
        {
            if (Data == null || Data.Length <= 0) { throw new Exception("Array is empty"); }
            double Sum = 0;
            foreach (double Value in Data)
            {
                Sum = Sum + Math.Pow(Value - Mean, 2);
            }
            return Math.Pow(Sum / (Data.Length - 1), 0.5);
        }
        public static double AbsStandardDeviation(double[] Data, double Mean)
        {
            if (Data == null || Data.Length <= 0) { throw new Exception("Array is empty"); }
            double Sum = 0;
            foreach (double Value in Data)
            {
                Sum = Sum + Math.Pow(Math.Abs(Value) - Mean, 2);
            }
            return Math.Pow(Sum / (Data.Length - 1), 0.5);
        }
        public static double LinearValue(double X, double X1, double X2, double Y1, double Y2)
        {
            return Y1 + (Y2 - Y1) * (X - X1) / (X2 - X1);
        }
        public static void CalcParabolaVertex(double x1, double y1, double x2, double y2, double x3, double y3, out double x, out double y)
        {
            double denom = (x1 - x2) * (x1 - x3) * (x2 - x3);
            double A = (x3 * (y2 - y1) + x2 * (y1 - y3) + x1 * (y3 - y2)) / denom;
            double B = (x3 * x3 * (y1 - y2) + x2 * x2 * (y3 - y1) + x1 * x1 * (y2 - y3)) / denom;
            double C = (x2 * x3 * (x2 - x3) * y1 + x3 * x1 * (x3 - x1) * y2 + x1 * x2 * (x1 - x2) * y3) / denom;

            x = -B / (2 * A);
            y = C - B * B / (4 * A);
        }
    }
}

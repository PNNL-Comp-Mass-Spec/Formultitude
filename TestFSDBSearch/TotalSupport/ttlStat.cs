// Decompiled with JetBrains decompiler
// Type: TestFSDBSearch.TotalSupport.ttlStat
// Assembly: TestFSDBSearch, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C36EBD2C-6652-4FDC-A74D-B15E46A79224
// Assembly location: F:\Documents\Projects\NikolaTolic\Formularity\Lib\TestFSDBSearch.exe

using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Text;

namespace TestFSDBSearch.TotalSupport
{
  public class ttlStat
  {
    public double[] Data;
    private int mCount;
    private double mMin;
    private double mMax;
    private double mMean;
    private double mMedian;
    private double mVar;
    private double mStDev;
    private double mAvgDev;
    private double mSum;

    public override string ToString()
    {
      StringBuilder stringBuilder1 = new StringBuilder(string.Empty);
      string str;
      try
      {
        StringBuilder stringBuilder2 = new StringBuilder(ttlAI.AI_MergeTokens(ttlEstimator.count.ToString(), Conversions.ToString(this.mCount), "=") + "\r\n");
        stringBuilder2.AppendLine(ttlAI.AI_MergeTokens(ttlEstimator.sum.ToString(), Conversions.ToString(this.mSum), "="));
        stringBuilder2.AppendLine(ttlAI.AI_MergeTokens(ttlEstimator.min.ToString(), Conversions.ToString(this.mMin), "="));
        stringBuilder2.AppendLine(ttlAI.AI_MergeTokens(ttlEstimator.max.ToString(), Conversions.ToString(this.mMax), "="));
        stringBuilder2.AppendLine(ttlAI.AI_MergeTokens(ttlEstimator.mean.ToString(), Conversions.ToString(this.mMean), "="));
        stringBuilder2.AppendLine(ttlAI.AI_MergeTokens(ttlEstimator.median.ToString(), Conversions.ToString(this.mMedian), "="));
        stringBuilder2.AppendLine(ttlAI.AI_MergeTokens(ttlEstimator.stdev.ToString(), Conversions.ToString(this.mStDev), "="));
        stringBuilder2.AppendLine(ttlAI.AI_MergeTokens(ttlEstimator.avgdev.ToString(), Conversions.ToString(this.mAvgDev), "="));
        str = stringBuilder2.ToString();
      }
      catch (Exception ex)
      {
        ProjectData.SetProjectError(ex);
        str = "error compiling statistic - " + ex.Message;
        ProjectData.ClearProjectError();
      }
      return str;
    }

    public double EstimatorValue(ttlEstimator e)
    {
      switch (e)
      {
        case ttlEstimator.count:
          return (double) this.mCount;
        case ttlEstimator.mean:
          return this.mMean;
        case ttlEstimator.median:
          return this.mMedian;
        case ttlEstimator.variance:
          return this.mVar;
        case ttlEstimator.stdev:
          return this.mStDev;
        case ttlEstimator.avgdev:
          return this.mAvgDev;
        case ttlEstimator.min:
          return this.mMin;
        case ttlEstimator.max:
          return this.mMax;
        case ttlEstimator.sum:
          return this.mSum;
        default:
          return double.NaN;
      }
    }

    public int Count
    {
      get
      {
        return this.mCount;
      }
    }

    public double Min
    {
      get
      {
        return this.mMin;
      }
    }

    public double Max
    {
      get
      {
        return this.mMax;
      }
    }

    public double Mean
    {
      get
      {
        return this.mMean;
      }
    }

    public double Median
    {
      get
      {
        return this.mMedian;
      }
    }

    public double Var
    {
      get
      {
        return this.mVar;
      }
    }

    public double StDev
    {
      get
      {
        return this.mStDev;
      }
    }

    public double Sum
    {
      get
      {
        return this.mSum;
      }
    }

    public double Percentile(double Pct)
    {
      double num;
      try
      {
        if (!(Pct >= 0.0 & Pct <= 100.0))
          throw new ArgumentOutOfRangeException();
        int index = checked ((int) Math.Round(Math.Round(unchecked ((double) this.mCount * Pct / 100.0))));
        num = (double) index == (double) this.mCount * Pct / 100.0 ? this.Data[index] : (index >= checked (this.mCount - 1) ? this.Data[index] : (this.Data[index] + this.Data[checked (index + 1)]) / 2.0);
      }
      catch (Exception ex)
      {
        ProjectData.SetProjectError(ex);
        num = double.NegativeInfinity;
        ProjectData.ClearProjectError();
      }
      return num;
    }

    public double Quartile(int Quart)
    {
      double num;
      try
      {
        switch (Quart)
        {
          case 0:
            num = this.mMin;
            break;
          case 1:
            num = this.Percentile(25.0);
            break;
          case 2:
            num = this.mMedian;
            break;
          case 3:
            num = this.Percentile(75.0);
            break;
          case 4:
            num = this.mMax;
            break;
          default:
            num = double.NegativeInfinity;
            break;
        }
      }
      catch (Exception ex)
      {
        ProjectData.SetProjectError(ex);
        num = double.NegativeInfinity;
        ProjectData.ClearProjectError();
      }
      return num;
    }

    public bool SetData(double[] NewData, bool sort_data = true)
    {
      bool flag;
      try
      {
        if (!Information.IsNothing((object) NewData))
        {
          this.Data = new double[checked (NewData.Length - 1 + 1)];
          NewData.CopyTo((Array) this.Data, 0);
        }
        if (sort_data)
          Array.Sort<double>(this.Data);
        flag = true;
      }
      catch (Exception ex)
      {
        ProjectData.SetProjectError(ex);
        this.Data = (double[]) null;
        flag = false;
        ProjectData.ClearProjectError();
      }
      return flag;
    }

    public bool SetDataPoint(double NewDataPoint, int IncSize = 1000)
    {
      bool flag;
      try
      {
        if (this.mCount >= this.Data.Length)
          this.ManageDataArray(ttlArrayManagementType.aAdd, IncSize);
        this.Data[this.mCount] = NewDataPoint;
        checked { ++this.mCount; }
        flag = true;
      }
      catch (Exception ex)
      {
        ProjectData.SetProjectError(ex);
        flag = false;
        ProjectData.ClearProjectError();
      }
      return flag;
    }

    public bool Calculate()
    {
      bool flag;
      try
      {
        if (this.Data.Length > 0)
        {
          if (this.Data.Length > 1)
            Array.Sort<double>(this.Data);
          this.mCount = this.Data.Length;
          this.mMin = this.Data[0];
          this.mMax = this.Data[checked (this.mCount - 1)];
          int index1 = checked ((int) Math.Round(Conversion.Int(unchecked ((double) this.mCount / 2.0))));
          this.mMedian = this.mCount % 2 <= 0 ? (this.Data[checked (index1 - 1)] + this.Data[index1]) / 2.0 : this.Data[index1];
          this.mMean = 0.0;
          this.mVar = 0.0;
          this.mStDev = 0.0;
          this.mAvgDev = 0.0;
          this.mSum = 0.0;
          int num1 = 0;
          int num2 = checked (this.mCount - 1);
          int index2 = num1;
          double num3 = 0;
          while (index2 <= num2)
          {
            num3 += this.Data[index2];
            checked { ++index2; }
          }
          this.mSum = num3;
          this.mMean = num3 / (double) this.mCount;
          double num4 = 0.0;
          if (this.mCount > 1)
          {
            int num5 = 0;
            int num6 = checked (this.mCount - 1);
            int index3 = num5;
            while (index3 <= num6)
            {
              double num7 = this.Data[index3] - this.mMean;
              double num8 = num7 * num7;
              double num9 = num8 * num7 * num7;
              num4 += num7;
              this.mAvgDev += Math.Abs(num7);
              this.mVar += num8;
              checked { ++index3; }
            }
            this.mAvgDev /= (double) this.mCount;
            this.mVar = (this.mVar - num4 * num4 / (double) this.mCount) / (double) checked (this.mCount - 1);
            this.mStDev = Math.Sqrt(this.mVar);
          }
          flag = true;
        }
        else
          flag = false;
      }
      catch (Exception ex)
      {
        ProjectData.SetProjectError(ex);
        flag = false;
        ProjectData.ClearProjectError();
      }
      return flag;
    }

    public bool Frequency(double LoLimit, double HiLimit, int Resolution, double BinWidth, ref double[] BinCenter, ref int[] Count)
    {
      bool flag;
      try
      {
        if (this.mCount > 0)
        {
          if (!(Resolution > 0 & LoLimit < HiLimit))
            throw new ArgumentOutOfRangeException();
          BinWidth = (HiLimit - LoLimit) / (double) Resolution;
          BinCenter = new double[checked (Resolution + 1 + 1)];
          Count = new int[checked (Resolution + 1 + 1)];
          BinCenter[0] = LoLimit - BinWidth / 2.0;
          int num1 = 1;
          int num2 = checked (Resolution + 1);
          int index1 = num1;
          while (index1 <= num2)
          {
            BinCenter[index1] = BinCenter[checked (index1 - 1)] + BinWidth;
            checked { ++index1; }
          }
          int num3 = 0;
          int num4 = checked (this.mCount - 1);
          int index2 = num3;
          while (index2 <= num4)
          {
            int num5 = this.Data[index2] >= LoLimit ? (this.Data[index2] < HiLimit ? checked ((int) Math.Round(unchecked (Conversion.Int((this.Data[index2] - LoLimit) / BinWidth) + 1.0))) : checked (Resolution + 1)) : 0;
            int[] numArray1 = Count;
            int[] numArray2 = numArray1;
            int index3 = num5;
            int index4 = index3;
            int num6 = checked (numArray1[index3] + 1);
            numArray2[index4] = num6;
            checked { ++index2; }
          }
          flag = true;
        }
        else
          flag = false;
      }
      catch (Exception ex)
      {
        ProjectData.SetProjectError(ex);
        flag = false;
        ProjectData.ClearProjectError();
      }
      return flag;
    }

    public ttlStat(int NewDataLen)
    {
      try
      {
        if (NewDataLen <= 0)
          return;
        this.Data = new double[checked (NewDataLen - 1 + 1)];
      }
      catch (Exception ex)
      {
        ProjectData.SetProjectError(ex);
        this.Data = (double[]) null;
        ProjectData.ClearProjectError();
      }
    }

    ~ttlStat()
    {
      // ISSUE: explicit finalizer call
      // base.Finalize();
    }

    public double AverageDataSpacing(double DataWidth = -1.0)
    {
      double num1;
      try
      {
        if (this.mCount > 1)
        {
          int num2 = 0;
          int num3 = checked (this.mCount - 2);
          int index = num2;
          double num4 = 0;
          while (index <= num3)
          {
            num4 += Math.Abs(this.Data[checked (index + 1)] - this.Data[index]);
            checked { ++index; }
          }
          num1 = num4 / (double) checked (this.mCount - 1);
        }
        else
          num1 = DataWidth <= 0.0 ? this.mMax - this.mMin : DataWidth;
      }
      catch (Exception ex)
      {
        ProjectData.SetProjectError(ex);
        num1 = double.NaN;
        ProjectData.ClearProjectError();
      }
      return num1;
    }

    public bool ManageDataArray(ttlArrayManagementType mng_type, int sz = 1000)
    {
      bool flag;
      try
      {
        switch (mng_type)
        {
          case ttlArrayManagementType.aErase:
            this.mCount = 0;
            this.Data = (double[]) null;
            break;
          case ttlArrayManagementType.aInitialize:
            this.mCount = 0;
            this.Data = new double[checked (sz + 1)];
            break;
          case ttlArrayManagementType.aAdd:
            this.Data = (double[]) Utils.CopyArray((Array) this.Data, (Array) new double[checked (this.mCount + sz + 1)]);
            break;
          case ttlArrayManagementType.aTrim:
            this.Data = this.mCount <= 0 ? (double[]) null : (double[]) Utils.CopyArray((Array) this.Data, (Array) new double[checked (this.mCount - 1 + 1)]);
            break;
        }
        flag = true;
      }
      catch (Exception ex)
      {
        ProjectData.SetProjectError(ex);
        flag = false;
        ProjectData.ClearProjectError();
      }
      return flag;
    }
  }
}

// Decompiled with JetBrains decompiler
// Type: TestFSDBSearch.TotalCalibration
// Assembly: TestFSDBSearch, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C36EBD2C-6652-4FDC-A74D-B15E46A79224
// Assembly location: F:\Documents\Projects\NikolaTolic\Formultitude\Lib\TestFSDBSearch.exe

using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.IO;
using System.Text;
using TestFSDBSearch.TotalSupport;

namespace TestFSDBSearch
{
  public class TotalCalibration
  {
    private const double MIN_CAL_RANGE = 1.0;
    private const string FAILURE = "failure";
    private const string L_prefix = "LIntCal_";
    private const string Q_prefix = "QIntCal_";
    public double ttl_cal_start_ppm;
    public double ttl_cal_target_ppm;
    public double ttl_cal_min_sn;
    public double ttl_cal_min_abu_pct;
    public double ttl_cal_max_abu_pct;
    public TotalCalibration.ttlRegressionType ttl_cal_regression;
    public double ttl_cal_rf;
    public StringBuilder cal_log;
    private int m_ttl_CalPeaksCnt;
    private string m_ttl_FileName;
    private double m_ttl_mMin;
    private double m_ttl_mMax;
    private double m_ttl_dmMin;
    private double m_ttl_dmAvg;
    private int m_ttl_IdenticalPairs;
    private double m_ttl_CalcMinAbsAbu;
    public TotalCalibration.ttlCalSetScore CalScore;
    public TotalCalibration.ttlCalibrationPeak[] CalPeaks;
    public string LastErrMsg;
    private string[] deli;
    private double[] data_m;
    private double[] data_a;
    private double[] data_sn;
    private double[] data_ra;
    private int data_cnt;
    private double data_max_a;
    private double[] mL;
    private double[] mQ;

    public TotalCalibration()
    {
      this.ttl_cal_start_ppm = 5.0;
      this.ttl_cal_target_ppm = 1.0;
      this.ttl_cal_min_sn = 10.0;
      this.ttl_cal_min_abu_pct = 0.01;
      this.ttl_cal_max_abu_pct = 0.99;
      this.ttl_cal_regression = TotalCalibration.ttlRegressionType.linear;
      this.ttl_cal_rf = 1000000.0;
      this.cal_log = new StringBuilder(string.Empty);
      this.m_ttl_FileName = string.Empty;
      this.m_ttl_mMin = double.MaxValue;
      this.m_ttl_mMax = double.MinValue;
      this.m_ttl_dmMin = double.MaxValue;
      this.m_ttl_dmAvg = double.MinValue;
      this.m_ttl_IdenticalPairs = 0;
      this.m_ttl_CalcMinAbsAbu = 0.0;
      this.CalPeaks = new TotalCalibration.ttlCalibrationPeak[0];
      this.LastErrMsg = string.Empty;
      this.deli = new string[3]{ "\t", " ", "," };
      this.mL = new double[0];
      this.mQ = new double[0];
    }

    public static double RelativeError_PPM(double measured_val, double theoretical_val)
    {
      if (measured_val == theoretical_val)
        return 0.0;
      if (Math.Abs(theoretical_val) > 0.0)
        return (measured_val - theoretical_val) / theoretical_val * 1000000.0;
      return measured_val > 0.0 ? double.PositiveInfinity : double.NegativeInfinity;
    }

    public static int DistinctCount(ref Array a)
    {
      int num1 = 0;
      int num2;
      try
      {
        if (!Information.IsNothing((object) a) && a.Length > 0)
        {
          Array.Sort(a);
          num1 = 1;
          int num3 = 1;
          int num4 = checked (a.Length - 1);
          int num5 = num3;
          while (num5 <= num4)
          {
            if (Operators.ConditionalCompareObjectNotEqual(NewLateBinding.LateIndexGet((object) a, new object[1]
            {
              (object) num5
            }, (string[]) null), NewLateBinding.LateIndexGet((object) a, new object[1]
            {
              (object) checked (num5 - 1)
            }, (string[]) null), false))
              checked { ++num1; }
            checked { ++num5; }
          }
        }
        num2 = num1;
      }
      catch (Exception ex)
      {
        ProjectData.SetProjectError(ex);
        num2 = -1;
        ProjectData.ClearProjectError();
      }
      return num2;
    }

    public string CalibrationSetDescription()
    {
label_1:
      int num1 = 0;
      string str1;
      int num2 = 0;
      StringBuilder stringBuilder = new StringBuilder("calibration_set=" + this.m_ttl_FileName + "\r\n");
      try
            {
        ProjectData.ClearProjectError();
        num1 = -2;
label_2:
        int num3 = 2;
label_3:
        num3 = 3;
        stringBuilder.AppendLine("calibration_set_count=" + Conversions.ToString(this.CalPeaks.Length));
label_4:
        num3 = 4;
        stringBuilder.AppendLine("calibration_set_peak_min=" + Conversions.ToString(this.m_ttl_mMin));
label_5:
        num3 = 5;
        stringBuilder.AppendLine("calibration_set_peak_max=" + Conversions.ToString(this.m_ttl_mMax));
label_6:
        num3 = 6;
        stringBuilder.AppendLine("calibration_set_peak_avg_dm=" + Conversions.ToString(this.m_ttl_dmAvg));
label_7:
        num3 = 7;
        stringBuilder.AppendLine("calibration_set_peak_min_dm=" + Conversions.ToString(this.m_ttl_dmMin));
label_8:
        num3 = 8;
        str1 = stringBuilder.ToString();
        goto label_15;
label_10:
        num2 = num3;
        switch (num1 > -2 ? num1 : 1)
        {
          case 1:
            int num4 = num2 + 1;
            num2 = 0;
            switch (num4)
            {
              case 1:
                goto label_1;
              case 2:
                goto label_2;
              case 3:
                goto label_3;
              case 4:
                goto label_4;
              case 5:
                goto label_5;
              case 6:
                goto label_6;
              case 7:
                goto label_7;
              case 8:
                goto label_8;
              case 9:
                goto label_15;
            }
        }
      }
      catch (Exception ex) when (ex is Exception & (uint) num1 > 0U & num2 == 0)
      {
        ProjectData.SetProjectError(ex);
          return stringBuilder.ToString();
      }
      throw ProjectData.CreateProjectError(-2146828237);
label_15:
      string str2 = str1;
      if (num2 == 0)
        return str2;
      ProjectData.ClearProjectError();
      return str2;
    }

    public int Load(string FileName)
    {
      string[] strArray1 = new string[0];
      int num;
      try
      {
        this.cal_log = new StringBuilder("calibration log start = " + Conversions.ToString(DateTime.Now) + "\r\n");
        if (this.ManageCalPeaksArray(ttlArrayManagementType.aInitialize, 2000))
        {
          TextReader textReader = (TextReader) new StreamReader(FileName);
          textReader.ReadLine().Trim();
          while (textReader.Peek() != -1)
          {
            string[] strArray2 = textReader.ReadLine().Trim().Split(this.deli, StringSplitOptions.RemoveEmptyEntries);
            if (strArray2.Length >= 2)
            {
              this.CalPeaks[this.m_ttl_CalPeaksCnt] = new TotalCalibration.ttlCalibrationPeak();
              this.CalPeaks[this.m_ttl_CalPeaksCnt].a = 0.0;
              this.CalPeaks[this.m_ttl_CalPeaksCnt].m = double.Parse(strArray2[1]);
              this.CalPeaks[this.m_ttl_CalPeaksCnt].source = FileName;
              this.CalPeaks[this.m_ttl_CalPeaksCnt].order = 1;
              this.CalPeaks[this.m_ttl_CalPeaksCnt].name = strArray2[0];
              if (this.CalPeaks[this.m_ttl_CalPeaksCnt].m < this.m_ttl_mMin)
                this.m_ttl_mMin = this.CalPeaks[this.m_ttl_CalPeaksCnt].m;
              if (this.CalPeaks[this.m_ttl_CalPeaksCnt].m > this.m_ttl_mMax)
                this.m_ttl_mMax = this.CalPeaks[this.m_ttl_CalPeaksCnt].m;
              checked { ++this.m_ttl_CalPeaksCnt; }
            }
            if (this.m_ttl_CalPeaksCnt >= this.CalPeaks.Length)
              this.ManageCalPeaksArray(ttlArrayManagementType.aAdd, 2000);
          }
          this.ManageCalPeaksArray(ttlArrayManagementType.aTrim, 2000);
          this.m_ttl_FileName = FileName;
        }
        num = !this.CalPeaks_Diagnostics() ? -1 : this.m_ttl_CalPeaksCnt;
      }
      catch (Exception ex)
      {
        ProjectData.SetProjectError(ex);
        this.cal_log.AppendLine(ex.Message);
        num = -1;
        ProjectData.ClearProjectError();
      }
      finally
      {
      }
      return num;
    }

    private bool ManageCalPeaksArray(ttlArrayManagementType mng_type, int sz = 2000)
    {
      bool flag;
      try
      {
        switch (mng_type)
        {
          case ttlArrayManagementType.aErase:
            this.m_ttl_CalPeaksCnt = 0;
            this.CalPeaks = (TotalCalibration.ttlCalibrationPeak[]) null;
            break;
          case ttlArrayManagementType.aInitialize:
            this.m_ttl_CalPeaksCnt = 0;
            this.CalPeaks = new TotalCalibration.ttlCalibrationPeak[checked (sz + 1)];
            this.m_ttl_mMin = double.MaxValue;
            this.m_ttl_mMax = double.MinValue;
            break;
          case ttlArrayManagementType.aAdd:
            this.CalPeaks = (TotalCalibration.ttlCalibrationPeak[]) Utils.CopyArray((Array) this.CalPeaks, (Array) new TotalCalibration.ttlCalibrationPeak[checked (this.m_ttl_CalPeaksCnt + sz + 1)]);
            break;
          case ttlArrayManagementType.aTrim:
            this.CalPeaks = this.m_ttl_CalPeaksCnt <= 0 ? (TotalCalibration.ttlCalibrationPeak[]) null : (TotalCalibration.ttlCalibrationPeak[]) Utils.CopyArray((Array) this.CalPeaks, (Array) new TotalCalibration.ttlCalibrationPeak[checked (this.m_ttl_CalPeaksCnt - 1 + 1)]);
            break;
        }
        flag = true;
      }
      catch (Exception ex)
      {
        ProjectData.SetProjectError(ex);
        this.cal_log.AppendLine(ex.Message);
        flag = false;
        ProjectData.ClearProjectError();
      }
      return flag;
    }

    public bool CalPeaks_Diagnostics()
    {
      bool flag;
      try
      {
        this.m_ttl_mMin = 0.0;
        this.m_ttl_mMax = 0.0;
        this.m_ttl_dmMin = double.MaxValue;
        this.m_ttl_dmAvg = 0.0;
        if (this.CalPeaks.Length > 2)
        {
          Array.Sort<TotalCalibration.ttlCalibrationPeak>(this.CalPeaks);
          this.m_ttl_mMin = this.CalPeaks[0].m;
          this.m_ttl_mMax = this.CalPeaks[checked (this.m_ttl_CalPeaksCnt - 1)].m;
          int num1 = 1;
          int num2 = checked (this.m_ttl_CalPeaksCnt - 1);
          int index = num1;
          double num3 = 0;
          while (index <= num2)
          {
            double num4 = Math.Abs(this.CalPeaks[index].m - this.CalPeaks[checked (index - 1)].m);
            if (num4 < this.m_ttl_dmMin)
              this.m_ttl_dmMin = num4;
            num3 += num4;
            checked { ++index; }
          }
          this.m_ttl_dmAvg = num3 / (double) checked (this.m_ttl_CalPeaksCnt - 1);
          flag = true;
        }
        else
          flag = false;
      }
      catch (Exception ex)
      {
        ProjectData.SetProjectError(ex);
        this.cal_log.AppendLine(ex.Message);
        flag = false;
        ProjectData.ClearProjectError();
      }
      finally
      {
        this.cal_log.AppendLine(this.CalibrationSetDescription());
      }
      return flag;
    }

    public double[] ttl_LQ_InternalCalibration(ref double[] m, ref double[] a, ref double[] sn, double max_abu)
    {
      double[] numArray;
      try
      {
        this.data_m = m;
        this.data_a = a;
        this.data_sn = sn;
        this.data_cnt = this.data_m.Length;
        this.data_max_a = max_abu;
        this.data_ra = new double[checked (this.data_cnt - 1 + 1)];
        int num1 = 0;
        int num2 = checked (this.data_cnt - 1);
        int index = num1;
        while (index <= num2)
        {
          this.data_ra[index] = this.data_a[index] / this.data_max_a;
          checked { ++index; }
        }
        this.LQInternalCalibration();
        switch (this.ttl_cal_regression)
        {
          case TotalCalibration.ttlRegressionType.linear:
            numArray = this.mL;
            break;
          case TotalCalibration.ttlRegressionType.quadratic:
            numArray = this.mQ;
            break;
          default:
            numArray = (double[]) null;
            break;
        }
      }
      catch (Exception ex)
      {
        ProjectData.SetProjectError(ex);
        numArray = (double[]) null;
        ProjectData.ClearProjectError();
      }
      return numArray;
    }

    private string LQInternalCalibration()
    {
      StringBuilder stringBuilder = new StringBuilder(string.Empty);
      string str;
      try
      {
        this.mL = (double[]) null;
        this.mQ = (double[]) null;
        this.CalScore = new TotalCalibration.ttlCalSetScore();
        this.CalScore.Tol = this.ttl_cal_target_ppm / 2.0;
        if (this.m_ttl_CalPeaksCnt > 0)
        {
          stringBuilder.AppendLine(this.ttlMatchCalibrants());
          if (this.CalScore.HitCount >= 2)
          {
            if (this.CalScore.MaxHitsE - this.CalScore.MinHitsE < 1.0)
            {
              str = "calibration range too narrow!";
              goto label_21;
            }
            else
            {
              stringBuilder.AppendLine("match_min_m=" + Conversions.ToString(this.CalScore.MinHitsE));
              stringBuilder.AppendLine("match_max_m=" + Conversions.ToString(this.CalScore.MaxHitsE));
              stringBuilder.AppendLine("match_avg_err=" + Conversions.ToString(this.CalScore.AvgErr));
              stringBuilder.AppendLine("match_med_err=" + Conversions.ToString(this.CalScore.MedErr));
              stringBuilder.AppendLine("match_std_err=" + Conversions.ToString(this.CalScore.StDErr));
              TotalCalibration.ttlRegressionX ttlRegressionX1 = new TotalCalibration.ttlRegressionX(this.CalScore.HitCount);
              ttlRegressionX1.Type = TotalCalibration.ttlRegressionType.linear;
              ttlRegressionX1.Tolerance = this.ttl_cal_target_ppm;
              TotalCalibration.ttlRegressionX ttlRegressionX2 = new TotalCalibration.ttlRegressionX(this.CalScore.HitCount);
              ttlRegressionX2.Type = TotalCalibration.ttlRegressionType.quadratic;
              ttlRegressionX2.Tolerance = this.ttl_cal_target_ppm;
              int num1 = 0;
              int num2 = checked (this.CalScore.HitCount - 1);
              int index = num1;
              while (index <= num2)
              {
                ttlRegressionX1.x[index] = this.CalScore.Hits[index].DataE;
                ttlRegressionX1.y[index] = this.CalScore.Hits[index].DataT;
                ttlRegressionX1.id[index] = this.CalScore.Hits[index].DataDesc;
                ttlRegressionX1.ind[index] = this.CalScore.Hits[index].DataE_Ind;
                ttlRegressionX2.x[index] = this.CalScore.Hits[index].DataE;
                ttlRegressionX2.y[index] = this.CalScore.Hits[index].DataT;
                ttlRegressionX2.id[index] = this.CalScore.Hits[index].DataDesc;
                ttlRegressionX2.ind[index] = this.CalScore.Hits[index].DataE_Ind;
                checked { ++index; }
              }
              stringBuilder.AppendLine("linear_calibration=" + ttlRegressionX1.ProcessX());
              if (ttlRegressionX1.Success)
              {
                stringBuilder.AppendLine("linear_calibration=" + ttlRegressionX1.ToString());
                stringBuilder.AppendLine("linear_calibration_R^2=" + Conversions.ToString(ttlRegressionX1.R2));
                stringBuilder.Append(ttlRegressionX1.DetailsReports());
                this.mL = ttlRegressionX1.Recalculate(this.data_m);
              }
              else
                stringBuilder.AppendLine("linear_calibration=failure");
              if (this.CalScore.HitCount > 2)
              {
                stringBuilder.AppendLine("quadratic_calibration=" + ttlRegressionX2.ProcessX());
                if (ttlRegressionX2.Success)
                {
                  stringBuilder.AppendLine("quadratic_calibration=" + ttlRegressionX2.ToString());
                  stringBuilder.AppendLine("quadratic_calibration_R^2=" + Conversions.ToString(ttlRegressionX2.R2));
                  stringBuilder.Append(ttlRegressionX2.DetailsReports());
                  this.mQ = ttlRegressionX2.Recalculate(this.data_m);
                }
                else
                  stringBuilder.AppendLine("quadratic_calibration=failure");
              }
            }
          }
        }
        str = "could not calibrate peaks!";
      }
      catch (Exception ex)
      {
        ProjectData.SetProjectError(ex);
        str = "error calibrating peaks - " + ex.Message;
        ProjectData.ClearProjectError();
      }
      finally
      {
        this.cal_log.AppendLine(stringBuilder.ToString());
      }
label_21:
      return str;
    }

    public string ttlMatchCalibrants()
    {
      StringBuilder stringBuilder = new StringBuilder(string.Empty);
      string str;
      try
      {
        this.CalScore = new TotalCalibration.ttlCalSetScore();
        this.CalScore.Tol = this.ttl_cal_target_ppm / 2.0;
        if (this.m_ttl_CalPeaksCnt > 0 & this.data_cnt > 0)
        {
          this.CalScore.ManageHitsArray(ttlArrayManagementType.aInitialize, checked (2 * this.data_cnt));
          int num1 = 0;
          int num2 = checked (this.m_ttl_CalPeaksCnt - 1);
          int index1 = num1;
          while (index1 <= num2)
          {
            double m = this.CalPeaks[index1].m;
            int num3 = 0;
            int num4 = checked (this.data_cnt - 1);
            int index2 = num3;
            while (index2 <= num4)
            {
              if (this.data_sn[index2] >= this.ttl_cal_min_sn && this.data_ra[index2] >= this.ttl_cal_min_abu_pct & this.data_ra[index2] <= this.ttl_cal_max_abu_pct)
              {
                double num5 = TotalCalibration.RelativeError_PPM(this.data_m[index2], this.CalPeaks[index1].m);
                if (Math.Abs(num5) <= this.ttl_cal_start_ppm)
                {
                  TotalCalibration.ttlCalSetScore calScore = this.CalScore;
                  calScore.Hits[calScore.HitCount] = new TotalCalibration.ttlNumericMatch();
                  calScore.Hits[calScore.HitCount].DataT = m;
                  calScore.Hits[calScore.HitCount].DataT_Ind = index1;
                  calScore.Hits[calScore.HitCount].DataE = this.data_m[index2];
                  calScore.Hits[calScore.HitCount].DataE_Ind = index2;
                  calScore.Hits[calScore.HitCount].DataW = this.data_a[index2];
                  calScore.Hits[calScore.HitCount].DataDesc = this.CalPeaks[index1].name;
                  calScore.Hits[calScore.HitCount].Err = num5;
                  checked { ++calScore.HitCount; }
                }
              }
              checked { ++index2; }
            }
            checked { ++index1; }
          }
          this.CalScore.ManageHitsArray(ttlArrayManagementType.aTrim, 1000);
          str = "calibrant peaks total " + Conversions.ToString(this.m_ttl_CalPeaksCnt) + " matched " + Conversions.ToString(this.CalScore.HitCount) + "(" + Conversions.ToString(this.CalScore.HitCountUnq) + " distinct)";
        }
        else
          str = "is calibration set loaded? is data set loaded?";
      }
      catch (Exception ex)
      {
        ProjectData.SetProjectError(ex);
        str = "error matching calibrant peaks - " + ex.Message;
        ProjectData.ClearProjectError();
      }
      return str;
    }

    public enum ttlRegressionType
    {
      none = -1,
      auto = 0,
      linear = 1,
      quadratic = 2,
    }

    public class ttlRegression
    {
      private double m_a;
      private double m_b;
      private double m_c;
      private double x_mean;
      private double y_mean;
      private double m_r2;
      private double m_rmsd;
      private double m_cvrmsd;
      private int m_Count;
      private TotalCalibration.ttlRegressionType m_Type;
      private double m_RF;
      private bool m_Success;
      public string[] id;
      public int[] ind;
      public double[] x;
      public double[] y;
      public double[] yy;
      public double[] ycalc;
      private Decimal[] xp;
      private Decimal sx;
      private Decimal sy;
      private Decimal sx2;
      private Decimal sx3;
      private Decimal sx4;
      private Decimal sxy;
      private Decimal sx2y;
      public string LastErrMsg;

      public ttlRegression(int NewCount)
      {
        this.m_Count = 0;
        this.m_Type = TotalCalibration.ttlRegressionType.none;
        this.m_RF = 1000000.0;
        this.m_Success = false;
        this.id = new string[0];
        this.ind = new int[0];
        this.x = new double[0];
        this.y = new double[0];
        this.yy = new double[0];
        this.ycalc = new double[0];
        this.xp = new Decimal[5];
        this.LastErrMsg = string.Empty;
        if (NewCount <= 0)
          throw new ArgumentException();
        this.m_Count = NewCount;
        this.x = new double[checked (this.m_Count - 1 + 1)];
        this.y = new double[checked (this.m_Count - 1 + 1)];
        this.yy = new double[checked (this.m_Count - 1 + 1)];
        this.id = new string[checked (this.m_Count - 1 + 1)];
        this.ind = new int[checked (this.m_Count - 1 + 1)];
        this.ycalc = new double[checked (this.m_Count - 1 + 1)];
      }

      public int Count
      {
        get
        {
          return this.m_Count;
        }
        set
        {
          if (value <= 0)
            throw new ArgumentException();
          this.m_Count = value;
          this.x = (double[]) Utils.CopyArray((Array) this.x, (Array) new double[checked (this.m_Count - 1 + 1)]);
          this.y = (double[]) Utils.CopyArray((Array) this.y, (Array) new double[checked (this.m_Count - 1 + 1)]);
          this.id = (string[]) Utils.CopyArray((Array) this.id, (Array) new string[checked (this.m_Count - 1 + 1)]);
          this.ycalc = (double[]) Utils.CopyArray((Array) this.ycalc, (Array) new double[checked (this.m_Count - 1 + 1)]);
        }
      }

      public TotalCalibration.ttlRegressionType Type
      {
        get
        {
          return this.m_Type;
        }
        set
        {
          this.m_Type = value;
        }
      }

      public double RF
      {
        get
        {
          return this.m_RF;
        }
        set
        {
          if (value < 1.0)
            throw new ArgumentException("relative factor must be >= 1");
          this.m_RF = value;
        }
      }

      public double A
      {
        get
        {
          return this.m_a;
        }
      }

      public double B
      {
        get
        {
          return this.m_b;
        }
      }

      public double C
      {
        get
        {
          return this.m_c;
        }
      }

      public double R2
      {
        get
        {
          return this.m_r2;
        }
      }

      public double RMSD
      {
        get
        {
          return this.m_rmsd;
        }
      }

      public double CV_RMSD
      {
        get
        {
          return this.m_cvrmsd;
        }
      }

      public bool Success
      {
        get
        {
          return this.m_Success;
        }
        set
        {
          this.m_Success = value;
        }
      }

      public override string ToString()
      {
        StringBuilder stringBuilder = new StringBuilder("y = ");
        string str;
        try
        {
          switch (this.m_Type)
          {
            case TotalCalibration.ttlRegressionType.linear:
              stringBuilder.Append(Conversions.ToString(this.m_a) + " + " + Conversions.ToString(this.m_b) + " * x");
              break;
            case TotalCalibration.ttlRegressionType.quadratic:
              stringBuilder.Append(Conversions.ToString(this.m_a) + " + " + Conversions.ToString(this.m_b) + " * x + " + Conversions.ToString(this.m_c) + " * x^2");
              break;
            default:
              str = (string) null;
              goto label_9;
          }
          if (this.m_RF > 1.0)
            stringBuilder.Append(" (relative error calibration - " + (1.0 / this.m_RF).ToString("0.0E0") + ")");
          str = stringBuilder.ToString();
        }
        catch (Exception ex)
        {
          ProjectData.SetProjectError(ex);
          str = ex.Message;
          ProjectData.ClearProjectError();
        }
label_9:
        return str;
      }

      public double yCalculated(double data_x)
      {
        double num = 0;
        switch (this.m_Type)
        {
          case TotalCalibration.ttlRegressionType.linear:
            num = this.m_a + this.m_b * data_x;
            break;
          case TotalCalibration.ttlRegressionType.quadratic:
            num = this.m_a + this.m_b * data_x + this.m_c * data_x * data_x;
            break;
        }
        if (this.m_RF > 1.0)
          return data_x - data_x * num / this.m_RF;
        return num;
      }

      public bool RegressionCalculate()
      {
        bool success;
        try
        {
          this.m_Success = false;
          if (this.m_Count > 0 && this.PrepareCalculation())
          {
            switch (this.m_Type)
            {
              case TotalCalibration.ttlRegressionType.linear:
                if (this.m_Count < 2)
                  throw new ArgumentException();
                this.m_Success = this.Calculate_Linear();
                break;
              case TotalCalibration.ttlRegressionType.quadratic:
                if (this.m_Count < 3)
                  throw new ArgumentException();
                this.m_Success = this.Calculate_Quadratic();
                break;
            }
          }
          success = this.m_Success;
        }
        catch (Exception ex)
        {
          ProjectData.SetProjectError(ex);
          this.LastErrMsg = ex.Message;
          success = this.m_Success;
          ProjectData.ClearProjectError();
        }
        return success;
      }

      public double[] Recalculate(double[] x)
      {
        double[] numArray1;
        try
        {
          double[] numArray2 = new double[checked (x.Length - 1 + 1)];
          int num1 = 0;
          int num2 = checked (x.Length - 1);
          int index = num1;
          while (index <= num2)
          {
            numArray2[index] = this.yCalculated(x[index]);
            checked { ++index; }
          }
          numArray1 = numArray2;
        }
        catch (Exception ex)
        {
          ProjectData.SetProjectError(ex);
          this.LastErrMsg = ex.Message;
          numArray1 = (double[]) null;
          ProjectData.ClearProjectError();
        }
        return numArray1;
      }

      public string DetailsReports()
      {
        StringBuilder stringBuilder1 = new StringBuilder(string.Empty);
        string str;
        try
        {
          StringBuilder stringBuilder2 = new StringBuilder("id\texp_val\tthe_val\terr_ppm_exp\tres_val\terr_ppm_cal\r\n");
          int num1 = 0;
          int num2 = checked (this.m_Count - 1);
          int index = num1;
          while (index <= num2)
          {
            stringBuilder2.AppendLine(this.id[index] + "\t" + this.x[index].ToString("0.0000000") + "\t" + this.y[index].ToString("0.0000000") + "\t" + TotalCalibration.RelativeError_PPM(this.x[index], this.y[index]).ToString("0.000") + "\t" + this.ycalc[index].ToString("0.0000000") + "\t" + TotalCalibration.RelativeError_PPM(this.ycalc[index], this.y[index]).ToString("0.000"));
            checked { ++index; }
          }
          str = stringBuilder2.ToString();
        }
        catch (Exception ex)
        {
          ProjectData.SetProjectError(ex);
          str = "error compailing report - " + ex.Message;
          ProjectData.ClearProjectError();
        }
        return str;
      }

      private bool PrepareCalculation()
      {
        bool flag;
        try
        {
          this.sx = new Decimal();
          this.sy = new Decimal();
          this.sx2 = new Decimal();
          this.sx3 = new Decimal();
          this.sx4 = new Decimal();
          this.sxy = new Decimal();
          this.sx2y = new Decimal();
          this.yy = new double[checked (this.m_Count - 1 + 1)];
          if (this.m_RF > 1.0)
          {
            int num1 = 0;
            int num2 = checked (this.m_Count - 1);
            int index = num1;
            while (index <= num2)
            {
              this.yy[index] = (this.x[index] - this.y[index]) / this.y[index] * this.m_RF;
              this.sx = new Decimal(Convert.ToDouble(this.sx) + this.x[index]);
              this.sy = new Decimal(Convert.ToDouble(this.sy) + this.yy[index]);
              checked { ++index; }
            }
          }
          else
          {
            int num1 = 0;
            int num2 = checked (this.m_Count - 1);
            int index = num1;
            while (index <= num2)
            {
              this.yy[index] = this.y[index];
              this.sx = new Decimal(Convert.ToDouble(this.sx) + this.x[index]);
              this.sy = new Decimal(Convert.ToDouble(this.sy) + this.yy[index]);
              checked { ++index; }
            }
          }
          this.x_mean = Convert.ToDouble(Decimal.Divide(this.sx, new Decimal(this.m_Count)));
          this.y_mean = Convert.ToDouble(Decimal.Divide(this.sy, new Decimal(this.m_Count)));
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

      private bool Calculate_Linear()
      {
        bool flag;
        try
        {
          int num1 = 0;
          int num2 = checked (this.m_Count - 1);
          int index = num1;
          while (index <= num2)
          {
            this.sx2 = new Decimal(Convert.ToDouble(this.sx2) + (this.x[index] - this.x_mean) * (this.x[index] - this.x_mean));
            this.sxy = new Decimal(Convert.ToDouble(this.sxy) + (this.x[index] - this.x_mean) * (this.yy[index] - this.y_mean));
            checked { ++index; }
          }
          this.m_b = Convert.ToDouble(Decimal.Divide(this.sxy, this.sx2));
          this.m_a = this.y_mean - this.m_b * this.x_mean;
          if (this.Fill_ycalc())
          {
            this.CalculateR2();
            flag = true;
          }
          else
            flag = false;
        }
        catch (Exception ex)
        {
          ProjectData.SetProjectError(ex);
          this.LastErrMsg = ex.Message;
          flag = false;
          ProjectData.ClearProjectError();
        }
        return flag;
      }

      private bool Calculate_Quadratic()
      {
        bool flag;
        try
        {
          this.xp[0] = Decimal.One;
          int num1 = 0;
          int num2 = checked (this.m_Count - 1);
          int i = num1;
          while (i <= num2)
          {
            this.Fill_xp(i);
            this.sx2 = Decimal.Add(this.sx2, this.xp[2]);
            this.sx3 = Decimal.Add(this.sx3, this.xp[3]);
            this.sx4 = Decimal.Add(this.sx4, this.xp[4]);
            this.sxy = new Decimal(Convert.ToDouble(this.sxy) + Convert.ToDouble(this.xp[1]) * this.yy[i]);
            this.sx2y = new Decimal(Convert.ToDouble(this.sx2y) + Convert.ToDouble(this.xp[2]) * this.yy[i]);
            checked { ++i; }
          }
          Decimal d2 = Decimal.Add(Decimal.Subtract(Decimal.Multiply(this.sx4, Decimal.Subtract(Decimal.Multiply(this.sx2, new Decimal(this.m_Count)), Decimal.Multiply(this.sx, this.sx))), Decimal.Multiply(this.sx3, Decimal.Subtract(Decimal.Multiply(this.sx3, new Decimal(this.m_Count)), Decimal.Multiply(this.sx, this.sx2)))), Decimal.Multiply(this.sx2, Decimal.Subtract(Decimal.Multiply(this.sx3, this.sx), Decimal.Multiply(this.sx2, this.sx2))));
          Decimal num3 = Decimal.Divide(Decimal.Add(Decimal.Subtract(Decimal.Multiply(this.sx2y, Decimal.Subtract(Decimal.Multiply(this.sx2, new Decimal(this.m_Count)), Decimal.Multiply(this.sx, this.sx))), Decimal.Multiply(this.sxy, Decimal.Subtract(Decimal.Multiply(this.sx3, new Decimal(this.m_Count)), Decimal.Multiply(this.sx, this.sx2)))), Decimal.Multiply(this.sy, Decimal.Subtract(Decimal.Multiply(this.sx3, this.sx), Decimal.Multiply(this.sx2, this.sx2)))), d2);
          Decimal num4 = Decimal.Divide(Decimal.Add(Decimal.Subtract(Decimal.Multiply(this.sx4, Decimal.Subtract(Decimal.Multiply(this.sxy, new Decimal(this.m_Count)), Decimal.Multiply(this.sy, this.sx))), Decimal.Multiply(this.sx3, Decimal.Subtract(Decimal.Multiply(this.sx2y, new Decimal(this.m_Count)), Decimal.Multiply(this.sy, this.sx2)))), Decimal.Multiply(this.sx2, Decimal.Subtract(Decimal.Multiply(this.sx2y, this.sx), Decimal.Multiply(this.sxy, this.sx2)))), d2);
          this.m_a = Convert.ToDouble(Decimal.Divide(Decimal.Add(Decimal.Subtract(Decimal.Multiply(this.sx4, Decimal.Subtract(Decimal.Multiply(this.sx2, this.sy), Decimal.Multiply(this.sx, this.sxy))), Decimal.Multiply(this.sx3, Decimal.Subtract(Decimal.Multiply(this.sx3, this.sy), Decimal.Multiply(this.sx, this.sx2y)))), Decimal.Multiply(this.sx2, Decimal.Subtract(Decimal.Multiply(this.sx3, this.sxy), Decimal.Multiply(this.sx2, this.sx2y)))), d2));
          this.m_b = Convert.ToDouble(num4);
          this.m_c = Convert.ToDouble(num3);
          if (this.Fill_ycalc())
          {
            this.CalculateR2();
            flag = true;
          }
          else
            flag = false;
        }
        catch (Exception ex)
        {
          ProjectData.SetProjectError(ex);
          this.LastErrMsg = ex.Message;
          flag = false;
          ProjectData.ClearProjectError();
        }
        return flag;
      }

      private bool Fill_ycalc()
      {
        bool flag;
        try
        {
          int num1 = 0;
          int num2 = checked (this.m_Count - 1);
          int index = num1;
          while (index <= num2)
          {
            this.ycalc[index] = this.yCalculated(this.x[index]);
            checked { ++index; }
          }
          flag = true;
        }
        catch (Exception ex)
        {
          ProjectData.SetProjectError(ex);
          this.LastErrMsg = ex.Message;
          flag = false;
          ProjectData.ClearProjectError();
        }
        return flag;
      }

      private bool Fill_xp(int i)
      {
        bool flag;
        try
        {
          this.xp[0] = Decimal.One;
          int index = 1;
          do
          {
            this.xp[index] = new Decimal(Convert.ToDouble(this.xp[checked (index - 1)]) * this.x[i]);
            checked { ++index; }
          }
          while (index <= 4);
          flag = true;
        }
        catch (Exception ex)
        {
          ProjectData.SetProjectError(ex);
          this.LastErrMsg = ex.Message;
          flag = false;
          ProjectData.ClearProjectError();
        }
        return flag;
      }

      private void CalculateR2()
      {
        double num1 = 0;
        double num2 = 0;
        if (this.m_RF > 1.0)
        {
          int num3 = 0;
          int num4 = checked (this.m_Count - 1);
          int index = num3;
          while (index <= num4)
          {
            double num5 = (this.y[index] - this.ycalc[index]) / this.y[index] * this.m_RF;
            num1 += num5 * num5;
            double num6 = this.yy[index] - this.y_mean;
            num2 += num6 * num6;
            checked { ++index; }
          }
        }
        else
        {
          int num3 = 0;
          int num4 = checked (this.m_Count - 1);
          int index = num3;
          while (index <= num4)
          {
            double num5 = this.y[index] - this.ycalc[index];
            num1 += num5 * num5;
            double num6 = this.y[index] - this.y_mean;
            num2 += num6 * num6;
            checked { ++index; }
          }
        }
        this.m_r2 = num2 <= 0.0 ? 1.0 : 1.0 - num1 / num2;
        this.m_rmsd = num1 / (double) this.m_Count;
        this.m_cvrmsd = this.m_rmsd / this.y_mean;
      }
    }

    public class ttlRegressionX : TotalCalibration.ttlRegression
    {
      public double Tolerance;
      private int m_original_cnt;
      private int m_removed_cnt;

      public ttlRegressionX(int NewCount)
        : base(NewCount)
      {
        this.Tolerance = 1.0;
      }

      public int Count_Start
      {
        get
        {
          return this.m_original_cnt;
        }
      }

      public int Count_Removed
      {
        get
        {
          return this.m_removed_cnt;
        }
      }

      public string ProcessX()
      {
        double[] numArray1 = new double[0];
        string str;
        try
        {
          this.m_original_cnt = this.Count;
          bool flag = this.Count < 2;
          while (!flag)
          {
            if (this.RegressionCalculate())
            {
              object obj = (object) 0;
              double[] numArray2 = new double[checked (this.Count - 1 + 1)];
              int num1 = 0;
              int num2 = checked (this.Count - 1);
              int index1 = num1;
              while (index1 <= num2)
              {
                numArray2[index1] = Math.Abs(TotalCalibration.RelativeError_PPM(this.y[index1], this.ycalc[index1]));
                if (Operators.ConditionalCompareObjectGreater((object) numArray2[index1], obj, false))
                  obj = (object) numArray2[index1];
                checked { ++index1; }
              }
              if (Operators.ConditionalCompareObjectLessEqual(obj, (object) this.Tolerance, false))
              {
                flag = true;
              }
              else
              {
                int index2 = 0;
                int num3 = 0;
                int num4 = checked (this.Count - 1);
                int index3 = num3;
                while (index3 <= num4)
                {
                  if (Operators.ConditionalCompareObjectLess((object) numArray2[index3], obj, false))
                  {
                    this.x[index2] = this.x[index3];
                    this.y[index2] = this.y[index3];
                    this.id[index2] = this.id[index3];
                    this.ind[index2] = this.ind[index3];
                    this.ycalc[index2] = this.ycalc[index3];
                    checked { ++index2; }
                  }
                  checked { ++index3; }
                }
                if (index2 > 0)
                {
                  this.x = (double[]) Utils.CopyArray((Array) this.x, (Array) new double[checked (index2 - 1 + 1)]);
                  this.y = (double[]) Utils.CopyArray((Array) this.y, (Array) new double[checked (index2 - 1 + 1)]);
                  this.id = (string[]) Utils.CopyArray((Array) this.id, (Array) new string[checked (index2 - 1 + 1)]);
                  this.ind = (int[]) Utils.CopyArray((Array) this.ind, (Array) new int[checked (index2 - 1 + 1)]);
                  this.ycalc = (double[]) Utils.CopyArray((Array) this.ycalc, (Array) new double[checked (index2 - 1 + 1)]);
                  this.Count = index2;
                }
                else
                {
                  this.x = (double[]) null;
                  this.y = (double[]) null;
                  this.ycalc = (double[]) null;
                  this.id = (string[]) null;
                  this.ind = (int[]) null;
                  this.Count = 0;
                }
              }
            }
            if (!flag && !this.Success)
              flag = true;
          }
          this.m_removed_cnt = checked (this.m_original_cnt - this.Count);
          str = "calibration iteration successful; original reference point count " + Conversions.ToString(this.m_original_cnt) + " removed points " + Conversions.ToString(this.m_removed_cnt);
        }
        catch (Exception ex)
        {
          ProjectData.SetProjectError(ex);
          Exception exception = ex;
          this.Success = false;
          str = "error iterating calibration - " + exception.Message;
          ProjectData.ClearProjectError();
        }
        return str;
      }
    }

    public struct ttlCalibrationPeak : IComparable
    {
      public double m;
      public double a;
      public string source;
      public int order;
      public string name;

      public int CompareTo(object obj)
      {
        if (!(obj is TotalCalibration.ttlCalibrationPeak))
          throw new ArgumentException();
        object obj1 = obj;
        TotalCalibration.ttlCalibrationPeak undefinedPeak = new TotalCalibration.ttlCalibrationPeak();
        return this.m.CompareTo((obj1 != null ? (TotalCalibration.ttlCalibrationPeak) obj1 : undefinedPeak).m);
      }
    }

    public struct ttlNumericMatch : IComparable
    {
      public double DataT;
      public int DataT_Ind;
      public double DataE;
      public int DataE_Ind;
      public double DataW;
      public string DataDesc;
      public double Err;

      public int CompareTo(object obj)
      {
        if (!(obj is TotalCalibration.ttlNumericMatch))
          throw new ArgumentException();
        object obj1 = obj;
        TotalCalibration.ttlNumericMatch undefinedMatch = new TotalCalibration.ttlNumericMatch();
        return this.Err.CompareTo((obj1 != null ? (TotalCalibration.ttlNumericMatch) obj1 : undefinedMatch).Err);
      }
    }

    public class ttlCalSetScore : IComparable
    {
      public double Tol;
      public bool Complete;
      public bool Success;
      public double HitAMinPctUnq;
      public double HitAMinRange;
      public int HitCount;
      public int HitCountUnq;
      public double AvgErr;
      public double MedErr;
      public double StDErr;
      public double MinHitsE;
      public double MaxHitsE;
      public int HitCountA;
      public double AvgErrA;
      public double MedErrA;
      public double StDErrA;
      public double MinHitsEA;
      public double MaxHitsEA;
      public TotalCalibration.ttlNumericMatch[] Hits;

      public ttlCalSetScore()
      {
        this.Complete = true;
        this.Success = false;
        this.HitAMinPctUnq = 0.6;
        this.HitAMinRange = 100.0;
        this.Hits = new TotalCalibration.ttlNumericMatch[0];
      }

      public int CompareTo(object obj)
      {
        if (obj is TotalCalibration.ttlCalSetScore)
          return this.HitCountA.CompareTo(((TotalCalibration.ttlCalSetScore) obj).HitCountA);
        throw new ArgumentException();
      }

      public override string ToString()
      {
        StringBuilder stringBuilder = new StringBuilder(string.Empty);
        string message;
        try
        {
          stringBuilder.AppendLine("\tmatch_count=" + Conversions.ToString(this.HitCount));
          stringBuilder.AppendLine("\tavg_error=" + Conversions.ToString(this.AvgErr));
          stringBuilder.AppendLine("\tmed_error=" + Conversions.ToString(this.MedErr));
          stringBuilder.AppendLine("\tstd_error=" + Conversions.ToString(this.StDErr));
          stringBuilder.AppendLine("median_reduced_range=" + Conversions.ToString(this.Tol));
          stringBuilder.AppendLine("\treduced_match_count=" + Conversions.ToString(this.HitCountA));
          stringBuilder.AppendLine("\treduced_avg_error=" + Conversions.ToString(this.AvgErrA));
          stringBuilder.AppendLine("\treduced_med_error=" + Conversions.ToString(this.MedErrA));
          stringBuilder.AppendLine("\treduced_std_error=" + Conversions.ToString(this.StDErrA));
          message = stringBuilder.ToString();
        }
        catch (Exception ex)
        {
          ProjectData.SetProjectError(ex);
          message = ex.Message;
          ProjectData.ClearProjectError();
        }
        return message;
      }

      public bool ManageHitsArray(ttlArrayManagementType mng_type, int sz = 1000)
      {
        bool flag;
        try
        {
          switch (mng_type)
          {
            case ttlArrayManagementType.aErase:
              this.HitCount = 0;
              this.HitCountUnq = 0;
              this.HitCountA = 0;
              this.Hits = (TotalCalibration.ttlNumericMatch[]) null;
              this.Hits = (TotalCalibration.ttlNumericMatch[]) null;
              break;
            case ttlArrayManagementType.aInitialize:
              this.HitCount = 0;
              this.HitCountUnq = 0;
              this.HitCountA = 0;
              this.Hits = new TotalCalibration.ttlNumericMatch[checked (sz + 1)];
              break;
            case ttlArrayManagementType.aAdd:
              this.Hits = (TotalCalibration.ttlNumericMatch[]) Utils.CopyArray((Array) this.Hits, (Array) new TotalCalibration.ttlNumericMatch[checked (this.HitCount + sz + 1)]);
              break;
            case ttlArrayManagementType.aTrim:
              if (this.HitCount > 0)
              {
                this.Hits = (TotalCalibration.ttlNumericMatch[]) Utils.CopyArray((Array) this.Hits, (Array) new TotalCalibration.ttlNumericMatch[checked (this.HitCount - 1 + 1)]);
              }
              else
              {
                this.Hits = (TotalCalibration.ttlNumericMatch[]) null;
                this.Hits = (TotalCalibration.ttlNumericMatch[]) null;
              }
              this.Calculate_DistinctCount_Success();
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

      private void Calculate_DistinctCount_Success()
      {
        double[] NewData = new double[checked (this.Hits.Length - 1 + 1)];
        try
        {
          this.Success = false;
          this.MinHitsE = double.MaxValue;
          this.MaxHitsE = 0.0;
          this.MinHitsEA = double.MaxValue;
          this.MaxHitsEA = 0.0;
          double[] numArray = new double[checked (this.Hits.Length - 1 + 1)];
          Array.Sort<TotalCalibration.ttlNumericMatch>(this.Hits);
          ttlStat ttlStat1 = new ttlStat(this.Hits.Length);
          int num1 = 0;
          int num2 = checked (this.Hits.Length - 1);
          int index1 = num1;
          while (index1 <= num2)
          {
            numArray[index1] = this.Hits[index1].DataT;
            ttlStat1.Data[index1] = this.Hits[index1].Err;
            checked { ++index1; }
          }
          ttlStat1.Calculate();
          this.AvgErr = ttlStat1.Mean;
          this.MedErr = ttlStat1.Median;
          this.StDErr = ttlStat1.StDev;
          int num3 = 0;
          int num4 = checked (this.Hits.Length - 1);
          int index2 = num3;
          while (index2 <= num4)
          {
            if (this.Hits[index2].DataE < this.MinHitsE)
              this.MinHitsE = this.Hits[index2].DataE;
            if (this.Hits[index2].DataE > this.MaxHitsE)
              this.MaxHitsE = this.Hits[index2].DataE;
            if (Math.Abs(this.Hits[index2].Err - this.MedErr) <= this.Tol)
            {
              if (this.Hits[index2].DataE < this.MinHitsEA)
                this.MinHitsEA = this.Hits[index2].DataE;
              if (this.Hits[index2].DataE > this.MaxHitsEA)
                this.MaxHitsEA = this.Hits[index2].DataE;
              NewData[this.HitCountA] = this.Hits[index2].Err;
              checked { ++this.HitCountA; }
            }
            checked { ++index2; }
          }
          if (this.HitCountA > 0)
          {
            ttlStat ttlStat2 = new ttlStat(this.HitCountA);
            ttlStat2.SetData(NewData, false);
            ttlStat2.Calculate();
            this.AvgErrA = ttlStat2.Mean;
            this.MedErrA = ttlStat2.Median;
            this.StDErrA = ttlStat2.StDev;
          }
          else
          {
            this.AvgErrA = double.NaN;
            this.MedErrA = double.NaN;
            this.StDErrA = double.NaN;
          }
          Array a = (Array) numArray;
          this.HitCountUnq = TotalCalibration.DistinctCount(ref a);
          if (this.HitCount <= 0 || this.MaxHitsEA - this.MinHitsEA < this.HitAMinRange || (double) this.HitCountUnq / (double) this.HitCount < this.HitAMinPctUnq)
            return;
          this.Success = true;
        }
        catch (Exception ex)
        {
          ProjectData.SetProjectError(ex);
          this.Success = false;
          this.MinHitsEA = double.MaxValue;
          this.MaxHitsEA = 0.0;
          this.HitCountUnq = 0;
          ProjectData.ClearProjectError();
        }
      }

      private bool IsInCalibrationRange_EA(double e_val)
      {
        return e_val >= this.MinHitsEA & e_val <= this.MaxHitsEA;
      }

      private bool IsInCalibrationRange_E(double e_val)
      {
        return e_val >= this.MinHitsE & e_val <= this.MaxHitsE;
      }
    }
  }
}

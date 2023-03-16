// Decompiled with JetBrains decompiler
// Type: TestFSDBSearch.TotalIPDBSearch
// Assembly: TestFSDBSearch, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C36EBD2C-6652-4FDC-A74D-B15E46A79224
// Assembly location: F:\Documents\Projects\NikolaTolic\Formularity\Lib\TestFSDBSearch.exe

using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.IO;
using System.Text;
using TestFSDBSearch.TotalSupport;

namespace TestFSDBSearch
{
  public class TotalIPDBSearch
  {
    private double[] data_m;
    private double[] data_a;
    private double[] data_sn;
    private double[] data_ra;
    private int data_cnt;
    private double data_max_a;
    public string MFFileName;
    public string MFLoadedHeader;
    public int MFCount_Declared;
    public int MFCount_Major;
    public int MFCount_Minor;
    public int MFCount;
    public MFIsoPat[] MFS;
    public double m_ppm_tol;
    private IonizationMethod m_calc_ionization;
    private EPolarity m_calc_polarity;
    private string m_calc_adduct;
    private double m_calc_ccm;
    private int m_calc_cs;
    private int m_adduct_sign;
    private double m_adduct_m0;
    private CannonMF m_mf;
    private string m_charged_mass_calc_descriptive;
    private string m_charged_mass_calc_numeric;
    public int[] MFSInd;
    public double[] MFSM0;
    public bool MFIsSearchable;
    public double M02Find;
    public double M0AbsTol;
    public int M0Ind1;
    public int M0Ind2;
    public int M0Cnt;
    private int m_curr_rec_no;
    public MFIsoPat score_template;
    public MFIsoPat match_template_min_ind;
    public MFIsoPat match_template_max_ind;
    public bool m_best_err;
    public double m_min_major_sn;
    public double m_min_minor_sn;
    public double m_min_major_pa_mm_abs_2_report;
    public double m_min_p_to_score;
    public bool m_matched_peaks_report;
    public string m_IPDB_ec_filter;
    public ROX_List m_roxy;
    private TextWriter tw_peaks;
    private TextWriter tw_IPDB;
    private double tmp_search_val;
    private double tmp_search_val_tol;
    private int tmp_search_hit_ind1;
    private int tmp_search_hit_ind2;
    private int tmp_major_hit_cnt;
    private int tmp_total_hit_cnt;
    private int tmp_score_multi_hit_cnt;
    private int tmp_s_ind;
    private double tmp_s_val;
    public bool memorize_best_matches;
    public MFIsoPat[] IPDB_best_match;
    public MFIsoPatScore[] IPDB_score;
    private StringBuilder major_peaks_stat;
    public string deli;
    public string deli_out;
    public StringBuilder IPDB_log;
    private int diagnostic_error_handler_entries;
    private int diagnostic_multi_option_entry;

    public TotalIPDBSearch()
    {
      MFFileName = string.Empty;
      MFLoadedHeader = string.Empty;
      MFS = new MFIsoPat[0];
      m_ppm_tol = 1.0;
      m_calc_ionization = IonizationMethod.proton_detachment;
      m_calc_polarity = EPolarity.Positive;
      m_calc_adduct = string.Empty;
      m_calc_cs = 1;
      m_adduct_m0 = 0.0;
      m_mf = (CannonMF) null;
      MFSInd = new int[0];
      MFSM0 = new double[0];
      MFIsSearchable = false;
      m_best_err = true;
      m_min_major_sn = 1.0;
      m_min_minor_sn = 1.0;
      m_min_major_pa_mm_abs_2_report = 2.0;
      m_min_p_to_score = 0.001;
      m_matched_peaks_report = true;
      m_IPDB_ec_filter = "";
      tw_peaks = (TextWriter) null;
      tw_IPDB = (TextWriter) null;
      memorize_best_matches = true;
      deli = "\t";
      deli_out = ",";
      IPDB_log = new StringBuilder(string.Empty);
      SetCalculation();
    }

    public bool IPDB_Ready
    {
      get
      {
        return MFCount > 0;
      }
    }

    public IonizationMethod Ionization
    {
      get
      {
        return m_calc_ionization;
      }
      set
      {
        m_calc_ionization = value;
        if (!SetCalculation())
          throw new ArgumentException();
      }
    }

    public int CS
    {
      get
      {
        return m_calc_cs;
      }
      set
      {
        if (value < 0)
          throw new ArgumentException("charge state must be non-negative integer! for negative charge use one of negative ionization modes!");
        m_calc_cs = Math.Abs(value);
        if (!SetCalculation())
          throw new ArgumentException();
      }
    }

    public string Adduct
    {
      get
      {
        return m_calc_adduct;
      }
      set
      {
        m_calc_adduct = value.Replace(" ", "");
        m_adduct_sign = 0;
        if (m_calc_adduct.Length > 0)
        {
          if (Operators.CompareString(m_calc_adduct.Substring(0, 1), "-", false) == 0)
          {
            m_adduct_sign = -1;
            m_calc_adduct = value.Replace("-", "");
          }
          else
            m_adduct_sign = 1;
        }
        if (!SetCalculation())
          throw new ArgumentException();
      }
    }

    public EPolarity Polarity
    {
      get
      {
        return m_calc_polarity;
      }
    }

    public double CCM
    {
      get
      {
        return m_calc_ccm;
      }
    }

    public string ChargedMassFormula_Descriptive
    {
      get
      {
        return m_charged_mass_calc_descriptive;
      }
    }

    public string ChargedMassFormula_Numeric
    {
      get
      {
        return m_charged_mass_calc_numeric;
      }
    }

    public bool SetCalculation()
    {
      bool flag;
      try
      {
        switch (m_calc_ionization)
        {
          case IonizationMethod.none:
            m_calc_polarity = EPolarity.Neutral;
            m_calc_ccm = 0.0;
            break;
          case IonizationMethod.proton_detachment:
            m_calc_polarity = EPolarity.Negative;
            m_calc_ccm = 1.00727646688;
            break;
          case IonizationMethod.proton_attachment:
            m_calc_polarity = EPolarity.Positive;
            m_calc_ccm = 1.00727646688;
            break;
          case IonizationMethod.electron_detachment:
            m_calc_polarity = EPolarity.Positive;
            m_calc_ccm = -0.00054857990945;
            break;
          case IonizationMethod.electron_attachment:
            m_calc_polarity = EPolarity.Negative;
            m_calc_ccm = -0.00054857990945;
            break;
        }
        if (m_mf == null)
        {
          m_mf = new CannonMF();
          if (!m_mf.ReadElementsFromFile("Isotope.inf"))
          {
            flag = false;
            goto label_16;
          }
        }
        m_adduct_m0 = 0.0;
        if (m_calc_adduct.Length > 0)
        {
          if (!m_mf.SetMF(ttlAI.TranslateShortToLongMF(m_calc_adduct)))
          {
            flag = false;
            goto label_16;
          }
          else
            m_adduct_m0 = m_mf.GetMF_Mono0Mass();
        }
        flag = SetFormulaDescriptions();
      }
      catch (Exception ex)
      {
        ProjectData.SetProjectError(ex);
        flag = false;
        ProjectData.ClearProjectError();
      }
label_16:
      return flag;
    }

    public double GetChargedMass(double m0)
    {
      if (m_calc_cs > 0)
        return (m0 + (double) m_adduct_sign * m_adduct_m0) / (double) m_calc_cs + (double) m_calc_polarity * m_calc_ccm;
      return m0;
    }

    public double GetNeutralMass(double mz)
    {
      if (m_calc_cs > 0)
        return (double) m_calc_cs * (mz - (double) m_calc_polarity * m_calc_ccm) - (double) m_adduct_sign * m_adduct_m0;
      return mz;
    }

    public bool LoadTabulatedDB(string file_name)
    {
      var textReader = (TextReader) new StreamReader(file_name);
      var empty = string.Empty;
      var strArray1 = new string[0];
      bool flag;
      try
      {
        var strArray2 = textReader.ReadLine().Split(Conversions.ToChar(deli));
        MFCount_Declared = int.Parse(strArray2[0]);
        MFCount_Major = int.Parse(strArray2[1]);
        MFCount_Minor = int.Parse(strArray2[2]);
        if (ManageArrays(ttlArrayManagementType.aInitialize, MFCount_Declared))
        {
          int num1 = 0;
          while (MFCount < MFCount_Declared & num1 < 100 & textReader.Peek() != -1)
          {
            MF_BRW.NewMF(ref MFS[MFCount], MFCount_Major, MFCount_Minor);
            var str_rec = textReader.ReadLine();
            int num2 = 0;
            if (!MF_BRW.FromText(ref MFS[MFCount], str_rec, deli))
            {
              if (num2 > 0)
                checked { ++num1; }
              else
                MFLoadedHeader = str_rec;
            }
            else
              checked { ++MFCount; }
            checked { ++num2; }
          }
          if (ManageArrays(ttlArrayManagementType.aTrim, 7001))
          {
            MFFileName = file_name;
            flag = true;
            goto label_14;
          }
        }
        MFFileName = string.Empty;
        flag = false;
      }
      catch (Exception ex)
      {
        ProjectData.SetProjectError(ex);
        var exception = ex;
        MFCount = -1;
        MFS = (MFIsoPat[]) null;
        IPDB_log.AppendLine("LoadTabulatedDB & [" + exception.Message + "]");
        flag = false;
        ProjectData.ClearProjectError();
      }
      finally
      {
        textReader.Close();
        IPDB_log.AppendLine(Info());
      }
label_14:
      return flag;
    }

    private bool SetFormulaDescriptions()
    {
      var str1 = "M";
      var str2 = "+";
      bool flag;
      try
      {
        var str3 = ttlAI.AI_IonizationMethod_Enum2String(m_calc_ionization);
        var str4 = ttlAI.AI_IonizationMethod_Enum2PolarityString(m_calc_ionization);
        if (m_adduct_m0 > 0.0)
        {
          if (m_adduct_sign < 0)
            str2 = "-";
        }
        else
          str2 = string.Empty;
        m_charged_mass_calc_descriptive = str1 + str2 + m_calc_adduct;
        if (m_calc_cs > 1)
        {
          if (m_charged_mass_calc_descriptive.Length > 1)
            m_charged_mass_calc_descriptive = "(" + m_charged_mass_calc_descriptive + ")";
          m_charged_mass_calc_descriptive = m_charged_mass_calc_descriptive + "/" + m_calc_cs.ToString();
        }
        m_charged_mass_calc_descriptive = m_charged_mass_calc_descriptive + str3 + " (" + str4 + ")";
        m_charged_mass_calc_numeric = "M";
        var num1 = (double) m_adduct_sign * m_adduct_m0;
        if (m_calc_cs > 1)
        {
          m_charged_mass_calc_numeric = "M/" + m_calc_cs.ToString();
          num1 /= (double) m_calc_cs;
        }
        var num2 = num1 + (double) m_calc_polarity * m_calc_ccm;
        if (num2 > 0.0)
          m_charged_mass_calc_numeric = m_charged_mass_calc_numeric + "+" + num2.ToString("0.000000");
        else if (num2 < 0.0)
          m_charged_mass_calc_numeric += num2.ToString("0.000000");
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

    public string Info()
    {
label_1:
      int num1 = 0;
      string str1;
      int num2 = 0;
      var stringBuilder = new StringBuilder("database " + MFFileName + "\r\n");
      try
            {
        ProjectData.ClearProjectError();
        num1 = -2;
label_2:
        var num3 = 2;
label_3:
        num3 = 3;
        stringBuilder.AppendLine("records count " + Conversions.ToString(MFCount));
label_4:
        num3 = 4;
        if (major_peaks_stat != null)
          goto label_6;
label_5:
        num3 = 5;
        CalcMajorPeaksStat();
label_6:
        num3 = 7;
        if (major_peaks_stat == null)
          goto label_8;
label_7:
        num3 = 8;
        stringBuilder.AppendLine(major_peaks_stat.ToString());
label_8:
        num3 = 10;
        str1 = stringBuilder.ToString();
        goto label_15;
label_10:
        num2 = num3;
        switch (num1 > -2 ? num1 : 1)
        {
          case 1:
            var num4 = num2 + 1;
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
              case 7:
                goto label_6;
              case 8:
                goto label_7;
              case 9:
              case 10:
                goto label_8;
              case 11:
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
      var str2 = str1;
      if (num2 == 0)
        return str2;
      ProjectData.ClearProjectError();
      return str2;
    }

    private void DeleteDB()
    {
label_1:
      int num1 = 0;
      int num2 = 0;
      try
      {
        ProjectData.ClearProjectError();
        num1 = -2;
label_2:
        var num3 = 2;
        MFFileName = string.Empty;
label_3:
        num3 = 3;
        MFCount = 0;
label_4:
        num3 = 4;
        MFCount_Declared = 0;
label_5:
        num3 = 5;
        MFS = (MFIsoPat[]) null;
label_6:
        num3 = 6;
        MFIsSearchable = false;
label_7:
        num3 = 7;
        major_peaks_stat = (StringBuilder) null;
        goto label_14;
label_9:
        num2 = num3;
        switch (num1 > -2 ? num1 : 1)
        {
          case 1:
            var num4 = num2 + 1;
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
                goto label_14;
            }
        }
      }
      catch (Exception ex) when (ex is Exception & (uint) num1 > 0U & num2 == 0)
      {
        ProjectData.SetProjectError(ex);
          return;
      }
      throw ProjectData.CreateProjectError(-2146828237);
label_14:
      if (num2 == 0)
        return;
      ProjectData.ClearProjectError();
    }

    public bool GetMassErrors(double m, int mf_ind, ref double ppmErr, ref double amuErr)
    {
      bool flag;
      try
      {
        var theoretical_val = MFS[mf_ind].m_major[MFS[mf_ind].m_search_ind];
        amuErr = m - theoretical_val;
        ppmErr = RelativeError_PPM(m, theoretical_val);
        flag = true;
      }
      catch (Exception ex)
      {
        ProjectData.SetProjectError(ex);
        ppmErr = double.NaN;
        amuErr = double.NaN;
        flag = false;
        ProjectData.ClearProjectError();
      }
      return flag;
    }

    public static double RelativeError_PPM(double measured_val, double theoretical_val)
    {
      if (measured_val == theoretical_val)
        return 0.0;
      if (Math.Abs(theoretical_val) > 0.0)
        return (measured_val - theoretical_val) / theoretical_val * 1000000.0;
      return measured_val > 0.0 ? double.PositiveInfinity : double.NegativeInfinity;
    }

    private bool ManageArrays(ttlArrayManagementType MgtType, int lSz = 7001)
    {
      bool flag;
      try
      {
        switch (MgtType)
        {
          case ttlArrayManagementType.aErase:
            MFCount = 0;
            MFS = (MFIsoPat[]) null;
            break;
          case ttlArrayManagementType.aInitialize:
            MFCount = 0;
            MFS = new MFIsoPat[checked (lSz - 1 + 1)];
            break;
          case ttlArrayManagementType.aAdd:
            lSz = checked (MFCount + lSz);
            MFS = (MFIsoPat[]) Utils.CopyArray((Array) MFS, (Array) new MFIsoPat[checked (lSz + 1)]);
            break;
          case ttlArrayManagementType.aTrim:
            MFS = MFCount <= 0 ? (MFIsoPat[]) null : (MFIsoPat[]) Utils.CopyArray((Array) MFS, (Array) new MFIsoPat[checked (MFCount - 1 + 1)]);
            break;
        }
        flag = true;
      }
      catch (Exception ex)
      {
        ProjectData.SetProjectError(ex);
        IPDB_log.AppendLine("ManageArrays & [" + ex.Message + "]");
        flag = false;
        ProjectData.ClearProjectError();
      }
      return flag;
    }

    private bool CalcMajorPeaksStat()
    {
      bool flag;
      try
      {
        if (MFCount > 0)
        {
          major_peaks_stat = new StringBuilder("spacing statistics for major peaks\r\n");
          var ttlStat1 = new ttlStat(MFCount);
          var ttlStat2 = new ttlStat(MFCount);
          var num1 = 0;
          var num2 = checked (MFCount - 1);
          var index = num1;
          int num3 = 0;
          while (index <= num2)
          {
            if (MFS[index].dm_major[0] > 0.0 & MFS[index].dm_major[1] > 0.0)
            {
              ttlStat1.SetDataPoint(MFS[index].dm_major[0], 1000);
              ttlStat2.SetDataPoint(MFS[index].dm_major[0] + MFS[index].dm_major[1], 1000);
              int num4 = 0;
              checked { ++num4; }
            }
            else
              checked { ++num3; }
            checked { ++index; }
          }
          ttlStat1.ManageDataArray(ttlArrayManagementType.aTrim, 1000);
          ttlStat1.Calculate();
          ttlStat2.ManageDataArray(ttlArrayManagementType.aTrim, 1000);
          ttlStat2.Calculate();
          major_peaks_stat.AppendLine("1-2 major peaks median spacing " + ttlStat1.Median.ToString());
          major_peaks_stat.AppendLine("1-3 major peaks median spacing " + ttlStat2.Median.ToString());
          if (num3 > 0)
            major_peaks_stat.AppendLine("records without 3 major peaks " + Conversions.ToString(num3));
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

    private string data_helper_GetPeakString(int i)
    {
      var stringBuilder = new StringBuilder(i.ToString());
      stringBuilder.Append(deli_out + Conversions.ToString(data_m[i]));
      stringBuilder.Append(deli_out + Conversions.ToString(data_a[i]));
      stringBuilder.Append(deli_out + Conversions.ToString(data_sn[i]));
      stringBuilder.Append(deli_out + Conversions.ToString(data_ra[i]));
      return stringBuilder.ToString();
    }

    public int FindMF(string mf)
    {
      var flag = MFCount <= 0;
      var index = m_curr_rec_no;
      var longMf = ttlAI.TranslateShortToLongMF(mf);
      int num;
      try
      {
        if (!Versioned.IsNumeric((object) longMf))
        {
          while (!flag)
          {
            index = checked (index + 1) % MFCount;
            if (string.Compare(longMf, MFS[index].MF) == 0)
            {
              num = index;
              goto label_10;
            }
            else if (index == m_curr_rec_no)
              flag = true;
          }
        }
        num = -1;
      }
      catch (Exception ex)
      {
        ProjectData.SetProjectError(ex);
        IPDB_log.AppendLine("FindMF & [" + ex.Message + "]");
        num = -1;
        ProjectData.ClearProjectError();
      }
label_10:
      return num;
    }

    private void FindM0Range(ref int MinInd, ref int MaxInd)
    {
      var index1 = checked (MinInd + MaxInd) / 2;
      if (index1 == MinInd)
      {
        if (Math.Abs(M02Find - MFSM0[MinInd]) > M0AbsTol)
          MinInd = MaxInd;
        if (Math.Abs(M02Find - MFSM0[MaxInd]) <= M0AbsTol)
          return;
        MaxInd = index1;
      }
      else if (MFSM0[index1] > M02Find + M0AbsTol)
      {
        MaxInd = index1;
        FindM0Range(ref MinInd, ref MaxInd);
      }
      else if (MFSM0[index1] < M02Find - M0AbsTol)
      {
        MinInd = index1;
        FindM0Range(ref MinInd, ref MaxInd);
      }
      else
      {
        var index2 = index1;
        bool flag1 = false;
        while (!flag1)
        {
          checked { --index2; }
          if (index2 < MinInd)
            flag1 = true;
          else if (Math.Abs(M02Find - MFSM0[index2]) > M0AbsTol)
            flag1 = true;
        }
        var index3 = index1;
        bool flag2 = false;
        while (!flag2)
        {
          checked { ++index3; }
          if (index3 > MaxInd)
            flag2 = true;
          else if (Math.Abs(M02Find - MFSM0[index3]) > M0AbsTol)
            flag2 = true;
        }
        MinInd = checked (index2 + 1);
        MaxInd = checked (index3 - 1);
      }
    }

    private int FindMinIndGEVal(double m)
    {
      int num1;
      try
      {
        var index1 = 0;
        var index2 = checked (MFCount - 1);
        if (m <= MFSM0[index1])
          num1 = index1;
        else if (m > MFSM0[index2])
        {
          num1 = -1;
        }
        else
        {
          var flag = checked (index1 + 1) >= index2;
          while (!flag)
          {
            var index3 = checked (index1 + index2) / 2;
            if (index3 == index1)
              flag = true;
            else if (m == MFSM0[index3])
            {
              var num2 = index3;
              var num3 = index1;
              var index4 = num2;
              while (index4 >= num3)
              {
                if (m == MFSM0[index4])
                {
                  index1 = index4;
                  checked { index4 += -1; }
                }
                else
                {
                  flag = true;
                  break;
                }
              }
            }
            else if (m < MFSM0[index3])
              index2 = index3;
            else
              index1 = index3;
            if (!flag)
              flag = checked (index1 + 1) >= index2;
          }
          num1 = m > MFSM0[index1] ? index2 : index1;
        }
      }
      catch (Exception ex)
      {
        ProjectData.SetProjectError(ex);
        num1 = -1;
        ProjectData.ClearProjectError();
      }
      return num1;
    }

    private int FindMaxIndLEVal(double m)
    {
      int num1;
      try
      {
        var index1 = 0;
        var index2 = checked (MFCount - 1);
        if (m < MFSM0[index1])
          num1 = -1;
        else if (m >= MFSM0[index2])
        {
          num1 = index2;
        }
        else
        {
          var flag = checked (index1 + 1) >= index2;
          while (!flag)
          {
            var index3 = checked (index1 + index2) / 2;
            if (index3 == index1)
              flag = true;
            else if (m == MFSM0[index3])
            {
              var num2 = index3;
              var num3 = index2;
              var index4 = num2;
              while (index4 <= num3)
              {
                if (m == MFSM0[index4])
                {
                  index2 = index4;
                  checked { ++index4; }
                }
                else
                {
                  flag = true;
                  break;
                }
              }
            }
            else if (m < MFSM0[index3])
              index2 = index3;
            else
              index1 = index3;
            if (!flag)
              flag = checked (index1 + 1) >= index2;
          }
          num1 = m < MFSM0[index2] ? index1 : index2;
        }
      }
      catch (Exception ex)
      {
        ProjectData.SetProjectError(ex);
        num1 = -1;
        ProjectData.ClearProjectError();
      }
      return num1;
    }

    public bool CreateIndex()
    {
      bool flag;
      try
      {
        if (MFCount > 1)
        {
          MFSInd = new int[checked (MFCount - 1 + 1)];
          MFSM0 = new double[checked (MFCount - 1 + 1)];
          var num1 = 0;
          var num2 = checked (MFCount - 1);
          var index = num1;
          while (index <= num2)
          {
            MFSInd[index] = index;
            MFSM0[index] = MFS[index].m_major[MFS[index].m_search_ind];
            checked { ++index; }
          }
          Array.Sort<double, int>(MFSM0, MFSInd);
        }
        flag = true;
      }
      catch (Exception ex)
      {
        ProjectData.SetProjectError(ex);
        IPDB_log.AppendLine("CreateIndex & [" + ex.Message + "]");
        flag = false;
        ProjectData.ClearProjectError();
      }
      return flag;
    }

    public int FindMatches(double m)
    {
      int num;
      try
      {
        SetMass4Search(m);
        if (MFCount > 0)
        {
          M0Ind1 = 0;
          M0Ind2 = checked (MFCount - 1);
          FindM0Range(ref M0Ind1, ref M0Ind2);
          M0Cnt = M0Ind1 > M0Ind2 ? 0 : checked (M0Ind2 - M0Ind1 + 1);
        }
        num = M0Cnt;
      }
      catch (Exception ex)
      {
        ProjectData.SetProjectError(ex);
        IPDB_log.AppendLine("FindMatches & [" + ex.Message + "]");
        num = -1;
        ProjectData.ClearProjectError();
      }
      return num;
    }

    public int FindMatches(double m1, double m2)
    {
      int num;
      try
      {
        if (MFCount > 0)
        {
          M02Find = (m1 + m2) / 2.0;
          M0AbsTol = Math.Abs(m2 - m1) / 2.0;
          M0Ind1 = 0;
          M0Ind2 = checked (MFCount - 1);
          FindM0Range(ref M0Ind1, ref M0Ind2);
          M0Cnt = M0Ind1 > M0Ind2 ? 0 : checked (M0Ind2 - M0Ind1 + 1);
        }
        num = M0Cnt;
      }
      catch (Exception ex)
      {
        ProjectData.SetProjectError(ex);
        IPDB_log.AppendLine("FindMatches & [" + ex.Message + "]");
        num = -1;
        ProjectData.ClearProjectError();
      }
      return num;
    }

    private void SetMass4Search(double m)
    {
      M02Find = m_calc_cs <= 0 ? m : (m - (double) m_calc_polarity * m_calc_ccm) * (double) m_calc_cs;
      M0AbsTol = m_ppm_tol * M02Find / 1000000.0;
    }

    public MFIsoPat GetMFIsoPat_mzSpace(int ind)
    {
      var mfIsoPat1 = new MFIsoPat();
      MFIsoPat mfIsoPat2;
      try
      {
        var mfIsoPat3 = MF_BRW.NewMF_FromTemplate(MFS[ind]);
        if (m_calc_cs > 0)
        {
          var num1 = 0;
          var num2 = checked (mfIsoPat3.m_major.Length - 1);
          var index1 = num1;
          while (index1 <= num2)
          {
            if (mfIsoPat3.m_major[index1] > 0.0)
              mfIsoPat3.m_major[index1] = GetChargedMass(mfIsoPat3.m_major[index1]);
            checked { ++index1; }
          }
          var num3 = 0;
          var num4 = checked (mfIsoPat3.m_minor.GetLength(0) - 1);
          var index2 = num3;
          while (index2 <= num4)
          {
            var num5 = 0;
            var num6 = checked (mfIsoPat3.m_minor.GetLength(1) - 1);
            var index3 = num5;
            while (index3 <= num6)
            {
              if (mfIsoPat3.m_minor[index2, index3] > 0.0)
                mfIsoPat3.m_minor[index2, index3] = GetChargedMass(mfIsoPat3.m_minor[index2, index3]);
              checked { ++index3; }
            }
            checked { ++index2; }
          }
          var num7 = 0;
          var num8 = checked (mfIsoPat3.dm_major.Length - 1);
          var index4 = num7;
          while (index4 <= num8)
          {
            mfIsoPat3.dm_major[index4] = mfIsoPat3.dm_major[index4] / (double) m_calc_cs;
            checked { ++index4; }
          }
        }
        mfIsoPat2 = mfIsoPat3;
      }
      catch (Exception ex)
      {
        ProjectData.SetProjectError(ex);
        var mfIsoPat3 = new MFIsoPat();
        mfIsoPat2 = mfIsoPat3;
        ProjectData.ClearProjectError();
      }
      return mfIsoPat2;
    }

    public bool ttlSearch(ref double[] m, ref double[] a, ref double[] sn, ref double max_abu, string in_file_name)
    {
      bool flag;
      try
      {
        data_m = m;
        data_a = a;
        data_sn = sn;
        data_cnt = data_m.Length;
        data_max_a = max_abu;
        tmp_major_hit_cnt = 0;
        tmp_total_hit_cnt = 0;
        data_ra = new double[checked (data_cnt - 1 + 1)];
        var num1 = 0;
        var num2 = checked (data_cnt - 1);
        var index = num1;
        while (index <= num2)
        {
          data_ra[index] = data_a[index] / data_max_a;
          checked { ++index; }
        }
        IPDB_log.AppendLine(DBSurveySearch(in_file_name));
        flag = true;
      }
      catch (Exception ex)
      {
        ProjectData.SetProjectError(ex);
        IPDB_log.AppendLine(ex.Message);
        flag = false;
        ProjectData.ClearProjectError();
      }
      finally
      {
        IPDB_log.AppendLine("error handler calls " + Conversions.ToString(diagnostic_error_handler_entries));
        IPDB_log.AppendLine("multi peaks options calls " + Conversions.ToString(diagnostic_multi_option_entry));
      }
      return flag;
    }

    private string DBSurveySearch(string in_file_name)
    {
      string str;
      try
      {
        if (m_matched_peaks_report)
        {
          tw_peaks = (TextWriter) new StreamWriter(ttlAI.GetFileName_A(in_file_name, "p_ipdb", TotalDeclarations.WordType.prefix, ".csv"));
          tw_peaks.WriteLine("ind,mz,intensity,sn,rel_int" + deli_out + "mf_ind" + deli_out + "mf" + deli_out + "m0" + deli_out + "search_m" + deli_out + "p" + deli_out + "major_ind" + deli_out + "minor_ind" + deli_out + "err_ppm");
        }
        tw_IPDB = (TextWriter) new StreamWriter(ttlAI.GetFileName_A(in_file_name, "s_ipdb", TotalDeclarations.WordType.prefix, ".csv"));
        tw_IPDB.WriteLine("mf_ind" + deli_out + "mf" + deli_out + "ec" + deli_out + "C" + deli_out + "H" + deli_out + "O" + deli_out + "NSP" + deli_out + "other" + deli_out + "search_m0" + deli_out + "search_m" + deli_out + MFIsoPatScore.Header(deli_out));
        if (memorize_best_matches)
        {
          IPDB_best_match = new MFIsoPat[checked (MFCount - 1 + 1)];
          IPDB_score = new MFIsoPatScore[checked (MFCount - 1 + 1)];
        }
        else
        {
          IPDB_best_match = (MFIsoPat[]) null;
          IPDB_score = (MFIsoPatScore[]) null;
        }
        m_roxy = m_IPDB_ec_filter.Length <= 0 ? (ROX_List) null : new ROX_List(m_IPDB_ec_filter);
        var num1 = 0;
        var num2 = checked (MFCount - 1);
        var rec_ind = num1;
        while (rec_ind <= num2)
        {
          SearchIPDBRecord(rec_ind);
          checked { ++rec_ind; }
        }
        str = "major peaks match " + Conversions.ToString(tmp_major_hit_cnt) + " minor peaks total match " + Conversions.ToString(checked (tmp_total_hit_cnt - tmp_major_hit_cnt)) + " multi peaks mf count " + Conversions.ToString(tmp_score_multi_hit_cnt);
      }
      catch (Exception ex)
      {
        ProjectData.SetProjectError(ex);
        str = ex.Message;
        ProjectData.ClearProjectError();
      }
      finally
      {
        if (tw_peaks != null)
        {
          tw_peaks.Close();
          tw_peaks = (TextWriter) null;
        }
        if (tw_IPDB != null)
        {
          tw_IPDB.Close();
          tw_IPDB = (TextWriter) null;
        }
      }
      return str;
    }

    private int SearchPeaks()
    {
      int num;
      try
      {
        tmp_search_hit_ind1 = 0;
        tmp_search_hit_ind2 = checked (data_cnt - 1);
        FindMRange(ref tmp_search_hit_ind1, ref tmp_search_hit_ind2);
        num = tmp_search_hit_ind1 > tmp_search_hit_ind2 ? 0 : checked (tmp_search_hit_ind2 - tmp_search_hit_ind1 + 1);
      }
      catch (Exception ex)
      {
        ProjectData.SetProjectError(ex);
        num = -1;
        ProjectData.ClearProjectError();
      }
      return num;
    }

    private void FindMRange(ref int MinInd, ref int MaxInd)
    {
      var index1 = checked (MinInd + MaxInd) / 2;
      if (index1 == MinInd)
      {
        if (Math.Abs(tmp_search_val - data_m[MinInd]) > tmp_search_val_tol)
          MinInd = MaxInd;
        if (Math.Abs(tmp_search_val - data_m[MaxInd]) <= tmp_search_val_tol)
          return;
        MaxInd = index1;
      }
      else if (data_m[index1] > tmp_search_val + tmp_search_val_tol)
      {
        MaxInd = index1;
        FindMRange(ref MinInd, ref MaxInd);
      }
      else if (data_m[index1] < tmp_search_val - tmp_search_val_tol)
      {
        MinInd = index1;
        FindMRange(ref MinInd, ref MaxInd);
      }
      else
      {
        var index2 = index1;
        bool flag1 = false;
        while (!flag1)
        {
          checked { --index2; }
          if (index2 < MinInd)
            flag1 = true;
          else if (Math.Abs(tmp_search_val - data_m[index2]) > tmp_search_val_tol)
            flag1 = true;
        }
        var index3 = index1;
        bool flag2 = false;
        while (!flag2)
        {
          checked { ++index3; }
          if (index3 > MaxInd)
            flag2 = true;
          else if (Math.Abs(tmp_search_val - data_m[index3]) > tmp_search_val_tol)
            flag2 = true;
        }
        MinInd = checked (index2 + 1);
        MaxInd = checked (index3 - 1);
      }
    }

    private int SearchIPDBRecord(int rec_ind)
    {
      int num1 = 0;
      if (!Information.IsNothing((object) m_roxy) && !ECFilter(MFS[rec_ind].MF))
      {
        num1 = -1;
      }
      else
      {
        score_template = MF_BRW.NewMF_FromTemplate_ForScoring(MFS[rec_ind]);
        match_template_min_ind = MF_BRW.NewMF_FromTemplate_ForMatchRange(MFS[rec_ind]);
        match_template_max_ind = MF_BRW.NewMF_FromTemplate_ForMatchRange(MFS[rec_ind]);
        var mfs = MFS;
        var index1 = rec_ind;
        var num2 = checked (mfs[index1].N_Count + mfs[index1].S_Count + mfs[index1].P_Count);
        tmp_s_ind = mfs[index1].m_search_ind;
        tmp_s_val = GetChargedMass(mfs[index1].m_major[tmp_s_ind]);
        var shortMf = ttlAI.TranslateLongToShortMF(mfs[index1].MF, "");
        var num3 = 0;
        var num4 = checked (MFCount_Major - 1);
        var index2 = num3;
        var num5 = 0;
        while (index2 <= num4)
        {
          tmp_search_val = GetChargedMass(mfs[index1].m_major[index2]);
          tmp_search_val_tol = tmp_search_val * m_ppm_tol / 1000000.0;
          if (SearchPeaks() > 0)
          {
            match_template_min_ind.m_major[index2] = (double) tmp_search_hit_ind1;
            match_template_max_ind.m_major[index2] = (double) tmp_search_hit_ind2;
            var tmpSearchHitInd1 = tmp_search_hit_ind1;
            var tmpSearchHitInd2 = tmp_search_hit_ind2;
            var i = tmpSearchHitInd1;
            while (i <= tmpSearchHitInd2)
            {
              if (data_sn[i] >= m_min_major_sn)
              {
                var num6 = RelativeError_PPM(data_m[i], tmp_search_val);
                if (tw_peaks != null)
                  tw_peaks.WriteLine(data_helper_GetPeakString(i) + deli_out + Conversions.ToString(mfs[index1].ID) + deli_out + shortMf + deli_out + Conversions.ToString(mfs[index1].m_major[index2]) + deli_out + Conversions.ToString(tmp_search_val) + deli_out + Conversions.ToString(mfs[index1].p_major[index2]) + deli_out + Conversions.ToString(index2) + deli_out + "-1" + deli_out + Conversions.ToString(num6));
                var major = score_template.m_major;
                var numArray = major;
                var index3 = index2;
                var index4 = index3;
                var num7 = major[index3] + 1.0;
                numArray[index4] = num7;
                checked { ++num5; }
                checked { ++tmp_major_hit_cnt; }
                checked { ++tmp_total_hit_cnt; }
              }
              checked { ++i; }
            }
          }
          checked { ++index2; }
        }
        if (num5 > 0)
        {
          var num6 = 0;
          var num7 = checked (MFCount_Major - 1);
          var index3 = num6;
          while (index3 <= num7)
          {
            var num8 = 0;
            var num9 = checked (MFCount_Minor - 1);
            var index4 = num8;
            while (index4 <= num9)
            {
              tmp_search_val = GetChargedMass(mfs[index1].m_minor[index3, index4]);
              tmp_search_val_tol = tmp_search_val * m_ppm_tol / 1000000.0;
              var num10 = SearchPeaks();
              if (num10 > 0)
              {
                score_template.m_minor[index3, index4] = (double) num10;
                match_template_min_ind.m_minor[index3, index4] = (double) tmp_search_hit_ind1;
                match_template_max_ind.m_minor[index3, index4] = (double) tmp_search_hit_ind2;
                var tmpSearchHitInd1 = tmp_search_hit_ind1;
                var tmpSearchHitInd2 = tmp_search_hit_ind2;
                var i = tmpSearchHitInd1;
                while (i <= tmpSearchHitInd2)
                {
                  if (data_sn[i] >= m_min_minor_sn)
                  {
                    var num11 = RelativeError_PPM(data_m[i], tmp_search_val);
                    if (tw_peaks != null)
                    {
                      tw_peaks.WriteLine(data_helper_GetPeakString(i) + deli_out + Conversions.ToString(mfs[index1].ID) + deli_out + shortMf + deli_out + Conversions.ToString(mfs[index1].m_minor[index3, index4]) + deli_out + Conversions.ToString(tmp_search_val) + deli_out + Conversions.ToString(mfs[index1].p_minor[index3, index4]) + deli_out + Conversions.ToString(index3) + deli_out + Conversions.ToString(index4) + deli_out + Conversions.ToString(num11));
                      checked { ++tmp_total_hit_cnt; }
                    }
                  }
                  checked { ++i; }
                }
              }
              checked { ++index4; }
            }
            checked { ++index3; }
          }
          var score = MF_BRW.MF_Get_Score(score_template, m_min_p_to_score);
          if (!IsUniqueScore(rec_ind, match_template_min_ind, match_template_max_ind, out var best_match))
            checked { ++tmp_score_multi_hit_cnt; }
          var MFExpected = MFS[rec_ind];
          var MFMeasured = best_match;
          ref var local1 = ref score.p_d1;
          ref var local2 = ref score.p_d2;
          ref var local3 = ref score.p_dinf;
          var flag = false;
          ref var local4 = ref flag;
          if (MF_BRW.MF_GetPDistance(MFExpected, MFMeasured, ref local1, ref local2, ref local3, ref local4) >= 0.0)
          {
            MF_BRW.MF_GetPSum_Scores(ref score, MFS[rec_ind], best_match);
            score.tma_m_err_ppm = RelativeError_PPM(best_match.m_major[tmp_s_ind], tmp_s_val);
            if (memorize_best_matches)
            {
              IPDB_best_match[rec_ind] = MF_BRW.NewMF_FromTemplate(best_match);
              IPDB_score[rec_ind] = score;
            }
            if (tw_IPDB != null && score.pa_mm_abs >= m_min_major_pa_mm_abs_2_report)
              tw_IPDB.WriteLine(Conversions.ToString(mfs[index1].ID) + deli_out + shortMf + deli_out + mfs[index1].EC + deli_out + Conversions.ToString(mfs[index1].C_Count) + deli_out + Conversions.ToString(mfs[index1].H_Count) + deli_out + Conversions.ToString(mfs[index1].O_Count) + deli_out + Conversions.ToString(num2) + deli_out + Conversions.ToString(mfs[index1].Other_Count) + deli_out + Conversions.ToString(mfs[index1].m_major[tmp_s_ind]) + deli_out + Conversions.ToString(tmp_s_val) + deli_out + score.ToString(deli_out));
          }
        }
      }
      return num1;
    }

    private bool IsUniqueScore(int rec_ind, MFIsoPat min_match_ind, MFIsoPat max_match_ind, out MFIsoPat best_match)
    {
      var ntuplesMixedRadix1 = new NTuples_MixedRadix();
      var ntuplesMixedRadix2 = new NTuples_MixedRadix();
      bool flag1;
      best_match = new MFIsoPat();
      try
      {
        var num1 = double.MaxValue;
        var num2 = double.MaxValue;
        best_match = IsUniqueScore_A(ref rec_ind, ref min_match_ind, ref max_match_ind);
        if (!Information.IsNothing((object) best_match.m_major))
        {
          flag1 = true;
        }
        else
        {
          checked { ++diagnostic_multi_option_entry; }
          ntuplesMixedRadix1.M = new int[checked (MFCount_Major - 1 + 1)];
          var num3 = 0;
          var num4 = checked (MFCount_Major - 1);
          var index1 = num3;
          while (index1 <= num4)
          {
            ntuplesMixedRadix1.M[index1] = max_match_ind.m_major[index1] < 0.0 ? 0 : checked ((int) Math.Round(unchecked (max_match_ind.m_major[index1] - min_match_ind.m_major[index1])));
            checked { ++index1; }
          }
          ntuplesMixedRadix2.M = new int[checked (MFCount_Major * MFCount_Minor - 1 + 1)];
          var num5 = 0;
          var num6 = checked (MFCount_Major - 1);
          var index2 = num5;
          while (index2 <= num6)
          {
            var num7 = 0;
            var num8 = checked (MFCount_Minor - 1);
            var index3 = num7;
            while (index3 <= num8)
            {
              int index4 = 0;
              ntuplesMixedRadix2.M[index4] = max_match_ind.m_minor[index2, index3] < 0.0 ? 0 : checked ((int) Math.Round(unchecked (max_match_ind.m_minor[index2, index3] - min_match_ind.m_minor[index2, index3])));
              checked { ++index4; }
              checked { ++index3; }
            }
            checked { ++index2; }
          }
          if (ntuplesMixedRadix1.Initialize())
          {
            do
            {
              var mf1 = MF_BRW.NewMF_FromTemplate(MFS[rec_ind]);
              var num7 = 0;
              var num8 = checked (MFCount_Major - 1);
              var index3 = num7;
              while (index3 <= num8)
              {
                mf1.m_major[index3] = 0.0;
                mf1.p_major[index3] = 0.0f;
                if (max_match_ind.m_major[index3] >= 0.0)
                {
                  var index4 = checked ((int) Math.Round(unchecked (min_match_ind.m_major[index3] + (double) ntuplesMixedRadix1.NTuple[index3])));
                  mf1.m_major[index3] = data_m[index4];
                  mf1.p_major[index3] = (float) data_a[index4];
                }
                checked { ++index3; }
              }
              if (ntuplesMixedRadix2.Initialize())
              {
                do
                {
                  var mf2 = MF_BRW.NewMF_FromTemplate(mf1);
                  var index4 = 0;
                  var num9 = 0;
                  var num10 = checked (MFCount_Major - 1);
                  var index5 = num9;
                  while (index5 <= num10)
                  {
                    var num11 = 0;
                    var num12 = checked (MFCount_Minor - 1);
                    var index6 = num11;
                    while (index6 <= num12)
                    {
                      mf2.m_minor[index5, index6] = 0.0;
                      mf2.p_minor[index5, index6] = 0.0f;
                      if (max_match_ind.m_minor[index5, index6] >= 0.0)
                      {
                        var index7 = checked ((int) Math.Round(unchecked (min_match_ind.m_minor[index5, index6] + (double) ntuplesMixedRadix2.NTuple[index4])));
                        mf2.m_minor[index5, index6] = data_m[index7];
                        mf2.p_minor[index5, index6] = (float) data_a[index7];
                      }
                      checked { ++index4; }
                      checked { ++index6; }
                    }
                    checked { ++index5; }
                  }
                  if (MF_BRW.MF_Abu2PseudoP(ref mf2))
                  {
                    var MFExpected = MFS[rec_ind];
                    var MFMeasured = mf2;
                    double num11 = 0;
                    ref var local1 = ref num11;
                    double num12 = 0;
                    ref var local2 = ref num12;
                    double num13 = 0;
                    ref var local3 = ref num13;
                    var flag2 = false;
                    ref var local4 = ref flag2;
                    var pdistance = MF_BRW.MF_GetPDistance(MFExpected, MFMeasured, ref local1, ref local2, ref local3, ref local4);
                    var num14 = Math.Abs(RelativeError_PPM(mf2.m_major[tmp_s_ind], tmp_s_val));
                    if (m_best_err)
                    {
                      if (num14 < num2)
                      {
                        num2 = num14;
                        num1 = pdistance;
                        best_match = MF_BRW.NewMF_FromTemplate(mf2);
                      }
                      else if (num14 == num2 && pdistance < num1)
                      {
                        num1 = pdistance;
                        best_match = MF_BRW.NewMF_FromTemplate(mf2);
                      }
                    }
                    else if (pdistance < num1)
                    {
                      num1 = pdistance;
                      best_match = MF_BRW.NewMF_FromTemplate(mf2);
                    }
                  }
                }
                while (ntuplesMixedRadix2.SetNext());
              }
            }
            while (ntuplesMixedRadix1.SetNext());
          }
          flag1 = false;
        }
      }
      catch (Exception ex)
      {
        ProjectData.SetProjectError(ex);
        checked { ++diagnostic_error_handler_entries; }
        flag1 = false;
        ProjectData.ClearProjectError();
      }
      return flag1;
    }

    private MFIsoPat IsUniqueScore_A(ref int rec_ind, ref MFIsoPat min_match_ind, ref MFIsoPat max_match_ind)
    {
      var mfIsoPat1 = new MFIsoPat();
      var mfIsoPat2 = new MFIsoPat();
      var mfIsoPat3 = new MFIsoPat();
      try
      {
        var mf = MF_BRW.NewMF_FromTemplate(MFS[rec_ind]);
        var num1 = 0;
        var num2 = checked (MFCount_Major - 1);
        var index1 = num1;
        while (index1 <= num2)
        {
          mf.m_major[index1] = 0.0;
          mf.p_major[index1] = 0.0f;
          if (max_match_ind.m_major[index1] >= 0.0)
          {
            if (min_match_ind.m_major[index1] != max_match_ind.m_major[index1])
            {
              mfIsoPat3 = mfIsoPat2;
              goto label_19;
            }
            else
            {
              var index2 = checked ((int) Math.Round(min_match_ind.m_major[index1]));
              if (data_sn[index2] >= m_min_major_sn)
              {
                mf.m_major[index1] = data_m[index2];
                mf.p_major[index1] = (float) data_a[index2];
              }
            }
          }
          var num3 = 0;
          var num4 = checked (MFCount_Minor - 1);
          var index3 = num3;
          while (index3 <= num4)
          {
            mf.m_minor[index1, index3] = 0.0;
            mf.p_minor[index1, index3] = 0.0f;
            if (max_match_ind.m_minor[index1, index3] >= 0.0)
            {
              if (min_match_ind.m_minor[index1, index3] != max_match_ind.m_minor[index1, index3])
              {
                mfIsoPat3 = mfIsoPat2;
                goto label_19;
              }
              else
              {
                var index2 = checked ((int) Math.Round(min_match_ind.m_minor[index1, index3]));
                mf.m_minor[index1, index3] = data_m[index2];
                mf.p_minor[index1, index3] = (float) data_a[index2];
              }
            }
            checked { ++index3; }
          }
          checked { ++index1; }
        }
        mfIsoPat3 = !MF_BRW.MF_Abu2PseudoP(ref mf) ? mfIsoPat2 : MF_BRW.NewMF_FromTemplate(mf);
      }
      catch (Exception ex)
      {
        ProjectData.SetProjectError(ex);
        checked { ++diagnostic_error_handler_entries; }
        mfIsoPat3 = mfIsoPat2;
        ProjectData.ClearProjectError();
      }
label_19:
      return mfIsoPat3;
    }

    private bool ECFilter(string mf)
    {
      var flag1 = true;
      var cannonMf = new CannonMF();
      bool flag2;
      try
      {
        if (m_roxy.z_cnt > 0)
        {
          flag2 = false;
        }
        else
        {
          if (cannonMf.SetMF(mf))
          {
            var num1 = 0;
            if (m_roxy.e_cnt > 0)
            {
              flag1 = false;
              var num2 = 0;
              var num3 = checked (m_roxy.e_cnt - 1);
              var index = num2;
              while (index <= num3)
              {
                if (string.Compare(m_roxy.e_ec[index], cannonMf.GetEC()) == 0)
                  flag1 = true;
                checked { ++index; }
              }
            }
            else
            {
              var num2 = 0;
              var num3 = checked (m_roxy.r_cnt - 1);
              var index1 = num2;
              while (index1 <= num3)
              {
                var elements = (Elements) Enum.Parse(typeof (Elements), m_roxy.r_elements[index1]);
                if (cannonMf.MMF[(int) elements] > 0)
                  checked { ++num1; }
                checked { ++index1; }
              }
              if (num1 < m_roxy.r_cnt)
                flag1 = false;
              if (flag1)
              {
                var num4 = 0;
                var num5 = 0;
                if (m_roxy.o_cnt > 0)
                {
                  var num6 = 0;
                  var num7 = checked (m_roxy.o_cnt - 1);
                  var index2 = num6;
                  while (index2 <= num7)
                  {
                    var elements = (Elements) Enum.Parse(typeof (Elements), m_roxy.o_elements[index2]);
                    if (cannonMf.MMF[(int) elements] > 0)
                    {
                      checked { ++num4; }
                      checked { num5 += cannonMf.MMF[unchecked ((int) elements)]; }
                    }
                    checked { ++index2; }
                  }
                  if (num4 <= 0)
                    flag1 = false;
                }
              }
              if (flag1)
              {
                var num4 = 0;
                var num5 = 0;
                var num6 = checked (m_roxy.x_cnt - 1);
                var index2 = num5;
                while (index2 <= num6)
                {
                  var elements = (Elements) Enum.Parse(typeof (Elements), m_roxy.x_elements[index2]);
                  if (cannonMf.MMF[(int) elements] > 0)
                    checked { ++num4; }
                  checked { ++index2; }
                }
                if (num4 > 0)
                  flag1 = false;
              }
            }
          }
          else
            flag1 = false;
          flag2 = flag1;
        }
      }
      catch (Exception ex)
      {
        ProjectData.SetProjectError(ex);
        flag2 = false;
        ProjectData.ClearProjectError();
      }
      return flag2;
    }
  }
}

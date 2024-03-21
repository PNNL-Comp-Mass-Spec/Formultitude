// Decompiled with JetBrains decompiler
// Type: TestFSDBSearch.TotalSupport.MF_BRW
// Assembly: TestFSDBSearch, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C36EBD2C-6652-4FDC-A74D-B15E46A79224
// Assembly location: F:\Documents\Projects\NikolaTolic\Formultitude\Lib\TestFSDBSearch.exe

using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Diagnostics;
using System.Text;

namespace TestFSDBSearch.TotalSupport
{
  public class MF_BRW
  {
    public static double MF_SCORE_MAJOR = 1.0;
    public static double MF_SCORE_MINOR = 0.1;
    public static double MF_SCORE_NOT_USED = -1.0;
    public static int MF_ID_NOT_ASSIGNED = -1;
    public static int MF_ID_SCORE = -2;

    [DebuggerNonUserCode]
    public MF_BRW()
    {
    }

    public static bool FromText(ref MFIsoPat mf, string str_rec, string deli = "\t")
    {
      string[] strArray = str_rec.Split(Conversions.ToChar(deli));
      bool flag;
      try
      {
        mf.ID = Conversions.ToInteger(strArray[0]);
        mf.MF = strArray[1];
        int index1 = 3;
        int num1 = 0;
        int num2 = checked (mf.m_major.Length - 1);
        int index2 = num1;
        while (index2 <= num2)
        {
          mf.m_major[index2] = double.Parse(strArray[index1]);
          int index3 = checked (index1 + 1);
          mf.p_major[index2] = float.Parse(strArray[index3]);
          index1 = checked (index3 + 1);
          mf.p_major_sum += mf.p_major[index2];
          checked { ++index2; }
        }
        int num3 = 0;
        int num4 = checked (mf.m_minor.GetLength(0) - 1);
        int index4 = num3;
        while (index4 <= num4)
        {
          int num5 = 0;
          int num6 = checked (mf.m_minor.GetLength(1) - 1);
          int index3 = num5;
          while (index3 <= num6)
          {
            mf.m_minor[index4, index3] = double.Parse(strArray[index1]);
            int index5 = checked (index1 + 1);
            mf.p_minor[index4, index3] = float.Parse(strArray[index5]);
            index1 = checked (index5 + 1);
            checked { ++index3; }
          }
          checked { ++index4; }
        }
        int num7 = 0;
        int num8 = checked (mf.m_major.Length - 2);
        int index6 = num7;
        while (index6 <= num8)
        {
          mf.dm_major[index6] = mf.m_major[checked (index6 + 1)] - mf.m_major[index6];
          mf.dp_major[index6] = mf.p_major[checked (index6 + 1)] - mf.p_major[index6];
          checked { ++index6; }
        }
        CannonMF cannonMf = new CannonMF();
        if (cannonMf.SetMF(ttlAI.TranslateShortToLongMF(mf.MF)))
        {
          mf.Total_Count = cannonMf.GetMF_AtomCount();
          mf.Other_Count = mf.Total_Count;
          mf.MF = cannonMf.GetMF(true);
          mf.EC = cannonMf.GetEC();
          mf.C_Count = cannonMf.MMF[6];
          checked { mf.Other_Count -= mf.C_Count; }
          mf.O_Count = cannonMf.MMF[8];
          checked { mf.Other_Count -= mf.O_Count; }
          mf.H_Count = cannonMf.MMF[1];
          checked { mf.Other_Count -= mf.H_Count; }
          mf.N_Count = cannonMf.MMF[7];
          checked { mf.Other_Count -= mf.N_Count; }
          mf.S_Count = cannonMf.MMF[16];
          checked { mf.Other_Count -= mf.S_Count; }
          mf.P_Count = cannonMf.MMF[15];
          checked { mf.Other_Count -= mf.P_Count; }
        }
        else
          mf.EC = string.Empty;
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

    public static string MFToFancyString(ref MFIsoPat mf)
    {
      StringBuilder stringBuilder = new StringBuilder("ID ");
      string message;
      try
      {
        if (mf.ID >= 0)
          stringBuilder.AppendLine(Conversions.ToString(mf.ID));
        else
          stringBuilder.AppendLine(string.Empty);
        stringBuilder.AppendLine("MF " + mf.MF);
        stringBuilder.AppendLine("EC " + mf.EC);
        int num1 = 0;
        int num2 = checked (mf.m_major.Length - 1);
        int index1 = num1;
        while (index1 <= num2)
        {
          if ((double) mf.p_major[index1] > 0.0)
          {
            stringBuilder.AppendLine("major peak (" + mf.m_major[index1].ToString("0.000000") + "," + mf.p_major[index1].ToString("0.0000") + ")");
            int num3 = 0;
            int num4 = checked (mf.m_minor.GetLength(1) - 1);
            int index2 = num3;
            while (index2 <= num4)
            {
              if ((double) mf.p_minor[index1, index2] > 0.0)
                stringBuilder.AppendLine("\t(" + mf.m_minor[index1, index2].ToString("0.000000") + "," + mf.p_minor[index1, index2].ToString("0.0000") + ")");
              checked { ++index2; }
            }
          }
          checked { ++index1; }
        }
        stringBuilder.AppendLine("major peaks probability sum  " + Conversions.ToString(mf.p_major_sum));
        stringBuilder.AppendLine("search mass  " + Conversions.ToString(mf.m_major[mf.m_search_ind]));
        stringBuilder.AppendLine("major peaks delta values");
        int num5 = 0;
        int num6 = checked (mf.m_major.Length - 2);
        int index3 = num5;
        while (index3 <= num6)
        {
          stringBuilder.AppendLine("\t(" + mf.dm_major[index3].ToString("0.000000") + "," + mf.dp_major[index3].ToString("0.0000") + ")");
          checked { ++index3; }
        }
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

    public static string MFToFancyString_PeaksOnly(ref MFIsoPat mf)
    {
      StringBuilder stringBuilder = new StringBuilder();
      string message;
      try
      {
        stringBuilder.Append("major peaks ");
        int index1 = 0;
        do
        {
          stringBuilder.Append("(" + mf.m_major[index1].ToString("0.000000") + "," + mf.p_major[index1].ToString("0.000") + ")");
          checked { ++index1; }
        }
        while (index1 <= 2);
        stringBuilder.AppendLine();
        int index2 = 0;
        do
        {
          stringBuilder.Append("minor peaks ");
          int index3 = 0;
          do
          {
            stringBuilder.Append("(" + mf.m_minor[index2, index3].ToString("0.000000") + "," + mf.p_minor[index2, index3].ToString("0.000") + ")");
            checked { ++index3; }
          }
          while (index3 <= 2);
          stringBuilder.AppendLine();
          checked { ++index2; }
        }
        while (index2 <= 2);
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

    public static string MFMinorPeaksToFancyString(double[,] m, double[,] a)
    {
      StringBuilder stringBuilder = new StringBuilder(string.Empty);
      string message;
      try
      {
        int num1 = 0;
        int num2 = checked (m.GetLength(0) - 1);
        int index1 = num1;
        while (index1 <= num2)
        {
          stringBuilder.Append("minor peaks ");
          int num3 = 0;
          int num4 = checked (m.GetLength(1) - 1);
          int index2 = num3;
          while (index2 <= num4)
          {
            stringBuilder.Append("(" + m[index1, index2].ToString(Conversions.ToString(32)) + "," + a[index1, index2].ToString("0.000") + ")");
            checked { ++index2; }
          }
          stringBuilder.AppendLine();
          checked { ++index1; }
        }
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

    public static string MFMinorPeaksAbuToDeliString(double[,] a, string deli = "\t")
    {
      StringBuilder stringBuilder = new StringBuilder(string.Empty);
      string message;
      try
      {
        int num1 = 0;
        int num2 = checked (a.GetLength(0) - 1);
        int index1 = num1;
        while (index1 <= num2)
        {
          int num3 = 0;
          int num4 = checked (a.GetLength(1) - 1);
          int index2 = num3;
          while (index2 <= num4)
          {
            stringBuilder.Append(Conversions.ToString(a[index1, index2]) + deli);
            checked { ++index2; }
          }
          checked { ++index1; }
        }
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

    public static string MFToShortString(ref MFIsoPat mf, string deli = "\t")
    {
      string str;
      try
      {
        if (Versioned.IsNumeric((object) deli))
        {
          int totalWidth = int.Parse(deli);
          str = mf.m_major[mf.m_search_ind].ToString("0.000000").PadLeft(12) + ttlAI.TranslateLongToShortMF(mf.MF, "").PadLeft(totalWidth) + mf.EC.PadLeft(totalWidth);
        }
        else
          str = mf.m_major[mf.m_search_ind].ToString("0.000000") + deli + ttlAI.TranslateLongToShortMF(mf.MF, "") + deli + mf.EC;
      }
      catch (Exception ex)
      {
        ProjectData.SetProjectError(ex);
        str = ex.Message;
        ProjectData.ClearProjectError();
      }
      return str;
    }

    public static string MFToDelimitedString_PeaksOnly(ref MFIsoPat mf, string deli = "\t")
    {
      StringBuilder stringBuilder = new StringBuilder();
      string message;
      try
      {
        int num1 = 0;
        int num2 = checked (mf.m_major.Length - 1);
        int index1 = num1;
        while (index1 <= num2)
        {
          stringBuilder.Append(Conversions.ToString(mf.m_major[index1]) + deli + Conversions.ToString(mf.p_major[index1]) + deli);
          checked { ++index1; }
        }
        int num3 = 0;
        int num4 = checked (mf.m_major.Length - 1);
        int index2 = num3;
        while (index2 <= num4)
        {
          int num5 = 0;
          int num6 = checked (mf.m_minor.GetLength(1) - 1);
          int index3 = num5;
          while (index3 <= num6)
          {
            stringBuilder.Append(Conversions.ToString(mf.m_minor[index2, index3]) + deli + Conversions.ToString(mf.p_minor[index2, index3]) + deli);
            checked { ++index3; }
          }
          checked { ++index2; }
        }
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

    public static string MFToDelimitedString_Full(ref MFIsoPat mf, string deli = "\t")
    {
      StringBuilder stringBuilder = new StringBuilder(Conversions.ToString(mf.ID) + deli + ttlAI.TranslateLongToShortMF(mf.MF, "") + deli + mf.EC);
      string message;
      try
      {
        stringBuilder.Append(deli + Conversions.ToString(mf.C_Count));
        stringBuilder.Append(deli + Conversions.ToString(mf.H_Count));
        stringBuilder.Append(deli + Conversions.ToString(mf.O_Count));
        stringBuilder.Append(deli + Conversions.ToString(mf.N_Count));
        stringBuilder.Append(deli + Conversions.ToString(mf.S_Count));
        stringBuilder.Append(deli + Conversions.ToString(mf.P_Count));
        stringBuilder.Append(deli + Conversions.ToString(mf.Other_Count));
        stringBuilder.Append(deli + Conversions.ToString(mf.m_search_ind));
        stringBuilder.Append(deli + MF_BRW.MFToDelimitedString_PeaksOnly(ref mf, "\t"));
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

    public static string MFToDelimitedString_DB(ref MFIsoPat mf, string deli = "\t")
    {
      StringBuilder stringBuilder = new StringBuilder(Conversions.ToString(mf.ID) + deli + ttlAI.TranslateLongToShortMF(mf.MF, "") + deli + mf.EC);
      string message;
      try
      {
        stringBuilder.Append(deli + MF_BRW.MFToDelimitedString_PeaksOnly(ref mf, "\t"));
        int num1 = 0;
        int num2 = checked (mf.m_major.Length - 2);
        int index = num1;
        while (index <= num2)
        {
          stringBuilder.Append(deli + Conversions.ToString(mf.dm_major[index]) + deli + Conversions.ToString(mf.dp_major[index]));
          checked { ++index; }
        }
        stringBuilder.Append(deli + Conversions.ToString(mf.p_major_sum));
        stringBuilder.Append(deli + "0");
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

    public static bool NewMF(ref MFIsoPat mf, int major_cnt, int minor_cnt)
    {
      bool flag;
      try
      {
        mf.ID = -1;
        mf.m_major = new double[checked (major_cnt - 1 + 1)];
        mf.p_major = new float[checked (major_cnt - 1 + 1)];
        mf.m_minor = new double[checked (major_cnt - 1 + 1), checked (minor_cnt - 1 + 1)];
        mf.p_minor = new float[checked (major_cnt - 1 + 1), checked (minor_cnt - 1 + 1)];
        mf.dm_major = new double[checked (major_cnt - 2 + 1)];
        mf.dp_major = new float[checked (major_cnt - 2 + 1)];
        mf.p_major_sum = 0.0f;
        mf.Total_Count = 0;
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

    public static MFIsoPat NewMF_FromTemplate(MFIsoPat mf)
    {
      MFIsoPat mfIsoPat1 = new MFIsoPat();
      int length1 = mf.m_major.Length;
      int length2 = mf.m_minor.GetLength(1);
      MFIsoPat mfIsoPat2;
      try
      {
        mfIsoPat1.ID = mf.ID;
        mfIsoPat1.MF = mf.MF;
        mfIsoPat1.EC = mf.EC;
        mfIsoPat1.C_Count = mf.C_Count;
        mfIsoPat1.H_Count = mf.H_Count;
        mfIsoPat1.N_Count = mf.N_Count;
        mfIsoPat1.O_Count = mf.O_Count;
        mfIsoPat1.S_Count = mf.S_Count;
        mfIsoPat1.P_Count = mf.P_Count;
        mfIsoPat1.Other_Count = mf.Other_Count;
        mfIsoPat1.Total_Count = mf.Total_Count;
        mfIsoPat1.m_major = new double[checked (length1 - 1 + 1)];
        mfIsoPat1.p_major = new float[checked (length1 - 1 + 1)];
        mfIsoPat1.m_minor = new double[checked (length1 - 1 + 1), checked (length2 - 1 + 1)];
        mfIsoPat1.p_minor = new float[checked (length1 - 1 + 1), checked (length2 - 1 + 1)];
        if (length1 > 1)
        {
          mfIsoPat1.dm_major = new double[checked (length1 - 2 + 1)];
          mfIsoPat1.dp_major = new float[checked (length1 - 2 + 1)];
        }
        int num1 = 0;
        int num2 = checked (length1 - 1);
        int index1 = num1;
        while (index1 <= num2)
        {
          mfIsoPat1.m_major[index1] = mf.m_major[index1];
          mfIsoPat1.p_major[index1] = mf.p_major[index1];
          int num3 = 0;
          int num4 = checked (length2 - 1);
          int index2 = num3;
          while (index2 <= num4)
          {
            mfIsoPat1.m_minor[index1, index2] = mf.m_minor[index1, index2];
            mfIsoPat1.p_minor[index1, index2] = mf.p_minor[index1, index2];
            checked { ++index2; }
          }
          checked { ++index1; }
        }
        if (length1 > 1)
        {
          int num3 = 0;
          int num4 = checked (length1 - 2);
          int index2 = num3;
          while (index2 <= num4)
          {
            mfIsoPat1.dm_major[index2] = mf.dm_major[index2];
            mfIsoPat1.dp_major[index2] = mf.dp_major[index2];
            checked { ++index2; }
          }
        }
        mfIsoPat1.m_search_ind = mf.m_search_ind;
        mfIsoPat1.p_major_sum = mf.p_major_sum;
        mfIsoPat2 = mfIsoPat1;
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

    public static MFIsoPat NewMF_FromTemplate_ForScoring(MFIsoPat mf)
    {
      MFIsoPat mfIsoPat1 = new MFIsoPat();
      int length1 = mf.m_major.Length;
      int length2 = mf.m_minor.GetLength(1);
      MFIsoPat mfIsoPat2;
      try
      {
        MFIsoPat mfIsoPat3 = MF_BRW.NewMF_FromTemplate(mf);
        int num1 = 0;
        int num2 = checked (length1 - 1);
        int index1 = num1;
        while (index1 <= num2)
        {
          mfIsoPat3.m_major[index1] = mf.m_major[index1] <= 0.0 ? MF_BRW.MF_SCORE_NOT_USED : 0.0;
          int num3 = 0;
          int num4 = checked (length2 - 1);
          int index2 = num3;
          while (index2 <= num4)
          {
            mfIsoPat3.m_minor[index1, index2] = mf.m_minor[index1, index2] <= 0.0 ? MF_BRW.MF_SCORE_NOT_USED : 0.0;
            checked { ++index2; }
          }
          checked { ++index1; }
        }
        mfIsoPat3.ID = MF_BRW.MF_ID_SCORE;
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

    public static MFIsoPat NewMF_FromTemplate_ForMatchRange(MFIsoPat mf)
    {
      MFIsoPat mfIsoPat1 = new MFIsoPat();
      int length1 = mf.m_major.Length;
      int length2 = mf.m_minor.GetLength(1);
      MFIsoPat mfIsoPat2;
      try
      {
        MFIsoPat mfIsoPat3 = MF_BRW.NewMF_FromTemplate(mf);
        int num1 = 0;
        int num2 = checked (length1 - 1);
        int index1 = num1;
        while (index1 <= num2)
        {
          mfIsoPat3.m_major[index1] = MF_BRW.MF_SCORE_NOT_USED;
          int num3 = 0;
          int num4 = checked (length2 - 1);
          int index2 = num3;
          while (index2 <= num4)
          {
            mfIsoPat3.m_minor[index1, index2] = MF_BRW.MF_SCORE_NOT_USED;
            checked { ++index2; }
          }
          checked { ++index1; }
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

    public static MFIsoPatScore MF_Get_Score(MFIsoPat mf, double min_p_4_max_score = 0.0)
    {
      MFIsoPatScore mfIsoPatScore1 = new MFIsoPatScore();
      MFIsoPatScore mfIsoPatScore2;
      try
      {
        if (mf.ID != MF_BRW.MF_ID_SCORE)
          throw new ArgumentException();
        int length1 = mf.m_major.Length;
        int length2 = mf.m_minor.GetLength(1);
        bool flag = mf.m_major[mf.m_search_ind] > 0.0;
        int num1 = 0;
        int num2 = checked (length1 - 1);
        int index1 = num1;
        while (index1 <= num2)
        {
          if (mf.m_major[index1] >= 0.0)
          {
            if ((double) mf.p_major[index1] >= min_p_4_max_score)
              mfIsoPatScore1.pa_mm_max += MF_BRW.MF_SCORE_MAJOR;
            if (mf.m_major[index1] > 0.0)
            {
              checked { ++mfIsoPatScore1.major_count; }
              if (flag)
                mfIsoPatScore1.pa_mm_abs += MF_BRW.MF_SCORE_MAJOR;
              if (mf.m_major[index1] > 1.0)
                checked { ++mfIsoPatScore1.major_multi_count; }
            }
          }
          int num3 = 0;
          int num4 = checked (length2 - 1);
          int index2 = num3;
          while (index2 <= num4)
          {
            if (mf.m_minor[index1, index2] >= 0.0)
            {
              if ((double) mf.p_minor[index1, index2] >= min_p_4_max_score)
                mfIsoPatScore1.pa_mm_max += MF_BRW.MF_SCORE_MINOR;
              if (mf.m_minor[index1, index2] > 0.0)
              {
                checked { ++mfIsoPatScore1.minor_count; }
                mfIsoPatScore1.pa_mm_abs += MF_BRW.MF_SCORE_MINOR;
                if (mf.m_minor[index1, index2] > 1.0)
                  checked { ++mfIsoPatScore1.minor_multi_count; }
              }
            }
            checked { ++index2; }
          }
          checked { ++index1; }
        }
        mfIsoPatScore2 = mfIsoPatScore1;
      }
      catch (Exception ex)
      {
        ProjectData.SetProjectError(ex);
          var mfIsoPatScore3 = new MFIsoPatScore();;
        mfIsoPatScore2 = mfIsoPatScore3;
        ProjectData.ClearProjectError();
      }
      return mfIsoPatScore2;
    }

      public static double MF_GetPDistance(MFIsoPat MFExpected, MFIsoPat MFMeasured, ref double d_1, ref double d_2, ref double d_inf)
      {
          bool UseMinorPeaks = false;
          var result = MF_GetPDistance(MFExpected, MFMeasured, ref d_1, ref d_2, ref d_inf, ref UseMinorPeaks);
          return result;
      }

      public static double MF_GetPDistance(MFIsoPat MFExpected, MFIsoPat MFMeasured, ref double d_1, ref double d_2, ref double d_inf, ref bool UseMinorPeaks)
    {
      d_1 = 0.0;
      d_2 = 0.0;
      d_inf = 0.0;
      int length1 = MFExpected.m_major.Length;
      int length2 = MFExpected.m_minor.GetLength(1);
      int num1 = 0;
      int num2 = checked (length1 - 1);
      int index1 = num1;
      while (index1 <= num2)
      {
        double x1 = (double) Math.Abs(MFExpected.p_major[index1] - MFMeasured.p_major[index1]);
        d_1 += x1;
        d_2 += Math.Pow(x1, 2.0);
        if (x1 > d_inf)
          d_inf = x1;
        if (UseMinorPeaks)
        {
          int num3 = 0;
          int num4 = checked (length2 - 1);
          int index2 = num3;
          while (index2 <= num4)
          {
            if (MFExpected.m_minor[index1, index2] != MFExpected.m_major[index1])
            {
              double x2 = (double) Math.Abs(MFExpected.p_minor[index1, index2] - MFMeasured.p_minor[index1, index2]);
              d_1 += x2;
              d_2 += Math.Pow(x2, 2.0);
              if (x2 > d_inf)
                d_inf = x2;
            }
            checked { ++index2; }
          }
        }
        checked { ++index1; }
      }
      d_2 = Math.Sqrt(d_2);
      return d_2;
    }

    internal static bool MF_GetPSum_Scores(ref MFIsoPatScore score, MFIsoPat MFExpected, MFIsoPat MFMeasured)
    {
      int length1 = MFExpected.m_major.Length;
      int length2 = MFExpected.m_minor.GetLength(1);
      score.pa_sum_abs = 0.0;
      score.pa_sum_max = 0.0;
      int num1 = 0;
      int num2 = checked (length1 - 1);
      int index1 = num1;
      while (index1 <= num2)
      {
        if (MFExpected.m_major[index1] > 0.0)
        {
          score.pa_sum_max += (double) MFExpected.p_major[index1];
          if (MFMeasured.m_major[index1] > 0.0)
          {
            score.pa_sum_abs += (double) MFExpected.p_major[index1];
            int num3 = 0;
            int num4 = checked (length2 - 1);
            int index2 = num3;
            while (index2 <= num4)
            {
              if (MFExpected.m_minor[index1, index2] > 0.0)
              {
                score.pa_sum_max += (double) MFExpected.p_minor[index1, index2];
                if (MFExpected.m_minor[index1, index2] != MFExpected.m_major[index1] && MFMeasured.m_minor[index1, index2] > 0.0)
                  score.pa_sum_abs += (double) MFExpected.p_minor[index1, index2];
              }
              checked { ++index2; }
            }
          }
        }
        checked { ++index1; }
      }
      return true;
    }

    public static bool MF_Abu2PseudoP(ref MFIsoPat mf)
    {
      double num1 = 0.0;
      int length1 = mf.m_major.Length;
      int length2 = mf.m_minor.GetLength(1);
      int num2 = 0;
      int num3 = checked (length1 - 1);
      int index1 = num2;
      while (index1 <= num3)
      {
        if ((double) mf.p_major[index1] > num1)
          num1 = (double) mf.p_major[index1];
        int num4 = 0;
        int num5 = checked (length2 - 1);
        int index2 = num4;
        while (index2 <= num5)
        {
          if ((double) mf.p_minor[index1, index2] > num1)
            num1 = (double) mf.p_minor[index1, index2];
          checked { ++index2; }
        }
        checked { ++index1; }
      }
      if (num1 <= 0.0)
        return false;
      int num6 = 0;
      int num7 = checked (length1 - 1);
      int num8 = num6;
      while (num8 <= num7)
      {
        float[] pMajor = mf.p_major;
        float[] numArray1 = pMajor;
        int index2 = num8;
        int index3 = index2;
        double num4 = (double) (pMajor[index2] / (float) num1);
        numArray1[index3] = (float) num4;
        int num5 = 0;
        int num9 = checked (length2 - 1);
        int num10 = num5;
        while (num10 <= num9)
        {
          float[,] pMinor = mf.p_minor;
          float[,] numArray2 = pMinor;
          int index4 = num8;
          int index5 = index4;
          int index6 = num10;
          int index7 = index6;
          double num11 = (double) (pMinor[index4, index6] / (float) num1);
          numArray2[index5, index7] = (float) num11;
          checked { ++num10; }
        }
        checked { ++num8; }
      }
      return true;
    }

    public static bool MF_Range(MFIsoPat mf, ref double min_major, ref double max_major)
    {
      bool flag;
      try
      {
        min_major = double.MaxValue;
        max_major = double.MinValue;
        int num1 = 0;
        int num2 = checked (mf.m_major.Length - 1);
        int index = num1;
        while (index <= num2)
        {
          if (mf.m_major[index] < min_major)
            min_major = mf.m_major[index];
          if (mf.m_major[index] > max_major)
            max_major = mf.m_major[index];
          checked { ++index; }
        }
        flag = min_major <= max_major;
      }
      catch (Exception ex)
      {
        ProjectData.SetProjectError(ex);
        flag = false;
        ProjectData.ClearProjectError();
      }
      return flag;
    }

    public static string Header_MFIsoPat(MFIsoPat mf, string deli = "\t")
    {
      int length1 = mf.m_major.Length;
      int length2 = mf.m_minor.GetLength(1);
      StringBuilder stringBuilder = new StringBuilder("id" + deli + nameof (mf) + deli + "ec" + deli + "C_cnt" + deli + "H_cnt" + deli + "O_cnt" + deli + "N_cnt" + deli + "S_cnt" + deli + "P_cnt" + deli + "Other_cnt" + deli + "search_ind");
      string message;
      try
      {
        int num1 = 0;
        int num2 = checked (length1 - 1);
        int num3 = num1;
        while (num3 <= num2)
        {
          stringBuilder.Append(deli + "the_m_mjr" + Conversions.ToString(num3) + deli + "the_p_mjr" + Conversions.ToString(num3));
          checked { ++num3; }
        }
        int num4 = 0;
        int num5 = checked (length1 - 1);
        int num6 = num4;
        while (num6 <= num5)
        {
          int num7 = 0;
          int num8 = checked (length2 - 1);
          int num9 = num7;
          while (num9 <= num8)
          {
            stringBuilder.Append(deli + "the_m_mnr" + Conversions.ToString(num6) + "," + Conversions.ToString(num9) + deli + "the_p_mnr" + Conversions.ToString(num6) + "," + Conversions.ToString(num9));
            checked { ++num9; }
          }
          checked { ++num6; }
        }
        stringBuilder.Append(deli + "mjr_cnt" + deli + "mnr_cnt" + deli + "search_m" + deli + "search_a" + deli + "adj_dm");
        int num10 = 0;
        int num11 = checked (length1 - 1);
        int num12 = num10;
        while (num12 <= num11)
        {
          stringBuilder.Append(deli + "adj_m_mjr" + Conversions.ToString(num12) + deli + "adj_p_mjr" + Conversions.ToString(num12));
          checked { ++num12; }
        }
        int num13 = 0;
        int num14 = checked (length1 - 1);
        int num15 = num13;
        while (num15 <= num14)
        {
          int num7 = 0;
          int num8 = checked (length2 - 1);
          int num9 = num7;
          while (num9 <= num8)
          {
            stringBuilder.Append(deli + "adj_m_mnr" + Conversions.ToString(num15) + "," + Conversions.ToString(num9) + deli + "adj_p_mnr" + Conversions.ToString(num15) + "," + Conversions.ToString(num9));
            checked { ++num9; }
          }
          checked { ++num15; }
        }
        int num16 = 0;
        int num17 = checked (length1 - 1);
        int num18 = num16;
        while (num18 <= num17)
        {
          stringBuilder.Append(deli + "exp_m_mjr" + Conversions.ToString(num18) + deli + "exp_p_mjr" + Conversions.ToString(num18));
          checked { ++num18; }
        }
        int num19 = 0;
        int num20 = checked (length1 - 1);
        int num21 = num19;
        while (num21 <= num20)
        {
          int num7 = 0;
          int num8 = checked (length2 - 1);
          int num9 = num7;
          while (num9 <= num8)
          {
            stringBuilder.Append(deli + "exp_m_mnr" + Conversions.ToString(num21) + "," + Conversions.ToString(num9) + deli + "exp_p_mnr" + Conversions.ToString(num21) + "," + Conversions.ToString(num9));
            checked { ++num9; }
          }
          checked { ++num21; }
        }
        int num22 = 0;
        int num23 = checked (length1 - 1);
        int num24 = num22;
        while (num24 <= num23)
        {
          int num7 = 0;
          int num8 = checked (length2 - 1);
          int num9 = num7;
          while (num9 <= num8)
          {
            stringBuilder.Append(deli + "exp_a_mnr" + Conversions.ToString(num24) + "," + Conversions.ToString(num9));
            checked { ++num9; }
          }
          checked { ++num24; }
        }
        stringBuilder.Append(deli + "target" + deli + "score_mjr" + deli + "score_mnr");
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
  }
}

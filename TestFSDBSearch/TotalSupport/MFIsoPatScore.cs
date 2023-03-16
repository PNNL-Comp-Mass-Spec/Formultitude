// Decompiled with JetBrains decompiler
// Type: TestFSDBSearch.TotalSupport.MFIsoPatScore
// Assembly: TestFSDBSearch, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C36EBD2C-6652-4FDC-A74D-B15E46A79224
// Assembly location: F:\Documents\Projects\NikolaTolic\Formularity\Lib\TestFSDBSearch.exe

using Microsoft.VisualBasic.CompilerServices;
using System.Text;

namespace TestFSDBSearch.TotalSupport
{
  public struct MFIsoPatScore
  {
    public double pa_mm_abs;
    public double pa_mm_max;
    public int major_count;
    public int minor_count;
    public int major_multi_count;
    public int minor_multi_count;
    public double p_d1;
    public double p_d2;
    public double p_dinf;
    public double pa_sum_abs;
    public double pa_sum_max;
    public double tma_m_err_ppm;

    public string ToString(string deli = "\t")
    {
      StringBuilder stringBuilder = new StringBuilder(this.pa_mm_abs.ToString());
      stringBuilder.Append(deli + Conversions.ToString(this.pa_mm_max));
      if (this.pa_mm_max > 0.0)
        stringBuilder.Append(deli + Conversions.ToString(this.pa_mm_abs / this.pa_mm_max));
      else
        stringBuilder.Append(deli + "na");
      stringBuilder.Append(deli + Conversions.ToString(this.major_count));
      stringBuilder.Append(deli + Conversions.ToString(this.minor_count));
      stringBuilder.Append(deli + Conversions.ToString(this.major_multi_count));
      stringBuilder.Append(deli + Conversions.ToString(this.minor_multi_count));
      stringBuilder.Append(deli + Conversions.ToString(this.p_d1));
      stringBuilder.Append(deli + Conversions.ToString(this.p_d2));
      stringBuilder.Append(deli + Conversions.ToString(this.p_dinf));
      stringBuilder.Append(deli + Conversions.ToString(this.pa_sum_abs));
      stringBuilder.Append(deli + Conversions.ToString(this.pa_sum_max));
      if (this.pa_sum_max > 0.0)
        stringBuilder.Append(deli + Conversions.ToString(this.pa_sum_abs / this.pa_sum_max));
      else
        stringBuilder.Append(deli + "na");
      stringBuilder.Append(deli + Conversions.ToString(this.tma_m_err_ppm));
      return stringBuilder.ToString();
    }

    public static string Header(string deli = "\t")
    {
      StringBuilder stringBuilder = new StringBuilder("pa_mm_abs");
      stringBuilder.Append(deli + "pa_mm_max");
      stringBuilder.Append(deli + "pa_mm_rel");
      stringBuilder.Append(deli + "major_count");
      stringBuilder.Append(deli + "minor_count");
      stringBuilder.Append(deli + "major_multi_count");
      stringBuilder.Append(deli + "minor_multi_count");
      stringBuilder.Append(deli + "p_d1");
      stringBuilder.Append(deli + "p_d2");
      stringBuilder.Append(deli + "p_dinf");
      stringBuilder.Append(deli + "pa_sum_abs");
      stringBuilder.Append(deli + "pa_sum_max");
      stringBuilder.Append(deli + "pa_sum_rel");
      stringBuilder.Append(deli + "tma_m_err_ppm");
      return stringBuilder.ToString();
    }
  }
}

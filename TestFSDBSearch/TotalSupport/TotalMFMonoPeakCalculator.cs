// Decompiled with JetBrains decompiler
// Type: TestFSDBSearch.TotalSupport.TotalMFMonoPeakCalculator
// Assembly: TestFSDBSearch, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C36EBD2C-6652-4FDC-A74D-B15E46A79224
// Assembly location: F:\Documents\Projects\NikolaTolic\Formultitude\Lib\TestFSDBSearch.exe

using Microsoft.VisualBasic.CompilerServices;
using System;

namespace TestFSDBSearch.TotalSupport
{
  public class TotalMFMonoPeakCalculator
  {
    private IonizationMethod m_calc_ionization;
    private string m_calc_adduct;
    private EPolarity m_calc_polarity;
    private double m_calc_ccm;
    private int m_calc_cs;
    private int m_adduct_sign;
    private double m_adduct_m0;
    private CannonMF m_mf;
    private string m_charged_mass_calc_descriptive;
    private string m_charged_mass_calc_numeric;

    public TotalMFMonoPeakCalculator()
    {
      this.m_calc_ionization = IonizationMethod.none;
      this.m_calc_adduct = string.Empty;
      this.m_calc_polarity = EPolarity.Neutral;
      this.m_calc_ccm = 0.0;
      this.m_calc_cs = 1;
      this.m_adduct_m0 = 0.0;
      this.m_mf = (CannonMF) null;
    }

    public int CS
    {
      get
      {
        return this.m_calc_cs;
      }
      set
      {
        this.m_calc_cs = Math.Abs(value);
        if (!this.SetCalculation())
          throw new ArgumentException();
      }
    }

    public IonizationMethod Ionization
    {
      get
      {
        return this.m_calc_ionization;
      }
      set
      {
        this.m_calc_ionization = value;
        if (!this.SetCalculation())
          throw new ArgumentException();
      }
    }

    public EPolarity Polarity
    {
      get
      {
        return this.m_calc_polarity;
      }
    }

    public string Adduct
    {
      get
      {
        return this.m_calc_adduct;
      }
      set
      {
        this.m_calc_adduct = value.Replace(" ", "");
        if (this.m_calc_adduct.Length > 0)
        {
          if (Operators.CompareString(this.m_calc_adduct.Substring(0, 1), "-", false) == 0)
          {
            this.m_adduct_sign = -1;
            this.m_calc_adduct = value.Replace("-", "");
          }
          else
            this.m_adduct_sign = 1;
        }
        if (!this.SetCalculation())
          throw new ArgumentException();
      }
    }

    public double CCM
    {
      get
      {
        return this.m_calc_ccm;
      }
    }

    public string ChargedMassFormula_Descriptive
    {
      get
      {
        return this.m_charged_mass_calc_descriptive;
      }
    }

    public string ChargedMassFormula_Numeric
    {
      get
      {
        return this.m_charged_mass_calc_numeric;
      }
    }

    public bool SetCalculation()
    {
      bool flag;
      try
      {
        switch (this.m_calc_ionization)
        {
          case IonizationMethod.none:
            this.m_calc_polarity = EPolarity.Neutral;
            this.m_calc_ccm = 0.0;
            break;
          case IonizationMethod.proton_detachment:
            this.m_calc_polarity = EPolarity.Negative;
            this.m_calc_ccm = 1.00727646688;
            break;
          case IonizationMethod.proton_attachment:
            this.m_calc_polarity = EPolarity.Positive;
            this.m_calc_ccm = 1.00727646688;
            break;
          case IonizationMethod.electron_detachment:
            this.m_calc_polarity = EPolarity.Positive;
            this.m_calc_ccm = -0.00054857990945;
            break;
          case IonizationMethod.electron_attachment:
            this.m_calc_polarity = EPolarity.Negative;
            this.m_calc_ccm = -0.00054857990945;
            break;
        }
        if (this.m_mf == null)
        {
          this.m_mf = new CannonMF();
          if (!this.m_mf.ReadElementsFromFile("Isotope.inf"))
          {
            flag = false;
            goto label_16;
          }
        }
        this.m_adduct_m0 = 0.0;
        if (this.m_calc_adduct.Length > 0)
        {
          if (!this.m_mf.SetMF(ttlAI.TranslateShortToLongMF(this.m_calc_adduct)))
          {
            flag = false;
            goto label_16;
          }
          else
            this.m_adduct_m0 = this.m_mf.GetMF_Mono0Mass();
        }
        flag = this.SetFormulaDescriptions();
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
      if (this.m_calc_cs > 0)
        return (m0 + (double) this.m_adduct_sign * this.m_adduct_m0) / (double) this.m_calc_cs + (double) this.m_calc_polarity * this.m_calc_ccm;
      return m0;
    }

    public double GetNeutralMass_MF(string mf)
    {
      double num;
      try
      {
        num = !this.m_mf.SetMF(ttlAI.TranslateShortToLongMF(mf)) ? -1.0 : this.m_mf.GetMF_Mono0Mass();
      }
      catch (Exception ex)
      {
        ProjectData.SetProjectError(ex);
        num = -1.0;
        ProjectData.ClearProjectError();
      }
      return num;
    }

    public double GetChargedMass_MF(string mf)
    {
      double num;
      try
      {
        num = !this.m_mf.SetMF(ttlAI.TranslateShortToLongMF(mf)) ? -1.0 : this.GetChargedMass(this.m_mf.GetMF_Mono0Mass());
      }
      catch (Exception ex)
      {
        ProjectData.SetProjectError(ex);
        num = -1.0;
        ProjectData.ClearProjectError();
      }
      return num;
    }

    private bool SetFormulaDescriptions()
    {
      string str1 = "M";
      string str2 = "+";
      bool flag;
      try
      {
        string str3 = ttlAI.AI_IonizationMethod_Enum2String(this.m_calc_ionization);
        if (this.m_adduct_m0 > 0.0)
        {
          if (this.m_adduct_sign < 0)
            str2 = "-";
        }
        else
          str2 = string.Empty;
        this.m_charged_mass_calc_descriptive = str1 + str2 + this.m_calc_adduct;
        if (this.m_calc_cs > 1)
        {
          if (this.m_charged_mass_calc_descriptive.Length > 1)
            this.m_charged_mass_calc_descriptive = "(" + this.m_charged_mass_calc_descriptive + ")";
          this.m_charged_mass_calc_descriptive = this.m_charged_mass_calc_descriptive + "/" + this.m_calc_cs.ToString();
        }
        this.m_charged_mass_calc_descriptive += str3;
        this.m_charged_mass_calc_numeric = "M";
        double num1 = (double) this.m_adduct_sign * this.m_adduct_m0;
        if (this.m_calc_cs > 1)
        {
          this.m_charged_mass_calc_numeric = "M/" + this.m_calc_cs.ToString();
          num1 /= (double) this.m_calc_cs;
        }
        double num2 = num1 + (double) this.m_calc_polarity * this.m_calc_ccm;
        if (num2 > 0.0)
          this.m_charged_mass_calc_numeric = this.m_charged_mass_calc_numeric + "+" + num2.ToString("0.000000");
        else if (num2 < 0.0)
          this.m_charged_mass_calc_numeric += num2.ToString("0.000000");
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

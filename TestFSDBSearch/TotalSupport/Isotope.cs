// Decompiled with JetBrains decompiler
// Type: TestFSDBSearch.TotalSupport.Isotope
// Assembly: TestFSDBSearch, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C36EBD2C-6652-4FDC-A74D-B15E46A79224
// Assembly location: F:\Documents\Projects\NikolaTolic\Formultitude\Lib\TestFSDBSearch.exe

using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using System;

namespace TestFSDBSearch.TotalSupport
{
  public struct Isotope
  {
    public string Name;
    public string Symbol;
    public double MassNumber;
    public double ReAtMa;
    public double ComPct;
    public int Valence;

    public string ToString(string deli = "\t")
    {
      string str;
      try
      {
        if (Information.IsNothing((object) this.Name))
          this.Name = string.Empty;
        if (Information.IsNothing((object) this.Symbol))
          this.Symbol = string.Empty;
        str = this.Name + deli + this.Symbol + deli + Conversions.ToString(this.MassNumber) + deli + Conversions.ToString(this.ReAtMa) + deli + Conversions.ToString(this.ComPct);
      }
      catch (Exception ex)
      {
        ProjectData.SetProjectError(ex);
        str = "Isotope.ToString - " + ex.Message;
        ProjectData.ClearProjectError();
      }
      return str;
    }
  }
}

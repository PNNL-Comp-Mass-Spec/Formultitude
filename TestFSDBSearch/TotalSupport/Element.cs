// Decompiled with JetBrains decompiler
// Type: TestFSDBSearch.TotalSupport.Element
// Assembly: TestFSDBSearch, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C36EBD2C-6652-4FDC-A74D-B15E46A79224
// Assembly location: F:\Documents\Projects\NikolaTolic\Formultitude\Lib\TestFSDBSearch.exe

using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Text;

namespace TestFSDBSearch.TotalSupport
{
  public struct Element
  {
    public string AtomicSymbol;
    public int AtomicNumber;
    public Isotope[] Isotopes;
    public double StAtWe;

    public bool AddIsotope(Isotope NewIsotope)
    {
      bool flag;
      try
      {
        int index;
        if (Information.IsNothing((object) this.Isotopes))
        {
          index = 0;
          this.Isotopes = new Isotope[1];
        }
        else
        {
          index = this.Isotopes.Length;
          this.Isotopes = (Isotope[]) Utils.CopyArray((Array) this.Isotopes, (Array) new Isotope[checked (index + 1)]);
        }
        this.Isotopes[index] = NewIsotope;
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

    public string ToString(string deli = "\t")
    {
      StringBuilder stringBuilder1 = new StringBuilder(string.Empty);
      string str;
      try
      {
        if (Information.IsNothing((object) this.AtomicSymbol))
          this.AtomicSymbol = string.Empty;
        StringBuilder stringBuilder2 = new StringBuilder(this.AtomicSymbol + deli + Conversions.ToString(this.AtomicNumber) + deli + Conversions.ToString(this.StAtWe) + "\r\n");
        if (!Information.IsNothing((object) this.Isotopes))
        {
          int num1 = 0;
          int num2 = checked (this.Isotopes.Length - 1);
          int index = num1;
          while (index <= num2)
          {
            stringBuilder2.Append("\t" + this.Isotopes[index].ToString("\t") + "\r\n");
            checked { ++index; }
          }
        }
        str = stringBuilder2.ToString();
      }
      catch (Exception ex)
      {
        ProjectData.SetProjectError(ex);
        str = "Element.ToString - " + ex.Message;
        ProjectData.ClearProjectError();
      }
      return str;
    }
  }
}

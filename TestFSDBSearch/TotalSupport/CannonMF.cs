// Decompiled with JetBrains decompiler
// Type: TestFSDBSearch.TotalSupport.CannonMF
// Assembly: TestFSDBSearch, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C36EBD2C-6652-4FDC-A74D-B15E46A79224
// Assembly location: F:\Documents\Projects\NikolaTolic\Formularity\Lib\TestFSDBSearch.exe

using Microsoft.VisualBasic.CompilerServices;
using System;
using System.IO;
using System.Text;

namespace TestFSDBSearch.TotalSupport
{
  public class CannonMF
  {
    private const string TKN_COMMA = ",";
    private const string TKN_SPACE = " ";
    private const string TKN_COLON = ":";
    private const string TKN_NAME = "name";
    private const string TKN_EQUAL = "=";
    private const string TKN_ISO_ATOMIC_NUMBER = "atomic number";
    private const string TKN_ISO_ATOMIC_SYMBOL = "atomic symbol";
    private const string TKN_ISO_MASS_NUMBER = "mass number";
    private const string TKN_ISO_RELATIVE_ATOMIC_MASS = "relative atomic mass";
    private const string TKN_ISO_ISOTOPIC_COMPOSITION = "isotopic composition";
    private const string TKN_ISO_STANDARD_ATOMIC_WEIGHT = "standard atomic weight";
    private const string TKN_ISO_NOTES = "notes";
    private const string TKN_ISO_VALENCE = "valence";
    public int[] ElI;
    public int[] MMF;
    public Element[] Els;

    public CannonMF()
    {
      this.ElI = new int[0];
      this.MMF = new int[0];
      this.Els = new Element[0];
    }

    public bool ReadElementsFromFile(string FileName)
    {
      string[] strArray1 = new string[0];
      Isotope NewIsotope = new Isotope();
      bool flag;
      try
      {
        TextReader textReader = (TextReader) new StreamReader(FileName);
        this.Els = new Element[110];
        Elements elements = Elements.H;
        do
        {
          this.Els[(int) elements].AtomicNumber = (int) elements;
          this.Els[(int) elements].AtomicSymbol = elements.ToString();
          ++elements;
        }
        while (elements <= Elements.Une);
        while (textReader.Peek() != -1)
        {
          string[] strArray2 = textReader.ReadLine().Trim().Split('=');
          int index = 0;
          if (strArray2.Length == 2)
          {
            string lower = strArray2[0].Trim().ToLower();
            if (Operators.CompareString(lower, "atomic number", false) == 0)
              index = int.Parse(strArray2[1].Trim());
            else if (Operators.CompareString(lower, "atomic symbol", false) == 0)
              NewIsotope.Symbol = strArray2[1].Trim();
            else if (Operators.CompareString(lower, "mass number", false) == 0)
              NewIsotope.MassNumber = (double) int.Parse(strArray2[1].Trim());
            else if (Operators.CompareString(lower, "relative atomic mass", false) == 0)
              NewIsotope.ReAtMa = double.Parse(ttlAI.FragmentOfChoice(strArray2[1].Trim(), 0, "(", true, "0", "-1", "0"));
            else if (Operators.CompareString(lower, "isotopic composition", false) == 0)
              NewIsotope.ComPct = double.Parse(ttlAI.FragmentOfChoice(strArray2[1].Trim(), 0, "(", true, "0", "-1", "0")) / 100.0;
            else if (Operators.CompareString(lower, "standard atomic weight", false) == 0)
            {
              if (index > 0 & index <= 109)
                this.Els[index].StAtWe = double.Parse(ttlAI.FragmentOfChoice(strArray2[1].Trim(), 0, "(", true, "0", "-1", "0"));
            }
            else if (Operators.CompareString(lower, "notes", false) != 0)
            {
              if (Operators.CompareString(lower, "valence", false) == 0)
                NewIsotope.Valence = int.Parse(strArray2[1].Trim());
              else if (Operators.CompareString(lower, "name", false) == 0)
                NewIsotope.Name = strArray2[1].Trim();
              else
                index = -1;
            }
          }
          else
          {
            if (index > 0 & index <= 109)
              this.Els[index].AddIsotope(NewIsotope);
            index = -1;
            NewIsotope = new Isotope();
          }
        }
        textReader.Close();
        flag = true;
      }
      catch (Exception ex)
      {
        ProjectData.SetProjectError(ex);
        flag = false;
        ProjectData.ClearProjectError();
      }
      finally
      {
      }
      return flag;
    }

    public bool SetMF(string mf)
    {
      string[] strArray1 = new string[0];
      string[] strArray2 = new string[0];
      string[] separator1 = new string[1]{ " " };
      string[] separator2 = new string[1]{ ":" };
      bool flag;
      try
      {
        this.MMF = new int[110];
        string[] strArray3 = mf.Split(separator1, StringSplitOptions.None);
        int num1 = 0;
        int num2 = checked (strArray3.Length - 1);
        int index = num1;
        while (index <= num2)
        {
          string[] strArray4 = strArray3[index].Split(separator2, StringSplitOptions.None);
          this.MMF[(int) Enum.Parse(typeof (Elements), strArray4[0])] = int.Parse(strArray4[1]);
          checked { ++index; }
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

    public bool AddMF(string mf)
    {
      string[] strArray1 = new string[0];
      string[] strArray2 = new string[0];
      string[] separator1 = new string[1]{ " " };
      string[] separator2 = new string[1]{ ":" };
      bool flag;
      try
      {
        string[] strArray3 = mf.Split(separator1, StringSplitOptions.None);
        int num1 = 0;
        int num2 = checked (strArray3.Length - 1);
        int index1 = num1;
        while (index1 <= num2)
        {
          string[] strArray4 = strArray3[index1].Split(separator2, StringSplitOptions.None);
          Elements elements = (Elements) Enum.Parse(typeof (Elements), strArray4[0]);
          int[] mmf = this.MMF;
          int[] numArray = mmf;
          int index2 = (int) elements;
          int index3 = index2;
          int num3 = checked (mmf[index2] + int.Parse(strArray4[1]));
          numArray[index3] = num3;
          checked { ++index1; }
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

    public bool SubstractMF(string mf)
    {
      string[] strArray1 = new string[0];
      string[] strArray2 = new string[0];
      string[] separator1 = new string[1]{ " " };
      string[] separator2 = new string[1]{ ":" };
      bool flag;
      try
      {
        string[] strArray3 = mf.Split(separator1, StringSplitOptions.None);
        int num1 = 0;
        int num2 = checked (strArray3.Length - 1);
        int index1 = num1;
        while (index1 <= num2)
        {
          string[] strArray4 = strArray3[index1].Split(separator2, StringSplitOptions.None);
          Elements elements = (Elements) Enum.Parse(typeof (Elements), strArray4[0]);
          int[] mmf = this.MMF;
          int[] numArray = mmf;
          int index2 = (int) elements;
          int index3 = index2;
          int num3 = checked (mmf[index2] - int.Parse(strArray4[1]));
          numArray[index3] = num3;
          checked { ++index1; }
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

    public bool MultiMF(long f)
    {
      bool flag;
      try
      {
        int index = 1;
        do
        {
          this.MMF[index] = checked ((int) (f * (long) this.MMF[index]));
          checked { ++index; }
        }
        while (index <= 108);
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

    public string GetMF(bool xHillFormat = true)
    {
      StringBuilder stringBuilder = new StringBuilder(string.Empty);
      string str;
      try
      {
        if (xHillFormat & this.MMF[6] != 0)
        {
          stringBuilder.Append(Elements.C.ToString() + ":" + Conversions.ToString(this.MMF[6]) + " ");
          if (this.MMF[1] != 0)
            stringBuilder.Append(Elements.H.ToString() + ":" + Conversions.ToString(this.MMF[1]) + " ");
          Elements elements = Elements.H;
          do
          {
            if (this.MMF[(int) elements] != 0 && elements != Elements.C & elements != Elements.H)
              stringBuilder.Append(elements.ToString() + ":" + Conversions.ToString(this.MMF[(int) elements]) + " ");
            ++elements;
          }
          while (elements <= Elements.Une);
        }
        else
        {
          Elements elements = Elements.H;
          do
          {
            if (this.MMF[(int) elements] != 0)
              stringBuilder.Append(elements.ToString() + ":" + Conversions.ToString(this.MMF[(int) elements]) + " ");
            ++elements;
          }
          while (elements <= Elements.Une);
        }
        str = stringBuilder.ToString().Trim();
      }
      catch (Exception ex)
      {
        ProjectData.SetProjectError(ex);
        str = (string) null;
        ProjectData.ClearProjectError();
      }
      return str;
    }

    public string GetMF_PosCount(bool xHillFormat = true)
    {
      StringBuilder stringBuilder = new StringBuilder(string.Empty);
      string str;
      try
      {
        if (xHillFormat & this.MMF[6] > 0)
        {
          stringBuilder.Append(Elements.C.ToString() + ":" + Conversions.ToString(this.MMF[6]) + " ");
          if (this.MMF[1] > 0)
            stringBuilder.Append(Elements.H.ToString() + ":" + Conversions.ToString(this.MMF[1]) + " ");
          Elements elements = Elements.H;
          do
          {
            if (this.MMF[(int) elements] > 0 && elements != Elements.C & elements != Elements.H)
              stringBuilder.Append(elements.ToString() + ":" + Conversions.ToString(this.MMF[(int) elements]) + " ");
            ++elements;
          }
          while (elements <= Elements.Une);
        }
        else
        {
          Elements elements = Elements.H;
          do
          {
            if (this.MMF[(int) elements] > 0)
              stringBuilder.Append(elements.ToString() + ":" + Conversions.ToString(this.MMF[(int) elements]) + " ");
            ++elements;
          }
          while (elements <= Elements.Une);
        }
        str = stringBuilder.ToString().Trim();
      }
      catch (Exception ex)
      {
        ProjectData.SetProjectError(ex);
        str = (string) null;
        ProjectData.ClearProjectError();
      }
      return str;
    }

    public string GetMF_NegCount(bool xHillFormat = true)
    {
      StringBuilder stringBuilder = new StringBuilder(string.Empty);
      string str;
      try
      {
        if (xHillFormat & this.MMF[6] < 0)
        {
          stringBuilder.Append(Elements.C.ToString() + ":" + Conversions.ToString(Math.Abs(this.MMF[6])) + " ");
          if (this.MMF[1] < 0)
            stringBuilder.Append(Elements.H.ToString() + ":" + Conversions.ToString(Math.Abs(this.MMF[1])) + " ");
          Elements elements = Elements.H;
          do
          {
            if (this.MMF[(int) elements] < 0 && elements != Elements.C & elements != Elements.H)
              stringBuilder.Append(elements.ToString() + ":" + Conversions.ToString(Math.Abs(this.MMF[(int) elements])) + " ");
            ++elements;
          }
          while (elements <= Elements.Une);
        }
        else
        {
          Elements elements = Elements.H;
          do
          {
            if (this.MMF[(int) elements] < 0)
              stringBuilder.Append(elements.ToString() + ":" + Conversions.ToString(Math.Abs(this.MMF[(int) elements])) + " ");
            ++elements;
          }
          while (elements <= Elements.Une);
        }
        str = stringBuilder.ToString().Trim();
      }
      catch (Exception ex)
      {
        ProjectData.SetProjectError(ex);
        str = (string) null;
        ProjectData.ClearProjectError();
      }
      return str;
    }

    public double GetMF_Mono0Mass()
    {
      double num1 = 0.0;
      double num2;
      try
      {
        Elements elements = Elements.H;
        do
        {
          if (this.MMF[(int) elements] != 0)
            num1 += (double) this.MMF[(int) elements] * this.Els[(int) elements].Isotopes[0].ReAtMa;
          ++elements;
        }
        while (elements <= Elements.Une);
        num2 = num1;
      }
      catch (Exception ex)
      {
        ProjectData.SetProjectError(ex);
        num2 = -1.0;
        ProjectData.ClearProjectError();
      }
      return num2;
    }

    public double GetMF_Mono0Mass_NegCount()
    {
      double num1 = 0.0;
      double num2;
      try
      {
        Elements elements = Elements.H;
        do
        {
          if (this.MMF[(int) elements] < 0)
            num1 += (double) Math.Abs(this.MMF[(int) elements]) * this.Els[(int) elements].Isotopes[0].ReAtMa;
          ++elements;
        }
        while (elements <= Elements.Une);
        num2 = num1;
      }
      catch (Exception ex)
      {
        ProjectData.SetProjectError(ex);
        num2 = -1.0;
        ProjectData.ClearProjectError();
      }
      return num2;
    }

    public double GetMF_Mono0Mass_PosCount()
    {
      double num1 = 0.0;
      double num2;
      try
      {
        Elements elements = Elements.H;
        do
        {
          if (this.MMF[(int) elements] > 0)
            num1 += (double) this.MMF[(int) elements] * this.Els[(int) elements].Isotopes[0].ReAtMa;
          ++elements;
        }
        while (elements <= Elements.Une);
        num2 = num1;
      }
      catch (Exception ex)
      {
        ProjectData.SetProjectError(ex);
        num2 = -1.0;
        ProjectData.ClearProjectError();
      }
      return num2;
    }

    public int GetMF_AtomCount()
    {
      int num1;
      try
      {
        Elements elements = Elements.H;
        int num2 = 0;
        do
        {
          checked { num2 += this.MMF[unchecked ((int) elements)]; }
          ++elements;
        }
        while (elements <= Elements.Une);
        num1 = num2;
      }
      catch (Exception ex)
      {
        ProjectData.SetProjectError(ex);
        num1 = -1;
        ProjectData.ClearProjectError();
      }
      return num1;
    }

    public double GetMF_NominalMass()
    {
      double num1 = 0.0;
      double num2;
      try
      {
        Elements elements = Elements.H;
        do
        {
          if (this.MMF[(int) elements] != 0)
            num1 += (double) this.MMF[(int) elements] * this.Els[(int) elements].Isotopes[0].MassNumber;
          ++elements;
        }
        while (elements <= Elements.Une);
        num2 = num1;
      }
      catch (Exception ex)
      {
        ProjectData.SetProjectError(ex);
        num2 = -1.0;
        ProjectData.ClearProjectError();
      }
      return num2;
    }

    public double GetMF_ValenceSum()
    {
      int num1 = 0;
      double num2;
      try
      {
        Elements elements = Elements.H;
        do
        {
          if (this.MMF[(int) elements] != 0)
            checked { num1 += this.MMF[unchecked ((int) elements)] * this.Els[unchecked ((int) elements)].Isotopes[0].Valence; }
          ++elements;
        }
        while (elements <= Elements.Une);
        num2 = (double) num1;
      }
      catch (Exception ex)
      {
        ProjectData.SetProjectError(ex);
        num2 = -1.0;
        ProjectData.ClearProjectError();
      }
      return num2;
    }

    public string GetEC()
    {
      StringBuilder stringBuilder = new StringBuilder(string.Empty);
      string message;
      try
      {
        Elements elements = Elements.H;
        do
        {
          if (this.MMF[(int) elements] != 0)
            stringBuilder.Append(elements.ToString());
          ++elements;
        }
        while (elements <= Elements.Une);
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

    public string GetCIACounts(string deli = ",")
    {
      return Conversions.ToString(this.MMF[6]) + deli + Conversions.ToString(this.MMF[1]) + deli + Conversions.ToString(this.MMF[8]) + deli + Conversions.ToString(this.MMF[7]) + deli + "0" + deli + Conversions.ToString(this.MMF[16]) + deli + Conversions.ToString(this.MMF[15]) + deli + Conversions.ToString(this.MMF[11]);
    }
  }
}

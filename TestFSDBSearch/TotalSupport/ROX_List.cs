// Decompiled with JetBrains decompiler
// Type: TestFSDBSearch.TotalSupport.ROX_List
// Assembly: TestFSDBSearch, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C36EBD2C-6652-4FDC-A74D-B15E46A79224
// Assembly location: F:\Documents\Projects\NikolaTolic\Formularity\Lib\TestFSDBSearch.exe

using Microsoft.VisualBasic.CompilerServices;
using System;

namespace TestFSDBSearch.TotalSupport
{
  public class ROX_List
  {
    public string deli_elements;
    public string deli_options;
    public int r_cnt;
    public int o_cnt;
    public int x_cnt;
    public int e_cnt;
    public int z_cnt;
    public string[] r_elements;
    public string[] o_elements;
    public string[] x_elements;
    public string[] e_ec;
    public string[] z_elements;

    public ROX_List(string list)
    {
      this.deli_elements = ",";
      this.deli_options = "/";
      this.r_elements = new string[0];
      this.o_elements = new string[0];
      this.x_elements = new string[0];
      this.e_ec = new string[0];
      this.z_elements = new string[0];
      string[] strArray1 = new string[0];
      try
      {
        string[] strArray2 = list.Split(Conversions.ToChar(this.deli_elements));
        if (strArray2.Length <= 0)
          return;
        this.r_elements = new string[checked (strArray2.Length - 1 + 1)];
        this.o_elements = new string[checked (strArray2.Length - 1 + 1)];
        this.x_elements = new string[checked (strArray2.Length - 1 + 1)];
        this.e_ec = new string[checked (strArray2.Length - 1 + 1)];
        this.z_elements = new string[checked (strArray2.Length - 1 + 1)];
        int num1 = 0;
        int num2 = checked (strArray2.Length - 1);
        int index = num1;
        while (index <= num2)
        {
          string[] strArray3 = strArray2[index].Split(Conversions.ToChar(this.deli_options));
          if (strArray3.Length == 2)
          {
            string lower = strArray3[0].ToLower();
            if (Operators.CompareString(lower, ROX_List.rox_elements.r.ToString(), false) == 0)
            {
              this.r_elements[this.r_cnt] = strArray3[1];
              checked { ++this.r_cnt; }
            }
            else if (Operators.CompareString(lower, ROX_List.rox_elements.o.ToString(), false) == 0)
            {
              this.o_elements[this.o_cnt] = strArray3[1];
              checked { ++this.o_cnt; }
            }
            else if (Operators.CompareString(lower, ROX_List.rox_elements.x.ToString(), false) == 0)
            {
              this.x_elements[this.x_cnt] = strArray3[1];
              checked { ++this.x_cnt; }
            }
            else if (Operators.CompareString(lower, ROX_List.rox_elements.e.ToString(), false) == 0)
            {
              this.e_ec[this.e_cnt] = strArray3[1];
              checked { ++this.e_cnt; }
            }
            else
            {
              this.z_elements[this.z_cnt] = strArray2[index];
              checked { ++this.z_cnt; }
            }
          }
          else
          {
            this.z_elements[this.z_cnt] = strArray2[index];
            checked { ++this.z_cnt; }
          }
          checked { ++index; }
        }
        this.r_elements = this.r_cnt <= 0 ? (string[]) null : (string[]) Utils.CopyArray((Array) this.r_elements, (Array) new string[checked (this.r_cnt - 1 + 1)]);
        this.o_elements = this.o_cnt <= 0 ? (string[]) null : (string[]) Utils.CopyArray((Array) this.o_elements, (Array) new string[checked (this.o_cnt - 1 + 1)]);
        this.x_elements = this.x_cnt <= 0 ? (string[]) null : (string[]) Utils.CopyArray((Array) this.x_elements, (Array) new string[checked (this.x_cnt - 1 + 1)]);
        this.e_ec = this.e_cnt <= 0 ? (string[]) null : (string[]) Utils.CopyArray((Array) this.e_ec, (Array) new string[checked (this.e_cnt - 1 + 1)]);
        this.z_elements = this.z_cnt <= 0 ? (string[]) null : (string[]) Utils.CopyArray((Array) this.z_elements, (Array) new string[checked (this.z_cnt - 1 + 1)]);
      }
      catch (Exception ex)
      {
        ProjectData.SetProjectError(ex);
        throw new ArgumentException();
      }
    }

    public enum rox_elements
    {
      r = 1,
      o = 2,
      x = 3,
      e = 4,
      z = 5,
    }
  }
}

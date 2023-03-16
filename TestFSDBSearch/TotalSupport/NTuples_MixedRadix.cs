// Decompiled with JetBrains decompiler
// Type: TestFSDBSearch.TotalSupport.NTuples_MixedRadix
// Assembly: TestFSDBSearch, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C36EBD2C-6652-4FDC-A74D-B15E46A79224
// Assembly location: F:\Documents\Projects\NikolaTolic\Formularity\Lib\TestFSDBSearch.exe

using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Text;

namespace TestFSDBSearch.TotalSupport
{
  public class NTuples_MixedRadix
  {
    public int[] M;
    public int[] NTuple;
    private long mCnt;

    public NTuples_MixedRadix()
    {
      this.NTuple = new int[0];
      this.mCnt = -1L;
    }

    public long Count
    {
      get
      {
        return this.mCnt;
      }
    }

    public int N
    {
      get
      {
        return this.M.Length;
      }
    }

    public bool Initialize()
    {
      this.NTuple = new int[checked (this.M.Length - 1 + 1)];
      this.mCnt = 1L;
      return true;
    }

    public bool SetNext()
    {
      int index1 = checked (this.N - 1);
      int num1 = -1;
      bool flag = index1 < 0;
      while (!flag)
      {
        if (this.NTuple[index1] == this.M[index1])
        {
          if (index1 > 0)
          {
            this.NTuple[index1] = 0;
            checked { --index1; }
          }
          else
            flag = true;
        }
        else
        {
          num1 = index1;
          flag = true;
        }
      }
      if (num1 < 0)
        return false;
      int[] ntuple = this.NTuple;
      int[] numArray = ntuple;
      int index2 = num1;
      int index3 = index2;
      int num2 = checked (ntuple[index2] + 1);
      numArray[index3] = num2;
      checked { ++this.mCnt; }
      return true;
    }

    public string NTupleToString()
    {
      StringBuilder stringBuilder = new StringBuilder();
      string str;
      try
      {
        int num1 = 0;
        int num2 = checked (this.NTuple.Length - 1);
        int index = num1;
        while (index <= num2)
        {
          stringBuilder.Append(this.NTuple[index].ToString() + " ");
          checked { ++index; }
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
  }
}

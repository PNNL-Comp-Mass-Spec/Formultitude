// Decompiled with JetBrains decompiler
// Type: TestFSDBSearch.TotalSupport.ttlAI
// Assembly: TestFSDBSearch, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C36EBD2C-6652-4FDC-A74D-B15E46A79224
// Assembly location: F:\Documents\Projects\NikolaTolic\Formultitude\Lib\TestFSDBSearch.exe

using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace TestFSDBSearch.TotalSupport
{
  public class ttlAI
  {
    [DebuggerNonUserCode]
    public ttlAI()
    {
    }

    public static string AI_IonizationMethod_Enum2String(IonizationMethod im)
    {
      switch (im)
      {
        case IonizationMethod.none:
          return "";
        case IonizationMethod.proton_detachment:
          return "-p";
        case IonizationMethod.proton_attachment:
          return "+p";
        case IonizationMethod.electron_detachment:
          return "-e";
        case IonizationMethod.electron_attachment:
          return "+e";
        default:
          throw new ArgumentException();
      }
    }

    public static IonizationMethod AI_IonizationMethod_String2Enum(string ims)
    {
      string Left = ims.Trim();
      if (Operators.CompareString(Left, "", false) == 0)
        return IonizationMethod.none;
      if (Operators.CompareString(Left, "+e", false) == 0)
        return IonizationMethod.electron_attachment;
      if (Operators.CompareString(Left, "-e", false) == 0)
        return IonizationMethod.electron_detachment;
      if (Operators.CompareString(Left, "+p", false) == 0)
        return IonizationMethod.proton_attachment;
      if (Operators.CompareString(Left, "-p", false) == 0)
        return IonizationMethod.proton_detachment;
      throw new ArgumentException();
    }

    public static string AI_IonizationMethod_Enum2PolarityString(IonizationMethod im)
    {
      switch (im)
      {
        case IonizationMethod.none:
          return EPolarity.Neutral.ToString().ToLower();
        case IonizationMethod.proton_detachment:
        case IonizationMethod.electron_attachment:
          return EPolarity.Negative.ToString().ToLower();
        case IonizationMethod.proton_attachment:
        case IonizationMethod.electron_detachment:
          return EPolarity.Positive.ToString().ToLower();
        default:
          return string.Empty;
      }
    }

    public static string AI_GetMFFromLine(string ln, string deli, bool mf_long_format = true)
    {
      StringBuilder stringBuilder = new StringBuilder(string.Empty);
      string str;
      try
      {
        string[] strArray = ln.Trim().Split(Conversions.ToChar(deli));
        if (strArray.Length == 1)
        {
          stringBuilder.Append(strArray[0]);
        }
        else
        {
          int num1 = int.Parse(strArray[2]);
          int num2 = int.Parse(strArray[3]);
          int num3 = int.Parse(strArray[4]);
          int num4 = int.Parse(strArray[5]);
          int num5 = int.Parse(strArray[6]);
          int num6 = int.Parse(strArray[7]);
          int num7 = int.Parse(strArray[8]);
          int num8 = int.Parse(strArray[9]);
          if (checked (num1 + num5) > 0)
            stringBuilder.Append("C" + Conversions.ToString(checked (num1 + num5)));
          if (num2 > 0)
            stringBuilder.Append("H" + Conversions.ToString(num2));
          if (num3 > 0)
            stringBuilder.Append("O" + Conversions.ToString(num3));
          if (num4 > 0)
            stringBuilder.Append("N" + Conversions.ToString(num4));
          if (num6 > 0)
            stringBuilder.Append("S" + Conversions.ToString(num6));
          if (num7 > 0)
            stringBuilder.Append("P" + Conversions.ToString(num7));
          if (num8 > 0)
            stringBuilder.Append("Na" + Conversions.ToString(num8));
        }
        str = !mf_long_format ? stringBuilder.ToString() : ttlAI.TranslateShortToLongMF(stringBuilder.ToString());
      }
      catch (Exception ex)
      {
        ProjectData.SetProjectError(ex);
        str = string.Empty;
        ProjectData.ClearProjectError();
      }
      return str;
    }

    internal static string GetFileName_A(string FullFileName, string AddToFileName = "", TotalDeclarations.WordType pos = TotalDeclarations.WordType.suffix, string Extension = "")
    {
      FileInfo fileInfo = new FileInfo(FullFileName);
      string str = fileInfo.Name.Substring(0, checked (fileInfo.Name.Length - fileInfo.Extension.Length));
      if (AddToFileName.Length > 0)
      {
        switch (pos)
        {
          case TotalDeclarations.WordType.prefix:
            str = AddToFileName + str;
            break;
          case TotalDeclarations.WordType.suffix:
            str += AddToFileName;
            break;
        }
      }
      if (Extension.Length > 0)
        return fileInfo.Directory.FullName + Conversions.ToString(Path.DirectorySeparatorChar) + str + Extension;
      return fileInfo.Directory.FullName + Conversions.ToString(Path.DirectorySeparatorChar) + str + fileInfo.Extension;
    }

    public static int AI_GetNiceInteger(int n, int d, int @base = 10)
    {
      return checked ((int) Math.Round(unchecked (d >= 0 ? Math.Ceiling((double) n / (double) @base) : Math.Floor((double) n / (double) @base) * (double) @base)));
    }

    public static bool AI_IsCommentLine(string ln)
    {
      if (ln.Length > 0)
      {
        string lower = ln.Substring(0, 1).ToLower();
        if (Operators.CompareString(lower, "#", false) == 0 || Operators.CompareString(lower, "@", false) == 0 || (Operators.CompareString(lower, "/", false) == 0 || Operators.CompareString(lower, "\\", false) == 0) || Operators.CompareString(lower, "'", false) == 0)
          return true;
      }
      return false;
    }

    public static bool AI_SegmentsOverlap_Discrete(int[] a, int[] b, ref int[] o)
    {
      bool flag;
      try
      {
        o[0] = Math.Max(a[0], b[0]);
        o[1] = Math.Min(a[1], b[1]);
        flag = o[0] <= o[1];
      }
      catch (Exception ex)
      {
        ProjectData.SetProjectError(ex);
        flag = false;
        ProjectData.ClearProjectError();
      }
      return flag;
    }

    public static bool AI_SegmentsOverlap_Discrete(int a1, int a2, int b1, int b2, bool OneIsOverlap = true)
    {
      bool flag;
      try
      {
        int num1 = Math.Max(a1, b1);
        int num2 = Math.Min(a2, b2);
        flag = !OneIsOverlap ? num1 < num2 : num1 <= num2;
      }
      catch (Exception ex)
      {
        ProjectData.SetProjectError(ex);
        flag = false;
        ProjectData.ClearProjectError();
      }
      return flag;
    }

    public static double AI_SegmentsOverlap_Continuos(double a1, double a2, double b1, double b2)
    {
      double num = Math.Max(a1, b1);
      return Math.Min(a2, b2) - num;
    }

    public static string AI_MergeTokens(string Tkn1, string Tkn2, string Conjunction = "=")
    {
      string str;
      try
      {
        str = Tkn1 + Conjunction + Tkn2;
      }
      catch (Exception ex)
      {
        ProjectData.SetProjectError(ex);
        str = string.Empty;
        ProjectData.ClearProjectError();
      }
      return str;
    }

    public static string AI_BracketToken(string Tkn)
    {
      return "[" + Tkn + "]";
    }

    public static bool AI_IsSamePair_Char(char x1, char y1, char x2, char y2)
    {
      if ((int) x1 == (int) x2)
        return (int) y1 == (int) y2;
      if ((int) x1 == (int) y2)
        return (int) y1 == (int) x2;
      return false;
    }

    public static string AI_NumberPair(double n1, double n2, string deli = ",", string num_fmt = "")
    {
      if (Strings.Len(num_fmt) > 0)
        return "(" + n1.ToString(num_fmt) + deli + n2.ToString(num_fmt) + ")";
      return "(" + n1.ToString() + deli + n2.ToString() + ")";
    }

    public static string AI_FileWithPath(string p, string f)
    {
      string str;
      try
      {
        char[] charArray = p.ToCharArray();
        str = (int) charArray[checked (charArray.Length - 1)] == (int) Path.DirectorySeparatorChar ? p + f : p + Conversions.ToString(Path.DirectorySeparatorChar) + f;
      }
      catch (Exception ex)
      {
        ProjectData.SetProjectError(ex);
        str = string.Empty;
        ProjectData.ClearProjectError();
      }
      return str;
    }

    public static int SmartArrayIncrease(double PercentDone, int CurrentSize, int MinIncrease = 231)
    {
      int val1 = 0;
      if (PercentDone > 0.0)
        val1 = PercentDone <= 0.5 ? checked (2 * CurrentSize) : checked ((int) Math.Round(unchecked ((1.0 - PercentDone) / PercentDone * (double) CurrentSize)));
      return Math.Max(val1, MinIncrease);
    }

    public static bool AlignListView(ref int ind1, ref int ind2, int lst_view_cnt, int lst_ttl_cnt)
    {
label_1:
      int num1 = 0;
      bool flag;
      int num2 = 0;
      try
      {
        ProjectData.ClearProjectError();
        num1 = -2;
label_2:
        int num3 = 2;
        if (!(ind1 == -1 | ind1 >= lst_ttl_cnt))
          goto label_4;
label_3:
        num3 = 3;
        ind1 = checked (lst_ttl_cnt - lst_view_cnt);
label_4:
        num3 = 5;
        if (ind1 >= 0)
          goto label_6;
label_5:
        num3 = 6;
        ind1 = 0;
label_6:
        num3 = 8;
        ind2 = checked (ind1 + lst_view_cnt - 1);
label_7:
        num3 = 9;
        if (ind2 < lst_ttl_cnt)
          goto label_9;
label_8:
        num3 = 10;
        ind2 = checked (lst_ttl_cnt - 1);
label_9:
        num3 = 12;
        flag = ind1 <= ind2;
        goto label_16;
label_11:
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
              case 5:
                goto label_4;
              case 6:
                goto label_5;
              case 7:
              case 8:
                goto label_6;
              case 9:
                goto label_7;
              case 10:
                goto label_8;
              case 11:
              case 12:
                goto label_9;
              case 13:
                goto label_16;
            }
        }
      }
      catch (Exception ex) when (ex is Exception & (uint) num1 > 0U & num2 == 0)
      {
        ProjectData.SetProjectError(ex);
          return false;
      }
      throw ProjectData.CreateProjectError(-2146828237);
label_16:
      int num5 = flag ? 1 : 0;
      if (num2 == 0)
        return num5 != 0;
      ProjectData.ClearProjectError();
      return num5 != 0;
    }

    public static string DelimitedBytes(string s, string deli = " ")
    {
      string str;
      try
      {
        StringBuilder stringBuilder = new StringBuilder(string.Empty);
        ASCIIEncoding asciiEncoding = new ASCIIEncoding();
        byte[] numArray = new byte[checked (s.Length - 1 + 1)];
        byte[] bytes = asciiEncoding.GetBytes(s);
        int num1 = 0;
        int num2 = checked (bytes.Length - 1);
        int index = num1;
        while (index <= num2)
        {
          stringBuilder.Append(Conversions.ToString(bytes[index]) + deli);
          checked { ++index; }
        }
        str = stringBuilder.ToString().Trim();
      }
      catch (Exception ex)
      {
        ProjectData.SetProjectError(ex);
        str = "error reporting bytes - " + ex.Message;
        ProjectData.ClearProjectError();
      }
      return str;
    }

    public static bool IsUpperCase(string s)
    {
      bool flag;
      try
      {
        string upper = s.ToUpper();
        flag = string.Compare(s, upper, false) == 0;
      }
      catch (Exception ex)
      {
        ProjectData.SetProjectError(ex);
        flag = false;
        ProjectData.ClearProjectError();
      }
      return flag;
    }

    public static string MessageAndError(string msg, Exception ex)
    {
label_1:
      int num1 = 0;
      string str1;
      int num2 = 0;
      try
      {
        ProjectData.ClearProjectError();
        num1 = -2;
label_2:
        int num3 = 2;
        str1 = (msg + " (error: " + ex.Message + ")").Trim();
        goto label_9;
label_4:
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
                goto label_9;
            }
        }
      }
      catch (Exception ex1) when (ex1 is Exception & (uint) num1 > 0U & num2 == 0)
      {
        ProjectData.SetProjectError(ex1);
          var message = new StringBuilder();
          if (!string.IsNullOrEmpty(msg))
            message.Append(message);

          if (ex != null)
              message.Append(" (error: " + ex.Message + ")");

          return message.ToString();
      }
      throw ProjectData.CreateProjectError(-2146828237);
label_9:
      string str2 = str1;
      if (num2 == 0)
        return str2;
      ProjectData.ClearProjectError();
      return str2;
    }

    public static bool AI_IsElementIsotope(ref string el)
    {
      bool flag;
      try
      {
        if (el.Length <= 5)
        {
          Match match = Regex.Match(el, "\\d+");
          if (match.Success && match.Value.Length > 0)
          {
            flag = true;
            goto label_6;
          }
        }
        flag = false;
      }
      catch (Exception ex)
      {
        ProjectData.SetProjectError(ex);
        flag = false;
        ProjectData.ClearProjectError();
      }
label_6:
      return flag;
    }

    public static bool AI_ElementProperForm(ref string el)
    {
      bool flag;
      try
      {
        if (el.Length <= 5)
        {
          Match match1 = Regex.Match(el, "\\d+");
          Match match2 = Regex.Match(el, "\\D+");
          if (match2.Success)
          {
            string str1 = ttlAI.AI_ProperCase(match2.Value);
            switch (str1.Length)
            {
              case 1:
              case 2:
                if (match1.Success)
                {
                  string str2 = match1.Value;
                  el = str2 + str1;
                  flag = true;
                  goto label_11;
                }
                else
                {
                  el = str1;
                  break;
                }
            }
          }
        }
        flag = false;
      }
      catch (Exception ex)
      {
        ProjectData.SetProjectError(ex);
        el = (string) null;
        flag = false;
        ProjectData.ClearProjectError();
      }
label_11:
      return flag;
    }

    public static string AI_PureElement(string el)
    {
      string str1;
      try
      {
        if (el.Length <= 5)
        {
          Regex.Match(el, "\\d+");
          Match match = Regex.Match(el, "\\D+");
          if (match.Success)
          {
            string str2 = ttlAI.AI_ProperCase(match.Value);
            switch (str2.Length)
            {
              case 1:
              case 2:
                str1 = str2;
                goto label_8;
            }
          }
        }
        str1 = string.Empty;
      }
      catch (Exception ex)
      {
        ProjectData.SetProjectError(ex);
        str1 = string.Empty;
        ProjectData.ClearProjectError();
      }
label_8:
      return str1;
    }

    public static string AI_ProperCase(string s)
    {
      string str;
      try
      {
        str = s.Length <= 1 ? s.ToUpper() : s.ToUpper().Substring(0, 1) + s.ToLower().Substring(1, checked (s.Length - 1));
      }
      catch (Exception ex)
      {
        ProjectData.SetProjectError(ex);
        str = s.ToUpper();
        ProjectData.ClearProjectError();
      }
      return str;
    }

    public static void Split2ElementsString(string CompoundTwoElements, ref Elements El1, ref Elements El2)
    {
label_1:
      int num1 = 0;
      int num2 = 0;
      try
      {
        ProjectData.ClearProjectError();
        num1 = -2;
label_2:
        int num3 = 2;
        El1 = (Elements) 0;
label_3:
        num3 = 3;
        El2 = (Elements) 0;
label_4:
        num3 = 4;
        CompoundTwoElements.Trim();
label_5:
        num3 = 5;
        switch (CompoundTwoElements.Length)
        {
          case 2:
            break;
          case 3:
            goto label_8;
          case 4:
            goto label_16;
          default:
            goto label_18;
        }
label_6:
        num3 = 9;
        El1 = (Elements) Enum.Parse(typeof (Elements), CompoundTwoElements.Substring(0, 1));
label_7:
        num3 = 10;
        El2 = (Elements) Enum.Parse(typeof (Elements), CompoundTwoElements.Substring(1, 1));
        goto label_18;
label_8:
        num3 = 13;
        string strA = CompoundTwoElements.Substring(2, 1).Trim();
label_9:
        num3 = 14;
        if (string.Compare(strA, CompoundTwoElements.Substring(2, 1), false) != 0)
          goto label_12;
label_10:
        num3 = 15;
        El1 = (Elements) Enum.Parse(typeof (Elements), CompoundTwoElements.Substring(0, 1));
label_11:
        num3 = 16;
        El2 = (Elements) Enum.Parse(typeof (Elements), CompoundTwoElements.Substring(1, 2));
        goto label_15;
label_12:
        num3 = 18;
label_13:
        num3 = 19;
        El1 = (Elements) Enum.Parse(typeof (Elements), CompoundTwoElements.Substring(0, 2));
label_14:
        num3 = 20;
        El2 = (Elements) Enum.Parse(typeof (Elements), CompoundTwoElements.Substring(2, 1));
label_15:
        goto label_18;
label_16:
        num3 = 24;
        El1 = (Elements) Enum.Parse(typeof (Elements), CompoundTwoElements.Substring(0, 2));
label_17:
        num3 = 25;
        El2 = (Elements) Enum.Parse(typeof (Elements), CompoundTwoElements.Substring(2, 2));
label_18:
        goto label_25;
label_20:
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
              case 11:
              case 22:
              case 26:
                goto label_18;
              case 7:
              case 9:
                goto label_6;
              case 10:
                goto label_7;
              case 12:
              case 13:
                goto label_8;
              case 14:
                goto label_9;
              case 15:
                goto label_10;
              case 16:
                goto label_11;
              case 17:
              case 21:
                goto label_15;
              case 18:
                goto label_12;
              case 19:
                goto label_13;
              case 20:
                goto label_14;
              case 23:
              case 24:
                goto label_16;
              case 25:
                goto label_17;
              case 27:
                goto label_25;
            }
        }
      }
      catch (Exception ex) when (ex is Exception & (uint) num1 > 0U & num2 == 0)
      {
        ProjectData.SetProjectError(ex);
          return;
      }
      throw ProjectData.CreateProjectError(-2146828237);
label_25:
      if (num2 == 0)
        return;
      ProjectData.ClearProjectError();
    }

    internal static string TranslateShortToLongMF(string ShortMF)
    {
      if (ShortMF.Length <= 0 || ShortMF.IndexOf(":") >= 0)
        return ShortMF;
      char[] charArray = ShortMF.ToCharArray();
      StringBuilder stringBuilder = new StringBuilder(charArray[0].ToString());
      int num1 = 1;
      int num2 = checked (charArray.Length - 1);
      int index = num1;
      while (index <= num2)
      {
        if (Versioned.IsNumeric((object) charArray[index]))
        {
          if (Versioned.IsNumeric((object) charArray[checked (index - 1)]))
            stringBuilder.Append(charArray[index].ToString());
          else if ((int) charArray[checked (index - 1)] == (int) char.Parse("-"))
            stringBuilder.Append(charArray[index].ToString());
          else
            stringBuilder.Append(":" + charArray[index].ToString());
        }
        else if ((int) charArray[index] == (int) char.Parse("-"))
          stringBuilder.Append(":" + charArray[index].ToString());
        else if (Versioned.IsNumeric((object) charArray[checked (index - 1)]))
          stringBuilder.Append(" " + charArray[index].ToString());
        else if (ttlAI.IsUpperCase(charArray[index].ToString()))
          stringBuilder.Append(":1 " + charArray[index].ToString());
        else
          stringBuilder.Append(charArray[index].ToString());
        checked { ++index; }
      }
      if (!Versioned.IsNumeric((object) charArray[checked (charArray.Length - 1)]))
        stringBuilder.Append(":1");
      return stringBuilder.ToString();
    }

    public static string TranslateLongToShortMF(string LongMF, string ShortDeli = "")
    {
label_1:
      int num1 = 0;
      string str1;
      int num2 = 0;
        StringBuilder stringBuilder = new StringBuilder();
      try
            {
        ProjectData.ClearProjectError();
        num1 = -2;
label_2:
        int num3 = 2;
        if (LongMF.Length > 0)
          goto label_4;
label_3:
        str1 = LongMF;
        goto label_24;
label_4:
        num3 = 5;
        string[] strArray1 = LongMF.Split(' ');
label_5:
        num3 = 6;
label_6:
        num3 = 7;
        int num4 = 0;
        int num5 = checked (strArray1.Length - 1);
        int index = num4;
        goto label_16;
label_7:
        num3 = 8;
        string[] strArray2 = strArray1[index].Split(':');
label_8:
        num3 = 9;
        stringBuilder.Append(strArray2[0]);
label_9:
        num3 = 10;
        if (strArray2.Length <= 1)
          goto label_12;
label_10:
        num3 = 11;
        if (int.Parse(strArray2[1]) <= 1)
          goto label_12;
label_11:
        num3 = 12;
        stringBuilder.Append(strArray2[1]);
label_12:
label_13:
        num3 = 15;
        if (index >= checked (strArray1.Length - 1))
          goto label_15;
label_14:
        num3 = 16;
        stringBuilder.Append(ShortDeli);
label_15:
        num3 = 18;
        checked { ++index; }
label_16:
        if (index <= num5)
          goto label_7;
label_17:
        num3 = 19;
        str1 = stringBuilder.ToString();
        goto label_24;
label_19:
        num2 = num3;
        switch (num1 > -2 ? num1 : 1)
        {
          case 1:
            int num6 = num2 + 1;
            num2 = 0;
            switch (num6)
            {
              case 1:
                goto label_1;
              case 2:
                goto label_2;
              case 3:
                goto label_3;
              case 4:
              case 5:
                goto label_4;
              case 6:
                goto label_5;
              case 7:
                goto label_6;
              case 8:
                goto label_7;
              case 9:
                goto label_8;
              case 10:
                goto label_9;
              case 11:
                goto label_10;
              case 12:
                goto label_11;
              case 13:
              case 14:
                goto label_12;
              case 15:
                goto label_13;
              case 16:
                goto label_14;
              case 17:
              case 18:
                goto label_15;
              case 19:
                goto label_17;
              case 20:
                goto label_24;
            }
        }
      }
      catch (Exception ex) when (ex is Exception & (uint) num1 > 0U & num2 == 0)
      {
        ProjectData.SetProjectError(ex);
          return stringBuilder.ToString();
      }
      throw ProjectData.CreateProjectError(-2146828237);
label_24:
      string str2 = str1;
      if (num2 == 0)
        return str2;
      ProjectData.ClearProjectError();
      return str2;
    }

    public static string EC4LongMF(string LongMF)
    {
label_1:
      int num1 = 0;
      string str1;
      int num2 = 0;
      StringBuilder stringBuilder = new StringBuilder();
      try
      {
        ProjectData.ClearProjectError();
        num1 = -2;
label_2:
        int num3 = 2;
        if (LongMF.Length > 0)
          goto label_4;
label_3:
        str1 = LongMF;
        goto label_30;
label_4:
        num3 = 5;
        string[] strArray1 = LongMF.Split(' ');
label_5:
        num3 = 6;
        string[] array = new string[checked (strArray1.Length - 1 + 1)];
label_6:
        num3 = 7;
        int num4 = 0;
        int num5 = checked (strArray1.Length - 1);
        int index = num4;
        goto label_10;
label_7:
        num3 = 8;
        string[] strArray2 = strArray1[index].Split(':');
label_8:
        num3 = 9;
        array[index] = ttlAI.AI_PureElement(strArray2[0]);
label_9:
        num3 = 10;
        checked { ++index; }
label_10:
        if (index <= num5)
          goto label_7;
label_11:
        num3 = 11;
        if (array.Length <= 1)
          goto label_21;
label_12:
        num3 = 12;
        Array.Sort<string>(array);
label_13:
        num3 = 13;
label_14:
        num3 = 14;
        int num6 = 1;
        int num7 = checked (array.Length - 1);
        index = num6;
        goto label_19;
label_15:
        num3 = 15;
        if (string.Compare(array[index], array[checked (index - 1)]) == 0)
          goto label_17;
label_16:
        num3 = 16;
        stringBuilder.Append(array[index]);
label_17:
label_18:
        num3 = 18;
        checked { ++index; }
label_19:
        if (index <= num7)
          goto label_15;
label_20:
        num3 = 19;
        str1 = stringBuilder.ToString();
        goto label_30;
label_21:
        num3 = 21;
label_22:
        num3 = 22;
        str1 = array[0];
        goto label_30;
label_25:
        num2 = num3;
        switch (num1 > -2 ? num1 : 1)
        {
          case 1:
            int num8 = num2 + 1;
            num2 = 0;
            switch (num8)
            {
              case 1:
                goto label_1;
              case 2:
                goto label_2;
              case 3:
                goto label_3;
              case 4:
              case 5:
                goto label_4;
              case 6:
                goto label_5;
              case 7:
                goto label_6;
              case 8:
                goto label_7;
              case 9:
                goto label_8;
              case 10:
                goto label_9;
              case 11:
                goto label_11;
              case 12:
                goto label_12;
              case 13:
                goto label_13;
              case 14:
                goto label_14;
              case 15:
                goto label_15;
              case 16:
                goto label_16;
              case 17:
                goto label_17;
              case 18:
                goto label_18;
              case 19:
                goto label_20;
              case 20:
              case 23:
                goto label_30;
              case 21:
                goto label_21;
              case 22:
                goto label_22;
              case 24:
                goto label_30;
            }
        }
      }
      catch (Exception ex) when (ex is Exception & (uint) num1 > 0U & num2 == 0)
      {
        ProjectData.SetProjectError(ex);
          return stringBuilder.ToString();
      }
      throw ProjectData.CreateProjectError(-2146828237);
label_30:
      string str2 = str1;
      if (num2 == 0)
        return str2;
      ProjectData.ClearProjectError();
      return str2;
    }

    public static string FragmentOfChoice(string s, int pos, string deli, bool ErrorValueIfNotNumeric, string ValueIfEmpty = "0", string ValueIfError = "-1", string ValueIfNothing = "0")
    {
      string str;
      try
      {
        if (Information.IsNothing((object) s))
          str = ValueIfNothing;
        else if (s.Length <= 0)
        {
          str = ValueIfEmpty;
        }
        else
        {
          string[] strArray = s.Split(Conversions.ToChar(deli));
          str = !ErrorValueIfNotNumeric ? strArray[pos] : (!Versioned.IsNumeric((object) strArray[pos]) ? ValueIfError : strArray[pos]);
        }
      }
      catch (Exception ex)
      {
        ProjectData.SetProjectError(ex);
        str = ValueIfError;
        ProjectData.ClearProjectError();
      }
      return str;
    }
  }
}

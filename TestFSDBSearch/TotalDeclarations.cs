// Decompiled with JetBrains decompiler
// Type: TestFSDBSearch.TotalDeclarations
// Assembly: TestFSDBSearch, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C36EBD2C-6652-4FDC-A74D-B15E46A79224
// Assembly location: F:\Documents\Projects\NikolaTolic\Formularity\Lib\TestFSDBSearch.exe

using Microsoft.VisualBasic.CompilerServices;
using System.Diagnostics;

namespace TestFSDBSearch
{
  [StandardModule]
  internal sealed class TotalDeclarations
  {
    public const int NOT_FOUND_IND = -1;
    public const double tiny = 1E-20;
    public const double huge = 1E+307;
    public const double gcr = 0.61803398875;
    public const double b2 = 1.902160583104;
    public const double M = 1000000.0;
    public const double E_MASS_U = 0.00054857990945;
    public const double P_MASS_U = 1.00727646688;
    public const double N_MASS_U = 1.0086649156;
    public const double H_MASS_U = 1.00782503214;
    public const string IM_sub_p = "-p";
    public const string IM_add_p = "+p";
    public const string IM_sub_e = "-e";
    public const string IM_add_e = "+e";
    public const string IM_none = "";
    public const string OP_ADD = "+";
    public const string OP_SUB = "-";
    public const string OP_MULTI = "*";
    public const string OP_PROD = "x";
    public const string OP_ZERO = "0";
    public const double dm1stIsotopic = 1.0033554;
    public const double dm2ndIsotopic = 2.006685;
    public const double KendrickMassConstantCH2 = 0.9988834;
    public const string TKN_EMPTY = "";
    public const string TKN_SPACE = " ";
    public const string TKN_COMMA = ",";
    public const string TKN_UNDERSCORE = "_";
    public const string TKN_DASH = "-";
    public const string TKN_EQUAL = "=";
    public const string TKN_COLON = ":";
    public const string TKN_SLASH = "/";
    public const string TKN_NA = "na";
    public const string FMT_6DP = "0.000000";
    public const string FMT_5DP = "0.00000";
    public const string FMT_4DP = "0.0000";
    public const string FMT_3DP = "0.000";
    public const string FMT_2DP = "0.00";
    public const string FMT_1DP = "0.0";
    public const string FMT_0DP = "0";
    public const string FMT_EXP = "0.00E0";
    public const string FMT_EXP_1DP = "0.0E0";
    public const string FMT_PCT = "0.00%";
    public const string HEAD_PEAKS = "ind,mz,intensity,sn,rel_int";
    public const string FILE_NAME_PREFIX_IPDB_scores = "s_ipdb";
    public const string FILE_NAME_PREFIX_IPDB_peaks = "p_ipdb";
    public const string FILE_NAME_Isotopes = "Isotope.inf";

    [DebuggerNonUserCode]
    static TotalDeclarations()
    {
    }

    public enum WordType
    {
      prefix = 1,
      suffix = 2,
    }
  }
}

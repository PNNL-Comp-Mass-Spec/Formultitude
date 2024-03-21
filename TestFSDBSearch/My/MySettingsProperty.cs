// Decompiled with JetBrains decompiler
// Type: TestFSDBSearch.My.MySettingsProperty
// Assembly: TestFSDBSearch, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C36EBD2C-6652-4FDC-A74D-B15E46A79224
// Assembly location: F:\Documents\Projects\NikolaTolic\Formultitude\Lib\TestFSDBSearch.exe

using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace TestFSDBSearch.My
{
  [DebuggerNonUserCode]
  [HideModuleName]
  [StandardModule]
  [CompilerGenerated]
  public sealed class MySettingsProperty
  {
    [HelpKeyword("My.Settings")]
    public static MySettings Settings
    {
      get
      {
        MySettings mySettings = MySettings.Default;
        return mySettings;
      }
    }
  }
}

// Decompiled with JetBrains decompiler
// Type: TestFSDBSearch.My.MySettings
// Assembly: TestFSDBSearch, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C36EBD2C-6652-4FDC-A74D-B15E46A79224
// Assembly location: F:\Documents\Projects\NikolaTolic\Formultitude\Lib\TestFSDBSearch.exe

using Microsoft.VisualBasic.ApplicationServices;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace TestFSDBSearch.My
{
  [GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "10.0.0.0")]
  [CompilerGenerated]
  [EditorBrowsable(EditorBrowsableState.Advanced)]
  public sealed class MySettings : ApplicationSettingsBase
  {
    private static MySettings defaultInstance = (MySettings) SettingsBase.Synchronized((SettingsBase) new MySettings());
    private static object addedHandlerLockObject = RuntimeHelpers.GetObjectValue(new object());
    private static bool addedHandler;

    [DebuggerNonUserCode]
    public MySettings()
    {
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    [DebuggerNonUserCode]
    private static void AutoSaveSettings(object sender, EventArgs e)
    {
      if (!MyProject.Application.SaveMySettingsOnExit)
        return;
      MySettingsProperty.Settings.Save();
    }

    public static MySettings Default
    {
      get
      {
        if (!MySettings.addedHandler)
        {
          object handlerLockObject = MySettings.addedHandlerLockObject;
          ObjectFlowControl.CheckForSyncLockOnValueType(handlerLockObject);
          bool lockTaken = false;
          try
          {
            Monitor.Enter(handlerLockObject, ref lockTaken);
            if (!MySettings.addedHandler)
            {
              MyProject.Application.Shutdown += (ShutdownEventHandler) ((sender, e) =>
              {
                if (!MyProject.Application.SaveMySettingsOnExit)
                  return;
                MySettingsProperty.Settings.Save();
              });
              MySettings.addedHandler = true;
            }
          }
          finally
          {
            if (lockTaken)
              Monitor.Exit(handlerLockObject);
          }
        }
        MySettings defaultInstance = MySettings.defaultInstance;
        return defaultInstance;
      }
    }
  }
}

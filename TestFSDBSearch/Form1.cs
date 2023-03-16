// Decompiled with JetBrains decompiler
// Type: TestFSDBSearch.Form1
// Assembly: TestFSDBSearch, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C36EBD2C-6652-4FDC-A74D-B15E46A79224
// Assembly location: F:\Documents\Projects\NikolaTolic\Formularity\Lib\TestFSDBSearch.exe

using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using TestFSDBSearch.TotalSupport;

namespace TestFSDBSearch
{
  [DesignerGenerated]
  public class Form1 : Form
  {
    private static List<WeakReference> __ENCList = new List<WeakReference>();
    private IContainer components;
    [AccessedThroughProperty("TabTest")]
    private TabControl _TabTest;
    [AccessedThroughProperty("TabCalibrationPage")]
    private TabPage _TabCalibrationPage;
    [AccessedThroughProperty("TabTestIPDB")]
    private TabPage _TabTestIPDB;
    [AccessedThroughProperty("cmdCalibrate")]
    private Button _cmdCalibrate;
    [AccessedThroughProperty("Label8")]
    private Label _Label8;
    [AccessedThroughProperty("txtMaxRelAbu")]
    private TextBox _txtMaxRelAbu;
    [AccessedThroughProperty("Label7")]
    private Label _Label7;
    [AccessedThroughProperty("txtCalFile")]
    private TextBox _txtCalFile;
    [AccessedThroughProperty("Label6")]
    private Label _Label6;
    [AccessedThroughProperty("txtMinRelAbu")]
    private TextBox _txtMinRelAbu;
    [AccessedThroughProperty("txtRF")]
    private TextBox _txtRF;
    [AccessedThroughProperty("txtStartTolerance")]
    private TextBox _txtStartTolerance;
    [AccessedThroughProperty("Label3")]
    private Label _Label3;
    [AccessedThroughProperty("Label5")]
    private Label _Label5;
    [AccessedThroughProperty("txtMinSN")]
    private TextBox _txtMinSN;
    [AccessedThroughProperty("Label1")]
    private Label _Label1;
    [AccessedThroughProperty("cmbRegression")]
    private ComboBox _cmbRegression;
    [AccessedThroughProperty("txtTargetTolerance")]
    private TextBox _txtTargetTolerance;
    [AccessedThroughProperty("Label2")]
    private Label _Label2;
    [AccessedThroughProperty("Label4")]
    private Label _Label4;
    [AccessedThroughProperty("txtResults")]
    private TextBox _txtResults;
    [AccessedThroughProperty("txtPeaksFile")]
    private TextBox _txtPeaksFile;
    [AccessedThroughProperty("txtIPDBMinorMinSN")]
    private TextBox _txtIPDBMinorMinSN;
    [AccessedThroughProperty("Label10")]
    private Label _Label10;
    [AccessedThroughProperty("Label9")]
    private Label _Label9;
    [AccessedThroughProperty("txtIPDBMajorMinSN")]
    private TextBox _txtIPDBMajorMinSN;
    [AccessedThroughProperty("Label13")]
    private Label _Label13;
    [AccessedThroughProperty("Label12")]
    private Label _Label12;
    [AccessedThroughProperty("Label14")]
    private Label _Label14;
    [AccessedThroughProperty("txtIPDBAdduct")]
    private TextBox _txtIPDBAdduct;
    [AccessedThroughProperty("txtIPDBChargeState")]
    private TextBox _txtIPDBChargeState;
    [AccessedThroughProperty("cmdIPDBSearch")]
    private Button _cmdIPDBSearch;
    [AccessedThroughProperty("cmdIPDBMergeWithCIA")]
    private Button _cmdIPDBMergeWithCIA;
    [AccessedThroughProperty("Label16")]
    private Label _Label16;
    [AccessedThroughProperty("txtIPDBMTol")]
    private TextBox _txtIPDBMTol;
    [AccessedThroughProperty("txtIPDBMinPA2Report")]
    private TextBox _txtIPDBMinPA2Report;
    [AccessedThroughProperty("Label15")]
    private Label _Label15;
    [AccessedThroughProperty("chkIPDBReportPeakHits")]
    private CheckBox _chkIPDBReportPeakHits;
    [AccessedThroughProperty("cmbIPDBIonization")]
    private ComboBox _cmbIPDBIonization;
    [AccessedThroughProperty("Label17")]
    private Label _Label17;
    [AccessedThroughProperty("txtIPDB_EC_Filter")]
    private TextBox _txtIPDB_EC_Filter;
    [AccessedThroughProperty("txtIPDB")]
    private TextBox _txtIPDB;
    [AccessedThroughProperty("txtIPDBMinPToScore")]
    private TextBox _txtIPDBMinPToScore;
    [AccessedThroughProperty("Label18")]
    private Label _Label18;
    [AccessedThroughProperty("TabTestFormula")]
    private TabPage _TabTestFormula;
    [AccessedThroughProperty("cmdTestMF")]
    private Button _cmdTestMF;
    [AccessedThroughProperty("Label23")]
    private Label _Label23;
    [AccessedThroughProperty("txtTestMF")]
    private TextBox _txtTestMF;
    [AccessedThroughProperty("txtTestAdduct")]
    private TextBox _txtTestAdduct;
    [AccessedThroughProperty("txtTestCS")]
    private TextBox _txtTestCS;
    [AccessedThroughProperty("Label20")]
    private Label _Label20;
    [AccessedThroughProperty("Label21")]
    private Label _Label21;
    [AccessedThroughProperty("cmbTestIonization")]
    private ComboBox _cmbTestIonization;
    [AccessedThroughProperty("Label19")]
    private Label _Label19;
    [AccessedThroughProperty("cmdIPDBmzCalc")]
    private Button _cmdIPDBmzCalc;
    private string m_peaks_file_name;
    private double[] peak_m;
    private double[] peak_a;
    private double[] peak_sn;
    private double max_a;
    private double[] peak_m_cal;
    private TotalCalibration my_cal;
    private TotalIPDBSearch myIPDB;
    private TotalMFMonoPeakCalculator myMFCalc;
    private string IPDB_out_file_name;

    [DebuggerNonUserCode]
    static Form1()
    {
    }

    [DebuggerNonUserCode]
    private static void __ENCAddToList(object value)
    {
      lock (Form1.__ENCList)
      {
        if (Form1.__ENCList.Count == Form1.__ENCList.Capacity)
        {
          int index1 = 0;
          int num1 = 0;
          int num2 = checked (Form1.__ENCList.Count - 1);
          int index2 = num1;
          while (index2 <= num2)
          {
            if (Form1.__ENCList[index2].IsAlive)
            {
              if (index2 != index1)
                Form1.__ENCList[index1] = Form1.__ENCList[index2];
              checked { ++index1; }
            }
            checked { ++index2; }
          }
          Form1.__ENCList.RemoveRange(index1, checked (Form1.__ENCList.Count - index1));
          Form1.__ENCList.Capacity = Form1.__ENCList.Count;
        }
        Form1.__ENCList.Add(new WeakReference(RuntimeHelpers.GetObjectValue(value)));
      }
    }

    [DebuggerNonUserCode]
    protected override void Dispose(bool disposing)
    {
      try
      {
        if ((!disposing || this.components == null) && !false)
          return;
        this.components.Dispose();
      }
      finally
      {
        base.Dispose(disposing);
      }
    }

    [DebuggerStepThrough]
    private void InitializeComponent()
    {
      this.TabTest = new TabControl();
      this.TabCalibrationPage = new TabPage();
      this.cmdCalibrate = new Button();
      this.Label8 = new Label();
      this.txtMaxRelAbu = new TextBox();
      this.Label7 = new Label();
      this.txtCalFile = new TextBox();
      this.Label6 = new Label();
      this.txtMinRelAbu = new TextBox();
      this.txtRF = new TextBox();
      this.txtStartTolerance = new TextBox();
      this.Label3 = new Label();
      this.Label5 = new Label();
      this.txtMinSN = new TextBox();
      this.Label1 = new Label();
      this.cmbRegression = new ComboBox();
      this.txtTargetTolerance = new TextBox();
      this.Label2 = new Label();
      this.Label4 = new Label();
      this.TabTestIPDB = new TabPage();
      this.txtIPDBMinPToScore = new TextBox();
      this.Label18 = new Label();
      this.Label17 = new Label();
      this.txtIPDB_EC_Filter = new TextBox();
      this.cmbIPDBIonization = new ComboBox();
      this.chkIPDBReportPeakHits = new CheckBox();
      this.Label16 = new Label();
      this.txtIPDBMTol = new TextBox();
      this.txtIPDBMinPA2Report = new TextBox();
      this.Label15 = new Label();
      this.cmdIPDBMergeWithCIA = new Button();
      this.cmdIPDBSearch = new Button();
      this.Label14 = new Label();
      this.txtIPDBAdduct = new TextBox();
      this.txtIPDBChargeState = new TextBox();
      this.Label13 = new Label();
      this.Label12 = new Label();
      this.txtIPDBMinorMinSN = new TextBox();
      this.Label10 = new Label();
      this.Label9 = new Label();
      this.txtIPDBMajorMinSN = new TextBox();
      this.txtIPDB = new TextBox();
      this.TabTestFormula = new TabPage();
      this.cmbTestIonization = new ComboBox();
      this.Label19 = new Label();
      this.cmdTestMF = new Button();
      this.Label23 = new Label();
      this.txtTestMF = new TextBox();
      this.txtTestAdduct = new TextBox();
      this.txtTestCS = new TextBox();
      this.Label20 = new Label();
      this.Label21 = new Label();
      this.txtResults = new TextBox();
      this.txtPeaksFile = new TextBox();
      this.cmdIPDBmzCalc = new Button();
      this.TabTest.SuspendLayout();
      this.TabCalibrationPage.SuspendLayout();
      this.TabTestIPDB.SuspendLayout();
      this.TabTestFormula.SuspendLayout();
      this.SuspendLayout();
      this.TabTest.Controls.Add((Control) this.TabCalibrationPage);
      this.TabTest.Controls.Add((Control) this.TabTestIPDB);
      this.TabTest.Controls.Add((Control) this.TabTestFormula);
      TabControl tabTest1 = this.TabTest;
      Point point1 = new Point(12, 12);
      Point point2 = point1;
      tabTest1.Location = point2;
      this.TabTest.Name = "TabTest";
      this.TabTest.SelectedIndex = 0;
      TabControl tabTest2 = this.TabTest;
      Size size1 = new Size(624, 284);
      Size size2 = size1;
      tabTest2.Size = size2;
      this.TabTest.TabIndex = 0;
      this.TabCalibrationPage.Controls.Add((Control) this.cmdCalibrate);
      this.TabCalibrationPage.Controls.Add((Control) this.Label8);
      this.TabCalibrationPage.Controls.Add((Control) this.txtMaxRelAbu);
      this.TabCalibrationPage.Controls.Add((Control) this.Label7);
      this.TabCalibrationPage.Controls.Add((Control) this.txtCalFile);
      this.TabCalibrationPage.Controls.Add((Control) this.Label6);
      this.TabCalibrationPage.Controls.Add((Control) this.txtMinRelAbu);
      this.TabCalibrationPage.Controls.Add((Control) this.txtRF);
      this.TabCalibrationPage.Controls.Add((Control) this.txtStartTolerance);
      this.TabCalibrationPage.Controls.Add((Control) this.Label3);
      this.TabCalibrationPage.Controls.Add((Control) this.Label5);
      this.TabCalibrationPage.Controls.Add((Control) this.txtMinSN);
      this.TabCalibrationPage.Controls.Add((Control) this.Label1);
      this.TabCalibrationPage.Controls.Add((Control) this.cmbRegression);
      this.TabCalibrationPage.Controls.Add((Control) this.txtTargetTolerance);
      this.TabCalibrationPage.Controls.Add((Control) this.Label2);
      this.TabCalibrationPage.Controls.Add((Control) this.Label4);
      TabPage tabCalibrationPage1 = this.TabCalibrationPage;
      point1 = new Point(4, 22);
      Point point3 = point1;
      tabCalibrationPage1.Location = point3;
      this.TabCalibrationPage.Name = "TabCalibrationPage";
      TabPage tabCalibrationPage2 = this.TabCalibrationPage;
      Padding padding1 = new Padding(3);
      Padding padding2 = padding1;
      tabCalibrationPage2.Padding = padding2;
      TabPage tabCalibrationPage3 = this.TabCalibrationPage;
      size1 = new Size(616, 258);
      Size size3 = size1;
      tabCalibrationPage3.Size = size3;
      this.TabCalibrationPage.TabIndex = 0;
      this.TabCalibrationPage.Text = "Calibration";
      this.TabCalibrationPage.UseVisualStyleBackColor = true;
      Button cmdCalibrate1 = this.cmdCalibrate;
      point1 = new Point(511, 178);
      Point point4 = point1;
      cmdCalibrate1.Location = point4;
      this.cmdCalibrate.Name = "cmdCalibrate";
      Button cmdCalibrate2 = this.cmdCalibrate;
      size1 = new Size(64, 30);
      Size size4 = size1;
      cmdCalibrate2.Size = size4;
      this.cmdCalibrate.TabIndex = 11;
      this.cmdCalibrate.Text = "calibrate";
      this.cmdCalibrate.UseVisualStyleBackColor = true;
      this.Label8.AutoSize = true;
      Label label8_1 = this.Label8;
      point1 = new Point(360, 91);
      Point point5 = point1;
      label8_1.Location = point5;
      this.Label8.Name = "Label8";
      Label label8_2 = this.Label8;
      size1 = new Size(64, 13);
      Size size5 = size1;
      label8_2.Size = size5;
      this.Label8.TabIndex = 34;
      this.Label8.Text = "max rel.abu.";
      TextBox txtMaxRelAbu1 = this.txtMaxRelAbu;
      point1 = new Point(438, 88);
      Point point6 = point1;
      txtMaxRelAbu1.Location = point6;
      this.txtMaxRelAbu.Name = "txtMaxRelAbu";
      TextBox txtMaxRelAbu2 = this.txtMaxRelAbu;
      size1 = new Size(71, 20);
      Size size6 = size1;
      txtMaxRelAbu2.Size = size6;
      this.txtMaxRelAbu.TabIndex = 33;
      this.Label7.AutoSize = true;
      Label label7_1 = this.Label7;
      point1 = new Point(359, 68);
      Point point7 = point1;
      label7_1.Location = point7;
      this.Label7.Name = "Label7";
      Label label7_2 = this.Label7;
      size1 = new Size(61, 13);
      Size size7 = size1;
      label7_2.Size = size7;
      this.Label7.TabIndex = 32;
      this.Label7.Text = "min rel.abu.";
      this.txtCalFile.AllowDrop = true;
      TextBox txtCalFile1 = this.txtCalFile;
      point1 = new Point(22, 118);
      Point point8 = point1;
      txtCalFile1.Location = point8;
      this.txtCalFile.Name = "txtCalFile";
      TextBox txtCalFile2 = this.txtCalFile;
      size1 = new Size(553, 20);
      Size size8 = size1;
      txtCalFile2.Size = size8;
      this.txtCalFile.TabIndex = 19;
      this.txtCalFile.Text = "drop calibration file";
      this.Label6.AutoSize = true;
      Label label6_1 = this.Label6;
      point1 = new Point(359, 22);
      Point point9 = point1;
      label6_1.Location = point9;
      this.Label6.Name = "Label6";
      Label label6_2 = this.Label6;
      size1 = new Size(178, 13);
      Size size9 = size1;
      label6_2.Size = size9;
      this.Label6.TabIndex = 30;
      this.Label6.Text = "peaks filter for calibration calculation";
      TextBox txtMinRelAbu1 = this.txtMinRelAbu;
      point1 = new Point(437, 65);
      Point point10 = point1;
      txtMinRelAbu1.Location = point10;
      this.txtMinRelAbu.Name = "txtMinRelAbu";
      TextBox txtMinRelAbu2 = this.txtMinRelAbu;
      size1 = new Size(72, 20);
      Size size10 = size1;
      txtMinRelAbu2.Size = size10;
      this.txtMinRelAbu.TabIndex = 31;
      TextBox txtRf1 = this.txtRF;
      point1 = new Point(155, 92);
      Point point11 = point1;
      txtRf1.Location = point11;
      this.txtRF.Name = "txtRF";
      TextBox txtRf2 = this.txtRF;
      size1 = new Size(72, 20);
      Size size11 = size1;
      txtRf2.Size = size11;
      this.txtRF.TabIndex = 29;
      TextBox txtStartTolerance1 = this.txtStartTolerance;
      point1 = new Point(155, 45);
      Point point12 = point1;
      txtStartTolerance1.Location = point12;
      this.txtStartTolerance.Name = "txtStartTolerance";
      TextBox txtStartTolerance2 = this.txtStartTolerance;
      size1 = new Size(72, 20);
      Size size12 = size1;
      txtStartTolerance2.Size = size12;
      this.txtStartTolerance.TabIndex = 20;
      this.Label3.AutoSize = true;
      Label label3_1 = this.Label3;
      point1 = new Point(359, 45);
      Point point13 = point1;
      label3_1.Location = point13;
      this.Label3.Name = "Label3";
      Label label3_2 = this.Label3;
      size1 = new Size(42, 13);
      Size size13 = size1;
      label3_2.Size = size13;
      this.Label3.TabIndex = 25;
      this.Label3.Text = "min s/n";
      this.Label5.AutoSize = true;
      Label label5_1 = this.Label5;
      point1 = new Point(21, 95);
      Point point14 = point1;
      label5_1.Location = point14;
      this.Label5.Name = "Label5";
      Label label5_2 = this.Label5;
      size1 = new Size(71, 13);
      Size size14 = size1;
      label5_2.Size = size14;
      this.Label5.TabIndex = 28;
      this.Label5.Text = "relative factor";
      TextBox txtMinSn1 = this.txtMinSN;
      point1 = new Point(437, 42);
      Point point15 = point1;
      txtMinSn1.Location = point15;
      this.txtMinSN.Name = "txtMinSN";
      TextBox txtMinSn2 = this.txtMinSN;
      size1 = new Size(72, 20);
      Size size15 = size1;
      txtMinSn2.Size = size15;
      this.txtMinSN.TabIndex = 24;
      this.Label1.AutoSize = true;
      Label label1_1 = this.Label1;
      point1 = new Point(19, 48);
      Point point16 = point1;
      label1_1.Location = point16;
      this.Label1.Name = "Label1";
      Label label1_2 = this.Label1;
      size1 = new Size(103, 13);
      Size size16 = size1;
      label1_2.Size = size16;
      this.Label1.TabIndex = 21;
      this.Label1.Text = "start tolerance (ppm)";
      this.cmbRegression.FormattingEnabled = true;
      ComboBox cmbRegression1 = this.cmbRegression;
      point1 = new Point(126, 19);
      Point point17 = point1;
      cmbRegression1.Location = point17;
      this.cmbRegression.Name = "cmbRegression";
      ComboBox cmbRegression2 = this.cmbRegression;
      size1 = new Size(121, 21);
      Size size17 = size1;
      cmbRegression2.Size = size17;
      this.cmbRegression.TabIndex = 27;
      TextBox txtTargetTolerance1 = this.txtTargetTolerance;
      point1 = new Point(155, 69);
      Point point18 = point1;
      txtTargetTolerance1.Location = point18;
      this.txtTargetTolerance.Name = "txtTargetTolerance";
      TextBox txtTargetTolerance2 = this.txtTargetTolerance;
      size1 = new Size(72, 20);
      Size size18 = size1;
      txtTargetTolerance2.Size = size18;
      this.txtTargetTolerance.TabIndex = 22;
      this.Label2.AutoSize = true;
      Label label2_1 = this.Label2;
      point1 = new Point(19, 72);
      Point point19 = point1;
      label2_1.Location = point19;
      this.Label2.Name = "Label2";
      Label label2_2 = this.Label2;
      size1 = new Size(101, 13);
      Size size19 = size1;
      label2_2.Size = size19;
      this.Label2.TabIndex = 23;
      this.Label2.Text = "end tolerance (ppm)";
      this.Label4.AutoSize = true;
      Label label4_1 = this.Label4;
      point1 = new Point(21, 23);
      Point point20 = point1;
      label4_1.Location = point20;
      this.Label4.Name = "Label4";
      Label label4_2 = this.Label4;
      size1 = new Size(86, 13);
      Size size20 = size1;
      label4_2.Size = size20;
      this.Label4.TabIndex = 26;
      this.Label4.Text = "regression model";
      this.TabTestIPDB.Controls.Add((Control) this.cmdIPDBmzCalc);
      this.TabTestIPDB.Controls.Add((Control) this.txtIPDBMinPToScore);
      this.TabTestIPDB.Controls.Add((Control) this.Label18);
      this.TabTestIPDB.Controls.Add((Control) this.Label17);
      this.TabTestIPDB.Controls.Add((Control) this.txtIPDB_EC_Filter);
      this.TabTestIPDB.Controls.Add((Control) this.cmbIPDBIonization);
      this.TabTestIPDB.Controls.Add((Control) this.chkIPDBReportPeakHits);
      this.TabTestIPDB.Controls.Add((Control) this.Label16);
      this.TabTestIPDB.Controls.Add((Control) this.txtIPDBMTol);
      this.TabTestIPDB.Controls.Add((Control) this.txtIPDBMinPA2Report);
      this.TabTestIPDB.Controls.Add((Control) this.Label15);
      this.TabTestIPDB.Controls.Add((Control) this.cmdIPDBMergeWithCIA);
      this.TabTestIPDB.Controls.Add((Control) this.cmdIPDBSearch);
      this.TabTestIPDB.Controls.Add((Control) this.Label14);
      this.TabTestIPDB.Controls.Add((Control) this.txtIPDBAdduct);
      this.TabTestIPDB.Controls.Add((Control) this.txtIPDBChargeState);
      this.TabTestIPDB.Controls.Add((Control) this.Label13);
      this.TabTestIPDB.Controls.Add((Control) this.Label12);
      this.TabTestIPDB.Controls.Add((Control) this.txtIPDBMinorMinSN);
      this.TabTestIPDB.Controls.Add((Control) this.Label10);
      this.TabTestIPDB.Controls.Add((Control) this.Label9);
      this.TabTestIPDB.Controls.Add((Control) this.txtIPDBMajorMinSN);
      this.TabTestIPDB.Controls.Add((Control) this.txtIPDB);
      TabPage tabTestIpdb1 = this.TabTestIPDB;
      point1 = new Point(4, 22);
      Point point21 = point1;
      tabTestIpdb1.Location = point21;
      this.TabTestIPDB.Name = "TabTestIPDB";
      TabPage tabTestIpdb2 = this.TabTestIPDB;
      padding1 = new Padding(3);
      Padding padding3 = padding1;
      tabTestIpdb2.Padding = padding3;
      TabPage tabTestIpdb3 = this.TabTestIPDB;
      size1 = new Size(616, 258);
      Size size21 = size1;
      tabTestIpdb3.Size = size21;
      this.TabTestIPDB.TabIndex = 1;
      this.TabTestIPDB.Text = "IPDB Survey";
      this.TabTestIPDB.UseVisualStyleBackColor = true;
      TextBox txtIpdbMinPtoScore1 = this.txtIPDBMinPToScore;
      point1 = new Point(376, 119);
      Point point22 = point1;
      txtIpdbMinPtoScore1.Location = point22;
      this.txtIPDBMinPToScore.Name = "txtIPDBMinPToScore";
      TextBox txtIpdbMinPtoScore2 = this.txtIPDBMinPToScore;
      size1 = new Size(31, 20);
      Size size22 = size1;
      txtIpdbMinPtoScore2.Size = size22;
      this.txtIPDBMinPToScore.TabIndex = 45;
      this.txtIPDBMinPToScore.Text = "0.001";
      this.Label18.AutoSize = true;
      Label label18_1 = this.Label18;
      point1 = new Point(227, 125);
      Point point23 = point1;
      label18_1.Location = point23;
      this.Label18.Name = "Label18";
      Label label18_2 = this.Label18;
      size1 = new Size(141, 13);
      Size size23 = size1;
      label18_2.Size = size23;
      this.Label18.TabIndex = 44;
      this.Label18.Text = "min.peak probability to score";
      this.Label17.AutoSize = true;
      Label label17_1 = this.Label17;
      point1 = new Point(30, 198);
      Point point24 = point1;
      label17_1.Location = point24;
      this.Label17.Name = "Label17";
      Label label17_2 = this.Label17;
      size1 = new Size(73, 13);
      Size size24 = size1;
      label17_2.Size = size24;
      this.Label17.TabIndex = 42;
      this.Label17.Text = "database filter";
      this.txtIPDB_EC_Filter.AllowDrop = true;
      TextBox txtIpdbEcFilter1 = this.txtIPDB_EC_Filter;
      point1 = new Point(116, 198);
      Point point25 = point1;
      txtIpdbEcFilter1.Location = point25;
      this.txtIPDB_EC_Filter.Name = "txtIPDB_EC_Filter";
      TextBox txtIpdbEcFilter2 = this.txtIPDB_EC_Filter;
      size1 = new Size(465, 20);
      Size size25 = size1;
      txtIpdbEcFilter2.Size = size25;
      this.txtIPDB_EC_Filter.TabIndex = 41;
      this.txtIPDB_EC_Filter.Text = "r/C,r/H,o/Cl,o/Br,o/F,x/N,x/S,x/P,e/CHNOP";
      this.cmbIPDBIonization.FormattingEnabled = true;
      ComboBox cmbIpdbIonization1 = this.cmbIPDBIonization;
      point1 = new Point(105, 29);
      Point point26 = point1;
      cmbIpdbIonization1.Location = point26;
      this.cmbIPDBIonization.Name = "cmbIPDBIonization";
      ComboBox cmbIpdbIonization2 = this.cmbIPDBIonization;
      size1 = new Size(56, 21);
      Size size26 = size1;
      cmbIpdbIonization2.Size = size26;
      this.cmbIPDBIonization.TabIndex = 40;
      this.chkIPDBReportPeakHits.AutoSize = true;
      this.chkIPDBReportPeakHits.Checked = true;
      this.chkIPDBReportPeakHits.CheckState = CheckState.Checked;
      CheckBox ipdbReportPeakHits1 = this.chkIPDBReportPeakHits;
      point1 = new Point(435, 17);
      Point point27 = point1;
      ipdbReportPeakHits1.Location = point27;
      this.chkIPDBReportPeakHits.Name = "chkIPDBReportPeakHits";
      this.chkIPDBReportPeakHits.RightToLeft = RightToLeft.Yes;
      CheckBox ipdbReportPeakHits2 = this.chkIPDBReportPeakHits;
      size1 = new Size((int) sbyte.MaxValue, 17);
      Size size27 = size1;
      ipdbReportPeakHits2.Size = size27;
      this.chkIPDBReportPeakHits.TabIndex = 39;
      this.chkIPDBReportPeakHits.Text = "output matched peak";
      this.chkIPDBReportPeakHits.TextAlign = ContentAlignment.MiddleRight;
      this.chkIPDBReportPeakHits.UseVisualStyleBackColor = true;
      this.Label16.AutoSize = true;
      Label label16_1 = this.Label16;
      point1 = new Point(228, 14);
      Point point28 = point1;
      label16_1.Location = point28;
      this.Label16.Name = "Label16";
      Label label16_2 = this.Label16;
      size1 = new Size(107, 13);
      Size size28 = size1;
      label16_2.Size = size28;
      this.Label16.TabIndex = 38;
      this.Label16.Text = "mass tolerance (ppm)";
      TextBox txtIpdbmTol1 = this.txtIPDBMTol;
      point1 = new Point(377, 11);
      Point point29 = point1;
      txtIpdbmTol1.Location = point29;
      this.txtIPDBMTol.Name = "txtIPDBMTol";
      TextBox txtIpdbmTol2 = this.txtIPDBMTol;
      size1 = new Size(31, 20);
      Size size29 = size1;
      txtIpdbmTol2.Size = size29;
      this.txtIPDBMTol.TabIndex = 37;
      this.txtIPDBMTol.Text = "0.5";
      this.txtIPDBMinPA2Report.Enabled = false;
      TextBox ipdbMinPa2Report1 = this.txtIPDBMinPA2Report;
      point1 = new Point(377, 90);
      Point point30 = point1;
      ipdbMinPa2Report1.Location = point30;
      this.txtIPDBMinPA2Report.Name = "txtIPDBMinPA2Report";
      TextBox ipdbMinPa2Report2 = this.txtIPDBMinPA2Report;
      size1 = new Size(31, 20);
      Size size30 = size1;
      ipdbMinPa2Report2.Size = size30;
      this.txtIPDBMinPA2Report.TabIndex = 36;
      this.txtIPDBMinPA2Report.Text = "2";
      this.Label15.AutoSize = true;
      this.Label15.Enabled = false;
      Label label15_1 = this.Label15;
      point1 = new Point(228, 96);
      Point point31 = point1;
      label15_1.Location = point31;
      this.Label15.Name = "Label15";
      Label label15_2 = this.Label15;
      size1 = new Size(88, 13);
      Size size31 = size1;
      label15_2.Size = size31;
      this.Label15.TabIndex = 35;
      this.Label15.Text = "min. p/a to report";
      Button ipdbMergeWithCia1 = this.cmdIPDBMergeWithCIA;
      point1 = new Point(447, 83);
      Point point32 = point1;
      ipdbMergeWithCia1.Location = point32;
      this.cmdIPDBMergeWithCIA.Name = "cmdIPDBMergeWithCIA";
      Button ipdbMergeWithCia2 = this.cmdIPDBMergeWithCIA;
      size1 = new Size(99, 30);
      Size size32 = size1;
      ipdbMergeWithCia2.Size = size32;
      this.cmdIPDBMergeWithCIA.TabIndex = 34;
      this.cmdIPDBMergeWithCIA.Text = "merge with CIA";
      this.cmdIPDBMergeWithCIA.UseVisualStyleBackColor = true;
      Button cmdIpdbSearch1 = this.cmdIPDBSearch;
      point1 = new Point(447, 47);
      Point point33 = point1;
      cmdIpdbSearch1.Location = point33;
      this.cmdIPDBSearch.Name = "cmdIPDBSearch";
      Button cmdIpdbSearch2 = this.cmdIPDBSearch;
      size1 = new Size(99, 30);
      Size size33 = size1;
      cmdIpdbSearch2.Size = size33;
      this.cmdIPDBSearch.TabIndex = 33;
      this.cmdIPDBSearch.Text = "search";
      this.cmdIPDBSearch.UseVisualStyleBackColor = true;
      this.Label14.AutoSize = true;
      Label label14_1 = this.Label14;
      point1 = new Point(25, 37);
      Point point34 = point1;
      label14_1.Location = point34;
      this.Label14.Name = "Label14";
      Label label14_2 = this.Label14;
      size1 = new Size(51, 13);
      Size size34 = size1;
      label14_2.Size = size34;
      this.Label14.TabIndex = 31;
      this.Label14.Text = "ionization";
      TextBox txtIpdbAdduct1 = this.txtIPDBAdduct;
      point1 = new Point(105, 89);
      Point point35 = point1;
      txtIpdbAdduct1.Location = point35;
      this.txtIPDBAdduct.Name = "txtIPDBAdduct";
      TextBox txtIpdbAdduct2 = this.txtIPDBAdduct;
      size1 = new Size(56, 20);
      Size size35 = size1;
      txtIpdbAdduct2.Size = size35;
      this.txtIPDBAdduct.TabIndex = 30;
      this.txtIPDBAdduct.Text = "H";
      TextBox txtIpdbChargeState1 = this.txtIPDBChargeState;
      point1 = new Point(105, 63);
      Point point36 = point1;
      txtIpdbChargeState1.Location = point36;
      this.txtIPDBChargeState.Name = "txtIPDBChargeState";
      TextBox txtIpdbChargeState2 = this.txtIPDBChargeState;
      size1 = new Size(56, 20);
      Size size36 = size1;
      txtIpdbChargeState2.Size = size36;
      this.txtIPDBChargeState.TabIndex = 29;
      this.txtIPDBChargeState.Text = "1";
      this.Label13.AutoSize = true;
      Label label13_1 = this.Label13;
      point1 = new Point(23, 93);
      Point point37 = point1;
      label13_1.Location = point37;
      this.Label13.Name = "Label13";
      Label label13_2 = this.Label13;
      size1 = new Size(40, 13);
      Size size37 = size1;
      label13_2.Size = size37;
      this.Label13.TabIndex = 27;
      this.Label13.Text = "adduct";
      this.Label12.AutoSize = true;
      Label label12_1 = this.Label12;
      point1 = new Point(23, 68);
      Point point38 = point1;
      label12_1.Location = point38;
      this.Label12.Name = "Label12";
      Label label12_2 = this.Label12;
      size1 = new Size(66, 13);
      Size size38 = size1;
      label12_2.Size = size38;
      this.Label12.TabIndex = 26;
      this.Label12.Text = "charge state";
      TextBox txtIpdbMinorMinSn1 = this.txtIPDBMinorMinSN;
      point1 = new Point(377, 64);
      Point point39 = point1;
      txtIpdbMinorMinSn1.Location = point39;
      this.txtIPDBMinorMinSN.Name = "txtIPDBMinorMinSN";
      TextBox txtIpdbMinorMinSn2 = this.txtIPDBMinorMinSN;
      size1 = new Size(31, 20);
      Size size39 = size1;
      txtIpdbMinorMinSn2.Size = size39;
      this.txtIPDBMinorMinSN.TabIndex = 24;
      this.txtIPDBMinorMinSN.Text = "5";
      this.Label10.AutoSize = true;
      Label label10_1 = this.Label10;
      point1 = new Point(228, 68);
      Point point40 = point1;
      label10_1.Location = point40;
      this.Label10.Name = "Label10";
      Label label10_2 = this.Label10;
      size1 = new Size(102, 13);
      Size size40 = size1;
      label10_2.Size = size40;
      this.Label10.TabIndex = 23;
      this.Label10.Text = "minor peaks min s/n";
      this.Label9.AutoSize = true;
      Label label9_1 = this.Label9;
      point1 = new Point(228, 41);
      Point point41 = point1;
      label9_1.Location = point41;
      this.Label9.Name = "Label9";
      Label label9_2 = this.Label9;
      size1 = new Size(102, 13);
      Size size41 = size1;
      label9_2.Size = size41;
      this.Label9.TabIndex = 22;
      this.Label9.Text = "major peaks min s/n";
      TextBox txtIpdbMajorMinSn1 = this.txtIPDBMajorMinSN;
      point1 = new Point(377, 38);
      Point point42 = point1;
      txtIpdbMajorMinSn1.Location = point42;
      this.txtIPDBMajorMinSN.Name = "txtIPDBMajorMinSN";
      TextBox txtIpdbMajorMinSn2 = this.txtIPDBMajorMinSN;
      size1 = new Size(31, 20);
      Size size42 = size1;
      txtIpdbMajorMinSn2.Size = size42;
      this.txtIPDBMajorMinSN.TabIndex = 21;
      this.txtIPDBMajorMinSN.Text = "7";
      this.txtIPDB.AllowDrop = true;
      TextBox txtIpdb1 = this.txtIPDB;
      point1 = new Point(28, 162);
      Point point43 = point1;
      txtIpdb1.Location = point43;
      this.txtIPDB.Name = "txtIPDB";
      TextBox txtIpdb2 = this.txtIPDB;
      size1 = new Size(553, 20);
      Size size43 = size1;
      txtIpdb2.Size = size43;
      this.txtIPDB.TabIndex = 20;
      this.txtIPDB.Text = "drop database file";
      this.TabTestFormula.Controls.Add((Control) this.cmbTestIonization);
      this.TabTestFormula.Controls.Add((Control) this.Label19);
      this.TabTestFormula.Controls.Add((Control) this.cmdTestMF);
      this.TabTestFormula.Controls.Add((Control) this.Label23);
      this.TabTestFormula.Controls.Add((Control) this.txtTestMF);
      this.TabTestFormula.Controls.Add((Control) this.txtTestAdduct);
      this.TabTestFormula.Controls.Add((Control) this.txtTestCS);
      this.TabTestFormula.Controls.Add((Control) this.Label20);
      this.TabTestFormula.Controls.Add((Control) this.Label21);
      TabPage tabTestFormula1 = this.TabTestFormula;
      point1 = new Point(4, 22);
      Point point44 = point1;
      tabTestFormula1.Location = point44;
      this.TabTestFormula.Name = "TabTestFormula";
      TabPage tabTestFormula2 = this.TabTestFormula;
      padding1 = new Padding(3);
      Padding padding4 = padding1;
      tabTestFormula2.Padding = padding4;
      TabPage tabTestFormula3 = this.TabTestFormula;
      size1 = new Size(616, 258);
      Size size44 = size1;
      tabTestFormula3.Size = size44;
      this.TabTestFormula.TabIndex = 2;
      this.TabTestFormula.Text = "Test Formula";
      this.TabTestFormula.UseVisualStyleBackColor = true;
      this.cmbTestIonization.FormattingEnabled = true;
      ComboBox cmbTestIonization1 = this.cmbTestIonization;
      point1 = new Point(138, 62);
      Point point45 = point1;
      cmbTestIonization1.Location = point45;
      this.cmbTestIonization.Name = "cmbTestIonization";
      ComboBox cmbTestIonization2 = this.cmbTestIonization;
      size1 = new Size(55, 21);
      Size size45 = size1;
      cmbTestIonization2.Size = size45;
      this.cmbTestIonization.TabIndex = 59;
      this.Label19.AutoSize = true;
      Label label19_1 = this.Label19;
      point1 = new Point(47, 69);
      Point point46 = point1;
      label19_1.Location = point46;
      this.Label19.Name = "Label19";
      Label label19_2 = this.Label19;
      size1 = new Size(51, 13);
      Size size46 = size1;
      label19_2.Size = size46;
      this.Label19.TabIndex = 58;
      this.Label19.Text = "ionization";
      Button cmdTestMf1 = this.cmdTestMF;
      point1 = new Point(411, 162);
      Point point47 = point1;
      cmdTestMf1.Location = point47;
      this.cmdTestMF.Name = "cmdTestMF";
      Button cmdTestMf2 = this.cmdTestMF;
      size1 = new Size(88, 25);
      Size size47 = size1;
      cmdTestMf2.Size = size47;
      this.cmdTestMF.TabIndex = 57;
      this.cmdTestMF.Text = "test";
      this.cmdTestMF.UseVisualStyleBackColor = true;
      this.Label23.AutoSize = true;
      Label label23_1 = this.Label23;
      point1 = new Point(47, 167);
      Point point48 = point1;
      label23_1.Location = point48;
      this.Label23.Name = "Label23";
      Label label23_2 = this.Label23;
      size1 = new Size(41, 13);
      Size size48 = size1;
      label23_2.Size = size48;
      this.Label23.TabIndex = 56;
      this.Label23.Text = "formula";
      TextBox txtTestMf1 = this.txtTestMF;
      point1 = new Point(138, 165);
      Point point49 = point1;
      txtTestMf1.Location = point49;
      this.txtTestMF.Name = "txtTestMF";
      TextBox txtTestMf2 = this.txtTestMF;
      size1 = new Size(228, 20);
      Size size49 = size1;
      txtTestMf2.Size = size49;
      this.txtTestMF.TabIndex = 55;
      this.txtTestMF.Text = "C16H32O2";
      TextBox txtTestAdduct1 = this.txtTestAdduct;
      point1 = new Point(138, 88);
      Point point50 = point1;
      txtTestAdduct1.Location = point50;
      this.txtTestAdduct.Name = "txtTestAdduct";
      TextBox txtTestAdduct2 = this.txtTestAdduct;
      size1 = new Size(55, 20);
      Size size50 = size1;
      txtTestAdduct2.Size = size50;
      this.txtTestAdduct.TabIndex = 51;
      TextBox txtTestCs1 = this.txtTestCS;
      point1 = new Point(138, 114);
      Point point51 = point1;
      txtTestCs1.Location = point51;
      this.txtTestCS.Name = "txtTestCS";
      TextBox txtTestCs2 = this.txtTestCS;
      size1 = new Size(55, 20);
      Size size51 = size1;
      txtTestCs2.Size = size51;
      this.txtTestCS.TabIndex = 50;
      this.txtTestCS.Text = "1";
      this.Label20.AutoSize = true;
      Label label20_1 = this.Label20;
      point1 = new Point(45, 92);
      Point point52 = point1;
      label20_1.Location = point52;
      this.Label20.Name = "Label20";
      Label label20_2 = this.Label20;
      size1 = new Size(40, 13);
      Size size52 = size1;
      label20_2.Size = size52;
      this.Label20.TabIndex = 49;
      this.Label20.Text = "adduct";
      this.Label21.AutoSize = true;
      Label label21_1 = this.Label21;
      point1 = new Point(45, 119);
      Point point53 = point1;
      label21_1.Location = point53;
      this.Label21.Name = "Label21";
      Label label21_2 = this.Label21;
      size1 = new Size(66, 13);
      Size size53 = size1;
      label21_2.Size = size53;
      this.Label21.TabIndex = 48;
      this.Label21.Text = "charge state";
      TextBox txtResults1 = this.txtResults;
      point1 = new Point(12, 328);
      Point point54 = point1;
      txtResults1.Location = point54;
      this.txtResults.Multiline = true;
      this.txtResults.Name = "txtResults";
      this.txtResults.ScrollBars = ScrollBars.Vertical;
      TextBox txtResults2 = this.txtResults;
      size1 = new Size(620, 170);
      Size size54 = size1;
      txtResults2.Size = size54;
      this.txtResults.TabIndex = 4;
      this.txtResults.WordWrap = false;
      this.txtPeaksFile.AllowDrop = true;
      TextBox txtPeaksFile1 = this.txtPeaksFile;
      point1 = new Point(12, 302);
      Point point55 = point1;
      txtPeaksFile1.Location = point55;
      this.txtPeaksFile.Name = "txtPeaksFile";
      TextBox txtPeaksFile2 = this.txtPeaksFile;
      size1 = new Size(620, 20);
      Size size55 = size1;
      txtPeaksFile2.Size = size55;
      this.txtPeaksFile.TabIndex = 3;
      this.txtPeaksFile.Text = "drop peaks file";
      Button cmdIpdBmzCalc1 = this.cmdIPDBmzCalc;
      point1 = new Point(447, 120);
      Point point56 = point1;
      cmdIpdBmzCalc1.Location = point56;
      this.cmdIPDBmzCalc.Name = "cmdIPDBmzCalc";
      Button cmdIpdBmzCalc2 = this.cmdIPDBmzCalc;
      size1 = new Size(99, 30);
      Size size56 = size1;
      cmdIpdBmzCalc2.Size = size56;
      this.cmdIPDBmzCalc.TabIndex = 46;
      this.cmdIPDBmzCalc.Text = "m/z calc.";
      this.cmdIPDBmzCalc.UseVisualStyleBackColor = true;
      this.AutoScaleDimensions = new SizeF(6f, 13f);
      this.AutoScaleMode = AutoScaleMode.Font;
      size1 = new Size(648, 510);
      this.ClientSize = size1;
      this.Controls.Add((Control) this.txtResults);
      this.Controls.Add((Control) this.txtPeaksFile);
      this.Controls.Add((Control) this.TabTest);
      this.Name = nameof (Form1);
      this.Text = "TestIPDBSearch";
      this.TabTest.ResumeLayout(false);
      this.TabCalibrationPage.ResumeLayout(false);
      this.TabCalibrationPage.PerformLayout();
      this.TabTestIPDB.ResumeLayout(false);
      this.TabTestIPDB.PerformLayout();
      this.TabTestFormula.ResumeLayout(false);
      this.TabTestFormula.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();
    }

    public virtual TabControl TabTest
    {
      [DebuggerNonUserCode] get
      {
        return this._TabTest;
      }
      [DebuggerNonUserCode, MethodImpl(MethodImplOptions.Synchronized)] set
      {
        this._TabTest = value;
      }
    }

    public virtual TabPage TabCalibrationPage
    {
      [DebuggerNonUserCode] get
      {
        return this._TabCalibrationPage;
      }
      [DebuggerNonUserCode, MethodImpl(MethodImplOptions.Synchronized)] set
      {
        this._TabCalibrationPage = value;
      }
    }

    public virtual TabPage TabTestIPDB
    {
      [DebuggerNonUserCode] get
      {
        return this._TabTestIPDB;
      }
      [DebuggerNonUserCode, MethodImpl(MethodImplOptions.Synchronized)] set
      {
        this._TabTestIPDB = value;
      }
    }

    public virtual Button cmdCalibrate
    {
      [DebuggerNonUserCode] get
      {
        return this._cmdCalibrate;
      }
      [DebuggerNonUserCode, MethodImpl(MethodImplOptions.Synchronized)] set
      {
        EventHandler eventHandler = new EventHandler(this.cmdCalibrate_Click);
        if (this._cmdCalibrate != null)
          this._cmdCalibrate.Click -= eventHandler;
        this._cmdCalibrate = value;
        if (this._cmdCalibrate == null)
          return;
        this._cmdCalibrate.Click += eventHandler;
      }
    }

    public virtual Label Label8
    {
      [DebuggerNonUserCode] get
      {
        return this._Label8;
      }
      [DebuggerNonUserCode, MethodImpl(MethodImplOptions.Synchronized)] set
      {
        this._Label8 = value;
      }
    }

    public virtual TextBox txtMaxRelAbu
    {
      [DebuggerNonUserCode] get
      {
        return this._txtMaxRelAbu;
      }
      [DebuggerNonUserCode, MethodImpl(MethodImplOptions.Synchronized)] set
      {
        this._txtMaxRelAbu = value;
      }
    }

    public virtual Label Label7
    {
      [DebuggerNonUserCode] get
      {
        return this._Label7;
      }
      [DebuggerNonUserCode, MethodImpl(MethodImplOptions.Synchronized)] set
      {
        this._Label7 = value;
      }
    }

    public virtual TextBox txtCalFile
    {
      [DebuggerNonUserCode] get
      {
        return this._txtCalFile;
      }
      [DebuggerNonUserCode, MethodImpl(MethodImplOptions.Synchronized)] set
      {
        DragEventHandler dragEventHandler1 = new DragEventHandler(this.txtCalFile_DragEnter);
        DragEventHandler dragEventHandler2 = new DragEventHandler(this.txtCalFile_DragDrop);
        if (this._txtCalFile != null)
        {
          this._txtCalFile.DragEnter -= dragEventHandler1;
          this._txtCalFile.DragDrop -= dragEventHandler2;
        }
        this._txtCalFile = value;
        if (this._txtCalFile == null)
          return;
        this._txtCalFile.DragEnter += dragEventHandler1;
        this._txtCalFile.DragDrop += dragEventHandler2;
      }
    }

    public virtual Label Label6
    {
      [DebuggerNonUserCode] get
      {
        return this._Label6;
      }
      [DebuggerNonUserCode, MethodImpl(MethodImplOptions.Synchronized)] set
      {
        this._Label6 = value;
      }
    }

    public virtual TextBox txtMinRelAbu
    {
      [DebuggerNonUserCode] get
      {
        return this._txtMinRelAbu;
      }
      [DebuggerNonUserCode, MethodImpl(MethodImplOptions.Synchronized)] set
      {
        this._txtMinRelAbu = value;
      }
    }

    public virtual TextBox txtRF
    {
      [DebuggerNonUserCode] get
      {
        return this._txtRF;
      }
      [DebuggerNonUserCode, MethodImpl(MethodImplOptions.Synchronized)] set
      {
        this._txtRF = value;
      }
    }

    public virtual TextBox txtStartTolerance
    {
      [DebuggerNonUserCode] get
      {
        return this._txtStartTolerance;
      }
      [DebuggerNonUserCode, MethodImpl(MethodImplOptions.Synchronized)] set
      {
        this._txtStartTolerance = value;
      }
    }

    public virtual Label Label3
    {
      [DebuggerNonUserCode] get
      {
        return this._Label3;
      }
      [DebuggerNonUserCode, MethodImpl(MethodImplOptions.Synchronized)] set
      {
        this._Label3 = value;
      }
    }

    public virtual Label Label5
    {
      [DebuggerNonUserCode] get
      {
        return this._Label5;
      }
      [DebuggerNonUserCode, MethodImpl(MethodImplOptions.Synchronized)] set
      {
        this._Label5 = value;
      }
    }

    public virtual TextBox txtMinSN
    {
      [DebuggerNonUserCode] get
      {
        return this._txtMinSN;
      }
      [DebuggerNonUserCode, MethodImpl(MethodImplOptions.Synchronized)] set
      {
        this._txtMinSN = value;
      }
    }

    public virtual Label Label1
    {
      [DebuggerNonUserCode] get
      {
        return this._Label1;
      }
      [DebuggerNonUserCode, MethodImpl(MethodImplOptions.Synchronized)] set
      {
        this._Label1 = value;
      }
    }

    public virtual ComboBox cmbRegression
    {
      [DebuggerNonUserCode] get
      {
        return this._cmbRegression;
      }
      [DebuggerNonUserCode, MethodImpl(MethodImplOptions.Synchronized)] set
      {
        this._cmbRegression = value;
      }
    }

    public virtual TextBox txtTargetTolerance
    {
      [DebuggerNonUserCode] get
      {
        return this._txtTargetTolerance;
      }
      [DebuggerNonUserCode, MethodImpl(MethodImplOptions.Synchronized)] set
      {
        this._txtTargetTolerance = value;
      }
    }

    public virtual Label Label2
    {
      [DebuggerNonUserCode] get
      {
        return this._Label2;
      }
      [DebuggerNonUserCode, MethodImpl(MethodImplOptions.Synchronized)] set
      {
        this._Label2 = value;
      }
    }

    public virtual Label Label4
    {
      [DebuggerNonUserCode] get
      {
        return this._Label4;
      }
      [DebuggerNonUserCode, MethodImpl(MethodImplOptions.Synchronized)] set
      {
        this._Label4 = value;
      }
    }

    public virtual TextBox txtResults
    {
      [DebuggerNonUserCode] get
      {
        return this._txtResults;
      }
      [DebuggerNonUserCode, MethodImpl(MethodImplOptions.Synchronized)] set
      {
        this._txtResults = value;
      }
    }

    public virtual TextBox txtPeaksFile
    {
      [DebuggerNonUserCode] get
      {
        return this._txtPeaksFile;
      }
      [DebuggerNonUserCode, MethodImpl(MethodImplOptions.Synchronized)] set
      {
        DragEventHandler dragEventHandler1 = new DragEventHandler(this.txtPeaksFile_DragDrop);
        DragEventHandler dragEventHandler2 = new DragEventHandler(this.txtPeaksFile_DragEnter);
        if (this._txtPeaksFile != null)
        {
          this._txtPeaksFile.DragDrop -= dragEventHandler1;
          this._txtPeaksFile.DragEnter -= dragEventHandler2;
        }
        this._txtPeaksFile = value;
        if (this._txtPeaksFile == null)
          return;
        this._txtPeaksFile.DragDrop += dragEventHandler1;
        this._txtPeaksFile.DragEnter += dragEventHandler2;
      }
    }

    public virtual TextBox txtIPDBMinorMinSN
    {
      [DebuggerNonUserCode] get
      {
        return this._txtIPDBMinorMinSN;
      }
      [DebuggerNonUserCode, MethodImpl(MethodImplOptions.Synchronized)] set
      {
        this._txtIPDBMinorMinSN = value;
      }
    }

    public virtual Label Label10
    {
      [DebuggerNonUserCode] get
      {
        return this._Label10;
      }
      [DebuggerNonUserCode, MethodImpl(MethodImplOptions.Synchronized)] set
      {
        this._Label10 = value;
      }
    }

    public virtual Label Label9
    {
      [DebuggerNonUserCode] get
      {
        return this._Label9;
      }
      [DebuggerNonUserCode, MethodImpl(MethodImplOptions.Synchronized)] set
      {
        this._Label9 = value;
      }
    }

    public virtual TextBox txtIPDBMajorMinSN
    {
      [DebuggerNonUserCode] get
      {
        return this._txtIPDBMajorMinSN;
      }
      [DebuggerNonUserCode, MethodImpl(MethodImplOptions.Synchronized)] set
      {
        this._txtIPDBMajorMinSN = value;
      }
    }

    public virtual Label Label13
    {
      [DebuggerNonUserCode] get
      {
        return this._Label13;
      }
      [DebuggerNonUserCode, MethodImpl(MethodImplOptions.Synchronized)] set
      {
        this._Label13 = value;
      }
    }

    public virtual Label Label12
    {
      [DebuggerNonUserCode] get
      {
        return this._Label12;
      }
      [DebuggerNonUserCode, MethodImpl(MethodImplOptions.Synchronized)] set
      {
        this._Label12 = value;
      }
    }

    public virtual Label Label14
    {
      [DebuggerNonUserCode] get
      {
        return this._Label14;
      }
      [DebuggerNonUserCode, MethodImpl(MethodImplOptions.Synchronized)] set
      {
        this._Label14 = value;
      }
    }

    public virtual TextBox txtIPDBAdduct
    {
      [DebuggerNonUserCode] get
      {
        return this._txtIPDBAdduct;
      }
      [DebuggerNonUserCode, MethodImpl(MethodImplOptions.Synchronized)] set
      {
        EventHandler eventHandler = new EventHandler(this.txtIPDBAdduct_LostFocus);
        if (this._txtIPDBAdduct != null)
          this._txtIPDBAdduct.LostFocus -= eventHandler;
        this._txtIPDBAdduct = value;
        if (this._txtIPDBAdduct == null)
          return;
        this._txtIPDBAdduct.LostFocus += eventHandler;
      }
    }

    public virtual TextBox txtIPDBChargeState
    {
      [DebuggerNonUserCode] get
      {
        return this._txtIPDBChargeState;
      }
      [DebuggerNonUserCode, MethodImpl(MethodImplOptions.Synchronized)] set
      {
        EventHandler eventHandler = new EventHandler(this.txtIPDBChargeState_LostFocus);
        if (this._txtIPDBChargeState != null)
          this._txtIPDBChargeState.LostFocus -= eventHandler;
        this._txtIPDBChargeState = value;
        if (this._txtIPDBChargeState == null)
          return;
        this._txtIPDBChargeState.LostFocus += eventHandler;
      }
    }

    public virtual Button cmdIPDBSearch
    {
      [DebuggerNonUserCode] get
      {
        return this._cmdIPDBSearch;
      }
      [DebuggerNonUserCode, MethodImpl(MethodImplOptions.Synchronized)] set
      {
        EventHandler eventHandler = new EventHandler(this.cmdIPDBSearch_Click);
        if (this._cmdIPDBSearch != null)
          this._cmdIPDBSearch.Click -= eventHandler;
        this._cmdIPDBSearch = value;
        if (this._cmdIPDBSearch == null)
          return;
        this._cmdIPDBSearch.Click += eventHandler;
      }
    }

    public virtual Button cmdIPDBMergeWithCIA
    {
      [DebuggerNonUserCode] get
      {
        return this._cmdIPDBMergeWithCIA;
      }
      [DebuggerNonUserCode, MethodImpl(MethodImplOptions.Synchronized)] set
      {
        this._cmdIPDBMergeWithCIA = value;
      }
    }

    public virtual Label Label16
    {
      [DebuggerNonUserCode] get
      {
        return this._Label16;
      }
      [DebuggerNonUserCode, MethodImpl(MethodImplOptions.Synchronized)] set
      {
        this._Label16 = value;
      }
    }

    public virtual TextBox txtIPDBMTol
    {
      [DebuggerNonUserCode] get
      {
        return this._txtIPDBMTol;
      }
      [DebuggerNonUserCode, MethodImpl(MethodImplOptions.Synchronized)] set
      {
        this._txtIPDBMTol = value;
      }
    }

    public virtual TextBox txtIPDBMinPA2Report
    {
      [DebuggerNonUserCode] get
      {
        return this._txtIPDBMinPA2Report;
      }
      [DebuggerNonUserCode, MethodImpl(MethodImplOptions.Synchronized)] set
      {
        this._txtIPDBMinPA2Report = value;
      }
    }

    public virtual Label Label15
    {
      [DebuggerNonUserCode] get
      {
        return this._Label15;
      }
      [DebuggerNonUserCode, MethodImpl(MethodImplOptions.Synchronized)] set
      {
        this._Label15 = value;
      }
    }

    public virtual CheckBox chkIPDBReportPeakHits
    {
      [DebuggerNonUserCode] get
      {
        return this._chkIPDBReportPeakHits;
      }
      [DebuggerNonUserCode, MethodImpl(MethodImplOptions.Synchronized)] set
      {
        this._chkIPDBReportPeakHits = value;
      }
    }

    public virtual ComboBox cmbIPDBIonization
    {
      [DebuggerNonUserCode] get
      {
        return this._cmbIPDBIonization;
      }
      [DebuggerNonUserCode, MethodImpl(MethodImplOptions.Synchronized)] set
      {
        EventHandler eventHandler = new EventHandler(this.cmbIPDBIonization_SelectedIndexChanged);
        if (this._cmbIPDBIonization != null)
          this._cmbIPDBIonization.SelectedIndexChanged -= eventHandler;
        this._cmbIPDBIonization = value;
        if (this._cmbIPDBIonization == null)
          return;
        this._cmbIPDBIonization.SelectedIndexChanged += eventHandler;
      }
    }

    public virtual Label Label17
    {
      [DebuggerNonUserCode] get
      {
        return this._Label17;
      }
      [DebuggerNonUserCode, MethodImpl(MethodImplOptions.Synchronized)] set
      {
        this._Label17 = value;
      }
    }

    public virtual TextBox txtIPDB_EC_Filter
    {
      [DebuggerNonUserCode] get
      {
        return this._txtIPDB_EC_Filter;
      }
      [DebuggerNonUserCode, MethodImpl(MethodImplOptions.Synchronized)] set
      {
        this._txtIPDB_EC_Filter = value;
      }
    }

    public virtual TextBox txtIPDB
    {
      [DebuggerNonUserCode] get
      {
        return this._txtIPDB;
      }
      [DebuggerNonUserCode, MethodImpl(MethodImplOptions.Synchronized)] set
      {
        DragEventHandler dragEventHandler1 = new DragEventHandler(this.txtIPDB_DragDrop);
        DragEventHandler dragEventHandler2 = new DragEventHandler(this.txtIPDB_DragEnter);
        if (this._txtIPDB != null)
        {
          this._txtIPDB.DragDrop -= dragEventHandler1;
          this._txtIPDB.DragEnter -= dragEventHandler2;
        }
        this._txtIPDB = value;
        if (this._txtIPDB == null)
          return;
        this._txtIPDB.DragDrop += dragEventHandler1;
        this._txtIPDB.DragEnter += dragEventHandler2;
      }
    }

    public virtual TextBox txtIPDBMinPToScore
    {
      [DebuggerNonUserCode] get
      {
        return this._txtIPDBMinPToScore;
      }
      [DebuggerNonUserCode, MethodImpl(MethodImplOptions.Synchronized)] set
      {
        this._txtIPDBMinPToScore = value;
      }
    }

    public virtual Label Label18
    {
      [DebuggerNonUserCode] get
      {
        return this._Label18;
      }
      [DebuggerNonUserCode, MethodImpl(MethodImplOptions.Synchronized)] set
      {
        this._Label18 = value;
      }
    }

    internal virtual TabPage TabTestFormula
    {
      [DebuggerNonUserCode] get
      {
        return this._TabTestFormula;
      }
      [DebuggerNonUserCode, MethodImpl(MethodImplOptions.Synchronized)] set
      {
        this._TabTestFormula = value;
      }
    }

    internal virtual Button cmdTestMF
    {
      [DebuggerNonUserCode] get
      {
        return this._cmdTestMF;
      }
      [DebuggerNonUserCode, MethodImpl(MethodImplOptions.Synchronized)] set
      {
        EventHandler eventHandler = new EventHandler(this.cmdTestMF_Click);
        if (this._cmdTestMF != null)
          this._cmdTestMF.Click -= eventHandler;
        this._cmdTestMF = value;
        if (this._cmdTestMF == null)
          return;
        this._cmdTestMF.Click += eventHandler;
      }
    }

    public virtual Label Label23
    {
      [DebuggerNonUserCode] get
      {
        return this._Label23;
      }
      [DebuggerNonUserCode, MethodImpl(MethodImplOptions.Synchronized)] set
      {
        this._Label23 = value;
      }
    }

    public virtual TextBox txtTestMF
    {
      [DebuggerNonUserCode] get
      {
        return this._txtTestMF;
      }
      [DebuggerNonUserCode, MethodImpl(MethodImplOptions.Synchronized)] set
      {
        this._txtTestMF = value;
      }
    }

    public virtual TextBox txtTestAdduct
    {
      [DebuggerNonUserCode] get
      {
        return this._txtTestAdduct;
      }
      [DebuggerNonUserCode, MethodImpl(MethodImplOptions.Synchronized)] set
      {
        EventHandler eventHandler = new EventHandler(this.txtTestAdduct_LostFocus);
        if (this._txtTestAdduct != null)
          this._txtTestAdduct.LostFocus -= eventHandler;
        this._txtTestAdduct = value;
        if (this._txtTestAdduct == null)
          return;
        this._txtTestAdduct.LostFocus += eventHandler;
      }
    }

    public virtual TextBox txtTestCS
    {
      [DebuggerNonUserCode] get
      {
        return this._txtTestCS;
      }
      [DebuggerNonUserCode, MethodImpl(MethodImplOptions.Synchronized)] set
      {
        EventHandler eventHandler = new EventHandler(this.txtTestCS_LostFocus);
        if (this._txtTestCS != null)
          this._txtTestCS.LostFocus -= eventHandler;
        this._txtTestCS = value;
        if (this._txtTestCS == null)
          return;
        this._txtTestCS.LostFocus += eventHandler;
      }
    }

    public virtual Label Label20
    {
      [DebuggerNonUserCode] get
      {
        return this._Label20;
      }
      [DebuggerNonUserCode, MethodImpl(MethodImplOptions.Synchronized)] set
      {
        this._Label20 = value;
      }
    }

    public virtual Label Label21
    {
      [DebuggerNonUserCode] get
      {
        return this._Label21;
      }
      [DebuggerNonUserCode, MethodImpl(MethodImplOptions.Synchronized)] set
      {
        this._Label21 = value;
      }
    }

    public virtual ComboBox cmbTestIonization
    {
      [DebuggerNonUserCode] get
      {
        return this._cmbTestIonization;
      }
      [DebuggerNonUserCode, MethodImpl(MethodImplOptions.Synchronized)] set
      {
        EventHandler eventHandler = new EventHandler(this.cmbTestIonization_SelectedIndexChanged);
        if (this._cmbTestIonization != null)
          this._cmbTestIonization.SelectedIndexChanged -= eventHandler;
        this._cmbTestIonization = value;
        if (this._cmbTestIonization == null)
          return;
        this._cmbTestIonization.SelectedIndexChanged += eventHandler;
      }
    }

    public virtual Label Label19
    {
      [DebuggerNonUserCode] get
      {
        return this._Label19;
      }
      [DebuggerNonUserCode, MethodImpl(MethodImplOptions.Synchronized)] set
      {
        this._Label19 = value;
      }
    }

    public virtual Button cmdIPDBmzCalc
    {
      [DebuggerNonUserCode] get
      {
        return this._cmdIPDBmzCalc;
      }
      [DebuggerNonUserCode, MethodImpl(MethodImplOptions.Synchronized)] set
      {
        EventHandler eventHandler = new EventHandler(this.cmdIPDBmzCalc_Click);
        if (this._cmdIPDBmzCalc != null)
          this._cmdIPDBmzCalc.Click -= eventHandler;
        this._cmdIPDBmzCalc = value;
        if (this._cmdIPDBmzCalc == null)
          return;
        this._cmdIPDBmzCalc.Click += eventHandler;
      }
    }

    public Form1()
    {
      Form1.__ENCAddToList((object) this);
      this.InitializeComponent();
      Array values1 = Enum.GetValues(typeof (TotalCalibration.ttlRegressionType));
      try
      {
        foreach (object obj in values1)
          this.cmbRegression.Items.Add((object) (TotalCalibration.ttlRegressionType) Conversions.ToInteger(obj));
      }
      catch (Exception ex)
      {
          Console.WriteLine(ex.Message);
      }
      this.my_cal = new TotalCalibration();
      this.txtMinSN.Text = Conversions.ToString(this.my_cal.ttl_cal_min_sn);
      this.txtMinRelAbu.Text = Conversions.ToString(this.my_cal.ttl_cal_min_abu_pct);
      this.txtMaxRelAbu.Text = Conversions.ToString(this.my_cal.ttl_cal_max_abu_pct);
      this.txtStartTolerance.Text = Conversions.ToString(this.my_cal.ttl_cal_start_ppm);
      this.txtTargetTolerance.Text = Conversions.ToString(this.my_cal.ttl_cal_target_ppm);
      this.txtRF.Text = this.my_cal.ttl_cal_rf.ToString("0.0E0");
      this.cmbRegression.SelectedItem = (object) this.my_cal.ttl_cal_regression;
      Array values2 = Enum.GetValues(typeof (IonizationMethod));
      try
      {
        foreach (object obj in values2)
        {
          IonizationMethod integer = (IonizationMethod) Conversions.ToInteger(obj);
          this.cmbIPDBIonization.Items.Add((object) ttlAI.AI_IonizationMethod_Enum2String(integer));
          this.cmbTestIonization.Items.Add((object) ttlAI.AI_IonizationMethod_Enum2String(integer));
        }
      }
      catch (Exception ex)
      {
          Console.WriteLine(ex.Message);
      }
      this.cmbIPDBIonization.SelectedItem = (object) "-p";
      this.cmbTestIonization.SelectedItem = (object) "-p";
      try
      {
        this.myMFCalc = new TotalMFMonoPeakCalculator();
        this.myMFCalc.Ionization = ttlAI.AI_IonizationMethod_String2Enum(Conversions.ToString(this.cmbTestIonization.SelectedItem));
        this.myMFCalc.CS = int.Parse(this.txtTestCS.Text);
        this.myMFCalc.Adduct = this.txtTestAdduct.Text;
      }
      catch (Exception ex)
      {
        ProjectData.SetProjectError(ex);
        ProjectData.ClearProjectError();
      }
    }

    private void txtCalFile_DragDrop(object sender, DragEventArgs e)
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
        string[] data = (string[]) e.Data.GetData("FileDrop", false);
label_3:
        num3 = 3;
        int num4 = 0;
        int num5 = checked (data.Length - 1);
        int index1 = num4;
        goto label_19;
label_4:
        num3 = 4;
        FileInfo fileInfo1 = new FileInfo(data[index1]);
label_5:
        num3 = 5;
        if ((fileInfo1.Attributes & FileAttributes.Directory) != FileAttributes.Directory)
          goto label_15;
label_6:
        num3 = 6;
        DirectoryInfo directoryInfo = new DirectoryInfo(fileInfo1.FullName);
label_7:
        num3 = 7;
        FileInfo[] files = directoryInfo.GetFiles();
label_8:
        num3 = 8;
        FileInfo[] fileInfoArray = files;
        int index2 = 0;
        goto label_14;
label_10:
        num3 = 9;
        FileInfo fileInfo2;
        if ((fileInfo2.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
          goto label_12;
label_11:
        num3 = 10;
        this.txtCalFile.Text = fileInfo2.FullName;
label_12:
        checked { ++index2; }
label_13:
        num3 = 12;
label_14:
        if (index2 < fileInfoArray.Length)
        {
          fileInfo2 = fileInfoArray[index2];
          goto label_10;
        }
        else
          goto label_17;
label_15:
        num3 = 14;
label_16:
        num3 = 15;
        this.txtCalFile.Text = fileInfo1.FullName;
label_17:
label_18:
        num3 = 17;
        checked { ++index1; }
label_19:
        if (index1 <= num5)
          goto label_4;
        else
          goto label_26;
label_21:
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
                goto label_4;
              case 5:
                goto label_5;
              case 6:
                goto label_6;
              case 7:
                goto label_7;
              case 8:
                goto label_8;
              case 9:
                goto label_10;
              case 10:
                goto label_11;
              case 11:
                goto label_12;
              case 12:
                goto label_13;
              case 13:
              case 16:
                goto label_17;
              case 14:
                goto label_15;
              case 15:
                goto label_16;
              case 17:
                goto label_18;
              case 18:
                goto label_26;
            }
        }
      }
      catch (Exception ex) when (ex is Exception & (uint) num1 > 0U & num2 == 0)
      {
        ProjectData.SetProjectError(ex);
          return;
      }
      throw ProjectData.CreateProjectError(-2146828237);
label_26:
      if (num2 == 0)
        return;
      ProjectData.ClearProjectError();
    }

    private void txtCalFile_DragEnter(object sender, DragEventArgs e)
    {
      if (e.Data.GetDataPresent(DataFormats.FileDrop))
        e.Effect = DragDropEffects.All;
      else
        e.Effect = DragDropEffects.None;
    }

    private void txtPeaksFile_DragDrop(object sender, DragEventArgs e)
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
        string[] data = (string[]) e.Data.GetData("FileDrop", false);
label_3:
        num3 = 3;
        int num4 = 0;
        int num5 = checked (data.Length - 1);
        int index1 = num4;
        goto label_19;
label_4:
        num3 = 4;
        FileInfo fileInfo1 = new FileInfo(data[index1]);
label_5:
        num3 = 5;
        if ((fileInfo1.Attributes & FileAttributes.Directory) != FileAttributes.Directory)
          goto label_15;
label_6:
        num3 = 6;
        DirectoryInfo directoryInfo = new DirectoryInfo(fileInfo1.FullName);
label_7:
        num3 = 7;
        FileInfo[] files = directoryInfo.GetFiles();
label_8:
        num3 = 8;
        FileInfo[] fileInfoArray = files;
        int index2 = 0;
        goto label_14;
label_10:
        num3 = 9;
        FileInfo fileInfo2;
        if ((fileInfo2.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
          goto label_12;
label_11:
        num3 = 10;
        this.txtPeaksFile.Text = fileInfo2.FullName;
label_12:
        checked { ++index2; }
label_13:
        num3 = 12;
label_14:
        if (index2 < fileInfoArray.Length)
        {
          fileInfo2 = fileInfoArray[index2];
          goto label_10;
        }
        else
          goto label_17;
label_15:
        num3 = 14;
label_16:
        num3 = 15;
        this.txtPeaksFile.Text = fileInfo1.FullName;
label_17:
label_18:
        num3 = 17;
        checked { ++index1; }
label_19:
        if (index1 <= num5)
          goto label_4;
        else
          goto label_26;
label_21:
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
                goto label_4;
              case 5:
                goto label_5;
              case 6:
                goto label_6;
              case 7:
                goto label_7;
              case 8:
                goto label_8;
              case 9:
                goto label_10;
              case 10:
                goto label_11;
              case 11:
                goto label_12;
              case 12:
                goto label_13;
              case 13:
              case 16:
                goto label_17;
              case 14:
                goto label_15;
              case 15:
                goto label_16;
              case 17:
                goto label_18;
              case 18:
                goto label_26;
            }
        }
      }
      catch (Exception ex) when (ex is Exception & (uint) num1 > 0U & num2 == 0)
      {
        ProjectData.SetProjectError(ex);
          return;
      }
      throw ProjectData.CreateProjectError(-2146828237);
label_26:
      if (num2 == 0)
        return;
      ProjectData.ClearProjectError();
    }

    private void txtPeaksFile_DragEnter(object sender, DragEventArgs e)
    {
      if (e.Data.GetDataPresent(DataFormats.FileDrop))
        e.Effect = DragDropEffects.All;
      else
        e.Effect = DragDropEffects.None;
    }

    private void cmdCalibrate_Click(object sender, EventArgs e)
    {
      try
      {
        this.my_cal = new TotalCalibration();
        if (this.my_cal.Load(this.txtCalFile.Text) > 0)
        {
          this.my_cal.ttl_cal_regression = (TotalCalibration.ttlRegressionType) Conversions.ToInteger(this.cmbRegression.SelectedItem);
          this.my_cal.ttl_cal_min_sn = double.Parse(this.txtMinSN.Text);
          this.my_cal.ttl_cal_min_abu_pct = double.Parse(this.txtMinRelAbu.Text);
          this.my_cal.ttl_cal_max_abu_pct = double.Parse(this.txtMaxRelAbu.Text);
          this.my_cal.ttl_cal_start_ppm = double.Parse(this.txtStartTolerance.Text);
          this.my_cal.ttl_cal_target_ppm = double.Parse(this.txtTargetTolerance.Text);
          this.my_cal.ttl_cal_rf = double.Parse(this.txtRF.Text);
          if (this.LoadPeaks() > 0)
          {
            this.peak_m_cal = this.my_cal.ttl_LQ_InternalCalibration(ref this.peak_m, ref this.peak_a, ref this.peak_sn, this.max_a);
            if (this.peak_m_cal != null)
            {
              this.txtResults.AppendText("calibration succeded!\r\n");
              this.txtResults.AppendText(this.WriteResults() + "\r\n");
            }
            else
              this.txtResults.AppendText("calibration failed!\r\n");
            this.txtResults.AppendText(this.my_cal.cal_log.ToString() + "\r\n");
          }
          else
            this.txtResults.AppendText("failed to load peaks from " + this.txtPeaksFile.Text);
        }
        else
        {
          this.txtResults.AppendText(this.my_cal.CalibrationSetDescription() + "\r\n");
          this.txtResults.AppendText("this calibration set is not well designed!");
        }
      }
      catch (Exception ex)
      {
        ProjectData.SetProjectError(ex);
        this.txtResults.AppendText(ex.Message + "\r\n");
        ProjectData.ClearProjectError();
      }
    }

    private int LoadPeaks()
    {
      int index = 0;
      string[] strArray1 = new string[0];
      int num;
      try
      {
        this.max_a = 0.0;
        this.m_peaks_file_name = this.txtPeaksFile.Text;
        TextReader textReader = (TextReader) new StreamReader(this.m_peaks_file_name);
        this.peak_m = new double[20001];
        this.peak_a = new double[20001];
        this.peak_sn = new double[20001];
        textReader.ReadLine();
        while (textReader.Peek() != -1)
        {
          string[] strArray2 = textReader.ReadLine().Split('\t');
          this.peak_m[index] = double.Parse(strArray2[0]);
          this.peak_a[index] = double.Parse(strArray2[1]);
          this.peak_sn[index] = double.Parse(strArray2[2]);
          if (this.peak_a[index] > this.max_a)
            this.max_a = this.peak_a[index];
          checked { ++index; }
        }
        textReader.Close();
        if (index > 0)
        {
          this.peak_m = (double[]) Utils.CopyArray((Array) this.peak_m, (Array) new double[checked (index - 1 + 1)]);
          this.peak_a = (double[]) Utils.CopyArray((Array) this.peak_a, (Array) new double[checked (index - 1 + 1)]);
          this.peak_sn = (double[]) Utils.CopyArray((Array) this.peak_sn, (Array) new double[checked (index - 1 + 1)]);
        }
        else
        {
          this.peak_m = (double[]) null;
          this.peak_a = (double[]) null;
          this.peak_sn = (double[]) null;
        }
        num = index;
      }
      catch (Exception ex)
      {
        ProjectData.SetProjectError(ex);
        num = -1;
        ProjectData.ClearProjectError();
      }
      finally
      {
      }
      return num;
    }

    private string WriteResults()
    {
      string str;
      try
      {
        TextWriter textWriter = (TextWriter) new StreamWriter(this.txtPeaksFile.Text.Replace(".txt", "_out.txt"));
        textWriter.WriteLine("peak\tabu\tsn");
        int num1 = 0;
        int num2 = checked (this.peak_m.Length - 1);
        int index = num1;
        while (index <= num2)
        {
          textWriter.WriteLine(Conversions.ToString(this.peak_m_cal[index]) + "\t" + Conversions.ToString(this.peak_a[index]) + "\t" + Conversions.ToString(this.peak_sn[index]));
          checked { ++index; }
        }
        textWriter.Close();
        str = "recalibrated peaks successfully written to file";
      }
      catch (Exception ex)
      {
        ProjectData.SetProjectError(ex);
        str = "error writing results - " + ex.Message;
        ProjectData.ClearProjectError();
      }
      finally
      {
      }
      return str;
    }

    private void cmdIPDBSearch_Click(object sender, EventArgs e)
    {
      try
      {
        this.myIPDB = new TotalIPDBSearch();
        if ((uint) this.LoadPeaks() > 0U)
        {
          this.myIPDB.IPDB_log.AppendLine(Conversions.ToString(this.peak_m.Length) + " loaded peaks from [" + this.txtPeaksFile.Text + "]");
          if (this.myIPDB.LoadTabulatedDB(this.txtIPDB.Text))
          {
            this.myIPDB.Ionization = ttlAI.AI_IonizationMethod_String2Enum(Conversions.ToString(this.cmbIPDBIonization.SelectedItem));
            this.myIPDB.CS = int.Parse(this.txtIPDBChargeState.Text);
            this.myIPDB.Adduct = this.txtIPDBAdduct.Text;
            if (this.myIPDB.SetCalculation())
            {
              this.myIPDB.m_ppm_tol = double.Parse(this.txtIPDBMTol.Text);
              this.myIPDB.m_min_major_sn = double.Parse(this.txtIPDBMajorMinSN.Text);
              this.myIPDB.m_min_minor_sn = double.Parse(this.txtIPDBMinorMinSN.Text);
              this.myIPDB.m_min_major_pa_mm_abs_2_report = double.Parse(this.txtIPDBMinPA2Report.Text);
              this.myIPDB.m_matched_peaks_report = this.chkIPDBReportPeakHits.Checked;
              this.myIPDB.m_min_p_to_score = double.Parse(this.txtIPDBMinPToScore.Text);
              this.myIPDB.m_IPDB_ec_filter = this.txtIPDB_EC_Filter.Text;
              this.myIPDB.ttlSearch(ref this.peak_m, ref this.peak_a, ref this.peak_sn, ref this.max_a, this.m_peaks_file_name);
            }
          }
        }
        else
          this.myIPDB.IPDB_log.AppendLine("failed to load peaks from [" + this.txtPeaksFile.Text + "]");
        this.txtResults.AppendText(this.myIPDB.IPDB_log.ToString());
      }
      catch (Exception ex)
      {
        ProjectData.SetProjectError(ex);
        this.txtResults.AppendText(ex.Message + "\r\n");
        ProjectData.ClearProjectError();
      }
    }

    private void txtIPDB_DragDrop(object sender, DragEventArgs e)
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
        string[] data = (string[]) e.Data.GetData("FileDrop", false);
label_3:
        num3 = 3;
        int num4 = 0;
        int num5 = checked (data.Length - 1);
        int index1 = num4;
        goto label_19;
label_4:
        num3 = 4;
        FileInfo fileInfo1 = new FileInfo(data[index1]);
label_5:
        num3 = 5;
        if ((fileInfo1.Attributes & FileAttributes.Directory) != FileAttributes.Directory)
          goto label_15;
label_6:
        num3 = 6;
        DirectoryInfo directoryInfo = new DirectoryInfo(fileInfo1.FullName);
label_7:
        num3 = 7;
        FileInfo[] files = directoryInfo.GetFiles();
label_8:
        num3 = 8;
        FileInfo[] fileInfoArray = files;
        int index2 = 0;
        goto label_14;
label_10:
        num3 = 9;
        FileInfo fileInfo2;
        if ((fileInfo2.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
          goto label_12;
label_11:
        num3 = 10;
        this.txtIPDB.Text = fileInfo2.FullName;
label_12:
        checked { ++index2; }
label_13:
        num3 = 12;
label_14:
        if (index2 < fileInfoArray.Length)
        {
          fileInfo2 = fileInfoArray[index2];
          goto label_10;
        }
        else
          goto label_17;
label_15:
        num3 = 14;
label_16:
        num3 = 15;
        this.txtIPDB.Text = fileInfo1.FullName;
label_17:
label_18:
        num3 = 17;
        checked { ++index1; }
label_19:
        if (index1 <= num5)
          goto label_4;
        else
          goto label_26;
label_21:
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
                goto label_4;
              case 5:
                goto label_5;
              case 6:
                goto label_6;
              case 7:
                goto label_7;
              case 8:
                goto label_8;
              case 9:
                goto label_10;
              case 10:
                goto label_11;
              case 11:
                goto label_12;
              case 12:
                goto label_13;
              case 13:
              case 16:
                goto label_17;
              case 14:
                goto label_15;
              case 15:
                goto label_16;
              case 17:
                goto label_18;
              case 18:
                goto label_26;
            }
        }
      }
      catch (Exception ex) when (ex is Exception & (uint) num1 > 0U & num2 == 0)
      {
        ProjectData.SetProjectError(ex);
          return;
       }
      throw ProjectData.CreateProjectError(-2146828237);
label_26:
      if (num2 == 0)
        return;
      ProjectData.ClearProjectError();
    }

    private void txtIPDB_DragEnter(object sender, DragEventArgs e)
    {
      if (e.Data.GetDataPresent(DataFormats.FileDrop))
        e.Effect = DragDropEffects.All;
      else
        e.Effect = DragDropEffects.None;
    }

    private void txtIsotopesFile_DragEnter(object sender, DragEventArgs e)
    {
      if (e.Data.GetDataPresent(DataFormats.FileDrop))
        e.Effect = DragDropEffects.All;
      else
        e.Effect = DragDropEffects.None;
    }

    private void cmdTestMF_Click(object sender, EventArgs e)
    {
      try
      {
        this.txtResults.Clear();
        this.txtResults.AppendText("neutral monoisotopic mass for " + this.txtTestMF.Text + " " + Conversions.ToString(this.myMFCalc.GetNeutralMass_MF(this.txtTestMF.Text.Trim())) + "\r\n");
        this.txtResults.AppendText("ionization " + this.myMFCalc.Ionization.ToString() + "\r\n");
        this.txtResults.AppendText("polarity " + this.myMFCalc.Polarity.ToString() + "\r\n");
        this.txtResults.AppendText("adduct " + this.myMFCalc.Adduct + "\r\n");
        this.txtResults.AppendText("charge state " + Conversions.ToString(this.myMFCalc.CS) + "\r\n");
        this.txtResults.AppendText("charge carrier mass " + Conversions.ToString(this.myMFCalc.CCM) + "\r\n");
        this.txtResults.AppendText("charged monosiotopic mass  " + Conversions.ToString(this.myMFCalc.GetChargedMass_MF(this.txtTestMF.Text.Trim())) + "\r\n");
        this.txtResults.AppendText("charged mono.mass calculation description " + this.myMFCalc.ChargedMassFormula_Descriptive + "\r\n");
        this.txtResults.AppendText("charged mono.mass calculation formula " + this.myMFCalc.ChargedMassFormula_Numeric + "\r\n");
      }
      catch (Exception ex)
      {
        ProjectData.SetProjectError(ex);
        this.txtResults.AppendText(ex.Message + "\r\n");
        ProjectData.ClearProjectError();
      }
    }

    private void txtTestAdduct_LostFocus(object sender, EventArgs e)
    {
      this.myMFCalc.Adduct = this.txtTestAdduct.Text.Trim();
    }

    private void txtTestCS_LostFocus(object sender, EventArgs e)
    {
      this.myMFCalc.CS = int.Parse(this.txtTestCS.Text);
    }

    private void cmbTestIonization_SelectedIndexChanged(object sender, EventArgs e)
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
        this.myMFCalc.Ionization = ttlAI.AI_IonizationMethod_String2Enum(Conversions.ToString(this.cmbTestIonization.SelectedItem));
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
      catch (Exception ex) when (ex is Exception & (uint) num1 > 0U & num2 == 0)
      {
        ProjectData.SetProjectError(ex);
          return;
      }
      throw ProjectData.CreateProjectError(-2146828237);
label_9:
      if (num2 == 0)
        return;
      ProjectData.ClearProjectError();
    }

    private void cmbIPDBIonization_SelectedIndexChanged(object sender, EventArgs e)
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
        this.myIPDB.Ionization = ttlAI.AI_IonizationMethod_String2Enum(Conversions.ToString(this.cmbIPDBIonization.SelectedItem));
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
      catch (Exception ex) when (ex is Exception & (uint) num1 > 0U & num2 == 0)
      {
        ProjectData.SetProjectError(ex);
          return;
      }
      throw ProjectData.CreateProjectError(-2146828237);
label_9:
      if (num2 == 0)
        return;
      ProjectData.ClearProjectError();
    }

    private void txtIPDBChargeState_LostFocus(object sender, EventArgs e)
    {
      this.myIPDB.CS = int.Parse(this.txtIPDBChargeState.Text);
    }

    private void txtIPDBAdduct_LostFocus(object sender, EventArgs e)
    {
      this.myIPDB.Adduct = this.txtIPDBAdduct.Text.Trim();
    }

    private void cmdIPDBmzCalc_Click(object sender, EventArgs e)
    {
      try
      {
        this.myIPDB = new TotalIPDBSearch();
        this.myIPDB.Ionization = ttlAI.AI_IonizationMethod_String2Enum(Conversions.ToString(this.cmbIPDBIonization.SelectedItem));
        this.myIPDB.CS = int.Parse(this.txtIPDBChargeState.Text);
        this.myIPDB.Adduct = this.txtIPDBAdduct.Text;
        this.txtResults.AppendText(this.myIPDB.ChargedMassFormula_Descriptive + "\r\n");
        this.txtResults.AppendText(this.myIPDB.ChargedMassFormula_Numeric + "\r\n");
      }
      catch (Exception ex)
      {
        ProjectData.SetProjectError(ex);
        this.txtResults.AppendText(ex.Message + "\r\n");
        ProjectData.ClearProjectError();
      }
    }
  }
}

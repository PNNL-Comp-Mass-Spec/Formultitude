using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using System.IO;
using System.Xml;
using System.Threading;
using System.Globalization;

using Support;
using TestFSDBSearch;
using CIA;

namespace CiaUi
{
    public partial class FormularityUIForm : Form
    {
        public CCia oCCia = new CCia();

        private readonly CiaAdvancedForm oCiaAdvancedForm;
        //System.Windows.Forms.CheckBox [] GoldenRuleFilterUsage;
        private readonly string[] DBPeaksTableHeaders = { "Index", "Neutral mass", "Formula", "Error, ppm" };
        public enum EPlotType { ErrorVsNeutralMass, ErrorVs };
        public FormularityUIForm()
        {
            InitializeComponent();
            try
            {
                oCiaAdvancedForm = new CiaAdvancedForm(this);
                this.SuspendLayout();

                comboBoxIonization.DataSource = Enum.GetValues(typeof(TestFSDBSearch.TotalSupport.IonizationMethod));

                //Calibration
                comboBoxCalRegressionModel.DataSource = Enum.GetValues(typeof(TotalCalibration.ttlRegressionType));

                //================
                //CIA tab
                //================
                //Formula assignment
                //textBoxDropDB.Text = Path.GetFileName( oCCia.GetCiaDBFilename() );

                comboBoxFormulaScore.DataSource = oCCia.GetFormulaScoreNames();
                comboBoxRelationErrorType.DataSource = Enum.GetNames(typeof(CCia.RelationErrorType));

                for (var Relation = 0; Relation < CCia.RelationBuildingBlockFormulas.Length; Relation++)
                {
                    checkedListBoxRelations.Items.Add(oCCia.FormulaToName(CCia.RelationBuildingBlockFormulas[Relation]), oCCia.GetActiveRelationFormulaBuildingBlocks()[Relation]);
                }

                comboBoxSpecialFilters.Items.Clear();
                var SpecialFilterNames = Enum.GetNames(typeof(CCia.ESpecialFilters));
                var SpecialFilterRules = oCCia.GetSpecialFilterRules();

                for (var SpecialFilter = 0; SpecialFilter < SpecialFilterRules.Length; SpecialFilter++)
                {
                    comboBoxSpecialFilters.Items.Add(SpecialFilterNames[SpecialFilter] + ": " + SpecialFilterRules[SpecialFilter]);
                }
                checkBoxGenerateReports.Checked = oCCia.GetGenerateReports();

                //================
                //IPA tab (Isotopic pattern algorithm)
                //============
                buttonIpaMergeWithCIA.Visible = false;

                var DefaultParametersFile = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "\\DefaultParameters.xml";

                if (File.Exists(DefaultParametersFile))
                {
                    oCCia.LoadParameters(DefaultParametersFile);
                }

                //===============
                //Spectra files area
                //===============

                //checkBoxCIA
                //checkBoxIpa

                UpdateCiaAndIpaDialogs();
                //===============
                //chartError tab
                //=============
                comboBoxPlotType.DataSource = Enum.GetNames(typeof(EPlotType));
                comboBoxPlotType.SelectedIndex = 0;

                //================
                //DB inspector tab
                //================
                numericUpDownDBMass.Enabled = false;
                tableLayoutPanelDBPeaks.Enabled = false;
                tableLayoutPanelDBPeaks.AutoScroll = true;
                tableLayoutPanelDBPeaks.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
                tableLayoutPanelDBPeaks.ColumnStyles.Clear();
                tableLayoutPanelDBPeaks.ColumnCount = DBPeaksTableHeaders.Length;

                for (var iColumn = 0; iColumn < tableLayoutPanelDBPeaks.ColumnCount; iColumn++)
                {
                    tableLayoutPanelDBPeaks.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(SizeType.Percent, (float)100.0 / DBPeaksTableHeaders.Length));
                }
                tableLayoutPanelDBPeaks.RowStyles.Clear();
                tableLayoutPanelDBPeaks.RowCount = 5 + 1;//Extra Row without RowStyle!!!

                for (var iRow = 0; iRow < tableLayoutPanelDBPeaks.RowCount; iRow++)
                {
                    tableLayoutPanelDBPeaks.RowStyles.Add(new System.Windows.Forms.RowStyle(SizeType.Absolute, new System.Windows.Forms.TextBox().Height + 2 * new System.Windows.Forms.TextBox().Margin.Top));
                }
                for (var iRow = 0; iRow < tableLayoutPanelDBPeaks.RowCount - 1; iRow++)
                {// "-1" Extra Row without Controls!!!
                    for (var iColumn = 0; iColumn < tableLayoutPanelDBPeaks.ColumnCount; iColumn++)
                    {
                        var oTextBox = new System.Windows.Forms.TextBox
                        {
                            Anchor = AnchorStyles.None,
                            ReadOnly = true,
                            AutoSize = true,
                            TextAlign = HorizontalAlignment.Center
                        };

                        if (iRow == 0)
                        {
                            oTextBox.ReadOnly = true;
                            oTextBox.Text = DBPeaksTableHeaders[iColumn];
                        }
                        tableLayoutPanelDBPeaks.Controls.Add(oTextBox, iColumn, iRow);
                    }
                }

                //================
                //File convertor tab
                //================
                comboBoxDBAction.DataSource = DBActionMenu;
                comboBoxDBAction.SelectedIndex = 0;
                checkBoxDBCalculateMassFromFormula.Checked = oCCia.GetDBCalculateMassFromFormula();
                checkBoxDBSortByMass.Checked = oCCia.GetDBSort();
                checkBoxDBMassRangePerCsvFile.Checked = oCCia.GetDBMassRangePerCsvFile();
                numericUpDownDBMassRange.Value = (decimal)oCCia.GetDBMassRange();

                //================
                //Filter check tab
                //================

                //================
                //About tab
                //================
                richTextBoxAbout.SelectionFont = new System.Drawing.Font("Microsoft Sans Seri", 10, FontStyle.Bold);
                richTextBoxAbout.SelectedText = "Authors:\n\n";
                richTextBoxAbout.SelectionFont = new System.Drawing.Font("Microsoft Sans Seri", 8, FontStyle.Regular);
                richTextBoxAbout.SelectedText = "Andrey Liyu (Program user interface & CIA conversion from Matlab)";
                richTextBoxAbout.SelectedText = "\rNikola Tolic (Internal calibration, IPA function and DB)";
                richTextBoxAbout.SelectedText = "\rElizabeth Kujawinski & Krista Longnecker (original CIA Matlab code and DB)";
                richTextBoxAbout.SelectedText = "\r\rCompiled: 4/12/2017";

                richTextBoxAbout.SelectionFont = new System.Drawing.Font("Microsoft Sans Seri", 10, FontStyle.Bold);
                richTextBoxAbout.SelectedText = "\r\rDisclaimer:\n";
                richTextBoxAbout.SelectionFont = new System.Drawing.Font("Microsoft Sans Seri", 8, FontStyle.Regular);
                richTextBoxAbout.SelectedText = "This material was prepared as an account of work sponsored by an agency of the United States Government.";
                richTextBoxAbout.SelectedText = "Neither the United States Government nor the United States Department of Energy, nor the Contractor, nor any or their employees, ";
                richTextBoxAbout.SelectedText = "nor any jurisdiction or organization that has cooperated in the development of these materials, ";
                richTextBoxAbout.SelectedText = "makes any warranty, express or implied, or assumes any legal liability or responsibility for the accuracy, ";
                richTextBoxAbout.SelectedText = "completeness, or usefulness or any information, apparatus, product, software, or process disclosed, or represents that its use ";
                richTextBoxAbout.SelectedText = "would not infringe privately owned rights.";
                richTextBoxAbout.SelectedText = "Reference herein to any specific commercial product, process, or service by trade name, trademark,";
                richTextBoxAbout.SelectedText = "manufacturer, or otherwise does not necessarily constitute or imply its endorsement, recommendation, ";
                richTextBoxAbout.SelectedText = "or favoring by the United States Government or any agency thereof, or Battelle Memorial Institute. The ";
                richTextBoxAbout.SelectedText = "views and opinions of authors expressed herein do not necessarily state or reflect those of the United ";
                richTextBoxAbout.SelectedText = "States Government or any agency thereof.";

                richTextBoxAbout.SelectionFont = new System.Drawing.Font("Microsoft Sans Seri", 10, FontStyle.Regular);
                richTextBoxAbout.SelectedText = "\r\r\rPACIFIC NORTHWEST NATIONAL LABORATORY";
                richTextBoxAbout.SelectedText = "\roperated by BATTELLE";
                richTextBoxAbout.SelectedText = "\rfor the UNITED STATES DEPARTMENT OF ENERGY";
                richTextBoxAbout.SelectedText = "\runder Contract DE-AC05-76RL01830";

                //==============
                //Chains
                //=============
                tableLayoutPanelChains.Enabled = false;
                tableLayoutPanelChains.AutoScroll = true;
                tableLayoutPanelChains.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
                tableLayoutPanelChains.ColumnStyles.Clear();
                var ChainColumnNames = new[] { "Types", "PairCount", "PairDistance", "Formula", "FormulaDistance", "ChainCount" };
                tableLayoutPanelChains.ColumnCount = ChainColumnNames.Length;//PairCount,PairDistance,Formula,FormulaDistance,ChainCount
                for (var iColumn = 0; iColumn < tableLayoutPanelChains.ColumnCount; iColumn++)
                {
                    tableLayoutPanelChains.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(SizeType.Percent, (float)100.0 / ChainColumnNames.Length));
                }
                tableLayoutPanelChains.RowStyles.Clear();
                tableLayoutPanelChains.RowCount = 5 + 1;//Extra Row without RowStyle!!!
                for (var RowIndex = 0; RowIndex < tableLayoutPanelChains.RowCount; RowIndex++)
                {
                    tableLayoutPanelChains.RowStyles.Add(new System.Windows.Forms.RowStyle(SizeType.Absolute, new System.Windows.Forms.TextBox().Height + 2 * new System.Windows.Forms.TextBox().Margin.Top));
                }
                for (var iRow = 0; iRow < tableLayoutPanelChains.RowCount - 1; iRow++)
                {// "-1" Extra Row without Controls!!!
                    for (var ColumnIndex = 0; ColumnIndex < tableLayoutPanelChains.ColumnCount; ColumnIndex++)
                    {
                        var oTextBox = new System.Windows.Forms.TextBox
                        {
                            Anchor = AnchorStyles.None,
                            ReadOnly = true,
                            AutoSize = true,
                            TextAlign = HorizontalAlignment.Center
                        };

                        if (iRow == 0)
                        {
                            oTextBox.ReadOnly = true;
                            oTextBox.Text = ChainColumnNames[ColumnIndex];
                        }
                        tableLayoutPanelChains.Controls.Add(oTextBox, ColumnIndex, iRow);
                    }
                }

                this.ResumeLayout();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Something (" + ex.Message + ") was wrong during UI initilization.");
            }
        }

        private void UpdateCiaAndIpaDialogs()
        {
            //this.SuspendLayout();
            //input data
            textBoxAdduct.Text = oCCia.Ipa.Adduct;
            comboBoxIonization.Text = oCCia.Ipa.Ionization.ToString();
            numericUpDownCharge.Value = oCCia.Ipa.CS;

            checkBoxPreAlignment.Checked = oCCia.GetPreAlignment();
            //calibration
            //if ( textBoxRefPeakFilename.Text != Path.GetFileName( oCCia.GetRefPeakFilename() ) ){
            //oCCia.oTotalCalibration.Load( oCCia.GetRefPeakFilename() );
            textBoxRefPeakFilename.Text = Path.GetFileName(oCCia.GetRefPeakFilename());
            //}
            comboBoxCalRegressionModel.Text = oCCia.oTotalCalibration.ttl_cal_regression.ToString();
            numericUpDownCalRelFactor.Value = (decimal)oCCia.oTotalCalibration.ttl_cal_rf;
            numericUpDownCalStartTolerance.Value = (decimal)oCCia.oTotalCalibration.ttl_cal_start_ppm;
            numericUpDownCalEndTolerance.Value = (decimal)oCCia.oTotalCalibration.ttl_cal_target_ppm;
            numericUpDownCalMinSN.Value = (decimal)oCCia.oTotalCalibration.ttl_cal_min_sn;
            numericUpDownCalMinRelAbun.Value = (decimal)oCCia.oTotalCalibration.ttl_cal_min_abu_pct;
            numericUpDownCalMaxRelAbun.Value = (decimal)oCCia.oTotalCalibration.ttl_cal_max_abu_pct;
            //CIA
            //Alignment
            checkBoxAlignment.Checked = oCCia.GetAlignment();
            numericUpDownAlignmentTolerance.Value = (decimal)oCCia.GetAlignmentPpmTolerance();
            oCiaAdvancedForm.numericUpDownCIAAdvMinPeaksPerChain.Value = oCCia.GetMinPeaksPerChain();

            if (oCCia.GetCiaDBFilename().Length != 0)
            {
                if (textBoxCiaDBFilename.Text != Path.GetFileName(oCCia.GetCiaDBFilename()))
                {
                    oCCia.LoadCiaDB();
                    textBoxCiaDBFilename.Text = Path.GetFileName(oCCia.GetCiaDBFilename());
                }
            }

            checkBoxStaticDynamicFormulaTolerance.Checked = oCCia.GetStaticDynamicPpmError();
            numericUpDownFormulaToleranceStdDevGain.Value = (decimal)oCCia.GetStdDevErrorGain();
            //numericUpDownFormulaToleranceStdDevGain.Enabled = checkBoxStaticDynamicFormulaTolerance.Checked;//???
            numericUpDownFormulaTolerance.Value = (decimal)oCCia.GetFormulaPPMTolerance();
            numericUpDownDBMassLimit.Value = (decimal)oCCia.GetMassLimit();
            comboBoxFormulaScore.SelectedIndex = (int)oCCia.GetFormulaScore();
            //checkBoxUseCIAFormulaScore.Checked = oCCia.GetUseCIAFormulaScore();
            oCiaAdvancedForm.checkBoxCIAAdvUseKendrick.Checked = oCCia.GetUseKendrick();
            oCiaAdvancedForm.checkBoxCIAAdvUseC13.Checked = oCCia.GetUseC13();
            oCiaAdvancedForm.numericUpDownCIAAdvC13Tolerance.Value = (decimal)oCCia.GetC13Tolerance();
            checkBoxUseFormulaFilters.Checked = oCCia.GetUseFormulaFilter();
            oCiaAdvancedForm.checkBoxGoldenRule1.Checked = oCCia.GetGoldenRuleFilterUsage()[0];//ElementalCounts
            oCiaAdvancedForm.checkBoxGoldenRule2.Checked = oCCia.GetGoldenRuleFilterUsage()[1];//ValenceRules
            oCiaAdvancedForm.checkBoxGoldenRule3.Checked = oCCia.GetGoldenRuleFilterUsage()[2];//ElementalRatios
            oCiaAdvancedForm.checkBoxGoldenRule4.Checked = oCCia.GetGoldenRuleFilterUsage()[3];//HeteroatomCount
            oCiaAdvancedForm.checkBoxGoldenRule5.Checked = oCCia.GetGoldenRuleFilterUsage()[4];//PositiveAtoms
            oCiaAdvancedForm.checkBoxGoldenRule6.Checked = oCCia.GetGoldenRuleFilterUsage()[5];//IntegerDBE
            comboBoxSpecialFilters.SelectedIndex = (int)oCCia.GetSpecialFilter();
            textBoxUserDefinedFilter.Text = oCCia.GetUserDefinedFilter();

            checkBoxUseRelation.Checked = oCCia.GetUseRelation();
            numericUpDownMaxRelationGaps.Value = oCCia.GetMaxRelationGaps();
            numericUpDownRelationErrorValue.Value = (decimal)oCCia.GetRelationErrorAMU();
            comboBoxRelationErrorType.SelectedIndex = (int)oCCia.GetRelationErrorType();
            oCiaAdvancedForm.checkBoxCIAAdvBackward.Checked = oCCia.GetUseBackward();

            for (var BlockIndex = 0; BlockIndex < oCCia.GetActiveRelationFormulaBuildingBlocks().Length; BlockIndex++)
            {
                checkedListBoxRelations.SetItemChecked(BlockIndex, oCCia.GetActiveRelationFormulaBuildingBlocks()[BlockIndex]);
            }
            oCiaAdvancedForm.checkBoxGenerateReports.Checked = oCCia.GetGenerateReports();
            //checkBoxLogReport.Checked = oCCia.GetLogReportStatus();
            //oCiaAdvancedForm.comboBoxOutputFileDelimiter.Text = oCCia.GetOutputFileDelimiterType().ToString();
            oCiaAdvancedForm.comboBoxErrorType.Text = oCCia.GetErrorType().ToString();

            //IPA
            oCCia.LoadIpaDB();
            numericUpDownIpaMassTolerance.Value = (decimal)oCCia.Ipa.m_ppm_tol;
            numericUpDownIpaMajorPeaksMinSN.Value = (decimal)oCCia.Ipa.m_min_major_sn;
            numericUpDownIpaMinorPeaksMinSN.Value = (decimal)oCCia.Ipa.m_min_minor_sn;
            numericUpDownIpaMinMajorPeaksToAbsToReport.Value = (decimal)oCCia.Ipa.m_min_major_pa_mm_abs_2_report;
            checkBoxIpaMatchedPeakReport.Checked = oCCia.Ipa.m_matched_peaks_report;
            numericUpDownIpaMinPeakProbabilityToScore.Value = (decimal)oCCia.Ipa.m_min_p_to_score;
            textBoxIpaFilter.Text = oCCia.Ipa.m_IPDB_ec_filter;
            buttonIpaMergeWithCIA.Visible = false;
        }

        private void CIAUIForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            var DefaultParametersFile = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "\\DefaultParameters.xml";
            oCCia.GetSaveParameterText(DefaultParametersFile);
        }

        //Input files
        private void textBoxAdduct_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.KeyCode == Keys.Return)
                {
                    oCCia.Ipa.Adduct = textBoxAdduct.Text;
                    textBoxResult.Text = oCCia.Ipa.ChargedMassFormula_Descriptive;
                    //numericUpDownDBMass_ValueChanged( sender, e );//to update DB instector tab
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void textBoxAdduct_Leave(object sender, EventArgs e)
        {
            try
            {
                oCCia.Ipa.Adduct = textBoxAdduct.Text;
                textBoxResult.Text = oCCia.Ipa.ChargedMassFormula_Descriptive;
                //numericUpDownDBMass_ValueChanged( sender, e );//to update DB instector tab
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void comboBoxIonization_SelectedIndexChanged(object sender, EventArgs e)
        {
            oCCia.Ipa.Ionization = (TestFSDBSearch.TotalSupport.IonizationMethod)Enum.Parse(typeof(TestFSDBSearch.TotalSupport.IonizationMethod), comboBoxIonization.Text);
            textBoxResult.Text = oCCia.Ipa.ChargedMassFormula_Descriptive;
            //numericUpDownDBMass_ValueChanged( sender, e );//to update DB instector tab
        }

        private void numericUpDownCharge_ValueChanged(object sender, EventArgs e)
        {
            oCCia.Ipa.CS = (int)Math.Abs(numericUpDownCharge.Value);
            textBoxResult.Text = oCCia.Ipa.ChargedMassFormula_Descriptive;
            //numericUpDownDBMass_ValueChanged( sender, e );//to update DB instector tab
        }

        private void CheckToProcess()
        {
            var CalibrationReady = (TotalCalibration.ttlRegressionType)comboBoxCalRegressionModel.SelectedValue == TotalCalibration.ttlRegressionType.none
                                   || (TotalCalibration.ttlRegressionType)comboBoxCalRegressionModel.SelectedValue != TotalCalibration.ttlRegressionType.none && textBoxRefPeakFilename.TextLength > 0;

            var CIAReady = oCCia.GetCiaDBFilename().Length > 0 && CalibrationReady;
            checkBoxCIA.Enabled = CIAReady;

            var IpaReady = oCCia.Ipa.IPDB_Ready && CalibrationReady;
            checkBoxIpa.Enabled = IpaReady;

            if (CIAReady && checkBoxCIA.Checked
                    || IpaReady && checkBoxIpa.Checked)
            {
                textBoxDropSpectraFiles.BackColor = Color.LightGreen;
                textBoxDropSpectraFiles.Enabled = true;
            }
            else
            {
                textBoxDropSpectraFiles.BackColor = SystemColors.ControlLight;
                textBoxDropSpectraFiles.Enabled = false;
            }

            var ChainCalibrationReady = (TotalCalibration.ttlRegressionType)comboBoxCalRegressionModel.SelectedValue != TotalCalibration.ttlRegressionType.none
                                        && textBoxRefPeakFilename.TextLength > 0;

            if (ChainCalibrationReady)
            {
                textBoxChainDropSpectraFile.BackColor = Color.LightGreen;
                textBoxChainDropSpectraFile.Enabled = true;
            }
            else
            {
                textBoxChainDropSpectraFile.BackColor = SystemColors.ControlLight;
                textBoxChainDropSpectraFile.Enabled = false;
            }
        }

        private void checkBoxCIA_CheckedChanged(object sender, EventArgs e)
        {
            CheckToProcess();
        }

        private void checkBoxIpa_CheckedChanged(object sender, EventArgs e)
        {
            CheckToProcess();
        }

        private void textBoxRefPeakFilename_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void textBoxRefPeakFilename_DragDrop(object sender, DragEventArgs e)
        {
            var Filenames = (string[])e.Data.GetData(DataFormats.FileDrop);
            //oCCia.oTotalCalibration.Load( Filenames [ 0 ] );
            textBoxRefPeakFilename.Text = Path.GetFileName(Filenames[0]);
            oCCia.SetRefPeakFilename(Filenames[0]);
            CheckToProcess();
        }

        private void comboBoxCalRegressionModel_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxCalRegressionModel.Text == nameof(TotalCalibration.ttlRegressionType.none))
            {
                textBoxRefPeakFilename.Enabled = false;
                numericUpDownCalRelFactor.Enabled = false;
                numericUpDownCalStartTolerance.Enabled = false;
                numericUpDownCalEndTolerance.Enabled = false;
                numericUpDownCalMinSN.Enabled = false;
                numericUpDownCalMinRelAbun.Enabled = false;
                numericUpDownCalMaxRelAbun.Enabled = false;
            }
            else
            {
                textBoxRefPeakFilename.Enabled = true;
                numericUpDownCalRelFactor.Enabled = true;
                numericUpDownCalStartTolerance.Enabled = true;
                numericUpDownCalEndTolerance.Enabled = true;
                numericUpDownCalMinSN.Enabled = true;
                numericUpDownCalMinRelAbun.Enabled = true;
                numericUpDownCalMaxRelAbun.Enabled = true;
            }
            CheckToProcess();
        }

        private void textBoxDropSpectraFiles_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void textBoxDropSpectraFiles_DragDrop(object sender, DragEventArgs e)
        {
            textBoxDropSpectraFiles.BackColor = Color.Red;
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("ja-JP");
            try
            {
                var Filenames = (string[])e.Data.GetData(DataFormats.FileDrop);

                //Read files & Calibration
                oCCia.Ipa.Adduct = textBoxAdduct.Text;
                oCCia.Ipa.Ionization = (TestFSDBSearch.TotalSupport.IonizationMethod)Enum.Parse(typeof(TestFSDBSearch.TotalSupport.IonizationMethod), comboBoxIonization.Text);
                oCCia.Ipa.CS = (int)Math.Abs(numericUpDownCharge.Value);
                oCCia.SetPreAlignment(checkBoxPreAlignment.Checked);

                oCCia.SetStaticDynamicPpmError(checkBoxStaticDynamicFormulaTolerance.Checked);
                oCCia.SetInputData(null);

                oCCia.oTotalCalibration.ttl_cal_regression = (TotalCalibration.ttlRegressionType)Enum.Parse(typeof(TotalCalibration.ttlRegressionType), comboBoxCalRegressionModel.Text);
                oCCia.oTotalCalibration.ttl_cal_rf = (double)numericUpDownCalRelFactor.Value;
                oCCia.oTotalCalibration.ttl_cal_start_ppm = (double)numericUpDownCalStartTolerance.Value;
                oCCia.oTotalCalibration.ttl_cal_target_ppm = (double)numericUpDownCalEndTolerance.Value;
                oCCia.oTotalCalibration.ttl_cal_min_sn = (double)numericUpDownCalMinSN.Value;
                oCCia.oTotalCalibration.ttl_cal_min_abu_pct = (double)numericUpDownCalMinRelAbun.Value;
                oCCia.oTotalCalibration.ttl_cal_max_abu_pct = (double)numericUpDownCalMaxRelAbun.Value;

                if (checkBoxCIA.Checked)
                {
                    //Alignment
                    oCCia.SetAlignment(checkBoxAlignment.Checked);
                    oCCia.SetAlignmentPpmTolerance((double)numericUpDownAlignmentTolerance.Value);

                    //Formula assignment
                    oCCia.SetMassLimit((double)numericUpDownDBMassLimit.Value);
                    //oCCia.SetInputData( InputData );
                    //oCCia.SetChainBlocks( ChainBlocks );
                    //oCCia.SetBlockMasses( BlockMasses );
                    oCCia.SetStdDevErrorGain((double)numericUpDownFormulaToleranceStdDevGain.Value);

                    var CurrentFormulaScore = (CCia.EFormulaScore)Array.IndexOf(oCCia.GetFormulaScoreNames(), comboBoxFormulaScore.Text);
                    oCCia.SetFormulaScore(CurrentFormulaScore);

                    if (CurrentFormulaScore == CCia.EFormulaScore.UserDefined)
                    {
                        oCCia.SetUserDefinedScore(textBoxFormulaScoreUserDefined.Text);
                    }
                    if (!checkBoxCIAUseDefault.Checked)
                    {
                        oCCia.SetUseKendrick(oCiaAdvancedForm.checkBoxCIAAdvUseKendrick.Checked);
                        oCCia.SetUseC13(oCiaAdvancedForm.checkBoxCIAAdvUseC13.Checked);
                        oCCia.SetC13Tolerance((double)oCiaAdvancedForm.numericUpDownCIAAdvC13Tolerance.Value);
                    }
                    else
                    {
                        oCCia.SetUseKendrick(true);
                        oCCia.SetUseC13(true);
                        oCCia.SetC13Tolerance((double)numericUpDownFormulaTolerance.Value);
                    }

                    //Filters
                    oCCia.SetUseFormulaFilter(checkBoxUseFormulaFilters.Checked);
                    var GoldenFilters = new bool[oCCia.GoldenRuleFilterNames.Length];

                    if (!checkBoxCIAUseDefault.Checked)
                    {
                        GoldenFilters[0] = oCiaAdvancedForm.checkBoxGoldenRule1.Checked;
                        GoldenFilters[1] = oCiaAdvancedForm.checkBoxGoldenRule2.Checked;
                        GoldenFilters[2] = oCiaAdvancedForm.checkBoxGoldenRule3.Checked;
                        GoldenFilters[3] = oCiaAdvancedForm.checkBoxGoldenRule4.Checked;
                        GoldenFilters[4] = oCiaAdvancedForm.checkBoxGoldenRule5.Checked;
                        GoldenFilters[5] = oCiaAdvancedForm.checkBoxGoldenRule6.Checked;
                    }
                    else
                    {
                        GoldenFilters[0] = true;
                        GoldenFilters[1] = true;
                        GoldenFilters[2] = true;
                        GoldenFilters[3] = true;
                        GoldenFilters[4] = true;
                        GoldenFilters[5] = false;
                    }
                    oCCia.SetGoldenRuleFilterUsage(GoldenFilters);

                    oCCia.SetSpecialFilter((CCia.ESpecialFilters)Enum.Parse(typeof(CCia.ESpecialFilters), comboBoxSpecialFilters.Text.Split(new[] { ':' })[0]));
                    oCCia.SetUserDefinedFilter(textBoxUserDefinedFilter.Text);
                    //Relationships
                    oCCia.SetUseRelation(checkBoxUseRelation.Checked);
                    oCCia.SetMaxRelationGaps((int)numericUpDownMaxRelationGaps.Value);
                    oCCia.SetRelationErrorType((CCia.RelationErrorType)Enum.Parse(typeof(CCia.RelationErrorType), comboBoxRelationErrorType.Text));
                    oCCia.SetRelationErrorAMU((double)numericUpDownRelationErrorValue.Value);

                    if (!checkBoxCIAUseDefault.Checked)
                    {
                        oCCia.SetUseBackward(oCiaAdvancedForm.checkBoxCIAAdvBackward.Checked);
                    }
                    else
                    {
                        oCCia.SetUseBackward(false);
                    }

                    var ActiveRelationBlocks = new bool[CCia.RelationBuildingBlockFormulas.Length];

                    for (var ActiveFormula = 0; ActiveFormula < ActiveRelationBlocks.Length; ActiveFormula++)
                    {
                        ActiveRelationBlocks[ActiveFormula] = checkedListBoxRelations.GetItemChecked(ActiveFormula);
                    }
                    oCCia.SetActiveRelationFormulaBuildingBlocks(ActiveRelationBlocks);

                    //Reports
                    oCCia.SetGenerateReports(checkBoxGenerateReports.Checked);

                    if (!checkBoxCIAUseDefault.Checked)
                    {
                        oCCia.SetErrorType((CCia.EErrorType)Enum.Parse(typeof(CCia.EErrorType), oCiaAdvancedForm.comboBoxErrorType.Text));
                    }
                    else
                    {
                        oCCia.SetErrorType(CCia.EErrorType.Signed);
                    }

                    //change textbox
                    textBoxDropSpectraFiles.Text = "Drop Spectra Files";
                    textBoxDropSpectraFiles.AppendText("\r\nProcessed files:");

                    foreach (var Filename in Filenames)
                    {
                        textBoxDropSpectraFiles.AppendText("\r\n" + Path.GetFileName(Filename));
                    }
                }
                if (checkBoxIpa.Checked)
                {
                    var b = oCCia.Ipa.SetCalculation();

                    oCCia.Ipa.m_ppm_tol = (double)numericUpDownIpaMassTolerance.Value;
                    oCCia.Ipa.m_min_major_sn = (double)numericUpDownIpaMajorPeaksMinSN.Value;
                    oCCia.Ipa.m_min_minor_sn = (double)numericUpDownIpaMinorPeaksMinSN.Value;

                    oCCia.Ipa.m_min_major_pa_mm_abs_2_report = (double)numericUpDownIpaMinMajorPeaksToAbsToReport.Value;
                    oCCia.Ipa.m_matched_peaks_report = checkBoxIpaMatchedPeakReport.Checked;

                    oCCia.Ipa.m_min_p_to_score = (double)numericUpDownIpaMinPeakProbabilityToScore.Value;

                    oCCia.Ipa.m_IPDB_ec_filter = textBoxIpaFilter.Text;
                }
                if (checkBoxCIA.Checked && checkBoxIpa.Checked)
                {
                    oCCia.SetProcessType(CCia.ProcessType.CiaIpa);
                }
                else if (checkBoxCIA.Checked && !checkBoxIpa.Checked)
                {
                    oCCia.SetProcessType(CCia.ProcessType.Cia);
                }
                else
                {
                    oCCia.SetProcessType(CCia.ProcessType.Ipa);
                }
                oCCia.Process(Filenames);
            }
            catch (Exception Ex)
            {
                MessageBox.Show(Ex.Message);
                textBoxDropSpectraFiles.BackColor = Color.Pink;
            }
            textBoxDropSpectraFiles.BackColor = Color.LightGreen;
        }

        //tabs
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var ddd = tabControl1.TabPages[tabControl1.SelectedIndex].Text;

            if (ddd == "CIA DB inspector")
            {
                numericUpDownDBMass_ValueChanged(sender, e);
            }
        }
        //CIA tab
        private void checkBoxAlignment_CheckedChanged(object sender, EventArgs e)
        {
            numericUpDownAlignmentTolerance.Enabled = checkBoxAlignment.Checked;
            checkBoxStaticDynamicFormulaTolerance.Enabled = !checkBoxAlignment.Checked;

            if (checkBoxAlignment.Checked)
            {
                //no DynamicPPM  out of public enum RelationErrorType { AMU, PPM, GapPPM, DynamicPPM };
                comboBoxRelationErrorType.DataSource = Enum.GetNames(typeof(CCia.RelationErrorType)).ToList<string>().GetRange(0, 3);
                checkBoxStaticDynamicFormulaTolerance.Checked = !checkBoxAlignment.Checked;
            }
            else
            {
                comboBoxRelationErrorType.DataSource = Enum.GetNames(typeof(CCia.RelationErrorType));
            }
        }

        private void textBoxCiaDBFilename_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void textBoxCiaDBFilename_DragDrop(object sender, DragEventArgs e)
        {
            var Filenames = (string[])e.Data.GetData(DataFormats.FileDrop);
            oCCia.LoadCiaDB(Filenames[0]);
            textBoxCiaDBFilename.Text = Path.GetFileName(Filenames[0]);
            numericUpDownDBMass.Enabled = true;
            textBoxDBRecords.Text = oCCia.GetDBRecords().ToString();
            textBoxDBMinMass.Text = oCCia.GetDBMinMass().ToString();
            textBoxDBMaxMass.Text = oCCia.GetDBMaxMass().ToString();
            textBoxDBMinError.Text = oCCia.GetDBMinError().ToString();
            textBoxDBMaxError.Text = oCCia.GetDBMaxError().ToString();
            CheckToProcess();
        }

        private void checkBoxStaticDynamicFormulaTolerance_CheckedChanged(object sender, EventArgs e)
        {
            numericUpDownFormulaTolerance.Enabled = !checkBoxStaticDynamicFormulaTolerance.Checked;
            numericUpDownFormulaToleranceStdDevGain.Enabled = checkBoxStaticDynamicFormulaTolerance.Checked;
        }

        private void numericUpDownnumericUpDownFormulaTolerance_ValueChanged(object sender, EventArgs e)
        {
            oCCia.SetFormulaPPMTolerance((double)numericUpDownFormulaTolerance.Value);
            numericUpDownDBMass_ValueChanged(sender, e);//to update DB inspector tab
        }

        private void comboBoxFormulaScore_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBoxFormulaScoreUserDefined.Visible = comboBoxFormulaScore.SelectedIndex == (int)CCia.EFormulaScore.UserDefined;
            labelFormulaScoreUserDefined.Visible = textBoxFormulaScoreUserDefined.Visible;
        }

        private void checkBoxUseFormulaFilters_CheckedChanged(object sender, EventArgs e)
        {
            //groupBoxGoldenRuleFilters.Enabled = checkBoxUseFormulaFilters.Checked;
            comboBoxSpecialFilters.Enabled = checkBoxUseFormulaFilters.Checked;
            textBoxUserDefinedFilter.Enabled = checkBoxUseFormulaFilters.Checked;
        }

        private void checkBoxUseRelation_CheckedChanged(object sender, EventArgs e)
        {
            numericUpDownMaxRelationGaps.Enabled = checkBoxUseRelation.Checked;
            comboBoxRelationErrorType.Enabled = checkBoxUseRelation.Checked;
            numericUpDownRelationErrorValue.Enabled = checkBoxUseRelation.Checked;
            checkedListBoxRelations.Enabled = checkBoxUseRelation.Checked;
        }

        private void comboBoxRelationErrorType_SelectedIndexChanged(object sender, EventArgs e)
        {
            numericUpDownRelationErrorValue.Enabled = (CCia.RelationErrorType)Enum.Parse(typeof(CCia.RelationErrorType), comboBoxRelationErrorType.Text) != CCia.RelationErrorType.DynamicPPM;
        }

        private void buttonLoadCiaParameters_Click(object sender, EventArgs e)
        {
            textBoxAdduct.Text = "H";
            comboBoxIonization.Text = nameof(TestFSDBSearch.TotalSupport.IonizationMethod.proton_attachment);
            numericUpDownCharge.Value = 1;

            checkBoxAlignment.Checked = true;
            numericUpDownAlignmentTolerance.Value = (decimal)oCCia.GetAlignmentPpmTolerance();

            checkBoxUseRelation.Checked = true;
            numericUpDownDBMassLimit.Value = 500;
            comboBoxFormulaScore.SelectedIndex = (int)CCia.EFormulaScore.MinNSPAndError;

            checkBoxUseFormulaFilters.Checked = true;
            //for( int GoldenRuleFilter = 0; GoldenRuleFilter < GoldenRuleFilterUsage.Length - 1; GoldenRuleFilter++ ) {
            //    GoldenRuleFilterUsage [ GoldenRuleFilter ].Checked = true;
            //}
            //GoldenRuleFilterUsage [ 5].Checked = false;
            comboBoxSpecialFilters.SelectedIndex = 0;
            textBoxUserDefinedFilter.Text = string.Empty;

            checkBoxUseRelation.Checked = true;
            numericUpDownMaxRelationGaps.Value = 5;
            numericUpDownRelationErrorValue.Value = (decimal)0.00002;

            for (var RelationBlock = 0; RelationBlock < checkedListBoxRelations.Items.Count; RelationBlock++)
            {
                if (RelationBlock == 0 || RelationBlock == 2 || RelationBlock == 6)
                {
                    checkedListBoxRelations.SetItemChecked(RelationBlock, true);
                }
                else
                {
                    checkedListBoxRelations.SetItemChecked(RelationBlock, false);
                }
            }

            oCiaAdvancedForm.checkBoxGenerateReports.Checked = false;
            oCiaAdvancedForm.checkBoxCIAAdvAddChains.Checked = false;
            oCiaAdvancedForm.numericUpDownCIAAdvMinPeaksPerChain.Value = 3;
            oCiaAdvancedForm.comboBoxErrorType.Text = nameof(CCia.EErrorType.CIA);
        }

        private void buttonSwitchToAdvanced_Click(object sender, EventArgs e)
        {
            oCiaAdvancedForm.numericUpDownCharge.Value = numericUpDownCharge.Value;
            oCiaAdvancedForm.textBoxAdduct.Text = textBoxAdduct.Text;
            oCiaAdvancedForm.comboBoxIonization.Text = comboBoxIonization.Text;
            oCiaAdvancedForm.textBoxResult.Text = textBoxResult.Text;

            oCiaAdvancedForm.textBoxCalFile.Text = textBoxRefPeakFilename.Text;
            oCiaAdvancedForm.comboBoxCalRegressionModel.SelectedIndex = comboBoxCalRegressionModel.SelectedIndex;
            oCiaAdvancedForm.numericUpDownCalStartTolerance.Value = numericUpDownCalStartTolerance.Value;
            oCiaAdvancedForm.numericUpDownCalRelFactor.Value = numericUpDownCalRelFactor.Value;
            oCiaAdvancedForm.numericUpDownCalEndTolerance.Value = numericUpDownCalEndTolerance.Value;
            oCiaAdvancedForm.numericUpDownCalMinSN.Value = numericUpDownCalMinSN.Value;
            oCiaAdvancedForm.numericUpDownCalMinRelAbun.Value = numericUpDownCalMinRelAbun.Value;
            oCiaAdvancedForm.numericUpDownCalMaxRelAbun.Value = numericUpDownCalMaxRelAbun.Value;

            oCiaAdvancedForm.checkBoxAlignment.Checked = checkBoxAlignment.Checked;
            oCiaAdvancedForm.numericUpDownAlignmentTolerance.Value = numericUpDownAlignmentTolerance.Value;
            oCiaAdvancedForm.textBoxDropDB.Text = textBoxCiaDBFilename.Text;
            oCiaAdvancedForm.numericUpDownFormulaTolerance.Value = numericUpDownFormulaTolerance.Value;
            oCiaAdvancedForm.numericUpDownDBMassLimit.Value = numericUpDownDBMassLimit.Value;
            oCiaAdvancedForm.comboBoxFormulaScore.SelectedIndex = comboBoxFormulaScore.SelectedIndex;
            oCiaAdvancedForm.checkBoxUseFormulaFilters.Checked = checkBoxUseFormulaFilters.Checked;
            oCiaAdvancedForm.checkBoxUseRelation.Checked = checkBoxUseRelation.Checked;
            oCiaAdvancedForm.numericUpDownMaxRelationGaps.Value = numericUpDownMaxRelationGaps.Value;
            oCiaAdvancedForm.comboBoxRelationErrorType.SelectedIndex = comboBoxRelationErrorType.SelectedIndex;
            oCiaAdvancedForm.numericUpDownRelationErrorValue.Value = numericUpDownRelationErrorValue.Value;

            for (var RelationIndex = 0; RelationIndex < checkedListBoxRelations.CheckedItems.Count; RelationIndex++)
            {
                oCiaAdvancedForm.checkedListBoxRelations.SetItemChecked(RelationIndex, checkedListBoxRelations.GetItemChecked(RelationIndex));
            }

            oCiaAdvancedForm.comboBoxSpecialFilters.SelectedIndex = comboBoxSpecialFilters.SelectedIndex;
            oCiaAdvancedForm.textBoxUserDefinedFilter.Text = textBoxUserDefinedFilter.Text;
            oCiaAdvancedForm.CheckToProcess();

            this.Visible = false;
            var sss = oCiaAdvancedForm.ShowDialog(this);

            numericUpDownCharge.Value = oCiaAdvancedForm.numericUpDownCharge.Value;
            textBoxAdduct.Text = oCiaAdvancedForm.textBoxAdduct.Text;
            comboBoxIonization.Text = oCiaAdvancedForm.comboBoxIonization.Text;
            textBoxResult.Text = oCiaAdvancedForm.textBoxResult.Text;

            textBoxRefPeakFilename.Text = oCiaAdvancedForm.textBoxCalFile.Text;
            comboBoxCalRegressionModel.SelectedIndex = oCiaAdvancedForm.comboBoxCalRegressionModel.SelectedIndex;
            numericUpDownCalStartTolerance.Value = oCiaAdvancedForm.numericUpDownCalStartTolerance.Value;
            numericUpDownCalRelFactor.Value = oCiaAdvancedForm.numericUpDownCalRelFactor.Value;
            numericUpDownCalEndTolerance.Value = oCiaAdvancedForm.numericUpDownCalEndTolerance.Value;
            numericUpDownCalMinSN.Value = oCiaAdvancedForm.numericUpDownCalMinSN.Value;
            numericUpDownCalMinRelAbun.Value = oCiaAdvancedForm.numericUpDownCalMinRelAbun.Value;
            numericUpDownCalMaxRelAbun.Value = oCiaAdvancedForm.numericUpDownCalMaxRelAbun.Value;

            checkBoxAlignment.Checked = oCiaAdvancedForm.checkBoxAlignment.Checked;
            numericUpDownAlignmentTolerance.Value = oCiaAdvancedForm.numericUpDownAlignmentTolerance.Value;
            textBoxCiaDBFilename.Text = oCiaAdvancedForm.textBoxDropDB.Text;
            numericUpDownFormulaTolerance.Value = oCiaAdvancedForm.numericUpDownFormulaTolerance.Value;
            numericUpDownDBMassLimit.Value = oCiaAdvancedForm.numericUpDownDBMassLimit.Value;
            comboBoxFormulaScore.SelectedIndex = oCiaAdvancedForm.comboBoxFormulaScore.SelectedIndex;
            checkBoxUseFormulaFilters.Checked = oCiaAdvancedForm.checkBoxUseFormulaFilters.Checked;
            checkBoxUseRelation.Checked = oCiaAdvancedForm.checkBoxUseRelation.Checked;
            numericUpDownMaxRelationGaps.Value = oCiaAdvancedForm.numericUpDownMaxRelationGaps.Value;
            comboBoxRelationErrorType.SelectedIndex = oCiaAdvancedForm.comboBoxRelationErrorType.SelectedIndex;
            numericUpDownRelationErrorValue.Value = oCiaAdvancedForm.numericUpDownRelationErrorValue.Value;

            for (var RelationIndex = 0; RelationIndex < checkedListBoxRelations.CheckedItems.Count; RelationIndex++)
            {
                checkedListBoxRelations.SetItemChecked(RelationIndex, oCiaAdvancedForm.checkedListBoxRelations.GetItemChecked(RelationIndex));
            }

            comboBoxSpecialFilters.SelectedIndex = oCiaAdvancedForm.comboBoxSpecialFilters.SelectedIndex;
            textBoxUserDefinedFilter.Text = oCiaAdvancedForm.textBoxUserDefinedFilter.Text;
            CheckToProcess();

            this.Visible = true;
        }

        //Ipa tab
        private void textBoxIpaDBFilename_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void textBoxIpaDBFilename_DragDrop(object sender, DragEventArgs e)
        {
            var Filenames = (string[])e.Data.GetData(DataFormats.FileDrop);
            oCCia.Ipa.LoadTabulatedDB(Filenames[0]);
            CheckToProcess();
            textBoxIpaDBFilename.Text = Filenames[0];
            oCCia.SetIpaDBFilename(Filenames[0]);
        }

        //Error plot tab
        private readonly List<double> XData = new List<double>();
        private readonly List<double> YData = new List<double>();
        private void chartError_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void chartError_DragDrop(object sender, DragEventArgs e)
        {
            var Filenames = (string[])e.Data.GetData(DataFormats.FileDrop);
            var Lines = File.ReadAllLines(Filenames[0]);
            var FileName = Path.GetFileNameWithoutExtension(Filenames[0]);
            var Headers = Lines[0].Split(new[] { ',' });
            var XAxisColumnIndex = -1;
            var YAxisColumnIndex = -1;

            for (var Column = 0; Column < Headers.Length; Column++)
            {
                if (Headers[Column] == textBoxXAxisColumnHeader.Text) { XAxisColumnIndex = Column; }
                if (Headers[Column] == textBoxYAxisColumnHeader.Text) { YAxisColumnIndex = Column; }
                if (XAxisColumnIndex != -1 && YAxisColumnIndex != -1)
                {
                    break;
                }
            }
            if (XAxisColumnIndex == -1)
            {
                MessageBox.Show("There is not " + textBoxXAxisColumnHeader.Text + " column header");
                return;
            }
            if (YAxisColumnIndex == -1)
            {
                MessageBox.Show("There is not " + textBoxYAxisColumnHeader.Text + " column header");
                return;
            }

            for (var Line = 1; Line < Lines.Length; Line++)
            {
                var Words = Lines[Line].Split(new[] { ',' });

                if (Words[YAxisColumnIndex] == "0") { continue; }
                XData.Add(double.Parse(Words[XAxisColumnIndex]));
                YData.Add(double.Parse(Words[YAxisColumnIndex]));
            }

            chartError.Series[0].Name = string.Empty;//SeriesName;
            chartError.Series[0].Points.Clear();
            double XMin;
            double XMax;
            double YMin;
            double YMax;

            switch ((EPlotType)Enum.Parse(typeof(EPlotType), comboBoxPlotType.Text))
            {
                case EPlotType.ErrorVsNeutralMass:
                    chartError.Series[0].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point;
                    chartError.ChartAreas[0].AxisX.Title = "Neutral mass, Da";
                    chartError.ChartAreas[0].AxisY.Title = "Error, ppm";
                    YMin = YData[0];
                    YMax = YMin;

                    for (var Point = 0; Point < XData.Count; Point++)
                    {
                        chartError.Series[0].Points.AddXY(XData[Point], YData[Point]);

                        if (YMin > YData[Point]) { YMin = YData[Point]; }
                        if (YMax < YData[Point]) { YMax = YData[Point]; }
                    }
                    XMin = XData[0];
                    XMax = XData[XData.Count - 1];
                    break;
                case EPlotType.ErrorVs:
                    chartError.Series[0].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Column;
                    chartError.ChartAreas[0].AxisX.Title = "Error, ppm";
                    chartError.ChartAreas[0].AxisY.Title = "Counts";
                    var BinCount = (int)Math.Ceiling(Math.Sqrt(XData.Count));
                    XMin = YData[0];
                    XMax = XMin;

                    for (var Index = 1; Index < YData.Count; Index++)
                    {
                        if (XMin > YData[Index]) { XMin = YData[Index]; }
                        if (XMax < YData[Index]) { XMax = YData[Index]; }
                    }
                    var BinSize = (XMax - XMin) / BinCount;
                    var Bins = new int[BinCount];
                    YMin = 0;
                    YMax = 0;

                    foreach (var Y in YData)
                    {
                        var BinIndex = (int)Math.Floor((Y - XMin) / BinSize);

                        if (BinIndex >= BinCount) { BinIndex = BinCount - 1; }
                        Bins[BinIndex]++;

                        if (YMax < Bins[BinIndex]) { YMax = Bins[BinIndex]; }
                    }
                    for (var Point = 0; Point < Bins.Length; Point++)
                    {
                        var XValue = XMin + BinSize * (Point + 0.5);
                        chartError.Series[0].Points.AddXY(XValue, Bins[Point]);
                    }
                    break;
                default:
                    return;
            }
            chartError.ChartAreas[0].AxisX.Interval = (XMax - XMin) / 5;
            chartError.ChartAreas[0].AxisY.Interval = (YMax - YMin) / 5;
            chartError.ChartAreas[0].AxisX.LabelStyle.Format = "0.#e-0";
            chartError.ChartAreas[0].AxisY.LabelStyle.Format = "0.#e-0";
        }

        //DB tools tab
        private void numericUpDownDBMass_ValueChanged(object sender, EventArgs e)
        {
            //textBoxResult.Text = oCCia.Ipa.ChargedMassFormula_Descriptive;

            if (numericUpDownDBMass.Value < 0) { return; }
            tableLayoutPanelDBPeaks.SuspendLayout();
            tableLayoutPanelDBPeaks.Enabled = true;
            var Mass = (double)numericUpDownDBMass.Value;
            var NeutralMass = oCCia.Ipa.GetNeutralMass(Mass);
            textBoxDBNeutralMass.Text = NeutralMass.ToString();
            var Error = CPpmError.PpmToError(NeutralMass, oCCia.GetFormulaPPMTolerance());
            textBoxDBNeutralMassPlusError.Text = (NeutralMass + Error).ToString();
            textBoxDBNeutralMassMinusError.Text = (NeutralMass - Error).ToString();
            int Records;

            if (!oCCia.GetDBLimitIndexes(NeutralMass, out var LowerIndex, out var UpperIndex))
            {
                Records = 0;
            }
            else
            {
                Records = UpperIndex - LowerIndex + 1;
            }
            var Rows = Records + 2;//+ Head + Last Row without Controls
            textBoxDBRecordsInErrorRange.Text = Records.ToString();

            if (tableLayoutPanelDBPeaks.RowStyles.Count > Rows)
            {
                for (var Row = tableLayoutPanelDBPeaks.RowCount - 1; Row >= Rows; Row--)
                {
                    for (var iColumn = 0; iColumn < tableLayoutPanelDBPeaks.ColumnCount; iColumn++)
                    {
                        tableLayoutPanelDBPeaks.Controls.RemoveAt(tableLayoutPanelDBPeaks.Controls.Count - 1);
                    }
                    tableLayoutPanelDBPeaks.RowStyles.RemoveAt(Row);
                }
                tableLayoutPanelDBPeaks.RowCount = Rows;
            }
            else if (tableLayoutPanelDBPeaks.RowStyles.Count < Rows)
            {
                tableLayoutPanelDBPeaks.RowCount = Rows;

                for (var Row = tableLayoutPanelDBPeaks.RowStyles.Count - 1; Row < Rows - 1; Row++)
                {//"Count-1" due Last Row without Controls
                    tableLayoutPanelDBPeaks.RowStyles.Add(new System.Windows.Forms.RowStyle(SizeType.Absolute, new System.Windows.Forms.TextBox().Height + 2 * new System.Windows.Forms.TextBox().Margin.Top));

                    for (var iColumn = 0; iColumn < tableLayoutPanelDBPeaks.ColumnCount; iColumn++)
                    {
                        var oTextBox = new System.Windows.Forms.TextBox
                        {
                            Anchor = AnchorStyles.None,
                            ReadOnly = true,
                            AutoSize = true,
                            TextAlign = HorizontalAlignment.Center
                        };
                        tableLayoutPanelDBPeaks.Controls.Add(oTextBox, iColumn, Row);
                    }
                }
            }
            for (var Row = 1; Row < Rows - 1; Row++)
            {
                var DBIndex = LowerIndex + Row - 1;
                tableLayoutPanelDBPeaks.GetControlFromPosition(0, Row).Text = DBIndex.ToString();
                tableLayoutPanelDBPeaks.GetControlFromPosition(1, Row).Text = oCCia.GetDBMass(DBIndex).ToString();
                tableLayoutPanelDBPeaks.GetControlFromPosition(2, Row).Text = oCCia.GetDBFormulaName(DBIndex);
                tableLayoutPanelDBPeaks.GetControlFromPosition(3, Row).Text = CPpmError.SignedPpmPPM(NeutralMass, oCCia.GetDBMass(DBIndex)).ToString("E");
            }
            tableLayoutPanelDBPeaks.ResumeLayout();
        }
        /*
        private void textBoxDBDropFiles_DragEnter( object sender, DragEventArgs e ) {
            if( e.Data.GetDataPresent( DataFormats.FileDrop ) == true ) {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void textBoxDBDropFiles_DragDrop( object sender, DragEventArgs e ) {
            try {
                if( oCCia.GetDBFilename().Length == 0 ) { throw new Exception( "Drop DB file." ); }
                string [] Filenames = ( string [] ) e.Data.GetData( DataFormats.FileDrop );
                //???oCCia.ReadFiles( Filenames);
                //oCCia.ReportFormulas();
            } catch {
            }
        }
        */
        //File convertor tab
        private readonly string[] InputFileTextFormats = { ".txt", ".csv", ".xls", ".xlsx" };

        private readonly string[] DBActionMenu = {
            "One ASCII -> one binary",
            "Many ASCIIs -> one binary",
            "Binary -> CSV"};
        private void comboBoxDBAction_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxDBAction.Text == DBActionMenu[0])
            {
                checkBoxDBCalculateMassFromFormula.Enabled = true;
                checkBoxDBSortByMass.Enabled = true;
                checkBoxDBMassRangePerCsvFile.Enabled = false;
            }
            else if (comboBoxDBAction.Text == DBActionMenu[1])
            {
                checkBoxDBCalculateMassFromFormula.Enabled = true;
                checkBoxDBSortByMass.Enabled = true;
                checkBoxDBMassRangePerCsvFile.Enabled = false;
            }
            else if (comboBoxDBAction.Text == DBActionMenu[2])
            {
                checkBoxDBCalculateMassFromFormula.Enabled = false;
                checkBoxDBSortByMass.Enabled = false;
                checkBoxDBMassRangePerCsvFile.Enabled = true;
            }
        }

        private void checkBoxDBMassRangePerCsvFile_CheckedChanged(object sender, EventArgs e)
        {
            numericUpDownDBMassRange.Enabled = checkBoxDBMassRangePerCsvFile.Enabled;
        }

        private void textBoxConvertDBs_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void textBoxConvertDBs_DragDrop(object sender, DragEventArgs e)
        {
            var Filenames = (string[])e.Data.GetData(DataFormats.FileDrop);
            oCCia.SetDBCalculateMassFromFormula(checkBoxDBCalculateMassFromFormula.Checked);
            oCCia.SetDBSort(checkBoxDBSortByMass.Checked);
            oCCia.SetDBMassRangePerCsvFile(checkBoxDBMassRangePerCsvFile.Checked);
            oCCia.SetDBMassRange((double)numericUpDownDBMassRange.Value);

            if (comboBoxDBAction.Text == DBActionMenu[0])
            {
                foreach (var Filename in Filenames)
                {
                    if (InputFileTextFormats.Contains(Path.GetExtension(Filename)))
                    {
                        oCCia.DBConvertAsciiToBin(Filename);
                    }
                    else
                    {
                        MessageBox.Show("Extension of file (" + Path.GetFileName(Filename) + ") is not ASCII");
                    }
                }
            }
            else if (comboBoxDBAction.Text == DBActionMenu[1])
            {
                var ErrorTypeExtension = false;

                foreach (var Filename in Filenames)
                {
                    if (!InputFileTextFormats.Contains(Path.GetExtension(Filename)))
                    {
                        MessageBox.Show("Extension of file (" + Path.GetFileName(Filename) + ") is not ASCII");
                        ErrorTypeExtension = true;
                    }
                }
                if (!ErrorTypeExtension)
                {
                    oCCia.DBConvertAsciisToBin(Filenames);
                }
            }
            else if (comboBoxDBAction.Text == DBActionMenu[2])
            {
                foreach (var Filename in Filenames)
                {
                    if (Path.GetExtension(Filename) == ".bin")
                    {
                        oCCia.DBConvertBinToCsv(Filename);
                    }
                    else
                    {
                        MessageBox.Show("Extension of file (" + Path.GetFileName(Filename) + ") is not bin");
                    }
                }
            }
        }

        private void textBoxCompareReports_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void textBoxCompareReports_DragDrop(object sender, DragEventArgs e)
        {
            var Filenames = (string[])e.Data.GetData(DataFormats.FileDrop);
            CompareReports(Filenames);
        }

        private void CompareReports(string[] Filenames)
        {
            var TotalSamples = 0;
            var SampleNames = new List<string>();

            foreach (var Filename in Filenames)
            {
                var ColumnHeaders = File.ReadAllLines(Filename)[0].Split(oCCia.WordSeparators);
                var Samples = ColumnHeaders.Length - 10;
                TotalSamples += Samples;

                for (var Sample = 0; Sample < Samples; Sample++)
                {
                    SampleNames.Add(ColumnHeaders[10 + Sample]);
                }
            }
            var FormulaDict = new SortedDictionary<double, ReportData>();
            var StartSample = 0;

            foreach (var Filename in Filenames)
            {
                var LineString = File.ReadAllLines(Filename);
                var ColumnHeaders = File.ReadAllLines(Filename)[0].Split(oCCia.WordSeparators);
                var Samples = ColumnHeaders.Length - 10;

                for (var Line = 1; Line < LineString.Length; Line++)
                {
                    var LineParts = LineString[Line].Split(oCCia.WordSeparators);
                    var Formula = new short[CCia.ElementCount];

                    for (var Element = 0; Element < CCia.ElementCount; Element++)
                    {
                        Formula[Element] = Int16.Parse(LineParts[2 + Element]);
                    }
                    if (!oCCia.IsFormula(Formula))
                    {
                        continue;
                    }
                    var NeutralMass = oCCia.FormulaToNeutralMass(Formula);
                    double[] Abundances;

                    if (!FormulaDict.ContainsKey(NeutralMass))
                    {
                        var Data = new ReportData
                        {
                            Formula = (short[])Formula.Clone(),
                            Abundances = new double[TotalSamples]
                        };
                        FormulaDict.Add(NeutralMass, Data);
                        Abundances = Data.Abundances;
                    }
                    else
                    {
                        Abundances = FormulaDict[NeutralMass].Abundances;
                    }
                    for (var Sample = 0; Sample < Samples; Sample++)
                    {
                        Abundances[StartSample + Sample] = double.Parse(LineParts[10 + Sample]);
                    }
                }
                StartSample += Samples;
            }
            var Delimiter = ",";
            var HeaderLine = "NeutralMass" + Delimiter + "Mass";

            foreach (var Element in Enum.GetNames(typeof(CCia.EElemIndex)))
            {
                HeaderLine = HeaderLine + Delimiter + Element;
            }
            foreach (var SampleName in SampleNames)
            {
                HeaderLine = HeaderLine + Delimiter + SampleName;
            }
            var oStreamWriter = new StreamWriter(Path.GetDirectoryName(Filenames[0]) + "\\Comparision.csv");
            oStreamWriter.WriteLine(HeaderLine);

            foreach (var KVP in FormulaDict)
            {
                var Line = KVP.Key.ToString() + Delimiter + oCCia.Ipa.GetChargedMass(KVP.Key).ToString();

                foreach (var Count in KVP.Value.Formula)
                {
                    Line = Line + Delimiter + Count.ToString();
                }
                foreach (var Abundance in KVP.Value.Abundances)
                {
                    Line = Line + Delimiter + Abundance.ToString();
                }
                oStreamWriter.WriteLine(Line);
            }
            oStreamWriter.Close();
        }

        private void textBoxConverFiles_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void textBoxConverFiles_DragDrop(object sender, DragEventArgs e)
        {
            var Filenames = (string[])e.Data.GetData(DataFormats.FileDrop);

            foreach (var Filename in Filenames)
            {
                XmlToXls(Filename);
            }
        }

        private void XmlToXls(string Filename)
        {
            var XmlDoc = new XmlDocument();
            XmlDoc.Load(Filename);
            //check Bruker instrument
            var Nodes = XmlDoc.GetElementsByTagName("fileinfo");

            if (Nodes.Count != 1) { return; }
            if (Nodes[0].Attributes["appname"].Value != "Bruker Compass DataAnalysis") { return; }
            //read peaks
            var MsPeakNodes = XmlDoc.GetElementsByTagName("ms_peaks");

            if (MsPeakNodes.Count != 1) { return; }
            var MsPeakNode = MsPeakNodes[0];
            var Peaks = MsPeakNode.ChildNodes.Count;
            var myLengthsArray = new int[2] { Peaks, 5 };
            var myBoundsArray = new int[2] { 1, 1 };
            var myArray = Array.CreateInstance(typeof(double), myLengthsArray, myBoundsArray);
            double Maxi = 0;

            for (var Peak = 1; Peak <= Peaks; Peak++)
            {
                //<pk res="930674.5" algo="FTMS" fwhm="0.000218" a="102.53" sn="7.15" i="646225.1" mz="203.034719"/>
                var Attributes = MsPeakNode.ChildNodes[Peak - 1].Attributes;
                myArray.SetValue(double.Parse(Attributes["mz"].Value), Peak, 1);
                myArray.SetValue(double.Parse(Attributes["i"].Value), Peak, 2);
                myArray.SetValue(double.Parse(Attributes["sn"].Value), Peak, 3);
                myArray.SetValue(double.Parse(Attributes["res"].Value), Peak, 4);
                var Currenti = (double)myArray.GetValue(Peak, 2);

                if (Maxi < Currenti) { Maxi = Currenti; }
            }
            XmlDoc = null;

            for (var Peak = 1; Peak <= Peaks; Peak++)
            {
                var rel_ab = (double)myArray.GetValue(Peak, 2) / Maxi;
                myArray.SetValue(rel_ab, Peak, 5);
            }

            var MyApp = new Microsoft.Office.Interop.Excel.Application
            {
                Visible = false
            };
            var MyBook = MyApp.Workbooks.Add(1);
            Microsoft.Office.Interop.Excel.Worksheet MySheet = MyBook.Sheets[1];
            MySheet.Cells[1, 1].Value = "mz";
            MySheet.Cells[1, 2].Value = "i";
            MySheet.Cells[1, 3].Value = "sn";
            MySheet.Cells[1, 4].Value = "res";
            MySheet.Cells[1, 5].Value = "rel_ab";

            var MyRange = MySheet.get_Range("A2", "E" + Peaks.ToString());
            MyRange.Value = myArray;
            MyBook.SaveAs(Filename.Substring(0, Filename.Length - Path.GetExtension(Filename).Length) + ".xls");
            var sss = Path.GetFileNameWithoutExtension(Filename);

            oCCia.CleanComObject(MyRange);
            MyRange = null;
            oCCia.CleanComObject(MySheet);
            MySheet = null;
            MyBook.Close(null, null, null);
            oCCia.CleanComObject(MyBook);
            MyBook = null;
            MyApp.Quit();
            oCCia.CleanComObject(MyApp);
            MyApp = null;
            GC.Collect();
        }

        //filter check tab
        private void buttonFilterCheckFormula_Click(object sender, EventArgs e)
        {
            try
            {
                var UserDefinedFilter = new System.Data.DataTable();
                UserDefinedFilter.Columns.Add("Mass", typeof(double));

                foreach (var Name in Enum.GetNames(typeof(CCia.EElemIndex)))
                {
                    UserDefinedFilter.Columns.Add(Name, typeof(short));
                }
                UserDefinedFilter.Columns.Add("UserDefinedFilter", typeof(bool), textBoxFilter.Text);
                UserDefinedFilter.Rows.Add(UserDefinedFilter.NewRow());

                var Mass = (int)numericUpDownCAtoms.Value * CElements.C
                           + (int)numericUpDownHAtoms.Value * CElements.H
                           + (int)numericUpDownOAtoms.Value * CElements.O
                           + (int)numericUpDownNAtoms.Value * CElements.N
                           + (int)numericUpDownSAtoms.Value * CElements.S
                           + (int)numericUpDownPAtoms.Value * CElements.P
                           + (int)numericUpDownNaAtoms.Value * CElements.Na;
                textBoxNeutralMass.Text = Mass.ToString();

                UserDefinedFilter.Rows[0]["Mass"] = Mass;
                UserDefinedFilter.Rows[0][nameof(CCia.EElemIndex.C)] = numericUpDownCAtoms.Value;
                UserDefinedFilter.Rows[0][nameof(CCia.EElemIndex.H)] = numericUpDownHAtoms.Value;
                UserDefinedFilter.Rows[0][nameof(CCia.EElemIndex.O)] = numericUpDownOAtoms.Value;
                UserDefinedFilter.Rows[0][nameof(CCia.EElemIndex.N)] = numericUpDownNAtoms.Value;
                UserDefinedFilter.Rows[0][nameof(CCia.EElemIndex.S)] = numericUpDownSAtoms.Value;
                UserDefinedFilter.Rows[0][nameof(CCia.EElemIndex.P)] = numericUpDownPAtoms.Value;
                UserDefinedFilter.Rows[0][nameof(CCia.EElemIndex.Na)] = numericUpDownNaAtoms.Value;

                textBoxFilterResult.Text = ((bool)UserDefinedFilter.Rows[0]["UserDefinedFilter"]).ToString();
            }
            catch (Exception ex)
            {
                textBoxFilterResult.Text = "Error: " + ex.Message;
            }
        }

        private readonly string[] DBCompositions = {
                "C", "H", "N", "O", "P", "S", "CH", "CN", "CO", "CP",
                "CS", "HN", "HO", "HP", "HS", "NO", "NP", "NS", "OP", "OS",
                "PS", "CHN", "CHO", "CHP", "CHS", "CNO", "CNP", "CNS", "COP", "COS",
                "CPS", "HNO", "HNP", "HNS", "HOP", "HOS", "HPS", "NOP", "NOS", "NPS",
                "OPS", "CHNO", "CHNP", "CHNS", "CHOP", "CHOS", "CHPS", "CNOP", "CNOS", "CNPS",
                "COPS", "HNOP", "HNOS", "HNPS", "HOPS", "NOPS", "CHNOP", "CHNOS", "CHNPS", "CHOPS",
                "CNOPS", "HNOPS", "CHNOPS"
        };
        private void buttonFilterCheckDB_Click(object sender, EventArgs e)
        {
            if (oCCia.DBFormulas == null)
            {
                return;
            }
            var UserDefinedFilter = new System.Data.DataTable();
            UserDefinedFilter.Columns.Add("Mass", typeof(double));

            foreach (var Name in Enum.GetNames(typeof(CCia.EElemIndex)))
            {
                UserDefinedFilter.Columns.Add(Name, typeof(short));
            }
            UserDefinedFilter.Columns.Add("UserDefinedFilter", typeof(bool), textBoxFilter.Text);
            UserDefinedFilter.Rows.Add(UserDefinedFilter.NewRow());

            var CompositionCounts = new int[DBCompositions.Length];

            for (var DBFormulaIndex = 0; DBFormulaIndex < oCCia.DBFormulas.Length; DBFormulaIndex++)
            {
                UserDefinedFilter.Rows[0]["Mass"] = oCCia.DBMasses[DBFormulaIndex];
                var CurFormula = oCCia.DBFormulas[DBFormulaIndex];

                for (var Element = 0; Element < CCia.ElementCount; Element++)
                {
                    UserDefinedFilter.Rows[0][Enum.GetName(typeof(CCia.EElemIndex), Element)] = CurFormula[Element];
                }
                if (!(bool)UserDefinedFilter.Rows[0]["UserDefinedFilter"])
                {
                    continue;
                }

                var bC = CurFormula[(int)CCia.EElemIndex.C] > 0 || CurFormula[(int)CCia.EElemIndex.C13] > 0;
                var bH = CurFormula[(int)CCia.EElemIndex.H] > 0;
                var bO = CurFormula[(int)CCia.EElemIndex.O] > 0;
                var bN = CurFormula[(int)CCia.EElemIndex.N] > 0;
                var bS = CurFormula[(int)CCia.EElemIndex.S] > 0;
                var bP = CurFormula[(int)CCia.EElemIndex.P] > 0;
                var bNa = CurFormula[(int)CCia.EElemIndex.Na] > 0;

                for (var CompositionIndex = 0; CompositionIndex < DBCompositions.Length; CompositionIndex++)
                {
                    var Composition = DBCompositions[CompositionIndex];

                    if (Composition.Contains("C")) { if (!bC) { continue; } }
                    if (Composition.Contains("H")) { if (!bH) { continue; } }
                    if (Composition.Contains("O")) { if (!bO) { continue; } }
                    if (Composition.Contains("N")) { if (!bN) { continue; } }
                    if (Composition.Contains("S")) { if (!bS) { continue; } }
                    if (Composition.Contains("P")) { if (!bP) { continue; } }
                    if (Composition.Contains("Na")) { if (!bNa) { continue; } }
                    CompositionCounts[CompositionIndex]++;
                }
            }
            var Lines = new string[DBCompositions.Length + 2];
            Lines[0] = "Total DB formulas," + oCCia.DBFormulas.Length;
            Lines[1] = "Filter," + textBoxFilter.Text;

            for (var DBFormulaIndex = 0; DBFormulaIndex < DBCompositions.Length; DBFormulaIndex++)
            {
                Lines[DBFormulaIndex + 2] = DBCompositions[DBFormulaIndex] + "," + CompositionCounts[DBFormulaIndex];
            }
            File.WriteAllLines(System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + "\\DBFilteredCompositions.csv", Lines);
        }
        //chain tab
        private void textBoxDropSpectraFile_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void textBoxDropSpectraFile_DragDrop(object sender, DragEventArgs e)
        {
            textBoxChainDropSpectraFile.BackColor = Color.Red;
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("ja-JP");
            try
            {
                var Filenames = (string[])e.Data.GetData(DataFormats.FileDrop);
                Support.CFileReader.ReadFile(Filenames[0], out var InputData);
                var ChainBlocks = new CChainBlocks();
                ChainBlocks.KnownMassBlocksFromFile("dmTransformations_MalakReal.csv");
                var IsotopeFilename = "Isotope.inf";
                CIsotope.ConvertIsotopeFileIntoIsotopeDistanceFile(IsotopeFilename);
                var DistancePeaks = ChainBlocks.GetPairChainIsotopeStatistics(InputData);
                var Records = 0;

                foreach (var CurPairDistance in DistancePeaks)
                {
                    Records += CurPairDistance.DistancePeakTypeList.Count;
                }

                tableLayoutPanelChains.SuspendLayout();
                var Rows = Records + 2;//+ Head + Last Row without Controls
                if (tableLayoutPanelChains.RowStyles.Count > Rows)
                {
                    for (var Row = tableLayoutPanelChains.RowCount - 1; Row >= Rows; Row--)
                    {
                        for (var iColumn = 0; iColumn < tableLayoutPanelChains.ColumnCount; iColumn++)
                        {
                            tableLayoutPanelChains.Controls.RemoveAt(tableLayoutPanelChains.Controls.Count - 1);
                        }
                        tableLayoutPanelChains.RowStyles.RemoveAt(Row);
                    }
                    tableLayoutPanelChains.RowCount = Rows;
                }
                else if (tableLayoutPanelChains.RowStyles.Count < Rows)
                {
                    tableLayoutPanelChains.RowCount = Rows;

                    for (var Row = tableLayoutPanelChains.RowStyles.Count - 1; Row < Rows - 1; Row++)
                    {//"Count-1" due Last Row without Controls
                        tableLayoutPanelChains.RowStyles.Add(new System.Windows.Forms.RowStyle(SizeType.Absolute, new System.Windows.Forms.TextBox().Height + 2 * new System.Windows.Forms.TextBox().Margin.Top));

                        for (var iColumn = 0; iColumn < tableLayoutPanelChains.ColumnCount; iColumn++)
                        {
                            var oTextBox = new System.Windows.Forms.TextBox
                            {
                                Anchor = AnchorStyles.None,
                                ReadOnly = true,
                                AutoSize = true,
                                TextAlign = HorizontalAlignment.Center
                            };
                            tableLayoutPanelChains.Controls.Add(oTextBox, iColumn, Row);
                        }
                    }
                }
                var RecordIndex = 1;

                foreach (var CurPairDistance in DistancePeaks)
                {
                    for (var TypeIndex = 0; TypeIndex < CurPairDistance.DistancePeakTypeList.Count; TypeIndex++)
                    {
                        //string [] ChainColumnNames = new string [] { "Types", "PairCount", "PairDistance", "Formula", "FormulaDistance", "ChainCount" };
                        tableLayoutPanelChains.GetControlFromPosition(0, RecordIndex).Text = CurPairDistance.DistancePeakTypeList[TypeIndex].ToString();
                        tableLayoutPanelChains.GetControlFromPosition(1, RecordIndex).Text = CurPairDistance.PairCount.ToString();
                        tableLayoutPanelChains.GetControlFromPosition(2, RecordIndex).Text = CurPairDistance.Mean.ToString("F6");
                        tableLayoutPanelChains.GetControlFromPosition(3, RecordIndex).Text = CurPairDistance.FormulaList[TypeIndex];
                        tableLayoutPanelChains.GetControlFromPosition(4, RecordIndex).Text = CurPairDistance.FormulaDistanceList[TypeIndex].ToString("F6");
                        tableLayoutPanelChains.GetControlFromPosition(5, RecordIndex).Text = CurPairDistance.ChainCount.ToString();
                        RecordIndex++;
                    }
                }
                textBoxChainDropSpectraFile.BackColor = Color.LightGreen;
                tableLayoutPanelChains.Enabled = true;
                tableLayoutPanelChains.ResumeLayout();
                /*
                    string OutputFilename = Path.GetDirectoryName( Filenames [ 0 ] ) + "\\" + Path.GetFileNameWithoutExtension( Filenames [ 0 ] );
                    int FileCount = Filenames.Length;
                    //Read files & Calibration
                    oCCia.Ipa.Adduct = textBoxAdduct.Text;
                    oCCia.Ipa.Ionization = ( TestFSDBSearch.TotalSupport.IonizationMethod ) Enum.Parse( typeof( TestFSDBSearch.TotalSupport.IonizationMethod ), comboBoxIonization.Text );
                    oCCia.Ipa.CS = ( int ) Math.Abs( numericUpDownCharge.Value );

                    oCCia.oTotalCalibration.ttl_cal_regression = ( TotalCalibration.ttlRegressionType ) Enum.Parse( typeof( TotalCalibration.ttlRegressionType ), comboBoxCalRegressionModel.Text );
                    oCCia.oTotalCalibration.ttl_cal_rf = ( double ) numericUpDownCalRelFactor.Value;
                    oCCia.oTotalCalibration.ttl_cal_start_ppm = ( double ) numericUpDownCalStartTolerance.Value;
                    oCCia.oTotalCalibration.ttl_cal_target_ppm = ( double ) numericUpDownCalEndTolerance.Value;
                    oCCia.oTotalCalibration.ttl_cal_min_sn = ( double ) numericUpDownCalMinSN.Value;
                    oCCia.oTotalCalibration.ttl_cal_min_abu_pct = ( double ) numericUpDownCalMinRelAbun.Value;
                    oCCia.oTotalCalibration.ttl_cal_max_abu_pct = ( double ) numericUpDownCalMaxRelAbun.Value;

                    int ChainMinPeaks = (int) numericUpDownChainMinPeaks.Value;
                    double PeakPpmError = ( double ) numericUpDownChainPpmError.Value;

                    Support.InputData Data = new Support.InputData();
                    Support.CFileReader.ReadFile( Filenames [ 0 ], out Data );
                    oCCia.oTotalCalibration.cal_log.Clear();
                    double [] CalMasses = oCCia.oTotalCalibration.ttl_LQ_InternalCalibration( ref Data.Masses, ref Data.Abundances, ref Data.S2Ns, Support.CArrayMath.Max( Data.Abundances) );
                    double MinMass = 0;
                    double MaxMass = Data.Masses[ Data.Masses.Length - 1];

                    //non-calibrated
                    CChainBlocks oCChainBlocks = new CChainBlocks();
                    oCChainBlocks.FindChains( Data, ChainMinPeaks, PeakPpmError, PeakPpmError, MaxMass, MinMass, MaxMass, false );
                    int ChainsCount = Data.Chains.Length;
                    textBoxChainRawNoncal.Text = ChainsCount.ToString();
                    int MaxChainsPeakIndex = Data.GetMaxChainLength();
                    textBoxChainRawNoncalMaxChainsPeakIndex.Text = MaxChainsPeakIndex.ToString();
                    textBoxChainRawNoncalMaxChainsPeakMass.Text = Data.Masses[ MaxChainsPeakIndex].ToString( "F8");

                    if ( ( checkBoxChainNoncalOutput.Checked == true) && ( checkBoxChainRawChainOutput.Checked == true )){
                        if ( checkBoxChainChainOutput.Checked == true ) {
                            File.WriteAllText( OutputFilename + "NoncalRawChains.csv", Data.ChainsToString() );
                        }
                    }
                    oCChainBlocks.CreateUniqueChains( Data, 3 * PeakPpmError );
                    ChainsCount = Data.Chains.Length;
                    textBoxChainUniqueNoncal.Text = ChainsCount.ToString();
                    MaxChainsPeakIndex = Data.GetMaxChainLength();
                    textBoxChainUniqueNoncalMaxChainsPeakIndex.Text = MaxChainsPeakIndex.ToString();
                    textBoxChainUniqueNoncalMaxChainsPeakMass.Text = Data.Masses [ MaxChainsPeakIndex ].ToString( "F8" );

                    if ( ( checkBoxChainNoncalOutput.Checked == true)
                            && ( checkBoxChainUniqueChainOutput.Checked == true ) ){
                        if ( checkBoxChainChainOutput.Checked == true ) {
                            File.WriteAllText( OutputFilename + "NoncalIniqueChains.csv", Data.ChainsToString() );
                        }
                    }
                    //calibrated
                    Data.Masses = CalMasses;
                    oCChainBlocks.FindChains( Data, ChainMinPeaks, PeakPpmError, PeakPpmError, MaxMass, MinMass, MaxMass, false );
                    ChainsCount = Data.Chains.Length;
                    textBoxChainRawCal.Text = ChainsCount.ToString();
                    MaxChainsPeakIndex = Data.GetMaxChainLength();
                    textBoxChainRawCalMaxChainsPeakIndex.Text = MaxChainsPeakIndex.ToString();
                    textBoxChainRawCalMaxChainsPeakMass.Text = Data.Masses [ MaxChainsPeakIndex ].ToString( "F8" );

                    if ( ( checkBoxChainCalOutput.Checked == true )
                            && ( checkBoxChainRawChainOutput.Checked == true ) ){
                        if(checkBoxChainChainOutput.Checked == true){
                            File.WriteAllText( OutputFilename + "CalRawChains.csv", Data.ChainsToString() );
                        }
                    }
                    oCChainBlocks.CreateUniqueChains( Data, PeakPpmError );
                    int UniqueCalChains = Data.Chains.Length;
                    textBoxChainUniqueCal.Text = UniqueCalChains.ToString();
                    MaxChainsPeakIndex = Data.GetMaxChainLength();
                    textBoxChainUniqueCalMaxChainsPeakIndex.Text = MaxChainsPeakIndex.ToString();
                    textBoxChainUniqueCalMaxChainsPeakMass.Text = Data.Masses [ MaxChainsPeakIndex ].ToString( "F8" );

                    if ( ( checkBoxChainCalOutput.Checked == true )
                            && ( checkBoxChainUniqueChainOutput.Checked == true ) ){
                        if ( checkBoxChainChainOutput.Checked == true ) {
                            File.WriteAllText( OutputFilename + "CalUniqueChains.csv", Data.ChainsToString() );
                        }
                    }

                    textBoxChainRawResult.Text = ( Convert.ToInt32( textBoxChainRawNoncal.Text) - Convert.ToInt32( textBoxChainRawCal.Text) ).ToString();
                    textBoxChainUniqueResult.Text = ( Convert.ToInt32( textBoxChainUniqueNoncal.Text ) - Convert.ToInt32( textBoxChainUniqueCal.Text ) ).ToString();
                    textBoxChainDropSpectraFile.BackColor = Color.LightGreen;
                 */
            }
            catch (Exception Ex)
            {
                MessageBox.Show(Ex.Message);
                textBoxChainDropSpectraFile.BackColor = Color.Pink;
            }
        }

        //Save/restore parameters
        private void buttonSaveParameters_Click(object sender, EventArgs e)
        {
            try
            {
                var OSD = new SaveFileDialog
                {
                    Title = "Save parameters",
                    InitialDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location),
                    Filter = "XML Files (.xml)|*.xml|All Files (*.*)|*.*",
                    FilterIndex = 1
                };

                if (OSD.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    oCCia.GetSaveParameterText(OSD.FileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Parameters weren't saved correctly: " + ex.Message);
            }
        }

        private void textBoxParameterFile_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void textBoxParameterFile_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                var Filenames = (string[])e.Data.GetData(DataFormats.FileDrop);
                var ParameterFilename = Filenames[0];
                textBoxParameterFile.Text = "Parameter file:" + ParameterFilename;

                if (Path.GetExtension(ParameterFilename) != ".xml")
                {
                    MessageBox.Show("Parameter file must have xml extension");
                    return;
                }
                oCCia.LoadParameters(ParameterFilename);
                UpdateCiaAndIpaDialogs();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Parameters weren't loaded correctly. Error - " + ex.Message);
            }
        }
    }

    internal class ReportData
    {
        public short[] Formula;
        public double[] Abundances;
    }
}

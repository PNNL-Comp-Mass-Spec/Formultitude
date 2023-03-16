using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.IO;
using System.Xml;
using System.Runtime.InteropServices;
using Microsoft.Office.Interop.Excel;
using System.Threading;
using System.Globalization;

using FileReader;
using TestFSDBSearch;
using CIA;

namespace CiaUi {
    public partial class FormularityUIForm : Form {
        public CCia oCCia = new CCia();
        CiaAdvancedForm oCiaAdvancedForm;
        //System.Windows.Forms.CheckBox [] GoldenRuleFilterUsage;
        string [] DBPeaksTableHeaders = new string [] { "Index", "Neutral mass", "Formula", "Error, ppm" };
        public enum EPlotType{ ErrorVsNeutralMass, ErrorVs};
        public FormularityUIForm() {
            InitializeComponent();
            oCiaAdvancedForm = new CiaAdvancedForm( this);
            this.SuspendLayout();
            //================
            //CIA tab 
            //================
            //Alignment
            checkBoxAlignment.Checked = oCCia.GetAlignment();
            numericUpDownAlignmentTolerance.Value = ( decimal ) oCCia.GetAlignmentPpmTolerance();

            //Formula assignment     
            textBoxDropDB.Text = "Drop DB files";
            if( oCCia.GetDBFilenames().Length > 0 ) {
                textBoxDropDB.AppendText( "\n\rLoaded:" );
                foreach( string Filename in oCCia.GetDBFilenames() ) {
                    textBoxDropDB.AppendText( "\n\r" + Filename );
                }
            }
            numericUpDownFormulaTolerance.Value = ( decimal ) oCCia.GetFormulaPPMTolerance();      
            numericUpDownDBMassLimit.Value = ( decimal ) oCCia.GetMassLimit();
            comboBoxFormulaScore.DataSource = oCCia.GetFormulaScoreNames();
            comboBoxFormulaScore.SelectedIndex = ( int) oCCia.GetFormulaScore();
            oCiaAdvancedForm.checkBoxCIAUseKendrick.Checked = oCCia.GetUseKendrick();
            oCiaAdvancedForm.checkBoxCIAUseC13.Checked = oCCia.GetUseC13();
            oCiaAdvancedForm.numericUpDownCIAC13Tolerance.Value = ( decimal ) oCCia.GetC13Tolerance();

            //Relations
            checkBoxUseRelation.Checked = oCCia.GetUseRelation();
            numericUpDownMaxRelationshipGaps.Value = oCCia.GetMaxRelationGaps();
            comboBoxRelationshipErrorType.DataSource = Enum.GetNames( typeof( CCia.RelationshipErrorType) );
            comboBoxRelationshipErrorType.SelectedIndex = ( int ) oCCia.GetRelationshipErrorType();
            numericUpDownRelationErrorValue.Value = ( decimal ) oCCia.GetRelationErrorAMU();
            oCiaAdvancedForm.checkBoxCIABackward.Checked = oCCia.GetUseBackward();
            short [] [] DftRelationFormulas = oCCia.GetRelationFormulaBuildingBlocks();
            for( int Relation = 0; Relation < DftRelationFormulas.Length; Relation++ ) {
                bool bb = false;
                if( Relation == 0 || Relation == 2 || Relation == 6 ) { bb = true; }
                checkedListBoxRelations.Items.Add( oCCia.FormulaToName( DftRelationFormulas [ Relation ] ), bb );
            }

            //Filters
            checkBoxUseFormulaFilters.Checked = oCCia.GetUseFormulaFilter();
            //Golden rules
            //GoldenRuleFilterUsage = new System.Windows.Forms.CheckBox [ oCCia.GetGoldenRuleFilterUsage().Length ];
            //System.Windows.Forms.GroupBox groupBoxGoldenRuleFilters = ( System.Windows.Forms.GroupBox ) this.Controls.Find( "groupBoxGoldenRuleFilters", true ) [ 0 ];
            //for( int GoldenRuleIndex = 0; GoldenRuleIndex < GoldenRuleFilterUsage.Length; GoldenRuleIndex++ ) {
            //    GoldenRuleFilterUsage [ GoldenRuleIndex ] = ( System.Windows.Forms.CheckBox ) groupBoxGoldenRuleFilters.Controls [ "checkBoxGoldenRule" + ( GoldenRuleIndex + 1 ).ToString() ];
            //    GoldenRuleFilterUsage [ GoldenRuleIndex ].Text = oCCia.GetGoldenRuleFilterNames() [ GoldenRuleIndex ];
            //    GoldenRuleFilterUsage [ GoldenRuleIndex ].Checked = oCCia.GetGoldenRuleFilterUsage() [ GoldenRuleIndex ];
            //}
            //Special filter
            comboBoxSpecialFilters.Items.Clear();
            string [] SpecialFilterNames = Enum.GetNames( typeof( CCia.ESpecialFilters ) );
            string [] SpecialFilterRules = oCCia.GetSpecialFilterRules();
            for( int SpecialFilter = 0; SpecialFilter < SpecialFilterRules.Length; SpecialFilter++ ) {
                comboBoxSpecialFilters.Items.Add( SpecialFilterNames [ SpecialFilter ] + ": " + SpecialFilterRules [ SpecialFilter ] );
            }
            comboBoxSpecialFilters.Text = oCCia.GetSpecialFilter().ToString();
            //User-defined filters
            textBoxUserDefinedFilter.Text = string.Empty;

            //Reports
            oCiaAdvancedForm.checkBoxIndividualFileReport.Checked = oCCia.GetGenerateIndividualFileReports();
            oCiaAdvancedForm.checkBoxChainReport.Checked = oCCia.GetGenerateChainReport();

            //Out file formats
            oCiaAdvancedForm.comboBoxOutputFileDelimiter.DataSource = Enum.GetNames( typeof( CCia.EDelimiters ) );
            oCiaAdvancedForm.comboBoxOutputFileDelimiter.Text = oCCia.GetOutputFileDelimiterType().ToString();
            oCiaAdvancedForm.comboBoxErrorType.DataSource = Enum.GetNames( typeof( CCia.EErrorType ) );
            oCiaAdvancedForm.comboBoxErrorType.Text = oCCia.GetErrorType().ToString();

            //checkBoxLogReport.Checked = oCCia.GetLogReportStatus();
            
            //================
            //IPA tab (Isotopic pattern algorithm)
            //============
            numericUpDownIpaMassTolerance.Value = ( decimal ) oCCia.Ipa.m_ppm_tol;
            numericUpDownIpaMajorPeaksMinSN.Value = ( decimal ) oCCia.Ipa.m_min_major_sn;
            numericUpDownIpaMinorPeaksMinSN.Value = ( decimal ) oCCia.Ipa.m_min_minor_sn;

            numericUpDownIpaMinMajorPeaksToAbsToReport.Value = ( decimal ) oCCia.Ipa.m_min_major_pa_mm_abs_2_report;
            checkBoxIpaMatchedPeakReport.Checked = oCCia.Ipa.m_matched_peaks_report;

            numericUpDownIpaMinPeakProbabilityToScore.Value = ( decimal ) oCCia.Ipa.m_min_p_to_score;
            //textBoxIpaFilter.Text = oCCia.m_IPDB_ec_filter;
            cmdIpaMergeWithCIA.Visible = false;

            //===============
            //chartError tab`
            //=============
            comboBoxPlotType.DataSource = Enum.GetNames( typeof( EPlotType ) );
            comboBoxPlotType.SelectedIndex = 0;                

            //================
            //DB inspector tab
            //================
            numericDBUpDownMass.Enabled = false;
            tableLayoutPanelDBPeaks.Enabled = false;
            tableLayoutPanelDBPeaks.AutoScroll = true;
            tableLayoutPanelDBPeaks.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
            tableLayoutPanelDBPeaks.ColumnStyles.Clear();
            tableLayoutPanelDBPeaks.ColumnCount = DBPeaksTableHeaders.Length;
            for( int iColumn = 0; iColumn < tableLayoutPanelDBPeaks.ColumnCount; iColumn++ ) {
                tableLayoutPanelDBPeaks.ColumnStyles.Add( new System.Windows.Forms.ColumnStyle( SizeType.Percent, ( float ) 100.0 / DBPeaksTableHeaders.Length) );
            }
            tableLayoutPanelDBPeaks.RowStyles.Clear();
            tableLayoutPanelDBPeaks.RowCount = 5 + 1;//Extra Row without RowStyle!!!
            for( int iRow = 0; iRow < tableLayoutPanelDBPeaks.RowCount; iRow++ ) {
                tableLayoutPanelDBPeaks.RowStyles.Add( new System.Windows.Forms.RowStyle( SizeType.Absolute, ( new System.Windows.Forms.TextBox() ).Height + 2 * ( new System.Windows.Forms.TextBox() ).Margin.Top ) );
            }
            for( int iRow = 0; iRow < tableLayoutPanelDBPeaks.RowCount - 1; iRow++ ) {// "-1" Extra Row without Controls!!!
                for( int iColumn = 0; iColumn < tableLayoutPanelDBPeaks.ColumnCount; iColumn++ ) {
                    System.Windows.Forms.TextBox oTextBox = new System.Windows.Forms.TextBox();
                    oTextBox.Anchor = AnchorStyles.None;
                    oTextBox.ReadOnly = true;
                    oTextBox.AutoSize = true;                  
                    oTextBox.TextAlign = HorizontalAlignment.Center;
                    if( iRow == 0){
                        oTextBox.ReadOnly = true;
                        oTextBox.Text = DBPeaksTableHeaders [ iColumn ];
                    }
                    tableLayoutPanelDBPeaks.Controls.Add( oTextBox, iColumn, iRow );
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
            numericUpDownDBMassRange.Value = (decimal) oCCia.GetDBMassRange();

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


            //===============
            //Spectra files area
            //===============
            comboBoxIonization.DataSource = Enum.GetValues( typeof( TestFSDBSearch.TotalSupport.IonizationMethod) );
            comboBoxIonization.Text = oCCia.Ipa.Ionization.ToString();
            textBoxAdduct.Text = oCCia.Ipa.Adduct;
            numericUpDownCharge.Value = oCCia.Ipa.CS;

            //checkBoxCIA
            //checkBoxIpa

            //Calibration
            comboBoxCalRegressionModel.DataSource = Enum.GetValues( typeof( TotalCalibration.ttlRegressionType ) );
            comboBoxCalRegressionModel.Text = oCCia.oTotalCalibration.ttl_cal_regression.ToString();
            numericUpDownCalRelFactor.Value = ( decimal ) oCCia.oTotalCalibration.ttl_cal_rf;
            numericUpDownCalStartTolerance.Value = ( decimal ) oCCia.oTotalCalibration.ttl_cal_start_ppm;
            numericUpDownCalEndTolerance.Value = ( decimal ) oCCia.oTotalCalibration.ttl_cal_target_ppm;
            numericUpDownCalMinSN.Value = ( decimal ) oCCia.oTotalCalibration.ttl_cal_min_sn;
            numericUpDownCalMinRelAbun.Value = ( decimal ) oCCia.oTotalCalibration.ttl_cal_min_abu_pct;
            numericUpDownCalMaxRelAbun.Value = ( decimal ) oCCia.oTotalCalibration.ttl_cal_max_abu_pct;

            this.ResumeLayout();

            string DefaultParametersFile = Path.GetDirectoryName( System.Reflection.Assembly.GetEntryAssembly().Location) + "\\DefaultParameters.xml";
            if( File.Exists( DefaultParametersFile ) == true ) {
                LoadParameters( DefaultParametersFile);
            }
        }
        private void CIAUIForm_FormClosing( object sender, FormClosingEventArgs e ) {
            string DefaultParametersFile = Path.GetDirectoryName( System.Reflection.Assembly.GetEntryAssembly().Location ) + "\\DefaultParameters.xml";
            SaveParameters( DefaultParametersFile );
        }

        //Input files
        private void textBoxAdduct_KeyDown( object sender, KeyEventArgs e ) {
            try {
                if( e.KeyCode == Keys.Return ) {
                    oCCia.Ipa.Adduct = textBoxAdduct.Text;
                    textBoxResult.Text = oCCia.Ipa.ChargedMassFormula_Descriptive;
                    //numericUpDownMass_ValueChanged( sender, e );//to update DB instector tab
                }
            } catch( Exception ex ) {
                MessageBox.Show( ex.Message );
            }
        }
        private void textBoxAdduct_Leave( object sender, EventArgs e ) {
            try {
                oCCia.Ipa.Adduct = textBoxAdduct.Text;
                textBoxResult.Text = oCCia.Ipa.ChargedMassFormula_Descriptive;
                //numericUpDownMass_ValueChanged( sender, e );//to update DB instector tab
            } catch( Exception ex ) {
                MessageBox.Show( ex.Message );
            }
        }
        private void comboBoxIonization_SelectedIndexChanged( object sender, EventArgs e ) {
            oCCia.Ipa.Ionization = ( TestFSDBSearch.TotalSupport.IonizationMethod ) Enum.Parse( typeof( TestFSDBSearch.TotalSupport.IonizationMethod), comboBoxIonization.Text );
            textBoxResult.Text = oCCia.Ipa.ChargedMassFormula_Descriptive;
            //numericUpDownMass_ValueChanged( sender, e );//to update DB instector tab
        }
        private void numericUpDownCharge_ValueChanged( object sender, EventArgs e ) {
            oCCia.Ipa.CS = ( int ) Math.Abs( numericUpDownCharge.Value );
            textBoxResult.Text = oCCia.Ipa.ChargedMassFormula_Descriptive;
            //numericUpDownMass_ValueChanged( sender, e );//to update DB instector tab
        }
        void CheckToProcess() {
            bool CalibrationReady = ( ( ( TotalCalibration.ttlRegressionType ) comboBoxCalRegressionModel.SelectedValue == TotalCalibration.ttlRegressionType.none )
                        | ( ( ( TotalCalibration.ttlRegressionType ) comboBoxCalRegressionModel.SelectedValue != TotalCalibration.ttlRegressionType.none ) & ( textBoxCalFile.TextLength > "Drop calibration file: ".Length ) ) );

            bool CIAReady = ( oCCia.GetDBFilenames().Length > 0 ) & CalibrationReady;
            checkBoxCIA.Enabled = CIAReady;

            bool IpaReady = oCCia.Ipa.IPDB_Ready & CalibrationReady;
            checkBoxIpa.Enabled = IpaReady;

            if( ( CIAReady == true ) && ( checkBoxCIA.Checked == true )
                    || ( IpaReady == true ) && ( checkBoxIpa.Checked == true ) ) {
                textBoxDropSpectraFiles.BackColor = Color.LightGreen;
                textBoxDropSpectraFiles.Enabled = true;
            } else {
                textBoxDropSpectraFiles.BackColor = SystemColors.ControlLight;
                textBoxDropSpectraFiles.Enabled = false;
            }
        }
        private void checkBoxCIA_CheckedChanged( object sender, EventArgs e ) {
            CheckToProcess();
        }
        private void checkBoxIpa_CheckedChanged( object sender, EventArgs e ) {
            CheckToProcess();
        }
        private void textBoxCalFile_DragEnter( object sender, DragEventArgs e ) {
            if( e.Data.GetDataPresent( DataFormats.FileDrop ) == true ) {
                e.Effect = DragDropEffects.Copy;
            }
        }
        private void textBoxCalFile_DragDrop( object sender, DragEventArgs e ) {
            string [] Filenames = ( string [] ) e.Data.GetData( DataFormats.FileDrop );
            oCCia.oTotalCalibration.Load( Filenames [ 0 ] );
            textBoxCalFile.Text = "Drop calibration file: " + Path.GetFileName( Filenames [ 0 ] );
            CheckToProcess();
        }
        private void comboBoxCalRegressionModel_SelectedIndexChanged( object sender, EventArgs e ) {
            if( comboBoxCalRegressionModel.Text == TotalCalibration.ttlRegressionType.none.ToString() ) {
                textBoxCalFile.Enabled = false;
                numericUpDownCalRelFactor.Enabled = false;
                numericUpDownCalStartTolerance.Enabled = false;
                numericUpDownCalEndTolerance.Enabled = false;
                numericUpDownCalMinSN.Enabled = false;
                numericUpDownCalMinRelAbun.Enabled = false;
                numericUpDownCalMaxRelAbun.Enabled = false;
            } else {
                textBoxCalFile.Enabled = true;
                numericUpDownCalRelFactor.Enabled = true;
                numericUpDownCalStartTolerance.Enabled = true;
                numericUpDownCalEndTolerance.Enabled = true;
                numericUpDownCalMinSN.Enabled = true;
                numericUpDownCalMinRelAbun.Enabled = true;
                numericUpDownCalMaxRelAbun.Enabled = true;
            }
            CheckToProcess();
        }
        private void textBoxDropSpectraFiles_DragEnter( object sender, DragEventArgs e ) {
            if( e.Data.GetDataPresent( DataFormats.FileDrop ) == true ) {
                e.Effect = DragDropEffects.Copy;
            }
        }
        private void textBoxDropSpectraFiles_DragDrop( object sender, DragEventArgs e ) {
            textBoxDropSpectraFiles.BackColor = Color.Red;
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture( "ja-JP" );
            try {
                string [] Filenames = ( string [] ) e.Data.GetData( DataFormats.FileDrop );
                //log file
                string LogFileName = DateTime.Now.ToString();
                LogFileName = Path.GetDirectoryName( Filenames [ 0 ] ) + "\\" + "Report" + LogFileName.Replace( "/", "" ).Replace( ":", "" ).Replace( " ", "" ) + ".log";
                StreamWriter oStreamLogWriter = new StreamWriter( LogFileName );

                int FileCount = Filenames.Length;
                double [] [] Masses = new double [ FileCount ] [];
                double [] [] Abundances = new double [ FileCount ] [];
                double [] [] SNs = new double [ FileCount ] [];
                double [] [] Resolutions = new double [ FileCount ] [];
                double [] [] RelAbundances = new double [ FileCount ] [];
                
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
                double [] MaxAbundances = new double [ FileCount ];
                double [] [] CalMasses = new double [ FileCount ] [];
                for( int FileIndex = 0; FileIndex < FileCount; FileIndex++ ) {
                    //read files
                    FileReader.FileReader.ReadFile( Filenames [ FileIndex ], out Masses [ FileIndex ], out Abundances [ FileIndex ], out SNs [ FileIndex ], out Resolutions [ FileIndex ], out RelAbundances [ FileIndex ] );
                    double MaxAbundance = Abundances [ FileIndex ] [ 0 ];
                    foreach( double Abundabce in Abundances [ FileIndex ] ) { if( MaxAbundance < Abundabce ) { MaxAbundance = Abundabce; } }
                    MaxAbundances [ FileIndex ] = MaxAbundance;
                    //Calibration
                    if( oCCia.oTotalCalibration.ttl_cal_regression == TotalCalibration.ttlRegressionType.none ) {
                        CalMasses [ FileIndex ] = new double [ Masses [ FileIndex ].Length ];
                        for( int PeakIndex = 0; PeakIndex < CalMasses.Length; PeakIndex++ ) {
                            CalMasses [ PeakIndex ] = Masses [ PeakIndex ];
                        }
                    } else {
                        oCCia.oTotalCalibration.cal_log.Clear();
                        CalMasses [ FileIndex ] = oCCia.oTotalCalibration.ttl_LQ_InternalCalibration( ref Masses [ FileIndex ], ref Abundances [ FileIndex ], ref SNs [ FileIndex ], MaxAbundance );
                        oStreamLogWriter.WriteLine();
                        oStreamLogWriter.WriteLine( "Calibration of " + Path.GetFileName( Filenames [ FileIndex ] ) );
                        oStreamLogWriter.WriteLine();
                        oStreamLogWriter.Write( oCCia.oTotalCalibration.cal_log );
                    }
                }
                if( checkBoxCIA.Checked == true ) {
                    //if( oCCia.GetDBFilenames().Length == 0 ) { throw new Exception( "Drop DB file." ); }
                    //Alignment
                    oCCia.SetAlignment( checkBoxAlignment.Checked );
                    oCCia.SetAlignmentPpmTolerance( ( double ) numericUpDownAlignmentTolerance.Value );

                    //Formula assignment
                    oCCia.SetMassLimit( ( double ) numericUpDownDBMassLimit.Value );
                    oCCia.SetFormulaScore( ( CCia.EFormulaScore ) Array.IndexOf( oCCia.GetFormulaScoreNames(), comboBoxFormulaScore.Text ) );
                    if( checkBoxCIAUseDefault.Checked == false ) {
                        oCCia.SetUseKendrick( oCiaAdvancedForm.checkBoxCIAUseKendrick.Checked );
                        oCCia.SetUseC13( oCiaAdvancedForm.checkBoxCIAUseC13.Checked );
                        oCCia.SetC13Tolerance( ( double ) oCiaAdvancedForm.numericUpDownCIAC13Tolerance.Value );
                    } else {
                        oCCia.SetUseKendrick( true);
                        oCCia.SetUseC13( true);
                        oCCia.SetC13Tolerance( ( double ) numericUpDownFormulaTolerance.Value );
                    }

                    //Filters
                    oCCia.SetUseFormulaFilter( checkBoxUseFormulaFilters.Checked );
                    bool [] GoldenFilters = new bool [ oCCia.GoldenRuleFilterNames.Length ];
                    if( checkBoxCIAUseDefault.Checked == false ) {
                        GoldenFilters [ 0 ] = oCiaAdvancedForm.checkBoxGoldenRule1.Checked;
                        GoldenFilters [ 1 ] = oCiaAdvancedForm.checkBoxGoldenRule2.Checked;
                        GoldenFilters [ 2 ] = oCiaAdvancedForm.checkBoxGoldenRule3.Checked;
                        GoldenFilters [ 3 ] = oCiaAdvancedForm.checkBoxGoldenRule4.Checked;
                        GoldenFilters [ 4 ] = oCiaAdvancedForm.checkBoxGoldenRule5.Checked;
                        GoldenFilters [ 5 ] = oCiaAdvancedForm.checkBoxGoldenRule6.Checked;
                    } else {
                        GoldenFilters [ 0 ] = true;
                        GoldenFilters [ 1 ] = true;
                        GoldenFilters [ 2 ] = true;
                        GoldenFilters [ 3 ] = true;
                        GoldenFilters [ 4 ] = true;
                        GoldenFilters [ 5 ] = false;
                    }
                    oCCia.SetGoldenRuleFilterUsage( GoldenFilters );

                    oCCia.SetSpecialFilter( ( CCia.ESpecialFilters ) Enum.Parse( typeof( CCia.ESpecialFilters ), comboBoxSpecialFilters.Text.Split( new char [] { ':' } ) [ 0 ] ) );
                    oCCia.SetUserDefinedFilter( textBoxUserDefinedFilter.Text );
                    //Relationships
                    oCCia.SetUseRelation( checkBoxUseRelation.Checked );
                    oCCia.SetMaxRelationGaps( ( int ) numericUpDownMaxRelationshipGaps.Value );
                    oCCia.SetRelationshipErrorType( ( CCia.RelationshipErrorType ) Enum.Parse( typeof( CCia.RelationshipErrorType ), comboBoxRelationshipErrorType.Text ) ); 
                    oCCia.SetRelationErrorAMU( ( double ) numericUpDownRelationErrorValue.Value );
                    if( checkBoxCIAUseDefault.Checked == false ) {
                        oCCia.SetUseBackward( oCiaAdvancedForm.checkBoxCIABackward.Checked );                        
                    } else {
                        oCCia.SetUseBackward( false );
                    }

                    short [] [] ActiveRelationBlocks = new short [ checkedListBoxRelations.CheckedItems.Count ] [];
                    for( int ActiveFormula = 0; ActiveFormula < checkedListBoxRelations.CheckedItems.Count; ActiveFormula++ ) {
                        ActiveRelationBlocks [ ActiveFormula ] = oCCia.NameToFormula( checkedListBoxRelations.CheckedItems [ ActiveFormula ].ToString() );
                    }
                    oCCia.SetRelationFormulaBuildingBlocks( ActiveRelationBlocks );
                    
                    if( checkBoxCIAUseDefault.Checked == false ) {
                        //Reports
                        oCCia.SetGenerateIndividualFileReports( oCiaAdvancedForm.checkBoxIndividualFileReport.Checked );
                        oCCia.SetGenerateChainReport( oCiaAdvancedForm.checkBoxChainReport.Checked );
                        //File formats
                        oCCia.SetOutputFileDelimiterType( ( CCia.EDelimiters ) Enum.Parse( typeof( CCia.EDelimiters ), oCiaAdvancedForm.comboBoxOutputFileDelimiter.Text ) );
                        oCCia.SetErrorType( ( CCia.EErrorType ) Enum.Parse( typeof( CCia.EErrorType ), oCiaAdvancedForm.comboBoxErrorType.Text ) );
                    } else {
                        //Reports
                        if( checkBoxAlignment.Checked == true ) {
                            oCCia.SetGenerateIndividualFileReports( false );
                        } else {
                            oCCia.SetGenerateIndividualFileReports( true);
                        }
                        oCCia.SetGenerateChainReport( false);
                        //File formats
                        oCCia.SetOutputFileDelimiterType( CCia.EDelimiters.Comma);
                        oCCia.SetErrorType( CCia.EErrorType.Signed);
                    }

                    //oCCia.SetLogReportStatus( checkBoxLogReport.Checked );

                    //Process                    
                    oCCia.Process( Filenames, Masses, Abundances, SNs, Resolutions, RelAbundances, CalMasses, oStreamLogWriter );
 
                    //change textbox
                    textBoxDropSpectraFiles.Text = "Drop Spectra Files";
                    textBoxDropSpectraFiles.AppendText( "\r\nProcessed files:" );
                    foreach( string Filename in Filenames ) {
                        textBoxDropSpectraFiles.AppendText( "\r\n" + Path.GetFileName( Filename ) );
                    }
                }
                if( checkBoxIpa.Checked == true ) {
                    bool b = oCCia.Ipa.SetCalculation();

                    oCCia.Ipa.m_ppm_tol = ( double ) numericUpDownIpaMassTolerance.Value;
                    oCCia.Ipa.m_min_major_sn = ( double ) numericUpDownIpaMajorPeaksMinSN.Value;
                    oCCia.Ipa.m_min_minor_sn = ( double ) numericUpDownIpaMinorPeaksMinSN.Value;

                    oCCia.Ipa.m_min_major_pa_mm_abs_2_report = ( double ) numericUpDownIpaMinMajorPeaksToAbsToReport.Value;
                    oCCia.Ipa.m_matched_peaks_report = checkBoxIpaMatchedPeakReport.Checked;

                    oCCia.Ipa.m_min_p_to_score = ( double ) numericUpDownIpaMinPeakProbabilityToScore.Value;

                    oCCia.Ipa.m_IPDB_ec_filter = textBoxIpaFilter.Text;

                    for( int FileIndex = 0; FileIndex < FileCount; FileIndex++ ) {
                        oCCia.Ipa.IPDB_log.Clear();                        
                        //oCCia.Ipa.ttlSearch( ref Masses [ FileIndex ], ref Abundances [ FileIndex ], ref SNs [ FileIndex ], ref MaxAbundances [ FileIndex ], Filenames [ FileIndex ] );
                        oCCia.Ipa.ttlSearch( ref CalMasses [ FileIndex ], ref Abundances [ FileIndex ], ref SNs [ FileIndex ], ref MaxAbundances [ FileIndex ], Filenames [ FileIndex ] );
                        //string LogFileName = Path.GetDirectoryName( Filenames [ FileIndex ] ) + "\\" + Path.GetFileNameWithoutExtension( Filenames [ FileIndex ] ) + ".log";
                        //StreamWriter oStreamLogWriter = new StreamWriter( LogFileName );
                        oStreamLogWriter.Write( oCCia.Ipa.IPDB_log );
                        
                    }
                }
                oStreamLogWriter.Flush();
                oStreamLogWriter.Close();    
            } catch( Exception Ex ) {
                MessageBox.Show( Ex.Message );
                textBoxDropSpectraFiles.BackColor = Color.Pink;
            }            
            textBoxDropSpectraFiles.BackColor = Color.LightGreen;
        }

        //tab
        private void tabControl1_SelectedIndexChanged( object sender, EventArgs e ) {
            string ddd = tabControl1.TabPages [ tabControl1.SelectedIndex ].Text;
            if( ddd == "CIA DB inspector" ) {
                numericUpDownMass_ValueChanged( sender, e );
            }
        }
        //CIA tab
        private void checkBoxAlignment_CheckedChanged( object sender, EventArgs e ) {
            numericUpDownAlignmentTolerance.Enabled = checkBoxAlignment.Checked;
        }
        private void textBoxDropDB_DragEnter( object sender, DragEventArgs e ) {
            if( e.Data.GetDataPresent( DataFormats.FileDrop ) == true ) {
                e.Effect = DragDropEffects.Copy;
            }
        }
        private void textBoxDropDB_DragDrop( object sender, DragEventArgs e ) {
            string [] Filenames = ( string [] ) e.Data.GetData( DataFormats.FileDrop );
            oCCia.LoadDBs( Filenames );
            textBoxDropDB.Text = "Drop DB files";
            textBoxDropDB.AppendText( "\r\nLoaded:" );
            foreach( string Filename in oCCia.GetDBFilenames() ) {
                textBoxDropDB.AppendText( "\r\n" + Path.GetFileName( Filename ) );
            }
            numericDBUpDownMass.Enabled = true;
            textBoxDBRecords.Text = oCCia.GetDBRecords().ToString();
            textBoxDBMinMass.Text = oCCia.GetDBMinMass().ToString();
            textBoxDBMaxMass.Text = oCCia.GetDBMaxMass().ToString();
            textBoxDBMinError.Text = oCCia.GetDBMinError().ToString();
            textBoxDBMaxError.Text = oCCia.GetDBMaxError().ToString();
            CheckToProcess();
        }
        private void numericUpDownFormulaError_ValueChanged( object sender, EventArgs e ) {
            oCCia.SetFormulaPPMTolerance( ( double ) numericUpDownFormulaTolerance.Value );
            numericUpDownMass_ValueChanged( sender, e );//to update DB instector tab
        }
        private void checkBoxUseFormulaFilters_CheckedChanged( object sender, EventArgs e ) { 
            //groupBoxGoldenRuleFilters.Enabled = checkBoxUseFormulaFilters.Checked;
            comboBoxSpecialFilters.Enabled = checkBoxUseFormulaFilters.Checked;
            textBoxUserDefinedFilter.Enabled = checkBoxUseFormulaFilters.Checked;
        }
        private void checkBoxUseRelation_CheckedChanged( object sender, EventArgs e ) {
            numericUpDownMaxRelationshipGaps.Enabled = checkBoxUseRelation.Checked;
            comboBoxRelationshipErrorType.Enabled = checkBoxUseRelation.Checked;
            numericUpDownRelationErrorValue.Enabled = checkBoxUseRelation.Checked;
            checkedListBoxRelations.Enabled = checkBoxUseRelation.Checked;
        }
        private void buttonLoadCiaParameters_Click( object sender, EventArgs e ) {
            textBoxAdduct.Text = "H";
            comboBoxIonization.Text = TestFSDBSearch.TotalSupport.IonizationMethod.proton_attachment.ToString();
            numericUpDownCharge.Value = 1;

            checkBoxAlignment.Checked = true;
            numericUpDownAlignmentTolerance.Value = ( decimal ) oCCia.GetAlignmentPpmTolerance();

            checkBoxUseRelation.Checked = true;
            numericUpDownDBMassLimit.Value = 500;
            comboBoxFormulaScore.SelectedIndex = ( int) CCia.EFormulaScore.HAcap;

            checkBoxUseFormulaFilters.Checked = true;
            //for( int GoldenRuleFilter = 0; GoldenRuleFilter < GoldenRuleFilterUsage.Length - 1; GoldenRuleFilter++ ) {
            //    GoldenRuleFilterUsage [ GoldenRuleFilter ].Checked = true;
            //}
            //GoldenRuleFilterUsage [ 5].Checked = false;
            comboBoxSpecialFilters.SelectedIndex = 0;
            textBoxUserDefinedFilter.Text = string.Empty;

            checkBoxUseRelation.Checked = true;
            numericUpDownMaxRelationshipGaps.Value = 5;
            numericUpDownRelationErrorValue.Value = ( decimal ) 0.00002;
            for( int RelationBlock = 0; RelationBlock < checkedListBoxRelations.Items.Count; RelationBlock++ ) {
                if( ( RelationBlock == 0 ) || ( RelationBlock == 2 ) || ( RelationBlock == 6 ) ) {
                    checkedListBoxRelations.SetItemChecked( RelationBlock, true );
                } else {
                    checkedListBoxRelations.SetItemChecked( RelationBlock, false );
                }
            }

            oCiaAdvancedForm.checkBoxIndividualFileReport.Checked = false;
            oCiaAdvancedForm.checkBoxChainReport.Checked = false;
            oCiaAdvancedForm.comboBoxErrorType.Text = CCia.EErrorType.CIA.ToString();
        }
        
        private void buttonSwitchToAdvanced_Click( object sender, EventArgs e ) {
            oCiaAdvancedForm.numericUpDownCharge.Value = numericUpDownCharge.Value;
            oCiaAdvancedForm.textBoxAdduct.Text = textBoxAdduct.Text;
            oCiaAdvancedForm.comboBoxIonization.Text = comboBoxIonization.Text;
            oCiaAdvancedForm.textBoxResult.Text = textBoxResult.Text;

            oCiaAdvancedForm.textBoxCalFile.Text = textBoxCalFile.Text;
            oCiaAdvancedForm.comboBoxCalRegressionModel.SelectedIndex = comboBoxCalRegressionModel.SelectedIndex;
            oCiaAdvancedForm.numericUpDownCalStartTolerance.Value = numericUpDownCalStartTolerance.Value;
            oCiaAdvancedForm.numericUpDownCalRelFactor.Value = numericUpDownCalRelFactor.Value;
            oCiaAdvancedForm.numericUpDownCalEndTolerance.Value = numericUpDownCalEndTolerance.Value;
            oCiaAdvancedForm.numericUpDownCalMinSN.Value = numericUpDownCalMinSN.Value;
            oCiaAdvancedForm.numericUpDownCalMinRelAbun.Value = numericUpDownCalMinRelAbun.Value;
            oCiaAdvancedForm.numericUpDownCalMaxRelAbun.Value = numericUpDownCalMaxRelAbun.Value;

            oCiaAdvancedForm.checkBoxAlignment.Checked = checkBoxAlignment.Checked;
            oCiaAdvancedForm.numericUpDownAlignmentTolerance.Value = numericUpDownAlignmentTolerance.Value;
            oCiaAdvancedForm.textBoxDropDB.Text = textBoxDropDB.Text;
            oCiaAdvancedForm.numericUpDownFormulaTolerance.Value = numericUpDownFormulaTolerance.Value;
            oCiaAdvancedForm.numericUpDownDBMassLimit.Value = numericUpDownDBMassLimit.Value;
            oCiaAdvancedForm.comboBoxFormulaScore.SelectedIndex = comboBoxFormulaScore.SelectedIndex;
            oCiaAdvancedForm.checkBoxUseFormulaFilters.Checked = checkBoxUseFormulaFilters.Checked;
            oCiaAdvancedForm.checkBoxUseRelation.Checked = checkBoxUseRelation.Checked;
            oCiaAdvancedForm.numericUpDownMaxRelationshipGaps.Value = numericUpDownMaxRelationshipGaps.Value;
            oCiaAdvancedForm.comboBoxRelationshipErrorType.SelectedIndex = comboBoxRelationshipErrorType.SelectedIndex;
            oCiaAdvancedForm.numericUpDownRelationErrorValue.Value = numericUpDownRelationErrorValue.Value;

            for( int RelationIndex = 0; RelationIndex < checkedListBoxRelations.CheckedItems.Count; RelationIndex++ ) {
                oCiaAdvancedForm.checkedListBoxRelations.SetItemChecked( RelationIndex, checkedListBoxRelations.GetItemChecked( RelationIndex ) );
            }

            oCiaAdvancedForm.comboBoxSpecialFilters.SelectedIndex = comboBoxSpecialFilters.SelectedIndex;
            oCiaAdvancedForm.textBoxUserDefinedFilter.Text = textBoxUserDefinedFilter.Text;
            oCiaAdvancedForm.CheckToProcess();

            this.Visible = false;
            DialogResult sss = oCiaAdvancedForm.ShowDialog( this );

            numericUpDownCharge.Value = oCiaAdvancedForm.numericUpDownCharge.Value;
            textBoxAdduct.Text = oCiaAdvancedForm.textBoxAdduct.Text;
            comboBoxIonization.Text = oCiaAdvancedForm.comboBoxIonization.Text;
            textBoxResult.Text = oCiaAdvancedForm.textBoxResult.Text;

            textBoxCalFile.Text = oCiaAdvancedForm.textBoxCalFile.Text;
            comboBoxCalRegressionModel.SelectedIndex = oCiaAdvancedForm.comboBoxCalRegressionModel.SelectedIndex;
            numericUpDownCalStartTolerance.Value = oCiaAdvancedForm.numericUpDownCalStartTolerance.Value;
            numericUpDownCalRelFactor.Value = oCiaAdvancedForm.numericUpDownCalRelFactor.Value;
            numericUpDownCalEndTolerance.Value = oCiaAdvancedForm.numericUpDownCalEndTolerance.Value;
            numericUpDownCalMinSN.Value = oCiaAdvancedForm.numericUpDownCalMinSN.Value;
            numericUpDownCalMinRelAbun.Value = oCiaAdvancedForm.numericUpDownCalMinRelAbun.Value;
            numericUpDownCalMaxRelAbun.Value = oCiaAdvancedForm.numericUpDownCalMaxRelAbun.Value;

            checkBoxAlignment.Checked = oCiaAdvancedForm.checkBoxAlignment.Checked;
            numericUpDownAlignmentTolerance.Value = oCiaAdvancedForm.numericUpDownAlignmentTolerance.Value;
            textBoxDropDB.Text = oCiaAdvancedForm.textBoxDropDB.Text;
            numericUpDownFormulaTolerance.Value = oCiaAdvancedForm.numericUpDownFormulaTolerance.Value;
            numericUpDownDBMassLimit.Value = oCiaAdvancedForm.numericUpDownDBMassLimit.Value;
            comboBoxFormulaScore.SelectedIndex = oCiaAdvancedForm.comboBoxFormulaScore.SelectedIndex;
            checkBoxUseFormulaFilters.Checked = oCiaAdvancedForm.checkBoxUseFormulaFilters.Checked;
            checkBoxUseRelation.Checked = oCiaAdvancedForm.checkBoxUseRelation.Checked;
            numericUpDownMaxRelationshipGaps.Value = oCiaAdvancedForm.numericUpDownMaxRelationshipGaps.Value;
            comboBoxRelationshipErrorType.SelectedIndex = oCiaAdvancedForm.comboBoxRelationshipErrorType.SelectedIndex;
            numericUpDownRelationErrorValue.Value = oCiaAdvancedForm.numericUpDownRelationErrorValue.Value;

            for( int RelationIndex = 0; RelationIndex < checkedListBoxRelations.CheckedItems.Count; RelationIndex++ ) {
                checkedListBoxRelations.SetItemChecked( RelationIndex, oCiaAdvancedForm.checkedListBoxRelations.GetItemChecked( RelationIndex ) );
            }

            comboBoxSpecialFilters.SelectedIndex = oCiaAdvancedForm.comboBoxSpecialFilters.SelectedIndex;
            textBoxUserDefinedFilter.Text = oCiaAdvancedForm.textBoxUserDefinedFilter.Text;
            CheckToProcess();

            this.Visible = true;
        }

        //Ipa tab
        private void textBoxIpaDropDBFile_DragEnter( object sender, DragEventArgs e ) {
            if( e.Data.GetDataPresent( DataFormats.FileDrop ) == true ) {
                e.Effect = DragDropEffects.Copy;
            }
        }
        private void textBoxIpaDropDBFile_DragDrop( object sender, DragEventArgs e ) {
            string [] Filenames = ( string [] ) e.Data.GetData( DataFormats.FileDrop );
            oCCia.Ipa.LoadTabulatedDB( Filenames [ 0 ] );//???
            CheckToProcess();
            textBoxIpaDropDBFile.Text = Filenames [ 0 ];
        }

        //Error plot tab
        List<double> XData = new List<double>();
        List<double> YData = new List<double>();
        private void chartError_DragEnter( object sender, DragEventArgs e ) {
            if( e.Data.GetDataPresent( DataFormats.FileDrop ) == true ) {
                e.Effect = DragDropEffects.Copy;
            }
        }
        private void chartError_DragDrop( object sender, DragEventArgs e ) {
            string [] Filenames = ( string [] ) e.Data.GetData( DataFormats.FileDrop );
            string [] Lines = File.ReadAllLines( Filenames[ 0]);
            string FileName = Path.GetFileNameWithoutExtension( Filenames [ 0 ] );
            string [] Headers = Lines [ 0].Split( new char [] { ',' } );
            int XAxisColumnIndex = -1;
            int YAxisColumnIndex = -1;
            for( int Column = 0; Column < Headers.Length; Column++ ) {
                if( Headers [ Column ] == textBoxXAxisColumnHeader.Text ) { XAxisColumnIndex = Column; }
                if( Headers [ Column ] == textBoxYAxisColumnHeader.Text ) { YAxisColumnIndex = Column; }
                if( ( XAxisColumnIndex != -1 ) && ( YAxisColumnIndex != -1 ) ) {
                    break;
                }
            }
            if( ( XAxisColumnIndex != -1 ) == false ) {
                MessageBox.Show( "There is not " + textBoxXAxisColumnHeader.Text + " column header");
                return;
            }
            if( ( YAxisColumnIndex != -1 ) == false ) {
                MessageBox.Show( "There is not " + textBoxYAxisColumnHeader.Text + " column header" );
                return;
            }
            
            for( int Line = 1; Line < Lines.Length; Line++ ) {
                string [] Words = Lines [ Line ].Split( new char [] { ',' } );
                if( Words [ YAxisColumnIndex ] == "0" ) { continue; }
                XData.Add( double.Parse( Words [ XAxisColumnIndex ] ) );
                YData.Add( double.Parse( Words [ YAxisColumnIndex ] ) );
            }

            chartError.Series [ 0 ].Name = string.Empty;//SeriesName;
            chartError.Series [ 0 ].Points.Clear();
            double XMin;
            double XMax;
            double YMin;
            double YMax;

            switch( (EPlotType) Enum.Parse( typeof( EPlotType), comboBoxPlotType.Text) ){
                case EPlotType.ErrorVsNeutralMass:
                    chartError.Series [ 0 ].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point;
                    chartError.ChartAreas [ 0 ].AxisX.Title = "Neutral mass, Da";
                    chartError.ChartAreas [ 0 ].AxisY.Title = "Error, ppm";                    
                    YMin = YData [ 0];
                    YMax = YMin;
                    for( int Point = 0; Point < XData.Count;  Point++ ) {
                        chartError.Series [ 0 ].Points.AddXY( XData [ Point ], YData [ Point ] );
                        if( YMin > YData [ Point ] ) { YMin = YData [ Point ]; }
                        if( YMax < YData [ Point ] ) { YMax = YData [ Point ]; }
                    }
                    XMin = XData [ 0 ];
                    XMax = XData [ XData.Count - 1 ];
                    break;
                case EPlotType.ErrorVs:
                    chartError.Series [ 0 ].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Column;
                    chartError.ChartAreas [ 0 ].AxisX.Title = "Error, ppm";
                    chartError.ChartAreas [ 0 ].AxisY.Title = "Counts";
                    int BinCount = (int) Math.Ceiling( Math.Sqrt( XData.Count ) );
                    XMin = YData[ 0];
                    XMax = XMin;
                    for( int Index = 1; Index < YData.Count; Index++ ) {
                        if( XMin > YData [ Index ] ) { XMin = YData [ Index ]; }
                        if( XMax < YData [ Index ] ) { XMax = YData [ Index ]; }
                    }
                    double BinSize = ( XMax - XMin ) / BinCount;
                    int [] Bins = new int [ BinCount];
                    YMin = 0;
                    YMax = 0;
                    foreach( double Y in YData ) {
                        int BinIndex = (int) Math.Floor( ( Y - XMin ) / BinSize );
                        if( BinIndex >= BinCount ) { BinIndex = BinCount - 1; }
                        Bins [ BinIndex ]++;
                        if( YMax < Bins [ BinIndex ] ) { YMax = Bins [ BinIndex ]; }
                    }                 
                    for( int Point = 0; Point < Bins.Length; Point++ ) {
                        double XValue = XMin + BinSize * ( Point + 0.5 );
                        chartError.Series [ 0 ].Points.AddXY( XValue, Bins [ Point ] );
                    }
                    break;
                default:
                    return;
            }
            chartError.ChartAreas [ 0 ].AxisX.Interval = ( XMax - XMin ) / 5;
            chartError.ChartAreas [ 0 ].AxisY.Interval = ( YMax - YMin ) / 5;
            chartError.ChartAreas [ 0 ].AxisX.LabelStyle.Format = "0.#e-0";
            chartError.ChartAreas [ 0 ].AxisY.LabelStyle.Format = "0.#e-0";
        }

        //DB tools tab
        private void numericUpDownMass_ValueChanged( object sender, EventArgs e ) {
            //textBoxResult.Text = oCCia.Ipa.ChargedMassFormula_Descriptive;
            if( numericDBUpDownMass.Value < 0 ) { return; }
            tableLayoutPanelDBPeaks.SuspendLayout();
            tableLayoutPanelDBPeaks.Enabled = true;
            double Mass = (double) numericDBUpDownMass.Value;
            double NeutralMass = oCCia.Ipa.GetNeutralMass( Mass );
            textBoxDBNeutralMass.Text = NeutralMass.ToString();
            double Error = oCCia.PpmToError( NeutralMass, oCCia.GetFormulaPPMTolerance() );
            textBoxDBNeutralMassPlusError.Text = ( NeutralMass + Error ).ToString();
            textBoxDBNeutralMassMinusError.Text = ( NeutralMass - Error ).ToString();
            int LowerIndex, UpperIndex;
            int Records;
            if( oCCia.GetDBLimitIndexes( NeutralMass, out LowerIndex, out UpperIndex ) == false ) {
                Records = 0;
            } else {
                Records = UpperIndex - LowerIndex + 1;
            }
            int Rows = Records + 2;//+ Head + Last Row without Controls
            textBoxDBRecordsInErrorRange.Text = Records.ToString();
            if( tableLayoutPanelDBPeaks.RowStyles.Count > Rows) {
                for( int Row = tableLayoutPanelDBPeaks.RowCount - 1; Row >= Rows; Row-- ) {
                    for( int iColumn = 0; iColumn < tableLayoutPanelDBPeaks.ColumnCount; iColumn++ ) {
                        tableLayoutPanelDBPeaks.Controls.RemoveAt( tableLayoutPanelDBPeaks.Controls.Count - 1);
                    }
                    tableLayoutPanelDBPeaks.RowStyles.RemoveAt( Row);
                }
                tableLayoutPanelDBPeaks.RowCount = Rows;
            } else if( tableLayoutPanelDBPeaks.RowStyles.Count < Rows ) {
                tableLayoutPanelDBPeaks.RowCount = Rows;
                for( int Row = tableLayoutPanelDBPeaks.RowStyles.Count - 1; Row < Rows - 1; Row++ ) {//"Count-1" due Last Row without Controls
                    tableLayoutPanelDBPeaks.RowStyles.Add( new System.Windows.Forms.RowStyle( SizeType.Absolute, ( new System.Windows.Forms.TextBox() ).Height + 2 * ( new System.Windows.Forms.TextBox() ).Margin.Top ) );
                    for( int iColumn = 0; iColumn < tableLayoutPanelDBPeaks.ColumnCount; iColumn++ ) {
                        System.Windows.Forms.TextBox oTextBox = new System.Windows.Forms.TextBox();
                        oTextBox.Anchor = AnchorStyles.None;
                        oTextBox.ReadOnly = true;
                        oTextBox.AutoSize = true;
                        oTextBox.TextAlign = HorizontalAlignment.Center;
                        tableLayoutPanelDBPeaks.Controls.Add( oTextBox, iColumn, Row );
                    }
                }
            }
            for( int Row = 1; Row < Rows - 1; Row++ ) {
                int DBIndex = LowerIndex + Row - 1;
                tableLayoutPanelDBPeaks.GetControlFromPosition( 0, Row ).Text = DBIndex.ToString();
                tableLayoutPanelDBPeaks.GetControlFromPosition( 1, Row ).Text = oCCia.GetDBMass( DBIndex ).ToString();
                tableLayoutPanelDBPeaks.GetControlFromPosition( 2, Row ).Text = oCCia.GetDBFormulaName( DBIndex );
                tableLayoutPanelDBPeaks.GetControlFromPosition( 3, Row ).Text = oCCia.SignedMassErrorPPM( NeutralMass, oCCia.GetDBMass( DBIndex ) ).ToString( "E" );
            }
            tableLayoutPanelDBPeaks.ResumeLayout();
        }
        private void textBoxDBDropFiles_DragEnter( object sender, DragEventArgs e ) {
            if( e.Data.GetDataPresent( DataFormats.FileDrop ) == true ) {
                e.Effect = DragDropEffects.Copy;
            }
        }
        private void textBoxDBDropFiles_DragDrop( object sender, DragEventArgs e ) {
            try {
                if( oCCia.GetDBFilenames().Length == 0 ) { throw new Exception( "Drop DB file." ); }
                string [] Filenames = ( string [] ) e.Data.GetData( DataFormats.FileDrop );
                //???oCCia.ReadFiles( Filenames);
                //oCCia.ReportFormulas();
            } catch {
            }
        }

        //File convertor tab
        string [] InputFileTextFormats = { ".txt", ".csv", ".xls", ".xlsx" };
        string [] DBActionMenu = {
            "One ASCII -> one binary",
            "Many ASCIIs -> one binary",
            "Binary -> CSV"};
        private void comboBoxDBAction_SelectedIndexChanged( object sender, EventArgs e ) {
            if( comboBoxDBAction.Text == DBActionMenu [ 0 ] ) {
                checkBoxDBCalculateMassFromFormula.Enabled = true;
                checkBoxDBSortByMass.Enabled = true;
                checkBoxDBMassRangePerCsvFile.Enabled = false;
            } else if( comboBoxDBAction.Text == DBActionMenu [ 1 ] ) {
                checkBoxDBCalculateMassFromFormula.Enabled = true;
                checkBoxDBSortByMass.Enabled = true;
                checkBoxDBMassRangePerCsvFile.Enabled = false;
            } else if( comboBoxDBAction.Text == DBActionMenu [ 2 ] ) {
                checkBoxDBCalculateMassFromFormula.Enabled = false;
                checkBoxDBSortByMass.Enabled = false;
                checkBoxDBMassRangePerCsvFile.Enabled = true;
            }
        }
        private void checkBoxDBMassRangePerCsvFile_CheckedChanged( object sender, EventArgs e ) {
            numericUpDownDBMassRange.Enabled = checkBoxDBMassRangePerCsvFile.Enabled;
        }
        private void textBoxConvertDBs_DragEnter( object sender, DragEventArgs e ) {
            if( e.Data.GetDataPresent( DataFormats.FileDrop ) == true ) {
                e.Effect = DragDropEffects.Copy;
            }
        }
        private void textBoxConvertDBs_DragDrop( object sender, DragEventArgs e ) {
            string [] Filenames = ( string [] ) e.Data.GetData( DataFormats.FileDrop );
            oCCia.SetDBCalculateMassFromFormula( checkBoxDBCalculateMassFromFormula.Checked);
            oCCia.SetDBSort( checkBoxDBSortByMass.Checked);
            oCCia.SetDBMassRangePerCsvFile( checkBoxDBMassRangePerCsvFile.Checked);
            oCCia.SetDBMassRange( ( double) numericUpDownDBMassRange.Value);
            if( comboBoxDBAction.Text == DBActionMenu [ 0 ] ) {
                foreach( string Filename in Filenames ) {
                    if( InputFileTextFormats.Contains( Path.GetExtension( Filename ) ) == true ) {
                        oCCia.DBConvertAsciiToBin( Filename );
                    } else {
                        MessageBox.Show( "Extention of file (" + Path.GetFileName( Filename ) + ") is not ASCII" );
                    }
                }
            } else if( comboBoxDBAction.Text == DBActionMenu [ 1 ] ) {
                bool ErrorTypeExtention = false;
                foreach( string Filename in Filenames ) {
                    if( InputFileTextFormats.Contains( Path.GetExtension( Filename ) ) != true ) {                        
                        MessageBox.Show( "Extention of file (" + Path.GetFileName( Filename ) + ") is not ASCII" );
                        ErrorTypeExtention = true;
                    }
                }
                if( ErrorTypeExtention == false ) {
                    oCCia.DBConvertAsciisToBin( Filenames );
                }
            } else if( comboBoxDBAction.Text == DBActionMenu [ 2 ] ) {
                foreach( string Filename in Filenames ) {
                    if( Path.GetExtension( Filename ) == ".bin" ) {
                        oCCia.DBConvertBinToCsv( Filename);
                    } else {
                        MessageBox.Show( "Extention of file (" + Path.GetFileName( Filename ) + ") is not bin" );
                    }
                }
            }
        }
        private void textBoxCompareReports_DragEnter( object sender, DragEventArgs e ) {
            if( e.Data.GetDataPresent( DataFormats.FileDrop ) == true ) {
                e.Effect = DragDropEffects.Copy;
            }
        }
        private void textBoxCompareReports_DragDrop( object sender, DragEventArgs e ) {
            string [] Filenames = ( string [] ) e.Data.GetData( DataFormats.FileDrop );
            CompareReports( Filenames );
        }
        void CompareReports( string [] Filenames ) {
            int TotalSamples = 0;
            List<string> SampleNames = new List<string>();
            foreach( string Filename in Filenames ) {
                string [] ColumnHeaders = File.ReadAllLines( Filename ) [ 0 ].Split( oCCia.WordSeparators);
                int Samples = ColumnHeaders.Length - 10;
                TotalSamples = TotalSamples + Samples;
                for( int Sample = 0; Sample < Samples; Sample++ ) {
                    SampleNames.Add( ColumnHeaders [ 10 + Sample ] );
                }
            }
            SortedDictionary<double, ReportData> FormulaDict = new SortedDictionary<double, ReportData>();
            int StartSample = 0;
            foreach( string Filename in Filenames ) {
                string [] LineString = File.ReadAllLines( Filename );
                string [] ColumnHeaders = File.ReadAllLines( Filename ) [ 0 ].Split( oCCia.WordSeparators);
                int Samples = ColumnHeaders.Length - 10;
                for( int Line = 1; Line < LineString.Length; Line++ ) {
                    string [] LineParts = LineString[ Line].Split( oCCia.WordSeparators);
                    short [] Formula = new short[ CCia.Elements];
                    for( int Element = 0; Element < CCia.Elements; Element++ ) {
                        Formula [ Element ] = Int16.Parse( LineParts [ 2 + Element ] );
                    }
                    if( oCCia.IsFormula( Formula ) == false ) {
                        continue;
                    }
                    double NeutralMass = oCCia.FormulaToNeutralMass( Formula );
                    double [] Abundances;
                    if( FormulaDict.ContainsKey( NeutralMass ) == false ) {
                        ReportData Data = new ReportData();
                        Data.Formula = ( short [] ) Formula.Clone();
                        Data.Abundances = new double [ TotalSamples ];
                        FormulaDict.Add( NeutralMass, Data );
                        Abundances = Data.Abundances;
                    } else {
                        Abundances = FormulaDict [ NeutralMass ].Abundances;
                    }
                    for( int Sample = 0; Sample < Samples; Sample++ ) {
                        Abundances [ StartSample + Sample ] = double.Parse( LineParts [ 10 + Sample ] );
                    }                    
                }
                StartSample = StartSample + Samples;
            }
            string Delimiter = ",";
            string HeaderLine = "NeutralMass" + Delimiter + "Mass";
            foreach( string Element in Enum.GetNames( typeof( CCia.EElemNumber ) ) ) {
                HeaderLine = HeaderLine + Delimiter + Element;
            }
            foreach( string SampleName in SampleNames ) {
                HeaderLine = HeaderLine + Delimiter + SampleName;
            }
            StreamWriter oStreamWriter = new StreamWriter( Path.GetDirectoryName( Filenames [ 0 ] ) + "\\Comparision.csv" );            
            oStreamWriter.WriteLine( HeaderLine );

            foreach( KeyValuePair<double, ReportData> KVP in FormulaDict ) {
                string Line = KVP.Key.ToString() + Delimiter + oCCia.Ipa.GetChargedMass( KVP.Key ).ToString();
                foreach( short Count in KVP.Value.Formula ) {
                    Line = Line + Delimiter + Count.ToString();
                }
                foreach( double Abundance in KVP.Value.Abundances ) {
                    Line = Line + Delimiter + Abundance.ToString();
                }
                oStreamWriter.WriteLine( Line );
            }
            oStreamWriter.Close();
        }
        private void textBoxConverFiles_DragEnter( object sender, DragEventArgs e ) {
            if( e.Data.GetDataPresent( DataFormats.FileDrop ) == true ) {
                e.Effect = DragDropEffects.Copy;
            }
        }
        private void textBoxConverFiles_DragDrop( object sender, DragEventArgs e ) {
            string [] Filenames = ( string [] ) e.Data.GetData( DataFormats.FileDrop );
            foreach( string Filename in Filenames ) {
                XmlToXls( Filename );
            }
        }
        private void XmlToXls( string Filename ) {
            XmlDocument XmlDoc = new XmlDocument();
            XmlDoc.Load( Filename);
            //check Bruker instrument
            XmlNodeList Nodes = XmlDoc.GetElementsByTagName( "fileinfo" );
            if( Nodes.Count != 1 ) { return; }
            if( Nodes [ 0 ].Attributes [ "appname" ].Value != "Bruker Compass DataAnalysis" ) { return; }
            //read peaks
            XmlNodeList MsPeakNodes = XmlDoc.GetElementsByTagName( "ms_peaks" );
            if( MsPeakNodes.Count != 1 ) { return; }
            XmlNode MsPeakNode = MsPeakNodes [ 0 ];
            int Peaks = MsPeakNode.ChildNodes.Count;
            int [] myLengthsArray = new int [ 2 ] { Peaks, 5 };
            int [] myBoundsArray = new int [ 2 ] { 1, 1 };
            Array myArray = Array.CreateInstance( typeof( double ), myLengthsArray, myBoundsArray );
            double Maxi = 0;
            for( int Peak = 1; Peak <= Peaks; Peak++ ) {
                //<pk res="930674.5" algo="FTMS" fwhm="0.000218" a="102.53" sn="7.15" i="646225.1" mz="203.034719"/>
                XmlAttributeCollection Attributes = MsPeakNode.ChildNodes [ Peak - 1].Attributes;
                myArray.SetValue( double.Parse( Attributes [ "mz" ].Value), Peak, 1 );
                myArray.SetValue( double.Parse( Attributes [ "i" ].Value), Peak, 2 );
                myArray.SetValue( double.Parse( Attributes [ "sn" ].Value), Peak, 3 );
                myArray.SetValue( double.Parse( Attributes [ "res" ].Value), Peak, 4 );
                double Currenti = (double) myArray.GetValue( Peak, 2 );
                if( Maxi < Currenti ) { Maxi = Currenti; }
                
            }
            XmlDoc = null;                       
            for ( int Peak = 1; Peak <= Peaks; Peak++ ){
                double rel_ab = ( (double) myArray.GetValue( Peak, 2 ) ) / Maxi;
                myArray.SetValue( rel_ab, Peak, 5 );
            }

            Microsoft.Office.Interop.Excel.Application MyApp = new Microsoft.Office.Interop.Excel.Application();
            MyApp.Visible = false;
            Microsoft.Office.Interop.Excel.Workbook MyBook = MyApp.Workbooks.Add( 1);
            // Microsoft.Office.Interop.Excel.Worksheet MySheet = MyBook.Sheets [ 1 ];
            Microsoft.Office.Interop.Excel.Worksheet MySheet = (Worksheet)MyBook.Worksheets[1];
            MySheet.Cells [ 1, 1] = "mz";
            MySheet.Cells [ 1, 2] = "i";
            MySheet.Cells [ 1, 3] = "sn";
            MySheet.Cells [ 1, 4] = "res";
            MySheet.Cells [ 1, 5] = "rel_ab";

            Microsoft.Office.Interop.Excel.Range MyRange = MySheet.get_Range( "A2", "E" + Peaks.ToString() ) ;
            MyRange.Value = myArray;
            MyBook.SaveAs( Filename.Substring( 0, Filename.Length - Path.GetExtension( Filename).Length ) + ".xls" );
            string sss = Path.GetFileNameWithoutExtension( Filename );

            oCCia.CleanComObject( MyRange );
            MyRange = null;
            oCCia.CleanComObject( MySheet );
            MySheet = null;
            MyBook.Close( null, null, null );
            oCCia.CleanComObject( MyBook );
            MyBook = null;
            MyApp.Quit();
            oCCia.CleanComObject( MyApp );
            MyApp = null;
            GC.Collect();            
        }

        //filter check tab
        private void buttonFilterCheck_Click( object sender, EventArgs e ) {
            try {
                System.Data.DataTable UserDefinedFilter = new System.Data.DataTable();
                UserDefinedFilter.Columns.Add( "Mass", typeof( double ) );
                foreach( string Name in Enum.GetNames( typeof( CCia.EElemNumber ) ) ) {
                    UserDefinedFilter.Columns.Add( Name, typeof( short ) );
                }
                UserDefinedFilter.Columns.Add( "UserDefinedFilter", typeof( bool ), textBoxFilter.Text );
                UserDefinedFilter.Rows.Add( UserDefinedFilter.NewRow() );

                double Mass = ( ( int) numericUpDownCAtoms.Value) * CCia.C
                        + ( ( int) numericUpDownHAtoms.Value) * CCia.H
                        + ( ( int) numericUpDownOAtoms.Value) * CCia.O
                        + ( ( int) numericUpDownNAtoms.Value) * CCia.N
                        + ( ( int) numericUpDownSAtoms.Value) * CCia.S
                        + ( ( int) numericUpDownPAtoms.Value) * CCia.P
                        + ( ( int) numericUpDownNaAtoms.Value) * CCia.Na;
                textBoxNeutralMass.Text = Mass.ToString();

                UserDefinedFilter.Rows [ 0 ] [ "Mass" ] = Mass;
                UserDefinedFilter.Rows [ 0 ] [ CCia.EElemNumber.C.ToString() ] = numericUpDownCAtoms.Value;
                UserDefinedFilter.Rows [ 0 ] [ CCia.EElemNumber.H.ToString() ] = numericUpDownHAtoms.Value;
                UserDefinedFilter.Rows [ 0 ] [ CCia.EElemNumber.O.ToString() ] = numericUpDownOAtoms.Value;
                UserDefinedFilter.Rows [ 0 ] [ CCia.EElemNumber.N.ToString() ] = numericUpDownNAtoms.Value;
                UserDefinedFilter.Rows [ 0 ] [ CCia.EElemNumber.S.ToString() ] = numericUpDownSAtoms.Value;
                UserDefinedFilter.Rows [ 0 ] [ CCia.EElemNumber.P.ToString() ] = numericUpDownPAtoms.Value;
                UserDefinedFilter.Rows [ 0 ] [ CCia.EElemNumber.Na.ToString() ] = numericUpDownNaAtoms.Value;

                textBoxFilterResult.Text = ( ( bool ) UserDefinedFilter.Rows [ 0 ] [ "UserDefinedFilter" ] ).ToString();
            } catch( Exception ex ) {
                textBoxFilterResult.Text = "Error: " + ex.Message;
            }
        }

        //Save/restore parameters
        private void buttonSaveParameters_Click( object sender, EventArgs e ) {
            SaveFileDialog OSD = new SaveFileDialog();
            OSD.Title = "Save parameters";
            OSD.InitialDirectory = System.IO.Path.GetDirectoryName( System.Reflection.Assembly.GetEntryAssembly().Location );
            OSD.Filter = "XML Files (.xml)|*.xml|All Files (*.*)|*.*";
            OSD.FilterIndex = 1;
            if( OSD.ShowDialog() == System.Windows.Forms.DialogResult.OK ) {
                SaveParameters( OSD.FileName);
            }
        }
        private void SaveParameters( string Filename ) {
            try {
                XmlWriter xmlWriter = XmlWriter.Create( Filename );

                xmlWriter.WriteStartDocument();
                xmlWriter.WriteStartElement( "DefaultParameters" );
                xmlWriter.WriteStartElement( "InputFilesTab" );
                xmlWriter.WriteStartElement( "Adduct" );
                xmlWriter.WriteString( textBoxAdduct.Text );
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement( "Ionization" );
                xmlWriter.WriteString( comboBoxIonization.Text );
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement( "Charge" );
                xmlWriter.WriteString( numericUpDownCharge.Value.ToString() );
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement( "Calibration" );
                xmlWriter.WriteStartElement( "Regression" );
                xmlWriter.WriteString( comboBoxCalRegressionModel.Text );
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement( "RelFactor" );
                xmlWriter.WriteString( numericUpDownCalRelFactor.Value.ToString() );
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement( "StartTolerance" );
                xmlWriter.WriteString( numericUpDownCalStartTolerance.Value.ToString() );
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement( "EndTolerance" );
                xmlWriter.WriteString( numericUpDownCalEndTolerance.Value.ToString() );
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement( "PeakFilters" );
                xmlWriter.WriteStartElement( "MinSToN" );
                xmlWriter.WriteString( numericUpDownCalMinSN.Value.ToString() );
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement( "MinRelAbun" );
                xmlWriter.WriteString( numericUpDownCalMinRelAbun.Value.ToString() );
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement( "MaxRelAbun" );
                xmlWriter.WriteString( numericUpDownCalMaxRelAbun.Value.ToString() );
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();
                
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();//InputFilesTab
                xmlWriter.WriteStartElement( "CiaTab" );
                xmlWriter.WriteStartElement( "UseAlignment" );
                xmlWriter.WriteString( checkBoxAlignment.Checked.ToString() );
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement( "AlignmentTolerance" );
                xmlWriter.WriteString( numericUpDownAlignmentTolerance.Value.ToString() );
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement( "FormulaTolerance" );
                xmlWriter.WriteString( numericUpDownFormulaTolerance.Value.ToString() );
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement( "DbMassLimit" );
                xmlWriter.WriteString( numericUpDownDBMassLimit.Value.ToString() );
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement( "FormulaScore" );
                xmlWriter.WriteString( ( ( CCia.EFormulaScore ) comboBoxFormulaScore.SelectedIndex ).ToString() );
                xmlWriter.WriteEndElement();
                //xmlWriter.WriteStartElement( "UseCiaFormulaScore" );
                //xmlWriter.WriteString( checkBoxUseCIAFormulaScore.Checked.ToString() );
                //xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement( "UseKendrick" );
                xmlWriter.WriteString( oCiaAdvancedForm.checkBoxCIAUseKendrick.Checked.ToString() );
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement( "UseC13" );
                xmlWriter.WriteString( oCiaAdvancedForm.checkBoxCIAUseC13.Checked.ToString() );
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement( "C13Tolerance" );
                xmlWriter.WriteString( oCiaAdvancedForm.numericUpDownCIAC13Tolerance.Value.ToString() );
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement( "UseFormulaFilters" );
                xmlWriter.WriteString( checkBoxUseFormulaFilters.Checked.ToString() );
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement( "ElementalCounts" );
                xmlWriter.WriteString( oCiaAdvancedForm.checkBoxGoldenRule1.Checked.ToString() );
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement( "ValenceRules" );
                xmlWriter.WriteString( oCiaAdvancedForm.checkBoxGoldenRule2.Checked.ToString() );
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement( "ElementalRatios" );
                xmlWriter.WriteString( oCiaAdvancedForm.checkBoxGoldenRule3.Checked.ToString() );
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement( "HeteroatomCounts" );
                xmlWriter.WriteString( oCiaAdvancedForm.checkBoxGoldenRule4.Checked.ToString() );
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement( "PositiveAtoms" );
                xmlWriter.WriteString( oCiaAdvancedForm.checkBoxGoldenRule5.Checked.ToString() );
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement( "IntegerDBE" );
                xmlWriter.WriteString( oCiaAdvancedForm.checkBoxGoldenRule6.Checked.ToString() );
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement( "SpecialFilter" );
                string ccc = comboBoxSpecialFilters.Text.Split( new char[]{ ':'})[ 0];
                xmlWriter.WriteString( comboBoxSpecialFilters.Text.Split( new char[]{ ':'})[ 0] );
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement( "UserDefinedFilter" );
                xmlWriter.WriteString( textBoxUserDefinedFilter.Text);
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement( "UseRelationship" );
                xmlWriter.WriteString( checkBoxUseRelation.Checked.ToString() );
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement( "MaxRelationshipGaps" );
                xmlWriter.WriteString( numericUpDownMaxRelationshipGaps.Value.ToString() );
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement( "RelationError" );
                xmlWriter.WriteString( numericUpDownRelationErrorValue.Value.ToString() );
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement( "FormulaBuildingBlocks" );
                xmlWriter.WriteStartElement( "CH2" );
                xmlWriter.WriteString( checkedListBoxRelations.GetItemChecked( 0 ).ToString() );
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement( "CH4O-1" );
                xmlWriter.WriteString( checkedListBoxRelations.GetItemChecked( 1 ).ToString() );
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement( "H2" );
                xmlWriter.WriteString( checkedListBoxRelations.GetItemChecked( 2 ).ToString() );
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement( "C2H40" );
                xmlWriter.WriteString( checkedListBoxRelations.GetItemChecked( 3 ).ToString() );
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement( "CO2" );
                xmlWriter.WriteString( checkedListBoxRelations.GetItemChecked( 4 ).ToString() );
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement( "C2H20" );
                xmlWriter.WriteString( checkedListBoxRelations.GetItemChecked( 5 ).ToString() );
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement( "O" );
                xmlWriter.WriteString( checkedListBoxRelations.GetItemChecked( 6 ).ToString() );
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement( "CH" );
                xmlWriter.WriteString( checkedListBoxRelations.GetItemChecked( 7 ).ToString() );
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement( "HN" );
                xmlWriter.WriteString( checkedListBoxRelations.GetItemChecked( 8 ).ToString() );
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement( "O3P" );
                xmlWriter.WriteString( checkedListBoxRelations.GetItemChecked( 9 ).ToString() );
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement( "Output" );
                xmlWriter.WriteStartElement( "IndividualFileReports" );
                xmlWriter.WriteString( oCiaAdvancedForm.checkBoxIndividualFileReport.Checked.ToString() );
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement( "LogReports" );
                xmlWriter.WriteString( oCiaAdvancedForm.checkBoxLogReport.Checked.ToString() );
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement( "Delimiters" );
                xmlWriter.WriteString( oCiaAdvancedForm.comboBoxOutputFileDelimiter.Text );
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement( "Error" );
                xmlWriter.WriteString( ( ( CCia.EErrorType ) oCiaAdvancedForm.comboBoxErrorType.SelectedIndex ).ToString() );
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();
                //end CiaTab
                xmlWriter.WriteStartElement( "IpaTab" );
                xmlWriter.WriteStartElement( "MassTol" );
                xmlWriter.WriteString( numericUpDownIpaMassTolerance.Value.ToString() );
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement( "MajorPeaksMinSToN" );
                xmlWriter.WriteString( numericUpDownIpaMajorPeaksMinSN.Value.ToString() );
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement( "MinorPeaksMinSToN" );
                xmlWriter.WriteString( numericUpDownIpaMinorPeaksMinSN.Value.ToString() );
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement( "MathedPeakReport" );
                xmlWriter.WriteString( checkBoxIpaMatchedPeakReport.Checked.ToString() );
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement( "MinPeakProbabilityToScore" );
                xmlWriter.WriteString( numericUpDownIpaMinPeakProbabilityToScore.Value.ToString() );
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement( "DbFilter" );
                xmlWriter.WriteString( textBoxIpaFilter.Text );
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();//IpaTab
                xmlWriter.WriteEndDocument();
                xmlWriter.Close();
            } catch( Exception ex ) {
                MessageBox.Show( ex.Message);
            }                
        }
        private void buttonLoadParameters_Click( object sender, EventArgs e ) {
            OpenFileDialog OFD = new OpenFileDialog();
            OFD.Title = "Load parameters";
            OFD.InitialDirectory = System.IO.Path.GetDirectoryName( System.Reflection.Assembly.GetEntryAssembly().Location );
            OFD.Filter = "XML Files (.xml)|*.xml|All Files (*.*)|*.*";
            OFD.FilterIndex = 1;
            OFD.Multiselect = false;
            if( OFD.ShowDialog() == System.Windows.Forms.DialogResult.OK){
                LoadParameters( OFD.FileName );
            }
        }
        private void LoadParameters( string Filename){
            try {
                XmlDocument XmlDoc = new XmlDocument();
                XmlDoc.Load( Filename );
                XmlNode XmlDocRoot = XmlDoc.DocumentElement;
                this.SuspendLayout();
                XmlNode XmlNode = XmlDocRoot.SelectSingleNode( "//DefaultParameters/InputFilesTab/Adduct" );
                if( XmlNode != null ) {
                    oCCia.Ipa.Adduct = XmlNode.InnerText;
                    textBoxAdduct.Text = oCCia.Ipa.Adduct;
                }
                XmlNode = XmlDocRoot.SelectSingleNode( "//DefaultParameters/InputFilesTab/Ionization" );
                if( XmlNode != null ) {
                    oCCia.Ipa.Ionization = ( TestFSDBSearch.TotalSupport.IonizationMethod) Enum.Parse( typeof( TestFSDBSearch.TotalSupport.IonizationMethod ), XmlNode.InnerText );
                    comboBoxIonization.Text = oCCia.Ipa.Ionization.ToString();
                }
                XmlNode = XmlDocRoot.SelectSingleNode( "//DefaultParameters/InputFilesTab/Charge" );
                if( XmlNode != null ) {
                    oCCia.Ipa.CS = int.Parse( XmlNode.InnerText );
                    numericUpDownCharge.Value = oCCia.Ipa.CS;
                }

                XmlNode = XmlDocRoot.SelectSingleNode( "//DefaultParameters/InputFilesTab/Calibration/Regression" );
                if( XmlNode != null ) {
                    oCCia.oTotalCalibration.ttl_cal_regression = ( TestFSDBSearch.TotalCalibration.ttlRegressionType ) Enum.Parse( typeof( TestFSDBSearch.TotalCalibration.ttlRegressionType ), XmlNode.InnerText );
                    comboBoxCalRegressionModel.Text = oCCia.oTotalCalibration.ttl_cal_regression.ToString();
                }
                XmlNode = XmlDocRoot.SelectSingleNode( "//DefaultParameters/InputFilesTab/Calibration/RelFactor" );
                if( XmlNode != null ) {
                    oCCia.oTotalCalibration.ttl_cal_rf = double.Parse( XmlNode.InnerText );
                    numericUpDownCalRelFactor.Value = ( decimal ) oCCia.oTotalCalibration.ttl_cal_rf;
                }
                XmlNode = XmlDocRoot.SelectSingleNode( "//DefaultParameters/InputFilesTab/Calibration/StartTolerance" );
                if( XmlNode != null ) {
                    oCCia.oTotalCalibration.ttl_cal_start_ppm = double.Parse( XmlNode.InnerText );
                    numericUpDownCalStartTolerance.Value = ( decimal ) oCCia.oTotalCalibration.ttl_cal_start_ppm;
                }
                XmlNode = XmlDocRoot.SelectSingleNode( "//DefaultParameters/InputFilesTab/Calibration/EndTolerance" );
                if( XmlNode != null ) {
                    oCCia.oTotalCalibration.ttl_cal_target_ppm = double.Parse( XmlNode.InnerText );
                    numericUpDownCalEndTolerance.Value = ( decimal ) oCCia.oTotalCalibration.ttl_cal_target_ppm;
                }                
                XmlNode = XmlDocRoot.SelectSingleNode( "//DefaultParameters/InputFilesTab/Calibration/PeakFilters/MinSToN" );
                if( XmlNode != null ) {
                    oCCia.oTotalCalibration.ttl_cal_min_sn = double.Parse( XmlNode.InnerText );
                    numericUpDownCalMinSN.Value = ( decimal ) oCCia.oTotalCalibration.ttl_cal_min_sn;
                } 
                XmlNode = XmlDocRoot.SelectSingleNode( "//DefaultParameters/InputFilesTab/Calibration/PeakFilters/MinRelAbun" );
                if( XmlNode != null ) {
                    oCCia.oTotalCalibration.ttl_cal_min_abu_pct = double.Parse( XmlNode.InnerText );
                    numericUpDownCalMinRelAbun.Value = ( decimal ) oCCia.oTotalCalibration.ttl_cal_min_abu_pct;
                } 
                XmlNode = XmlDocRoot.SelectSingleNode( "//DefaultParameters/InputFilesTab/Calibration/PeakFilters/MaxRelAbun" );
                if( XmlNode != null ) {
                    oCCia.oTotalCalibration.ttl_cal_max_abu_pct = double.Parse( XmlNode.InnerText );
                    numericUpDownCalMaxRelAbun.Value = ( decimal ) oCCia.oTotalCalibration.ttl_cal_max_abu_pct;
                }

                XmlNode = XmlDocRoot.SelectSingleNode( "//DefaultParameters/CiaTab/UseAlignment" );
                if( XmlNode != null ) {
                    oCCia.SetAlignment( bool.Parse( XmlNode.InnerText) );
                    checkBoxAlignment.Checked = oCCia.GetAlignment();
                }
                XmlNode = XmlDocRoot.SelectSingleNode( "//DefaultParameters/CiaTab/AlignmentTolerance" );
                if( XmlNode != null ) {
                    oCCia.SetAlignmentPpmTolerance( double.Parse( XmlNode.InnerText ) );
                    numericUpDownAlignmentTolerance.Value = ( decimal ) oCCia.GetAlignmentPpmTolerance();
                }
                XmlNode = XmlDocRoot.SelectSingleNode( "//DefaultParameters/CiaTab/FormulaTolerance" );
                if( XmlNode != null ) {
                    oCCia.SetFormulaPPMTolerance( double.Parse( XmlNode.InnerText ) );
                    numericUpDownFormulaTolerance.Value = ( decimal ) oCCia.GetFormulaPPMTolerance();
                }
                XmlNode = XmlDocRoot.SelectSingleNode( "//DefaultParameters/CiaTab/DbMassLimit" );
                if( XmlNode != null ) {
                    oCCia.SetMassLimit( double.Parse( XmlNode.InnerText ) );
                    numericUpDownDBMassLimit.Value = ( decimal ) oCCia.GetMassLimit();
                }
                XmlNode = XmlDocRoot.SelectSingleNode( "//DefaultParameters/CiaTab/FormulaScore" );
                if( XmlNode != null ) {
                    CCia.EFormulaScore ttt = ( CCia.EFormulaScore ) Enum.Parse( typeof( CCia.EFormulaScore ), XmlNode.InnerText );
                    oCCia.SetFormulaScore( ttt);
                    comboBoxFormulaScore.SelectedIndex = ( int) ttt;
                }
                //XmlNode = XmlDocRoot.SelectSingleNode( "//DefaultParameters/CiaTab/UseCiaFormulaScore" );
                //if( XmlNode != null ) {
                //    oCCia.SetUseCIAFormulaScore( bool.Parse( XmlNode.InnerText ) );
                //    checkBoxUseCIAFormulaScore.Checked = oCCia.GetUseCIAFormulaScore();
                //}
                XmlNode = XmlDocRoot.SelectSingleNode( "//DefaultParameters/CiaTab/UseKendrick" );
                if( XmlNode != null ) {
                    oCCia.SetUseKendrick( bool.Parse( XmlNode.InnerText ) );
                    oCiaAdvancedForm.checkBoxCIAUseKendrick.Checked = oCCia.GetUseKendrick();
                }
                XmlNode = XmlDocRoot.SelectSingleNode( "//DefaultParameters/CiaTab/UseC13" );
                if( XmlNode != null ) {
                    oCCia.SetUseC13( bool.Parse( XmlNode.InnerText ) );
                    oCiaAdvancedForm.checkBoxCIAUseC13.Checked = oCCia.GetUseC13();
                }
                XmlNode = XmlDocRoot.SelectSingleNode( "//DefaultParameters/CiaTab/C13Tolerance" );
                if( XmlNode != null ) {
                    oCCia.SetC13Tolerance( double.Parse( XmlNode.InnerText ) );
                    oCiaAdvancedForm.numericUpDownCIAC13Tolerance.Value = ( decimal ) oCCia.GetC13Tolerance();
                }
                XmlNode = XmlDocRoot.SelectSingleNode( "//DefaultParameters/CiaTab/UseFormulaFilters" );
                if( XmlNode != null ) {
                    oCCia.SetUseFormulaFilter( bool.Parse( XmlNode.InnerText ) );
                    checkBoxUseFormulaFilters.Checked = oCCia.GetUseFormulaFilter();
                }
                XmlNode = XmlDocRoot.SelectSingleNode( "//DefaultParameters/CiaTab/ElementalCounts" );
                if( XmlNode != null ) {
                    oCiaAdvancedForm.checkBoxGoldenRule1.Checked = bool.Parse( XmlNode.InnerText );
                }
                XmlNode = XmlDocRoot.SelectSingleNode( "//DefaultParameters/CiaTab/ValenceRules" );
                if( XmlNode != null ) {
                    oCiaAdvancedForm.checkBoxGoldenRule2.Checked = bool.Parse( XmlNode.InnerText );
                }
                XmlNode = XmlDocRoot.SelectSingleNode( "//DefaultParameters/CiaTab/ElementalRatios" );
                if( XmlNode != null ) {
                    oCiaAdvancedForm.checkBoxGoldenRule3.Checked = bool.Parse( XmlNode.InnerText );
                }
                XmlNode = XmlDocRoot.SelectSingleNode( "//DefaultParameters/CiaTab/HeteroatomCounts" );
                if( XmlNode != null ) {
                    oCiaAdvancedForm.checkBoxGoldenRule4.Checked = bool.Parse( XmlNode.InnerText );
                }
                XmlNode = XmlDocRoot.SelectSingleNode( "//DefaultParameters/CiaTab/PositiveAtoms" );
                if( XmlNode != null ) {
                    oCiaAdvancedForm.checkBoxGoldenRule5.Checked = bool.Parse( XmlNode.InnerText );
                }
                XmlNode = XmlDocRoot.SelectSingleNode( "//DefaultParameters/CiaTab/IntegerDBE" );
                if( XmlNode != null ) {
                    oCiaAdvancedForm.checkBoxGoldenRule6.Checked = bool.Parse( XmlNode.InnerText );
                }
                XmlNode = XmlDocRoot.SelectSingleNode( "//DefaultParameters/CiaTab/SpecialFilter" );
                if( XmlNode != null ) {
                    CCia.ESpecialFilters fff = ( CCia.ESpecialFilters ) Enum.Parse( typeof( CCia.ESpecialFilters ), XmlNode.InnerText );
                    oCCia.SetSpecialFilter( fff );
                    comboBoxSpecialFilters.SelectedIndex = ( int ) fff;
                }
                XmlNode = XmlDocRoot.SelectSingleNode( "//DefaultParameters/CiaTab/UserDefinedFilter" );
                if( XmlNode != null ) {
                    oCCia.SetUserDefinedFilter( XmlNode.InnerText );
                    textBoxUserDefinedFilter.Text = XmlNode.InnerText;
                }
                XmlNode = XmlDocRoot.SelectSingleNode( "//DefaultParameters/CiaTab/UseRelationship" );
                if( XmlNode != null ) {
                    oCCia.SetUseRelation( bool.Parse( XmlNode.InnerText ) );
                    checkBoxUseRelation.Checked = oCCia.GetUseRelation();
                }
                XmlNode = XmlDocRoot.SelectSingleNode( "//DefaultParameters/CiaTab/MaxRelationshipGaps" );
                if( XmlNode != null ) {
                    oCCia.SetMaxRelationGaps( int.Parse( XmlNode.InnerText ) );
                    numericUpDownMaxRelationshipGaps.Value = oCCia.GetMaxRelationGaps();
                }
                XmlNode = XmlDocRoot.SelectSingleNode( "//DefaultParameters/CiaTab/RelationError" );
                if( XmlNode != null ) {
                    oCCia.SetRelationErrorAMU( double.Parse( XmlNode.InnerText ) );
                    numericUpDownRelationErrorValue.Value = ( decimal) oCCia.GetRelationErrorAMU();
                }
                XmlNode = XmlDocRoot.SelectSingleNode( "//DefaultParameters/CiaTab/FormulaBuildingBlocks/CH2" );
                if( XmlNode != null ) {
                    checkedListBoxRelations.SetItemChecked( 0, bool.Parse( XmlNode.InnerText ) );
                }
                XmlNode = XmlDocRoot.SelectSingleNode( "//DefaultParameters/CiaTab/FormulaBuildingBlocks/CH4O-1" );
                if( XmlNode != null ) {
                    checkedListBoxRelations.SetItemChecked( 1, bool.Parse( XmlNode.InnerText ) );
                }
                XmlNode = XmlDocRoot.SelectSingleNode( "//DefaultParameters/CiaTab/FormulaBuildingBlocks/H2" );
                if( XmlNode != null ) {
                    checkedListBoxRelations.SetItemChecked( 2, bool.Parse( XmlNode.InnerText ) );
                }
                XmlNode = XmlDocRoot.SelectSingleNode( "//DefaultParameters/CiaTab/FormulaBuildingBlocks/C2H40" );
                if( XmlNode != null ) {
                    checkedListBoxRelations.SetItemChecked( 3, bool.Parse( XmlNode.InnerText ) );
                }
                XmlNode = XmlDocRoot.SelectSingleNode( "//DefaultParameters/CiaTab/FormulaBuildingBlocks/CO2" );
                if( XmlNode != null ) {
                    checkedListBoxRelations.SetItemChecked( 4, bool.Parse( XmlNode.InnerText ) );
                }
                XmlNode = XmlDocRoot.SelectSingleNode( "//DefaultParameters/CiaTab/FormulaBuildingBlocks/C2H20" );
                if( XmlNode != null ) {
                    checkedListBoxRelations.SetItemChecked( 5, bool.Parse( XmlNode.InnerText ) );
                }
                XmlNode = XmlDocRoot.SelectSingleNode( "//DefaultParameters/CiaTab/FormulaBuildingBlocks/O" );
                if( XmlNode != null ) {
                    checkedListBoxRelations.SetItemChecked( 6, bool.Parse( XmlNode.InnerText ) );
                }
                XmlNode = XmlDocRoot.SelectSingleNode( "//DefaultParameters/CiaTab/FormulaBuildingBlocks/CH" );
                if( XmlNode != null ) {
                    checkedListBoxRelations.SetItemChecked( 7, bool.Parse( XmlNode.InnerText ) );
                }
                XmlNode = XmlDocRoot.SelectSingleNode( "//DefaultParameters/CiaTab/FormulaBuildingBlocks/HN" );
                if( XmlNode != null ) {
                    checkedListBoxRelations.SetItemChecked( 8, bool.Parse( XmlNode.InnerText ) );
                }
                XmlNode = XmlDocRoot.SelectSingleNode( "//DefaultParameters/CiaTab/FormulaBuildingBlocks/O3P" );
                if( XmlNode != null ) {
                    checkedListBoxRelations.SetItemChecked( 9, bool.Parse( XmlNode.InnerText ) );
                }
                //??? don't write into object
                XmlNode = XmlDocRoot.SelectSingleNode( "//DefaultParameters/CiaTab/Output/IndividualFileReports" );
                if( XmlNode != null ) {
                    oCCia.SetGenerateIndividualFileReports( bool.Parse( XmlNode.InnerText ) );
                    oCiaAdvancedForm.checkBoxIndividualFileReport.Checked = oCCia.GetGenerateIndividualFileReports();
                }
                //XmlNode = XmlDocRoot.SelectSingleNode( "//DefaultParameters/CiaTab/Output/LogReports" );
                //if( XmlNode != null ) {
                //    oCCia.SetLogReportStatus( bool.Parse( XmlNode.InnerText ) );
                //    checkBoxLogReport.Checked = oCCia.GetLogReportStatus();
                //}
                XmlNode = XmlDocRoot.SelectSingleNode( "//DefaultParameters/CiaTab/Output/Delimiters" );
                if( XmlNode != null ) {
                    oCCia.SetOutputFileDelimiterType( ( CCia.EDelimiters ) Enum.Parse( typeof( CCia.EDelimiters ), XmlNode.InnerText ) );
                    oCiaAdvancedForm.comboBoxOutputFileDelimiter.Text = oCCia.GetOutputFileDelimiterType().ToString();
                }
                XmlNode = XmlDocRoot.SelectSingleNode( "//DefaultParameters/CiaTab/Output/Error" );
                if( XmlNode != null ) {
                    oCCia.SetErrorType( ( CCia.EErrorType ) Enum.Parse( typeof( CCia.EErrorType ), XmlNode.InnerText ) );
                    oCiaAdvancedForm.comboBoxErrorType.Text = oCCia.GetErrorType().ToString();
                }

                XmlNode = XmlDocRoot.SelectSingleNode( "//DefaultParameters/IpaTab/MassTol" );
                if( XmlNode != null ) {
                    oCCia.Ipa.m_ppm_tol = double.Parse( XmlNode.InnerText );
                    numericUpDownIpaMassTolerance.Value = (decimal) oCCia.Ipa.m_ppm_tol;
                }
                XmlNode = XmlDocRoot.SelectSingleNode( "//DefaultParameters/IpaTab/MajorPeaksMinSToN" );
                if( XmlNode != null ) {
                    oCCia.Ipa.m_min_major_sn = double.Parse( XmlNode.InnerText );
                    numericUpDownIpaMajorPeaksMinSN.Value = ( decimal ) oCCia.Ipa.m_min_major_sn;
                }
                XmlNode = XmlDocRoot.SelectSingleNode( "//DefaultParameters/IpaTab/MinorPeaksMinSToN" );
                if( XmlNode != null ) {
                    oCCia.Ipa.m_min_minor_sn = double.Parse( XmlNode.InnerText );
                    numericUpDownIpaMinorPeaksMinSN.Value = ( decimal ) oCCia.Ipa.m_min_minor_sn;
                }
                XmlNode = XmlDocRoot.SelectSingleNode( "//DefaultParameters/IpaTab/MathedPeakReport" );
                if( XmlNode != null ) {
                    oCCia.Ipa.m_matched_peaks_report = bool.Parse( XmlNode.InnerText );
                    checkBoxIpaMatchedPeakReport.Checked = oCCia.Ipa.m_matched_peaks_report;
                }
                XmlNode = XmlDocRoot.SelectSingleNode( "//DefaultParameters/IpaTab/MinPeakProbabilityToScore" );
                if( XmlNode != null ) {
                    oCCia.Ipa.m_min_p_to_score = double.Parse( XmlNode.InnerText );
                    numericUpDownIpaMinPeakProbabilityToScore.Value = ( decimal ) oCCia.Ipa.m_min_p_to_score;
                }
                XmlNode = XmlDocRoot.SelectSingleNode( "//DefaultParameters/IpaTab/DbFilter" );
                if( XmlNode != null ) {
                    oCCia.Ipa.m_IPDB_ec_filter = XmlNode.InnerText;
                    textBoxIpaFilter.Text = oCCia.Ipa.m_IPDB_ec_filter;
                }
                this.ResumeLayout();
            } catch( Exception ex ) {
                MessageBox.Show( ex.Message );
            }
        }
    }
    class ReportData {
        public short [] Formula;
        public double [] Abundances;
    }     
}

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
using System.Threading;
using System.Globalization;

using FileReader;
using TestFSDBSearch;
using CIA;

namespace CiaUi {
    public partial class CiaAdvancedForm : Form {
        public CiaAdvancedForm() {
            InitializeComponent();
        }
        FormularityUIForm oFormularityUIForm;
        CCia oCCia;
        System.Windows.Forms.CheckBox [] GoldenRuleFilterUsage;
        public CiaAdvancedForm( FormularityUIForm oFormularityUIForm ) {
            InitializeComponent();
            this.oFormularityUIForm = oFormularityUIForm;
            this.oCCia = oFormularityUIForm.oCCia;
            this.SuspendLayout();

            comboBoxIonization.DataSource = Enum.GetValues( typeof( TestFSDBSearch.TotalSupport.IonizationMethod ) );
            comboBoxCalRegressionModel.DataSource = Enum.GetValues( typeof( TotalCalibration.ttlRegressionType ) );
            comboBoxFormulaScore.DataSource = oCCia.GetFormulaScoreNames();
            comboBoxRelationshipErrorType.DataSource = Enum.GetNames( typeof( CCia.RelationshipErrorType ) );

            //Formula assignment     
            short [] [] DftRelationFormulas = oCCia.GetRelationFormulaBuildingBlocks();
            for( int Relation = 0; Relation < DftRelationFormulas.Length; Relation++ ) {
                bool bb = false;
                if( Relation == 0 || Relation == 2 || Relation == 6 ) { bb = true; }
                checkedListBoxRelations.Items.Add( oCCia.FormulaToName( DftRelationFormulas [ Relation ] ), bb );
            }
            //Golden rules
            GoldenRuleFilterUsage = new System.Windows.Forms.CheckBox [ oCCia.GetGoldenRuleFilterUsage().Length ];
            System.Windows.Forms.GroupBox groupBoxGoldenRuleFilters = ( System.Windows.Forms.GroupBox ) this.Controls.Find( "groupBoxGoldenRuleFilters", true ) [ 0 ];
            for( int GoldenRuleIndex = 0; GoldenRuleIndex < GoldenRuleFilterUsage.Length; GoldenRuleIndex++ ) {
                GoldenRuleFilterUsage [ GoldenRuleIndex ] = ( System.Windows.Forms.CheckBox ) groupBoxGoldenRuleFilters.Controls [ "checkBoxGoldenRule" + ( GoldenRuleIndex + 1 ).ToString() ];
                GoldenRuleFilterUsage [ GoldenRuleIndex ].Text = oCCia.GetGoldenRuleFilterNames() [ GoldenRuleIndex ];
                GoldenRuleFilterUsage [ GoldenRuleIndex ].Checked = oCCia.GetGoldenRuleFilterUsage() [ GoldenRuleIndex ];
            }
            //Special filter
            comboBoxSpecialFilters.Items.Clear();
            string [] SpecialFilterNames = Enum.GetNames( typeof( CCia.ESpecialFilters ) );
            string [] SpecialFilterRules = oCCia.GetSpecialFilterRules();
            for( int SpecialFilter = 0; SpecialFilter < SpecialFilterRules.Length; SpecialFilter++ ) {
                comboBoxSpecialFilters.Items.Add( SpecialFilterNames [ SpecialFilter ] + ": " + SpecialFilterRules [ SpecialFilter ] );
            }
            comboBoxSpecialFilters.Text = oCCia.GetSpecialFilter().ToString();

            //Reports
            checkBoxIndividualFileReport.Checked = oCCia.GetGenerateIndividualFileReports();
            checkBoxChainReport.Checked = oCCia.GetGenerateChainReport();

            //Out file formats
            comboBoxOutputFileDelimiter.DataSource = Enum.GetNames( typeof( CCia.EDelimiters ) );
            comboBoxOutputFileDelimiter.Text = oCCia.GetOutputFileDelimiterType().ToString();
            comboBoxErrorType.DataSource = Enum.GetNames( typeof( CCia.EErrorType ) );
            comboBoxErrorType.Text = oCCia.GetErrorType().ToString();

            //checkBoxLogReport.Checked = oCCia.GetLogReportStatus();
            this.ResumeLayout();
        }

        private void buttonSwitchToSimple_Click( object sender, EventArgs e ) {
            this.Visible = false;
        }

        private void numericUpDownCharge_ValueChanged( object sender, EventArgs e ) {
            oCCia.Ipa.CS = ( int ) Math.Abs( numericUpDownCharge.Value );
            textBoxResult.Text = oCCia.Ipa.ChargedMassFormula_Descriptive;
        }
        private void textBoxAdduct_KeyDown( object sender, KeyEventArgs e ) {
            try {
                if( e.KeyCode == Keys.Return ) {
                    oCCia.Ipa.Adduct = textBoxAdduct.Text;
                    textBoxResult.Text = oCCia.Ipa.ChargedMassFormula_Descriptive;
                }
            } catch( Exception ex ) {
                MessageBox.Show( ex.Message );
            }
        }
        private void textBoxAdduct_Leave( object sender, EventArgs e ) {
            try {
                oCCia.Ipa.Adduct = textBoxAdduct.Text;
                textBoxResult.Text = oCCia.Ipa.ChargedMassFormula_Descriptive;
            } catch( Exception ex ) {
                MessageBox.Show( ex.Message );
            }
        }
        private void comboBoxIonization_SelectedIndexChanged( object sender, EventArgs e ) {
            oCCia.Ipa.Ionization = ( TestFSDBSearch.TotalSupport.IonizationMethod ) Enum.Parse( typeof( TestFSDBSearch.TotalSupport.IonizationMethod ), comboBoxIonization.Text );
            textBoxResult.Text = oCCia.Ipa.ChargedMassFormula_Descriptive;
        }

        private void checkBoxAlignment_CheckedChanged( object sender, EventArgs e ) {
            numericUpDownAlignmentTolerance.Enabled = checkBoxAlignment.Checked;
            checkBoxIndividualFileReport.Enabled = checkBoxAlignment.Checked;
            if( checkBoxAlignment.Checked == false ) {
                checkBoxIndividualFileReport.Checked = true;
            }
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

                //if( oCCia.GetDBFilenames().Length == 0 ) { throw new Exception( "Drop DB file." ); }
                //Alignment
                oCCia.SetAlignment( checkBoxAlignment.Checked );
                oCCia.SetAlignmentPpmTolerance( ( double ) numericUpDownAlignmentTolerance.Value );

                //Formula assignment
                oCCia.SetMassLimit( ( double ) numericUpDownDBMassLimit.Value );
                //not use oCCia.SetUseCIAFormulaScore( checkBoxUseCIAFormulaScore.Checked );
                oCCia.SetFormulaScore( ( CCia.EFormulaScore ) Array.IndexOf( oCCia.GetFormulaScoreNames(), comboBoxFormulaScore.Text ) );
                oCCia.SetUseKendrick( checkBoxCIAUseKendrick.Checked );
                oCCia.SetUseC13( checkBoxCIAUseC13.Checked );
                oCCia.SetC13Tolerance( ( double ) numericUpDownCIAC13Tolerance.Value );

                //Filters
                oCCia.SetUseFormulaFilter( checkBoxUseFormulaFilters.Checked );
                bool [] GoldenFilters = new bool [ GoldenRuleFilterUsage.Length ];
                for( int DftFilter = 0; DftFilter < GoldenRuleFilterUsage.Length; DftFilter++ ) {
                    GoldenFilters [ DftFilter ] = GoldenRuleFilterUsage [ DftFilter ].Checked;
                }
                oCCia.SetGoldenRuleFilterUsage( GoldenFilters );

                oCCia.SetSpecialFilter( ( CCia.ESpecialFilters ) Enum.Parse( typeof( CCia.ESpecialFilters ), comboBoxSpecialFilters.Text.Split( new char [] { ':' } ) [ 0 ] ) );
                oCCia.SetUserDefinedFilter( textBoxUserDefinedFilter.Text );
                //Relationships
                oCCia.SetUseRelation( checkBoxUseRelation.Checked );
                oCCia.SetMaxRelationGaps( ( int ) numericUpDownMaxRelationshipGaps.Value );
                oCCia.SetRelationshipErrorType( ( CCia.RelationshipErrorType ) Enum.Parse( typeof( CCia.RelationshipErrorType ), comboBoxRelationshipErrorType.Text ) );
                oCCia.SetRelationErrorAMU( ( double ) numericUpDownRelationErrorValue.Value );
                oCCia.SetUseBackward( checkBoxCIABackward.Checked );

                short [] [] ActiveRelationBlocks = new short [ checkedListBoxRelations.CheckedItems.Count ] [];
                for( int ActiveFormula = 0; ActiveFormula < checkedListBoxRelations.CheckedItems.Count; ActiveFormula++ ) {
                    ActiveRelationBlocks [ ActiveFormula ] = oCCia.NameToFormula( checkedListBoxRelations.CheckedItems [ ActiveFormula ].ToString() );
                }
                oCCia.SetRelationFormulaBuildingBlocks( ActiveRelationBlocks );

                //Reports
                oCCia.SetGenerateIndividualFileReports( checkBoxIndividualFileReport.Checked );
                oCCia.SetGenerateChainReport( checkBoxChainReport.Checked );

                //File formats
                oCCia.SetOutputFileDelimiterType( ( CCia.EDelimiters ) Enum.Parse( typeof( CCia.EDelimiters ), comboBoxOutputFileDelimiter.Text ) );
                oCCia.SetErrorType( ( CCia.EErrorType ) Enum.Parse( typeof( CCia.EErrorType ), comboBoxErrorType.Text ) );

                //oCCia.SetLogReportStatus( checkBoxLogReport.Checked );

                //Process                    
                oCCia.Process( Filenames, Masses, Abundances, SNs, Resolutions, RelAbundances, CalMasses, oStreamLogWriter );

                //change textbox
                textBoxDropSpectraFiles.Text = "Drop Spectra Files";
                textBoxDropSpectraFiles.AppendText( "\r\nProcessed files:" );
                foreach( string Filename in Filenames ) {
                    textBoxDropSpectraFiles.AppendText( "\r\n" + Path.GetFileName( Filename ) );
                }

                oStreamLogWriter.Flush();
                oStreamLogWriter.Close();
            } catch( Exception Ex ) {
                MessageBox.Show( Ex.Message );
                textBoxDropSpectraFiles.BackColor = Color.Pink;
            }
            textBoxDropSpectraFiles.BackColor = Color.LightGreen;
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
            CheckToProcess();
        }
        public void CheckToProcess() {
            bool CalibrationReady = ( ( ( TotalCalibration.ttlRegressionType ) comboBoxCalRegressionModel.SelectedValue == TotalCalibration.ttlRegressionType.none )
                        | ( ( ( TotalCalibration.ttlRegressionType ) comboBoxCalRegressionModel.SelectedValue != TotalCalibration.ttlRegressionType.none ) & ( textBoxCalFile.TextLength > "Drop calibration file: ".Length ) ) );
            bool CIAReady = ( oCCia.GetDBFilenames().Length > 0 ) & CalibrationReady;
            if( CIAReady == true) {
                textBoxDropSpectraFiles.BackColor = Color.LightGreen;
                textBoxDropSpectraFiles.Enabled = true;
            } else {
                textBoxDropSpectraFiles.BackColor = SystemColors.ControlLight;
                textBoxDropSpectraFiles.Enabled = false;
            }
        }

        private void checkBoxCIAUseC13_CheckedChanged( object sender, EventArgs e ) {
            numericUpDownCIAC13Tolerance.Enabled = checkBoxCIAUseC13.Checked;
        }

        private void checkBoxAlignment_CheckedChanged_1( object sender, EventArgs e ) {
            numericUpDownAlignmentTolerance.Enabled = checkBoxAlignment.Checked;
        }

        private void checkBoxUseRelation_CheckedChanged( object sender, EventArgs e ) {
            numericUpDownMaxRelationshipGaps.Enabled = checkBoxUseRelation.Checked;
            comboBoxRelationshipErrorType.Enabled = checkBoxUseRelation.Checked;
            numericUpDownRelationErrorValue.Enabled = checkBoxUseRelation.Checked;
            checkedListBoxRelations.Enabled = checkBoxUseRelation.Checked;
            checkBoxCIABackward.Enabled = checkBoxUseRelation.Checked;
        }

        private void buttonSetAdvancedDefault_Click( object sender, EventArgs e ) {
            checkBoxCIABackward.Checked = true;
            checkBoxCIAUseKendrick.Checked = true;
            checkBoxCIAUseC13.Checked = true;
            numericUpDownCIAC13Tolerance.Value = numericUpDownFormulaTolerance.Value;
            if( checkBoxAlignment.Checked == true ) {
                oCCia.SetGenerateIndividualFileReports( false );
            } else {
                oCCia.SetGenerateIndividualFileReports( true );
            }
            oCCia.SetGenerateChainReport( false );
            oCCia.SetOutputFileDelimiterType( CCia.EDelimiters.Comma );
            oCCia.SetErrorType( CCia.EErrorType.Signed );
        }
    }
}

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

using FileReader;
using TestFSDBSearch;
using CIA;

namespace CiaUi {
    public partial class CIAUIForm : Form {
        CCia oCCia = new CCia();
        System.Windows.Forms.CheckBox [] GoldenRuleFilterUsage;
        string [] DBPeaksTableHeaders = new string [] { "Index", "Neutral mass", "Formula", "Error, ppm" };
        public enum EPlotType{ ErrorVsNeutralMass, ErrorVs};
        public CIAUIForm() {
            InitializeComponent();
          
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
            checkBoxUseCIAFormulaScore.Checked = oCCia.GetUseCIAFormulaScore();
            checkBoxUseKendrick.Checked = oCCia.GetUseKendrick(); 

            //Relations
            checkBoxUseRelation.Checked = oCCia.GetUseRelation();
            numericUpDownMaxRelationshipGaps.Value = oCCia.GetMaxRelationGaps();
            numericUpDownRelationErrorAMU.Value = ( decimal ) oCCia.GetRelationErrorAMU();
            short [] [] DftRelationFormulas = oCCia.GetRelationFormulaBuildingBlocks();
            for( int Relation = 0; Relation < DftRelationFormulas.Length; Relation++ ) {
                bool bb = false;
                if( Relation == 0 || Relation == 2 || Relation == 6 ) { bb = true; }
                checkedListBoxRelations.Items.Add( oCCia.FormulaToName( DftRelationFormulas [ Relation ] ), bb );
            }

            //Filters
            checkBoxUseFormulaFilters.Checked = oCCia.GetUseFormulaFilter();
            //Golden filters
            GoldenRuleFilterUsage = new System.Windows.Forms.CheckBox [ oCCia.GetGoldenRuleFilterUsage().Length ];
            System.Windows.Forms.GroupBox groupBoxGoldenRuleFilters = ( System.Windows.Forms.GroupBox ) this.Controls.Find( "groupBoxGoldenRuleFilters", true ) [ 0 ];
            for( int GoldenRuleFilter = 0; GoldenRuleFilter < GoldenRuleFilterUsage.Length; GoldenRuleFilter++ ) {
                GoldenRuleFilterUsage [ GoldenRuleFilter ] = ( System.Windows.Forms.CheckBox ) groupBoxGoldenRuleFilters.Controls [ "checkBoxDftFilter" + ( GoldenRuleFilter + 1 ).ToString() ];
                GoldenRuleFilterUsage [ GoldenRuleFilter ].Text = oCCia.GetGoldenRuleFilterNames() [ GoldenRuleFilter ];
                GoldenRuleFilterUsage [ GoldenRuleFilter ].Checked = oCCia.GetGoldenRuleFilterUsage() [ GoldenRuleFilter ];
            }
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
            checkBoxSingleFileReport.Checked = oCCia.GetGenerateSingleFileReports();
            checkBoxChainReport.Checked = oCCia.GetGenerateChainReport();

            //Out file formats
            comboBoxOutputFileDelimeter.DataSource = Enum.GetNames( typeof( CCia.EDelimeters) );
            comboBoxOutputFileDelimeter.Text = oCCia.GetOutputFileDelimeterType().ToString();
            comboBoxErrorType.DataSource = Enum.GetNames( typeof( CCia.EErrorType) );
            comboBoxErrorType.Text = oCCia.GetErrorType().ToString();

            checkBoxLogReport.Checked = oCCia.GetLogReportStatus();
            
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

            //===============
            //chartError tab`
            //=============
            comboBoxPlotType.DataSource = Enum.GetNames( typeof( EPlotType ) );
            comboBoxPlotType.SelectedIndex = 0;                

            //================
            //DB inspector tab
            //================
            numericUpDownMass.Enabled = false;
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


            //===============
            //Spectra files area
            //===============
            comboBoxPolarity.DataSource = Enum.GetValues( typeof( TestFSDBSearch.TotalSupport.EPolarity ) );
            comboBoxPolarity.Text = oCCia.Ipa.Polarity.ToString();
            comboBoxIonPhysics.DataSource = Enum.GetValues( typeof( TestFSDBSearch.TotalSupport.IonPhysics ) );
            comboBoxIonPhysics.Text = oCCia.Ipa.IP.ToString();
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
        //CIA tab
        private void checkBoxAlignment_CheckedChanged( object sender, EventArgs e ) {
            numericUpDownAlignmentTolerance.Enabled = checkBoxAlignment.Checked;
            checkBoxSingleFileReport.Enabled = checkBoxAlignment.Checked;
            if( checkBoxAlignment.Checked == false ) {
                checkBoxSingleFileReport.Checked = true;
            }
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
            numericUpDownMass.Enabled = true;
            textBoxDBRecords.Text = oCCia.GetDBRecords().ToString();
            textBoxDBMinMass.Text = oCCia.GetDBMinMass().ToString();
            textBoxDBMaxMass.Text = oCCia.GetDBMaxMass().ToString();
            textBoxDBMinError.Text = oCCia.GetDBMinError().ToString();
            textBoxDBMaxError.Text = oCCia.GetDBMaxError().ToString();
            CheckToProcess();
        }
        private void numericUpDownFormulaError_ValueChanged( object sender, EventArgs e ) {
            oCCia.SetFormulaPPMTolerance( ( double ) numericUpDownFormulaTolerance.Value );
            numericUpDownMass_ValueChanged( sender, e );
        }
        private void checkBoxUseFormulaFilters_CheckedChanged( object sender, EventArgs e ) { 
            groupBoxGoldenRuleFilters.Enabled = checkBoxUseFormulaFilters.Checked;
            comboBoxSpecialFilters.Enabled = checkBoxUseFormulaFilters.Checked;
            textBoxUserDefinedFilter.Enabled = checkBoxUseFormulaFilters.Checked;
        }
        private void checkBoxUseRelation_CheckedChanged( object sender, EventArgs e ) {
            numericUpDownMaxRelationshipGaps.Enabled = checkBoxUseRelation.Checked;
            numericUpDownRelationErrorAMU.Enabled = checkBoxUseRelation.Checked;
            checkedListBoxRelations.Enabled = checkBoxUseRelation.Checked;
        }
        private void buttonCiaWoAlignmentDftSettings_Click( object sender, EventArgs e ) {
            checkBoxAlignment.Checked = false;
            numericUpDownDBMassLimit.Value = 500;
            comboBoxFormulaScore.SelectedIndex = (int) CCia.EFormulaScore.HAcap;
            checkBoxUseFormulaFilters.Checked = false;
            for( int GoldenRuleFilter = 0; GoldenRuleFilter < GoldenRuleFilterUsage.Length; GoldenRuleFilter++ ) {
                GoldenRuleFilterUsage [ GoldenRuleFilter ].Checked = true;
            }
            checkBoxUseRelation.Checked = true;
            numericUpDownMaxRelationshipGaps.Value = 5;
            numericUpDownRelationErrorAMU.Value = ( decimal ) 0.00002;
            for( int RelationBlock = 0; RelationBlock < checkedListBoxRelations.Items.Count; RelationBlock++ ) {
                if( ( RelationBlock == 0 ) || ( RelationBlock == 2 ) || ( RelationBlock == 6 ) ) {
                    checkedListBoxRelations.SetItemChecked( RelationBlock, true );
                } else {
                    checkedListBoxRelations.SetItemChecked( RelationBlock, false );
                }
            }
            comboBoxSpecialFilters.Text = CCia.ESpecialFilters.None.ToString();
            textBoxUserDefinedFilter.Text = string.Empty;
            checkBoxSingleFileReport.Checked = true;
            checkBoxChainReport.Checked = false;
            comboBoxErrorType.Text = CCia.EErrorType.CIA.ToString();
        }
        private void buttonCiaWithAlignmentDftSettings_Click( object sender, EventArgs e ) {
            checkBoxUseRelation.Checked = true;
            numericUpDownAlignmentTolerance.Value = ( decimal ) oCCia.GetAlignmentPpmTolerance();
            numericUpDownDBMassLimit.Value = 500;
            comboBoxFormulaScore.SelectedIndex = ( int) CCia.EFormulaScore.HAcap;
            checkBoxUseFormulaFilters.Checked = false;
            for( int GoldenRuleFilter = 0; GoldenRuleFilter < GoldenRuleFilterUsage.Length; GoldenRuleFilter++ ) {
                GoldenRuleFilterUsage [ GoldenRuleFilter ].Checked = true;
            }
            checkBoxUseRelation.Checked = true;
            numericUpDownMaxRelationshipGaps.Value = 5;
            numericUpDownRelationErrorAMU.Value = ( decimal ) 0.00002;
            for( int RelationBlock = 0; RelationBlock < checkedListBoxRelations.Items.Count; RelationBlock++ ) {
                if( ( RelationBlock == 0 ) || ( RelationBlock == 2 ) || ( RelationBlock == 6 ) ) {
                    checkedListBoxRelations.SetItemChecked( RelationBlock, true );
                } else {
                    checkedListBoxRelations.SetItemChecked( RelationBlock, false );
                }
            }
            comboBoxSpecialFilters.Text = CCia.ESpecialFilters.None.ToString();
            textBoxUserDefinedFilter.Text = string.Empty;
            checkBoxSingleFileReport.Checked = false;
            checkBoxChainReport.Checked = false;
            comboBoxErrorType.Text = CCia.EErrorType.CIA.ToString();
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
            if( numericUpDownMass.Value < 0 ) { return; }
            tableLayoutPanelDBPeaks.SuspendLayout();
            tableLayoutPanelDBPeaks.Enabled = true;
            double Mass = (double) numericUpDownMass.Value;
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
                oCCia.ReadFiles( Filenames);
                oCCia.ReportFormulas();
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
            string Delimeter = ",";
            string HeaderLine = "NeutralMass" + Delimeter + "Mass";
            foreach( string Element in Enum.GetNames( typeof( CCia.EElemNumber ) ) ) {
                HeaderLine = HeaderLine + Delimeter + Element;
            }
            foreach( string SampleName in SampleNames ) {
                HeaderLine = HeaderLine + Delimeter + SampleName;
            }
            StreamWriter oStreamWriter = new StreamWriter( Path.GetDirectoryName( Filenames [ 0 ] ) + "\\Comparision.csv" );            
            oStreamWriter.WriteLine( HeaderLine );

            foreach( KeyValuePair<double, ReportData> KVP in FormulaDict ) {
                string Line = KVP.Key.ToString() + Delimeter + oCCia.Ipa.GetChargedMass( KVP.Key ).ToString();
                foreach( short Count in KVP.Value.Formula ) {
                    Line = Line + Delimeter + Count.ToString();
                }
                foreach( double Abundance in KVP.Value.Abundances ) {
                    Line = Line + Delimeter + Abundance.ToString();
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
            Microsoft.Office.Interop.Excel.Worksheet MySheet = MyBook.Sheets [ 1 ];
            MySheet.Cells [ 1, 1].Value = "mz";
            MySheet.Cells [ 1, 2].Value = "i";
            MySheet.Cells [ 1, 3].Value = "sn";
            MySheet.Cells [ 1, 4].Value = "res";
            MySheet.Cells [ 1, 5].Value = "rel_ab";

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

        //Input files
        private void comboBoxDataPolarity_SelectedIndexChanged( object sender, EventArgs e ) {
            TestFSDBSearch.TotalSupport.EPolarity CurrentPolarity = ( TestFSDBSearch.TotalSupport.EPolarity ) Enum.Parse( typeof( TestFSDBSearch.TotalSupport.EPolarity ), comboBoxPolarity.Text );
            oCCia.Ipa.Polarity = CurrentPolarity;
            numericUpDownMass_ValueChanged( sender, e );
        }
        private void textBoxAdduct_KeyDown( object sender, KeyEventArgs e ) {
            try {
                if( e.KeyCode == Keys.Return ) {
                    oCCia.Ipa.Adduct = textBoxAdduct.Text;
                    numericUpDownMass_ValueChanged( sender, e );
                }
            } catch( Exception ex ) {
                MessageBox.Show( ex.Message );
            }
        }
        private void textBoxAdduct_Leave( object sender, EventArgs e ) {
            try{
                oCCia.Ipa.Adduct = textBoxAdduct.Text;
                numericUpDownMass_ValueChanged( sender, e );
            } catch( Exception ex ) {
                MessageBox.Show( ex.Message );
            }
        }

        private void comboBoxIonPhysics_SelectedIndexChanged( object sender, EventArgs e ) {
            oCCia.Ipa.IP = ( TestFSDBSearch.TotalSupport.IonPhysics ) Enum.Parse( typeof( TestFSDBSearch.TotalSupport.IonPhysics ), comboBoxIonPhysics.Text );
            numericUpDownMass_ValueChanged( sender, e );
        }
        private void numericUpDownCharge_ValueChanged( object sender, EventArgs e ) {
            oCCia.Ipa.CS = ( int ) Math.Abs( numericUpDownCharge.Value );
            numericUpDownMass_ValueChanged( sender, e );
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
                textBoxDropFiles.BackColor = Color.LightGreen;
                textBoxDropFiles.Enabled = true;
            } else {
                textBoxDropFiles.BackColor = SystemColors.ControlLight;
                textBoxDropFiles.Enabled = false;
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
        private void textBoxDropFiles_DragEnter( object sender, DragEventArgs e ) {
            if( e.Data.GetDataPresent( DataFormats.FileDrop ) == true ) {
                e.Effect = DragDropEffects.Copy;
            }
        }
        private void textBoxDropFiles_DragDrop( object sender, DragEventArgs e ) {
            textBoxDropFiles.BackColor = Color.Red;
            try {
                string [] Filenames = ( string [] ) e.Data.GetData( DataFormats.FileDrop );
                if( checkBoxCIA.Checked == true ) {
                    if( oCCia.GetDBFilenames().Length == 0 ) { throw new Exception( "Drop DB file." ); }

                    //Calibration                                        
                    oCCia.oTotalCalibration.ttl_cal_regression = ( TotalCalibration.ttlRegressionType ) Enum.Parse( typeof( TotalCalibration.ttlRegressionType ), comboBoxCalRegressionModel.Text );
                    oCCia.oTotalCalibration.ttl_cal_rf = ( double ) numericUpDownCalRelFactor.Value;
                    oCCia.oTotalCalibration.ttl_cal_start_ppm = ( double ) numericUpDownCalStartTolerance.Value;
                    oCCia.oTotalCalibration.ttl_cal_target_ppm = ( double ) numericUpDownCalEndTolerance.Value;
                    oCCia.oTotalCalibration.ttl_cal_min_sn = ( double ) numericUpDownCalMinSN.Value;
                    oCCia.oTotalCalibration.ttl_cal_min_abu_pct = ( double ) numericUpDownCalMinRelAbun.Value;
                    oCCia.oTotalCalibration.ttl_cal_max_abu_pct = ( double ) numericUpDownCalMaxRelAbun.Value;

                    //Alignment
                    oCCia.SetAlignment( checkBoxAlignment.Checked );
                    oCCia.SetAlignmentPpmTolerance( ( double ) numericUpDownAlignmentTolerance.Value );

                    //Formula assignment
                    oCCia.SetMassLimit( ( double ) numericUpDownDBMassLimit.Value );
                    oCCia.SetUseCIAFormulaScore( checkBoxUseCIAFormulaScore.Checked );
                    oCCia.SetFormulaScore( ( CCia.EFormulaScore ) Array.IndexOf( oCCia.GetFormulaScoreNames(), comboBoxFormulaScore.Text ) );
                    oCCia.SetUseKendrick( checkBoxUseKendrick.Checked );

                    //Filters
                    oCCia.SetUseFormulaFilter( checkBoxUseFormulaFilters.Checked );
                    bool [] GoldenFilters = new bool [ GoldenRuleFilterUsage.Length ];
                    for( int DftFilter = 0; DftFilter < GoldenRuleFilterUsage.Length; DftFilter++ ) {
                        GoldenFilters [ DftFilter ] = GoldenRuleFilterUsage [ DftFilter ].Checked;
                    }
                    oCCia.SetGoldenRuleFilterUsage( GoldenFilters );

                    oCCia.SetSpecialFilter( ( CCia.ESpecialFilters ) Enum.Parse( typeof( CCia.ESpecialFilters ), comboBoxSpecialFilters.Text.Split( new char[] {':'})[ 0] ) );
                    oCCia.SetUserDefinedFilter( textBoxUserDefinedFilter.Text );
                    //Relationships
                    oCCia.SetUseRelation( checkBoxUseRelation.Checked );
                    oCCia.SetMaxRelationGaps( ( int ) numericUpDownMaxRelationshipGaps.Value );
                    oCCia.SetRelationErrorAMU( ( double ) numericUpDownRelationErrorAMU.Value );

                    short [] [] ActiveRelationBlocks = new short [ checkedListBoxRelations.CheckedItems.Count ] [];
                    for( int ActiveFormula = 0; ActiveFormula < checkedListBoxRelations.CheckedItems.Count; ActiveFormula++ ) {
                        ActiveRelationBlocks [ ActiveFormula ] = oCCia.NameToFormula( checkedListBoxRelations.CheckedItems [ ActiveFormula ].ToString() );
                    }
                    oCCia.SetRelationFormulaBuildingBlocks( ActiveRelationBlocks );

                    //Reports
                    oCCia.SetGenerateSingleFileReports( checkBoxSingleFileReport.Checked );
                    oCCia.SetGenerateChainReport( checkBoxChainReport.Checked );

                    //File formats
                    oCCia.SetOutputFileDelimeterType( ( CCia.EDelimeters ) Enum.Parse( typeof( CCia.EDelimeters ), comboBoxOutputFileDelimeter.Text ) );
                    oCCia.SetErrorType( ( CCia.EErrorType ) Enum.Parse( typeof( CCia.EErrorType ), comboBoxErrorType.Text ) );

                    oCCia.SetLogReportStatus( checkBoxLogReport.Checked );

                    //Process                    
                    oCCia.Process( Filenames );

                    //change textbox
                    textBoxDropFiles.Text = "Drop Spectra Files";
                    textBoxDropFiles.AppendText( "\r\nProcessed files:" );
                    foreach( string Filename in Filenames ) {
                        textBoxDropFiles.AppendText( "\r\n" + Path.GetFileName( Filename ) );
                    }
                }
                if( checkBoxIpa.Checked == true ) {
                    oCCia.Ipa.SetCalculation( (TestFSDBSearch.TotalSupport.IonPhysics) comboBoxIonPhysics.SelectedItem,
                            (  TestFSDBSearch.TotalSupport.EPolarity) comboBoxPolarity.SelectedItem, textBoxAdduct.Text, ( int ) numericUpDownCharge.Value );

                    oCCia.Ipa.m_ppm_tol = ( double ) numericUpDownIpaMassTolerance.Value;
                    oCCia.Ipa.m_min_major_sn = ( double ) numericUpDownIpaMajorPeaksMinSN.Value;
                    oCCia.Ipa.m_min_minor_sn = ( double ) numericUpDownIpaMinorPeaksMinSN.Value;

                    oCCia.Ipa.m_min_major_pa_mm_abs_2_report = ( double ) numericUpDownIpaMinMajorPeaksToAbsToReport.Value;
                    oCCia.Ipa.m_matched_peaks_report = checkBoxIpaMatchedPeakReport.Checked;

                    oCCia.Ipa.m_min_p_to_score = ( double ) numericUpDownIpaMinPeakProbabilityToScore.Value;

                    oCCia.Ipa.m_IPDB_ec_filter = textBoxIpaFilter.Text;

                    foreach( string Filename in Filenames){
                        double [] Masses;
                        double [] Abundances;
                        double [] SNs;
                        double [] Resolutions;
                        double [] RelAbundances;
                        FileReader.FileReader.ReadFile( Filename, out Masses, out Abundances, out SNs, out Resolutions, out RelAbundances );
                        double MaxAbundance = 0;
                        foreach( double Abundance in Abundances ) {
                            if( MaxAbundance < Abundance ) { MaxAbundance = Abundance; }
                        }
                        oCCia.Ipa.IPDB_log.Clear();
                        oCCia.Ipa.ttlSearch( ref Masses, ref Abundances, ref SNs, ref MaxAbundance, Filename );
                        string LogFileName = Path.GetDirectoryName( Filename ) + "\\" + Path.GetFileNameWithoutExtension( Filename ) + ".log";
                        StreamWriter oStreamLogWriter = new StreamWriter( LogFileName );
                        oStreamLogWriter.Write( oCCia.Ipa.IPDB_log );
                        oStreamLogWriter.Close();
                    }
                }
            } catch( Exception Ex ) {
                MessageBox.Show( Ex.Message );
                textBoxDropFiles.BackColor = Color.Pink;
            }
            textBoxDropFiles.BackColor = Color.LightGreen;
        }

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
                xmlWriter.WriteStartElement( "Polarity" );
                xmlWriter.WriteString( comboBoxPolarity.Text );
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement( "Adduct" );
                xmlWriter.WriteString( textBoxAdduct.Text );
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement( "IonPhysics" );
                xmlWriter.WriteString( comboBoxIonPhysics.Text );
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
                xmlWriter.WriteStartElement( "UseCiaFormulaScore" );
                xmlWriter.WriteString( checkBoxUseCIAFormulaScore.Checked.ToString() );
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement( "UseKendrick" );
                xmlWriter.WriteString( checkBoxUseKendrick.Checked.ToString() );
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement( "UseFormulaFilters" );
                xmlWriter.WriteString( checkBoxUseFormulaFilters.Checked.ToString() );
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement( "NumberOfElementsWithinMassRange" );
                xmlWriter.WriteString( GoldenRuleFilterUsage [ 0 ].Checked.ToString() );
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement( "ValenceRules" );
                xmlWriter.WriteString( GoldenRuleFilterUsage [ 1 ].Checked.ToString() );
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement( "ElementalRatios" );
                xmlWriter.WriteString( GoldenRuleFilterUsage [ 2 ].Checked.ToString() );
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement( "HeteroatomCounts" );
                xmlWriter.WriteString( GoldenRuleFilterUsage [ 3 ].Checked.ToString() );
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement( "PositiveAtoms" );
                xmlWriter.WriteString( GoldenRuleFilterUsage [ 4 ].Checked.ToString() );
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
                xmlWriter.WriteString( numericUpDownRelationErrorAMU.Value.ToString() );
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
                xmlWriter.WriteStartElement( "SingleFileReports" );
                xmlWriter.WriteString( checkBoxSingleFileReport.Checked.ToString() );
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement( "LogReports" );
                xmlWriter.WriteString( checkBoxLogReport.Checked.ToString() );
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement( "Delimeters" );
                xmlWriter.WriteString( comboBoxOutputFileDelimeter.Text );
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement( "Error" );
                xmlWriter.WriteString( ( ( CCia.EErrorType ) comboBoxErrorType.SelectedIndex ).ToString() );
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
                XmlNode XmlNode = XmlDocRoot.SelectSingleNode( "//DefaultParameters/InputFilesTab/Polarity" );
                if( XmlNode != null ) {
                    oCCia.Ipa.Polarity = ( TestFSDBSearch.TotalSupport.EPolarity ) Enum.Parse( typeof( TestFSDBSearch.TotalSupport.EPolarity ), XmlNode.InnerText );
                    string sss = oCCia.Ipa.Polarity.ToString();
                    comboBoxPolarity.Text = oCCia.Ipa.Polarity.ToString();
                }
                XmlNode = XmlDocRoot.SelectSingleNode( "//DefaultParameters/InputFilesTab/Adduct" );
                if( XmlNode != null ) {
                    oCCia.Ipa.Adduct = XmlNode.InnerText;
                    textBoxAdduct.Text = oCCia.Ipa.Adduct;
                }
                XmlNode = XmlDocRoot.SelectSingleNode( "//DefaultParameters/InputFilesTab/IonPhysics" );
                if( XmlNode != null ) {
                    oCCia.Ipa.IP = ( TestFSDBSearch.TotalSupport.IonPhysics ) Enum.Parse( typeof( TestFSDBSearch.TotalSupport.IonPhysics ), XmlNode.InnerText );
                    comboBoxIonPhysics.Text = oCCia.Ipa.IP.ToString();
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
                XmlNode = XmlDocRoot.SelectSingleNode( "//DefaultParameters/CiaTab/UseCiaFormulaScore" );
                if( XmlNode != null ) {
                    oCCia.SetUseCIAFormulaScore( bool.Parse( XmlNode.InnerText ) );
                    checkBoxUseCIAFormulaScore.Checked = oCCia.GetUseCIAFormulaScore();
                }
                XmlNode = XmlDocRoot.SelectSingleNode( "//DefaultParameters/CiaTab/UseKendrick" );
                if( XmlNode != null ) {
                    oCCia.SetUseKendrick( bool.Parse( XmlNode.InnerText ) );
                    checkBoxUseKendrick.Checked = oCCia.GetUseKendrick();
                }
                XmlNode = XmlDocRoot.SelectSingleNode( "//DefaultParameters/CiaTab/UseFormulaFilters" );
                if( XmlNode != null ) {
                    oCCia.SetUseFormulaFilter( bool.Parse( XmlNode.InnerText ) );
                    checkBoxUseFormulaFilters.Checked = oCCia.GetUseFormulaFilter();
                }
                XmlNode = XmlDocRoot.SelectSingleNode( "//DefaultParameters/CiaTab/NumberOfElementsWithinMassRange" );
                if( XmlNode != null ) {
                    GoldenRuleFilterUsage [ 0 ].Checked = bool.Parse( XmlNode.InnerText );
                }
                XmlNode = XmlDocRoot.SelectSingleNode( "//DefaultParameters/CiaTab/ValenceRules" );
                if( XmlNode != null ) {
                    GoldenRuleFilterUsage [ 1 ].Checked = bool.Parse( XmlNode.InnerText );
                }
                XmlNode = XmlDocRoot.SelectSingleNode( "//DefaultParameters/CiaTab/ElementalRatios" );
                if( XmlNode != null ) {
                    GoldenRuleFilterUsage [ 2 ].Checked = bool.Parse( XmlNode.InnerText );
                }
                XmlNode = XmlDocRoot.SelectSingleNode( "//DefaultParameters/CiaTab/HeteroatomCounts" );
                if( XmlNode != null ) {
                    GoldenRuleFilterUsage [ 3 ].Checked = bool.Parse( XmlNode.InnerText );
                }
                XmlNode = XmlDocRoot.SelectSingleNode( "//DefaultParameters/CiaTab/PositiveAtoms" );
                if( XmlNode != null ) {
                    GoldenRuleFilterUsage [ 4 ].Checked = bool.Parse( XmlNode.InnerText );
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
                    numericUpDownRelationErrorAMU.Value = ( decimal) oCCia.GetRelationErrorAMU();
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
                XmlNode = XmlDocRoot.SelectSingleNode( "//DefaultParameters/CiaTab/FormulaBuildingBlocks/C2H2O" );
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
                XmlNode = XmlDocRoot.SelectSingleNode( "//DefaultParameters/CiaTab/Output/SingleFileReports" );
                if( XmlNode != null ) {
                    oCCia.SetGenerateSingleFileReports( bool.Parse( XmlNode.InnerText ) );
                    checkBoxSingleFileReport.Checked = oCCia.GetGenerateSingleFileReports();
                }
                XmlNode = XmlDocRoot.SelectSingleNode( "//DefaultParameters/CiaTab/Output/LogReports" );
                if( XmlNode != null ) {
                    oCCia.SetLogReportStatus( bool.Parse( XmlNode.InnerText ) );
                    checkBoxLogReport.Checked = oCCia.GetLogReportStatus();
                }
                XmlNode = XmlDocRoot.SelectSingleNode( "//DefaultParameters/CiaTab/Output/Delimeters" );
                if( XmlNode != null ) {
                    oCCia.SetOutputFileDelimeterType( ( CCia.EDelimeters ) Enum.Parse( typeof( CCia.EDelimeters ), XmlNode.InnerText ) );
                    comboBoxOutputFileDelimeter.Text = oCCia.GetOutputFileDelimeterType().ToString();
                }
                XmlNode = XmlDocRoot.SelectSingleNode( "//DefaultParameters/CiaTab/Output/Error" );
                if( XmlNode != null ) {
                    oCCia.SetErrorType( ( CCia.EErrorType ) Enum.Parse( typeof( CCia.EErrorType ), XmlNode.InnerText ) );
                    comboBoxErrorType.Text = oCCia.GetErrorType().ToString();
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

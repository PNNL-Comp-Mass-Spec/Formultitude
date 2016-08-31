namespace TestSpectrumNoiseLevel {
    partial class TestSpectrumNoiseLevelForm {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose( bool disposing ) {
            if( disposing && ( components != null ) ) {
                components.Dispose();
            }
            base.Dispose( disposing );
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.scatterGraph1 = new NationalInstruments.UI.WindowsForms.ScatterGraph();
            this.RawSignal = new NationalInstruments.UI.ScatterPlot();
            this.xAxis1 = new NationalInstruments.UI.XAxis();
            this.yAxis1 = new NationalInstruments.UI.YAxis();
            this.MinNoise = new NationalInstruments.UI.ScatterPlot();
            this.MaxNoise = new NationalInstruments.UI.ScatterPlot();
            this.Signal = new NationalInstruments.UI.ScatterPlot();
            this.label1 = new System.Windows.Forms.Label();
            this.numericUpDownNoiseGain = new System.Windows.Forms.NumericUpDown();
            this.textBoxDropRawData = new System.Windows.Forms.TextBox();
            this.textBoxDropOurPeakFile = new System.Windows.Forms.TextBox();
            this.checkBoxSubstractNoiseLevel = new System.Windows.Forms.CheckBox();
            this.textBoxDropBrukerPeakData = new System.Windows.Forms.TextBox();
            this.buttonCompare = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.scatterGraph1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownNoiseGain)).BeginInit();
            this.SuspendLayout();
            // 
            // scatterGraph1
            // 
            this.scatterGraph1.InteractionMode = ((NationalInstruments.UI.GraphInteractionModes)((((((((NationalInstruments.UI.GraphInteractionModes.ZoomX | NationalInstruments.UI.GraphInteractionModes.ZoomY) 
            | NationalInstruments.UI.GraphInteractionModes.ZoomAroundPoint) 
            | NationalInstruments.UI.GraphInteractionModes.PanX) 
            | NationalInstruments.UI.GraphInteractionModes.PanY) 
            | NationalInstruments.UI.GraphInteractionModes.DragCursor) 
            | NationalInstruments.UI.GraphInteractionModes.DragAnnotationCaption) 
            | NationalInstruments.UI.GraphInteractionModes.EditRange)));
            this.scatterGraph1.Location = new System.Drawing.Point(-2, 43);
            this.scatterGraph1.Name = "scatterGraph1";
            this.scatterGraph1.Plots.AddRange(new NationalInstruments.UI.ScatterPlot[] {
            this.RawSignal,
            this.MinNoise,
            this.MaxNoise,
            this.Signal});
            this.scatterGraph1.Size = new System.Drawing.Size(1537, 743);
            this.scatterGraph1.TabIndex = 0;
            this.scatterGraph1.UseColorGenerator = true;
            this.scatterGraph1.XAxes.AddRange(new NationalInstruments.UI.XAxis[] {
            this.xAxis1});
            this.scatterGraph1.YAxes.AddRange(new NationalInstruments.UI.YAxis[] {
            this.yAxis1});
            // 
            // RawSignal
            // 
            this.RawSignal.XAxis = this.xAxis1;
            this.RawSignal.YAxis = this.yAxis1;
            // 
            // MinNoise
            // 
            this.MinNoise.XAxis = this.xAxis1;
            this.MinNoise.YAxis = this.yAxis1;
            // 
            // MaxNoise
            // 
            this.MaxNoise.XAxis = this.xAxis1;
            this.MaxNoise.YAxis = this.yAxis1;
            // 
            // Signal
            // 
            this.Signal.XAxis = this.xAxis1;
            this.Signal.YAxis = this.yAxis1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(88, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(57, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Noise gain";
            // 
            // numericUpDownNoiseGain
            // 
            this.numericUpDownNoiseGain.DecimalPlaces = 1;
            this.numericUpDownNoiseGain.Location = new System.Drawing.Point(156, 15);
            this.numericUpDownNoiseGain.Name = "numericUpDownNoiseGain";
            this.numericUpDownNoiseGain.Size = new System.Drawing.Size(78, 20);
            this.numericUpDownNoiseGain.TabIndex = 2;
            this.numericUpDownNoiseGain.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // textBoxDropRawData
            // 
            this.textBoxDropRawData.AllowDrop = true;
            this.textBoxDropRawData.Location = new System.Drawing.Point(437, 3);
            this.textBoxDropRawData.Multiline = true;
            this.textBoxDropRawData.Name = "textBoxDropRawData";
            this.textBoxDropRawData.ReadOnly = true;
            this.textBoxDropRawData.Size = new System.Drawing.Size(134, 34);
            this.textBoxDropRawData.TabIndex = 3;
            this.textBoxDropRawData.Text = "Drop raw data";
            this.textBoxDropRawData.DragDrop += new System.Windows.Forms.DragEventHandler(this.textBoxDropRawData_DragDrop);
            this.textBoxDropRawData.DragEnter += new System.Windows.Forms.DragEventHandler(this.textBoxDropRawData_DragEnter);
            // 
            // textBoxDropOurPeakFile
            // 
            this.textBoxDropOurPeakFile.AllowDrop = true;
            this.textBoxDropOurPeakFile.Location = new System.Drawing.Point(854, -1);
            this.textBoxDropOurPeakFile.Multiline = true;
            this.textBoxDropOurPeakFile.Name = "textBoxDropOurPeakFile";
            this.textBoxDropOurPeakFile.ReadOnly = true;
            this.textBoxDropOurPeakFile.Size = new System.Drawing.Size(134, 34);
            this.textBoxDropOurPeakFile.TabIndex = 4;
            this.textBoxDropOurPeakFile.Text = "Drop peak our file";
            this.textBoxDropOurPeakFile.DragDrop += new System.Windows.Forms.DragEventHandler(this.textBoxDropOurPeakFile_DragDrop);
            this.textBoxDropOurPeakFile.DragEnter += new System.Windows.Forms.DragEventHandler(this.textBoxDropOurPeakFile_DragEnter);
            // 
            // checkBoxSubstractNoiseLevel
            // 
            this.checkBoxSubstractNoiseLevel.AutoSize = true;
            this.checkBoxSubstractNoiseLevel.Location = new System.Drawing.Point(278, 16);
            this.checkBoxSubstractNoiseLevel.Name = "checkBoxSubstractNoiseLevel";
            this.checkBoxSubstractNoiseLevel.Size = new System.Drawing.Size(124, 17);
            this.checkBoxSubstractNoiseLevel.TabIndex = 5;
            this.checkBoxSubstractNoiseLevel.Text = "Substract noise level";
            this.checkBoxSubstractNoiseLevel.UseVisualStyleBackColor = true;
            // 
            // textBoxDropBrukerPeakData
            // 
            this.textBoxDropBrukerPeakData.AllowDrop = true;
            this.textBoxDropBrukerPeakData.Location = new System.Drawing.Point(1028, -1);
            this.textBoxDropBrukerPeakData.Multiline = true;
            this.textBoxDropBrukerPeakData.Name = "textBoxDropBrukerPeakData";
            this.textBoxDropBrukerPeakData.ReadOnly = true;
            this.textBoxDropBrukerPeakData.Size = new System.Drawing.Size(134, 34);
            this.textBoxDropBrukerPeakData.TabIndex = 6;
            this.textBoxDropBrukerPeakData.Text = "Drop peak Bruker file";
            this.textBoxDropBrukerPeakData.DragDrop += new System.Windows.Forms.DragEventHandler(this.textBoxDropBrukerPeakData_DragDrop);
            this.textBoxDropBrukerPeakData.DragEnter += new System.Windows.Forms.DragEventHandler(this.textBoxDropBrukerPeakData_DragEnter);
            // 
            // buttonCompare
            // 
            this.buttonCompare.Location = new System.Drawing.Point(1205, 2);
            this.buttonCompare.Name = "buttonCompare";
            this.buttonCompare.Size = new System.Drawing.Size(107, 31);
            this.buttonCompare.TabIndex = 7;
            this.buttonCompare.Text = "Compare peaks";
            this.buttonCompare.UseVisualStyleBackColor = true;
            this.buttonCompare.Click += new System.EventHandler(this.buttonCompare_Click);
            // 
            // TestSpectrumNoiseLevelForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1547, 798);
            this.Controls.Add(this.buttonCompare);
            this.Controls.Add(this.textBoxDropBrukerPeakData);
            this.Controls.Add(this.checkBoxSubstractNoiseLevel);
            this.Controls.Add(this.textBoxDropOurPeakFile);
            this.Controls.Add(this.textBoxDropRawData);
            this.Controls.Add(this.numericUpDownNoiseGain);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.scatterGraph1);
            this.Name = "TestSpectrumNoiseLevelForm";
            this.Text = "Test spectrum noise level";
            ((System.ComponentModel.ISupportInitialize)(this.scatterGraph1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownNoiseGain)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private NationalInstruments.UI.WindowsForms.ScatterGraph scatterGraph1;
        private NationalInstruments.UI.ScatterPlot RawSignal;
        private NationalInstruments.UI.XAxis xAxis1;
        private NationalInstruments.UI.YAxis yAxis1;
        private NationalInstruments.UI.ScatterPlot MinNoise;
        private NationalInstruments.UI.ScatterPlot MaxNoise;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown numericUpDownNoiseGain;
        private NationalInstruments.UI.ScatterPlot Signal;
        private System.Windows.Forms.TextBox textBoxDropRawData;
        private System.Windows.Forms.TextBox textBoxDropOurPeakFile;
        private System.Windows.Forms.CheckBox checkBoxSubstractNoiseLevel;
        private System.Windows.Forms.TextBox textBoxDropBrukerPeakData;
        private System.Windows.Forms.Button buttonCompare;
    }
}


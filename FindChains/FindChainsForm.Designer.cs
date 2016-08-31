namespace FindChains {
    partial class FindChainsForm {
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
            this.label1 = new System.Windows.Forms.Label();
            this.numericUpDownAbsError = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownRangeMin = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.numericUpDownRangeMax = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.numericUpDownMinPeaksInChain = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.numericUpDownBinsPerErrorRange = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.numericUpDownFrequencyError = new System.Windows.Forms.NumericUpDown();
            this.label10 = new System.Windows.Forms.Label();
            this.checkBoxFrequency = new System.Windows.Forms.CheckBox();
            this.checkBoxFileFormatPeakAbundance = new System.Windows.Forms.CheckBox();
            this.checkBoxFileFormatPeakMass = new System.Windows.Forms.CheckBox();
            this.checkBoxFileFormatPeakIndex = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.checkBoxBinLinks = new System.Windows.Forms.CheckBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.textBoxError = new System.Windows.Forms.TextBox();
            this.textBoxMaxError = new System.Windows.Forms.TextBox();
            this.numericUpDownPpmError = new System.Windows.Forms.NumericUpDown();
            this.label8 = new System.Windows.Forms.Label();
            this.numericUpDownMaxPeakToStartChain = new System.Windows.Forms.NumericUpDown();
            this.label9 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.checkBoxPPMProcess = new System.Windows.Forms.CheckBox();
            this.checkBoxAmuProcess = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownAbsError)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownRangeMin)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownRangeMax)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMinPeaksInChain)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownBinsPerErrorRange)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownFrequencyError)).BeginInit();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPpmError)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxPeakToStartChain)).BeginInit();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(19, 74);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(50, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Error, DA";
            // 
            // numericUpDownAbsError
            // 
            this.numericUpDownAbsError.DecimalPlaces = 7;
            this.numericUpDownAbsError.Increment = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.numericUpDownAbsError.Location = new System.Drawing.Point(119, 70);
            this.numericUpDownAbsError.Name = "numericUpDownAbsError";
            this.numericUpDownAbsError.Size = new System.Drawing.Size(131, 20);
            this.numericUpDownAbsError.TabIndex = 1;
            this.numericUpDownAbsError.Value = new decimal(new int[] {
            1,
            0,
            0,
            262144});
            // 
            // numericUpDownRangeMin
            // 
            this.numericUpDownRangeMin.DecimalPlaces = 3;
            this.numericUpDownRangeMin.Location = new System.Drawing.Point(119, 22);
            this.numericUpDownRangeMin.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numericUpDownRangeMin.Name = "numericUpDownRangeMin";
            this.numericUpDownRangeMin.Size = new System.Drawing.Size(131, 20);
            this.numericUpDownRangeMin.TabIndex = 3;
            this.numericUpDownRangeMin.Value = new decimal(new int[] {
            13,
            0,
            0,
            0});
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(19, 26);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Range min, mz";
            // 
            // numericUpDownRangeMax
            // 
            this.numericUpDownRangeMax.DecimalPlaces = 3;
            this.numericUpDownRangeMax.Location = new System.Drawing.Point(119, 46);
            this.numericUpDownRangeMax.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numericUpDownRangeMax.Name = "numericUpDownRangeMax";
            this.numericUpDownRangeMax.Size = new System.Drawing.Size(131, 20);
            this.numericUpDownRangeMax.TabIndex = 5;
            this.numericUpDownRangeMax.Value = new decimal(new int[] {
            15,
            0,
            0,
            0});
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(19, 50);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(80, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Range max, mz";
            // 
            // numericUpDownMinPeaksInChain
            // 
            this.numericUpDownMinPeaksInChain.Location = new System.Drawing.Point(135, 12);
            this.numericUpDownMinPeaksInChain.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numericUpDownMinPeaksInChain.Name = "numericUpDownMinPeaksInChain";
            this.numericUpDownMinPeaksInChain.Size = new System.Drawing.Size(131, 20);
            this.numericUpDownMinPeaksInChain.TabIndex = 7;
            this.numericUpDownMinPeaksInChain.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(35, 16);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(96, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "Min peaks in chain";
            // 
            // numericUpDownBinsPerErrorRange
            // 
            this.numericUpDownBinsPerErrorRange.Location = new System.Drawing.Point(119, 93);
            this.numericUpDownBinsPerErrorRange.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numericUpDownBinsPerErrorRange.Name = "numericUpDownBinsPerErrorRange";
            this.numericUpDownBinsPerErrorRange.Size = new System.Drawing.Size(131, 20);
            this.numericUpDownBinsPerErrorRange.TabIndex = 9;
            this.numericUpDownBinsPerErrorRange.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(19, 97);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(92, 13);
            this.label5.TabIndex = 8;
            this.label5.Text = "Bins in error range";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.numericUpDownFrequencyError);
            this.groupBox1.Controls.Add(this.label10);
            this.groupBox1.Controls.Add(this.checkBoxFrequency);
            this.groupBox1.Controls.Add(this.checkBoxFileFormatPeakAbundance);
            this.groupBox1.Controls.Add(this.checkBoxFileFormatPeakMass);
            this.groupBox1.Controls.Add(this.checkBoxFileFormatPeakIndex);
            this.groupBox1.Location = new System.Drawing.Point(39, 260);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(275, 129);
            this.groupBox1.TabIndex = 10;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Output files";
            // 
            // numericUpDownFrequencyError
            // 
            this.numericUpDownFrequencyError.DecimalPlaces = 7;
            this.numericUpDownFrequencyError.Enabled = false;
            this.numericUpDownFrequencyError.Increment = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.numericUpDownFrequencyError.Location = new System.Drawing.Point(169, 96);
            this.numericUpDownFrequencyError.Name = "numericUpDownFrequencyError";
            this.numericUpDownFrequencyError.Size = new System.Drawing.Size(85, 20);
            this.numericUpDownFrequencyError.TabIndex = 25;
            this.numericUpDownFrequencyError.Value = new decimal(new int[] {
            1,
            0,
            0,
            262144});
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(113, 98);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(50, 13);
            this.label10.TabIndex = 24;
            this.label10.Text = "Error, DA";
            // 
            // checkBoxFrequency
            // 
            this.checkBoxFrequency.AutoSize = true;
            this.checkBoxFrequency.Location = new System.Drawing.Point(31, 94);
            this.checkBoxFrequency.Name = "checkBoxFrequency";
            this.checkBoxFrequency.Size = new System.Drawing.Size(76, 17);
            this.checkBoxFrequency.TabIndex = 3;
            this.checkBoxFrequency.Text = "Frequency";
            this.checkBoxFrequency.UseVisualStyleBackColor = true;
            this.checkBoxFrequency.CheckedChanged += new System.EventHandler(this.checkBoxFrequency_CheckedChanged);
            // 
            // checkBoxFileFormatPeakAbundance
            // 
            this.checkBoxFileFormatPeakAbundance.AutoSize = true;
            this.checkBoxFileFormatPeakAbundance.Location = new System.Drawing.Point(31, 71);
            this.checkBoxFileFormatPeakAbundance.Name = "checkBoxFileFormatPeakAbundance";
            this.checkBoxFileFormatPeakAbundance.Size = new System.Drawing.Size(108, 17);
            this.checkBoxFileFormatPeakAbundance.TabIndex = 2;
            this.checkBoxFileFormatPeakAbundance.Text = "Peak abundance";
            this.checkBoxFileFormatPeakAbundance.UseVisualStyleBackColor = true;
            // 
            // checkBoxFileFormatPeakMass
            // 
            this.checkBoxFileFormatPeakMass.AutoSize = true;
            this.checkBoxFileFormatPeakMass.Checked = true;
            this.checkBoxFileFormatPeakMass.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxFileFormatPeakMass.Location = new System.Drawing.Point(31, 48);
            this.checkBoxFileFormatPeakMass.Name = "checkBoxFileFormatPeakMass";
            this.checkBoxFileFormatPeakMass.Size = new System.Drawing.Size(78, 17);
            this.checkBoxFileFormatPeakMass.TabIndex = 1;
            this.checkBoxFileFormatPeakMass.Text = "Peak mass";
            this.checkBoxFileFormatPeakMass.UseVisualStyleBackColor = true;
            // 
            // checkBoxFileFormatPeakIndex
            // 
            this.checkBoxFileFormatPeakIndex.AutoSize = true;
            this.checkBoxFileFormatPeakIndex.Checked = true;
            this.checkBoxFileFormatPeakIndex.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxFileFormatPeakIndex.Location = new System.Drawing.Point(31, 25);
            this.checkBoxFileFormatPeakIndex.Name = "checkBoxFileFormatPeakIndex";
            this.checkBoxFileFormatPeakIndex.Size = new System.Drawing.Size(79, 17);
            this.checkBoxFileFormatPeakIndex.TabIndex = 0;
            this.checkBoxFileFormatPeakIndex.Text = "Peak index";
            this.checkBoxFileFormatPeakIndex.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.checkBoxBinLinks);
            this.groupBox2.Location = new System.Drawing.Point(348, 275);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(179, 96);
            this.groupBox2.TabIndex = 11;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Debug output";
            // 
            // checkBoxBinLinks
            // 
            this.checkBoxBinLinks.AutoSize = true;
            this.checkBoxBinLinks.Location = new System.Drawing.Point(31, 25);
            this.checkBoxBinLinks.Name = "checkBoxBinLinks";
            this.checkBoxBinLinks.Size = new System.Drawing.Size(65, 17);
            this.checkBoxBinLinks.TabIndex = 0;
            this.checkBoxBinLinks.Text = "Bin links";
            this.checkBoxBinLinks.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(36, 414);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(55, 13);
            this.label6.TabIndex = 12;
            this.label6.Text = "Error, ppm";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(36, 442);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(77, 13);
            this.label7.TabIndex = 13;
            this.label7.Text = "Max error, ppm";
            // 
            // textBoxError
            // 
            this.textBoxError.Location = new System.Drawing.Point(135, 413);
            this.textBoxError.Name = "textBoxError";
            this.textBoxError.ReadOnly = true;
            this.textBoxError.Size = new System.Drawing.Size(112, 20);
            this.textBoxError.TabIndex = 14;
            // 
            // textBoxMaxError
            // 
            this.textBoxMaxError.Location = new System.Drawing.Point(135, 442);
            this.textBoxMaxError.Name = "textBoxMaxError";
            this.textBoxMaxError.ReadOnly = true;
            this.textBoxMaxError.Size = new System.Drawing.Size(112, 20);
            this.textBoxMaxError.TabIndex = 15;
            // 
            // numericUpDownPpmError
            // 
            this.numericUpDownPpmError.DecimalPlaces = 7;
            this.numericUpDownPpmError.Increment = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.numericUpDownPpmError.Location = new System.Drawing.Point(150, 64);
            this.numericUpDownPpmError.Name = "numericUpDownPpmError";
            this.numericUpDownPpmError.Size = new System.Drawing.Size(74, 20);
            this.numericUpDownPpmError.TabIndex = 17;
            this.numericUpDownPpmError.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(2, 68);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(55, 13);
            this.label8.TabIndex = 16;
            this.label8.Text = "Error, ppm";
            // 
            // numericUpDownMaxPeakToStartChain
            // 
            this.numericUpDownMaxPeakToStartChain.DecimalPlaces = 3;
            this.numericUpDownMaxPeakToStartChain.Location = new System.Drawing.Point(150, 21);
            this.numericUpDownMaxPeakToStartChain.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numericUpDownMaxPeakToStartChain.Name = "numericUpDownMaxPeakToStartChain";
            this.numericUpDownMaxPeakToStartChain.Size = new System.Drawing.Size(74, 20);
            this.numericUpDownMaxPeakToStartChain.TabIndex = 19;
            this.numericUpDownMaxPeakToStartChain.Value = new decimal(new int[] {
            500,
            0,
            0,
            0});
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(2, 26);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(137, 13);
            this.label9.TabIndex = 18;
            this.label9.Text = "Max peak to start chain, mz";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.numericUpDownBinsPerErrorRange);
            this.groupBox3.Controls.Add(this.label5);
            this.groupBox3.Controls.Add(this.numericUpDownRangeMax);
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Controls.Add(this.numericUpDownRangeMin);
            this.groupBox3.Controls.Add(this.label2);
            this.groupBox3.Controls.Add(this.numericUpDownAbsError);
            this.groupBox3.Controls.Add(this.label1);
            this.groupBox3.Location = new System.Drawing.Point(16, 76);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(273, 126);
            this.groupBox3.TabIndex = 20;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "AMU process";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.numericUpDownMaxPeakToStartChain);
            this.groupBox4.Controls.Add(this.label9);
            this.groupBox4.Controls.Add(this.numericUpDownPpmError);
            this.groupBox4.Controls.Add(this.label8);
            this.groupBox4.Location = new System.Drawing.Point(303, 82);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(256, 119);
            this.groupBox4.TabIndex = 21;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "PPM process";
            // 
            // checkBoxPPMProcess
            // 
            this.checkBoxPPMProcess.AutoSize = true;
            this.checkBoxPPMProcess.Checked = true;
            this.checkBoxPPMProcess.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxPPMProcess.Location = new System.Drawing.Point(325, 47);
            this.checkBoxPPMProcess.Name = "checkBoxPPMProcess";
            this.checkBoxPPMProcess.Size = new System.Drawing.Size(89, 17);
            this.checkBoxPPMProcess.TabIndex = 22;
            this.checkBoxPPMProcess.Text = "PPM process";
            this.checkBoxPPMProcess.UseVisualStyleBackColor = true;
            // 
            // checkBoxAmuProcess
            // 
            this.checkBoxAmuProcess.AutoSize = true;
            this.checkBoxAmuProcess.Location = new System.Drawing.Point(39, 47);
            this.checkBoxAmuProcess.Name = "checkBoxAmuProcess";
            this.checkBoxAmuProcess.Size = new System.Drawing.Size(90, 17);
            this.checkBoxAmuProcess.TabIndex = 23;
            this.checkBoxAmuProcess.Text = "AMU process";
            this.checkBoxAmuProcess.UseVisualStyleBackColor = true;
            // 
            // FindChainsForm
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(595, 492);
            this.Controls.Add(this.checkBoxAmuProcess);
            this.Controls.Add(this.checkBoxPPMProcess);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.textBoxMaxError);
            this.Controls.Add(this.textBoxError);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.numericUpDownMinPeaksInChain);
            this.Controls.Add(this.label4);
            this.Name = "FindChainsForm";
            this.Text = "Find chains";
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.FindChainsForm_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.FindChainsForm_DragEnter);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownAbsError)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownRangeMin)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownRangeMax)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMinPeaksInChain)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownBinsPerErrorRange)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownFrequencyError)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPpmError)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxPeakToStartChain)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown numericUpDownRangeMin;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown numericUpDownRangeMax;
        private System.Windows.Forms.Label label3;
        protected internal System.Windows.Forms.NumericUpDown numericUpDownAbsError;
        private System.Windows.Forms.NumericUpDown numericUpDownMinPeaksInChain;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown numericUpDownBinsPerErrorRange;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox checkBoxFileFormatPeakAbundance;
        private System.Windows.Forms.CheckBox checkBoxFileFormatPeakMass;
        private System.Windows.Forms.CheckBox checkBoxFileFormatPeakIndex;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox checkBoxBinLinks;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox textBoxError;
        private System.Windows.Forms.TextBox textBoxMaxError;
        protected internal System.Windows.Forms.NumericUpDown numericUpDownPpmError;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.NumericUpDown numericUpDownMaxPeakToStartChain;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.CheckBox checkBoxPPMProcess;
        private System.Windows.Forms.CheckBox checkBoxAmuProcess;
        private System.Windows.Forms.CheckBox checkBoxFrequency;
        protected internal System.Windows.Forms.NumericUpDown numericUpDownFrequencyError;
        private System.Windows.Forms.Label label10;
    }
}


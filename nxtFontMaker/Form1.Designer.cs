namespace nxtFontMaker {
    partial class Form1 {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.ImportFontButton = new System.Windows.Forms.Button();
            this.LoadNxtFontButton = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.CharListBox = new System.Windows.Forms.ListBox();
            this.AddCharButton = new System.Windows.Forms.Button();
            this.DrawPictureBox = new System.Windows.Forms.PictureBox();
            this.FontSizeNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.ApplyFontSizeButton = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.SpacingFirstCharListBox = new System.Windows.Forms.ListBox();
            this.SpacingSecondCharListBox = new System.Windows.Forms.ListBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.SpacingFurtherLeftButton = new System.Windows.Forms.Button();
            this.SpacingFurtherRightButton = new System.Windows.Forms.Button();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DrawPictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.FontSizeNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // ImportFontButton
            // 
            this.ImportFontButton.Location = new System.Drawing.Point(12, 12);
            this.ImportFontButton.Name = "ImportFontButton";
            this.ImportFontButton.Size = new System.Drawing.Size(191, 23);
            this.ImportFontButton.TabIndex = 0;
            this.ImportFontButton.Text = "import from existing font";
            this.ImportFontButton.UseVisualStyleBackColor = true;
            // 
            // LoadNxtFontButton
            // 
            this.LoadNxtFontButton.Location = new System.Drawing.Point(12, 41);
            this.LoadNxtFontButton.Name = "LoadNxtFontButton";
            this.LoadNxtFontButton.Size = new System.Drawing.Size(191, 23);
            this.LoadNxtFontButton.TabIndex = 2;
            this.LoadNxtFontButton.Text = "load nxtfont";
            this.LoadNxtFontButton.UseVisualStyleBackColor = true;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Location = new System.Drawing.Point(12, 97);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(776, 368);
            this.tabControl1.TabIndex = 3;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.DrawPictureBox);
            this.tabPage1.Controls.Add(this.AddCharButton);
            this.tabPage1.Controls.Add(this.CharListBox);
            this.tabPage1.Location = new System.Drawing.Point(4, 24);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(768, 340);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Draw";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.SpacingFurtherRightButton);
            this.tabPage2.Controls.Add(this.SpacingFurtherLeftButton);
            this.tabPage2.Controls.Add(this.pictureBox1);
            this.tabPage2.Controls.Add(this.label4);
            this.tabPage2.Controls.Add(this.label3);
            this.tabPage2.Controls.Add(this.SpacingSecondCharListBox);
            this.tabPage2.Controls.Add(this.SpacingFirstCharListBox);
            this.tabPage2.Location = new System.Drawing.Point(4, 24);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(768, 340);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Spacing";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // tabPage3
            // 
            this.tabPage3.Location = new System.Drawing.Point(4, 24);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(768, 340);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Saving";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // CharListBox
            // 
            this.CharListBox.FormattingEnabled = true;
            this.CharListBox.ItemHeight = 15;
            this.CharListBox.Items.AddRange(new object[] {
            "a"});
            this.CharListBox.Location = new System.Drawing.Point(6, 36);
            this.CharListBox.Name = "CharListBox";
            this.CharListBox.Size = new System.Drawing.Size(82, 289);
            this.CharListBox.TabIndex = 0;
            // 
            // AddCharButton
            // 
            this.AddCharButton.Location = new System.Drawing.Point(6, 7);
            this.AddCharButton.Name = "AddCharButton";
            this.AddCharButton.Size = new System.Drawing.Size(82, 23);
            this.AddCharButton.TabIndex = 1;
            this.AddCharButton.Text = "+ char";
            this.AddCharButton.UseVisualStyleBackColor = true;
            // 
            // DrawPictureBox
            // 
            this.DrawPictureBox.Location = new System.Drawing.Point(448, 7);
            this.DrawPictureBox.Name = "DrawPictureBox";
            this.DrawPictureBox.Size = new System.Drawing.Size(314, 314);
            this.DrawPictureBox.TabIndex = 2;
            this.DrawPictureBox.TabStop = false;
            // 
            // FontSizeNumericUpDown
            // 
            this.FontSizeNumericUpDown.Location = new System.Drawing.Point(668, 12);
            this.FontSizeNumericUpDown.Name = "FontSizeNumericUpDown";
            this.FontSizeNumericUpDown.Size = new System.Drawing.Size(120, 23);
            this.FontSizeNumericUpDown.TabIndex = 3;
            this.FontSizeNumericUpDown.Value = new decimal(new int[] {
            8,
            0,
            0,
            0});
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(574, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 15);
            this.label1.TabIndex = 4;
            this.label1.Text = "Font size";
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Location = new System.Drawing.Point(668, 41);
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(120, 23);
            this.numericUpDown1.TabIndex = 5;
            this.numericUpDown1.Value = new decimal(new int[] {
            16,
            0,
            0,
            0});
            // 
            // ApplyFontSizeButton
            // 
            this.ApplyFontSizeButton.Location = new System.Drawing.Point(574, 41);
            this.ApplyFontSizeButton.Name = "ApplyFontSizeButton";
            this.ApplyFontSizeButton.Size = new System.Drawing.Size(75, 23);
            this.ApplyFontSizeButton.TabIndex = 6;
            this.ApplyFontSizeButton.Text = "Apply";
            this.ApplyFontSizeButton.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(574, 67);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(214, 15);
            this.label2.TabIndex = 7;
            this.label2.Text = "warning: applying will wipe all progress";
            // 
            // SpacingFirstCharListBox
            // 
            this.SpacingFirstCharListBox.FormattingEnabled = true;
            this.SpacingFirstCharListBox.ItemHeight = 15;
            this.SpacingFirstCharListBox.Location = new System.Drawing.Point(6, 6);
            this.SpacingFirstCharListBox.Name = "SpacingFirstCharListBox";
            this.SpacingFirstCharListBox.Size = new System.Drawing.Size(120, 319);
            this.SpacingFirstCharListBox.TabIndex = 0;
            // 
            // SpacingSecondCharListBox
            // 
            this.SpacingSecondCharListBox.FormattingEnabled = true;
            this.SpacingSecondCharListBox.ItemHeight = 15;
            this.SpacingSecondCharListBox.Location = new System.Drawing.Point(167, 6);
            this.SpacingSecondCharListBox.Name = "SpacingSecondCharListBox";
            this.SpacingSecondCharListBox.Size = new System.Drawing.Size(120, 319);
            this.SpacingSecondCharListBox.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label3.Location = new System.Drawing.Point(132, 134);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(29, 30);
            this.label3.TabIndex = 2;
            this.label3.Text = "+";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label4.Location = new System.Drawing.Point(302, 134);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(29, 30);
            this.label4.TabIndex = 3;
            this.label4.Text = "=";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(462, 6);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(300, 300);
            this.pictureBox1.TabIndex = 4;
            this.pictureBox1.TabStop = false;
            // 
            // SpacingFurtherLeftButton
            // 
            this.SpacingFurtherLeftButton.Location = new System.Drawing.Point(462, 314);
            this.SpacingFurtherLeftButton.Name = "SpacingFurtherLeftButton";
            this.SpacingFurtherLeftButton.Size = new System.Drawing.Size(43, 23);
            this.SpacingFurtherLeftButton.TabIndex = 5;
            this.SpacingFurtherLeftButton.Text = "<<";
            this.SpacingFurtherLeftButton.UseVisualStyleBackColor = true;
            // 
            // SpacingFurtherRightButton
            // 
            this.SpacingFurtherRightButton.Location = new System.Drawing.Point(719, 314);
            this.SpacingFurtherRightButton.Name = "SpacingFurtherRightButton";
            this.SpacingFurtherRightButton.Size = new System.Drawing.Size(43, 23);
            this.SpacingFurtherRightButton.TabIndex = 6;
            this.SpacingFurtherRightButton.Text = ">>";
            this.SpacingFurtherRightButton.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 477);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.ApplyFontSizeButton);
            this.Controls.Add(this.numericUpDown1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.FontSizeNumericUpDown);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.LoadNxtFontButton);
            this.Controls.Add(this.ImportFontButton);
            this.Name = "Form1";
            this.Text = "nxtFontMaker";
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DrawPictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.FontSizeNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Button ImportFontButton;
        private Button LoadNxtFontButton;
        private TabControl tabControl1;
        private TabPage tabPage1;
        private PictureBox DrawPictureBox;
        private Button AddCharButton;
        private ListBox CharListBox;
        private TabPage tabPage2;
        private Button SpacingFurtherRightButton;
        private Button SpacingFurtherLeftButton;
        private PictureBox pictureBox1;
        private Label label4;
        private Label label3;
        private ListBox SpacingSecondCharListBox;
        private ListBox SpacingFirstCharListBox;
        private TabPage tabPage3;
        private NumericUpDown FontSizeNumericUpDown;
        private Label label1;
        private NumericUpDown numericUpDown1;
        private Button ApplyFontSizeButton;
        private Label label2;
    }
}
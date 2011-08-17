namespace Sudoku
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.button1 = new System.Windows.Forms.Button();
			this.SolvedLabel = new System.Windows.Forms.Label();
			this.SolvedGrid = new Sudoku.SudokuGrid();
			this.UnsolvedGrid = new Sudoku.SudokuGrid();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.UnsolvedGrid);
			this.groupBox1.Location = new System.Drawing.Point(12, 12);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(211, 272);
			this.groupBox1.TabIndex = 2;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Unsolved";
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.SolvedGrid);
			this.groupBox2.Location = new System.Drawing.Point(310, 12);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(211, 272);
			this.groupBox2.TabIndex = 3;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Solved";
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(229, 118);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(75, 23);
			this.button1.TabIndex = 4;
			this.button1.Text = "Solve! -->";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// SolvedLabel
			// 
			this.SolvedLabel.AutoSize = true;
			this.SolvedLabel.Location = new System.Drawing.Point(229, 144);
			this.SolvedLabel.Name = "SolvedLabel";
			this.SolvedLabel.Size = new System.Drawing.Size(35, 13);
			this.SolvedLabel.TabIndex = 5;
			this.SolvedLabel.Text = "label1";
			// 
			// SolvedGrid
			// 
			this.SolvedGrid.BackColor = System.Drawing.SystemColors.Control;
			this.SolvedGrid.Location = new System.Drawing.Point(7, 19);
			this.SolvedGrid.Name = "SolvedGrid";
			this.SolvedGrid.Size = new System.Drawing.Size(198, 240);
			this.SolvedGrid.TabIndex = 0;
			// 
			// UnsolvedGrid
			// 
			this.UnsolvedGrid.BackColor = System.Drawing.SystemColors.Control;
			this.UnsolvedGrid.Location = new System.Drawing.Point(6, 19);
			this.UnsolvedGrid.Name = "UnsolvedGrid";
			this.UnsolvedGrid.Size = new System.Drawing.Size(198, 240);
			this.UnsolvedGrid.TabIndex = 0;
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(534, 297);
			this.Controls.Add(this.SolvedLabel);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupBox1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.Name = "Form1";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Text = "Sudoku Solver";
			this.groupBox1.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.GroupBox groupBox2;
		public SudokuGrid SolvedGrid;
		public SudokuGrid UnsolvedGrid;
		private System.Windows.Forms.Label SolvedLabel;
		private System.Windows.Forms.Button button1;

	}
}


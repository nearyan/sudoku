namespace Sudoku
{
	partial class DebugForm
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
			this.sudokuGrid1 = new Sudoku.SudokuGrid();
			this.SuspendLayout();
			// 
			// sudokuGrid1
			// 
			this.sudokuGrid1.BackColor = System.Drawing.SystemColors.Control;
			this.sudokuGrid1.Location = new System.Drawing.Point(12, 12);
			this.sudokuGrid1.Name = "sudokuGrid1";
			this.sudokuGrid1.Size = new System.Drawing.Size(198, 279);
			this.sudokuGrid1.TabIndex = 0;
			// 
			// DebugForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(220, 302);
			this.Controls.Add(this.sudokuGrid1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.Name = "DebugForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "DebugForm";
			this.ResumeLayout(false);

		}

		#endregion

		public SudokuGrid sudokuGrid1;

	}
}
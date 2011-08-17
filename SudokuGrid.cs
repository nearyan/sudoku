using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Sudoku
{
	public partial class SudokuGrid : UserControl
	{
		SudokuPuzzle _puzzle;
		ToolTip tooltip = new ToolTip();

		public SudokuGrid()
		{
			InitializeComponent();

			tooltip.AutoPopDelay = int.MaxValue;
			tooltip.InitialDelay = 0;
			tooltip.ReshowDelay = 0;
			TextLabel.Text = "";
			PossibilitiesLabel.Text = "";
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public SudokuPuzzle Puzzle
		{
			get { UpdatePuzzle(); return _puzzle; }
			set { Initialize(value); }
		}

		public void Initialize(SudokuPuzzle puzzle)
		{
			if (puzzle == null)
				return;

			_puzzle = (SudokuPuzzle)puzzle.Clone();

			UpdateTextBoxes();
		}

		public void UpdateTextBoxes()
		{
			for (int i = 0; i < 9; ++i)
				for (int j = 0; j < 9; ++j)
					if (_puzzle[i, j].Value != 0)
						this.Controls[j * 9 + i].Text = _puzzle[i, j].Value.ToString();
					else
						this.Controls[j * 9 + i].Text = "";
		}

		public void UpdatePuzzle()
		{
			_puzzle = new SudokuPuzzle();
			for (int i = 0; i < 9; ++i)
				for (int j = 0; j < 9; ++j)
					try
					{
						if (this.Controls[j * 9 + i].Text.Length != 0)
							_puzzle[i, j].Value = int.Parse(this.Controls[j * 9 + i].Text);
						else
							_puzzle[i, j].Value = 0;
					}
					catch { _puzzle[i, j].Value = 0; }
					
		}

		private void textBox_KeyUp(object sender, KeyEventArgs e)
		{
			int index = Controls.IndexOf((Control)sender);
			int y = index / 9;
			int x = index % 9;

			if (e.KeyCode == Keys.Right && x != 8)
				++x;
			if (e.KeyCode == Keys.Left && x != 0)
				--x;
			if (e.KeyCode == Keys.Up && y != 0)
				--y;
			if (e.KeyCode == Keys.Down && y != 8)
				++y;

			if (e.KeyCode == Keys.Back)
				Controls[index].Text = "";

			int newIndex = 9 * y + x;
			if (index != newIndex)
				Controls[newIndex].Focus();
		}

		private void textBox_KeyPress(object sender, KeyPressEventArgs e)
		{
			int index = Controls.IndexOf((Control)sender);

			if (e.KeyChar >= '0' && e.KeyChar <= '9')
			{
				((Control)sender).Text = e.KeyChar.ToString();
				if (index < 80)
					Controls[index + 1].Focus();
			}

			e.Handled = true;
		}

		private void ClearButton_Click(object sender, EventArgs e)
		{
			for (int i = 0; i < 9; ++i)
				for (int j = 0; j < 9; ++j)
					this.Controls[j * 9 + i].Text = "";
		}

		private void textBox_MouseEnter(object sender, EventArgs e)
		{
			int index = Controls.IndexOf((Control)sender);
			int y = index / 9;
			int x = index % 9;

			PossibilitiesLabel.Text = "Possible values: " + _puzzle[x, y].Possibilities.ToString();
		}

		private void textBox_MouseLeave(object sender, EventArgs e)
		{
			PossibilitiesLabel.Text = "";
		}
	}
}

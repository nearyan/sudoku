using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace Sudoku
{
	public partial class DebugForm : Form, IDebugger
	{
		bool _active = true;
		Stopwatch _stopwatch;

		public DebugForm()
		{
			InitializeComponent();
		}

		#region IDebugger Members

		public void HighlightCell(int x, int y, Color color)
		{
			Control control = this.sudokuGrid1.Controls[y * 9 + x];
			switch (color)
			{
				case Color.Red:
					control.BackColor = System.Drawing.Color.LightPink;
					break;
				case Color.Blue:
					control.BackColor = System.Drawing.Color.LightSkyBlue;
					break;
				case Color.Green:
					control.BackColor = System.Drawing.Color.LightGreen;
					break;
				default:
					control.BackColor = System.Drawing.SystemColors.Window;
					break;
			}
		}

		public void HighlightCell(int x, int y)
		{
			Control control = this.sudokuGrid1.Controls[y * 9 + x];
			((TextBox)control).BorderStyle = BorderStyle.FixedSingle;
			return;
		}

		public new void Show()
		{
			if (Active)
			{
				if (_stopwatch != null)
					_stopwatch.Stop();
				sudokuGrid1.UpdateTextBoxes();
				this.ShowDialog();
				Clear();
				if (_stopwatch != null)
					_stopwatch.Start();
			}
		}

		void Clear()
		{
			for (int i = 0; i < 9; ++i)
				for (int j = 0; j < 9; ++j)
				{
					this.sudokuGrid1.Controls[j * 9 + i].BackColor = System.Drawing.SystemColors.Window;
					((TextBox)this.sudokuGrid1.Controls[j * 9 + i]).BorderStyle = BorderStyle.Fixed3D;
				}

			Hint = "";
		}

		public Stopwatch Stopwatch
		{
			get { return _stopwatch; }
			set { _stopwatch = value; }
		}

		public string Hint
		{
			get
			{
				return sudokuGrid1.TextLabel.Text;
			}
			set
			{
				sudokuGrid1.TextLabel.Text = value;
			}
		}

		public bool Active
		{
			get { return _active; }
			set { _active = value; }
		}

		#endregion
	}
}

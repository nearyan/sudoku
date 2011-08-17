using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Sudoku
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

		public void button1_Click(object sender, EventArgs e)
		{
			SudokuPuzzle puzzle = (SudokuPuzzle)UnsolvedGrid.Puzzle.Clone();

			DebugForm debugger = new DebugForm();
			debugger.Active = true;
			debugger.sudokuGrid1.Puzzle = puzzle;
			debugger.Stopwatch = puzzle.Stopwatch;
			puzzle.Debugger = debugger;

			puzzle.Solve();

			SolvedGrid.Puzzle = puzzle;
			SolvedLabel.Text = "Done in\n" + (puzzle.Stopwatch.Elapsed.Ticks / 10000.0).ToString() + "\nmilliseconds.";
		}
    }
}

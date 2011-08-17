/**
 * Copyright 2011 Sietse Ringers
 * 
 * This program  and all of its source files is free software: you can 
 * redistribute it and/or modify it under the terms of the GNU General 
 * Public License as published by the Free Software Foundation, version
 * 3.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * A copy of the GNU General Public License v3 is included in LICENSE.txt.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;

namespace Sudoku
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			// Generate a new puzzle. Can take rather long (up to half a minute or so)
			//Stopwatch stopwatch = new Stopwatch();
			//stopwatch.Start();
			//SudokuPuzzle sudoku = SudokuPuzzle.Generate();
			//stopwatch.Stop();

			//int hints = sudoku.Grid.Count(kvp => kvp.Value.IsSolved);
			//MessageBox.Show("Generated the puzzle in " + stopwatch.ElapsedMilliseconds + " milliseconds."
			//	+ "\nContains " + hints + " hints.", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);

			// Some test puzzles:
			SudokuPuzzle sudoku = new SudokuPuzzle();

			//sudoku.Fill("200807000 070610029 400903010 891000005 020109080 700000961 060305008 930068050 000201004");
			//sudoku.Fill("004090800 780020000 090008001 000600084 038000590 940001000 200300070 000060035 003070600");
			//sudoku.Fill("000400300 000801005 480000090 708000010 009207500 060000709 090000026 300504000 007009000");

			// Naked Sets, Intersection and Colors test
			//sudoku.Fill("000200000 065000900 070006040 000001005 710000009 009020010 001000700 087304020 000060094");

			// From http://www.forbeginners.info/sudoku-puzzles/extreme-1.htm
			sudoku.Fill("009748000 700000000 020109000 007000240 064010590 098000300 000803020 000000006 000275900");
			// From http://www.forbeginners.info/sudoku-puzzles/extreme-20.htm
			//sudoku.Fill("600000040 005002007 729000003 090040001 000060000 400080070 300000165 200400800 050000004");

			// From Folia; has two solutions
			//sudoku.Fill("026005073 050040286 730000000 000078009 180000307 079000040 200500600 008730004 000002005");

			// Put the unsolved sudoku on the left grid
			Form1 form = new Form1();
			form.UnsolvedGrid.Initialize(sudoku);

			// Simulate a click on the solve button to solve it
			form.button1_Click(null, EventArgs.Empty);

			Application.Run(form);	
		}
	}
}

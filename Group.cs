using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sudoku
{
	/// <summary>
	/// A row, column or 3x3-square. May contain each digit only once.
	/// </summary>
	public class Group : Range
	{
		IDebugger _debugger;

		public int Sum
		{
			get
			{
				int sum = 0;
				for (int i = 0; i < Width; ++i)
					for (int j = 0; j < Height; ++j)
						sum += this[i, j].Value;

				return sum;
			}
		}

		public Group(int width, int height, bool initialize)
			: base(width, height, initialize) { }

		public IDebugger Debugger
		{
			get { return _debugger; }
			set { _debugger = value; }
		}

		#region Solving methods
		/// <summary>
		/// For each number from 1 through 9, count the possible number of locations for it.
		/// If there is only one, then fill it in.
		/// </summary>
		/// <returns>True if a number was filled in.</returns>
		public bool Scan()
		{
			bool changed = false;

			for (int x = 1; x <= 9; ++x)
			{
				if (ContainsValue(x))
					continue;

				// Retrieve a list of all cells which could contain our number, x.
				var q = from kvp in this.Grid
						where kvp.Value.Possibilities[x]
						select kvp.Value;

				if (q.Count() == 1)
				{
					q.First().Value = x;
					changed = true;
				}
			}

			return changed;
		}

		/// <summary>
		/// We look for a set of n cells. If we combine the possible values for these values
		/// and find that there are n of them, then all of these possible values must be 
		/// in these n cells, so we exclude them as a possibility from any other cells in all 
		/// of the ranges that these cells share.
		/// (See http://www.sudokuslam.com/hints.html#naked.)
		/// </summary>
		/// <returns>True if any possibilities were excluded in any range.</returns>
		public bool NakedSets() // Hey, I wasn't the one that thought up this name
		{
			bool changed = false;

			for (int i = 0; i < 9; ++i)
				for (int j = i + 1; j < 9; ++j)
				{
					List<Cell> cells = new List<Cell> { Grid.ElementAt(i).Value, Grid.ElementAt(j).Value };
					if (cells.Count(cell => !cell.IsSolved) == 2) // If both cells are unsolved
						changed = TryNakedSets(cells) || changed;
				}

			for (int i = 0; i < 9; ++i)
				for (int j = i + 1; j < 9; ++j)
					for (int k = j + 1; k < 9; ++k)
					{
						List<Cell> cells = new List<Cell> { Grid.ElementAt(i).Value, Grid.ElementAt(j).Value, Grid.ElementAt(k).Value };
						if (cells.Count(cell => !cell.IsSolved) == 3) // If all cells are unsolved
							changed = TryNakedSets(cells) || changed;
					}

			return changed;
		}

		bool TryNakedSets(IEnumerable<Cell> cells)
		{
			bool changed = false;

			Possibilities or = Utilities.Or(cells.Select(cell => cell.Possibilities));
			if (or.Count == cells.Count())
			{
				if (false && Debugger != null)
				{
					foreach (Cell cell in cells)
					{
						Debugger.HighlightCell(cell.Coordinates.X, cell.Coordinates.Y);
					}
					Debugger.Hint = "Method: Naked Sets\n Values: " + or.ToString();
					Debugger.Show();
				}
				List<Group> ranges = Utilities.SelectCommonGroups(cells);
				foreach (Group range in ranges)
					changed = range.RemovePossibilities(or.Where(kvp => kvp.Value).Select(kvp => kvp.Key), cells) || changed;
			}

			return changed;
		}

		/// <summary>
		/// If all of the possible location for a certain value are contained within
		/// the intersection of the current region and another one, then we can exclude
		/// other possible location for our value within that other range.
		/// (See http://www.sudokuslam.com/hints.html#intersection.)
		/// </summary>
		/// <returns>True if any possibilities were excluded in any range.</returns>
		public bool Intersections()
		{
			bool changed = false;

			for (int x = 1; x <= 9; ++x)
			{
				// We do this only for numbers which are not already filled in somewhere
				// in this grid.
				if (ContainsValue(x))
					continue;

				// Select the cells which could contain x.
				var cells = from kvp in this.Grid
							where kvp.Value.Possibilities[x] && !kvp.Value.IsSolved
							select kvp.Value;

				List<Group> ranges = Utilities.SelectCommonGroups(cells);

				// If our cells are part of a second range, then x can be excluded as a
				// possibility from any of the other cells of this second range.
				if (ranges.Count > 1)
				{
					if (false && Debugger != null)
					{
						foreach (Cell cell in cells)
						{
							Debugger.HighlightCell(cell.Coordinates.X, cell.Coordinates.Y);
						}
						Debugger.Hint = "Method: Intersections\n Value: " + x.ToString();
						Debugger.Show();
					}
					foreach (Group range in ranges)
						changed = range.RemovePossibility(x, cells) || changed;
				}
			}

			return changed;
		}
		#endregion

		/// <summary>
		/// Checks if any number appears twice or more in the grid.
		/// </summary>
		/// <returns>The lowest number which is in the grid more than once; 0 if none is found.</returns>
		public int Check()
		{
			for (int x = 1; x <= 9; ++x)
			{
				var q = from kvp in this.Grid
						where kvp.Value.Value == x
						select kvp.Value;

				if (q.Count() > 1)
					return x;
			}

			return 0;
		}

		public bool ContainsValue(int val)
		{
			for (int i = 0; i < Width; ++i)
				for (int j = 0; j < Height; ++j)
					if (this[i, j].Value == val)
						return true;

			return false;
		}

		public List<KeyValuePair<Location, Cell>> PossibleLocations(int x)
		{
			return Grid.Where(kvp => kvp.Value.Possibilities[x]).ToList();
		}
	}
}

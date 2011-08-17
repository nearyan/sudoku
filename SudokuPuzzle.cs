using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web.UI;

// TODO: Detailed checking for consistency after each iteration
// TODO: Add location information to groups for better debug information
// TODO: Write a generator

namespace Sudoku
{
	public class SudokuPuzzle : Range, ICloneable, IEquatable<SudokuPuzzle>
	{
		List<Group> _rows;
		List<Group> _columns;
		List<Group> _squares;
		List<Group> groups;
		IDebugger _debugger;
		Stopwatch _stopwatch = new System.Diagnostics.Stopwatch();
		bool _isCopy = false;
		bool _generating = false;
		bool mightBeDirty = false;
		bool isDirty = false;

		public SudokuPuzzle()
			: base(9, 9, true)
		{
			Initialize();
		}

		public void Initialize()
		{
			_rows = new List<Group>(9);
			_columns = new List<Group>(9);
			_squares = new List<Group>(9);
			groups = new List<Group>(27);

			for (int i = 0; i < 9; ++i)
			{
				Group square = new Square();
				Group row = new Row();
				Group column = new Column();

				Cell cell;
				for (int j = 0; j < 9; j++)
				{
					cell = this[j, i];
					cell.Coordinates = new Location(j, i);
					row[j, 0] = cell;
					cell.Row = row;
					
					cell = this[i, j];
					column[0, j] = cell;
					cell.Column = column;
					
					cell = this[3*(i%3) + j%3, 3*(i/3) + j/3];
					square[j % 3, j / 3] = cell;
					cell.Square = square;
				}

				row.Location = i;
				column.Location = i;
				square.Location = i;

				Rows.Add(row);
				Columns.Add(column);
				Squares.Add(square);
			}

			groups.AddRange(_columns);
			groups.AddRange(_rows);
			groups.AddRange(_squares);
		}

		#region Properties
		public List<Group> Rows
		{
			get { return _rows; }
			set { _rows = value; }
		}
		public List<Group> Columns
		{
			get { return _columns; }
			set { _columns = value; }
		}
		public List<Group> Squares
		{
			get { return _squares; }
			set { _squares = value; }
		}

		public bool IsSolved
		{
			get
			{
				for (int i = 0; i < 9; ++i)
					for (int j = 0; j < 9; ++j)
						if (this[i, j].Value == 0)
							return false;

				return true;
			}
		}

		public IDebugger Debugger
		{
			get { return _debugger; }
			set
			{
				_debugger = value;
				foreach (Group group in groups)
					group.Debugger = value;
				foreach (KeyValuePair<Location, Cell> kvp in Grid)
					kvp.Value.Debugger = value;
			}
		}

		public bool IsCopy
		{
			get { return _isCopy; }
			set { _isCopy = value; }
		}

		public bool Generating
		{
			get { return _generating; }
			set { _generating = value; }
		}

		public Stopwatch Stopwatch
		{
			get { return _stopwatch; }
		}
		#endregion

		public bool Solve()
		{
			string message;
			_stopwatch.Reset();
			_stopwatch.Start();

			try
			{
				bool changed = true;
				int iterations = 0;

				// Try the simpler methods until they fail. Then, try the complicated methods
				// once to get unstuck; then continue using simple methods again. If everything
				// fails, try to bruteforce it.
				while (changed && !IsSolved &&iterations < 50)
				{
					changed = false;

					QuickMethods();
					changed = ComplicatedMethods() || changed;

					message = CheckConsistency();
					if (message.Length > 0)
						throw new Exception(message);

					// Only try to brute force when this puzzle isn't already an attempt to do just that,
					// or we might run into infinite loops.
					if (!changed && !IsSolved && !IsCopy && !Generating)
					{
						mightBeDirty = true;
						changed = BruteForce(true);
					}
					++iterations;
				}

				_stopwatch.Stop();

				if (IsSolved && !_isCopy && mightBeDirty && !isDirty)
					System.Windows.Forms.MessageBox.Show("This solution was obtained by guessing a value. It is possible that the puzzle has more than one solutions.", "Brute Forced", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);

				message = CheckConsistency();
				if (message.Length > 0)
					throw new Exception(message);
				return this.IsSolved;
			}
			catch (Exception e)
			{
				_stopwatch.Stop();
				if (!_isCopy && !_generating)
				{
					message = CheckConsistency();
					if (message.Length > 0)
						System.Windows.Forms.MessageBox.Show(message, "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Exclamation);
					else
						System.Windows.Forms.MessageBox.Show("The following error occured during solving: \n" + e.ToString(), "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Exclamation);
					return false;
				}
				else
					throw e;
			}
		}

		/// <summary>
		/// Keep trying quicker solving methods untill we get stuck.
		/// </summary>
		void QuickMethods()
		{
			bool changed = true;

			while (changed)
			{
				changed = false;

				foreach (Group range in groups)
					changed = range.Scan() || changed;

				foreach (Cell cell in this.Grid.Select(kvp => kvp.Value))
						changed = cell.AttemptToSolve() || changed;
			}
		}

		/// <summary>
		/// Try complicated methods once.
		/// </summary>
		/// <returns>True if any of them was succesfull at excluding a possibility or filling in a value.</returns>
		bool ComplicatedMethods()
		{
			bool changed = false;

			foreach (Group range in _rows.Union(_columns))
				changed = range.Intersections() || changed;
			foreach (Group range in groups)
			    changed = range.NakedSets() || changed;
			changed = Colors() || changed;
			changed = XWing() || changed;
			changed = XYWing() || changed;
			if (!IsCopy && !changed)
				changed = BruteForce(false) || changed;

			return changed;
		}

		#region Solving methods
		/// <summary>
		/// See http://www.sudokuslam.com/hints.html#colors1 and 
		/// http://www.sudokuslam.com/hints.html#colors2.
		/// </summary>
		/// <returns>True if any possibilities were excluded.</returns>
		/// <seealso cref="SudokuPuzzle.Color1"/>
		/// <seealso cref="SudokuPuzzle.Color2"/>
		/// <seealso cref="Cell.FindNextColor"/>
		bool Colors()
		{
			bool changed = false;

			for (int x = 1; x <= 9; ++x)
			{
				Group group = null;
				Cell first = null;
				Cell second = null;
				int count = 2;

				foreach (Group g in groups)
				{
					// Find all possible locations for x.
					List<KeyValuePair<Location, Cell>> locations = g.PossibleLocations(x);

					// If there are two of them, start coloring them.
					if (locations.Count == 2)
					{
						group = g;
						first = locations.First().Value;
						second = locations.Last().Value;

						first.Color = Color.Green;
						second.Color = Color.Blue;

						count += first.FindNextColor(x, group);
						count += second.FindNextColor(x, group);

						changed = Color2(x) || changed;
						changed = Color1(x) || changed;

						// Clear all colors for any following iterations.
						foreach (KeyValuePair<Location, Cell> kvp in Grid)
							kvp.Value.Color = Color.Nothing;
					}
				}

			}

			return changed;
		}

		/// <summary>
		/// Helper function for <see cref="SudokuPuzzle.Colors"/>. After that function has done
		/// the coloring, this one sees if the second coloring algoritm can be applied.
		/// </summary>
		/// <param name="value"></param>
		/// <returns>True if any values were filled in; false otherwise.</returns>
		/// <seealso cref="SudokuPuzzle.Colors"/>
		bool Color2(int value)
		{	
			foreach (Group g in groups)
			{
				// Find all colored cells in group g.
				IEnumerable<KeyValuePair<Location, Cell>> coloredCells = g.Grid.Where(kvp => kvp.Value.Color != Color.Nothing);

				// If there are two or more of them, and their colors match, then they either all contain our value,
				// or none of them does. Therefore, they don't, and no cell in the entire grid with their color has
				// our value, while every cell with the opposing color does.
				if (coloredCells.Count() >= 2)
				{
					Color color = Color.Nothing;

					// If there are two colored cells in this group and their colors oppose, nothing special is the matter.
					if (coloredCells.Count() == 2 && coloredCells.First().Value.Color != coloredCells.Last().Value.Color)
						continue;

					// If we are still here and there are two colored cells, then their color agree.
					if (coloredCells.Count() == 2)
						color = coloredCells.First().Value.Color;

					// If there are n colored cells, then (n-1) of them must have the same color.
					if (coloredCells.Count() > 2)
					{
						if (g.Grid.Count(kvp => kvp.Value.Color == Color.Green) > 1)
							color = Color.Green;
						else
							color = Color.Blue;
					}

					if (false && Debugger != null)
					{
						Debugger.Hint = "Method: Color 2\nValue: " + value.ToString();
						Debugger.Show();
					}

					IEnumerable<KeyValuePair<Location, Cell>> allColoredCells = Grid.Where(kvp => kvp.Value.Color != Color.Nothing);
					foreach (KeyValuePair<Location, Cell> kvp in allColoredCells)
					{
						if (kvp.Value.Color == color)
							kvp.Value.Possibilities[value] = false;
						else if (kvp.Value.Color == Utilities.OppositeColor(color))
							kvp.Value.Value = value;
					}
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Helper function for <see cref="SudokuPuzzle.Colors"/>. After that function has done
		/// the coloring, this one sees if the first coloring algoritm can be applied.
		/// </summary>
		/// <param name="value"></param>
		/// <returns>True if any possibilities were excluded; false otherwise.</returns>
		/// <seealso cref="SudokuPuzzle.Colors"/>
		public bool Color1(int value)
		{
			bool changed = false;

			// Find all pairs of cells which have no group in common, and which have two different colors.
			IEnumerable<Cell> greenCells = Grid.Where(kvp => kvp.Value.Color == Color.Green).Select(kvp => kvp.Value);
			IEnumerable<Cell> blueCells = Grid.Where(kvp => kvp.Value.Color == Color.Blue).Select(kvp => kvp.Value);
			var q = from greenCell in greenCells
					from blueCell in blueCells
					where greenCell.Row != blueCell.Row
						&& greenCell.Square != blueCell.Square
						&& greenCell.Column != blueCell.Column
					select new { First = greenCell, Second = blueCell };


			foreach (var pair in q)
			{
				// Find all intersections of all groups of both our cells.
				IEnumerable<Cell> cells = pair.First.Buddies.Intersect(pair.Second.Buddies).Where(cell => !cell.IsSolved);

				if (false && Debugger != null)
				{
					foreach (Cell cell in cells)
						Debugger.HighlightCell(cell.Coordinates.X, cell.Coordinates.Y);
					if (cells.Count() > 0)
					{
						Debugger.Hint = "Method: Color 1\nValue: " + value.ToString();
						Debugger.Show();
					}
				}

				foreach (Cell cell in cells)
				{
					// This cell is contained in two groups, both of which contain a colored
					// cell. The colors of these two cells oppose. Therefore, in one of
					// them is our value, excluding it in our current cell.
					if (cell.Possibilities[value])
					{
						cell.Possibilities[value] = false;
						changed = true;
					}
				}
			}

			return changed;
		}

		/// <summary>
		/// <para>For each possible value, we find a row which has only two possible locations
		/// for it. Call the first location A; the other one B. Then we find another such row
		/// for this value; call the first location of this second row C; the other one D.</para>
		/// <para>If it happens that A is in the same column as C, and B is in the same column
		/// as D, then our value cannot be in A and C simultaneously, or in B and D simultaneously.
		/// Therefore, our value is either in A and D, or in B and C. Either way, any cells which
		/// are on the edge of the rectangle defined by ABCD may not contain our value.</para>
		/// <para>(See http://www.sudokuslam.com/hints.html#xwing.)</para>
		/// </summary>
		/// <returns>True if any possibilities were excluded; false otherwise.</returns>
		public bool XWing()
		{
			bool changed = false;
			List<Group> candidateGroups;

			// We do this for rows and columns.
			List<List<Group>> groups = new List<List<Group>>(2);
			groups.Add(_rows);
			groups.Add(_columns);

			foreach (List<Group> group in groups)
			{
				for (int x = 1; x <= 9; ++x)
				{
					candidateGroups = new List<Group>();
					foreach (Group g in group)
						if (g.PossibleLocations(x).Count == 2)
							candidateGroups.Add(g);

					if (candidateGroups.Count != 2)
						continue;

					Cell a = candidateGroups.First().PossibleLocations(x).First().Value;
					Cell b = candidateGroups.First().PossibleLocations(x).Last().Value;
					Cell c = candidateGroups.Last().PossibleLocations(x).First().Value;
					Cell d = candidateGroups.Last().PossibleLocations(x).Last().Value;

					if (a.Coordinates.X == c.Coordinates.X && b.Coordinates.X == d.Coordinates.X)
					{
						if (false && Debugger != null)
						{
							a.Color = Color.Blue;
							d.Color = Color.Blue;
							c.Color = Color.Green;
							b.Color = Color.Green;
							Debugger.Hint = "Method: XWing\nValue: " + x;
							Debugger.Show();
						}

						changed = a.Row.RemovePossibility(x, new List<Cell> { a, b }) || changed;
						changed = c.Row.RemovePossibility(x, new List<Cell> { c, d }) || changed;
						changed = a.Column.RemovePossibility(x, new List<Cell> { a, c }) || changed;
						changed = b.Column.RemovePossibility(x, new List<Cell> { b, d }) || changed;
					}
				}
			}


			return changed;
		}

		/// <summary>
		/// <para>First, we look for any cell which has two possible values. One we call x; the other y.</para>
		/// <para>Then, we find any buddies of our cell which has two possible values again, of which one equals
		/// x and the other does not equal y. The other value we call z. Then we do the same, but now we look for
		/// cells which may contain y but not x. If any pair of cells which match these requirements have the same
		/// value for z, then there are two possibilities:</para>
		/// <list type="bullet">
		/// <item><description>Our first cell contains x. Then the x-cell may not contain x; therefore it contains z.</description></item>
		/// <item><description>Our first cell contains y. Then the y-cell may not contain y; therefore it contains z.</description></item>
		/// </list>
		/// <para>So z is in either the x-cell or the y-cell. Therefore, cells which are buddies of both the x-cell 
		/// and the y-cell may not contain z.</para>
		/// <para>(See http://www.sudokuslam.com/hints.html#xywing and 
		/// http://www.brainbashers.com/sudokuxywing.asp.)</para>
		/// </summary>
		/// <returns>True if any possibilities were excluded.</returns>
		public bool XYWing()
		{
			bool changed = false;

			// Find cells which have two possible values. One we call x, the other y.
			foreach (KeyValuePair<Location, Cell> kvp in this.Grid)
			{
				Cell cell = kvp.Value;
				if (cell.Possibilities.Count == 2)
				{
					int x = cell.Possibilities.Where(k => k.Value).First().Key;
					int y = cell.Possibilities.Where(k => k.Value).Last().Key;

					// Find any buddy of our cell which has two possible values again, of which exactly
					// one is x. Do the same for y.
					var xCells = from c in cell.Buddies
								 where c.Possibilities[x] && !c.Possibilities[y] && c.Possibilities.Count == 2
								 select c;
					var yCells = from c in cell.Buddies
								 where !c.Possibilities[x] && c.Possibilities[y] && c.Possibilities.Count == 2
								 select c;

					for (int i = 0; i < xCells.Count(); ++i)
					{
						// Store the other possible value of xCells[i]; call this z.
						Cell first = xCells.ElementAt(i);
						int z = first.Possibilities.Where(k => k.Value && k.Key != x).First().Key;
						for (int j = 0; j < yCells.Count(); ++j)
						{
							Cell second = yCells.ElementAt(j);
							if (second.Possibilities.Where(k => k.Value && k.Key != x).First().Key == z)
							{
								// So the other possible value for yCells[j] also turns out to be z. Then,
								// if our original cell turns out to have value x, then xCells[i] must be z.
								// On the other hand, if our original cell is y, then yCells[j] must be z.
								// Either way, cells which are buddies of both xCells[i] and yCells[j] cannot
								// contain z.

								if (false && Debugger != null)
								{
									cell.Color = Color.Red;
									first.Color = Color.Green;
									second.Color = Color.Blue;
									Debugger.Hint = "Method: XYWing";
									Debugger.Show();
								}

								IEnumerable<Cell> cells = first.Buddies.Intersect(second.Buddies).
									Where(c => c != first && c != second && c != cell);
								foreach (Cell c in cells)
								{
									if (c.Possibilities[z])
									{
										c.Possibilities[z] = false;
										changed = true;
									}
								}
							}
						}
					}
				}
			}

			return changed;
		}

		/// <summary>
		/// <para>Try to brute force the puzzle. In all cells which have two possible values, we fill in
		/// each one, and see what happens.</para>
		/// <para>When filling in a value leads to a contradiction (e.g. another cell turns out to have no
		/// possible values, or there is no possible location for value x in group y), then we may conclude with
		/// certainty that we filled in the wrong value. If, on the other hand, we manage to fill the entire
		/// grid if we guess a value, then we cannot be sure if this is the only solution. This is because
		/// we do not try every possible value in every non-filled cell; instead, this method only focuses
		/// on cells which have two possible values.</para>
		/// </summary>
		/// <param name="guess">Indicates if we should guess a value if no contradictions follow from
		/// trying a possibility.</param>
		/// <returns>True if any cells were filled in, or if the puzzle was solved and only one solution
		/// was found; false otherwise.</returns>
		public bool BruteForce(bool guess)
		{
			IEnumerable<Cell> cells = from kvp in Grid
									  where kvp.Value.Possibilities.Count == 2
									  select kvp.Value;
			List<SudokuPuzzle> solvedPuzzles = new List<SudokuPuzzle>();

			// Loop over cells which have only two possible values.
			foreach (Cell cell in cells)
			{
				SudokuPuzzle clone;

				IEnumerable<int> values = cell.Possibilities.Where(kvp => kvp.Value).Select(kvp => kvp.Key);
				foreach (int val in values)
				{
					 clone = (SudokuPuzzle)this.Clone();
					 clone.IsCopy = true;

					// Pick one of the values, and fill it in. Then three things can happen:
					// - We run into a contradiction. In this case, this value apparently cannot be in this cell,
					//   so we can exclude it. This means the cell can only have the other value.
					// - We solve the puzzle.
					// - We do not solve the puzzle, without running into contradictions. In this case, first
					//   try the other value; then do the same for other cells which have two possible values.
					clone[cell.Coordinates].Value = val;
					try
					{
						if (clone.Solve() && !solvedPuzzles.Contains(clone)) // We solved it; save it for later examination.
							solvedPuzzles.Add(clone);
					}
					catch // We ran into a contradiction; fill in the other value.
					{
						cell.Possibilities[val] = false;
						// We know that cell.Possibilities[val] was true, because it was selected by the query at the top
						// of the function, so we're certain something changed.
						return true;
					}
				}
			}

			if (!guess)
				return false;

			// We're still here; then we didn't return in the catch-block and we're supposed to pick a solution.
			if (solvedPuzzles.Count == 1)
			{
				SudokuPuzzle puzzle = solvedPuzzles.First();
				IEnumerable<Cell> unsolvedCells = Grid.Where(kvp => !kvp.Value.IsSolved).Select(kvp => kvp.Value);
				foreach (Cell c in unsolvedCells)
					c.Value = puzzle[c.Coordinates].Value;
				return true;
			}
			if (solvedPuzzles.Count > 1) // There are multiple distinct solutions
			{
				isDirty = true;
				_stopwatch.Stop();
				System.Windows.Forms.MessageBox.Show("The puzzle has at least " + solvedPuzzles.Count + " solutions:", "Multiple solutions",
					System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Exclamation);
				// Show each puzzle to the user.
				foreach (SudokuPuzzle puzzle in solvedPuzzles)
				{
					DebugForm form = new DebugForm();
					form.sudokuGrid1.Puzzle = puzzle;
					form.Show();
				}
				_stopwatch.Start();
				return false;
			}

			// No solutions found, and no possibilities excluded. We're doomed.
			return false;
		}
		#endregion

		public bool Fill(string s)
		{
			return Fill(new List<string>(s.Split(new char[] { ' ' })));
		}

		/// <summary>
		/// Fill the grid with the specified numbers.
		/// </summary>
		/// <param name="data">A list of nine strings, each of which consists of nine numbers
		/// concatenated. Use zero for empty cells.</param>
		/// <returns>True if succesfull, false otherwise.</returns>
		public bool Fill(List<string> data)
		{
			try
			{
				if (data.Count != 9)
					throw new Exception("Invalid number of strings.");

				for (int j = 0; j < 9; ++j)
				{
					// Get the current string.
					string s = data[j];

					// Split the string.
					char[] numbers = s.ToCharArray();
					if (numbers.Count() != 9)
						throw new Exception("Invalid number of digits in string '" + s + "'.");

					for (int i = 0; i < 9; ++i)
						// Convert the char to a string; then convert the resulting string to a number.
						// Put this inside the cell at the proper location.
						Grid[new Location(i, j)].Value = int.Parse(numbers[i].ToString());
				}
			}
			catch
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// Checks if the current puzzle contains any of the following impossibilities:
		/// <list type="bullet">
		/// <item><description>A number occurs more than once in a group.</description></item>
		/// <item><description>A number has no possible location in a group.</description></item>
		/// <item><description>A cell has no possible values.</description></item>
		/// </list>
		/// </summary>
		/// <returns>A string containing an error message if an impossibility was found; otherwise, the empty string.</returns>
		public string CheckConsistency()
		{
			foreach (Group group in groups)
			{
				int c = group.Check();
				if (c != 0)
					return "Number " + c.ToString() + " occurs more than once in " + group.GetType().Name + " " + group.Location + ".";

				for (int x = 1; x <= 9; ++x)
					if (group.PossibleLocations(x).Count == 0)
						return "Number " + x + " has no possible location in " + group.GetType().Name + " " + group.Location + ".";
			}

			foreach (Cell cell in this.Grid.Select(kvp => kvp.Value))
				if (cell.Possibilities.Count == 0)
					return "Cell " + cell.Coordinates.ToString() + " has no possible values.";

			return "";
		}

		/// <summary>
		/// Clones the current instance.
		/// </summary>
		/// <returns>A new puzzle with the same values and possible values for all cells.</returns>
		public object Clone()
		{
			SudokuPuzzle puzzle = new SudokuPuzzle();
			for (int j = 0; j < 9; ++j)
				for (int i = 0; i < 9; ++i)
				{
					puzzle[i, j].Value = this[i, j].Value;
					puzzle[i, j].Possibilities = (Possibilities)this[i, j].Possibilities.Clone();
				}

			puzzle.Debugger = Debugger;
			return puzzle;
		}

		public SudokuPuzzle CloneExcept(Location l)
		{
			SudokuPuzzle puzzle = new SudokuPuzzle();

			foreach (Cell cell in this.Grid.Where(kvp => !(kvp.Key.X == l.X && kvp.Key.Y == l.Y)).Select(kvp => kvp.Value))
			{
				puzzle[cell.Coordinates].Value = cell.Value;
			}

			return puzzle;
		}

		/// <summary>
		/// Checks if a puzzle equals the current instance.
		/// </summary>
		/// <param name="other">The puzzle to check.</param>
		/// <returns>True if all values of all cells of the other puzzle equal those of this one.
		/// Possible values for cells which have no value are not checked.</returns>
		public bool Equals(SudokuPuzzle other)
		{
			if (other == null)
				return false;

			foreach (Location location in this.Grid.Select(kvp => kvp.Key))
				if (this[location].Value != other[location].Value)
					return false;

			return true;
		}

		/// <summary>
		/// Picks a random cell from the argument. The less filled-in buddies a cell has
		/// the higher the chance it gets chosen.
		/// </summary>
		/// <param name="attempt">The puzzle to pick a cell from.</param>
		/// <returns>The cell.</returns>
		static Cell GetNextCell(SudokuPuzzle attempt, Random r)
		{
			var unsolvedCells = (from kvp in attempt.Grid
								 let weight = 25 - kvp.Value.Buddies.Count(c => c.IsSolved)
								 where !kvp.Value.IsSolved
								 orderby weight descending
								 select new { Cell = kvp.Value, Weight = weight }).ToList();

			int sum = unsolvedCells.Sum(a => a.Weight);

			int random = r.Next(sum);
			int i = -1;

			while (sum > random)
			{
				sum -= unsolvedCells.ElementAt(i + 1).Weight;
				++i;
			}

			return unsolvedCells.ElementAt(i).Cell;
		}

		public static void RemoveUnnecessaryValues(SudokuPuzzle puzzle)
		{
			List<Location> locations = puzzle.Grid.Where(kvp => kvp.Value.IsSolved).Select(kvp => kvp.Key).Shuffle().ToList();

			SudokuPuzzle attempt;

			foreach (Location location in locations)
			{
				attempt = puzzle.CloneExcept(location);
				attempt.Generating = true;
				try
				{
					if (attempt.Solve())
						puzzle[location].Value = 0;
				}
				catch { }
			}
		}

		public static SudokuPuzzle Generate()
		{
			SudokuPuzzle current = new SudokuPuzzle();
			Random r = new Random();

			while (true)
			{
				SudokuPuzzle attempt = (SudokuPuzzle)current.Clone();

				Cell cell = GetNextCell(attempt, r);

				// Pick a random value and fill it in.
				IEnumerable<int> possibleValues = from kvp in cell.Possibilities
												  where kvp.Value
												  select kvp.Key;
				int val = possibleValues.ElementAt(r.Next(possibleValues.Count()));
				cell.Value = val;

				// If we have less than 17 hints, then don't try to see if we can solve it -
				// no puzzle with less than 17 hints is known to exist,
				if (attempt.Grid.Count(kvp => kvp.Value.IsSolved) < 17)
				{
					current = attempt;
					continue;
				}

				try
				{
					SudokuPuzzle temp = (SudokuPuzzle)attempt.Clone();
					temp.Generating = true;
					if (temp.Solve())
					{
						RemoveUnnecessaryValues(attempt);
						return attempt;
					}
					else
						current = attempt;
				}
				catch { }
			}
		}
	}
}

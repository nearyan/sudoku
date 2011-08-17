using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sudoku
{
	public class Cell
	{
		int _value = 0;
		Location _coordinates;
		Group _row;
		Group _column;
		Group _square;
		List<Cell> _buddies = null;
		Color _color = Color.Nothing;
		Possibilities _possibilities = new Possibilities();
		IDebugger _debugger;

		public Cell(int value)
			: this()
		{
			Value = value;
		}

		public Cell()
		{
			for (int i = 0; i < 9; ++i)
				_possibilities[i + 1] = true;
		}

		public int Value
		{
			get { return _value; }
			set
			{
				int oldValue = _value;
				_value = value;

				// If the new value does not equal zero, remove it from the possible values
				// of our buddies.
				if (value != 0)
				{
					for (int i = 1; i <= 9; ++i)
						_possibilities[i] = value == i;
					Row.RemovePossibility(value);
					Column.RemovePossibility(value);
					Square.RemovePossibility(value);
				}
				// If the new value _does_ equal zero, and the old value doesn't, then add
				// the old value to the possible values of our buddies when appropriate.
				else if (oldValue != 0)
					foreach (Cell cell in Buddies)
						if (cell.Buddies.Count(c => c.Value == oldValue) == 0)
							cell.Possibilities[oldValue] = true;
			}
		}

		public Location Coordinates
		{
			get { return _coordinates; }
			set { _coordinates = value; }
		}

		public bool IsSolved
		{
			get { return Value != 0; }
		}

		public Group Row
		{
			get { return _row; }
			set { _row = value; }
		}
		public Group Column
		{
			get { return _column; }
			set { _column = value; }
		}
		public Group Square
		{
			get { return _square; }
			set { _square = value; }
		}

		public List<Cell> Buddies
		{
			get
			{
				if (_buddies == null)
				{
					_buddies = new List<Cell>(_row.ToList());
					_buddies.AddRange(_column.ToList());
					_buddies.AddRange(_square.ToList());
				}
				return _buddies;
			}
		}

		public Possibilities Possibilities
		{
			get { return _possibilities; }
			set { _possibilities = value; }
		}

		public Color Color
		{
			get { return _color; }
			set
			{
				_color = value;
				if (Debugger != null)
					Debugger.HighlightCell(_coordinates.X, _coordinates.Y, value);
			}
		}

		public IDebugger Debugger
		{
			get { return _debugger; }
			set { _debugger = value; }
		}

		public override string ToString()
		{
			return Coordinates.ToString() + ": " + Value.ToString();
		}

		/// <summary>
		/// If the cell has only one possible value, fill it in.
		/// </summary>
		/// <returns>True if succesfull, false otherwise.</returns>
		public bool AttemptToSolve()
		{
			// A cell can be solved only once.
			if (IsSolved)
				return false;

			if (Possibilities.Count(kvp => kvp.Value) == 1)
			{
				Value = Possibilities.First(kvp => kvp.Value).Key;
				return true;
			}

			return false;
		}

		/// <summary>
		/// Helper function for <see cref="SudokuPuzzle.Colors"/>. If this function
		/// is called on an uncolored cell, or on a cell which may not contain the
		/// argument, an exception is thrown.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="group"></param>
		/// <returns>The number of cells which received a color.</returns>
		/// <seealso cref="SudokuPuzzle.Colors"/>
		public int FindNextColor(int value, Group group)
		{
			int count = 0;
			List<Group> groups = new List<Group>(3);
			if (_square != group)
				groups.Add(_square);
			if (_row != group)
				groups.Add(_row);
			if (_column != group)
				groups.Add(_column);

			if (this.Color == Color.Nothing)
				throw new Exception("Call this function only on colored cells.");
			if (!this.Possibilities[value])
				throw new Exception("This cell may not contain the value " + value.ToString() + ".");

			foreach (Group g in groups)
			{
				// Find other cells which may contain our value...
				var q = from kvp in g.Grid
						where kvp.Value.Possibilities[value] && !kvp.Value.IsSolved && kvp.Value != this
						select kvp.Value;

				// If there is only one of those, then it receives the opposite color
				// than the one of this instance.
				if (q.Count() == 1)
				{
					// Find the other cell.
					Cell cell = q.First();
					
					// If we are in a group where there are two possible locations for our value,
					// and their colors match, then they either both contain our value, or neither
					// of them does. This is an impossibility.
					System.Diagnostics.Debug.Assert(cell.Color != Color);

					cell.Color = Utilities.OppositeColor(Color);
					++count;

					if (cell.Color == Color.Nothing)
						count += cell.FindNextColor(value, g);
				}
			}

			return count;
		}
	}
}

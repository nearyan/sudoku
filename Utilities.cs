using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Sudoku
{
	/// <summary>
	/// The coordinates for a cell.
	/// </summary>
	public struct Location
	{
		int _x, _y;

		public Location(int x, int y)
		{
			_x = x;
			_y = y;
		}

		public int X
		{
			get { return _x; }
			set { _x = value; }
		}

		public int Y
		{
			get { return _y; }
			set { _y = value; }
		}

		public override int GetHashCode()
		{
			return 9 * Y + X;
		}

		public override bool Equals(object obj)
		{
			Location cell;

			try
			{
				cell = (Location)obj;
			}
			catch
			{
				return false;
			}

			if (cell.X == X && cell.Y == Y)
				return true;

			return false;
		}

		public override string ToString()
		{
			return "(" + X + ", " + Y + ")";
		}
	}

	/// <summary>
	/// Colors for a cell. Used for <see cref="SudokuPuzzle.Colors"/> and for the debug grid.
	/// </summary>
	public enum Color
	{
		Nothing,
		Green,
		Blue,
		Red
	}

	/// <summary>
	/// Represents a list of possible values for a cell.
	/// </summary>
	public class Possibilities : Dictionary<int, bool>, ICloneable
	{
		public override string ToString()
		{
			var q = from kvp in this
					where kvp.Value
					select kvp.Key.ToString();

			return string.Join(", ", q.ToArray());
		}

		/// <summary>
		/// Returns the number of possible elements which have true as value.
		/// </summary>
		public new int Count
		{
			get
			{
				return this.Count(kvp => kvp.Value);
			}
		}

		public object Clone()
		{
			Possibilities p = new Possibilities();
			foreach (KeyValuePair<int, bool> kvp in this)
				p[kvp.Key] = kvp.Value;

			return p;
		}
	}

	public static class Utilities
	{
		public static Color OppositeColor(Color color)
		{
			if (color == Color.Blue)
				return Color.Green;
			else
				return Color.Blue;
		}

		/// <summary>
		/// Shuffles the current instance to a random order.
		/// </summary>
		/// <typeparam name="T">The type.</typeparam>
		/// <param name="toShuffle">The list to shuffle.</param>
		/// <returns>A new shuffled list.</returns>
		public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> toShuffle)
		{
			Random random = new Random();
			return toShuffle.OrderBy(n => random.Next());
		}

		/// <summary>
		/// Performs a logical or-operation on each of the elements of the arguments, and
		/// returns the result.
		/// </summary>
		/// <param name="operands">A list of Dictionary's which should be or'd together.</param>
		/// <returns>The result.</returns>
		public static Possibilities Or(IEnumerable<Possibilities> operands)
		{
			Possibilities first = operands.First();

			// Create the result dictionary with the appropriate capacity. We assume
			// the number of elements in each of the operands equal.
			Possibilities result = new Possibilities();

			foreach (KeyValuePair<int, bool> kvp in first)
			{
				bool val = false;
				foreach (Dictionary<int, bool> operand in operands)
					val = val || operand[kvp.Key];
				result[kvp.Key] = val;
			}

			return result;
		}

		/// <summary>
		/// Returns a list of groups that the arguments have in common.
		/// </summary>
		/// <param name="cells">The cells.</param>
		/// <returns>The groups the cells have in common.</returns>
		public static List<Group> SelectCommonGroups(IEnumerable<Cell> cells)
		{
			List<Group> result = new List<Group>(3);

			Group row = cells.First().Row;
			Group column = cells.First().Column;
			Group square = cells.First().Square;
			bool rowEquals = true;
			bool columnEquals = true;
			bool squareEquals = true;

			foreach (Cell cell in cells)
			{
				if (row != cell.Row)
					rowEquals = false;
				if (column != cell.Column)
					columnEquals = false;
				if (square != cell.Square)
					squareEquals = false;
			}

			if (rowEquals)
				result.Add(row);
			if (columnEquals)
				result.Add(column);
			if (squareEquals)
				result.Add(square);

			return result;
		}
	}

	/// <summary>
	/// Represents a way of communicating to the user which cells are being targeted
	/// by some solving algorithm. Implementers can highlight cells, and show textual
	/// tips. They should at least notify the user of their own name.
	/// </summary>
	public interface IDebugger
	{
		/// <summary>
		/// Highlight a particular cell with a certain color.
		/// </summary>
		/// <param name="x">The x-coordinate of the cell.</param>
		/// <param name="y">The y-coordinate of the cell.</param>
		/// <param name="color">The color that the background of the cell should be.</param>
		void HighlightCell(int x, int y, Color color);

		void HighlightCell(int x, int y);

		/// <summary>
		/// Show the information to the user.
		/// </summary>
		void Show();

		/// <summary>
		/// Indicates if the debugger gets shown or not.
		/// </summary>
		bool Active
		{
			get;
			set;
		}

		/// <summary>
		/// The stopwatch that should be stopped while the form is visible.
		/// </summary>
		Stopwatch Stopwatch
		{
			get;
			set;
		}

		/// <summary>
		/// A textual hint for the user.
		/// </summary>
		string Hint
		{
			get;
			set;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sudoku
{
	/// <summary>
	/// An arbitrary rectangular cluster of cells.
	/// </summary>
	public class Range
	{
		Dictionary<Location, Cell> _grid;
		int _width;
		int _height;
		RangeType _type;
		int _location;

		#region Properties
		public Cell this[int x, int y]
		{
			get { return this[new Location(x, y)]; }
			set { this[new Location(x, y)] = value; }
		}

		public Dictionary<Location, Cell> Grid
		{
			get { return _grid; }
		}

		public Cell this[Location l]
		{
			get { return _grid[l]; }
			set { _grid[l] = value; }
		}

		public int Width
		{
			get { return _width; }
			set { _width = value; }
		}

		public int Height
		{
			get { return _height; }
			set { _height = value; }
		}

		public RangeType Type
		{
			get { return _type; }
			set { _type = value; }
		}

		public int Location
		{
			get { return _location; }
			set { _location = value; }
		}
		#endregion

		public Range(int width, int height, bool initialize)
		{
			_width = width;
			_height = height;
			_grid = new Dictionary<Location, Cell>(_width * _height);

			if (initialize)
			{
				for (int i = 0; i < width; ++i)
					for (int j = 0; j < height; ++j)
						this[i, j] = new Cell(0);
			}
		}

		public IEnumerable<Cell> ToList()
		{
			return this._grid.Select(kvp => kvp.Value);
		}

		public bool ContainsCell(Cell cell)
		{
			return ContainsCell(cell.Coordinates);
		}

		public bool ContainsCell(Location location)
		{
			return this._grid.Where(kvp => kvp.Value.Coordinates.Equals(location)).Count() == 1;
		}

		/// <summary>
		/// Returns a list of cells which are contained both in the current range
		/// and in the argument.
		/// </summary>
		/// <param name="range">The other range.</param>
		/// <returns>The list of cells.</returns>
		public List<Cell> Intersection(Range range)
		{
			List<Cell> result = new List<Cell>();

			foreach (KeyValuePair<Location, Cell> kvp in this._grid)
				if (range.ContainsCell(kvp.Value))
					result.Add(kvp.Value);

			return result;
		}

		public bool RemovePossibility(int val)
		{
			return RemovePossibilities(new List<int> { val }, new List<Cell>(0));
		}

		public bool RemovePossibility(int val, IEnumerable<Cell> exceptions)
		{
			return RemovePossibilities(new List<int> { val }, exceptions);
		}

		/// <summary>
		/// Removes possible values from the range.
		/// </summary>
		/// <param name="possibilities">The possibilities.</param>
		/// <param name="exceptions">Cells which we should not touch. (Cells which are already solved
		/// are also not touched.)</param>
		/// <returns>True if any possibilities were actually removed.</returns>
		public bool RemovePossibilities(IEnumerable<int> possibilities, IEnumerable<Cell> exceptions)
		{
			bool changed = false;

			foreach (KeyValuePair<Location, Cell> kvp in this.Grid)
			{
				if (exceptions.Contains(kvp.Value) || kvp.Value.IsSolved)
					continue;

				foreach (int i in possibilities)
				{
					if (kvp.Value.Possibilities[i])
					{
						kvp.Value.Possibilities[i] = false;
						changed = true;
					}
				}
			}

			return changed;
		}

		public enum RangeType
		{
			Row,
			Column,
			Square
		}
	}
}

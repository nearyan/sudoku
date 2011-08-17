using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sudoku
{
	class Row : Group
	{
		public Row()
			: base(9, 1, true)
		{ }
	}

	class Column : Group
	{
		public Column()
			: base(1, 9, true)
		{ }
	}

	class Square : Group
	{
		public Square()
			: base(3, 3, true)
		{ }
	}
}

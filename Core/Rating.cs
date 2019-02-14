using System;
using System.Collections.Generic;
using System.Text;

namespace Core
{
	public struct Rating
	{
		public int Count { get; }
		public double Average { get; }

		internal Rating(int count, double average)
		{
			Count = count;
			Average = average;
		}
	}
}

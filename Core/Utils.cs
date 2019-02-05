using System;
using System.Collections.Generic;
using System.Text;

namespace Core
{
	static class Utils
	{
		public static IEnumerable<T> SingleEnumerable<T>(T value)
		{
			return new T[] { value };
		}
	}
}

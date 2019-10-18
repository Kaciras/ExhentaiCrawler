using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Core.Infrastructure;

namespace Core
{
	public static class Utils
	{
		public static IEnumerable<T> SingleEnumerable<T>(T value) => new T[] { value };

		public static void ForEach<T>(this IEnumerable<T> enumable, Action<T> action)
		{
			foreach (var item in enumable) action(item);
		}

		public static void ForEach<T>(this IEnumerable<T> enumable, Action<T, int> action)
		{
			var index = 0;
			foreach (var item in enumable) action(item, index++);
		}
	}
}

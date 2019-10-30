using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Core.Infrastructure;

namespace Core
{
	public static class Utils
	{
		private static readonly Regex DOMAIN = new Regex(
			@"^e[\-x]hentai\.org$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		internal static string ExtactEhentaiUriPath(string @string)
		{
			if (@string.Length == 0)
			{
				throw new UriFormatException("空URI字符串");
			}
			if (@string[0] == '/')
			{
				return @string;
			}

			var parsed = new Uri(@string);
			if (DOMAIN.IsMatch(parsed.Host))
			{
				return parsed.AbsolutePath;
			}
			throw new UriFormatException("该URI不是E绅士的网站");
		}

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

﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Core.Infrastructure;

namespace Core
{
	internal static class Utils
	{
		public static IEnumerable<T> SingleEnumerable<T>(T value)
		{
			return new T[] { value };
		}

		/// <summary>
		/// 解析表示范围的字符串，其格式级意义如下（XY都是非负整数）：
		///		X : 只有第X个
		///		X-Y : 第X到Y个
		///		X- : 第X个到末尾
		///		-Y : 开头到第Y个
		///		- : 全部范围
		/// </summary>
		/// <param name="string">范围字符串</param>
		/// <returns>范围(起始，结束)</returns>
		public static (int?, int?) ParseRange(string @string)
		{
			if (string.IsNullOrEmpty(@string))
			{
				throw new ArgumentException("范围字符串不能为null或空串");
			}

			var match = Regex.Match(@string, @"^(\d*)-(\d*)$");
			if (match.Success)
			{
				var start = StringToNullableInt(match.Groups[1].Value);
				var end = StringToNullableInt(match.Groups[2].Value);
				return (start, end);
			}
			else if (int.TryParse(@string, out var index))
			{
				return (index, index);
			}
			else
			{
				throw new ArgumentException("页码范围参数错误：" + @string);
			}
		}

		// 不能用三元运算，因为null和int不兼容，虽然返回值两个都兼容
		private static int? StringToNullableInt(string @string)
		{
			if (string.IsNullOrEmpty(@string))
			{
				return null;
			}
			else
			{
				return int.Parse(@string);
			}
		}
	}
}

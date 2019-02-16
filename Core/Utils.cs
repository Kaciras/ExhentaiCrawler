using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Core
{
	internal static class Utils
	{
		private static readonly char[] SIZE_UNITS = { 'K', 'M', 'G', 'T', 'P', 'E' };

		private static readonly Regex SIZE_TEXT = new Regex(@"([0-9.]+)\s*([A-Z]?)i?B?", RegexOptions.IgnoreCase);

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

		/// <summary>
		/// 转换如 xx.xx MB 这样的大小字符串为数值。
		/// </summary>
		/// <param name="string">表示大小的字符串</param>
		/// <param name="targetUnit">返回数值的单位，null表示字节</param>
		/// <returns>大小数值</returns>
		public static double ParseSize(string @string, char? targetUnit = null)
		{
			var match = SIZE_TEXT.Match(@string);
			if (!match.Success)
			{
				throw new ArgumentException("无法识别的大小文本：" + @string);
			}

			var unit = match.Groups[2].Value;
			var level = -1;

			if (unit.Length > 0)
			{
				level = Array.IndexOf(SIZE_UNITS, char.ToUpper(unit[0]));
				if (level == -1)
				{
					throw new ArgumentException("无法识别的大小单位：" + unit[0]);
				}
			}

			var targetLevel = -1; // 这段跟上面的好像可以提取个公共的方法
			if (targetUnit.HasValue)
			{
				targetLevel = Array.IndexOf(SIZE_UNITS, char.ToUpper(targetUnit.Value));
				if (targetLevel == -1)
				{
					throw new ArgumentException("无法识别的目标单位" + targetUnit);
				}
			}

			return double.Parse(match.Groups[1].Value) * Math.Pow(1024, level - targetLevel);
		}
	}
}

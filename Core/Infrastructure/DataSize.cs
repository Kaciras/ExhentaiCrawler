using System;
using System.Text.RegularExpressions;

namespace Core.Infrastructure
{
	public enum SizeUnit
	{
		Bytes,
		KB, MB, GB, TB, PB, EB
	}

	public readonly struct DataSize
	{
		private static readonly char[] SIZE_UNITS = { 'K', 'M', 'G', 'T', 'P', 'E' };

		private static readonly Regex SIZE_TEXT = new Regex(@"([+-]?[0-9.]+)\s*([A-Z]?)i?B?", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		public long Bytes { get; }

		public DataSize(long bytes)
		{
			Bytes = bytes;
		}

		public DataSize(double value, SizeUnit unit)
		{
			Bytes = (long)(value * Math.Pow(1024, (double)unit));
		}

		/// <summary>
		/// 获取该大小量在指定单位下的数值。
		/// </summary>
		/// <param name="unit">单位</param>
		/// <returns>数值</returns>
		public double OfUnit(SizeUnit unit) => ToDimension((double)unit);

		private double ToDimension(double dimension)
		{
			return Bytes / Math.Pow(1024, dimension);
		}

		/// <summary>
		/// 转换如 xx.xx MB 这样的字符串为大小。
		/// </summary>
		/// <param name="string">表示大小的字符串</param>
		/// <param name="targetUnit">返回数值的单位，null表示字节</param>
		/// <returns>大小数值</returns>
		/// <exception cref="FormatException">如果无法解析输入的字符串</exception>
		public static DataSize Parse(string @string)
		{
			var match = SIZE_TEXT.Match(@string);
			if (!match.Success)
			{
				throw new FormatException("无法识别的大小文本：" + @string);
			}

			var unit = match.Groups[2].Value;
			var level = 0;

			if (unit.Length > 0)
			{
				level = Array.IndexOf(SIZE_UNITS, char.ToUpper(unit[0])) + 1;
				if (level == 0)
				{
					throw new FormatException("无法识别的大小单位：" + unit[0]);
				}
			}

			var bytes = double.Parse(match.Groups[1].Value) * Math.Pow(1024, level);
			return new DataSize((long)Math.Round(bytes));
		}

		public static double ConvertUnit(double value, SizeUnit origin, SizeUnit target)
		{
			return new DataSize(value, origin).OfUnit(target);
		}

		public override string ToString()
		{
			var dim = (int)Math.Log(Bytes, 1024);
			var unit = dim == 0 ? "" : SIZE_UNITS[dim - 1].ToString();
			var number = Math.Round(ToDimension(dim), 2);
			return $"{number} {unit}B";
		}
	}
}

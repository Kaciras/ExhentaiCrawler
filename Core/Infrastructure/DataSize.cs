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

		/// <summary>
		/// 计算该大小量除以 1024^index 的值。
		/// </summary>
		/// <param name="index">指数</param>
		/// <returns>商</returns>
		private double ToDimension(double index)
		{
			return Bytes / Math.Pow(1024, index);
		}

		/// <summary>
		/// 转换如 xx.xx MB 这样的字符串为大小。
		/// </summary>
		/// <param name="string">表示大小的字符串</param>
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
			var bytesAbs = Math.Abs(Bytes);
			if (bytesAbs < 1024)
			{
				return Bytes.ToString() + " B";
			}
			var i = (int)Math.Log(bytesAbs, 1024) - 1;
			var unit = SIZE_UNITS[i].ToString();

			var number = Math.Round(ToDimension(i), 2);
			return $"{number} {unit}B";
		}

		public static DataSize operator +(DataSize left, DataSize right)
		{
			return new DataSize(left.Bytes + right.Bytes);
		}

		public static DataSize operator -(DataSize left, DataSize right)
		{
			return new DataSize(left.Bytes - right.Bytes);
		}

		/// <summary>
		/// 字节转大小
		/// </summary>
		public static implicit operator DataSize(int bytes) => new DataSize(bytes);
	}
}

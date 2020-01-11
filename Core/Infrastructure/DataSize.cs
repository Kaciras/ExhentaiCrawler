using System;
using System.Text.RegularExpressions;

namespace Core.Infrastructure
{
	public enum SizeUnit
	{
		Bytes, KB, MB, GB, TB, PB, EB
	}

	public readonly struct DataSize
	{
		private static readonly char[] SIZE_UNITS = { 'K', 'M', 'G', 'T', 'P', 'E', 'Z', 'Y' };

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
		/// 解析如 12.34 MB 这样的字符串。
		/// 可以指定单位之间的倍率，通常是1024，但在硬盘大小标注时常用1000。
		/// </summary>
		/// <param name="string">字符串</param>
		/// <param name="fraction">单位倍率</param>
		/// <returns>数据大小</returns>
		/// <exception cref="FormatException">如果无法解析输入的字符串</exception>
		public static DataSize Parse(string @string, int fraction = 1024)
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

			var bytes = double.Parse(match.Groups[1].Value) * Math.Pow(fraction, level);
			return new DataSize((long)Math.Round(bytes));
		}

		public override string ToString() => ToString(1024);

		/// <summary>
		/// 以人可读的形式显示该数据的大小，比如 1 KB, 96.23 GB。
		/// 可以指定单位之间的倍率，通常是1024，但在硬盘大小标注时常用1000。
		/// </summary>
		/// <param name="fraction">单位倍率</param>
		/// <returns>字符串表示</returns>
		public string ToString(int fraction)
		{
			var abs = Math.Abs(Bytes);
			if (abs < fraction)
			{
				return $"{Bytes} B";
			}

			var i = (int)Math.Log(abs, fraction);
			var unit = SIZE_UNITS[i - 1];
			var v = Math.Round(ToDimension(i), 2);

			return $"{v} {unit}B";
		}

		// =============================== 运算重载 ===============================

		public static DataSize operator +(DataSize left, DataSize right)
		{
			return new DataSize(left.Bytes + right.Bytes);
		}

		public static DataSize operator -(DataSize left, DataSize right)
		{
			return new DataSize(left.Bytes - right.Bytes);
		}

		// =============================== 类型转换 ===============================

		public static implicit operator DataSize(int bytes) => new DataSize(bytes);
		public static implicit operator DataSize(long bytes) => new DataSize(bytes);
	}
}

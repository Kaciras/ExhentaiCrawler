using System;
using Core.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test
{
	[TestClass]
	public class DataSizeTest
	{
		[DataTestMethod]
		[DataRow("abcdefg")]
		[DataRow("")]
		[DataRow("1.2.3 KB")]
		[DataRow("45 QB")]
		public void ParseInvaildString(string value)
		{
			Assert.ThrowsException<FormatException>(() => DataSize.Parse(value));
		}

		[DataTestMethod]
		[DataRow(5L << 20, "5MiB")]
		[DataRow(5L << 20, "5MB")]
		[DataRow(5L << 20, "5mb")]
		[DataRow(5L << 20, "5M")]
		[DataRow(5L << 20, "+5MB")]
		[DataRow(5L << 20, " 5MB ")]
		[DataRow(1L << 60, "1 EB")]
		[DataRow(0, "0 EB")]
		[DataRow(-(5L << 20), "-5MB")]
		public void ParseDataSize(long bytes, string value)
		{
			Assert.AreEqual(new DataSize(bytes), DataSize.Parse(value));
		}

		[DataTestMethod]
		[DataRow(666, 666, SizeUnit.Bytes)]
		[DataRow(1, 1024, SizeUnit.KB)]
		[DataRow(0.25, 262144, SizeUnit.MB)]
		[DataRow(0, 0, SizeUnit.TB)]
		[DataRow(-1, -1048576, SizeUnit.MB)]
		public void ConvertDataSize(double num, long bytes, SizeUnit unit)
		{
			Assert.AreEqual(num, new DataSize(bytes).OfUnit(unit));
		}

		[DataTestMethod]
		[DataRow("20.19 TB", 22199139764797L)]
		[DataRow("500 B", 500)]
		[DataRow("500 KB", 500 << 10)]
		[DataRow("0 B", 0)]
		[DataRow("-12.25 MB", -12840960)]
		public void TestToString(string text, long bytes)
		{
			Assert.AreEqual(text, new DataSize(bytes).ToString());
		}
	}
}

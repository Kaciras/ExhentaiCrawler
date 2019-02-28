using System;
using Core.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test
{
	[TestClass]
	public sealed class DataSizeTest
	{
		[TestMethod]
		public void ParseInvaildString()
		{
			Assert.ThrowsException<FormatException>(() => DataSize.Parse("ABCDEFG"));
			Assert.ThrowsException<FormatException>(() => DataSize.Parse("1.2.3 KB"));
			Assert.ThrowsException<FormatException>(() => DataSize.Parse("45 QB"));
		}

		[TestMethod]
		public void ParseDataSize()
		{
			Assert.AreEqual(new DataSize(0), DataSize.Parse("0 EB"));

			Assert.AreEqual(new DataSize(1L << 60), DataSize.Parse("1 EB"));

			var fiveMB = new DataSize(5 << 20);
			Assert.AreEqual(fiveMB, DataSize.Parse("5MB"));
			Assert.AreEqual(fiveMB, DataSize.Parse("5mb"));
			Assert.AreEqual(fiveMB, DataSize.Parse("5MiB"));
			Assert.AreEqual(fiveMB, DataSize.Parse("5M"));
			Assert.AreEqual(fiveMB, DataSize.Parse("+5MB"));
			Assert.AreEqual(fiveMB, DataSize.Parse(" 5MB "));

			Assert.AreEqual(new DataSize(-(5 << 20)), DataSize.Parse("-5MB"));
		}

		[TestMethod]
		public void ConvertDataSize()
		{
			Assert.AreEqual(500, new DataSize(500).OfUnit(SizeUnit.Bytes));

			Assert.AreEqual(1.0, new DataSize(1024).OfUnit(SizeUnit.KB));

			Assert.AreEqual(0, new DataSize(0).OfUnit(SizeUnit.TB));

			Assert.AreEqual(-1, new DataSize(-1048576).OfUnit(SizeUnit.MB));
		}

		[TestMethod]
		public void TestToString()
		{
			Assert.AreEqual("0 B", new DataSize().ToString());
			Assert.AreEqual("500 B", new DataSize(500).ToString());

			Assert.AreEqual("20.19 TB", new DataSize(20.19, SizeUnit.TB).ToString());

			Assert.AreEqual("500 KB", new DataSize(500 << 10).ToString());

			Assert.AreEqual("12.25 MB", new DataSize((12 << 20) + (252 << 10)).ToString());
		}
	}
}

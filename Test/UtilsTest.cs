using System;
using System.Collections.Generic;
using System.Text;
using Core;
using Core.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test
{
	[TestClass]
	public class UtilsTest
	{
		[TestMethod]
		public void ParseRange()
		{
			Assert.AreEqual((50, 50), Utils.ParseRange("50"));
			Assert.AreEqual((99, null), Utils.ParseRange("99-"));
			Assert.AreEqual((null, 99), Utils.ParseRange("-99"));
			Assert.AreEqual((64, 128), Utils.ParseRange("64-128"));
			Assert.AreEqual((null, null), Utils.ParseRange("-"));
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
	}
}

﻿using System;
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
		public void ParseSize()
		{
			Assert.AreEqual(1, Utils.ParseSize("1 EB", SizeUnit.EB));

			Assert.AreEqual(1024, Utils.ParseSize("1 MB", SizeUnit.KB));

			Assert.AreEqual(6710886.4, Utils.ParseSize("6.40 PB", SizeUnit.GB));

			Assert.AreEqual(6.4, Utils.ParseSize("6710886.4 GiB", SizeUnit.PB));

			// 前后头有空格也可以
			Assert.AreEqual(512, Utils.ParseSize(" 0.5K ", SizeUnit.Bytes));

			Assert.AreEqual(100D / 1024, Utils.ParseSize("100 kb", SizeUnit.MB));
		}
	}
}

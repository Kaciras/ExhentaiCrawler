using System;
using System.Collections.Generic;
using System.Text;
using Core;
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
	}
}

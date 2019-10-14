using System;
using System.Collections.Generic;
using System.Text;
using Cli;
using Core;
using Core.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test
{
	[TestClass]
	public class CliProgramTest
	{
		[TestMethod]
		public void ParseRange()
		{
			Assert.AreEqual(50..50, DownloadMode.ParseRange("50"));
			Assert.AreEqual(Range.StartAt(99), DownloadMode.ParseRange("99-"));
			Assert.AreEqual(Range.EndAt(99), DownloadMode.ParseRange("-99"));
			Assert.AreEqual(64..128, DownloadMode.ParseRange("64-128"));
			Assert.AreEqual(Range.All, DownloadMode.ParseRange("-"));
		}
	}
}

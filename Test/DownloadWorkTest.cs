using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Cli;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test
{
	[TestClass]
	public class DownloadWorkTest
	{
		[TestMethod]
		public void CheckImageFile()
		{
			DownloadWork.CheckImageFile(new FileInfo("WebArchive/test_image.png")).Should().BeTrue();
			DownloadWork.CheckImageFile(new FileInfo("WebArchive/bad_image.png")).Should().BeFalse();
		}
	}
}

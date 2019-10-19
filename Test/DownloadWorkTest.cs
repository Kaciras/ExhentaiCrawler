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

		// 【验证】FileInfo 一旦创建，其状态就固定了，之后就修改了文件也不会对其造成影响
		[TestMethod]
		public void StaleFileInfo()
		{
			var directory = Path.GetTempPath();
			var path = Path.Join(directory, "_exhentai_test_");
			File.Delete(path);

			var file = new FileInfo(path);
			file.Exists.Should().BeFalse();

			File.WriteAllText(path, "hello world");
			file.Exists.Should().BeFalse();

			var newFile = new FileInfo(path);
			newFile.Exists.Should().BeTrue();
		}
	}
}

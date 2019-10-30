using System;
using System.Collections.Generic;
using System.Text;
using Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test
{
	[TestClass]
	public class ImageLinkTest
	{
		[DataTestMethod]
		[DataRow("https://exhentai.org/s/54c79d6bcf/1490729-5")]
		[DataRow("/s/54c79d6bcf/1490729-5")]
		public void Parse(string @string)
		{
			var link = ImageLink.Parse(@string);
			Assert.AreEqual("54c79d6bcf", link.Key);
			Assert.AreEqual(1490729, link.GalleryId);
			Assert.AreEqual(5, link.Page);
		}

		[DataTestMethod]
		[DataRow("https://exhentai.org/g/1508961/a444e881de")]
		[DataRow("/g/1508961/a444e881de")]
		[DataRow("https://example.com/s/ab0369a1f5/1508961-4")]
		[DataRow("")]
		[DataRow("invalid")]
		public void ParseFail(string @string)
		{
			Assert.ThrowsException<UriFormatException>(() => ImageLink.Parse(@string));
		}
	}
}

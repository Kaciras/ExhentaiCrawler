using System;
using System.Collections.Generic;
using System.Text;
using Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test
{
	[TestClass]
	public class GalleryLinkTest
	{
		[DataTestMethod]
		[DataRow("https://exhentai.org/g/1508961/a444e881de")]
		[DataRow("https://exhentai.org/g/1508961/a444e881de/")]
		[DataRow("/g/1508961/a444e881de/")]
		public void Parse(string @string)
		{
			var link = GalleryLink.Parse(@string);
			Assert.AreEqual(1508961, link.Id);
			Assert.AreEqual("a444e881de", link.Token);
		}

		[DataTestMethod]
		[DataRow("https://exhentai.org/s/ab0369a1f5/1508961-4")]
		[DataRow("/s/ab0369a1f5/1508961-4")]
		[DataRow("https://example.com/g/1508961/a444e881de/")]
		[DataRow("")]
		[DataRow("invalid")]
		public void ParseFail(string @string)
		{
			Assert.ThrowsException<UriFormatException>(() => GalleryLink.Parse(@string));
		}
	}
}

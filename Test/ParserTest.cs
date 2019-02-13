using Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Test
{
	[TestClass]
	public class ParserTest
    {
		[TestMethod]
		public void ParseGallery()
		{
			var html = File.ReadAllText("WebArchive/GalleryPage.html");
			var gallery = new Gallery(null, 518681, "2aa630b122");

			GalleryParser.Parse(gallery, html);

			Assert.AreEqual("[Tanaka Aji] Unsweet Netorare Ochita Onna-tachi", gallery.Name);
			Assert.AreEqual("[田中あじ] アンスイート 寝取られ堕ちた女たち", gallery.JapaneseName);
			Assert.AreEqual(2, gallery.TorrnetCount);
		}
    }
}

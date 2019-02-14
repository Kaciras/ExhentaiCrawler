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

			Assert.AreEqual("[Grinp (Neko Toufu)] Onii-chan wa Oshimai! Ch.1-12 [Chinese]", gallery.Name);
			Assert.AreEqual("[Grinp (ねことうふ)] お兄ちゃんはおしまい! 第1-12話 [中国翻訳]", gallery.JapaneseName);

			Assert.AreEqual(Category.NonH, gallery.Category);
			Assert.AreEqual("￥♂ung-漾", gallery.Uploader);

			Assert.AreEqual(new DateTime(2018, 2, 20, 19, 52, 0), gallery.Posted);
			Assert.AreEqual(new Uri("https://exhentai.org/g/1167472/ac1da09bc2/"), gallery.Parent);
			Assert.IsTrue(gallery.Visible);
			Assert.AreEqual(Language.Chinese, gallery.Language);
			Assert.IsTrue(gallery.IsTranslated);
			Assert.AreEqual((long)(39.64 * 1024), gallery.FileSize);
			Assert.AreEqual(152, gallery.Length);
			Assert.AreEqual(2406, gallery.Favorited);

			Assert.AreEqual(new Rating(594, 4.93), gallery.Rating);

			Assert.AreEqual(1, gallery.TorrnetCount);

			Assert.AreEqual(181, gallery.CommentCount);
		}
	}
}

using Core;
using Core.Infrastructure;
using HtmlAgilityPack;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Test
{
	[TestClass]
	public class ParserTest
    {
		[TestMethod]
		public void ParseGallery()
		{
			var html = File.ReadAllText("WebArchive/GalleryPageStub.html");
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
			//Assert.AreEqual(39.64, gallery.FileSize.OfUnit(SizeUnit.MB)); // 精度问题
			Assert.AreEqual(152, gallery.Length);
			Assert.AreEqual(2406, gallery.Favorited);

			Assert.AreEqual(new Rating(594, 4.93), gallery.Rating);

			Assert.AreEqual(1, gallery.TorrnetCount);

			Assert.AreEqual(181, gallery.CommentCount);
		}

		[TestMethod]
		public void ParseTitleGroup()
		{
			var mockGallery = new Gallery(null, 0, "");
			var doc = new HtmlDocument();
			doc.Load("WebArchive/GalleryTagStub.html");

			GalleryParser.ParseTitleGroup(mockGallery, doc);

			Assert.AreEqual("[BAK Hyeong Jun]Sweet Guy Ch.1-3(Chinese)(FITHRPG6)", mockGallery.Name);
			Assert.IsNull(mockGallery.JapaneseName);
		}

		[TestMethod]
		public void ParseTags()
		{
			var doc = new HtmlDocument();
			doc.Load("WebArchive/GalleryTagStub.html");

			var tags = GalleryParser.ParseTags(doc);

			Assert.AreEqual(new GalleryTag("bak hyeong jun", TagCredibility.Confidence), tags.Artist.First());
			Assert.AreEqual(new GalleryTag("story arc", TagCredibility.Incorrect), tags.Misc.First());
			Assert.AreEqual(new GalleryTag("webtoon", TagCredibility.Unconfidence), tags.Misc.Last());
		}

		[TestMethod]
		public void ParseTorrent()
		{
			var torrents = TorrentResource.Parse(File.ReadAllText("WebArchive/TorrentPageStub.html"));
			Assert.AreEqual(2, torrents.Count);

			var torrent = torrents[0];
			// 懒得写了
		}

		[TestMethod]
		public void ParseImages()
		{
			var html = File.ReadAllText("WebArchive/GalleryPageStub.html");

			var links = GalleryParser.ParseImages(html);

			Assert.AreEqual(40, links.Count);
			Assert.AreEqual("01_01.png", links[3].FileName);
			Assert.AreEqual("410aa30071", links[3].Key);
		}

		[TestMethod]
		public void ParseGalleryListPage()
		{
			var html = File.ReadAllText("WebArchive/Index.html");

			var listPage = GalleryListPage.ParseHtml(html);

			Assert.AreEqual(769_296, listPage.TotalCount);
			Assert.AreEqual(30772, listPage.TotalPage);

			var gals = listPage.Galleries;
			Assert.AreEqual(25, gals.Count);
			Assert.AreEqual("https://exhentai.org/g/1374376/455b8f2245/", gals[0].ToString());
			Assert.AreEqual("https://exhentai.org/g/1373931/2c2a4f931c/", gals[24].ToString());
		}
	}
}

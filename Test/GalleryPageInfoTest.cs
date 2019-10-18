using Core;
using Core.Infrastructure;
using FluentAssertions;
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
			var info = GalleryPageInfo.Parse(File.ReadAllText("WebArchive/GalleryPageStub.html"));

			Assert.AreEqual("[Grinp (Neko Toufu)] Onii-chan wa Oshimai! Ch.1-12 [Chinese]", info.Name);
			Assert.AreEqual("[Grinp (ねことうふ)] お兄ちゃんはおしまい! 第1-12話 [中国翻訳]", info.JapaneseName);

			Assert.AreEqual(Category.NonH, info.Category);
			Assert.AreEqual("￥♂ung-漾", info.Uploader);

			Assert.AreEqual(new DateTime(2018, 2, 20, 19, 52, 0), info.Posted);
			Assert.AreEqual(new Uri("https://exhentai.org/g/1167472/ac1da09bc2/"), info.Parent);
			Assert.IsTrue(info.Visible);
			Assert.AreEqual(Language.Chinese, info.Language);
			Assert.IsTrue(info.IsTranslated);
			//Assert.AreEqual(39.64, gallery.FileSize.OfUnit(SizeUnit.MB)); // 精度问题
			Assert.AreEqual(152, info.Length);
			Assert.AreEqual(2417, info.Favorited);

			Assert.AreEqual(new Rating(596, 4.93), info.Rating);

			Assert.AreEqual(0, info.TorrnetCount);
			Assert.AreEqual(182, info.CommentCount);
		}

		[TestMethod]
		public void ParseTitleGroup()
		{
			var info = new GalleryPageInfo();

			var doc = new HtmlDocument();
			doc.Load("WebArchive/GalleryTagStub.html");
			info.ParseTitleGroup(doc);

			Assert.AreEqual("[BAK Hyeong Jun]Sweet Guy Ch.1-3(Chinese)(FITHRPG6)", info.Name);
			Assert.IsNull(info.JapaneseName);
		}

		[TestMethod]
		public void ParseTags()
		{
			var doc = new HtmlDocument();
			doc.Load("WebArchive/GalleryTagStub.html");

			var tags = GalleryPageInfo.ParseTags(doc);

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

			var thumbnails = GalleryPageInfo.ParseThumbnails(html);

			Assert.AreEqual(40, thumbnails.Count);
			Assert.AreEqual("01_01.png", thumbnails[3].FileName);
			Assert.AreEqual("410aa30071", thumbnails[3].Link.Key);
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

		[TestMethod]
		public void ParseNewVersions()
		{
			var doc = new HtmlDocument();
			doc.Load("WebArchive/NewVersionTestPage.html");

			var list = GalleryPageInfo.ParseNewVersions(doc);
			list.Should().HaveCount(4);
			list[0].Should().Be("https://exhentai.org/g/1489813/0446282389/");
			list[^1].Should().Be("https://exhentai.org/g/1502818/b0f0b40e2e/");
		}
	}
}

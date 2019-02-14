using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Web;

namespace Core
{
	sealed class GalleryParser
	{
		readonly static Regex IMAGE_URL = new Regex(@"https://exhentai.org/s/([^/]+)/(\d+)-(\d+)", RegexOptions.Compiled);

		// 种子直接正则了，注意以>开头，保证与文本中的符号(&gt;)区分开
		readonly static Regex TORRENT = new Regex(@">Torrent Download \( (\d+) \)");

		readonly static char[] SIZE_UNIT = { 'K', 'M', 'G', 'T' };

		public static Gallery Parse(Gallery gallery, string html)
		{
			var doc = new HtmlDocument();
			doc.LoadHtml(html);

			ParseTitleGroup(gallery, doc);
			ParseAttrGroup(gallery, doc);
			gallery.Tags = ParseTags(doc);

			// 计算图片列表一共几页，第一页已经包含在这次的响应里了
			var firstList = ParseImages(html);
			var pages = 1 + (gallery.Length - 1) / firstList.Count;
			gallery.imageListPage = new IList<string>[pages];
			gallery.imageListPage[0] = firstList;

			gallery.TorrnetCount = int.Parse(TORRENT.Match(html).Groups[1].Value);

			return gallery;
		}

		static void ParseTitleGroup(Gallery gallery, HtmlDocument doc)
		{
			var name = doc.GetElementbyId("gn").InnerText;
			if (name != null)
			{
				gallery.Name = HttpUtility.HtmlDecode(name);
			}

			var jpname = doc.GetElementbyId("gj")?.InnerText;
			if (jpname != null)
			{
				gallery.JapaneseName = HttpUtility.HtmlDecode(jpname);
			}
		}

		/// <summary>
		/// 解析在封面和标签中间的那一列属性列表
		/// </summary>
		static void ParseAttrGroup(Gallery gallery, HtmlDocument doc)
		{
			var categoryName = doc.GetElementbyId("gdc").FirstChild.FirstChild.Attributes["alt"].Value;
			gallery.Category = CategoryHelper.Parse(categoryName);

			gallery.Uploader = doc.GetElementbyId("gdn").FirstChild.InnerText;

			// 中间的表格元素，包含上传时间、文件大小等。
			// 需要注意的是表格不完整，<tbody>是依靠浏览器自动补全的
			var tableRows = doc.GetElementbyId("gdd").FirstChild.ChildNodes;

			// 第5项：File Size:	12.34 [MKG]B
			// 没见到比KB还小的单位
			var sizePart = tableRows[4].LastChild.InnerText.Split(" ");
			var f = Array.IndexOf(SIZE_UNIT, sizePart[1][0]);
			gallery.FileSize = (long)double.Parse(sizePart[0]) * (1 << (10 * f));

			// 第6项：Length: \d+ pages
			gallery.Length = int.Parse(tableRows[5].LastChild.InnerText.Split(" ")[0]);
		}

		static TagCollection ParseTags(HtmlDocument doc)
		{
			var tags = new TagCollection();

			// 跟上面一样，不完整的表格
			var rows = doc.GetElementbyId("taglist").FirstChild;

			foreach (var row in rows.ChildNodes)
			{
				// 表格第一列是标签的命名空间
				var name = row.FirstChild.InnerText;
				name = char.ToUpper(name[0]) + name.Substring(1, name.Length - 2);

				var property = typeof(TagCollection).GetProperty(name)
					?? throw new MissingMemberException($"发现程序中未定义的标签类型：{name}");

				// 后面每一个都是标签
				var list = new List<GalleryTag>();
				foreach (var item in row.LastChild.ChildNodes)
				{
					var lowPrower = item.HasClass("gtl");
					var value = item.FirstChild.InnerText;
					list.Add(new GalleryTag(value, lowPrower));
				}
				property.SetValue(tags, list);
			}

			return tags;
		}

		internal static IList<string> ParseImages(string html)
		{
			return IMAGE_URL.Matches(html).Select(match => match.Value).ToList();
		}
	}
}

using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace Core
{
	sealed class GalleryParser
	{
		readonly static Regex IMAGE_URL = new Regex(@"https://exhentai.org/s/([^/]+)/(\d+)-(\d+)");

		public static Gallery Parse(Gallery gallery, string html)
		{
			var doc = new HtmlDocument();
			doc.LoadHtml(html);

			ParseTitleGroup(gallery, doc);
			gallery.Tags = ParseTags(doc);
			gallery.firstImagePage = ParseImages(html);

			return gallery;
		}

		static void ParseTitleGroup(Gallery gallery, HtmlDocument doc)
		{
			var name = doc.GetElementbyId("gn").InnerText;
			if (name != null)
			{
				gallery.Name = name;
				gallery.JapaneseName = doc.GetElementbyId("gj")?.InnerText;
			}
		}

		// 挨着封面右边的那一列属性列表
		static void ParseAttrGroup(Gallery gallery, HtmlDocument doc)
		{
			var categoryName = doc.GetElementbyId("gdc").FirstChild.FirstChild.Attributes["alt"].Value;
			gallery.Category = CategoryExtention.Parse(categoryName);

			gallery.Uploader = doc.GetElementbyId("gdn").FirstChild.InnerText;

			// 中间的表格元素，包含上传时间、文件大小等
			var tableRows = doc.GetElementbyId("gdd").FirstChild.FirstChild.ChildNodes;

		}

		static TagCollection ParseTags(HtmlDocument doc)
		{
			var tags = new TagCollection();

			// 表格不完整，<tbody>是依靠浏览器自动补全的
			var rows = doc.GetElementbyId("taglist").FirstChild;

			foreach (var row in rows.ChildNodes)
			{
				var name = row.FirstChild.InnerText;
				name = char.ToUpper(name[0]) + name.Substring(1, name.Length - 2);

				var property = typeof(TagCollection).GetProperty(name)
					?? throw new MissingMemberException($"发现程序中未定义的标签类型：{name}");

				var list = new List<GalleryTag>();
				foreach (var item in row.LastChild.ChildNodes)
				{
					var lowPrower = item.HasClass("gtl");
					var value = item.FirstChild.InnerText;
					list.Add(new GalleryTag(lowPrower, value));
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

using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Web;
using System.Globalization;

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
			gallery.CommentCount = doc.GetElementbyId("cdiv").ChildNodes.Count / 2;

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

			// 第1项Posted: 2018-02-20 19:52
			var posted = tableRows[0].LastChild.InnerText;
			gallery.Posted = DateTime.ParseExact(posted, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);

			// 第2项Parent: 1167472 是个连接
			var parent = tableRows[1].LastChild.FirstChild;
			if (parent.NodeType != HtmlNodeType.Text)
			{
				gallery.Parent = new Uri(parent.Attributes["href"].Value);
			}

			// 第3项Visible:	Yes[No]
			gallery.Visible = tableRows[2].LastChild.InnerText == "Yes";

			// 第4项Language: Chinese &nbsp;(TR)?
			var lang = tableRows[3].LastChild.FirstChild;
			gallery.Language = Enum.Parse<Language>(lang.InnerText.Split(" ")[0]);
			gallery.IsTranslated = lang.NextSibling != null;

			// 第5项File Size: 39.64 [MKG]B，没见到比KB还小的单位
			gallery.FileSize = (long)Utils.ParseSize(tableRows[4].LastChild.InnerText, 'K');
			
			// 第6项Length: 152 pages
			gallery.Length = int.Parse(tableRows[5].LastChild.InnerText.Split(" ")[0]);

			// 第7项Favorited: 2406 times
			gallery.Favorited = int.Parse(tableRows[6].LastChild.InnerText.Split(" ")[0]);

			var ratingCount = int.Parse(doc.GetElementbyId("rating_count").InnerText);
			var ratingAvg = double.Parse(doc.GetElementbyId("rating_label").InnerText.Split(" ")[1]);
			gallery.Rating = new Rating(ratingCount, ratingAvg);
		}

		internal static TagCollection ParseTags(HtmlDocument doc)
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
					var value = item.FirstChild.InnerText;
					var cred = ParseTagCredibility(item.GetClasses().First());
					list.Add(new GalleryTag(value, cred));
				}
				property.SetValue(tags, list);
			}

			return tags;
		}

		/// <summary>
		/// 由标签元素的样式类解析标签的可信度。
		/// </summary>
		/// <param name="class">CSS类名</param>
		/// <returns>可信度枚举</returns>
		private static TagCredibility ParseTagCredibility(string @class)
		{
			switch (@class)
			{
				case "gt":
					return TagCredibility.Confidence;
				case "gtl":
					return TagCredibility.Unconfidence;
				case "gtw":
					return TagCredibility.Incorrect;
				default:
					throw new NotSupportedException("Unrecognized tag class: " + @class);
			}
		}

		internal static IList<string> ParseImages(string html)
		{
			return IMAGE_URL.Matches(html).Select(match => match.Value).ToList();
		}
	}
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Core.Infrastructure;
using HtmlAgilityPack;

namespace Core
{
	public sealed class GalleryPageInfo
	{
		// 种子直接正则了，注意以>开头，保证与文本中的符号(&gt;)区分开
		private static readonly Regex TORRENT = new Regex(@">Torrent Download \( (\d+) \)");

		public string Name { get; set; }
		public string JapaneseName { get; set; }

		public Category Category { get; set; }
		public string Uploader { get; set; }

		// 中间的一堆属性
		public DateTime Posted { get; set; }
		public GalleryLink Parent { get; set; }
		public bool Visible { get; set; }
		public Language Language { get; set; }
		public bool IsTranslated { get; set; }
		public DataSize FileSize { get; set; }
		public int Length { get; set; }
		public int Favorited { get; set; }
		public Rating Rating { get; set; }

		public TagCollection Tags { get; set; }

		public int TorrnetCount { get; set; }
		public int CommentCount { get; set; }

		// 如果有新版，则在简介栏和图片栏中间会出现链接列表，包含了新版的链接，新的靠后。
		// 【注意】列表里并非一定包括最新版，如果要获取最新的需要遍历，直到没有新版链接为止。
		public IList<GalleryLink> NewVersions { get; set; }

		public IList<ImageListItem> Thumbnails { get; set; }

		public static GalleryPageInfo Parse(string html)
		{
			var info = new GalleryPageInfo();
			var doc = new HtmlDocument();
			doc.LoadHtml(html);

			info.ParseTitleGroup(doc);
			info.ParseAttrGroup(doc);
			info.Tags = ParseTags(doc);

			// 计算图片列表一共几页，第一页已经包含在这次的响应里了
			info.Thumbnails = ParseThumbnails(doc);
			info.NewVersions = ParseNewVersions(doc);

			info.TorrnetCount = int.Parse(TORRENT.Match(html).Groups[1].Value);
			info.CommentCount = doc.GetElementbyId("cdiv").ChildNodes.Count / 2;
			return info;
		}

		internal void ParseTitleGroup(HtmlDocument doc)
		{
			var name = doc.GetElementbyId("gn").InnerText;
			if (name != null)
			{
				Name = HttpUtility.HtmlDecode(name);
			}

			var jpname = doc.GetElementbyId("gj")?.InnerText;
			if (jpname.Length > 0)
			{
				JapaneseName = HttpUtility.HtmlDecode(jpname);
			}
		}

		/// <summary>
		/// 解析在封面和标签中间的那一列属性列表
		/// </summary>
		private void ParseAttrGroup(HtmlDocument doc)
		{
			var categoryName = doc.GetElementbyId("gdc").FirstChild.FirstChild.InnerText;
			Category = CategoryHelper.Parse(categoryName);
			Uploader = doc.GetElementbyId("gdn").FirstChild.InnerText;

			// 中间的表格元素，包含上传时间、文件大小等。
			// 需要注意的是表格不完整，<tbody>是依靠浏览器自动补全的
			var tableRows = doc.GetElementbyId("gdd").FirstChild.ChildNodes;

			// 第1项Posted: 2018-02-20 19:52
			var posted = tableRows[0].LastChild.InnerText;
			Posted = DateTime.ParseExact(posted, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);

			// 第2项Parent: 1167472 是个连接
			var parent = tableRows[1].LastChild.FirstChild;
			if (parent.NodeType != HtmlNodeType.Text)
			{
				Parent = GalleryLink.Parse(parent.Attributes["href"].Value);
			}

			// 第3项 Visible: Yes[No]
			Visible = tableRows[2].LastChild.InnerText == "Yes";

			// 第4项Language: Chinese &nbsp;(TR)?
			var lang = tableRows[3].LastChild.FirstChild;
			Language = Enum.Parse<Language>(lang.InnerText.Split(" ")[0]);
			IsTranslated = lang.NextSibling != null;

			// 第5项File Size: 39.64 [MKG]B，没见到比KB还小的单位
			FileSize = DataSize.Parse(tableRows[4].LastChild.InnerText);

			// 第6项Length: 152 pages
			Length = int.Parse(tableRows[5].LastChild.InnerText.Split(" ")[0]);

			// 第7项Favorited: 2406 times
			Favorited = int.Parse(tableRows[6].LastChild.InnerText.Split(" ")[0]);

			// 最下面的星星
			var ratingCount = int.Parse(doc.GetElementbyId("rating_count").InnerText);
			var ratingAvg = double.Parse(doc.GetElementbyId("rating_label").InnerText.Split(" ")[1]);
			Rating = new Rating(ratingCount, ratingAvg);
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
				name = char.ToUpper(name[0]) + name[1..^1];

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

		internal static IList<GalleryLink> ParseNewVersions(HtmlDocument doc)
		{
			var listNode = doc.GetElementbyId("gnd");
			if (listNode == null)
			{
				return Array.Empty<GalleryLink>();
			}
			return listNode.SelectNodes("a")
				.Select(node => node.Attributes["href"].Value)
				.Select(GalleryLink.Parse).ToList();
		}

		/// <summary>
		/// 由标签元素的样式类解析标签的可信度。
		/// </summary>
		/// <param name="class">CSS类名</param>
		/// <returns>可信度枚举</returns>
		private static TagCredibility ParseTagCredibility(string @class) => @class switch
		{
			"gt" => TagCredibility.Confidence,
			"gtl" => TagCredibility.Unconfidence,
			"gtw" => TagCredibility.Incorrect,
			_ => throw new NotSupportedException("Unrecognized tag class: " + @class),
		};
		
		public static IList<ImageListItem> ParseThumbnails(string html)
		{
			var doc = new HtmlDocument();
			doc.LoadHtml(html);
			return ParseThumbnails(doc);
		}

		internal static IList<ImageListItem> ParseThumbnails(HtmlDocument doc)
		{
			var nodes = doc.GetElementbyId("gdt").ChildNodes;
			nodes.RemoveAt(nodes.Count - 1); // 去掉最后空白的div

			var result = new List<ImageListItem>();
			foreach (var item in nodes)
			{
				var anchor = item.FirstChild.FirstChild;
				var link = ImageLink.Parse(anchor.Attributes["href"].Value);
				var name = anchor.FirstChild.Attributes["title"].Value.Split(": ")[1];
				result.Add(new ImageListItem(link, name));
			}
			return result;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Text;
using HtmlAgilityPack;

namespace Core
{
	public sealed class GalleryListPage
	{
		public List<Uri> Galleries { get; set; }

		public int TotalCount { get; set; }

		// 可以直接计算
		public int TotalPage => 1 + (TotalCount - 1) / Galleries.Count;
		
		public static GalleryListPage ParseHtml(string html)
		{
			var result = new GalleryListPage();

			var doc = new HtmlDocument();
			doc.LoadHtml(html);
			var body = doc.DocumentNode.SelectSingleNode("/html/body/div/div[2]");

			// <p class="ip" style="margin-top:5px">Showing 1-25 of 769,296</p>
			var ip = body.SelectSingleNode("p").InnerText.Split(' ');
			result.TotalCount = int.Parse(ip[^1].Replace(",", ""));

			// 画册列表，第一行是表头
			var rows = body.SelectNodes("table[2]/tr");
			result.Galleries = new List<Uri>(rows.Count - 1);
			
			for (int i = 1; i < rows.Count; i++)
			{
				var el = rows[i].SelectSingleNode("td[3]/div/div[3]/a");
				result.Galleries.Add(new Uri(el.Attributes["href"].Value));
			}

			return result;
		}
	}
}

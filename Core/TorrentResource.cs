using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Core.Infrastructure;
using HtmlAgilityPack;

namespace Core
{
	public class TorrentResource
	{
		public DateTime Posted { get; set; }
		public int Size { get; set; } //KB
		public int Seeds { get; set; }
		public int Peers { get; set; }
		public int Downloads { get; set; }
		public string Uploader { get; set; }

		public string Uri { get; set; }
		public string FileName { get; set; }

		internal static IList<TorrentResource> Parse(string html)
		{
			var list = new List<TorrentResource>();
			var doc = new HtmlDocument();
			doc.LoadHtml(html);

			var forms = doc.DocumentNode.SelectNodes("/html/body/div/div[1]/div[1]/form/div//table");
			foreach (var form in forms)
			{
				list.Add(ParseForm(form));
			}
			return list;
		}

		private static TorrentResource ParseForm(HtmlNode node)
		{
			var torrent = new TorrentResource();

			// 这个表的单元格格式是固定的，提取公共部分
			string CellValue(int row, int col) 
				=> node.SelectSingleNode($"tr[{row}]/td[{col}]").LastChild.InnerText.TrimStart();

			torrent.Posted = DateTime.ParseExact(CellValue(1, 1), "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
			torrent.Size = (int)Utils.ParseSize(CellValue(1, 2), SizeUnit.KB);
			torrent.Seeds = int.Parse(CellValue(1, 4));
			torrent.Peers = int.Parse(CellValue(1, 5));
			torrent.Uploader = CellValue(2, 1);

			var downloadLink = node.SelectSingleNode($"tr[3]/td[1]/a[1]");
			torrent.Uri = downloadLink.Attributes["href"].Value;
			torrent.FileName = downloadLink.InnerText;

			return torrent;
		}
	}
}

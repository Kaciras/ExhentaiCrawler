using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Net.Http;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Net;

namespace Core
{
	public class ImageResource
	{
		static readonly Regex FULL_IMG = new Regex("https://exhentai.org/fullimg.php[^\"]+", RegexOptions.Compiled);
		static readonly Regex IMG_SRC = new Regex(" src=\"([^\"]+)", RegexOptions.Compiled);

		static readonly Regex SHOWKEY_VAR = new Regex("var showkey=\"(\\w+)\";", RegexOptions.Compiled);
		static readonly Regex FULLIMAGE = new Regex(@"fullimg.php\?gid=\d+&page=\d+&key=(\w+)", RegexOptions.Compiled);

		public Gallery Gallery { get; }
		public int Page { get; }

		public string FileName { get; set; }

		// 图片链接
		internal string ImageUrl { get; set; }
		internal string FullImageUrl { get; set; }

		internal string PreviousPageUrl { get; set; }
		internal string NextPageUrl { get; set; }

		readonly ExhentaiHttpClient client;

		public ImageResource(ExhentaiHttpClient client, Gallery gallery, int page)
		{
			this.client = client;
			Gallery = gallery;
			Page = page;
		}

		//public ImageResource GetPrevious()
		//{

		//}

		public async Task<ImageResource> GetNext()
		{
			if (Page >= Gallery.Length)
			{
				return null;
			}
			var html = await client.RequestPage(NextPageUrl);
			var next = new ImageResource(client, Gallery, Page + 1);
			ParsePage(next, html);

			return next;
		}

		public async Task<Stream> GetOriginal()
		{
			var content = await client.RequestImage(FullImageUrl ?? ImageUrl);
			return await content.ReadAsStreamAsync();
		}

		public static void ParsePage(ImageResource resource, string html)
		{
			var doc = new HtmlDocument();
			doc.LoadHtml(html);

			resource.PreviousPageUrl = doc.GetElementbyId("i2").FirstChild.FirstChild.Attributes["href"].Value;
			resource.NextPageUrl = doc.GetElementbyId("i3").FirstChild.Attributes["href"].Value;

			resource.ImageUrl = doc.GetElementbyId("i3").FirstChild.FirstChild.Attributes["src"].Value;

			var nameAndSize = doc.GetElementbyId("i4").FirstChild.InnerText;
			resource.FileName = nameAndSize.Split("::")[0].TrimEnd();

			// 如果在线浏览的图片已经是原始大小则没有此链接
			// 这个链接里与号被转义了
			resource.FullImageUrl = HttpUtility.HtmlDecode(doc.GetElementbyId("i7").LastChild?.Attributes["href"].Value);
		}
	}
}

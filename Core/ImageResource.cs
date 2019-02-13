using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Core
{
	public class ImageResource
	{
		internal static readonly Regex IMAGE_PATH = new Regex(@"/s/(?<IMG_KEY>\w+)/(?<GID>\d+)-(?<PAGE>\d+)", RegexOptions.Compiled);

		static readonly Regex FULL_IMG = new Regex("https://exhentai.org/fullimg.php[^\"]+", RegexOptions.Compiled);
		static readonly Regex SHOWKEY_VAR = new Regex("var showkey=\"(\\w+)\";", RegexOptions.Compiled);
		static readonly Regex FULLIMAGE = new Regex(@"fullimg.php\?gid=\d+&page=\d+&key=(\w+)", RegexOptions.Compiled);

		public Gallery Gallery { get; }
		public int Page { get; }
		public string ImageKey { get; }

		// 这个玩意隔一段时间就会变
		internal string ShowKey { get; set; }

		internal string FullImageUrl { get; set; }

		internal string PreviousImageKey { get; set; }
		internal string NextImageKey { get; set; }

		readonly ExhentaiClient client;

		public ImageResource(ExhentaiClient client, Gallery gallery, int page, string imageKey)
		{
			this.client = client;
			Gallery = gallery;
			Page = page;
			ImageKey = imageKey;
		}

		//public ImageResource GetPrevious()
		//{

		//}

		public async Task<ImageResource> GetNext()
		{
			var res = await client.RequestApi(new
			{
				method= "showpage",
				gid = Gallery.Id,
				imgkey = NextImageKey,
				page = Page + 1,
				showkey = ShowKey,
			});

			var next = new ImageResource(client, Gallery, Page + 1, NextImageKey);
			ParseApi(next, res);
			next.ShowKey = ShowKey; // 这个不变，也没在响应里出现

			return next;
		}

		public static void ParsePage(ImageResource resource, string html)
		{
			var doc = new HtmlDocument();
			doc.LoadHtml(html);

			var prevUrl = doc.GetElementbyId("i2").FirstChild.FirstChild.Attributes["href"].Value;
			resource.PreviousImageKey = IMAGE_PATH.Match(prevUrl).Groups["IMG_KEY"].Value;

			var nextUrl = doc.GetElementbyId("i3").FirstChild.Attributes["href"].Value;
			resource.NextImageKey = IMAGE_PATH.Match(nextUrl).Groups["IMG_KEY"].Value;

			resource.ShowKey = SHOWKEY_VAR.Match(html).Groups[1].Value;

			// 如果在线浏览的图片已经是原始大小则没有此链接
			resource.FullImageUrl = doc.GetElementbyId("i7").LastChild?.Attributes["href"].Value;
		}


		public static void ParseApi(ImageResource resource, JObject response)
		{
			resource.PreviousImageKey = IMAGE_PATH.Match((string)response["n"]).Groups["IMG_KEY"].Value;
			resource.NextImageKey = IMAGE_PATH.Match((string)response["i3"]).Groups["IMG_KEY"].Value;

			var match = FULL_IMG.Match((string)response["i7"]);
			if (match.Success)
			{
				resource.FullImageUrl = match.Value;
			}
		}
	}
}

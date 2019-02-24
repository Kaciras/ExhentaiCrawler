using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Core.Request;
using HtmlAgilityPack;

namespace Core
{
	public class ImageResource
	{
		public static readonly Regex IMAGE_PATH = new Regex(@"/s/(?<KEY>\w+)/(?<GID>\d+)-(?<PAGE>\d+)");

		public Gallery Gallery { get; }

		public string ImageKey { get; }
		public int Page { get; }
		public string FileName { get; }

		// 图片链接
		internal string ImageUrl { get; set; }
		internal string FullImageUrl { get; set; }

		private readonly ExhentaiClient client;

		public ImageResource(ExhentaiClient client, Gallery gallery, int page, string imageKey, string filename)
		{
			this.client = client;
			Gallery = gallery;
			Page = page;
			ImageKey = imageKey;
			FileName = filename;
		}

		public async Task<Stream> GetImageStream()
		{
			return await (await client.Request(ImageUrl)).Content.ReadAsStreamAsync();
		}

		/// <summary>
		/// 下载原始图片，此操作将消耗限额。
		/// 如果没有原图，则返回null
		/// </summary>
		public async Task<OriginImage> GetOriginal()
		{
			await EnsureImagePageLoaded();

			if (FullImageUrl == null)
			{
				return null;
			}
			using (var redirect = await client.Request(FullImageUrl))
			{
				return new OriginImage(client, redirect.Headers.Location);
			}
		}

		/// <summary>
		/// 访问图片页面将消耗1点配额，所以使用了懒加载，尽量推迟访问图片页。
		/// </summary>
		private async Task EnsureImagePageLoaded()
		{
			// 该方法虽然也是同步返回的情况多，但编译器能自动使用Task.CompletedTask避免分配，故没必要用ValueTask
			if (ImageUrl != null)
			{
				return; // 已经加载过了
			}
			var uri = $"https://exhentai.org/s/{ImageKey}/{Gallery.Id}-{Page}";
			var doc = new HtmlDocument();
			doc.LoadHtml(await client.RequestPage(uri));

			ImageUrl = doc.GetElementbyId("i3").FirstChild.FirstChild.Attributes["src"].Value;

			// 如果在线浏览的图片已经是原始大小则没有此链接。这个链接里与号被转义了
			FullImageUrl = HttpUtility.HtmlDecode(doc.GetElementbyId("i7").LastChild?.Attributes["href"].Value);
		}
	}
}

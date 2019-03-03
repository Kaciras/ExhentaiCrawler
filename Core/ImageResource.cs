using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Core.Infrastructure;
using Core.Request;
using HtmlAgilityPack;

namespace Core
{
	public class ImageResource
	{
		public static readonly Regex IMAGE_PATH = new Regex(@"/s/(?<KEY>\w+)/(?<GID>\d+)-(?<PAGE>\d+)");
		public static readonly Regex FULL_IMG_TEXT = new Regex(@"Download original (\d+) x (\d+) ([0-9A-Z. ]+) source");

		public Gallery Gallery { get; }

		public string ImageKey { get; }
		public int Page { get; }
		public string FileName { get; }

		internal string ImageUrl { get; set; }

		internal Uri OriginImageUrl { get; set; }
		internal DataSize OriginImageSize { get; set; }

		private readonly ExhentaiClient client;

		private IPRecord bindIP;

		internal ImageResource(ExhentaiClient client, Gallery gallery, int page, ImageLink link)
		{
			this.client = client;
			Gallery = gallery;
			Page = page;
			ImageKey = link.Key;
			FileName = link.FileName;
		}

		public async Task<Stream> GetImageStream(bool useBindIP = true)
		{
			await EnsureImagePageLoaded();

			if (useBindIP)
			{
				return await client.Request(new PeerImageRequest(ImageUrl), bindIP);
			}
			else
			{
				return await client.Request(new PeerImageRequest(ImageUrl));
			}
		}

		/// <summary>
		/// 下载原始图片，如果没有原图，则返回null。
		/// </summary>
		public async Task<OriginImage> GetOriginal()
		{
			await EnsureImagePageLoaded();

			if (OriginImageUrl == null)
			{
				return null;
			}
			var res = await client.NewSiteRequest(OriginImageUrl)
				.WithCost((int)Math.Ceiling(OriginImageSize.OfUnit(SizeUnit.MB) * 5)) // 0.2M 消耗一点限额，向上取整
				.Execute();

			return new OriginImage(client, res.IPRecord, res.FetchRedirectLocation());
		}

		/// <summary>
		/// 自动选择质量最高的图片版本下载，并保存到文件。
		/// </summary>
		/// <param name="fileToSave">保存的文件名</param>
		/// <param name="cancelToken">取消令牌</param>
		/// <exception cref="OperationCanceledException">如果完成之前被取消</exception>
		public async Task Download(string fileToSave, CancellationToken cancelToken = default)
		{
			try
			{
				using (var input = await GetStream())
				using (var output = File.OpenWrite(fileToSave))
				{
					await input.CopyToAsync(output, cancelToken);
				}
			}
			catch
			{ 
				File.Delete(fileToSave);
				throw; // 如果出现异常就删除文件，避免保存残缺的图片。
			}
		}

		// 选择图片的下载方式，优先使用与页面请求是同一个的IP
		private async Task<Stream> GetStream()
		{
			var originImage = await GetOriginal();
			if (originImage != null)
			{
				try
				{
					return await originImage.GetStream();
				}
				catch (ObjectDisposedException)
				{
					return await originImage.GetStream(false);
				}
			}
			else
			{
				try
				{
					return await GetImageStream();
				}
				catch (ObjectDisposedException)
				{
					return await GetImageStream(false);
				}
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
			var resp = await client.NewSiteRequest(uri).Execute();
			bindIP = resp.IPRecord;

			var doc = new HtmlDocument();
			doc.LoadHtml(resp.Content);

			ImageUrl = doc.GetElementbyId("i3").FirstChild.FirstChild.Attributes["src"].Value;

			// 如果在线浏览的图片已经是原始大小则没有此链接。注意这个链接被HTML转义了
			var fullImg = doc.GetElementbyId("i7").LastChild;
			if (fullImg != null)
			{
				OriginImageUrl = new Uri(HttpUtility.HtmlDecode(fullImg.Attributes["href"].Value));

				var match = FULL_IMG_TEXT.Match(fullImg.InnerText);
				OriginImageSize = DataSize.Parse(match.Groups[3].Value);
			}
		}
	}
}

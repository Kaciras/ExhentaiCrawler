using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Infrastructure;
using Core.Request;

namespace Core
{
	public sealed class Gallery
	{
		public int Id { get; }
		public string Token { get; }

		public GalleryPageInfo Info { get; private set; }

		private readonly ExhentaiClient client;

		/// <summary>
		/// 数组表示所有分页，里面的List表示每一页的图片列表。写起来有点奇怪...
		/// </summary>
		private IList<ImageThumbnail>[] imageListPage;

		private Gallery(ExhentaiClient client, int id, string token)
		{
			this.client = client;
			Id = id;
			Token = token;
		}

		public async Task<IList<TorrentResource>> GetTorrents()
		{
			var html = await client
				.NewSiteRequest($"https://exhentai.org/gallerytorrents.php?gid={Id}&t={Token}")
				.ExecuteForContent();
			return TorrentResource.Parse(html);
		}

		// ValueTask的使用：
		//	   ValueTask的创建开销比Task小，但传递开销更大，故其适合使用的场景应当满足以下条件：
		//	   1) 调用方只是简单地等待，而不对其做更多使用
		//	   2) 大多情况是同步返回，常见于具有缓存的逻辑
		//	   3) 返回值不是特别简单的类型，如bool、byte等能被系统自动缓存。
		// 虽然这里用哪个性能都没啥差别，但还是尝试下ValueTask。

		/// <summary>
		/// 获取该画册的一张图片。
		/// TODO: 并发？
		/// </summary>
		/// <param name="index">页码，从1开始</param>
		public async ValueTask<ImageResource> GetImage(int index)
		{
			if (index < 1 || index > Info.Length)
			{
				throw new ArgumentOutOfRangeException("错误的页码：" + index);
			}

			// 除了图片页面的URL以外，其他均由0开始算的
			index--;

			var pageSize = imageListPage[0].Count;
			var page = index / pageSize;

			var list = imageListPage[page];
			if (list == null)
			{
				var galleryPage = await client
					.NewSiteRequest($"https://exhentai.org/g/{Id}/{Token}?p={page}")
					.ExecuteForContent();

				list = imageListPage[page] = GalleryPageInfo.ParseThumbnails(galleryPage);
			}

			var link = list[index % pageSize];
			return new ImageResource(client, this, link);
		}

		public static async Task<Gallery> From(ExhentaiClient client, int id, string token)
		{
			var gallery = new Gallery(client, id, token);

			// hc=1 显示全部评论
			var html = await client
				.NewSiteRequest($"https://exhentai.org/g/{id}/{token}?hc=1")
				.ExecuteForContent();

			var info = GalleryPageInfo.Parse(html);

			// 计算图片列表一共几页，第一页已经包含在这次的响应里了
			var pages = 1 + (info.Length - 1) / info.Thumbnails.Count;
			var imageListPage = new IList<ImageThumbnail>[pages];
			imageListPage[0] = info.Thumbnails;

			gallery.Info = info;
			gallery.imageListPage = imageListPage;
			return gallery;
		}
	}
}

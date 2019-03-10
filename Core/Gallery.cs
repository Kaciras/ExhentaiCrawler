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

		public string Name { get; set; }
		public string JapaneseName { get; set; }

		public Category Category { get; set; }
		public string Uploader { get; set; }

		public DateTime Posted { get; set; }
		public Uri Parent { get; set; }
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

		private readonly ExhentaiClient client;

		/// <summary>
		/// 数组表示所有分页，里面的List表示每一页的图片列表。写起来有点奇怪...
		/// </summary>
		internal IList<ImageLink>[] imageListPage;

		public Gallery(ExhentaiClient client, int id, string token)
		{
			this.client = client;
			Id = id;
			Token = token;
		}

		public async Task<IList<TorrentResource>> GetTorrents()
		{
			var html = await client.NewSiteRequest($"https://exhentai.org/gallerytorrents.php?gid={Id}&t={Token}")
				.ExecuteForContent();
			return TorrentResource.Parse(html);
		}

		// TODO: 并发？
		/// <summary>
		/// 获取该画册的一张图片。
		/// </summary>
		/// <param name="index">页码，从1开始</param>
		public async ValueTask<ImageResource> GetImage(int index)
		{
			// ValueTask的使用：
			//	   ValueTask的创建开销比Task小，但传递开销更大，故其适合使用的场景应当满足以下条件：
			//	   1) 调用方只是简单地等待，而不对其做更多使用
			//	   2) 大多情况是同步返回，常见于具有缓存的逻辑
			// 虽然这里用哪个性能都没啥差别，但还是尝试下ValueTask。
			if (index < 1 || index > Length)
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

				list = imageListPage[page] = GalleryParser.ParseImages(galleryPage);
			}

			var link = list[index % pageSize];
			return new ImageResource(client, this, index + 1, link);
		}
	}
}

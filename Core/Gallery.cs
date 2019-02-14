using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Core
{
	public class Gallery
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
		public long FileSize { get; set; } // 单位是KB
		public int Length { get; set; }
		public int Favorited { get; set; }

		public Rating Rating { get; set; }

		public TagCollection Tags { get; set; }

		public int TorrnetCount { get; set; }

		public int CommentCount { get; set; }

		readonly ExhentaiHttpClient client;

		/// <summary>
		/// 数组表示所有分页，里面的List表示每一页的图片列表。写起来有点奇怪...
		/// </summary>
		internal IList<string>[] imageListPage;

		public Gallery(ExhentaiHttpClient client, int id, string token)
		{
			this.client = client;
			Id = id;
			Token = token;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="index">页码，从1开始</param>
		/// <returns></returns>
		public async Task<ImageResource> GetImage(int index)
		{
			if(index < 1 || index > Length)
			{
				throw new ArgumentOutOfRangeException("错误的页码：" + index);
			}

			// 除了图片页面的URL以外，其他均由0开始算的
			index--;

			var pageSize = imageListPage[0].Count;
			var page = index / pageSize;
			index = index % pageSize;

			var list = imageListPage[page];
			if (list == null)
			{
				var galleryPage = await client.RequestPage($"https://exhentai.org/g/{Id}/{Token}?p={page}");
				imageListPage[page] = GalleryParser.ParseImages(galleryPage);
			}

			var html = await client.RequestPage(list[index]);
			var resource = new ImageResource(client, this, index + 1);
			ImageResource.ParsePage(resource, html);

			return resource;
		}
	}
}

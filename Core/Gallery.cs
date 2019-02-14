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
		public string Parent { get; set; }
		public bool Visible { get; set; }
		public Language Language { get; set; }
		public bool IsTranslated { get; set; }
		public long FileSize { get; set; }
		public int Length { get; set; }
		public int Favorited { get; set; }

		public TagCollection Tags { get; set; }

		public int TorrnetCount { get; set; }

		readonly ExhentaiHttpClient client;
		internal IList<string> firstImagePage;

		public Gallery(ExhentaiHttpClient client, int id, string token)
		{
			this.client = client;
			Id = id;
			Token = token;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="page">页码，从1开始</param>
		/// <returns></returns>
		public async Task<ImageResource> GetImage(int page)
		{
			string url;

			if(page <= firstImagePage.Count)
			{
				url = firstImagePage[page - 1];
			}
			else
			{
				throw new NotImplementedException();
			}

			var html = await client.RequestPage(url);
			var ik = ImageResource.IMAGE_PATH.Match(url).Groups["IMG_KEY"].Value;

			var resource = new ImageResource(client, this, page, ik);
			ImageResource.ParsePage(resource, html);

			return resource;
		}
	}
}

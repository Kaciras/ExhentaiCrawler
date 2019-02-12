using System;
using System.Collections.Generic;
using System.Text;

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

		readonly ExhentaiClient client;
		internal ICollection<string> firstImagePage;

		public Gallery(ExhentaiClient client, int id, string token)
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
		public ImageResource GetImage(int page)
		{
			throw new NotImplementedException();
		}
	}
}

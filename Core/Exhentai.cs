using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Core
{
	public class Exhentai
	{
		const string GALLERY_RE_TEXT = @"^https://exhentai.org/g/(\d+)/(\w+)/?$";

		static readonly Regex COST = new Regex(@"You are currently at <strong>(\d+)</strong> towards");
		static readonly Regex GALLERY = new Regex(GALLERY_RE_TEXT, RegexOptions.Compiled);

		readonly ExhentaiHttpClient client;

		public Exhentai(ExhentaiHttpClient client)
		{
			this.client = client;
		}

		public async Task<string[]> GetList(FilterOptions options, int page)
		{
			if(page < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(page));
			}
			var html = await client.RequestPage($"https://exhentai.org/?page={page}&" + options.ToString());
			throw new NotImplementedException();
		}

		public Task<Gallery> GetGallery(string url)
		{
			var match = GALLERY.Match(url);
			if (!match.Success)
			{
				throw new ArgumentException(@"画册的URL格式不对，应当符合 " + GALLERY_RE_TEXT);
			}
			return GetGallery(int.Parse(match.Groups[1].Value), match.Groups[2].Value);
		}

		public async Task<Gallery> GetGallery(int id, string token)
		{
			// hc=1 显示全部评论
			var content = await client.RequestPage($"https://exhentai.org/g/{id}/{token}?hc=1");
			var gallery = new Gallery(client, id, token);
			GalleryParser.Parse(gallery, content);
			return gallery;
		}

		public async Task<int> GetCost()
		{
			var html = await client.RequestPage($"https://e-hentai.org/home.php");
			return int.Parse(COST.Match(html).Groups[1].Value);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
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

		readonly IExhentaiClient client;

		public Exhentai(IExhentaiClient client)
		{
			this.client = client;
		}

		public async Task Login(string username, string password)
		{
			var x = new HttpRequestMessage(HttpMethod.Post, "https://forums.e-hentai.org/index.php?act=Login&CODE=01");
			x.Headers.Referrer = new Uri("https://e-hentai.org/bounce_login.php?b=d&bt=1-1");

			x.Content = new FormUrlEncodedContent(new Dictionary<string, string>
			{
				{ "CookieDate", "1" },
				{ "b", "d" },
				{ "bt", "1-1" },
				{ "UserName", username },
				{ "PassWord", password },
				{ "ipb_login_submit", "Login!" },
			});

			using (var response = await client.Request(x))
			{
				response.EnsureSuccessStatusCode();

				var body = await response.Content.ReadAsStringAsync();
				if (!body.Contains("You are now logged in as"))
				{
					throw new ExhentaiException("登录失败");
				}
			}

			// 复制 .e-hentai 的Cookie到 .exhentai
			var collection = client.Cookies.GetCookies(new Uri(".e-hentai.org"));
			void CopyCookie(string name)
			{
				var cookie = collection[name];
				client.Cookies.Add(new Cookie(name, cookie.Value, "/", ".exhentai.org"));
			}
			CopyCookie("ipb_member_id");
			CopyCookie("ipb_pass_hash");
		}

		public void SetUser(string memberId, string passHash)
		{
			var cookies = client.Cookies;

			cookies.Add(new Cookie("ipb_member_id", memberId, "/", ".exhentai.org"));
			cookies.Add(new Cookie("ipb_pass_hash", passHash, "/", ".exhentai.org"));

			cookies.Add(new Cookie("ipb_member_id", memberId, "/", ".e-hentai.org"));
			cookies.Add(new Cookie("ipb_pass_hash", passHash, "/", ".e-hentai.org"));
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

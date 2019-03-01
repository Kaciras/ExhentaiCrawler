using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Core.Request;
using System.Linq;

namespace Core
{
	public class Exhentai
	{
		const string GALLERY_RE_TEXT = @"^https://exhentai.org/g/(\d+)/(\w+)/";

		private static readonly Regex COST = new Regex(@"You are currently at <strong>(\d+)</strong> towards");
		private static readonly Regex GALLERY = new Regex(GALLERY_RE_TEXT, RegexOptions.Compiled);

		private readonly ExhentaiClient client;

		public Exhentai(ExhentaiClient client)
		{
			this.client = client;
		}

		public async Task Login(string username, string password)
		{
			var form = new Dictionary<string, string>
			{
				{ "CookieDate", "1" },
				{ "b", "d" },
				{ "bt", "1-1" },
				{ "UserName", username },
				{ "PassWord", password },
				{ "ipb_login_submit", "Login!" },
			};

			var html = await client.NewSiteRequest("https://forums.e-hentai.org/index.php?act=Login&CODE=01")
				.ConfigureRequest(request => {
					request.Method = HttpMethod.Post;
					request.Headers.Referrer = new Uri("https://e-hentai.org/bounce_login.php?b=d&bt=1-1");
					request.Content = new FormUrlEncodedContent(form);
				})
				.ExecuteForContent();

			if (!html.Contains("You are now logged in"))
			{
				throw new ExhentaiException("登录失败");
			}

			// 复制 .e-hentai 的Cookie到 .exhentai
			void CopyCookie(CookieCollection cookies, string name)
			{
				var cookie = cookies[name];
				client.Cookies.Add(new Cookie(name, cookie.Value, "/", ".exhentai.org"));
			}

			var collection = client.Cookies.GetCookies(new Uri(".e-hentai.org"));
			CopyCookie(collection, "ipb_member_id");
			CopyCookie(collection, "ipb_pass_hash");
		}

		/// <summary>
		/// 设置与用户登录状态相关的几个Cookie。
		/// </summary>
		/// <param name="memberId">ipb_member_id 的值</param>
		/// <param name="passHash">ipb_pass_hash 的值</param>
		/// 
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
			if (page < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(page));
			}

			var param = new string[] { "page=" + page };
			var query = string.Join('&', param.Concat(options.AsParameters()));

			var html = await client.NewSiteRequest($"https://exhentai.org/?" + query).Execute();

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
			var html = await client.NewSiteRequest($"https://exhentai.org/g/{id}/{token}?hc=1").ExecuteForContent();

			var gallery = new Gallery(client, id, token);
			GalleryParser.Parse(gallery, html);
			return gallery;
		}

		public async Task<int> GetCost()
		{
			var html = await client.NewSiteRequest($"https://e-hentai.org/home.php").ExecuteForContent();
			return int.Parse(COST.Match(html).Groups[1].Value);
		}

		/// <summary>
		/// 创建一个默认的Exhentai对象，该对象使用简单的本地IP直连。
		/// </summary>
		public static Exhentai CreateDefault()
		{
			var client = new PooledExhentaiClient();
			client.AddLocalIP();
			return new Exhentai(client);
		}
	}
}

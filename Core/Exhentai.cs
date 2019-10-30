using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Core.Request;

[assembly: InternalsVisibleTo("Test")]
namespace Core
{
	public class Exhentai
	{
		public const string COOKIE_MEMBER_ID = "ipb_member_id";
		public const string COOKIE_PASS_HASH = "ipb_pass_hash";

		private static readonly Regex COST = new Regex(@"You are currently at <strong>(\d+)</strong> towards");

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
				.ConfigureRequest(request =>
				{
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
			CopyCookie(collection, COOKIE_MEMBER_ID);
			CopyCookie(collection, COOKIE_PASS_HASH);
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

			cookies.Add(new Cookie(COOKIE_MEMBER_ID, memberId, "/", ".exhentai.org"));
			cookies.Add(new Cookie(COOKIE_PASS_HASH, passHash, "/", ".exhentai.org"));

			cookies.Add(new Cookie(COOKIE_MEMBER_ID, memberId, "/", ".e-hentai.org"));
			cookies.Add(new Cookie(COOKIE_PASS_HASH, passHash, "/", ".e-hentai.org"));
		}

		public async Task<ListPage> GetList(FilterOptions options, int page)
		{
			if (page < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(page));
			}

			var param = new string[] { "page=" + page };
			var query = string.Join('&', param.Concat(options.AsParameters()));

			var response = await client.NewSiteRequest($"https://exhentai.org/?" + query).Execute();
			return ListPage.ParseHtml(response.Content);
		}

		/// <summary>
		/// 根据链接获取本子对象
		/// </summary>
		/// <param name="link">本子的链接</param>
		/// <returns>本子</returns>
		public Task<Gallery> GetGallery(GalleryLink link) => Gallery.From(client, link);

		// TODO: 没检查URI的正确性，检查与外头判断是否重了?
		public ImageResource GetImage(ImageLink link)
		{
			return new ImageResource(client, link);
		}

		public async Task<int> GetCost()
		{
			var html = await client
				.NewSiteRequest($"https://e-hentai.org/home.php")
				.ExecuteForContent();
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

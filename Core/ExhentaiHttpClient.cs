using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Core
{
	public class ExhentaiHttpClient
	{
		static readonly Regex BAN = new Regex(@"ban expires in(?: (\d+) hours)?(?: and (\d)+ minutes)?", RegexOptions.Compiled);

		readonly CookieContainer cookies;

		HttpClient client;

		ExhentaiHttpClient(CookieContainer cookies)
		{
			this.cookies = cookies;

			client = new HttpClient(new SocketsHttpHandler
			{
				AllowAutoRedirect = true,
				CookieContainer = cookies,
				AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
			});
		}

		internal async Task<HttpContent> RequestImage(string url)
		{
			var response = await client.GetAsync(url);
			response.EnsureSuccessStatusCode();
			return response.Content;
		}

		internal async Task<string> RequestPage(string url)
		{
			var request = new HttpRequestMessage(HttpMethod.Get, url);

			// 据说这两个头不同会影响返回的页面
			request.Headers.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
			request.Headers.UserAgent.ParseAdd(@"Mozilla/5.0 (Windows NT 6.3; Win64; x64; rv:65.0) Gecko/20100101 Firefox/65.0");
			request.Headers.AcceptLanguage.ParseAdd("zh,zh-CN;q=0.7,en;q=0.3");

			using (var response = await client.SendAsync(request))
			{
				var content = await response.Content.ReadAsStringAsync();
				CheckResponse(response, content);
				return content;
			}
		}

		internal async Task<JObject> RequestApi(object body)
		{
			var content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
			var response = await client.PostAsync("https://exhentai.org/api.php", content);
			var text = await response.Content.ReadAsStringAsync();

			try
			{
				return JsonConvert.DeserializeObject<JObject>(text);
			}
			catch (JsonException)
			{
				throw new ExhentaiException("API请求出错：" + text);
			}
		}

		public static ExhentaiHttpClient FromCookie(string memberId, string passHash)
		{
			var cookies = new CookieContainer();
			cookies.Add(new Cookie("ipb_member_id", memberId, "/", ".exhentai.org"));
			cookies.Add(new Cookie("ipb_pass_hash", passHash, "/", ".exhentai.org"));
			return new ExhentaiHttpClient(cookies);
		}

		public static async Task<ExhentaiHttpClient> Login(string username, string password)
		{
			var cookieContainer = new CookieContainer();
			var client = new HttpClient(new SocketsHttpHandler {
				AllowAutoRedirect = true,
				CookieContainer = cookieContainer,
				AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
			});

			var x = new HttpRequestMessage(HttpMethod.Post, "https://forums.e-hentai.org/index.php?act=Login&CODE=01");
			x.Headers.Referrer = new Uri("https://e-hentai.org/bounce_login.php?b=d&bt=1-1");

			// 据说这两个头不同会影响返回的页面
			x.Headers.UserAgent.ParseAdd(@"Mozilla/5.0 (Windows NT 6.3; Win64; x64; rv:65.0) Gecko/20100101 Firefox/65.0");
			x.Headers.AcceptLanguage.ParseAdd("zh,zh-CN;q=0.7,en;q=0.3");

			var content = new Dictionary<string, string>
			{
				{ "CookieDate", "1" },
				{ "b", "d" },
				{ "bt", "1-1" },
				{ "UserName", username },
				{ "PassWord", password },
				{ "ipb_login_submit", "Login!" },
			};
			x.Content = new FormUrlEncodedContent(content);

			var response = await client.SendAsync(x);
			response.EnsureSuccessStatusCode();

			var body = await response.Content.ReadAsStringAsync();
			if (!body.Contains("You are now logged in as"))
			{
				throw new ExhentaiException("登录失败");
			}
			return new ExhentaiHttpClient(cookieContainer);
		}

		/// <summary>
		/// 检查返回的页面，判断是否出现熊猫和封禁。
		/// </summary>
		/// <param name="response">响应</param>
		/// <param name="html">HTML页面</param>
		/// <exception cref="TemporarilyBannedException">如果被封禁了</exception>
		/// <exception cref="ExhentaiException">如果出现熊猫</exception>
		public static void CheckResponse(HttpResponseMessage response, string html)
		{
			var disposition = response.Content.Headers.ContentDisposition;
			if (disposition != null && disposition.FileName == "\"sadpanda.jpg\"")
			{
				throw new ExhentaiException("熊猫了");
			}

			var match = BAN.Match(html);
			if (match.Success)
			{
				var time = int.Parse(match.Groups[1]?.Value ?? "0");
				time += int.Parse(match.Groups[2]?.Value ?? "0") * 60;
				throw new TemporarilyBannedException(time);
			}
		}
	}
}

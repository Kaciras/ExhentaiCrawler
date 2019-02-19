using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Core.Infrastructure;

namespace Core
{
	public class ExhentaiHttpClient : IExhentaiClient
	{
		private const int TIMEOUT = 5;

		private static readonly Regex BAN = new Regex(@"ban expires in(?: (\d+) hours)?(?: and (\d)+ minutes)?", RegexOptions.Compiled);

		public CookieContainer Cookies { get; }

		private HttpClient client;

		public ExhentaiHttpClient(IWebProxy proxy = null) : this(new CookieContainer(), proxy) { }

		internal ExhentaiHttpClient(CookieContainer cookies, IWebProxy proxy)
		{
			Cookies = cookies;

			var handler = new SocketsHttpHandler
			{
				AllowAutoRedirect = false, // 对未登录的判定和Peer记录要求不自动跳转
				CookieContainer = cookies,
				AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
				ConnectTimeout = TimeSpan.FromSeconds(3),
			};
			client = new HttpClient(new RetryHandler(handler), true)
			{
				Timeout = TimeSpan.FromSeconds(TIMEOUT)
			};
		}

		public async Task<HttpResponseMessage> Request(HttpRequestMessage request)
		{
			// 据说这两个头不同会影响返回的页面
			request.Headers.UserAgent.ParseAdd(@"Mozilla/5.0 (Windows NT 6.3; Win64; x64; rv:65.0) Gecko/20100101 Firefox/65.0");
			request.Headers.AcceptLanguage.ParseAdd("zh,zh-CN;q=0.7,en;q=0.3");

			var response = await client.SendAsync(request);
			if (response.StatusCode == HttpStatusCode.Found && response.Headers.Location.Host == "forums.e-hentai.org")
			{
				throw new ExhentaiException("该请求需要登录");
			}
			return response;
		}

		public async Task<string> RequestPage(string url)
		{
			var request = new HttpRequestMessage(HttpMethod.Get, url);
			request.Headers.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");

			using (var response = await Request(request))
			{
				var content = await response.Content.ReadAsStringAsync();
				CheckResponse(response, content);
				return content;
			}
		}

		//internal async Task<JObject> RequestApi(object body)
		//{
		//	var content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
		//	var response = await client.PostAsync("https://exhentai.org/api.php", content);
		//	var text = await response.Content.ReadAsStringAsync();

		//	try
		//	{
		//		return JsonConvert.DeserializeObject<JObject>(text);
		//	}
		//	catch (JsonException)
		//	{
		//		throw new ExhentaiException("API请求出错：" + text);
		//	}
		//}

		/// <summary>
		/// 检查返回的页面，判断是否出现熊猫和封禁。
		/// </summary>
		/// <param name="response">响应</param>
		/// <param name="html">HTML页面</param>
		/// <exception cref="BannedException">如果被封禁了</exception>
		/// <exception cref="ExhentaiException">如果出现熊猫</exception>
		public static void CheckResponse(HttpResponseMessage response, string html)
		{
			var disposition = response.Content.Headers.ContentDisposition;
			if (disposition != null && disposition.FileName == "\"sadpanda.jpg\"")
			{
				throw new ExhentaiException("该请求需要登录");
			}
			if (html.Length > 3000)
			{
				return; // 封禁响应内容短，直接判断长度即可否定
			}
			var match = BAN.Match(html);
			if (match.Success)
			{
				var time = int.Parse(match.Groups[1]?.Value ?? "0");
				time += int.Parse(match.Groups[2]?.Value ?? "0") * 60;
				throw new BannedException(time);
			}
		}

		public void Dispose() => client.Dispose();
	}
}

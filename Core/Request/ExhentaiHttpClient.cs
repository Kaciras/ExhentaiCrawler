using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Core.Infrastructure;

namespace Core.Request
{
	public class ExhentaiHttpClient : ExhentaiClient
	{
		private const int TIMEOUT = 3;

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
				ResponseDrainTimeout = TimeSpan.FromSeconds(3),
			};
			client = new BrowserLikeHttpClient(handler)
			{
				Timeout = TimeSpan.FromSeconds(TIMEOUT)
			};
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

		public Task<T> Request<T>(ExhentaiRequest<T> request)
		{
			return request.Execute(client);
		}

		public void Dispose() => client.Dispose();
	}
}

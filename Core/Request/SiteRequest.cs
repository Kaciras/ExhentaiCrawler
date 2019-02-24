using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Core.Request
{
	// 内置的 HttpRequestHeaders 做了很大的建模，这里也不想在搞一套了，所以将配置部分暴露出来
	public delegate void RequestConfigurer(HttpRequestMessage request);

	public delegate T ResponseHandler<T>(HttpResponseMessage response, string body);

	public class SiteRequest<T> : ExhentaiRequest<T>
	{
		private static readonly Regex BAN = new Regex(@"ban expires in(?: (\d+) hours)?(?: and (\d)+ minutes)?", RegexOptions.Compiled);

		public int Cost { get; set; }
		public Uri Uri { get; }

		private RequestConfigurer RrequestConfigurer { get; set; }

		private readonly ResponseHandler<T> responseHandler;

		public SiteRequest(Uri uri, ResponseHandler<T> responseHandler)
		{
			Uri = uri;
			this.responseHandler = responseHandler;
		}

		public async Task<T> Execute(HttpClient client)
		{
			// HttpCompletionOption.ResponseHeadersRead 太长了写在下面不好看
			const HttpCompletionOption HEADERS_READ = HttpCompletionOption.ResponseHeadersRead;

			var request = new HttpRequestMessage(HttpMethod.Get, Uri);
			RrequestConfigurer?.Invoke(request);

			using (var response = await client.SendAsync(request, HEADERS_READ))
			{
				if((int)response.StatusCode >= 400)
				{
					throw new HttpStatusException((int)response.StatusCode);
				}
				var body = await response.Content.ReadAsStringAsync();
				CheckResponse(response, body);
				return responseHandler(response, body);
			}
		}

		/// <summary>
		/// 检查返回的页面，判断是否出现熊猫和封禁。
		/// </summary>
		/// <param name="response">响应</param>
		/// <param name="body">响应内容</param>
		/// <exception cref="BannedException">如果被封禁了</exception>
		/// <exception cref="ExhentaiException">如果出现熊猫</exception>
		public static void CheckResponse(HttpResponseMessage response, string body)
		{
			var disposition = response.Content.Headers.ContentDisposition;
			if (disposition?.FileName == "\"sadpanda.jpg\"")
			{
				throw new ExhentaiException("该请求需要登录");
			}
			if (response.StatusCode == HttpStatusCode.Found && response.Headers.Location.Host == "forums.e-hentai.org")
			{
				throw new ExhentaiException("该请求需要登录");
			}

			if (body.Length > 3000)
			{
				return; // 封禁响应内容短，直接判断长度即可否定
			}
			var match = BAN.Match(body);
			if (match.Success)
			{
				var time = int.Parse(match.Groups[1]?.Value ?? "0");
				time += int.Parse(match.Groups[2]?.Value ?? "0") * 60;
				throw new BannedException(time);
			}
		}
	}
}

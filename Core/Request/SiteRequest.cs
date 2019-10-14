using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Core.Request
{
	// 内置的 HttpRequestHeaders 做了很大的建模，并且这个类还没有公共的构造方法。
	// 所以这里也不想在搞一套了，直接将配置部分暴露出来
	public delegate void RequestConfigurer(HttpRequestMessage request);

	public delegate T ResponseHandler<T>(SiteResponse response);

	/// <summary>
	/// 表示想E绅士网站发送的请求，包括页面、API等，不包括静态资源的下载。
	/// </summary>
	/// <typeparam name="T">响应类型</typeparam>
	public class SiteRequest<T> : ExhentaiRequest<T>
	{
		private static readonly Regex BAN = new Regex(@"ban expires in(?: (\d+) hours)?(?: and (\d)+ minutes)?", RegexOptions.Compiled);

		public int Cost { get; set; }

		private RequestConfigurer RrequestConfigurer { get; set; }

		private readonly ResponseHandler<T> responseHandler;
		private readonly Uri uri;

		public SiteRequest(Uri uri, ResponseHandler<T> responseHandler)
		{
			this.uri = uri;
			this.responseHandler = responseHandler;
		}

		public async Task<T> Execute(IPRecord iPRecord, HttpClient client)
		{
			// HttpCompletionOption.ResponseHeadersRead 太长了写在下面不好看
			const HttpCompletionOption HEADERS_READ = HttpCompletionOption.ResponseHeadersRead;

			var request = new HttpRequestMessage(HttpMethod.Get, uri);
			RrequestConfigurer?.Invoke(request);

			using var response = await client.SendAsync(request, HEADERS_READ);
			if ((int)response.StatusCode >= 400)
			{
				throw new HttpStatusException(response.StatusCode);
			}

			// 要判断是不是封禁必须得检查响应体？
			var body = await response.Content.ReadAsStringAsync();
			CheckResponse(response, body);
			return responseHandler(new SiteResponse(iPRecord, response, body));
		}

		/// <summary>
		/// 检查返回的页面，判断是否出现熊猫和封禁。
		/// </summary>
		/// <param name="response">响应</param>
		/// <param name="body">响应内容</param>
		/// <exception cref="ExhentaiException">如果出现熊猫</exception>
		/// <exception cref="BanException">如果被封禁了</exception>
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
				throw new BanException(time);
			}
		}
	}
}

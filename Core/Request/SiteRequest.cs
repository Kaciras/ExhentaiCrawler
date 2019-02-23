using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Core.Request
{
	public abstract class SiteRequest<T> : ExhentaiRequest<T>
	{
		private static readonly Regex BAN = new Regex(@"ban expires in(?: (\d+) hours)?(?: and (\d)+ minutes)?", RegexOptions.Compiled);

		public int Cost { get; set; }

		public bool GFW => uri.Host == "exhentai.org";

		protected Uri uri;

		public SiteRequest(Uri uri)
		{
			this.uri = uri;
		}

		public async Task<T> Execute(HttpClient httpClient)
		{
			var response = await httpClient.SendAsync(CreateRequestMessage(), HttpCompletionOption.ResponseHeadersRead);
			var body = await response.Content.ReadAsStringAsync();
			CheckResponse(response, body);
			return HandleResponse(response, body);
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

		protected abstract HttpRequestMessage CreateRequestMessage();

		protected abstract T HandleResponse(HttpResponseMessage response, string body);
	}
}

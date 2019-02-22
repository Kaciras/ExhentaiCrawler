using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Infrastructure
{
	/// <summary>
	/// 为请求设置一些常见的头部，使其更像浏览器。
	/// </summary>
	public class BrowserLikeHttpClient : HttpClient
	{
		private const string ACCEPT = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
		private const string USER_AGENT = "Mozilla/5.0 (Windows NT 6.3; Win64; x64; rv:65.0) Gecko/20100101 Firefox/65.0";
		private const string ACCEPT_LANGUAGE = "zh,zh-CN;q=0.7,en;q=0.3";

		public BrowserLikeHttpClient(HttpMessageHandler handler) : base(handler, true) { }

		public override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			var headers = request.Headers;

			if(headers.Accept.Count == 0)
			{
				request.Headers.Accept.ParseAdd(ACCEPT);
			}

			// 使用 DoNotTrack 来代替Referer头
			if (headers.Referrer == null)
			{
				headers.Add("DNT", "1");
			}

			// 据说这两个头会影响返回的页面
			if (headers.AcceptLanguage.Count == 0)
			{
				headers.AcceptLanguage.ParseAdd(ACCEPT_LANGUAGE);
			}
			if (headers.UserAgent.Count == 0)
			{
				headers.UserAgent.ParseAdd(USER_AGENT);
			}

			return base.SendAsync(request, cancellationToken);
		}
	}
}

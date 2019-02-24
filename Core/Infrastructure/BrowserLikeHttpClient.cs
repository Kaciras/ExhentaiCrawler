using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Infrastructure
{
	/// <summary>
	/// 为请求设置一些常见的头部，使其更像浏览器。
	/// 由于DNT头需要避开Referrer，所以需要重写SendAsync方法。
	/// </summary>
	public class BrowserLikeHttpClient : HttpClient
	{
		private const string ACCEPT = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
		private const string USER_AGENT = "Mozilla/5.0 (Windows NT 6.3; Win64; x64; rv:65.0) Gecko/20100101 Firefox/65.0";
		private const string ACCEPT_LANGUAGE = "zh,zh-CN;q=0.7,en;q=0.3";

		public BrowserLikeHttpClient(HttpMessageHandler handler) : base(handler, true)
		{
			DefaultRequestHeaders.Accept.ParseAdd(ACCEPT);
			DefaultRequestHeaders.AcceptLanguage.ParseAdd(ACCEPT_LANGUAGE);
			DefaultRequestHeaders.UserAgent.ParseAdd(USER_AGENT);
		}

		public override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			// 如果没有 Referrer 头，就设置一个 DoNotTrack 来代替
			if (request.Headers.Referrer == null)
			{
				request.Headers.Add("DNT", "1");
			}
			return base.SendAsync(request, cancellationToken);
		}
	}
}

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
		private const string ACCEPT = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
		private const string USER_AGENT = "Mozilla/5.0 (Windows NT 6.3; Win64; x64; rv:65.0) Gecko/20100101 Firefox/65.0";
		private const string ACCEPT_LANGUAGE = "zh,zh-CN;q=0.7,en;q=0.3";

		public CookieContainer Cookies { get; }

		public TimeSpan Timeout
		{
			get => client.Timeout;
			set => client.Timeout = value;
		}

		private readonly HttpClient client;

		public ExhentaiHttpClient(IWebProxy proxy = null) : this(new CookieContainer(), proxy) { }

		internal ExhentaiHttpClient(CookieContainer cookies, IWebProxy proxy)
		{
			Cookies = cookies;

			var handler = new SocketsHttpHandler
			{
				AllowAutoRedirect = false, // 对未登录的判定和Peer记录要求不自动跳转
				CookieContainer = cookies,
				AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
			};

			client = new HttpClient(handler);

			var headers = client.DefaultRequestHeaders;
			headers.Accept.ParseAdd(ACCEPT);
			headers.Add("DNT", "1");
			headers.AcceptLanguage.ParseAdd(ACCEPT_LANGUAGE);
			headers.UserAgent.ParseAdd(USER_AGENT);
		}

		public Task<T> Request<T>(ExhentaiRequest<T> request)
		{
			return request.Execute(client);
		}

		public void Dispose() => client.Dispose();
	}
}

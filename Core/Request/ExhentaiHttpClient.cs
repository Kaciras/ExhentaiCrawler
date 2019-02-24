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

			client = new BrowserLikeHttpClient(handler);			
		}

		//const int REQUEST_PER_SECOND = 5;

		//private int token = REQUEST_PER_SECOND;

		public Task<T> Request<T>(ExhentaiRequest<T> request)
		{
			// TODO: Rate limiter
			return request.Execute(client);
		}

		public void Dispose() => client.Dispose();
	}
}

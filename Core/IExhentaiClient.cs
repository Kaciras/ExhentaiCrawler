using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
	public interface IExhentaiClient : IDisposable
	{
		CookieContainer Cookies { get; }

		Task<HttpResponseMessage> Request(HttpRequestMessage request);

		Task<string> RequestPage(string url);
	}

	public static class ExhentaiClientExtention
	{
		public static Task<HttpResponseMessage> Request(this IExhentaiClient client, string uri)
		{
			return client.Request(new HttpRequestMessage(HttpMethod.Get, uri));
		}

		public static Task<HttpResponseMessage> Request(this IExhentaiClient client, Uri uri)
		{
			return client.Request(new HttpRequestMessage(HttpMethod.Get, uri));
		}
	}
}

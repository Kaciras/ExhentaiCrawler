using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Core.Request
{
	public sealed class SiteResponse
	{
		public HttpResponseHeaders Headers { get; }

		public HttpContentHeaders ContentHeaders { get; }

		public string Content { get; }

		public HttpStatusCode StatusCode { get; set; }

		public IPRecord IPRecord { get; set; }

		public SiteResponse(IPRecord iPRecord, HttpResponseMessage message, string body)
		{
			IPRecord = iPRecord;
			Headers = message.Headers;
			ContentHeaders = message.Content.Headers;
			Content = body;
			StatusCode = message.StatusCode;
		}

		/// <summary>
		/// 检查响应是否是重定向响应，并返回Location头。
		/// </summary>
		public Uri FetchRedirectLocation()
		{
			if (StatusCode == HttpStatusCode.Found || StatusCode == HttpStatusCode.Moved)
			{
				return Headers.Location;
			}
			throw new HttpStatusException(StatusCode);
		}
	}
}

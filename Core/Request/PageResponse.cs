using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Core.Request
{
	public sealed class PageResponse
	{
		public HttpResponseHeaders Headers { get; }

		public HttpContentHeaders ContentHeaders { get; }

		public string Content { get; }

		public HttpStatusCode StatusCode { get; set; }

		public PageResponse(HttpResponseMessage message, string body)
		{
			Headers = message.Headers;
			ContentHeaders = message.Content.Headers;
			Content = body;
			StatusCode = message.StatusCode;
		}
	}
}

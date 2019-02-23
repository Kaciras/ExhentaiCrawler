using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Core.Request
{
	public class HtmlPageRequest<T> : SiteRequest<T>
	{
		private readonly Func<string, T> parser;

		public HtmlPageRequest(Uri uri, Func<string, T> parser) : base(uri)
		{
			this.parser = parser;
		}

		protected override HttpRequestMessage CreateRequestMessage() => new HttpRequestMessage(HttpMethod.Get, uri);

		protected override T HandleResponse(HttpResponseMessage response, string body) => parser(body);
	}
}

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Core.Request
{
	public class HtmlPageRequest : ExhentaiRequest<string>
	{
		private readonly Uri uri;

		public HtmlPageRequest(Uri uri)
		{
			this.uri = uri;
		}

		public Task<string> Execute(HttpClient httpClient)
		{
			return httpClient.GetStringAsync(uri);
		}
	}
}

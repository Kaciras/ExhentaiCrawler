using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Core.Request
{
	public class PeerImageRequest : ExhentaiRequest<Stream>
	{
		public int Cost => 0;
		public Uri Uri { get; }

		public PeerImageRequest(string uri) : this(new Uri(uri)) { }

		public PeerImageRequest(Uri uri)
		{
			Uri = uri;
		}

		public Task<Stream> Execute(HttpClient httpClient)
		{
			return httpClient.GetStreamAsync(Uri);
		}
	}
}

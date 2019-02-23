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
		public bool GFW => false;

		private readonly Uri uri;

		public PeerImageRequest(string uri) : this(new Uri(uri)) { }

		public PeerImageRequest(Uri uri)
		{
			this.uri = uri;
		}

		public Task<Stream> Execute(HttpClient httpClient)
		{
			return httpClient.GetStreamAsync(uri);
		}
	}
}

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

		private readonly Uri uri;

		public PeerImageRequest(string uri)
		{
			this.uri = new Uri(uri);
		}

		public PeerImageRequest(Uri uri)
		{
			this.uri = uri;
		}

		public async Task<Stream> Execute(IPRecord ip, HttpClient httpClient)
		{
			return await httpClient.GetStreamAsync(uri);
		}
	}
}

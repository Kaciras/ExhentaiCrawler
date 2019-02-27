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
		public bool GfwIntercepted => false;

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
			for (int i = 0; i < 2; i++)
			{
				try
				{
					//var response = await httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);
					//response.EnsureSuccessStatusCode();

					return await httpClient.GetStreamAsync(uri);
				}
				catch (TaskCanceledException)
				{
					// TODO: 据测试这里必须要重试一次，原因未知
				}
			}
			throw new TaskCanceledException();
		}
	}
}

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Core.Request
{
	public class FluentRequest
	{
		private int cost;
		private Uri uri;

		private readonly ExhentaiClient client;

		public FluentRequest(ExhentaiClient client)
		{
			this.client = client;
		}

		public FluentRequest ForUri(string uri) => ForUri(new Uri(uri));

		public FluentRequest ForUri(Uri uri)
		{
			this.uri = uri;
			return this;
		}

		public FluentRequest WithCost(int cost)
		{
			this.cost = cost;
			return this;
		}

		public HttpResponseMessage Execute()
		{
			return Execute((resp, body) => resp);
		}

		public T Execute<T>(ResponseHandler<T> handler)
		{

		}
	}

	public static class FluentRequestExtention
	{
		public static FluentRequest NewRequest(this ExhentaiClient client)
		{
			return new FluentRequest(client);
		}
	}
}

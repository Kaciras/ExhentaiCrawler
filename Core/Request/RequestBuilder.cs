using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Core.Request
{
	public class RequestBuilder
	{
		private int cost;
		private Uri uri;
		private RequestConfigurer configurer;

		private readonly ExhentaiClient client;

		public RequestBuilder(ExhentaiClient client)
		{
			this.client = client;
		}

		public RequestBuilder ForUri(string uri)
		{
			return ForUri(new Uri(uri));
		}

		public RequestBuilder ForUri(Uri uri)
		{
			this.uri = uri;
			return this;
		}

		public RequestBuilder WithCost(int cost)
		{
			this.cost = cost;
			return this;
		}

		public RequestBuilder ConfigureRequest(RequestConfigurer configurer)
		{
			this.configurer = configurer;
			return this;
		}

		#region 执行请求的方法

		public Task<string> ExecuteForContent()
		{
			return Execute((resp, body) => body);
		}

		public Task<PageResponse> Execute()
		{
			return Execute((resp, body) => new PageResponse(resp, body));
		}

		public Task<T> Execute<T>(ResponseHandler<T> handler)
		{
			var request = new SiteRequest<T>(uri, handler);
			request.Cost = cost;
			return client.Request(request);
		}

		#endregion
	}

	public static class ExhentaiClientExtention
	{
		public static RequestBuilder NewRequest(this ExhentaiClient client) => new RequestBuilder(client);
	}
}

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Core.Request
{
	public class SiteRequestBuilder
	{
		private int cost;
		private RequestConfigurer configurer;

		private readonly Uri uri;
		private readonly ExhentaiClient client;

		public SiteRequestBuilder(ExhentaiClient client, Uri uri)
		{
			this.client = client;
			this.uri = uri;
		}

		public SiteRequestBuilder WithCost(int cost)
		{
			this.cost = cost;
			return this;
		}

		public SiteRequestBuilder ConfigureRequest(RequestConfigurer configurer)
		{
			this.configurer = configurer;
			return this;
		}

		#region 执行请求的方法

		public Task<string> ExecuteForContent()
		{
			return Execute(resp => resp.Content);
		}

		public Task<SiteResponse> Execute()
		{
			return Execute(x => x);
		}

		public Task<T> Execute<T>(ResponseHandler<T> handler)
		{
			var request = new SiteRequest<T>(uri, handler)
			{
				Cost = cost,
				RrequestConfigurer = configurer,
			};
			return client.Request(request);
		}

		#endregion
	}

	/// <summary>
	/// 站点请求用得多，搞个扩展方便调用。
	/// </summary>
	public static class SiteRequestExtention
	{
		public static SiteRequestBuilder NewSiteRequest(this ExhentaiClient client, Uri uri)
		{
			return new SiteRequestBuilder(client, uri);
		}

		public static SiteRequestBuilder NewSiteRequest(this ExhentaiClient client, string uri)
		{
			return new SiteRequestBuilder(client, new Uri(uri));
		}
	}
}

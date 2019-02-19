using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Infrastructure
{
	public sealed class RetryHandler : DelegatingHandler
	{
		private int MaxRetries { get; set; }

		public RetryHandler(HttpMessageHandler inner, int maxRetries = 1) : base(inner)
		{
			MaxRetries = maxRetries;
		}

		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
		{
			HttpResponseMessage response = null;

			for (var i = MaxRetries; i >= 0; i--)
			{
				response = await base.SendAsync(request, token);
					return response;
			}
			return response;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Core.Request
{
	public interface ExhentaiClient : IDisposable
	{
		CookieContainer Cookies { get; }

		Task<T> Request<T>(ExhentaiRequest<T> request);
	}
}

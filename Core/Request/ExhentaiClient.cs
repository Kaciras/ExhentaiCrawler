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

		// 对于调用方来说，是否需要指定IP应该是能够确定的，所有使用重载而不是允许 iPRecord = null
		Task<T> Request<T>(ExhentaiRequest<T> request, IPRecord iPRecord);
	}
}

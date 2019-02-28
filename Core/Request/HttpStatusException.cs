using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;

namespace Core.Request
{
	public class HttpStatusException : HttpRequestException
	{
		public HttpStatusException(HttpStatusCode code) : base($"非预期的状态码{code}") { }
	}
}

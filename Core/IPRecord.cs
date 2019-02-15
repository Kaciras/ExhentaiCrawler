using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;

namespace Core
{
	public abstract class IPRecord
	{
		public static readonly IPRecord Local = new LocalIPRecord();

		/// <summary>
		/// 最近被封禁的到期时间，没有被封禁则视为在DateTime.MinValue的时间封禁。
		/// </summary>
		public DateTime BanExpires { get; internal set; } = DateTime.MinValue;

		/// <summary>
		/// 最近达到限额的时间，没有达到过限额也视为在DateTime.MinValue达到过。
		/// </summary>
		public DateTime LimitReached { get; internal set; } = DateTime.MinValue;

		public abstract void ConfigureHttpHandler(SocketsHttpHandler handler);
	}

	/// <summary>
	/// 本地IP（直连不用代理）也是一个IP资源。
	/// </summary>
	internal sealed class LocalIPRecord : IPRecord
	{
		public override void ConfigureHttpHandler(SocketsHttpHandler handler) => handler.UseProxy = false;
	}

	public sealed class WebProxyIPRecord : IPRecord
	{
		public IWebProxy Proxy { get; }

		public WebProxyIPRecord(IWebProxy proxy)
		{
			Proxy = proxy;
		}

		public override void ConfigureHttpHandler(SocketsHttpHandler handler)
		{
			handler.Proxy = Proxy;
			handler.UseProxy = true;
		}
	}
}

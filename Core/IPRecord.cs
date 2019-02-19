using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;

namespace Core
{
	internal sealed class IPRecord
	{
		/// <summary>
		/// 最近被封禁的到期时间，没有被封禁则视为在DateTime.MinValue的时间封禁。
		/// </summary>
		public DateTime BanExpires { get; set; } = DateTime.MinValue;

		/// <summary>
		/// 最近达到限额的时间，没有达到过限额也视为在DateTime.MinValue达到过。
		/// </summary>
		public DateTime LimitReached { get; set; } = DateTime.MinValue;

		/// <summary>
		/// 是否处于中国特色网络环境
		/// </summary>
		public bool GFW { get; }

		public IWebProxy Proxy { get; }

		public ExhentaiHttpClient Client { get; set; }

		public IPRecord(IWebProxy proxy, bool GFW)
		{
			Proxy = proxy;
			this.GFW = GFW;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Core;
using Core.Request;

namespace Cli
{
	public class ClientConfig
	{
		public bool EnableDirectConnection { get; set; }
		public IList<ProxyEntry> Proxies = Array.Empty<ProxyEntry>();

		public AuthCookies Cookies { get; set; }

		public Exhentai GetExhentai()
		{
			var client = new PooledExhentaiClient();
			var exhentai = new Exhentai(client);

			if (EnableDirectConnection)
			{
				client.AddLocalIP();
			}

			Proxies
				.Select(e => new WebProxy(e.Host, e.Port))
				.ForEach(proxy => client.AddProxy(proxy));

			if (Cookies != null)
			{
				exhentai.SetUser(Cookies.MemberId, Cookies.PassHash);
			}

			return exhentai;
		}
	}

	public sealed class AuthCookies
	{
		public string MemberId { get; set; }
		public string PassHash { get; set; }

		public AuthCookies() { }

		public AuthCookies(string id, string hash)
		{
			MemberId = id;
			PassHash = hash;
		}
	}

	public class ProxyEntry
	{
		public string Host { get; set; }
		public int Port { get; set; }
	}
}

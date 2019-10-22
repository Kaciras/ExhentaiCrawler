using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Core;
using Core.Request;
using Newtonsoft.Json;

namespace Cli
{
	public class ExhentaiConfig
	{
		const string FILE_NAME = "config.json";

		public bool EnableDirectConnection { get; set; } = true;

		public IList<ProxyEntry> Proxies { get; set; } = Array.Empty<ProxyEntry>();

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

		public void Save()
		{
			using var writer = new JsonTextWriter(new StreamWriter(FILE_NAME));
			JsonSerializer.Create().Serialize(writer, this);
		}

		public static ExhentaiConfig Load()
		{
			try
			{
				using var reader = new JsonTextReader(new StreamReader(FILE_NAME));
				return JsonSerializer.Create().Deserialize<ExhentaiConfig>(reader);
			}
			catch(IOException)
			{
				return new ExhentaiConfig();
			}
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

		public ProxyEntry() { }

		public ProxyEntry(string host, int port)
		{
			Host = host;
			Port = port;
		}
	}
}

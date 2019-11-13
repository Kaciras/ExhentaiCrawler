using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommandLine;
using Core;

// TODO: 怎么做交互？
namespace Cli
{
	[Verb("config", HelpText = "配置模式，通过交互的会话来修改配置文件")]
	public sealed class ConfigMode : RunMode
	{
		public async Task Start()
		{
			//var cookies = new AuthCookies("2723232", "67674c89175c751095d4c840532e6363");
			var cookies = await SelectCookies();

			var config = new ExhentaiConfig
			{
				Cookies = cookies,
				Proxies = new ProxyEntry[] { new ProxyEntry("localhost", 2081) },
			};
			config.Save();
			Console.WriteLine("保存配置文件");
		}

		private static async Task<AuthCookies> SelectCookies()
		{
			var candidate = new List<AuthCookies>();

			foreach (var item in BrowserInterop.GetFirefoxProfiles())
			{
				var auth = await BrowserInterop.InspectFirefox(item.Item2);
				if (auth != null)
				{
					Console.WriteLine($"Firefox - {item.Item1}");
					Console.WriteLine($"{Exhentai.COOKIE_MEMBER_ID}={auth.MemberId}");
					Console.WriteLine($"{Exhentai.COOKIE_PASS_HASH}={auth.PassHash}");
					candidate.Add(auth);
				}
			}

			var chrome = await BrowserInterop.InspectChrome();
			if (chrome != null)
			{
				Console.WriteLine("Chrome：");
				Console.WriteLine($"{Exhentai.COOKIE_MEMBER_ID}={chrome.MemberId}");
				Console.WriteLine($"{Exhentai.COOKIE_PASS_HASH}={chrome.PassHash}");
				candidate.Add(chrome);
			}

			return candidate.Count > 0 ? candidate[0] : null;
		}
	}
}

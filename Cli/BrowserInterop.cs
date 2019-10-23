using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;

namespace Cli
{
	public static class BrowserInterop
	{
		static readonly Regex SECTION = new Regex(@"^\[\w+\]$");

		static AuthCookies GetAuthCookies(IDictionary<string, string> dict)
		{
			var hasId = dict.TryGetValue("ipb_member_id", out var id);
			var hasPass = dict.TryGetValue("ipb_pass_hash", out var pass);
			return (hasId && hasPass) ? new AuthCookies(id, pass) : null;
		}

		static async Task<Dictionary<K, V>> CreatDict<K, V>(IAsyncEnumerable<KeyValuePair<K, V>> iter)
		{
			var result = new Dictionary<K, V>();
			await foreach (var kv in iter)
			{
				((ICollection<KeyValuePair<K, V>>)result).Add(kv);
			}
			return result;
		}

		/// <summary>
		/// 搜索Firefox的配置，返回一个以（配置名，路径）为元素的课枚举对象。
		/// 
		/// 所有的配置记录在：[用户目录]\AppData\Roaming\Mozilla\Firefox\profiles.ini
		/// 其中 [Profile*] 的段表示一个配置，*为从0开始的整数。
		/// </summary>
		/// <returns>全部的配置</returns>
		public static IList<(string, string)> EnumaerateFirefoxProfiles()
		{
			var appDataRoaming = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			var profileIni = Path.Join(appDataRoaming, @"Mozilla\Firefox\profiles.ini");

			// 此方法不能使用yield，因为会导致Reader提前关闭，并且yield不能再try-catch块里。
			try
			{
				using var reader = new StreamReader(profileIni);
				var list = new List<(string, string)>();

				string line;
				string name = null;
				string path = null;

				while ((line = reader.ReadLine()) != null)
				{
					if (SECTION.IsMatch(line) && name != null)
					{
						list.Add((name, path));
						name = null;
					}

					var kv = line.Split('=', 2);
					if (kv.Length == 2)
					{
						switch (kv[0])
						{
							case "Name":
								name = kv[1];
								break;
							case "Path":
								path = kv[1];
								break;
						}
					}
				}

				if (name != null)
				{
					list.Add((name, path));
				}
				return list;
			}
			catch (FileNotFoundException)
			{
				return Array.Empty<(string, string)>();
			}
		}

		public static async Task<AuthCookies> InspectFirefox(string profile)
		{
			try
			{
				await using var reader = new FirefoxCookieReader(profile);
				await reader.Open();
				var cookies = await CreatDict(reader.Read("e-hentai.org"));
				return GetAuthCookies(cookies);
			}
			catch (SqliteException)
			{
				return null; // 文件不存在 ERROR CODE = 14
			}
		}

		public static async Task<AuthCookies> InspectChrome()
		{
			try
			{
				await using var reader = new ChromeCookieReader();
				await reader.Open();
				var cookies = await CreatDict(reader.Read("e-hentai.org"));
				return GetAuthCookies(cookies);
			}
			catch (SqliteException)
			{
				return null;
			}
		}
	};
}

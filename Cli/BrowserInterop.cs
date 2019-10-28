using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Core;
using Microsoft.Data.Sqlite;
using SpecialFolder = System.Environment.SpecialFolder;

namespace Cli
{
	public static class BrowserInterop
	{

		static AuthCookies GetAuthCookies(IDictionary<string, string> dict)
		{
			var hasId = dict.TryGetValue(Exhentai.COOKIE_MEMBER_ID, out var id);
			var hasPass = dict.TryGetValue(Exhentai.COOKIE_PASS_HASH, out var pass);
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

		// 这个方法抽出来为了对比性能
		internal static IList<(string, string)> ParseProfiles(string text)
		{
			var list = new List<(string, string)>();
			string name = null;
			string path = null;

			var reader = new QuickIniTokenizer(text);
			while (reader.Read())
			{
				if (reader.TokenType == IniToken.Section && name != null)
				{
					list.Add((name, path));
					name = path = null;
				}
				else if (reader.TokenType == IniToken.Key)
				{
					// 【注意】不能使用 == 来比较Span的内容
					if (reader.CurrentValue.SequenceEqual("Name"))
					{
						name = new string(reader.ReadValue());
					}
					else if (reader.CurrentValue.SequenceEqual("Path"))
					{
						path = new string(reader.ReadValue());
					}
				}
			}
			if (name != null)
			{
				list.Add((name, path));
			}
			return list;
		}

		/// <summary>
		/// 搜索Firefox的配置，返回一个以（配置名，路径）为元素的课枚举对象。
		/// 
		/// 所有的配置记录在：[用户目录]\AppData\Roaming\Mozilla\Firefox\profiles.ini
		/// 其中 [Profile*] 的段表示一个配置，*为从0开始的整数。
		/// </summary>
		/// <returns>全部的配置</returns>
		public static IList<(string, string)> GetFirefoxProfiles()
		{
			try
			{
				var roaming = Environment.GetFolderPath(SpecialFolder.ApplicationData);
				var file = Path.Join(roaming, @"Mozilla\Firefox\profiles.ini");
				return ParseProfiles(File.ReadAllText(file));
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

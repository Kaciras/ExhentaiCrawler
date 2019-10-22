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
		static readonly Regex PROFILE_SECTION = new Regex(@"^\[\w+\]$");

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

		public static IEnumerable<(string, string)> EnumaerateFirefoxProfiles()
		{
			var appDataRoaming = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			var profileIni = $@"{appDataRoaming}\Mozilla\Firefox\profiles.ini";
			using var reader = new StreamReader(new FileStream(profileIni, FileMode.Open));

			string name = null;
			string path = null;

			while (!reader.EndOfStream)
			{
				var line = reader.ReadLine();

				if (PROFILE_SECTION.IsMatch(line) && name != null)
				{
					yield return (name, path);
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
				yield return (name, path);
			}
		}

		public static async Task<AuthCookies> InspectFirefox(string profile)
		{
			try
			{
				await using var reader = new FirefoxCookieReader(profile);
				await reader.Open();
				var cookies = await CreatDict(reader.ReadCookies("e-hentai.org"));
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
				var cookies = await CreatDict(reader.ReadCookies("e-hentai.org"));
				return GetAuthCookies(cookies);
			}
			catch (SqliteException)
			{
				return null;
			}
		}
	};
}

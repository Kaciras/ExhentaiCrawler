using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using System.Security.Cryptography;

namespace Cli
{
	public static class BrowserInterop
	{
		public static async Task<AuthCookies> InspectFirefox()
		{
			var appDataRoaming = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			var profileIni = $@"{appDataRoaming}\Mozilla\Firefox\profiles.ini";
			using var reader = new StreamReader(new FileStream(profileIni, FileMode.Open));

			while (!reader.EndOfStream)
			{
				var line = await reader.ReadLineAsync();
				var kv = line.Split('=', 2);

				if (kv.Length == 2 && kv[0] == "Path")
				{
					var cookies = await SelectCookies(kv[1] + @"\cookies.sqlite", "e-hentai.org");

					if (cookies.TryGetValue("ipb_member_id", out var id) && 
						cookies.TryGetValue("ipb_pass_hash", out var pass))
					{
						return new AuthCookies(id, pass);
					}
				}
			}

			return null;
		}

		static async Task<IDictionary<string, string>> SelectCookies(string file, string domain)
		{
			using var db = new SqliteConnection("Filename=" + file);
			await db.OpenAsync();

			var command = new SqliteCommand("SELECT name,value FROM moz_cookies WHERE baseDomain=@domain", db);
			command.Parameters.AddWithValue("@domain", domain);

			using var reader = await command.ExecuteReaderAsync();
			var cookies = new Dictionary<string, string>();

			while (await reader.ReadAsync())
			{
				cookies[reader.GetString(0)] = reader.GetString(1);
			}

			return cookies;
		}

		public static async Task<AuthCookies> InspectChrome()
		{
			var appDataLocal = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
			var file = appDataLocal + @"\Google\Chrome\User Data\Default\Cookies";

			var cookies = new Dictionary<string, string>(ReadCookies(file, "e-hentai.org"));

			if (cookies.TryGetValue("ipb_member_id", out var id) &&
				cookies.TryGetValue("ipb_pass_hash", out var pass))
			{
				return new AuthCookies(id, pass);
			}

			return null;
		}

		public static IEnumerable<KeyValuePair<string, string>> ReadCookies(string file, string domain)
		{
			if (!File.Exists(file))
			{
				throw new FileNotFoundException("Cant find cookie store", file);
			}
			using var conn = new SqliteConnection("Filename=" + file);
			conn.Open();

			using var command = conn.CreateCommand();
			command.CommandText = "SELECT name,encrypted_value FROM cookies WHERE host_key = @domain";
			command.Parameters.AddWithValue("domain", domain);

			using var reader = command.ExecuteReader();
			while (reader.Read())
			{
				var encryptedData = (byte[])reader[1];
				var decodedData = ProtectedData.Unprotect(encryptedData, null, DataProtectionScope.CurrentUser);
				var plainText = Encoding.UTF8.GetString(decodedData);

				yield return KeyValuePair.Create(reader.GetString(0), plainText);
			}
		}
	}
}

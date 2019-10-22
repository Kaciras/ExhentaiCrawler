using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace Cli
{
	public class ChromeCookieReader : BrowserCookieReader
	{
		readonly DbConnection db;
		readonly DbCommand command;

		public ChromeCookieReader()
		{
			var appDataLocal = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
			var file = appDataLocal + @"\Google\Chrome\User Data\Default\Cookies";
			db = new SqliteConnection($"Filename={file};Mode=ReadOnly");

			command = db.CreateCommand();
			command.CommandText = "SELECT name,encrypted_value FROM cookies WHERE host_key=@domain";
			command.Parameters.Add(new SqliteParameter("@domain", SqliteType.Text));
		}

		/// <summary>
		/// 读取Chrome的Cookie存储文件，从中找出符合指定域名的Cookies。
		/// 
		/// Chrome的Cookie文件里，值是使用DPAPI加密存储的，需要使用当前用户解密。
		/// host_key 如果是一级域则要加上前面的点。
		/// </summary>
		/// <param name="domain">域名</param>
		/// <returns>可迭代的键值对</returns>
		public async IAsyncEnumerable<KeyValuePair<string, string>> ReadCookies(string domain)
		{
			if (domain.Count(c => c == '.') == 1)
			{
				domain = "." + domain;
			}
			command.Parameters["@domain"].Value = domain;

			using var reader = await command.ExecuteReaderAsync();
			while (await reader.ReadAsync())
			{
				var encryptedData = (byte[])reader[1];
				var decodedData = ProtectedData.Unprotect(encryptedData, null, DataProtectionScope.CurrentUser);
				var plainText = Encoding.UTF8.GetString(decodedData);

				yield return KeyValuePair.Create(reader.GetString(0), plainText);
			}
		}

		public Task Open() => db.OpenAsync();

		public async ValueTask DisposeAsync() => await db.CloseAsync();
	}
}

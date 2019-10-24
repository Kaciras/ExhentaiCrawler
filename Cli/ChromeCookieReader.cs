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
	/// <summary>
	/// 读取Chrome浏览器当前用户的Cookies文件的类。
	/// </summary>
	public class ChromeCookieReader : CookieReader
	{
		readonly DbConnection db;
		readonly DbCommand command;

		// Chrome的Cookie存储在单个Sqlite数据库文件里，每个用户都有自己的存储，位置在：
		// [用户目录]\AppData\Local\Google\Chrome\User Data\Default\Cookies
		// 
		// Chrome的Cookie值是用 Data Protection API(DPAPI) 加密存储的，以用户的属性作为密钥，
		// 所以只能读取当前用户的Cookie。
		public ChromeCookieReader()
		{
			var appDataLocal = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
			var file = appDataLocal + @"\Google\Chrome\User Data\Default\Cookies";
			db = new SqliteConnection($"Filename={file};Mode=ReadOnly");

			// 有一个 host_key + name + path 的唯一索引，故要以host_key为条件查询
			command = db.CreateCommand();
			command.CommandText = "SELECT name,encrypted_value FROM cookies WHERE host_key=@domain";
			command.Parameters.Add(new SqliteParameter("@domain", SqliteType.Text));
		}

		/// <summary>
		/// 从Chrome的Cookie文件中找出指定域名的Cookies。
		/// 
		/// 1) encrypted_value 的值是使用DPAPI加密的，需要使用当前用户解密。
		/// 2) host_key 如果是一级域则要加上前面的点。
		/// </summary>
		/// <param name="domain">域名</param>
		/// <returns>可迭代的键值对</returns>
		public async IAsyncEnumerable<KeyValuePair<string, string>> Read(string domain)
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

		public ValueTask DisposeAsync() => db.DisposeAsync();
	}
}

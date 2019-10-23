using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace Cli
{
	/// <summary>
	/// 读取Firefox浏览器的Cookies文件的类。
	/// 
	/// Firefox的Cookie存储在配置目录下的cookies.sqlite文件中，使用Sqlite数据库格式。
	/// </summary>
	public class FirefoxCookieReader : BrowserCookieReader
	{
		readonly DbConnection db;
		readonly DbCommand command;

		public FirefoxCookieReader(string profile)
		{
			db = new SqliteConnection($@"Filename={profile}\cookies.sqlite;Mode=ReadOnly");

			// 有一个 baseDomain + originAttributes 的索引，故要以baseDomain为条件查询
			command = db.CreateCommand();
			command.CommandText = "SELECT name,value FROM moz_cookies WHERE baseDomain=@domain";
			command.Parameters.Add(new SqliteParameter("@domain", SqliteType.Text));
		}
		
		public async IAsyncEnumerable<KeyValuePair<string, string>> Read(string domain)
		{
			command.Parameters["@domain"].Value = domain;

			using var reader = await command.ExecuteReaderAsync();
			while (await reader.ReadAsync())
			{
				yield return KeyValuePair.Create(reader.GetString(0), reader.GetString(1));
			}
		}

		public Task Open() => db.OpenAsync();

		public ValueTask DisposeAsync() => db.DisposeAsync();
	}
}

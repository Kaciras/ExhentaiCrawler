using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace Cli
{
	public class FirefoxCookieReader : BrowserCookieReader
	{
		readonly DbConnection db;
		readonly DbCommand command;

		public FirefoxCookieReader(string profile)
		{
			db = new SqliteConnection($@"Filename={profile}\cookies.sqlite;Mode=ReadOnly");

			command = db.CreateCommand();
			command.CommandText = "SELECT name,value FROM moz_cookies WHERE baseDomain=@domain";
			command.Parameters.Add(new SqliteParameter("@domain", SqliteType.Text));
		}
		
		public async IAsyncEnumerable<KeyValuePair<string, string>> ReadCookies(string domain)
		{
			command.Parameters["@domain"].Value = domain;

			using var reader = await command.ExecuteReaderAsync();
			while (await reader.ReadAsync())
			{
				yield return KeyValuePair.Create(reader.GetString(0), reader.GetString(1));
			}
		}

		public Task Open() => db.OpenAsync();

		public async ValueTask DisposeAsync() => await db.CloseAsync();
	}
}

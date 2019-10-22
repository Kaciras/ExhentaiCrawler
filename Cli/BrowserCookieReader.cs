using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Cli
{
	interface BrowserCookieReader : IAsyncDisposable
	{
		Task Open();

		IAsyncEnumerable<KeyValuePair<string, string>> ReadCookies(string domain);
	}
}

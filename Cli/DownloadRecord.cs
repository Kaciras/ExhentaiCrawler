using System;
using System.Collections.Generic;
using System.Text;

namespace Cli
{
	class DownloadRecord
	{
		public IDictionary<int, string> IdMap { get; set; }
		public IDictionary<int, int> Versions { get; set; }
	}
}

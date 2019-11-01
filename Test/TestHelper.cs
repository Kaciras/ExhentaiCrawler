using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace Test
{
	public class TestHelper
	{
		public static JsonElement Setting { get; }

		static TestHelper()
		{
			var file = "settings.json";
			if (File.Exists("settings.local.json"))
			{
				file = "settings.local.json";
			}
			JsonDocument doc = JsonDocument.Parse(File.ReadAllText(file));
			Setting = doc.RootElement;
		}
	}
}

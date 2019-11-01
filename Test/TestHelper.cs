using System.IO;
using System.Text.Json;

namespace Test
{
	public class TestHelper
	{
		public static JsonElement Setting { get; }

		/*
		 * Microsoft.Extension.Configuration 试了下不好用，首先它兼容各种格式导致只能取String值；
		 * 并且判断属性存在需要 GetSection("name").Value != null 很丑。
		 * 
		 * 最常见的是用环境变量，但一旦IDE配置重置就得重设一遍太麻烦，所以我还是用文件 + gitignore。
		 */
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

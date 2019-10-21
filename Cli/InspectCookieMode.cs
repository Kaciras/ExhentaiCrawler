using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace Cli
{
	[Verb("inspect", HelpText = "搜索浏览器的中用于登陆E绅士网站的Cookie")]
	public sealed class InspectCookieMode : RunMode
	{
		[Option('s', "save", HelpText = "将Cookie记录到配置文件")]
		public bool Save { get; set; }

		public async Task Start()
		{
			var firefox = await BrowserInterop.InspectFirefox();
			if (firefox != null)
			{
				Console.WriteLine("Firefox：");
				Console.WriteLine("ipb_member_id=" + firefox.MemberId);
				Console.WriteLine("ipb_pass_hash=" + firefox.PassHash);
			}

			var chrome = await BrowserInterop.InspectChrome();
			if (chrome != null)
			{
				Console.WriteLine("Chrome：");
				Console.WriteLine("ipb_member_id=" + firefox.MemberId);
				Console.WriteLine("ipb_pass_hash=" + firefox.PassHash);
			}
		}
	}
}

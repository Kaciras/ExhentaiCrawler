using Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
	[TestClass]
    public class ExhentaiHttpClientTest
    {
		readonly ExhentaiHttpClient client;

		public ExhentaiHttpClientTest()
		{
			var cookies = new CookieContainer();
			cookies.Add(new Cookie("ipb_member_id", "2723232", "/", ".exhentai.org"));
			cookies.Add(new Cookie("ipb_pass_hash", "67674c89175c751095d4c840532e6363", "/", ".exhentai.org"));
			client = new ExhentaiHttpClient(cookies, null);
		}

		[TestMethod]
		public async Task Panda()
		{
			var invaildClient = new ExhentaiHttpClient();
			try
			{
				await invaildClient.RequestPage("https://exhentai.org/");
				Assert.Fail("没看见熊猫");
			}
			catch (ExhentaiException e)
			{
				Assert.AreEqual("该请求需要登录", e.Message);
			}
		}

		[TestMethod]
		public async Task RequestPage()
		{
			var html = await client.RequestPage("https://exhentai.org/g/518681/2aa630b122");
			StringAssert.Contains(html, "[田中あじ] アンスイート 寝取られ堕ちた女たち");
		}
    }
}

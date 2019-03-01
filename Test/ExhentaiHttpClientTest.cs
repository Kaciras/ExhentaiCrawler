using Core;
using Core.Request;
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
		readonly PooledExhentaiClient client;

		public ExhentaiHttpClientTest()
		{
			client = new PooledExhentaiClient();
			client.AddLocalIP();
			client.Cookies.Add(new Cookie("ipb_member_id", "2723232", "/", ".exhentai.org"));
			client.Cookies.Add(new Cookie("ipb_pass_hash", "67674c89175c751095d4c840532e6363", "/", ".exhentai.org"));
		}

		[TestMethod]
		public async Task Panda()
		{
			var invaildClient = new PooledExhentaiClient();
			invaildClient.AddLocalIP();
			try
			{
				await invaildClient.NewSiteRequest("https://exhentai.org/").Execute();
				Assert.Fail("未登录访问exhentai却没出异常");
			}
			catch (ExhentaiException e)
			{
				Assert.AreEqual("该请求需要登录", e.Message);
			}
		}

		[TestMethod]
		public async Task RequestPage()
		{
			var html = await client.NewSiteRequest("https://exhentai.org/g/518681/2aa630b122").ExecuteForContent();
			StringAssert.Contains(html, "[田中あじ] アンスイート 寝取られ堕ちた女たち");
		}
    }
}

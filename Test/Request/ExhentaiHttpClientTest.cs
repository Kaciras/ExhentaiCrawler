using System.Net;
using System.Threading.Tasks;
using Core;
using Core.Request;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Request
{
	[TestClass]
    public class ExhentaiHttpClientTest
    {
		private readonly PooledExhentaiClient client;

		public ExhentaiHttpClientTest()
		{
			client = new PooledExhentaiClient();
			client.AddLocalIP();

			var id = TestHelper.Setting.GetProperty("member_id").GetString();
			client.Cookies.Add(new Cookie("ipb_member_id", id, "/", ".exhentai.org"));

			var pass = TestHelper.Setting.GetProperty("pass_hash").GetString();
			client.Cookies.Add(new Cookie("ipb_pass_hash", pass, "/", ".exhentai.org"));
		}

		[TestMethod]
		public async Task Panda()
		{
			using var invaildClient = new PooledExhentaiClient();
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
			var html = await client
				.NewSiteRequest("https://exhentai.org/g/518681/2aa630b122")
				.ExecuteForContent();
			StringAssert.Contains(html, "[田中あじ] アンスイート 寝取られ堕ちた女たち");
		}
    }
}

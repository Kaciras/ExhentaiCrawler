using System.Net;
using System.Threading.Tasks;
using Core;
using Core.Request;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test
{
	[TestClass]
	public class ExhentaiTest
	{
		private static ExhentaiClient GetClient()
		{
			var setting = TestHelper.Setting;
			var client = new PooledExhentaiClient();

			if (setting.TryGetProperty("proxy", out var proxy))
			{
				var host = proxy.GetProperty("host").GetString();
				var port = proxy.GetProperty("port").GetInt32();
				client.AddProxy(new WebProxy(host, port));
			}
			return client;
		}

		[TestMethod]
		public async Task GetCost()
		{
			var client = GetClient();
			var exhentai = new Exhentai(client);
			exhentai.SetUser(
				TestHelper.Setting.GetProperty("member_id").GetString(),
				TestHelper.Setting.GetProperty("pass_hash").GetString());

			var cost = await exhentai.GetCost();

			Assert.IsTrue(cost >= 0);
			Assert.IsTrue(cost <= 5000);
		}

		[TestMethod]
		public async Task LoginFail()
		{
			var client = GetClient();
			var exhentai = new Exhentai(client);

			try
			{
				// qweasdzxc - qweasdzxc 瞎几把输入一个还蒙对了
				await exhentai.Login("qweasdzxc", "222");
				Assert.Fail("Expect to throw an exception");
			}
			catch (ExhentaiException e)
			{
				Assert.AreEqual("登录失败", e.Message);
			}
		}
	}
}

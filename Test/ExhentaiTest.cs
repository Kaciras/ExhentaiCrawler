using Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
	[TestClass]
	public class ExhentaiTest
	{
		readonly Exhentai exhentai = new Exhentai(ExhentaiHttpClient.FromCookie("2723232", "67674c89175c751095d4c840532e6363"));

		[TestMethod]
		public async Task GetCost()
		{
			var cost = await exhentai.GetCost();
			Assert.IsTrue(cost >= 0);
			Assert.IsTrue(cost <= 5000);
		}
	}
}

﻿using Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
	[TestClass]
	public class ExhentaiTest
	{
		[TestMethod]
		public async Task GetCost()
		{
			var exhentai = new Exhentai(new ExhentaiHttpClient(new WebProxy("localhost", 2080)));
			exhentai.SetUser("2723232", "67674c89175c751095d4c840532e6363");

			var cost = await exhentai.GetCost();
			Assert.IsTrue(cost >= 0);
			Assert.IsTrue(cost <= 5000);
		}

		[TestMethod]
		public async Task LoginFail()
		{
			var exhentai = new Exhentai(new ExhentaiHttpClient(new WebProxy("localhost", 2080)));
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

using Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Test
{
	[TestClass]
	public class UnitTest1
	{
		[TestMethod]
		public void FilterOptionsDefault()
		{
			var options = new FilterOptions();
			var defaultFilterText = "f_doujinshi=1&f_manga=1&f_artistcg=1&f_gamecg=1&f_western=1" +
				"&f_non-h=1&f_imageset=1&f_cosplay=1&f_asianporn=1&f_misc=1&f_search=&f_apply=Apply+Filter";

			// Uri params is order insensitive
			var expect = new HashSet<string>(defaultFilterText.Split("&"));
			var artucal = new HashSet<string>(options.ToString().Split("&"));
			Assert.IsTrue(expect.SetEquals(artucal));
		}

		[TestMethod]
		public void FilterOptions()
		{

		}

		[TestMethod]
		public async Task Panda()
		{
			var client = new ExhentaiClient("123", "qweasdzxc");
			try
			{
				await client.GetGallery(1360036, "b43a1a73e1");
				Assert.Fail("Ã»¿´¼ûÐÜÃ¨");
			}
			catch(ExhentaiException e)
			{
				Assert.AreEqual("ÐÜÃ¨ÁË", e.Message);
			}
		}

		// proxy
		[TestMethod]
		public async Task LoginFail()
		{
			try
			{
				await ExhentaiClient.Login("qweasdzxc", "qweasdzxc");
				Assert.Fail("Expect to throw an exception");
			}
			catch(ExhentaiException e)
			{
				Assert.AreEqual("µÇÂ¼Ê§°Ü", e.Message);
			}
		}
	}
}

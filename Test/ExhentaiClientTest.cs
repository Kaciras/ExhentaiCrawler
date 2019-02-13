using Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
	[TestClass]
    public class ExhentaiClientTest
    {
		readonly ExhentaiClient client = new ExhentaiClient("2723232", "67674c89175c751095d4c840532e6363");

		[TestMethod]
		public async Task RequestPage()
		{
			var html = await client.RequestPage("https://exhentai.org/g/518681/2aa630b122");
			StringAssert.Contains(html, "[田中あじ] アンスイート 寝取られ堕ちた女たち");
		}
    }
}

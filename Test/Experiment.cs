using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Test
{
	[TestClass]
	public sealed class Experiment
	{
		/// <summary>
		/// 没办法使用一个Regex就搞定
		/// </summary>
		[TestMethod]
		public void RegexCapture()
		{
			var re = new Regex(@"(?:(?<name1>A(BC))|(?<name2>C(DE)))", RegexOptions.Compiled);
			var text = "ACDE";

			var match = re.Match(text);

			// 返回Match时就进行了一次匹配
			Assert.IsTrue(match.Success);

			Assert.AreNotEqual("name2", match.Name);
			Assert.AreEqual(1, match.Captures.Count);
			Assert.AreSame(match, match.Captures[0]);
		}

		[TestMethod]
		public void NetstedCaptures()
		{
			var re = new Regex(@"A_(?<C0>BC(?<C1>DE))", RegexOptions.Compiled);
			var text = "A_BCDEF";

			var match = re.Match(text);

			Assert.IsTrue(match.Captures.Count > 0);
		}
	}
}

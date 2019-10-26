using System.IO;
using Cli;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test
{
	[TestClass]
	public class QuickIniTokenizerTest
	{
		private void AssertToken(ref QuickIniTokenizer tokenizer, IniToken type, string value)
		{
			Assert.IsTrue(tokenizer.Read());
			Assert.AreEqual(type, tokenizer.TokenType);
			Assert.AreEqual(value, new string(tokenizer.CurrentValue));
		}

		[TestMethod]
		public void MyTestMethod()
		{
			var buffer = File.ReadAllText(@"WebArchive/IniTokenizerTest.ini");
			var instance = new QuickIniTokenizer(buffer);

			AssertToken(ref instance, IniToken.Comment, " 用于测试 QuickIniTokenizer 的样例文件");
			AssertToken(ref instance, IniToken.Comment, " This file is a simple for test QuickIniTokenizer");
						 
			AssertToken(ref instance, IniToken.Section, "Kaciras");
			AssertToken(ref instance, IniToken.Key, "Looks");
			AssertToken(ref instance, IniToken.Value, "Handsome");
			AssertToken(ref instance, IniToken.Key, "key");
			AssertToken(ref instance, IniToken.Value, @"value\no\escape");

			AssertToken(ref instance, IniToken.Section, "SimpleIniTokenizer");
			AssertToken(ref instance, IniToken.Comment, " comment before option");
			AssertToken(ref instance, IniToken.Key, "Option.Alias");
			AssertToken(ref instance, IniToken.Value, "Property");
			AssertToken(ref instance, IniToken.Key, "Enable-Fast-preasing");

			Assert.IsFalse(instance.Read());
		}
	}
}

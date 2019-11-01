using System;
using System.IO;
using System.Threading.Tasks;
using Cli.Ini;
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

		private delegate void ThrowsAction(ref QuickIniTokenizer tokenizer);

		// ref struct 只能存在于栈上，不能被lambda捕获，故没法用Assert.ThrowsException
		private void AssertThrow<E>(ref QuickIniTokenizer tokenizer, ThrowsAction action) where E : Exception
		{
			try
			{
				action(ref tokenizer);
				Assert.Fail("Except to throw exception");
			}
			catch (Exception e)
			{
				if (e.GetType() != typeof(E))
				{
					Assert.Fail($"Except to throw {typeof(E)} but got {e}");
				}
			}
		}

		[TestMethod]
		public void ReadTokens()
		{
			var buffer = File.ReadAllText("WebArchive/IniTokenizerTest.ini");
			var instance = new QuickIniTokenizer(buffer, true);

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

		[TestMethod]
		public void InvalidSection()
		{
			var instance = new QuickIniTokenizer("[Section", true);
			AssertThrow<IniParsingException>(ref instance, (ref QuickIniTokenizer r) => r.Read());
		}

		[TestMethod]
		public void Relay()
		{
			var instance = new QuickIniTokenizer("[Sec");
			Assert.IsFalse(instance.Read());
			Assert.AreEqual(0, instance.Consumed);
		}

		[TestMethod]
		public void Relay2()
		{
			var instance = new QuickIniTokenizer("A  =  123");
			instance.Read();

			Assert.IsFalse(instance.Read());
			Assert.AreEqual(3, instance.Consumed);

		}

		[TestMethod]
		public void Relay3()
		{
			var instance = new QuickIniTokenizer("A  =  123", true);
			instance.Read();

			AssertToken(ref instance, IniToken.Value, "123");
		}

		[TestMethod]
		public void Relay4()
		{
			var instance = new QuickIniTokenizer("A  =  123");
			instance.Read();

			var nextBlock = "A  =  123"[instance.Consumed..^0] + "456";
			instance = new QuickIniTokenizer(nextBlock, true, instance.CurrentState);

			AssertToken(ref instance, IniToken.Value, "123456");
		}

		
	}
}

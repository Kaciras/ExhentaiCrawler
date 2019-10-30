using System;
using System.Threading;
using Core.Request;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Request
{
	[TestClass]
	public sealed class ProxyPoolTest
	{
		private readonly ProxyPool pool = new ProxyPool();

		[TestMethod]
		public void Get()
		{
			var ip0 = new IPRecord(null);
			var ip1 = new IPRecord(null);
			pool.Add(ip0);
			pool.Add(ip1);

			Assert.IsTrue(pool.TryGetAvailable(0, out var got0));
			Assert.AreSame(ip1, got0);

			Assert.IsTrue(pool.TryGetAvailable(0, out var got1));
			Assert.AreSame(ip0, got1);

			Assert.IsTrue(pool.TryGetAvailable(0, out var got2));
			Assert.AreSame(ip1, got2);
		}

		[TestMethod]
		public void GetEmpty()
		{
			Assert.IsFalse(pool.TryGetAvailable(0, out _));
		}

		[TestMethod]
		public void Remove()
		{
			var ip = new IPRecord(null);
			pool.Add(ip);
			ip.Removed = true;

			Assert.IsFalse(pool.TryGetAvailable(0, out _));
		}

		[TestMethod]
		public void Banned()
		{
			var ip = new IPRecord(null);
			pool.Add(ip);

			ip.BanExpires = DateTime.Now + TimeSpan.FromMilliseconds(30);

			// 要查询两次，第一次移动到封禁队列，第二次测试从封禁队列查询
			Assert.IsFalse(pool.TryGetAvailable(0, out _));
			Assert.IsFalse(pool.TryGetAvailable(0, out _));

			Thread.Sleep(50);
			Assert.IsTrue(pool.TryGetAvailable(0, out _));
		}

		[TestMethod]
		public void LimitionReached()
		{
			var ip = new IPRecord(null);
			pool.Add(ip);

			ip.LimitReached = DateTime.Now - TimeSpan.FromMinutes(2);

			// 与上面一样需要查询两次
			Assert.IsFalse(pool.TryGetAvailable(100, out _));
			Assert.IsFalse(pool.TryGetAvailable(100, out _));

			Assert.IsTrue(pool.TryGetAvailable(5, out _));
		}
	}
}

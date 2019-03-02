using System;
using System.Collections.Generic;
using System.Threading;
using Core.Request;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test
{
	[TestClass]
	public sealed class ProxyPoolTest
	{
		private readonly ProxyPool pool = new ProxyPool();

		/// <summary>
		/// 用于忽略 TryGetAvailable 的 out 参数
		/// </summary>
		private IPRecord ignore;

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
			Assert.IsFalse(pool.TryGetAvailable(0, out ignore));
		}

		[TestMethod]
		public void Remove()
		{
			var ip = new IPRecord(null);
			pool.Add(ip);
			ip.Removed = true;

			Assert.IsFalse(pool.TryGetAvailable(0, out ignore));
		}

		[TestMethod]
		public void LimitionReached()
		{
			var ip = new IPRecord(null);
			pool.Add(ip);

			ip.LimitReached = DateTime.Now - TimeSpan.FromMinutes(2);

			Assert.IsFalse(pool.TryGetAvailable(100, out ignore));
			Assert.IsTrue(pool.TryGetAvailable(5, out ignore));
		}

		[TestMethod]
		public void Banned()
		{
			var ip = new IPRecord(null);
			pool.Add(ip);

			ip.BanExpires = DateTime.Now + TimeSpan.FromMilliseconds(20);

			Assert.IsFalse(pool.TryGetAvailable(0, out ignore));
			Thread.Sleep(50);
			Assert.IsTrue(pool.TryGetAvailable(0, out ignore));
		}
	}
}

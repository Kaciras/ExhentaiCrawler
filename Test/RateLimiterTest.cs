using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Core.Infrastructure;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test
{
	[TestClass]
	public class RateLimiterTest
	{
		[TestMethod]
		public void AcquireSuccess()
		{
			var limiter = new RateLimiter(50, TimeSpan.FromMilliseconds(100));

			limiter.Acquire(45).Should().Be(0);

			Thread.Sleep(10);
			limiter.Acquire(10).Should().Be(0);
		}

		[TestMethod]
		public void AcquireFailed()
		{
			var limiter = new RateLimiter(10, TimeSpan.FromSeconds(1));

			limiter.Acquire(9);

			// 10毫秒不足以回复足够的令牌，如果令牌计算错误导致过多，则该测试将失败
			Thread.Sleep(10);
			limiter.Acquire(9).Should().Be(800);
		}

		[TestMethod]
		public void ExceedLimit()
		{
			var limiter = new RateLimiter(10, TimeSpan.FromSeconds(1));

			Action acquire = () => limiter.Acquire(15);
			acquire.Should().Throw<ArgumentException>();
		}

		[TestMethod]
		public void SuccessAfterWait()
		{
			var limiter = new RateLimiter(100, TimeSpan.FromSeconds(1));
			limiter.Acquire(50);

			// 可能会有点误差，但不应该差的太大
			var wait = limiter.Acquire(55);
			wait.Should().BeInRange(49.9, 51);

			Thread.Sleep(TimeSpan.FromMilliseconds(wait));
			limiter.Acquire(55).Should().BeLessOrEqualTo(0);
		}

		[TestMethod]
		public void Smooth()
		{
			var limiter = new RateLimiter(1000, TimeSpan.FromMilliseconds(200));

			for (int i = 100 ; i > 0; i--)
			{
				Thread.Sleep(2);
				limiter.Acquire(20).Should().BeLessOrEqualTo(0);
			}
		}

		[TestMethod]
		public void Bursty()
		{
			var limiter = new RateLimiter(100, TimeSpan.FromSeconds(1));

			for (int i = 100; i > 0; i--)
			{
				limiter.Acquire(1).Should().BeLessOrEqualTo(0);
			}

			limiter.Acquire(1).Should().BeGreaterThan(0);
		}
	}
}

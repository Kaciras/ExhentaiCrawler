using System;
using Core.Infrastructure;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using NSubstitute.Extensions;

namespace Test
{
	[TestClass]
	public class RateLimiterTest
	{
		private static readonly DateTime BASE_TIME = DateTime.UnixEpoch;

		[TestMethod]
		public void ConstructorError()
		{
			Assert.ThrowsException<ArgumentOutOfRangeException>(() => new RateLimiter(-3.1, 10));
			Assert.ThrowsException<ArgumentOutOfRangeException>(() => new RateLimiter(10, -1));
		}

		[TestMethod]
		public void ZeroSize()
		{
			var limiter = new RateLimiter(0, 1);

			limiter.Acquire(0).Should().Be(0);
			Assert.ThrowsException<ArgumentOutOfRangeException>(() => limiter.Acquire(double.Epsilon));
		}

		[TestMethod]
		public void ZeroRate()
		{
			var clock = Substitute.For<Clock>();
			clock.Now.Returns(BASE_TIME);
			var limiter = new RateLimiter(1, 0, clock);

			limiter.Acquire(1).Should().Be(0);

			clock.Configure().Now.Returns(DateTime.MaxValue);
			limiter.Acquire(1).Should().Be(double.PositiveInfinity);
		}

		[TestMethod]
		public void AcquireSuccess()
		{
			var clock = Substitute.For<Clock>();
			clock.Now.Returns(BASE_TIME);
			var limiter = new RateLimiter(50, 10, clock);

			limiter.Acquire(10).Should().Be(0);
			limiter.Acquire(20).Should().Be(0);
			limiter.Acquire(10).Should().Be(0);
		}

		[TestMethod]
		public void AcquireFailed()
		{
			var clock = Substitute.For<Clock>();
			clock.Now.Returns(BASE_TIME);
			var limiter = new RateLimiter(20, 5, clock);

			limiter.Acquire(15);

			// 1秒不足以回复足够的令牌，如果令牌计算错误导致填充过多，则该测试将失败
			clock.Now.Returns(BASE_TIME + TimeSpan.FromSeconds(1));
			limiter.Acquire(15).Should().Be(1000);
		}

		[TestMethod]
		public void SuccessAfterWait()
		{
			var clock = Substitute.For<Clock>();
			clock.Now.Returns(BASE_TIME);
			var limiter = new RateLimiter(100, 1, clock);

			limiter.Acquire(50);

			var wait = limiter.Acquire(55);
			wait.Should().BeApproximately(5000, 0.1);

			clock.Configure().Now.Returns(BASE_TIME + TimeSpan.FromMilliseconds(wait));
			limiter.Acquire(55).Should().Be(0);
		}

		[TestMethod]
		public void Smooth()
		{
			var clock = Substitute.For<Clock>();
			clock.Now.Returns(BASE_TIME);
			var limiter = new RateLimiter(100, 1, clock);

			for (int i = 99 ; i > 0; i--)
			{
				limiter.Acquire(2).Should().Be(0);

				var after = clock.Now + TimeSpan.FromSeconds(1);
				clock.Configure().Now.Returns(after);
			}
		}

		[TestMethod]
		public void Bursty()
		{
			var limiter = new RateLimiter(100, 1);

			for (int i = 100; i > 0; i--)
			{
				limiter.Acquire(1).Should().Be(0);
			}

			limiter.Acquire(1).Should().BeGreaterThan(0);
		}

		[TestMethod]
		public void TryAcquireSuccess()
		{
			var clock = Substitute.For<Clock>();
			clock.Now.Returns(BASE_TIME);
			var limiter = new RateLimiter(100, 1, clock);

			var fail = limiter.TryAcquireFailed(50, out var wait);
			fail.Should().BeFalse();
			wait.Should().Be(TimeSpan.Zero);
		}

		[TestMethod]
		public void TryAcquireFailed()
		{
			var clock = Substitute.For<Clock>();
			clock.Now.Returns(BASE_TIME);
			var limiter = new RateLimiter(100, 1, clock);

			limiter.Acquire(100);

			var fail = limiter.TryAcquireFailed(50, out var wait);
			fail.Should().BeTrue();
			wait.Should().Be(TimeSpan.FromSeconds(50));
		}
	}
}

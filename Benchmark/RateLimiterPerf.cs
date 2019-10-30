using System;
using BenchmarkDotNet.Attributes;
using Core.Infrastructure;

namespace Benchmark
{
	[RankColumn]
	public class RateLimiterPerf
	{
		private RateLimiter limiter;

		[GlobalSetup]
		public void Setup()
		{
			limiter = new RateLimiter(4e7, 1e6);
		}

		[Benchmark]
		public double Acquire()
		{
			return limiter.Acquire(1);
		}

		[Benchmark]
		public TimeSpan TryAcquireFailed()
		{
			limiter.TryAcquireFailed(1.5, out var wait);
			return wait;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Text;
using BenchmarkDotNet.Attributes;
using Core.Infrastructure;

namespace Benchmark
{
	[ClrJob, CoreJob(baseline: true)]
	[RankColumn]
	public class RateLimiterBenchmark
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

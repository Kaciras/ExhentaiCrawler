using System;
using BenchmarkDotNet.Running;

namespace Benchmark
{
	internal static class Program
	{
		private static void Main()
		{
			BenchmarkRunner.Run<RateLimiterBenchmark>();
		}
	}
}

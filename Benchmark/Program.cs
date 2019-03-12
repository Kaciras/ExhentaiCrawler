using System;
using BenchmarkDotNet.Running;

namespace Benchmark
{
	internal static class Program
	{
		private static void Main(string[] args)
		{
			BenchmarkRunner.Run<RateLimiterBenchmark>();
		}
	}
}

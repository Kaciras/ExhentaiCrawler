using System;
using BenchmarkDotNet.Attributes;
using Cli;

namespace Benchmark
{
	public class ParseRangePerf
	{
		[Benchmark]
		public Range OnlyStart()
		{
			return DownloadMode.ParseRange("8964-");
		}

		[Benchmark]
		public Range OnlyEnd()
		{
			return DownloadMode.ParseRange("8964-19260817");
		}

		[Benchmark]
		public Range StartToEnd()
		{
			return DownloadMode.ParseRange("19260817-");
		}

		[Benchmark]
		public Range All()
		{
			return DownloadMode.ParseRange("-");
		}

		[Benchmark]
		public Range OneIndex()
		{
			return DownloadMode.ParseRange("19260817");
		}
	}
}

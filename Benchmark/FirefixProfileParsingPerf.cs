using System;
using System.Collections.Generic;
using System.Linq;
using Benchmark.Properties;
using BenchmarkDotNet.Attributes;
using Cli;

// Span还是挺快的，比旧版按行检查字符串快60%
namespace Benchmark
{
	public class FirefixProfileParsingPerf
	{
		internal static IList<(string, string)> OldImpl(string[] lines)
		{
			var list = new List<(string, string)>();
			string name = null;
			string path = null;

			foreach (var line in lines)
			{
				if(line.Length == 0)
				{
					continue;
				}

				if (line[0] == '[' && line[^1] == ']' && name != null)
				{
					list.Add((name, path));
					name = path = null;
				}

				var kv = line.Split('=', 2);
				if (kv.Length == 2)
				{
					switch (kv[0])
					{
						case "Name":
							name = kv[1];
							break;
						case "Path":
							path = kv[1];
							break;
					}
				}
			}

			if (name != null)
			{
				list.Add((name, path));
			}
			return list;
		}

		private static readonly string Text = Resources.profiles;
		private static readonly string[] Lines = Resources.profiles.Split("\r\n");

		[GlobalSetup]
		public void CheckSameResult()
		{
			if(!OldImpl(Lines).SequenceEqual(BrowserInterop.ParseProfiles(Text)))
			{
				throw new Exception("Two implements return different value");
			}
		}

		[Benchmark]
		public object ByLines() => OldImpl(Lines);

		[Benchmark(Baseline = true)]
		public object BySpan() => BrowserInterop.ParseProfiles(Text);
	}
}

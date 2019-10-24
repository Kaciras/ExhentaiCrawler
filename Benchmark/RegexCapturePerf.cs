using System;
using System.Text.RegularExpressions;
using BenchmarkDotNet.Attributes;

namespace Benchmark
{
	// 差别不明显，Span 稍快一点，不过Span能避免分配而Capture.Value使用SubString。
	public class RegexCapturePerf
	{
		private static readonly Regex RE = new Regex(@"->>(\w+)<<-", RegexOptions.Compiled);

		[Params("Short ->>123<<-", "Long ->>2355678921003556789<<-")]
		public string Text { get; set; }

		[Benchmark]
		public long GroupValue()
		{
			return long.Parse(RE.Match(Text).Groups[1].Value);
		}

		[Benchmark]
		public long Span()
		{
			var group = RE.Match(Text).Groups[1];
			return long.Parse(Text.AsSpan(group.Index, group.Length));
		}
	}
}

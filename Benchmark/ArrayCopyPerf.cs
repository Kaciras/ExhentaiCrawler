using System;
using System.Linq;
using BenchmarkDotNet.Attributes;

namespace Benchmark
{
	// 经测试，这俩方法的差距很小，完全可以用更安全的Array.Copy代替Buffer.BlockCopy
	public class ArrayCopyPerf
	{
		[Params(4 * 1024 * 1024)]
		public int Size { get; set; }

		private char[] src;
		private char[] dest;

		[GlobalSetup]
		public void Fill()
		{
			if (Size == 0)
			{
				throw new Exception("Params没有注入");
			}
			dest = new char[Size];

			var r = new Random();
			src = Enumerable.Range(0, Size)
				.Select(_ => (char)r.Next(0, 65535))
				.ToArray();
		}

		[Benchmark]
		public char[] BufferBlockCopy()
		{
			Buffer.BlockCopy(src, 0, dest, 0, src.Length * sizeof(char));
			return dest;
		}

		[Benchmark]
		public char[] ArrayCopy()
		{
			Array.Copy(src, 0, dest, 0, src.Length);
			return dest;
		}
	}
}

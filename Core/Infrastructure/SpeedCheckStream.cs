using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Infrastructure
{
	public class SpeedCheckStream : Stream
	{
		public override bool CanRead => innerStream.CanRead;
		public override bool CanSeek => innerStream.CanSeek;
		public override bool CanWrite => innerStream.CanWrite;
		public override long Length => innerStream.Length;

		public override long Position
		{
			get => innerStream.Position;
			set => innerStream.Position = value;
		}

		private readonly Stream innerStream;

		private bool disposed;

		private DataSize minRead;
		private DateTime lastRead;

		public SpeedCheckStream(Stream innerStream)
		{
			this.innerStream = innerStream;
		}

		public void StartReadCheck(DataSize min)
		{
			minRead = min;
			lastRead = DateTime.Now;
		}

		private async Task ReadTimeoutLoop()
		{
			await Task.Delay(1000);
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			var length = innerStream.Read(buffer, offset, count);

			var speed = length / (DateTime.Now - lastRead).TotalMilliseconds;
			if (speed < minRead.Bytes)
			{
				var sps = new DataSize((long)speed);
				throw new IOException($"当前读取速度{sps} < {minRead}/S");
			}

			lastRead = DateTime.Now;
			return length;
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			innerStream.Write(buffer, offset, count);
		}

		protected override void Dispose(bool disposing)
		{
			disposed = true;
			innerStream.Dispose();
		}

		public override long Seek(long offset, SeekOrigin origin) => innerStream.Seek(offset, origin);
		public override void SetLength(long value) => innerStream.SetLength(value);
		public override void Flush() => innerStream.Flush();
	}
}

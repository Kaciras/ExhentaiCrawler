using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

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

		private long readLength;
		private long minRead;

		private long writeLength;

		private DateTime time;

		public SpeedCheckStream(Stream innerStream)
		{
			this.innerStream = innerStream;
		}

		public void StartReadCheck(long min)
		{
			ReadTimeout = 1000;
			minRead = min;
			time = DateTime.Now;
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			var length = innerStream.Read(buffer, offset, count);
			readLength += length;
			return length;
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			innerStream.Write(buffer, offset, count);
			writeLength += count;
		}

		public override long Seek(long offset, SeekOrigin origin) => innerStream.Seek(offset, origin);
		public override void SetLength(long value) => innerStream.SetLength(value);
		public override void Flush() => innerStream.Flush();
	}
}

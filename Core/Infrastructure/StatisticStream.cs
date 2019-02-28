using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Core.Infrastructure
{
	public class StatisticStream : Stream
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

		public DataSize ReadCount => new DataSize(readCount);
		public DataSize WriteCount => new DataSize(writeCount);

		private long readCount;
		private long writeCount;

		private readonly Stream innerStream;

		public StatisticStream(Stream innerStream)
		{
			this.innerStream = innerStream;
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			var length = innerStream.Read(buffer, offset, count);
			readCount += length;
			return length;
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			innerStream.Write(buffer, offset, count);
			writeCount += count;
		}

		public override long Seek(long offset, SeekOrigin origin) => innerStream.Seek(offset, origin);
		public override void SetLength(long value) => innerStream.SetLength(value);

		public override void Flush() => innerStream.Flush();
		protected override void Dispose(bool disposing) => innerStream.Dispose();
	}
}

using System;

// 仿照 Utf8JsonReader 写一个INI文件解析器，学习一下Span的用法。
namespace Cli
{
	public enum IniToken
	{
		None,
		Comment,
		Section,
		Key,
		Value,
	}

	// 不支持行内注释，等号和空格都是分隔符
	public ref struct SimpleIniTokenizer
	{
		public IniToken TokenType { get; private set; }
		public ReadOnlySpan<char> CurrentValue { get; private set; }

		private readonly ReadOnlySpan<char> buffer;

		private int consumed;

		public SimpleIniTokenizer(ReadOnlySpan<char> buffer)
		{
			this.buffer = buffer;
			consumed = 0;
			TokenType = IniToken.None;
			CurrentValue = ReadOnlySpan<char>.Empty;
		}

		public bool Read()
		{
			if(consumed >= buffer.Length)
			{
				return false;
			}

			switch (buffer[consumed])
			{
				case '#':
				case ';':
					ConsumeComment();
					TokenType = IniToken.Comment;
					break;
				case '[':
					ConsumeSection();
					TokenType = IniToken.Section;
					break;
				case '\r':
				case '\n':
				case '\t':
				case ' ':
					SkipWhiteSpace();
					return Read();
				case '=':
					ConsumeValue();
					TokenType = IniToken.Value;
					break;
				default:
					ConsumeKey();
					TokenType = IniToken.Key;
					break;
			}

			return true;
		}

		public void SkipWhiteSpace()
		{
			// Create local copy to avoid bounds checks.
			var local = buffer;
			for (; consumed < local.Length; consumed++)
			{
				switch (local[consumed])
				{
					case '\r':
					case '\n':
					case '\t':
					case ' ':
						break;
					default:
						return;
				}
			}
		}

		public void ConsumeComment()
		{
			var local = buffer.Slice(consumed + 1);
			var j = 0;

			for (; j < local.Length; j++)
			{
				switch (local[j])
				{
					case '\r':
					case '\n':
						goto SearchEnd;
				}
			}
		SearchEnd:
			consumed += j + 1;
			CurrentValue = local.Slice(0, j);
		}

		public void ConsumeSection()
		{
			var local = buffer.Slice(consumed + 1);
			var j = local.IndexOf(']');
			if (j < 0)
			{
				throw new Exception("数据不完整");
			}
			consumed += j + 2;
			CurrentValue = local.Slice(0, j);
		}

		public void ConsumeKey()
		{
			var local = buffer.Slice(consumed);
			var j = 0;

			for (; j < local.Length; j++)
			{
				switch (local[j])
				{
					case '\r':
					case '\n':
					case '\t':
					case ' ':
					case '=':
						goto SearchEnd;
				}
			}
		SearchEnd:
			consumed += j;
			CurrentValue = local.Slice(0, j);
		}

		public void ConsumeValue()
		{
			if (TokenType != IniToken.Key)
			{
				throw new Exception("等号出现在一行的开头");
			}

			// 前面的等号没有跳过
			consumed++;
			SkipWhiteSpace();

			var local = buffer.Slice(consumed);
			var j = 0;

			for (; j < local.Length; j++)
			{
				switch (local[j])
				{
					case '\r':
					case '\n':
						goto SearchEnd;
				}
			}
		SearchEnd:
			consumed += j;
			CurrentValue = local.Slice(0, j);
		}
	}
}

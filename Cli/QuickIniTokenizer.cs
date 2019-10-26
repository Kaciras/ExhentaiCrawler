using System;

// 仿照 Utf8JsonReader 写一个INI文件解析器，学习一下Span的用法。
namespace Cli
{
	public enum IniToken
	{
		// 用作特殊标记，比如在没有读取前的状态
		None,

		// 不支持行内注释，井号和分号是注释开头
		Comment,

		// 以方括号开头和结尾的
		Section,

		// 支持开关型选项（即没有值的），等号和空格都是值分隔符
		Key,

		// 值不包含前面的空白字符，但包括后面的
		Value,
	}

	public ref struct QuickIniTokenizer
	{
		public IniToken TokenType { get; private set; }

		public ReadOnlySpan<char> CurrentValue { get; private set; }

		private readonly ReadOnlySpan<char> buffer;

		private int consumed;

		public QuickIniTokenizer(ReadOnlySpan<char> buffer)
		{
			this.buffer = buffer;
			consumed = 0;
			TokenType = IniToken.None;
			CurrentValue = ReadOnlySpan<char>.Empty;
		}

		public bool Read()
		{
			if (consumed >= buffer.Length)
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

		// ======================== 以下是一些便捷方法 ========================

		public ReadOnlySpan<char> Get(IniToken token)
		{
			if (!Read())
			{
				throw new Exception("早就读完了");
			}
			if (TokenType != token)
			{
				throw new Exception($"Token不一致，预期{token}，实际{TokenType}");
			}
			return CurrentValue;
		}

		public ReadOnlySpan<char> GetValue() => Get(IniToken.Value);
	}
}

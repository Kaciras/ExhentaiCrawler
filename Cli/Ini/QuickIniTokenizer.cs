using System;

namespace Cli.Ini
{
	// 仿照 Utf8JsonReader 写一个INI文件解析器，学习一下Span的用法。
	public ref struct QuickIniTokenizer
	{
		public IniToken TokenType { get; private set; }

		public ReadOnlySpan<char> CurrentValue { get; private set; }

		public int Consumed { get; private set; }

		private readonly ReadOnlySpan<char> buffer;
		private readonly bool isFinalBlock;

		public QuickIniTokenizer(ReadOnlySpan<char> buffer,
			bool isFinalBlock = false, IniTokenizerState state = default)
		{
			this.buffer = buffer;
			this.isFinalBlock = isFinalBlock;
			Consumed = 0;
			TokenType = state.TokenType;
			CurrentValue = ReadOnlySpan<char>.Empty;
		}

		public IniTokenizerState CurrentState => new IniTokenizerState
		{
			TokenType = TokenType,
		};

		public bool Read()
		{
			if (Consumed >= buffer.Length)
			{
				return false;
			}

			switch (buffer[Consumed])
			{
				case '#':
				case ';':
					return ConsumeComment();
				case '[':
					return ConsumeSection();
				case '\r':
				case '\n':
				case '\t':
				case ' ':
					SkipWhiteSpace();
					return Read();
				case '=':
					return ConsumeValue();
				default:
					return ConsumeKey();
			}
		}

		public void SkipWhiteSpace()
		{
			// Create local copy to avoid bounds checks.
			var local = buffer;
			for (; Consumed < local.Length; Consumed++)
			{
				switch (local[Consumed])
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

		public bool ConsumeComment()
		{
			var local = buffer.Slice(Consumed + 1);
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

			if (!isFinalBlock)
			{
				return false;
			}

		SearchEnd:
			TokenType = IniToken.Comment;
			Consumed += j + 1;
			CurrentValue = local.Slice(0, j);
			return true;
		}

		public bool ConsumeSection()
		{
			var local = buffer.Slice(Consumed + 1);
			var j = local.IndexOf(']');

			if (j < 0)
			{
				if (isFinalBlock)
				{
					throw new IniParsingException("数据不完整");
				}
				return false;
			}

			TokenType = IniToken.Section;
			Consumed += j + 2;
			CurrentValue = local.Slice(0, j);
			return true;
		}

		public bool ConsumeKey()
		{
			var local = buffer.Slice(Consumed);
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

			if (!isFinalBlock)
			{
				return false;
			}

		SearchEnd:
			TokenType = IniToken.Key;
			Consumed += j;
			CurrentValue = local.Slice(0, j);
			return true;
		}

		public bool ConsumeValue()
		{
			if (TokenType != IniToken.Key)
			{
				throw new IniParsingException("等号出现在一行的开头");
			}

			// 前面的等号没有跳过
			var indexBuckup = Consumed;
			Consumed++;
			SkipWhiteSpace();

			var local = buffer.Slice(Consumed);
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

			if (!isFinalBlock)
			{
				Consumed = indexBuckup;
				return false;
			}

		SearchEnd:
			TokenType = IniToken.Value;
			Consumed += j;
			CurrentValue = local.Slice(0, j);
			return true;
		}

		// ======================== 以下是一些便捷方法 ========================

		public ReadOnlySpan<char> ReadToken(IniToken token)
		{
			if (!Read())
			{
				throw new IniParsingException("早就读完了");
			}
			if (TokenType != token)
			{
				throw new IniParsingException($"Token不一致，预期{token}，实际{TokenType}");
			}
			return CurrentValue;
		}

		public ReadOnlySpan<char> ReadValue() => ReadToken(IniToken.Value);

		public string GetString() => new string(CurrentValue);
	}
}

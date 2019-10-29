using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Cli.Ini
{
	public class IniSection : Dictionary<string, string> { }

	internal struct IniReadState
	{
		public IniSection section;
		public string key;
	}

	public class IniFile
	{
		public IniSection defaultSection = new IniSection();

		public IDictionary<string, IniSection> Sections = new Dictionary<string, IniSection>();

		public static IniFile Parse(ReadOnlySpan<char> data)
		{
			var iniFile = new IniFile();
			var current = new IniReadState { section = iniFile.defaultSection };
			var state = new IniTokenizerState();
			iniFile.AddTokens(data, true, ref state, ref current);
			return iniFile;
		}

		public static async Task<IniFile> Parse(TextReader reader, int bufferSize = 8192)
		{
			var iniFile = new IniFile();
			var current = new IniReadState { section = iniFile.defaultSection };

			var state = new IniTokenizerState();
			var bytesInBuffer = 0;
			var isFinalBlock = false;

			var buffer = ArrayPool<char>.Shared.Rent(bufferSize);
			try
			{
				while (true)
				{
					while (bytesInBuffer < buffer.Length)
					{
						var bytesRead = await reader.ReadAsync(buffer.AsMemory(bytesInBuffer));

						if (bytesRead == 0)
						{
							isFinalBlock = true;
							break;
						}
						bytesInBuffer += bytesRead;
					}

					var consumed = iniFile.AddTokens(
						new ReadOnlySpan<char>(buffer, 0, bytesInBuffer),
						isFinalBlock, ref state, ref current);

					bytesInBuffer -= consumed;

					if (isFinalBlock)
					{
						break;
					}
					PrepareNextBuffer(ref buffer, bytesInBuffer, consumed);
				}
			}
			finally
			{
				ArrayPool<char>.Shared.Return(buffer);
			}

			Debug.Assert(bytesInBuffer == 0);
			return iniFile;
		}

		private int AddTokens(
			ReadOnlySpan<char> buffer,
			bool isFinalBlock,
			ref IniTokenizerState tState,
			ref IniReadState rState)
		{
			var tokenizer = new QuickIniTokenizer(buffer, isFinalBlock, tState);

			while (tokenizer.Read())
			{
				switch (tokenizer.TokenType)
				{
					case IniToken.Section:
						rState.section = new IniSection();
						Sections[tokenizer.GetString()] = rState.section;
						break;
					case IniToken.Key:
						rState.key = tokenizer.GetString();
						break;
					case IniToken.Value:
						rState.section[rState.key] = tokenizer.GetString();
						break;
				}
			}

			tState = tokenizer.CurrentState;
			return tokenizer.Consumed;
		}

		// 准备好下一轮读取所用的缓冲，该方法将未使用的数据移到缓冲区的前面，并视情况调整缓冲区的大小。
		//
		// 【扩容】
		// 因为只能在Token处中断，所以如果出现超长的Token值超过缓冲容量，就必须扩容。
		// 
		// 【数据移动】
		// 官方JsonReader是定死了UTF8编码，然后对byte进行操作，但是我这用了char。
		// 如果用 Buffer.BlockCopy 移动数据，则需要乘char大小，或者用 Array.Copy。
		// 经测试 Array.Copy 性能也不比 Buffer.BlockCopy 差多少。
		//
		private static void PrepareNextBuffer(ref char[] buffer, int bytesInBuffer, int consumed)
		{
			if (bytesInBuffer > buffer.Length / 2)
			{
				var old = buffer;

				// 不能用 Math.Min 因为会溢出
				var newSize = buffer.Length < (int.MaxValue / 2) ? buffer.Length * 2 : int.MaxValue;
				buffer = ArrayPool<char>.Shared.Rent(newSize);

				Array.Copy(old, consumed, buffer, 0, bytesInBuffer);
				ArrayPool<char>.Shared.Return(old);
			}
			else
			{
				Array.Copy(buffer, consumed, buffer, 0, bytesInBuffer);
			}
		}
	}
}

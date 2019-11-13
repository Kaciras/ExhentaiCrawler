namespace Cli.Ini
{
	/// <summary>
	/// INI分词器的状态，将它传递给下一个分词器就可以继续读取。
	/// </summary>
	public struct IniTokenizerState
	{
		internal IniToken TokenType;
	}
}

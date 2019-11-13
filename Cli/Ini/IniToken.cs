namespace Cli.Ini
{
	public enum IniToken : byte
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
}

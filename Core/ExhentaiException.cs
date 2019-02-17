using System;

namespace Core
{
	/// <summary>
	/// 表示与Exhentai网站规则相关的异常，如封禁、熊猫、达到限额等。
	/// </summary>
	public class ExhentaiException : Exception
	{
		public ExhentaiException(string message) : base(message) { }
	}
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Cli
{
	/// <summary>
	/// 下载出现的异常，包括网络异常、IO错误等，用于跟代码错误产生的异常区分开。
	/// </summary>
	public class DownloadException : Exception
	{
		public DownloadException(Exception cause) : base($"下载失败，{cause.Message}", cause) { }
	}
}

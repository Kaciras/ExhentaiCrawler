using System;
using System.Collections.Generic;
using System.Text;

namespace Core
{
	/// <summary>
	/// 一个IP访问频率过快将会被封禁一段时间。
	/// </summary>
	public class TempBannedException : ExhentaiException
	{
		public TimeSpan Time { get; }
		public DateTime ReleaseTime { get; }

		public TempBannedException(int time) : base($"IP被封{time}分钟")
		{
			Time = TimeSpan.FromMinutes(time);
			ReleaseTime = DateTime.Now + Time;
		}
	}
}

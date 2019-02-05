using System;
using System.Collections.Generic;
using System.Text;

namespace Core
{
	public class TemporarilyBannedException : ExhentaiException
	{
		public TimeSpan Time { get; }
		public DateTime ReleaseTime { get; }

		public TemporarilyBannedException(int time) : base($"IP被封{time}分钟")
		{
			Time = TimeSpan.FromMinutes(time);
			ReleaseTime = DateTime.Now + Time;
		}
	}
}

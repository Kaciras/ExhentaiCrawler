using System;
using System.Collections.Generic;
using System.Text;

namespace Core
{
	public sealed class LimitReachedException : ExhentaiException
	{
		public LimitReachedException() : base("该IP或用户已经达到限额") {}
	}
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Infrastructure
{
	/// <summary>
	/// 对获取当前时间加个中间层，以便测试Mock
	/// </summary>
	public class Clock
	{
		public virtual DateTime Now => DateTime.Now;
	}
}

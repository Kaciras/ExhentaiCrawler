using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Infrastructure
{
	public class Clock
	{
		public virtual DateTime Now => DateTime.Now;
	}
}

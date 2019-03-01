using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Core.Infrastructure
{
	public sealed class AsyncResetEvent
	{
		private TaskCompletionSource<bool> source;

		public AsyncResetEvent(bool set)
		{
			source = set ? new TaskCompletionSource<bool>(true) : new TaskCompletionSource<bool>();
		}

		public Task Wait()
		{
			return source.Task;
		}

		public void Set()
		{
			source.SetResult(true);
		}

		public void Reset()
		{
			if (source.Task.IsCompleted)
			{
				source = new TaskCompletionSource<bool>();
			}
		}
	}
}

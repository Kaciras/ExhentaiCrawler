using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Infrastructure
{
	/// <summary>
	/// ManualResetEvent的异步版
	/// </summary>
	public sealed class AsyncResetEvent
	{
		/*
		 * TaskCompletionSource的 SetResult和TrySetCanceled 内部调用 TryXXX，故这里也
		 * 用相应的Try版本减少一层调用。
		 * 
		 * TaskCompletionSource的状态更改方法内部简单地调用Task的相关方法，这些方法使用
		 * 了CAS操作，所以它们是线程安全的，无需再加锁。
		 */

		private TaskCompletionSource<object> state;

		public AsyncResetEvent(bool set = false)
		{
			state = new TaskCompletionSource<object>();
			if (set)
				state.TrySetResult(null);
		}

		public bool IsSet => state.Task.IsCompleted;

		public Task Wait() => state.Task;

		public Task Wait(CancellationToken cancelToken)
		{
			if(state.Task.IsCompleted || !cancelToken.CanBeCanceled)
			{
				return state.Task;
			}
			if(cancelToken.IsCancellationRequested)
			{
				return Task.FromCanceled(cancelToken);
			}

			var taskCompletionSource = new TaskCompletionSource<object>();
			cancelToken.Register(() => taskCompletionSource.TrySetCanceled());

			return Task.WhenAny(state.Task, taskCompletionSource.Task);
		}

		public void Set()
		{
			state.TrySetResult(null);
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public void Reset()
		{
			if (state.Task.IsCompleted)
			{
				state = new TaskCompletionSource<object>();
			}
		}
	}
}

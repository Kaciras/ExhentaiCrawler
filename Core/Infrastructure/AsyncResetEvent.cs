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
		 * 用Try版本减少一层调用。
		 * 
		 * TaskCompletionSource的状态更改方法内部简单地调用Task的相关方法，这些方法使用
		 * 了CAS操作，所以它们是线程安全的，无需再加锁。
		 */

		private TaskCompletionSource<object> state;

		/// <summary>
		/// 以指定的初始状态创建实例
		/// </summary>
		/// <param name="set">初始状态</param>
		public AsyncResetEvent(bool set = false)
		{
			state = new TaskCompletionSource<object>();
			if (set)
				state.TrySetResult(null);
		}

		/// <summary>
		/// 该事件是否处于设置状态
		/// </summary>
		public bool IsSet => state.Task.IsCompleted;

		/// <summary>
		/// 等待该事件被设置
		/// </summary>
		/// <returns>等待任务</returns>
		public Task Wait() => state.Task;

		/// <summary>
		/// 等待该事件被设置，并且可以取消。
		/// 如果被取消则返回的任务将抛出TaskCanceledException
		/// </summary>
		/// <param name="cancelToken">取消令牌</param>
		/// <returns>等待任务</returns>
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

			var cancelSource = new TaskCompletionSource<object>();
			cancelToken.Register(() => cancelSource.TrySetCanceled());

			return WaitAnyAsync(state.Task, cancelSource.Task);
		}

		/// <summary>
		/// 等待多个任务里最先完成的那一个，与Task.WhenAny的差别是Task.WhenAny返回的是表示“等待其最先完成的”这一
		/// 任务，而该方法返回的是“其中最先完成的”,相当于剥去了外面的一层。
		/// </summary>
		/// <param name="tasks">多个任务</param>
		/// <returns>多个任务重最先完成的</returns>
		private static async Task WaitAnyAsync(params Task[] tasks) => await await Task.WhenAny(tasks);

		/// <summary>
		/// 设置该事件，将唤醒所有等待的任务，并且Wait方法不再挂起。
		/// </summary>
		public void Set()
		{
			state.TrySetResult(null);
		}

		/// <summary>
		/// 重置该事件，重置后Wait方法将挂起等待的任务
		/// </summary>
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

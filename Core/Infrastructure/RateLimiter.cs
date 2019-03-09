using System;
using System.Runtime.CompilerServices;

namespace Core.Infrastructure
{
	/// <summary>
	/// 基于令牌桶算法的速率限制器实现
	/// </summary>
	public class RateLimiter
	{
		private readonly int maxPermits;
		private readonly double stableInterval;

		private int stored;
		private DateTime lastAcquire;

		public RateLimiter(int permits, TimeSpan timeSpan)
		{
			stored = maxPermits = permits;
			stableInterval = timeSpan.TotalMilliseconds / maxPermits;
		}

		public bool TryAcquireFailed(int permits, out TimeSpan wait)
		{
			var mills = Acquire(permits);
			if (mills > 0)
			{
				wait = TimeSpan.FromMilliseconds(mills);
				return true;
			}
			else
			{
				wait = TimeSpan.Zero;
				return false;
			}
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public double Acquire(int permits)
		{
			var now = DateTime.Now;

			if (permits <= stored)
			{
				lastAcquire = now;
				stored -= permits;
				return 0;
			}

			// 需要的令牌数比上限还大，这个请求永远无法完成，属于异常情况
			if (permits > maxPermits)
			{
				throw new ArgumentException("请求的令牌数量大于令牌桶的上限");
			}

			// 上次拿走令牌之后又过了多久
			var restoreTime = (now - lastAcquire).TotalMilliseconds;

			// 装到需要的数量需要的时间 = 还差的令牌数 * 每令牌所需时间
			var wait = (permits - stored) * stableInterval;

			if (wait <= restoreTime)
			{
				var restore = (int)((restoreTime - wait) * stableInterval);
				lastAcquire = now;
				stored = Math.Min(stored + restore, maxPermits);
				return 0;
			}

			return wait - restoreTime;
		}
	}
}

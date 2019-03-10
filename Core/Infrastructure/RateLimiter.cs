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

		/// <summary>
		/// 每毫秒加入几个令牌（令牌/毫秒）
		/// </summary>
		private readonly double rate;

		private int stored;
		private DateTime lastAcquire;

		/// <summary>
		/// 以给定的令牌数和时间段创建一个速率限制器，其速率限制为 permits / timeSpan
		/// </summary>
		/// <param name="permits">指定时间段内填充的令牌数</param>
		/// <param name="timeSpan">时间段</param>
		public RateLimiter(int permits, TimeSpan timeSpan)
		{
			stored = maxPermits = permits;
			rate = maxPermits / timeSpan.TotalMilliseconds;
			lastAcquire = DateTime.Now;
		}

		/// <summary>
		/// 尝试获取令牌，注意该方法语义是反的，即获取失败返回true，成功返回false。
		/// 失败时wait参数将输出充满足够令牌所需的时间；成功时该参数无意义。
		/// </summary>
		/// <param name="permits">需要的令牌数</param>
		/// <param name="wait">如果方法返回true，则此参数输出充满足够令牌所需的时间</param>
		/// <returns>是否获取失败</returns>
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
			// 需要的令牌数比上限还大，这个请求永远无法完成，属于异常情况
			if (permits > maxPermits)
			{
				throw new ArgumentException("请求的令牌数量大于令牌桶的上限");
			}

			// 根据上次获取令牌的时间和剩余的令牌计算当前的令牌数：
			//     当前令牌 = 剩余令牌 + (当前时间 - 上次时间) * 每令牌所需时间，注意不能超出上限
			// 为了方便计算将其转换成int，向下取整是正确的
			var now = DateTime.Now;
			var actual = (int)((now - lastAcquire).TotalMilliseconds * rate);
			actual = Math.Min(stored + actual, maxPermits);

			if (permits <= actual)
			{
				lastAcquire = now;
				stored = actual - permits;
				return 0;
			}

			// 返回需要等待的时间 = (所需令牌数（令牌） - 当前令牌数（令牌）) / 每毫秒生成令牌数（令牌/毫秒）
			return (permits - actual) / rate;
		}
	}
}

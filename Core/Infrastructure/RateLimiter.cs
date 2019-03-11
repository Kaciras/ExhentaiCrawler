using System;
using System.Runtime.CompilerServices;

namespace Core.Infrastructure
{
	/// <summary>
	/// 基于令牌桶算法的速率限制器
	/// </summary>
	public sealed class RateLimiter
	{
		private readonly Clock clock;

		private readonly double maxPermits;

		/// <summary>
		/// 每毫秒加入几个令牌（令牌/毫秒）
		/// </summary>
		private readonly double rate;

		private double stored;
		private DateTime lastAcquire;

		/// <summary>
		/// 以给定的令牌数和时间段创建一个速率限制器
		/// </summary>
		/// <param name="maxPermits">桶容量</param>
		/// <param name="rate">每秒加入几个令牌</param>
		public RateLimiter(double maxPermits, double rate) : this(maxPermits, rate, new Clock()) { }

		internal RateLimiter(double maxPermits, double rate, Clock clock)
		{
			if(maxPermits < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(maxPermits), maxPermits, "桶容量不能为负数");
			}
			if (rate < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(rate), rate, "加入令牌的速率不能为负数");
			}

			this.clock = clock;
			this.rate = Math.Max(rate / 1000, double.Epsilon); // 下面的方法里有个除法，防止为0
			this.maxPermits = maxPermits;

			stored = maxPermits;
			lastAcquire = clock.Now;
		}

		/// <summary>
		/// 尝试获取令牌，注意该方法语义是反的，即获取失败返回true，成功返回false。
		/// 失败时wait参数将输出充满足够令牌所需的时间；成功时该参数无意义。
		/// </summary>
		/// <param name="permits">需要的令牌数</param>
		/// <param name="wait">充满足够令牌所需等待的时间</param>
		/// <returns>是否获取失败</returns>
		/// <exception cref="ArgumentOutOfRangeException">如果需要的令牌数大于桶的容量</exception>
		/// 
		public bool TryAcquireFailed(double permits, out TimeSpan wait)
		{
			var mills = Acquire(permits);
			wait = TimeSpan.FromMilliseconds(mills);
			return mills > 0;
		}

		/// <summary>
		/// 获取令牌，如果成功返回0，失败返回获取到足够令牌所需等待的时间(毫秒)
		/// </summary>
		/// <param name="permits">需要的令牌数</param>
		/// <returns>充满足够令牌所需的时间(毫秒)</returns>
		/// <exception cref="ArgumentOutOfRangeException">如果需要的令牌数大于桶的容量</exception>
		/// 
		[MethodImpl(MethodImplOptions.Synchronized)]
		public double Acquire(double permits)
		{
			// 需要的令牌数比上限还大，这个请求永远无法完成，返回值将失去意义
			if (permits > maxPermits)
			{
				throw new ArgumentOutOfRangeException(nameof(permits), "需要令牌数大于桶的容量");
			}

			// 根据上次获取令牌的时间和剩余的令牌计算当前的令牌数：
			// 当前令牌(令牌) = 剩余令牌(令牌) + (当前时间(毫秒) - 上次时间(毫秒)) * 每令牌所需时间(令牌/毫秒)
			var now = clock.Now;
			var actual = (now - lastAcquire).TotalMilliseconds * rate;
			actual = Math.Min(stored + actual, maxPermits);

			if (permits <= actual)
			{
				lastAcquire = now;
				stored = actual - permits;
				return 0;
			}

			// 返回需要等待的时间(毫秒) = (所需令牌数（令牌） - 当前令牌数（令牌）) / 每毫秒生成令牌数（令牌/毫秒）
			return (permits - actual) / rate;
		}

		#region Static Creators

		/// <summary>
		/// 创建一个速率限制器，其最多允许 duration 时间内获取 permits 个令牌
		/// </summary>
		/// <param name="permits">令牌数</param>
		/// <param name="duration">时间段</param>
		/// <returns>速率限制器</returns>
		public static RateLimiter FromDuration(double permits, TimeSpan duration)
		{
			return new RateLimiter(permits, permits / duration.TotalMilliseconds);
		}

		#endregion
	}
}

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Core.Infrastructure;

namespace Core.Request
{
	/// <summary>
	/// 该类使用了懒操作机制，对IP的修改（删除、封禁状态、配额达到）所造成的作用（移除和移动到队列等）需要在
	/// 某个请求访问到该IP时才执行。
	/// </summary>
	public class ProxyPool
	{
		private const int DEFAULT_LIMIT = 5000;

		private readonly PriorityQueue<IPRecord> banQueue = 
			new PriorityQueue<IPRecord>((a, b) => DateTime.Compare(a.BanExpires, b.BanExpires));

		private readonly PriorityQueue<IPRecord> limitQueue = 
			new PriorityQueue<IPRecord>((a, b) => DateTime.Compare(a.LimitReached, b.LimitReached));

		private readonly LinkedList<IPRecord> freeProxies = new LinkedList<IPRecord>();

		[MethodImpl(MethodImplOptions.Synchronized)]
		public void Add(IPRecord record)
		{
			freeProxies.AddFirst(record);
		}

		// TODO: 没有实现GFW
		[MethodImpl(MethodImplOptions.Synchronized)]
		public bool TryGetAvailable(int cost, out IPRecord record)
		{
			record = FindInQueue(banQueue, cost) ?? FindInQueue(limitQueue, cost);

			if (record == null)
			{
				var node = freeProxies.First;
				while (node != null)
				{
					freeProxies.RemoveFirst();
					var free = node.Value;

					if (free.Removed)
					{
						continue;
					}

					if (free.BanExpires > DateTime.Now)
					{
						banQueue.Enqueue(free);
					}
					else if (LimitAvaliable(free) < cost)
					{
						// 尽管该请求的cost可能较大，且该IP对于其它请求
						// 来说可能限额是足够的，但这里仍然要移入限额队列。
						limitQueue.Enqueue(free);
					}
					else
					{
						record = free;
						break;
					}

					node = freeProxies.First;
				}
			}

			if (record != null)
			{
				freeProxies.AddLast(record);
				return true;
			}
			return false;
		}

		private IPRecord FindInQueue(PriorityQueue<IPRecord> queue, int cost)
		{
			while (queue.Count > 0)
			{
				var available = queue.Peek();

				if (available.Removed)
				{
					queue.Dequeue();
					continue; // 清理所有被删除的IP
				}

				if (available.BanExpires < DateTime.Now && LimitAvaliable(available) >= cost)
				{
					return queue.Dequeue();
				}
				break;
			}
			return null;
		}

		/// <summary>
		/// 获取该IP可用的限额估计。注意该值仅能作为估计，因为可能有外部因素影响一个IP的
		/// 限额（如其他应用也使用了该IP去访问E绅士）。
		/// </summary>
		/// <param name="iPRecord">IP记录</param>
		/// <returns>可用的限额</returns>
		private int LimitAvaliable(IPRecord iPRecord)
		{
			var minutes = (DateTime.Now - iPRecord.LimitReached).TotalMinutes;
			return (int)Math.Min(minutes * 3, DEFAULT_LIMIT); // 防止整数溢出
		}
	}
}

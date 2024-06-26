﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Core.Infrastructure;

namespace Core.Request
{
	/// <summary>
	/// 管理IP地址，处理与E绅士网站限制策略（IP封禁，限额）有关的问题。
	/// </summary>
	internal sealed class ProxyPool
	{
		/// <summary>
		/// 默认的限额上限
		/// </summary>
		private const int DEFAULT_LIMIT = 5000;

		/*
		 * 该类使用了懒执行机制，对IP的修改（删除、封禁状态、配额达到）所造成的作用（移除和移动到队列等）需要在
		 * 某个请求获取IP时才处理，这样无需单独分配线程去管理IP状态。
		 * 
		 * TODO: 目前使用轮流策略，会使低速IP更多地被使用，需要改进
		 */

		private readonly LinkedList<IPRecord> freeProxies = new LinkedList<IPRecord>();

		private readonly PriorityQueue<IPRecord> banQueue = 
			new PriorityQueue<IPRecord>((a, b) => DateTime.Compare(a.BanExpires, b.BanExpires));

		private readonly PriorityQueue<IPRecord> limitQueue = 
			new PriorityQueue<IPRecord>((a, b) => DateTime.Compare(a.LimitReached, b.LimitReached));

		[MethodImpl(MethodImplOptions.Synchronized)]
		public void Add(IPRecord record)
		{
			freeProxies.AddFirst(record);
		}

		/// <summary>
		/// 尝试获取空闲且具有足够限额的IP。
		/// </summary>
		/// <param name="cost">需要的限额</param>
		/// <param name="record">输出IP</param>
		/// <returns>如果成功拿到</returns>
		[MethodImpl(MethodImplOptions.Synchronized)]
		public bool TryGetAvailable(int cost, out IPRecord record)
		{
			record = FindInQueue(banQueue, cost) ?? FindInQueue(limitQueue, cost) ?? FindInFree(cost);

			if (record != null)
			{
				freeProxies.AddLast(record);
			}
			return record != null;
		}

		private IPRecord FindInQueue(PriorityQueue<IPRecord> queue, int cost)
		{
			while (queue.Count > 0)
			{
				var ipRecord = queue.Peek();

				if (ipRecord.Removed)
				{
					queue.Dequeue();
				}
				else if (ipRecord.BanExpires < DateTime.Now && LimitAvaliable(ipRecord) >= cost)
				{
					return queue.Dequeue();
				}
				else
				{
					break; // 优先队列开头的都不可用，后面的一定也不行
				}
			}
			return null;
		}

		private IPRecord FindInFree(int cost)
		{
			for (var node = freeProxies.First; node != null; node = node.Next)
			{
				freeProxies.RemoveFirst();
				var iPRecord = node.Value;

				if (iPRecord.Removed)
				{
					continue;
				}

				if (iPRecord.BanExpires > DateTime.Now)
				{
					banQueue.Enqueue(iPRecord);
				}
				else if (LimitAvaliable(iPRecord) < cost)
				{
					// 尽管本次请求的cost可能较大，该IP对于其它请求
					// 来说限额可能是足够的，但这里仍然要移入限额队列。
					limitQueue.Enqueue(iPRecord);
				}
				else
				{
					return iPRecord;
				}
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

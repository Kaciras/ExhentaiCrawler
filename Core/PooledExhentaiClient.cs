﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Core.Infrastructure;

namespace Core
{
	public sealed class PooledExhentaiClient : IExhentaiClient
	{
		private const int LIMIT_PERIOD = 20;

		private readonly PriorityQueue<IPRecord> banQueue = new PriorityQueue<IPRecord>((a, b) => DateTime.Compare(a.BanExpires, b.BanExpires));
		private readonly PriorityQueue<IPRecord> limitQueue = new PriorityQueue<IPRecord>((a, b) => DateTime.Compare(a.LimitReached, b.LimitReached));

		public CookieContainer Cookies { get; } = new CookieContainer();

		public bool Rotation { get; set; }

		private readonly LinkedList<IPRecord> proxies = new LinkedList<IPRecord>();

		private bool disposed;

		public void AddProxy(IWebProxy proxy, bool GFW = false)
		{
			proxies.AddFirst(new IPRecord(proxy, GFW));
		}

		public void AddLocalIP(bool GFW = false)
		{
			proxies.AddFirst(new IPRecord(null, GFW));
		}

		public async Task<HttpResponseMessage> Request(HttpRequestMessage request)
		{
			if (TryGetAvailable(out var record))
			{
				var result = await record.Client.Request(request);
				GiveBack(record);
				return result;
			}
			throw new ExhentaiException("没有可用的IP");
		}

		public async Task<string> RequestPage(string url)
		{
			while (TryGetAvailable(out var record))
			{
				try
				{
					var result = await record.Client.RequestPage(url);
					GiveBack(record);
					return result;
				}
				catch (BannedException e)
				{
					record.BanExpires = e.ReleaseTime;
					AddToQueue(record, banQueue);
				}
				catch (LimitReachedException)
				{
					record.LimitReached = DateTime.Now;
					AddToQueue(record, limitQueue);
				}
			}
			throw new ExhentaiException("没有可用的IP");
		}

		public void Dispose()
		{
			disposed = true;
			proxies.ForEach(r => r.Client?.Dispose());
		}

		// 查找一个可用的IP
		[MethodImpl(MethodImplOptions.Synchronized)]
		private bool TryGetAvailable(out IPRecord record)
		{
			if (disposed)
			{
				throw new ObjectDisposedException(nameof(PooledExhentaiClient));
			}

			var free = proxies.First;
			if (free != null)
			{
				record = free.Value;
				proxies.RemoveFirst();
			}
			else
			{
				record = FindInQueue(banQueue) ?? FindInQueue(limitQueue);
			}

			if (record == null)
			{
				return false;
			}

			if (record.Client == null)
			{
				record.Client = new ExhentaiHttpClient(Cookies, record.Proxy);
			}
			return true;
		}
		
		private IPRecord FindInQueue(PriorityQueue<IPRecord> queue)
		{
			if (queue.Count == 0)
			{
				return null;
			}
			var now = DateTime.Now;
			var available = queue.Peek();

			if (available.BanExpires < now)
			{
				// 每分钟回复3点限额
				var costReduction = (now - available.LimitReached).Minutes * 3;
				if (costReduction > LIMIT_PERIOD)
				{
					return banQueue.Dequeue();
				}
			}
			return null;
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		private void GiveBack(IPRecord record)
		{
			if (Rotation)
				proxies.AddLast(record);
			else
				proxies.AddFirst(record);
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		private void AddToQueue(IPRecord record, PriorityQueue<IPRecord> queue)
		{
			record.Client.Dispose();
			record.Client = null;
			queue.Enqueue(record);
		}
	}
}
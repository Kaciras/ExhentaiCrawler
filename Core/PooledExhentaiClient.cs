using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
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

		// 查找一个可用的IP
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

			if(record == null)
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

		public Task<HttpResponseMessage> Request(HttpRequestMessage request)
		{
			if (TryGetAvailable(out var record))
			{
				return record.Client.Request(request);
			}
			throw new ExhentaiException("没有可用的IP");
		}

		public async Task<string> RequestPage(string url)
		{
			Exception lastException = null;

			while (TryGetAvailable(out var record))
			{
				try
				{
					var result = await record.Client.RequestPage(url);
					proxies.AddLast(record);
					return result;
				}
				catch (BannedException e)
				{
					lastException = e;
					record.Client.Dispose();
					record.Client = null;
					record.BanExpires = e.ReleaseTime;
					banQueue.Enqueue(record);
				}
				catch(LimitReachedException e)
				{
					lastException = e;
					record.Client.Dispose();
					record.Client = null;
					record.LimitReached = DateTime.Now;
					limitQueue.Enqueue(record);
				}
			}
			throw lastException;
		}

		public void Dispose()
		{
			disposed = true;
			proxies.ForEach(r => r.Client?.Dispose());
		}
	}
}

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Core.Infrastructure;

namespace Core.Request
{
	public sealed class PooledExhentaiClient : ExhentaiClient
	{
		public CookieContainer Cookies { get; } = new CookieContainer();

		private readonly ProxyPool proxyPool = new ProxyPool();
		private readonly ConcurrentDictionary<IPRecord, HttpClient> clientMap = new ConcurrentDictionary<IPRecord, HttpClient>();

		private bool disposed;

		public IPRecord AddProxy(IWebProxy proxy)
		{
			var record = new IPRecord(proxy);

			var handler = new SocketsHttpHandler
			{
				AllowAutoRedirect = false, // 对未登录的判定和Peer记录要求不自动跳转
				CookieContainer = Cookies,
				AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
				Proxy = record.Proxy,
			};

			clientMap[record] = new BrowserLikeHttpClient(handler)
			{
				Timeout = TimeSpan.FromSeconds(4),
			};

			proxyPool.Add(record);
			return record;
		}

		/// <summary>
		/// 启用本地IP
		/// </summary>
		public IPRecord AddLocalIP() => AddProxy(null);

		public void RemoveIP(IPRecord iPRecord)
		{
			if (clientMap.TryRemove(iPRecord, out var client))
			{
				client.Dispose();
				iPRecord.Removed = true; // 仅标记为删除，真正从代理池里移除要等到它被查看到
			}
			else
			{
				throw new ArgumentException("该IP不存在或已经被移除");
			}
		}

		public async Task<T> Request<T>(ExhentaiRequest<T> request)
		{
			if (disposed)
			{
				throw new ObjectDisposedException(nameof(ExhentaiClient));
			}

			while (proxyPool.TryGetAvailable(request.Cost, out var iPRecord))
			{
				try
				{
					return await SendWithIP(request, iPRecord);
				}
				catch (BanException e)
				{
					iPRecord.BanExpires = e.ReleaseTime;
				}
				catch (LimitReachedException)
				{
					iPRecord.LimitReached = DateTime.Now;
				}
			}
			throw new ExhentaiException("没有可用的IP");
		}

		public Task<T> Request<T>(ExhentaiRequest<T> request, IPRecord iPRecord)
		{
			if (disposed)
			{
				throw new ObjectDisposedException(nameof(ExhentaiClient));
			}
			return SendWithIP(request, iPRecord);
		}

		/// <summary>
		/// 若client被移除而IP还没有（见RemoveIP），则抛出ObjectDisposedException以使得与
		/// 正执行到一半的请求一致。
		/// </summary>
		private Task<T> SendWithIP<T>(ExhentaiRequest<T> request, IPRecord iPRecord)
		{
			if (clientMap.TryGetValue(iPRecord, out var client))
			{
				return request.Execute(iPRecord, client);
			}
			else
			{
				throw new ObjectDisposedException("HttpClient");
			}
		}

		public void Dispose()
		{
			disposed = true;
			clientMap.Values.ForEach(client => client.Dispose());
		}
	}
}

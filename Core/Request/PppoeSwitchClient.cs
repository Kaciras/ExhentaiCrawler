using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Http;
using Core.Infrastructure;

namespace Core.Request
{
	/// <summary>
	/// 当本地IP达到限额或被封禁时，就重新拨号切换IP.
	/// 这是当年搞爬虫缺少代理才想出的一招，仅适用于每次重新拨号后IP会改变的网络。
	/// </summary>
	public class PppoeSwitchClient : ExhentaiClient
	{
		public CookieContainer Cookies { get; } = new CookieContainer();

		private readonly string nIname;
		private readonly string user;
		private readonly string password;

		private readonly HttpClient httpClient;

		private readonly AsyncResetEvent resetEvent = new AsyncResetEvent(true);
		private readonly IPRecord localIP = new IPRecord(null);

		public PppoeSwitchClient(string nIname, string user, string password)
		{
			CheckNetworkInterface();

			this.nIname = nIname;
			this.user = user;
			this.password = password;

			// 重复代码!
			var handler = new SocketsHttpHandler
			{
				AllowAutoRedirect = false,
				CookieContainer = Cookies,
				AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
			};
			httpClient = new BrowserLikeHttpClient(handler);
		}

		private void CheckNetworkInterface()
		{
			var interfaces = NetworkInterface.GetAllNetworkInterfaces();
			var networkInterface = interfaces.First(ni => ni.Name == nIname);

			if (networkInterface == null)
			{
				throw new ArgumentException("指定的网卡不存在或被禁用");
			}
			if (networkInterface.NetworkInterfaceType != NetworkInterfaceType.Ethernet)
			{
				throw new ArgumentException("网卡类型错误");
			}
		}

		private void ReConnect()
		{
			resetEvent.Reset();
			var proc = Process.Start("Rasdial", string.Join(' ', nIname, user, password));

			proc.Exited += (sender, e) =>
			{
				proc.Dispose();
				resetEvent.Set();
			};
		}

		public async Task<T> Request<T>(ExhentaiRequest<T> request)
		{
			try
			{
				await resetEvent.Wait();
				return await request.Execute(localIP, httpClient);
			}
			catch (ExhentaiException e)
			when (e is LimitReachedException || e is BanException)
			{
				ReConnect();
				return await Request(request);
			}
		}

		public Task<T> Request<T>(ExhentaiRequest<T> request, IPRecord iPRecord) => Request(request);
	}
}

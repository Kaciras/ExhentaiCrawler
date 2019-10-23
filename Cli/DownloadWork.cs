using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Core;
using Core.Infrastructure;
using Newtonsoft.Json;

namespace Cli
{
	public sealed class DownloadWork
	{
		public const int DEFAULT_CONCURRENT = 4;
		public const int RETRY_LIMIT = 3;

		/// <summary>
		/// 下载的页面范围，默认全部页面都下载
		/// </summary>
		public Range Range { get; set; } = Range.All;

		/// <summary>
		/// 强制下载全部图片，即使已经存在
		/// </summary>
		public bool Force { get; set; }

		/// <summary>
		/// 下载线程数，注意并不是线程越多就一定越快
		/// </summary>
		public int Concurrent { get; set; } = DEFAULT_CONCURRENT;

		private readonly Gallery gallery;
		private readonly LocalGalleryStore store;
		private readonly CancellationTokenSource cancellation = new CancellationTokenSource();

		private long downloadSize;
		private int index;
		private int endIndex;

		public DownloadWork(Gallery gallery, LocalGalleryStore store)
		{
			this.gallery = gallery;
			this.store = store;
		}

		public async Task StartDownload()
		{
			var total = gallery.Info.Length;
			(index, endIndex) = Range.GetOffsetAndLength(total);
			endIndex += index;
			Console.WriteLine($"共{total}张图片，下载范围{index}-{endIndex}");

			// 启动下载线程并等待
			await Task.WhenAll(Enumerable.Range(0, Concurrent).Select(_ => RunWorker()));
			Console.WriteLine("下载任务结束，共下载了" + new DataSize(downloadSize));
		}

		private async Task RunWorker()
		{
			// Interlocked.Increment 返回增加后的值，所以是从1开始
			while (Interlocked.Increment(ref index) <= endIndex)
			{
				try
				{
					var size = await DownloadImage(index);
					Interlocked.Add(ref downloadSize, size);
				}
				catch (OperationCanceledException)
				{
					Console.WriteLine("用户取消了下载");
					return;
				}
				catch (DownloadException e)
				{
					var cause = e.InnerException;
					Console.WriteLine($"第{index}张图片下载失败：{cause.Message}");
					Console.WriteLine(cause.StackTrace);
				}
			}
		}

		/// <summary>
		/// 下载本子里第index张图片，该方法将同时被多个线程调用。
		/// </summary>
		/// <param name="index">图片序号</param>
		private async Task<long> DownloadImage(int index)
		{
			cancellation.Token.ThrowIfCancellationRequested();

			var image = await gallery.GetImage(index);
			var file = store.GetImageFile(image);

			// 如果已经下载过了就跳过
			if (!Force && file.Exists && CheckImageFile(file))
			{
				return file.Length;
			}

			// 只记录最后一次重试的异常
			Exception lastException = null;

			for (var retry = 0; retry < RETRY_LIMIT; retry++)
			{
				try
				{
					await image.Download(file.FullName, cancellation.Token);

					var time = DateTime.Now.ToLongTimeString();
					Console.WriteLine($"[{time}] 第{index}张图片下载完毕");

					return new FileInfo(file.FullName).Length;
				}
				catch (HttpRequestException e)
				{
					// 发送请求时出错，如SSL连接无法建立
					lastException = e;
				}
				catch (IOException e)
				{
					// 读取请求体的时候出错。（本地文件错误怎么办？）
					lastException = e;
				}
				catch (OperationCanceledException e)
				when (!cancellation.IsCancellationRequested)
				{
					// 超时也不抛个TimeoutException，而是取消……
					lastException = e;
				}
				Console.WriteLine($"重试下载第{index}张图({retry})");
			}

			throw new DownloadException(lastException);
		}

		public void Cancel() => cancellation.Cancel();

		/// <summary>
		/// 检查图片文件有没有损坏，TODO:无法检测出残缺的图片？
		/// </summary>
		/// <param name="file">文件对象</param>
		/// <returns>如果图片正常返回true</returns>
		public static bool CheckImageFile(FileInfo file)
		{
			try
			{
				Image.FromFile(file.FullName).Dispose();
				return true;
			}
			catch (OutOfMemoryException)
			{
				// 读取失败抛OOM异常什么鬼啦？？？
				Console.WriteLine($"无法解析的图片文件：{file.Name}");
				return false;
			}
		}
	}
}

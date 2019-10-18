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

		public Range Pages { get; set; }

		public string StorePath { get; set; }

		/// <summary>强制下载全部图片，即使已经存在</summary>
		public bool Force { get; set; }

		public int Concurrent { get; set; } = DEFAULT_CONCURRENT;

		private readonly Gallery gallery;
		private readonly CancellationTokenSource cancellation;

		private LocalGalleryStore store;

		private int index;
		private int endIndex;

		private DataSize downloadSize;

		public DownloadWork(Gallery gallery)
		{
			this.gallery = gallery;
			cancellation = new CancellationTokenSource();
		}

		public async Task StartDownload()
		{
			store = new LocalGalleryStore(StorePath, gallery);

			var name = gallery.Info.JapaneseName ?? gallery.Info.Name;
			Console.WriteLine("本子名：" + name);

			// 如果有新版就提示一下，但还是继续下载当前版本
			var newest = await gallery.GetLatestVersion();
			if (newest != gallery)
			{
				Console.WriteLine($"该本子有新版本：{newest.Uri}");
			}

			// 如果下载过旧版，就把旧版的图片都迁移过来
			var old = await GetExistsVersion();
			if (old != null)
			{
				old.MigrateTo(store);
				Console.WriteLine($"已将旧版目录[{old.Name}]合并");
			}
			else
			{
				store.Create();
			}

			(index, endIndex) = Pages.GetOffsetAndLength(gallery.Info.Length);
			endIndex += index;

			// 启动下载线程并等待
			await Task.WhenAll(Enumerable.Range(0, Concurrent).Select(_ => RunWorker()));
			Console.WriteLine("下载任务结束，共下载了" + downloadSize);
		}

		private async Task<LocalGalleryStore> GetExistsVersion()
		{
			for (var v = gallery; v != null; v = await v.GetParent())
			{
				var store = new LocalGalleryStore(StorePath, gallery);
				if (store.Exists()) return store;
			}
			return null;
		}

		private async Task RunWorker()
		{
			// Interlocked.Increment 返回增加后的值，所以是从1开始
			while (Interlocked.Increment(ref index) <= endIndex)
			{
				try
				{
					await DownloadImage(index);
				}
				catch (OperationCanceledException)
				{
					return; // 主动取消
				}
				catch (Exception e)
				{
					Console.WriteLine($"第{index}张图片下载失败：{e.Message}");
					Console.WriteLine(e.StackTrace);
				}
			}
		}

		/// <summary>
		/// 下载本子里第index张图片，该方法将同时被多个线程调用。
		/// </summary>
		/// <param name="index">图片序号</param>
		private async Task DownloadImage(int index)
		{
			cancellation.Token.ThrowIfCancellationRequested();

			var file = store.GetImageFile(index);

			if (Force && CheckImageFile(file))
			{
				return;
			}

			for (int retry = 0; retry < RETRY_LIMIT; retry++)
			{
				try
				{
					var image = await gallery.GetImage(index);
					await image.Download(file.FullName, cancellation.Token);

					var time = DateTime.Now.ToLongTimeString();
					Console.WriteLine($"[{time}] 第{index}张图片下载完毕");

					downloadSize += new DataSize(file.Length);
					break;
				}
				catch (HttpRequestException)
				{
					// 发送请求时出错，如SSL连接无法建立
				}
				catch (IOException)
				{
					// 读取请求体的时候出错。（本地文件错误怎么办？）
				}
				Console.WriteLine($"第{index}张图下载失败，重试 - {retry}");
			}
		}

		public void Cancel() => cancellation.Cancel();

		/// <summary>
		/// 检查图片文件有没有损坏，无法检测出残缺的图片？
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

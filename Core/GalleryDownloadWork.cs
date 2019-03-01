using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Core.Infrastructure;

namespace Core
{
	public sealed class GalleryDownloadWork
	{
		public const int DEFAULT_CONCURRENT = 1;

		public int? StartPage { get; set; }
		public int? EndPage { get; set; }

		public bool Force { get; set; }
		public string StorePath { get; set; }
		public bool Flatten { get; set; } // 不自动创建文件夹

		public int Concurrent { get; set; } = DEFAULT_CONCURRENT;

		private readonly Exhentai exhentai;
		private readonly string uri;
		private readonly CancellationTokenSource cancellation;

		private Gallery gallery;
		private string store;
		private ISet<string> downloaded;

		private int index;
		private DataSize downloadSize;

		public GalleryDownloadWork(Exhentai exhentai, string uri)
		{
			this.exhentai = exhentai;
			this.uri = uri;

			cancellation = new CancellationTokenSource();
		}

		public async Task Run()
		{
			gallery = await exhentai.GetGallery(uri);

			store = StorePath ?? Environment.CurrentDirectory;
			if(!Flatten)
			{
				// 以画册名创建文件夹保存，优先使用日本名
				store = Path.Combine(store, gallery.JapaneseName ?? gallery.Name);
			}
			Directory.CreateDirectory(store);
			downloaded = Force ? new SortedSet<string>() : ScanDownloaded();

			Console.WriteLine("下载图册：" + gallery.Name);
			var start = StartPage ?? 1;
			var end = EndPage ?? gallery.Length;

			// 启动下载线程
			var tasks = new Task[Concurrent];
			for (int i = 0; i < tasks.Length; i++)
			{
				tasks[i] = RunWorker();
			}

			await Task.WhenAll(tasks);
			Console.WriteLine("下载任务结束，共下载了" + downloadSize);
		}

		/// <summary>
		/// 扫描存储目录中已存在的图片文件，这些文件将被跳过不再去下载。
		/// </summary>
		/// <returns>文件名集合</returns>
		private ISet<string> ScanDownloaded()
		{
			var dirInfo = new DirectoryInfo(store);
			var exists = new HashSet<string>();

			foreach (var file in dirInfo.EnumerateFiles())
			{
				try
				{
					// 残缺文件无法检测出来？
					Image.FromFile(file.FullName).Dispose();
					exists.Add(file.Name);
				}
				catch
				{
					// 读取失败抛OOM异常什么鬼啦？？？
					Console.WriteLine($"无法解析已存在的图片文件：{file.Name}");
				}
			}
			return exists;
		}

		private async Task RunWorker()
		{
			var index = Interlocked.Increment(ref this.index);
			if (cancellation.IsCancellationRequested || index > gallery.Length)
			{
				return;
			}
			try
			{
				await DownloadImage(index);
			}
			catch(OperationCanceledException)
			{
				return;
			}
			catch(Exception e)
			{
				Console.WriteLine($"第{index}张图片下载失败：{e.Message}");
				Console.WriteLine(e.StackTrace);
			}
			await RunWorker();
		}

		private async Task DownloadImage(int index)
		{
			var image = await gallery.GetImage(index);

			if (downloaded.Contains(image.FileName))
			{
				return;
			}
			var fileToSave = Path.Combine(store, image.FileName);
			await image.Download(fileToSave, cancellation.Token);

			Console.WriteLine($"第{index}张图片{image.FileName}下载完毕");
			downloadSize += new DataSize(new FileInfo(fileToSave).Length);
		}
		
		public void Close()
		{
			cancellation.Cancel();
		}
	}
}

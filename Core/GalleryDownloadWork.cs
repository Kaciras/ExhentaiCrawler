﻿using System;
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

		private Gallery gallery;
		private string store;
		private ISet<string> downloaded;

		private int index;
		private DataSize downloadSize;

		public GalleryDownloadWork(Exhentai exhentai, string uri)
		{
			this.exhentai = exhentai;
			this.uri = uri;
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

			var tasks = new Task[Concurrent];
			for (int i = 0; i < tasks.Length; i++)
			{
				tasks[i] = RunWorker();
			}

			await Task.WhenAll(tasks);
			Console.WriteLine("下载完毕，下载了" + downloadSize);
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
					// 如果一次下载中断，可能出现不完整的文件，故必须测试读取。
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
			if (index > gallery.Length)
			{
				return;
			}
			try
			{
				await DownloadImage(index);
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

			using (var input = await GetStream(image))
			using (var output = File.OpenWrite(Path.Combine(store, image.FileName)))
			{
				var statisticStream = new StatisticStream(input);
				await statisticStream.CopyToAsync(output);

				downloadSize += statisticStream.ReadCount;
				Console.WriteLine($"第{index}张图片{image.FileName}下载完毕");
			}
		}

		// 选择图片的下载方式，优先使用与页面请求是同一个的IP
		private async Task<Stream> GetStream(ImageResource image)
		{
			var originImage = await image.GetOriginal();
			if (originImage == null)
			{
				try
				{
					return await image.GetImageStream();
				}
				catch (ObjectDisposedException)
				{
					return await image.GetImageStream(false);
				}
			}
			else
			{
				try
				{
					return await originImage.GetStream();
				}
				catch (ObjectDisposedException)
				{
					return await originImage.GetStream(false);
				}
			}
		}
	}
}

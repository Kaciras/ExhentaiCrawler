using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Core.Infrastructure;

namespace Core
{
	public sealed class GalleryDownloadWork
	{
		public const int DEFAULT_CONCURRENT = 4;
		const string STORE_PATH = @"C:\Users\XuFan\Desktop\exd";

		public int Concurrent { get; set; } = DEFAULT_CONCURRENT;

		private readonly Exhentai exhentai;

		private readonly string uri;
		private readonly int? startPage;
		private readonly int? endPage;
		private readonly bool force;

		private Gallery gallery;
		private ISet<string> downloaded;
		private int index;

		public GalleryDownloadWork(Exhentai exhentai, string uri, int? startPage, int? endPage, bool force)
		{
			this.exhentai = exhentai;
			this.uri = uri;
			this.startPage = startPage;
			this.endPage = endPage;
			this.force = force;
		}

		public async Task Run()
		{
			Directory.CreateDirectory(STORE_PATH);

			gallery = await exhentai.GetGallery(uri);
			downloaded = force ? new SortedSet<string>() : ScanDownloaded();

			Console.WriteLine("下载图册：" + gallery.Name);
			var start = startPage ?? 1;
			var end = endPage ?? gallery.Length;

			//var tasks = new Task[Concurrent];
			//for (int i = 0; i <= tasks.Length; i++)
			//{
			//	tasks[i] = RunWorker();
			//}

			await DownloadImage(40); // 这页是真的下载慢
			await DownloadImage(41);

			//await Task.WhenAll(tasks);
			Console.WriteLine("下载完毕");
		}

		/// <summary>
		/// 扫描存储目录中已存在的图片文件
		/// </summary>
		/// <returns>文件名集合</returns>
		private ISet<string> ScanDownloaded()
		{
			var exists = new HashSet<string>();
			var dirInfo = new DirectoryInfo(STORE_PATH);

			foreach (var file in dirInfo.EnumerateFiles())
			{
				try
				{
					// 如果一次下载中断，可能出现不完整的文件，故不能只看名字，必须测试读取。
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
			using (var input = await image.GetOriginal())
			using (var output = File.OpenWrite(Path.Combine(STORE_PATH, image.FileName)))
			{
				await input.CopyToAsync(output);
			}
		}
	}
}

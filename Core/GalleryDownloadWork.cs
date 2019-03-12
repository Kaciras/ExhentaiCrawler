using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Core.Infrastructure;

namespace Core
{
	public sealed class GalleryDownloadWork
	{
		public const int DEFAULT_CONCURRENT = 4;

		public int? StartPage { get; set; }
		public int? EndPage { get; set; }

		public bool Force { get; set; }
		public string StorePath { get; set; }
		public bool Flatten { get; set; } // 不自动创建文件夹

		/// <summary>
		/// 给文件名加上序号前缀，例如：XX_原名.png，XX是图片在E绅士网页上的顺序。
		/// 该选项针对文件名顺序与实际顺序不同的情况，如第二张图叫 01.png 而第一张却叫 02.png
		/// </summary>
		public bool IndexPrefix { get; set; }

		public int Concurrent { get; set; } = DEFAULT_CONCURRENT;

		private readonly Exhentai exhentai;
		private readonly string uri;
		private readonly CancellationTokenSource cancellation;

		private Gallery gallery;
		private string store;
		private ISet<string> downloaded;

		private int index;
		private int endIndex;

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

			// 以本子名创建文件夹保存，优先使用日本名
			var saveName = gallery.JapaneseName ?? gallery.Name;
			Console.WriteLine("本子名：" + saveName);

			store = StorePath ?? Environment.CurrentDirectory;
			if(!Flatten)
			{
				store = Path.Combine(store, saveName);
			}
			Directory.CreateDirectory(store);

			downloaded = Force ? new SortedSet<string>() : ScanDownloaded();
			index = StartPage ?? 1;
			endIndex = EndPage ?? gallery.Length;

			// 启动下载线程并等待
			await Task.WhenAll(Enumerable.Range(0, Concurrent).Select(_ => RunWorker()));
			Console.WriteLine("下载任务结束，共下载了" + downloadSize);
		}

		/// <summary>
		/// 扫描存储目录中已存在的图片文件，这些文件将被跳过不再去下载。
		/// </summary>
		/// <returns>文件名集合</returns>
		private ISet<string> ScanDownloaded()
		{
			return new DirectoryInfo(store)
				.EnumerateFiles()
				.Where(CheckImageFile)
				.Select(file => file.Name)
				.ToHashSet();
		}

		/// <summary>
		/// 检查图片文件有没有损坏，无法检测出残缺的图片？
		/// </summary>
		/// <param name="file">文件对象</param>
		/// <returns>如果图片正常返回true</returns>
		private static bool CheckImageFile(FileInfo file)
		{
			try
			{
				Image.FromFile(file.FullName).Dispose();
				return true;
			}
			catch
			{
				// 读取失败抛OOM异常什么鬼啦？？？
				Console.WriteLine($"无法解析已存在的图片文件：{file.Name}");
				return false;
			}
		}

		private async Task RunWorker()
		{
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

		private async Task DownloadImage(int index)
		{
			cancellation.Token.ThrowIfCancellationRequested();

			var image = await gallery.GetImage(index);
			var fileName = image.FileName;

			if (IndexPrefix)
			{
				var nums = (int)Math.Log10(gallery.Length) + 1;
				var prefix = index.ToString().PadLeft(nums, '0');
				fileName = $"{prefix}_{fileName}";
			}

			if (downloaded.Contains(fileName))
			{
				return;
			}

			for (int i = 0; i < 3; i++)
			{
				try
				{
					var storePath = Path.Combine(store, fileName);
					await image.Download(storePath, cancellation.Token);

					var time = DateTime.Now.ToLongTimeString();
					Console.WriteLine($"[{time}] 第{index}张图片{fileName}下载完毕");

					downloadSize += new DataSize(new FileInfo(storePath).Length);
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
				Console.WriteLine($"{fileName}下载失败，正在重试");
			}
		}
		
		public void Cancel()
		{
			cancellation.Cancel();
		}
	}
}

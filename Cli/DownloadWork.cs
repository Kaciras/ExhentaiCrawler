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
		public const int RETRY_TIMES = 3;

		public Range Pages { get; set; }

		public string StorePath { get; set; }

		/// <summary>强制下载全部图片，即使已经存在</summary>
		public bool Force { get; set; }

		public int Concurrent { get; set; } = DEFAULT_CONCURRENT;

		private readonly Gallery gallery;
		private readonly CancellationTokenSource cancellation;

		private string store;
		private ISet<string> downloaded;

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
			var directory = DirectoryNameOf(gallery);
			store = Path.Join(StorePath, directory);
			Console.WriteLine("本子名：" + directory);

			var old = await GetExistsVersion();
			if (old != null)
			{
				UpdateStoreDirectory(old);
			}
			else
			{
				Directory.CreateDirectory(store);
			}


			downloaded = Force ? new SortedSet<string>() : ScanDownloaded();
			(index, endIndex) = Pages.GetOffsetAndLength(gallery.Info.Length);
			endIndex += index;

			// 启动下载线程并等待
			await Task.WhenAll(Enumerable.Range(0, Concurrent).Select(_ => RunWorker()));
			Console.WriteLine("下载任务结束，共下载了" + downloadSize);
		}

		private string DirectoryNameOf(Gallery gallery)
		{
			var info = gallery.Info;
			return $"{gallery.Id} - {info.JapaneseName ?? info.Name}";
		}

		private async Task<Gallery> GetExistsVersion()
		{
			for (var v = gallery; v != null; v = await v.GetParent())
			{
				var name = DirectoryNameOf(gallery);
				if (Directory.Exists(Path.Join(StorePath, name)))
				{
					return gallery;
				}
			}
			return null;
		}

		private void UpdateStoreDirectory(Gallery oldGallery)
		{
			var oldName = DirectoryNameOf(oldGallery);
			var name = DirectoryNameOf(gallery);
			Directory.Move(Path.Join(StorePath, oldName), Path.Join(StorePath, name));

			var oldNumberLength = (int)Math.Log10(oldGallery.Info.Length);
			var numberLength = (int)Math.Log10(gallery.Info.Length);
			if (numberLength != oldNumberLength)
			{
				new DirectoryInfo(store)
					.EnumerateFiles()
					.ForEach((file, i) => file.MoveTo(i.ToString().PadLeft(numberLength, '0')));
			}

			Console.WriteLine($"已将旧版目录合并：{oldName}");
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
		public static bool CheckImageFile(FileInfo file)
		{
			try
			{
				Image.FromFile(file.FullName).Dispose();
				return true;
			}
			catch(OutOfMemoryException)
			{
				// 读取失败抛OOM异常什么鬼啦？？？
				Console.WriteLine($"无法解析的图片文件：{file.Name}");
				return false;
			}
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

			var image = await gallery.GetImage(index);

			// 使用序号并填充对齐作为文件名，保证文件顺序跟本子里的顺序一致。
			// 至于图片的原名就没什么用了。
			var nums = (int)Math.Log10(gallery.Info.Length) + 1;
			var fileName = index.ToString().PadLeft(nums, '0');

			if (downloaded.Contains(fileName))
			{
				return;
			}

			for (int i = 0; i < RETRY_TIMES; i++)
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
				Console.WriteLine($"{fileName}下载失败，重试 - {i}");
			}
		}

		public void Cancel() => cancellation.Cancel();
	}
}

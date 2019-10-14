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

		/// <summary>
		/// 给文件名加上序号前缀，例如：XX_原名.png，XX是图片在E绅士网页上的顺序。
		/// 该选项针对文件名顺序与实际顺序不同的情况，如第二张图叫 01.png 而第一张却叫 02.png
		/// </summary>
		public bool IndexPrefix { get; set; }

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
			store = await GetStoreDirectory();
			Directory.CreateDirectory(store);

			downloaded = Force ? new SortedSet<string>() : ScanDownloaded();

			(index, endIndex) = Pages.GetOffsetAndLength(gallery.Info.Length);
			endIndex += index;

			// 启动下载线程并等待
			await Task.WhenAll(Enumerable.Range(0, Concurrent).Select(_ => RunWorker()));
			Console.WriteLine("下载任务结束，共下载了" + downloadSize);
		}

		private async Task<string> GetStoreDirectory()
		{
			var store = StorePath ?? Environment.CurrentDirectory;

			// 保存到的目录名，以本子名创建文件夹保存，优先使用日本名。
			var name = gallery.Info.JapaneseName ?? gallery.Info.Name;
			Console.WriteLine("本子名：" + name);
			var directory = Path.Join(store, name);

			var versionFile = Path.Join(store, "versions.json");

			var s = new JsonSerializer();
			DownloadRecord record;

			try
			{
				using var reader = new JsonTextReader(new StreamReader(versionFile));
				record = s.Deserialize<DownloadRecord>(reader);
			}
			catch (FileNotFoundException)
			{
				record = new DownloadRecord()
				{
					IdMap = new Dictionary<int, string>(),
					Versions = new Dictionary<int, int>()
				};
			}

			var trace = new List<int>();
			var saveId = -1;

			for (var v = gallery; v != null; v = await v.GetParent())
			{
				if (record.Versions.TryGetValue(v.Id, out saveId))
				{
					break;
				}
				trace.Add(v.Id);
			}

			// 如果已经下载过了，就更新目录名为最新的本子名，没下载过就创建新的记录
			if (saveId >= 0)
			{
				var old = record.IdMap[saveId];
				record.IdMap[saveId] = directory;
				Directory.Move(Path.Join(store, old), directory);
			}
			else
			{
				saveId = record.IdMap.Count;
			}

			// 把新的版本加入到记录并保存
			trace.ForEach(gid => record.Versions[gid] = saveId);
			using var writer = new JsonTextWriter(new StreamWriter(versionFile));
			s.Serialize(writer, record);

			return directory;
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
			var fileName = image.FileName;

			if (IndexPrefix)
			{
				var nums = (int)Math.Log10(gallery.Info.Length) + 1;
				var prefix = index.ToString().PadLeft(nums, '0');
				fileName = $"{prefix}_{fileName}";
			}

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

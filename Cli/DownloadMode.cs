using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommandLine;
using Core;

namespace Cli
{
	[Verb("download", HelpText = "下载本子，格式：download <url>")]
	public sealed class DownloadMode : RunMode
	{
		[Value(0, Required = true, HelpText = "本子的网址")]
		public string Uri { get; set; }

		[Value(1, Default = "-", HelpText = "页码范围，格式：X-Y，表示从X到Y页，XY其中之一可以省略，分别表示第一页和最后一页。也可以是一个整数，表示下载指定页")]
		public string Pages { get; set; }

		[Option('f', "force", HelpText = "强制重新下载，即使在目录中已下载了部分图片")]
		public bool Force { get; set; }

		[Option('c', "concurrent", HelpText = "并发下载数")]
		public int Concurrent { get; set; }

		// ================================= 以上是选项 =================================

		const string STORE_PATH = @"E:/漫画";

		public async Task Start()
		{
			var exhentai = ExhentaiConfig.Load().GetExhentai();
			var gallery = await exhentai.GetGallery(GalleryLink.Parse(Uri));

			var name = gallery.Info.JapaneseName ?? gallery.Info.Name;
			Console.WriteLine("本子名：" + name);

			var store = new LocalGalleryStore(STORE_PATH, gallery);

			// 如果有新版就提示一下，但还是继续下载当前版本
			var newest = await gallery.GetLatestVersion();
			if (newest != gallery)
			{
				Console.WriteLine($"该本子有新版本：{newest.Uri}");
			}

			// 如果下载过旧版，就把旧版的图片都迁移过来，没有旧版就创建目录
			var old = await GetOldVersion(gallery);
			if (old == null)
			{
				store.Create();
			}
			else
			{
				old.MigrateTo(store);
				Console.WriteLine($"已将旧版目录[{old.Name}]合并");
			}

			var work = new DownloadWork(gallery, store)
			{
				Range = ParseRange(Pages),
				Force = Force,
				Concurrent = Concurrent,
			};

			// 使用 Ctrl + C 中断程序时取消work，以保证能做清理。
			Console.CancelKeyPress += (sender, e) =>
			{
				work.Cancel();
				e.Cancel = true;
			};

			Console.WriteLine("下载模式，在下载中途可以按　Ctrl + C 中止");
			await work.StartDownload();
		}

		static async Task<LocalGalleryStore> GetOldVersion(Gallery gallery)
		{
			for (var v = await gallery.GetParent();
				v != null;
				v = await v.GetParent())
			{
				var store = new LocalGalleryStore(STORE_PATH, v);
				if (store.Exists()) return store;
			}
			return null;
		}

		/// <summary>
		/// 解析表示范围的字符串，其格式级意义如下（XY都是非负整数）：
		///		X : 只有第X个
		///		X-Y : 第X到Y个
		///		X- : 第X个到末尾
		///		-Y : 开头到第Y个
		///		- : 全部范围
		/// </summary>
		/// <param name="string">范围字符串</param>
		/// <returns>范围</returns>
		public static Range ParseRange(string @string)
		{
			if (string.IsNullOrEmpty(@string))
			{
				throw new ArgumentException("范围字符串不能为null或空串");
			}

			Index GetIndex(Capture capture, Index @default)
			{
				var value = @string.AsSpan(capture.Index, capture.Length);
				return value.IsEmpty ? @default : int.Parse(value);
			}

			var match = Regex.Match(@string, @"^(\d*)-(\d*)$");
			if (match.Success)
			{
				var start = GetIndex(match.Groups[1], 0);
				var end = GetIndex(match.Groups[2], ^0);
				return start..end;
			}
			else if (int.TryParse(@string, out var index))
			{
				return index..index;
			}
			else
			{
				throw new ArgumentException("页码范围参数错误：" + @string);
			}
		}
	}
}

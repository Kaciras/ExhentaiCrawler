using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
	public sealed class GalleryDownloadWork
	{
		const string STORE_PATH = @"E:\漫画\(成年コミック) [なるさわ景] 亡国魔王の星彦くん [中国翻訳]";

		private readonly Exhentai exhentai;

		private readonly string uri;
		private readonly int? startPage;
		private readonly int? endPage;
		private readonly bool force;

		private Gallery gallery;

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
			gallery = await exhentai.GetGallery(uri);
			var start = startPage ?? 1;
			var end = endPage ?? gallery.Length;

			// 0.2MB消耗一点限额，这么算不准，因为一些小图片不走fullimg.php
			//var cost = gallery.FileSize / 1024 * 5;
			//Console.WriteLine(gallery.Name);
			//Console.WriteLine($"共{gallery.Length}张图片，预计下载将消耗{cost}点限额");

			var downloaded = force ? new SortedSet<string>() : ScanDownloaded();

			for (int i = 1; i <= gallery.Length; i++)
			{
				var image = await gallery.GetImage(i);
				if (!downloaded.Contains(image.FileName))
				{
					await DownloadImage(image);
				}
			}

			await DownloadImage(await gallery.GetImage(40));
			await DownloadImage(await gallery.GetImage(41));

			Console.WriteLine("下载完毕");
		}

		private ISet<string> ScanDownloaded()
		{
			Console.WriteLine("扫描存储目录中已存在的图片文件...");
			var exists = new HashSet<string>();
			var dirInfo = new DirectoryInfo(STORE_PATH);

			foreach (var file in dirInfo.EnumerateFiles())
			{
				try
				{
					// 如果一次下载中断，可能出现不完整的图片文件，故不能只看名字，必须测试读取图片。
					Image.FromFile(file.FullName).Dispose();
					exists.Add(file.Name);
				}
				catch
				{
					// 读取失败抛OOM异常什么鬼啦？？？
					Console.WriteLine($"无法解析图片文件：{file.Name}");
				}
			}
			return exists;
		}

		private async Task DownloadImage(ImageResource image)
		{
			using (var rs = await image.GetOriginal())
			using (var fs = File.OpenWrite(Path.Combine(STORE_PATH, image.FileName)))
			{
				rs.ReadTimeout = 3;
				await rs.CopyToAsync(fs);
			}
		}
	}
}

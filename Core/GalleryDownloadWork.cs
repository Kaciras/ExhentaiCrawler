using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
	public sealed class GalleryDownloadWork
	{
		private readonly Exhentai exhentai;

		private readonly string uri;
		private readonly int? startPage;
		private readonly int? endPage;

		public GalleryDownloadWork(Exhentai exhentai, string uri, int? startPage, int? endPage)
		{
			this.exhentai = exhentai;
			this.uri = uri;
			this.startPage = startPage;
			this.endPage = endPage;
		}

		public async Task Run()
		{
			const string STORE_PATH = @"C:\Users\XuFan\Desktop\";

			var gallery = await exhentai.GetGallery(uri);
			var start = startPage ?? 1;
			var end = endPage ?? gallery.Length;

			// 0.2MB消耗一点限额，这么算不准，因为一些小图片不走fullimg.php
			var cost = gallery.FileSize / 1024 * 5;

			Console.WriteLine(gallery.Name);
			Console.WriteLine($"共{gallery.Length}张图片，预计下载将消耗{cost}点限额");

			async Task Download(int index)
			{
				var image = await gallery.GetImage(index);
				using (var rs = await image.GetOriginal())
				using (var fs = File.OpenWrite(STORE_PATH + image.FileName))
				{
					rs.ReadTimeout = 3;
					rs.CopyTo(fs);
				}
			}

			await Download(40);
			await Download(41);

			Console.WriteLine("下载完毕");
		}
	}
}

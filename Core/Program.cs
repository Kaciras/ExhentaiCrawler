using CommandLine;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("Test")]
namespace Core
{
	[Verb("download", HelpText = "下载图册")]
	sealed class DownloadOptions
	{
		[Value(0, Required =true, HelpText ="图册网址")]
		public string Uri { get; set; }
	}

	[Verb("statistics", HelpText = "启动统计爬虫")]
	sealed class StatisticsOptions
	{

	}

	static class Program
	{
		static void Main(string[] args)
		{
			Parser.Default.ParseArguments<DownloadOptions, StatisticsOptions>(args)
				.WithParsed<DownloadOptions>(DownloadGallery)
				.WithParsed<StatisticsOptions>(RunStatisticsCrawler);
		}

		static void DownloadGallery(DownloadOptions options)
		{
			RunAsyncTask(() => DoDownloadGallery(options)).Wait();
		}

		static async Task RunAsyncTask(Func<Task> asyncAction)
		{
			try
			{
				await asyncAction();
			}
			catch(Exception e)
			{
				Console.WriteLine(e.Message);
				Console.WriteLine(e.StackTrace);
				Debugger.Break();
			}
		}

		static async Task DoDownloadGallery(DownloadOptions options)
		{
			const string STORE_PATH = @"C:\Users\XuFan\Desktop\";
			var client = ExhentaiHttpClient.FromCookie("2723232", "67674c89175c751095d4c840532e6363");
			var exhentai = new Exhentai(client);

			var gallery = await exhentai.GetGallery(options.Uri);

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

		static void RunStatisticsCrawler(StatisticsOptions options)
		{

		}
	}
}

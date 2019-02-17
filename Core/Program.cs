using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CommandLine;

[assembly: InternalsVisibleTo("Test")]
namespace Core
{
	[Verb("download", HelpText = "下载图册")]
	internal sealed class DownloadOptions
	{
		[Value(0, Required =true, HelpText ="图册网址，格式为")]
		public string Uri { get; set; }

		[Value(1, Default = "-", HelpText = "页码范围，格式：X-Y，表示从X到Y页，XY其中之一可以省略，分别表示第一页和最后一页。也可以是一个整数，表示下载指定页")]
		public string Pages { get; set; }

		[Option('f', "force", HelpText = "强制重新下载，即使在目录中以下载了部分图片")]
		public bool Force { get; set; }

		[Option('c', "concurrent", Default = GalleryDownloadWork.DEFAULT_CONCURRENT, HelpText = "并发下载数")]
		public int Concurrent { get; set; }
	}

	[Verb("statistics", HelpText = "启动统计爬虫")]
	internal sealed class StatisticsOptions
	{

	}

	internal static class Program
	{
		private static void Main(string[] args)
		{
			Parser.Default.ParseArguments<DownloadOptions, StatisticsOptions>(args)
				.WithParsed<DownloadOptions>(DownloadGallery)
				.WithParsed<StatisticsOptions>(RunStatisticsCrawler);
		}

		private static void DownloadGallery(DownloadOptions options)
		{
			(var start, var end) = Utils.ParseRange(options.Pages);

			var exhentai = new Exhentai(ExhentaiHttpClient.FromCookie("2723232", "67674c89175c751095d4c840532e6363"));
			var work = new GalleryDownloadWork(exhentai, options.Uri, start, end, options.Force);
			work.Concurrent = options.Concurrent;
			RunAsyncTask(work.Run).Wait();
		}

		private static void RunStatisticsCrawler(StatisticsOptions options)
		{

		}

		private static async Task RunAsyncTask(Func<Task> asyncAction)
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
	}
}

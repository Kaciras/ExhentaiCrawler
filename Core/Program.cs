using CommandLine;
using System;
using System.IO;
using System.Runtime.CompilerServices;
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
			var task = DoDownloadGallery(options);
			task.Wait();

			if(task.IsCompleted)
			{
				Console.WriteLine("下载完毕");
			}
			else
			{
				Console.WriteLine("下载失败:" + task.Exception.Message);
			}
		}

		static async Task DoDownloadGallery(DownloadOptions options)
		{
			var client = new ExhentaiClient("2723232", "67674c89175c751095d4c840532e6363");
			var gallery = await client.GetGallery(options.Uri);

			var i0 = await gallery.GetImage(1);
			var i1 = await i0.GetNext();

			using (var rs = await i1.GetOriginal())
			using (var fs = File.OpenWrite(@"C:\Users\XuFan\Desktop\" + i1.FileName))
			{
				rs.CopyTo(fs);
			}
		}

		static void RunStatisticsCrawler(StatisticsOptions options)
		{

		}
	}
}

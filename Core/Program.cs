using CommandLine;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

[assembly: InternalsVisibleTo("Test")]
namespace Core
{
	[Verb("download", HelpText = "下载图册")]
	internal sealed class DownloadOptions
	{
		[Value(0, Required =true, HelpText ="图册网址，格式为")]
		public string Uri { get; set; }

		[Value(1, HelpText = "页码范围，格式：X-Y，表示从X到Y页，XY其中之一可以省略，分别表示第一页和最后一页。也可以是一个整数，表示下载指定页")]
		public string Pages { get; set; }
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
			int? start, end;

			// 先来解析 options.Pages 参数
			if (options.Pages == null)
			{
				start = end = null;
			}
			else if (int.TryParse(options.Pages, out var index))
			{
				start = end = index;
			}
			else
			{
				var match = Regex.Match(options.Pages, @"^(\d*)-(\d*)$");
				if (match.Success)
				{
					start = StringToNullableInt(match.Groups[1].Value);
					end = StringToNullableInt(match.Groups[2].Value);
				}
				else
				{
					throw new ArgumentException("页码范围参数错误：" + options.Pages);
				}
			}

			var exhentai = new Exhentai(ExhentaiHttpClient.FromCookie("2723232", "67674c89175c751095d4c840532e6363"));
			var work = new GalleryDownloadWork(exhentai, options.Uri, start, end);
			RunAsyncTask(work.Run).Wait();
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

		private static void RunStatisticsCrawler(StatisticsOptions options)
		{

		}

		// 不能用三元运算，因为null和int不兼容，虽然返回值两个都兼容
		private static int? StringToNullableInt(string @string)
		{
			if (string.IsNullOrEmpty(@string))
			{
				return null;
			}
			else
			{
				return int.Parse(@string);
			}
		}
	}
}

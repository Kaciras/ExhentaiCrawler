using CommandLine;
using System;
using System.Runtime.CompilerServices;

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
			Console.WriteLine(options.Uri);
		}

		static void RunStatisticsCrawler(StatisticsOptions options)
		{

		}
	}
}

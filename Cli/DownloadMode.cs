using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommandLine;
using Core;
using Core.Request;

namespace Cli
{
	[Verb("download", HelpText = "下载本子，格式：download <url>")]
	public sealed class DownloadMode : RunMode
	{
		[Value(0, Required = true, HelpText = "本子的网址或图片网址")]
		public string Uri { get; set; }

		[Value(1, Default = "-", HelpText = "页码范围，格式：X-Y，表示从X到Y页，XY其中之一可以省略，分别表示第一页和最后一页。也可以是一个整数，表示下载指定页")]
		public string Pages { get; set; }

		[Option('f', "force", HelpText = "强制重新下载，即使在目录中已下载了部分图片")]
		public bool Force { get; set; }

		[Option('c', "concurrent", Default = DownloadWork.DEFAULT_CONCURRENT, HelpText = "并发下载数")]
		public int Concurrent { get; set; }

		// ================================= 以上是选项 =================================

		public async Task Start()
		{
			var client = new PooledExhentaiClient();
			client.AddLocalIP();
			client.AddProxy(new WebProxy("localhost", 2081));

			var exhentai = new Exhentai(client);
			exhentai.SetUser("2723232", "67674c89175c751095d4c840532e6363");

			Gallery gallery;

			if (ImageLink.TryParse(new Uri(Uri), out var link))
			{
				gallery = await exhentai.GetImage(link).GetGallery();
			}
			else
			{
				gallery = await exhentai.GetGallery(Uri);
			}

			var work = new DownloadWork(gallery)
			{
				Pages = ParseRange(Pages),
				Force = Force,
				StorePath = @"E:\漫画",
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

			var match = Regex.Match(@string, @"^(\d*)-(\d*)$");
			if (match.Success)
			{
				var start = ParseNullableInt(match.Groups[1].Value);
				var end = ParseNullableInt(match.Groups[2].Value);
				return (start ?? 0)..(end ?? ^0);
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

		private static int? ParseNullableInt(string @string)
		{
			return string.IsNullOrEmpty(@string) ? null : (int?)int.Parse(@string);
		}
	}
}

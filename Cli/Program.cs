using System;
using System.Diagnostics;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommandLine;
using Core;
using Core.Request;

[assembly: InternalsVisibleTo("Test")]
namespace Cli
{
	[Verb("login", HelpText = "设置并保存登录信息，以便以后使用")]
	internal sealed class LoginOptions
	{
		[Value(0, Required = true, HelpText = "用户名")]
		public string UserName { get; set; }

		[Value(1, Required = true, HelpText = "密码")]
		public string Password { get; set; }

		[Option('c', "cookie", HelpText = "Cookie模式，分别设置UserName和Password为 ipb_member_id 和 ipb_pass_hash")]
		public bool CookieMode { get; set; }
	}

	[Verb("download", HelpText = "下载图册")]
	internal sealed class DownloadOptions
	{
		[Value(0, Required =true, HelpText ="本子的网址或图片网址")]
		public string Uri { get; set; }

		[Value(1, Default = "-", HelpText = "页码范围，格式：X-Y，表示从X到Y页，XY其中之一可以省略，分别表示第一页和最后一页。也可以是一个整数，表示下载指定页")]
		public string Pages { get; set; }

		[Option('f', "force", HelpText = "强制重新下载，即使在目录中以下载了部分图片")]
		public bool Force { get; set; }

		[Option('c', "concurrent", Default = GalleryDownloadWork.DEFAULT_CONCURRENT, HelpText = "并发下载数")]
		public int Concurrent { get; set; }
	}

	internal static class Program
	{
		private static void Main(string[] args)
		{
			Parser.Default.ParseArguments<DownloadOptions, LoginOptions>(args)
				.WithParsed<LoginOptions>(Login)
				.WithParsed<DownloadOptions>(DownloadGallery);
		}

		private static void Login(LoginOptions options)
		{

		}

		private static void DownloadGallery(DownloadOptions options)
		{
			var client = new PooledExhentaiClient();
			client.AddLocalIP();
			client.AddProxy(new WebProxy("localhost", 2080));

			var exhentai = new Exhentai(client);
			exhentai.SetUser("2723232", "67674c89175c751095d4c840532e6363");

			var work = new GalleryDownloadWork(exhentai, options.Uri)
			{
				Pages = ParseRange(options.Pages),
				Force = options.Force,
				StorePath = @"C:\Users\XuFan\Desktop",
				Concurrent = options.Concurrent,
			};

			// 使用 Ctrl + C 中断程序时取消work，以保证能做清理。
			Console.CancelKeyPress += (sender, e) =>
			{
				work.Cancel();
				e.Cancel = true;
			};

			Console.WriteLine("下载模式，在下载中途可以按　Ctrl + C 中止");
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

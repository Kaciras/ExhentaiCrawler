using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CommandLine;

[assembly: InternalsVisibleTo("Test")]
[assembly: InternalsVisibleTo("Benchmark")]
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

	internal static class Program
	{
		private static void Main(string[] args)
		{
			Parser.Default.ParseArguments<DownloadMode, InspectCookieMode>(args)
				.WithParsed<RunMode>(mode => RunAsyncTask(mode.Start).Wait());
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

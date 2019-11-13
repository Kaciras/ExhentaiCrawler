using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CommandLine;

[assembly: InternalsVisibleTo("Test")]
[assembly: InternalsVisibleTo("Benchmark")]
namespace Cli
{
	internal static class Program
	{
		private static void Main(string[] args)
		{
			Parser.Default.ParseArguments<DownloadMode, ConfigMode>(args)
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

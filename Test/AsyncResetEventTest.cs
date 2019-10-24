using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Infrastructure;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test
{
	[TestClass]
	public class AsyncResetEventTest
	{
		[TestMethod]
		public void InitUnset()
		{
			var resetEvent = new AsyncResetEvent();
			var waitTask = resetEvent.Wait();

			waitTask.IsCompleted.Should().BeFalse();
		}

		[TestMethod]
		public void InitSet()
		{
			var resetEvent = new AsyncResetEvent(true);
			var waitTask = resetEvent.Wait();

			waitTask.IsCompleted.Should().BeTrue();
		}

		[Timeout(500)]
		[TestMethod]
		public async Task WaitWithSet()
		{
			var resetEvent = new AsyncResetEvent();

			var setter = Task.Delay(100).ContinueWith(_ => resetEvent.Set());
			await resetEvent.Wait();
		}

		[TestMethod]
		public void Reset()
		{
			var resetEvent = new AsyncResetEvent(true);
			resetEvent.Reset();

			resetEvent.Wait().IsCompleted.Should().BeFalse();
		}

		[Timeout(500)]
		[TestMethod]
		public void MutipleWait()
		{
			var resetEvent = new AsyncResetEvent();

			var waitTasks = Enumerable.Range(0, 3).Select(_ => resetEvent.Wait()).ToArray();
			waitTasks.Select(t => t.IsCompleted).Should().AllBeEquivalentTo(false);

			resetEvent.Set();
			Task.WaitAll(waitTasks);
		}

		[Timeout(200)]
		[TestMethod]
		public async Task Cancellation()
		{
			var resetEvent = new AsyncResetEvent();
			var time = DateTime.Now;
			var cancellation = new CancellationTokenSource(30);

			try
			{
				await resetEvent.Wait(cancellation.Token);
				Assert.Fail();
			}
			catch(TaskCanceledException)
			{
				(DateTime.Now - time).Milliseconds.Should().BeGreaterOrEqualTo(30);
			}
		}
	}
}

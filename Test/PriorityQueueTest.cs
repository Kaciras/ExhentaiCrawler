using System;
using Core.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test
{
	[TestClass]
	public sealed class PriorityQueueTest
	{
		private static readonly Comparison<int> comparison = (a, b) => a.CompareTo(b);

		[TestMethod]
		public void Enqueue_dequeue()
		{
			var q = new PriorityQueue<int>(comparison);

			for (var i = 0; i < 16; i++)
			{
				Assert.AreEqual(0, q.Count);

				q.Enqueue(i);

				Assert.AreEqual(1, q.Count);
				Assert.AreEqual(i, q.Peek());
				Assert.AreEqual(1, q.Count);
				Assert.AreEqual(i, q.Dequeue());
				Assert.AreEqual(0, q.Count);
			}
		}

		[TestMethod]
		public void Enqueue_all_dequeue_all()
		{
			var q = new PriorityQueue<int>(comparison);

			for (var i = 0; i < 33; i++)
			{
				q.Enqueue(i);
				Assert.AreEqual(i + 1, q.Count);
			}

			Assert.AreEqual(33, q.Count);

			for (var i = 0; i < 33; i++)
			{
				Assert.AreEqual(33 - i, q.Count);
				Assert.AreEqual(i, q.Peek());
				Assert.AreEqual(i, q.Dequeue());
			}

			Assert.AreEqual(0, q.Count);
		}

		[TestMethod]
		public void Reverse_Enqueue_all_dequeue_all()
		{
			var q = new PriorityQueue<int>(comparison);

			for (var i = 32; i >= 0; i--)
			{
				q.Enqueue(i);
				Assert.AreEqual(33 - i, q.Count);
			}

			Assert.AreEqual(33, q.Count);

			for (var i = 0; i < 33; i++)
			{
				Assert.AreEqual(33 - i, q.Count);
				Assert.AreEqual(i, q.Peek());
				Assert.AreEqual(i, q.Dequeue());
			}

			Assert.AreEqual(0, q.Count);
		}

		[TestMethod]
		public void Remove_from_middle()
		{
			var q = new PriorityQueue<int>(comparison);

			for (var i = 0; i < 33; i++)
			{
				q.Enqueue(i);
			}

			q.Remove(16);

			for (var i = 0; i < 16; i++)
			{
				Assert.AreEqual(i, q.Dequeue());
			}

			for (var i = 16; i < 32; i++)
			{
				Assert.AreEqual(i + 1, q.Dequeue());
			}
		}

		[TestMethod]
		public void Repro_329()
		{
			var queue = new PriorityQueue<int>(comparison);

			queue.Enqueue(2);
			queue.Enqueue(1);
			queue.Enqueue(5);
			queue.Enqueue(2);

			Assert.AreEqual(1, queue.Dequeue());
			Assert.AreEqual(2, queue.Dequeue());
			Assert.AreEqual(2, queue.Dequeue());
			Assert.AreEqual(5, queue.Dequeue());
		}
	}
}
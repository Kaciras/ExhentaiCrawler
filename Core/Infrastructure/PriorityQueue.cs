using System;
using System.Collections.Generic;

namespace Core.Infrastructure
{
	public sealed class PriorityQueue<T>
	{
		private readonly Comparison<T> compare;

		private T[] items;

		public PriorityQueue(Comparison<T> compare) : this(16, compare) {}

		public PriorityQueue(int capacity, Comparison<T> compare)
		{
			this.compare = compare;
			Count = 0;
			items = new T[capacity];
		}

		private bool IsHigherPriority(int left, int right)
		{
			return compare(items[left], items[right]) < 0;
		}

		private int Percolate(int index)
		{
			if (index >= Count || index < 0)
			{
				return index;
			}

			var parent = (index - 1) / 2;
			while (parent >= 0 && parent != index && IsHigherPriority(index, parent))
			{
				// swap index and parent
				var temp = items[index];
				items[index] = items[parent];
				items[parent] = temp;

				index = parent;
				parent = (index - 1) / 2;
			}

			return index;
		}

		private void Heapify(int index)
		{
			if (index >= Count || index < 0)
			{
				return;
			}

			while (true)
			{
				var left = 2 * index + 1;
				var right = 2 * index + 2;
				var first = index;

				if (left < Count && IsHigherPriority(left, first))
				{
					first = left;
				}

				if (right < Count && IsHigherPriority(right, first))
				{
					first = right;
				}

				if (first == index)
				{
					break;
				}

				// swap index and first
				var temp = items[index];
				items[index] = items[first];
				items[first] = temp;

				index = first;
			}
		}

		public int Count { get; private set; }

		public T Peek()
		{
			if (Count == 0)
			{
				throw new InvalidOperationException("Priority queue is empty");
			}
			return items[0];
		}

		private void RemoveAt(int index)
		{
			items[index] = items[--Count];
			items[Count] = default;

			if (Percolate(index) == index)
			{
				Heapify(index);
			}

			if (Count < items.Length / 4)
			{
				var temp = items;
				items = new T[items.Length / 2];
				Array.Copy(temp, 0, items, 0, Count);
			}
		}

		public T Dequeue()
		{
			var result = Peek();
			RemoveAt(0);
			return result;
		}

		public void Enqueue(T item)
		{
			if (Count >= items.Length)
			{
				var temp = items;
				items = new T[items.Length * 2];
				Array.Copy(temp, items, temp.Length);
			}

			var index = Count++;
			items[index] = item;
			Percolate(index);
		}

		public bool Remove(T item)
		{
			for (var i = 0; i < Count; ++i)
			{
				if (EqualityComparer<T>.Default.Equals(items[i], item))
				{
					RemoveAt(i);
					return true;
				}
			}
			return false;
		}
	}
}

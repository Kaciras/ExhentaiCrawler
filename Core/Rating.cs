namespace Core
{
	public readonly struct Rating
	{
		public int Count { get; }
		public double Average { get; }

		internal Rating(int count, double average)
		{
			Count = count;
			Average = average;
		}
	}
}

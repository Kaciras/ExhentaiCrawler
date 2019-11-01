namespace Core.Request
{
	public sealed class LimitReachedException : ExhentaiException
	{
		public LimitReachedException() : base("该IP或用户已经达到限额") {}
	}
}

namespace Core
{
	/// <summary>
	/// 从画册页面的图片预览列表上能得到的信息。
	/// </summary>
	internal readonly struct ImageLink
	{
		public readonly string Key;
		public readonly string FileName;

		public ImageLink(string key, string fileName)
		{
			Key = key;
			FileName = fileName;
		}
	}
}

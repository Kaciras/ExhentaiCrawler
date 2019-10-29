namespace Core
{
	/// <summary>
	/// 从画册页面的图片预览列表上能得到的信息。
	/// </summary>
	public readonly struct ImageListItem
	{
		public readonly ImageLink Link;
		public readonly string FileName;

		public ImageListItem(ImageLink link, string fileName)
		{
			Link = link;
			FileName = fileName;
		}
	}
}

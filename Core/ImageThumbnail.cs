namespace Core
{
	/// <summary>
	/// 从画册页面的图片预览列表上能得到的信息。
	/// </summary>
	public readonly struct ImageThumbnail
	{
		public readonly ImageLink Link;
		public readonly string FileName;

		public ImageThumbnail(ImageLink link, string fileName)
		{
			Link = link;
			FileName = fileName;
		}
	}
}

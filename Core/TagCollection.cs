using System;
using System.Collections.Generic;
using System.Text;

namespace Core
{
	/// <summary>
	/// 画册的标签集合，详情见：
	///	https://ehwiki.org/wiki/Gallery_Tagging
	///	https://ehwiki.org/wiki/Namespace
	/// </summary>
	public sealed class TagCollection
	{
		public ICollection<GalleryTag> Artist { get; set; }
		public ICollection<GalleryTag> Claracter { get; set; }
		public ICollection<GalleryTag> Female { get; set; }
		public ICollection<GalleryTag> Group { get; set; }
		public ICollection<GalleryTag> Language { get; set; }
		public ICollection<GalleryTag> Male { get; set; }
		public ICollection<GalleryTag> Parody { get; set; }
		public ICollection<GalleryTag> Reclass { get; set; }

		public ICollection<GalleryTag> Misc { get; set; }
	}

	public struct GalleryTag
	{
		public bool IsLowPrower;
		public string Value;

		public GalleryTag(bool lowPrower, string value)
		{
			IsLowPrower = lowPrower;
			Value = value;
		}
	}
}

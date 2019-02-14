using System;
using System.Collections.Generic;
using System.Linq;

namespace Core
{
	/// <summary>
	/// 网站最上面的搜索框，以及高级搜索选项。不包括文件搜索。
	/// </summary>
	public sealed class FilterOptions
	{
		ISet<Category> categories = new HashSet<Category>((Category[])Enum.GetValues(typeof(Category)));

		int? rating;

		public string SreachText { get; set; }

		public bool SearchGalleryNamae { get; set; } = true;
		public bool SearchGalleryTags { get; set; } = true;

		public bool SearchTorrentFilenames { get; set; }
		public bool SearchLowPowerTags { get; set; }
		public bool ShowExpungedGalleries { get; set; }

		public int? MinimumRating
		{
			get => rating;
			set
			{
				if (value != null && (value > 5 || value < 2))
				{
					throw new ArgumentException("MinimumRating must be null or between 2 and 5");
				}
				rating = value;
			}
		}

		public override string ToString()
		{
			var x = categories.Select((c) => $"f_{c.GetString()}=1");
			x = x.Concat(Utils.SingleEnumerable($"f_search={SreachText ?? ""}"));
			x = x.Concat(Utils.SingleEnumerable("f_apply=Apply+Filter"));
			return string.Join('&', x);
		}
	}
}

using System;
using System.Text.RegularExpressions;

namespace Core
{
	public sealed class ImageLink
	{
		public static readonly Regex PATH_RE = new Regex(@"/s/(?<KEY>\w+)/(?<GID>\d+)-(?<PAGE>\d+)/?");

		public string Key { get; }
		public int GalleryId { get; }
		public int Page { get; }

		public ImageLink(string key, int galleryId, int page)
		{
			Key = key;
			GalleryId = galleryId;
			Page = page;
		}

		public override string ToString()
		{
			return $"https://exhentai.org/s/{Key}/{GalleryId}-{Page}";
		}

		public static bool TryParse(Uri uri, out ImageLink result)
		{
			var match = PATH_RE.Match(uri.AbsolutePath);
			if (match.Success)
			{
				var page = int.Parse(match.Groups["PAGE"].Value);
				var gid = int.Parse(match.Groups["GID"].Value);
				result = new ImageLink(match.Groups["KEY"].Value, gid, page);
			}
			else
			{
				result = null;
			}
			return result != null;
		}

		public static ImageLink Parse(Uri uri)
		{
			if (TryParse(uri, out var result))
			{
				return result;
			}
			throw new UriFormatException();
		}
	}
}

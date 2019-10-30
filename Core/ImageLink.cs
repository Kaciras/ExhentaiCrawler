using System;
using System.Text.RegularExpressions;

namespace Core
{
	public sealed class ImageLink
	{
		private static readonly Regex REGEX = new Regex(
			@"/s/(?<KEY>\w+)/(?<GID>\d+)-(?<PAGE>\d+)/?", RegexOptions.Compiled);

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

		public static ImageLink Parse(string @string)
		{
			var path = Utils.ExtactEhentaiUriPath(@string);

			var match = REGEX.Match(path);
			if (!match.Success)
			{
				throw new UriFormatException($"图片的URL格式不对，{@string}");
			}
			var page = int.Parse(match.Groups["PAGE"].Value);
			var gid = int.Parse(match.Groups["GID"].Value);
			return new ImageLink(match.Groups["KEY"].Value, gid, page);
		}
	}
}

using System;
using System.Text.RegularExpressions;

namespace Core
{
	/// <summary>
	/// 本子的链接，格式为 https://e[-x]hentai.org/g/{ID}/{Token}。
	/// 因为链接的构造方式较多，所以统一用个对象表示。
	/// </summary>
	public sealed class GalleryLink
	{
		private static readonly Regex REGEX = new Regex(@"/g/(?<GID>\d+)/(?<TOKEN>\w+)/", RegexOptions.Compiled);

		public int Id { get; }
		public string Token { get; }

		public GalleryLink(int id, string token)
		{
			Id = id;
			Token = token;
		}

		public override string ToString()
		{
			return $"https://exhentai.org/g/{Id}/{Token}";
		}

		public static bool TryParse(string uri, out GalleryLink link)
		{
			var match = REGEX.Match(uri);
			if (match.Success)
			{
				var id = int.Parse(match.Groups["GID"].Value);
				link = new GalleryLink(id, match.Groups["TOKEN"].Value);
			}
			else
			{
				link = null;
			}
			return link != null;
		}

		public static GalleryLink Parse(string uri)
		{
			if (TryParse(uri, out var result))
			{
				return result;
			}
			throw new UriFormatException($"本子的URL格式不对，{uri}");
		}
	}
}

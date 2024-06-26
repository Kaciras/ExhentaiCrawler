﻿using System;
using System.Text.RegularExpressions;

namespace Core
{
	/// <summary>
	/// 本子的链接，格式为 https://e[-x]hentai.org/g/{ID}/{Token}。
	/// 因为链接的构造方式较多，所以统一用个对象表示。
	/// </summary>
	public sealed class GalleryLink
	{
		private static readonly Regex REGEX = new Regex(@"^/g/(?<GID>\d+)/(?<TOKEN>\w+)", RegexOptions.Compiled);

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

		public static GalleryLink Parse(string @string)
		{
			var path = Utils.ExtactEhentaiUriPath(@string);

			var match = REGEX.Match(path);
			if (!match.Success)
			{
				throw new UriFormatException($"本子的URL格式不对，{@string}");
			}
			var id = int.Parse(match.Groups["GID"].Value);
			return new GalleryLink(id, match.Groups["TOKEN"].Value);
		}
	}
}

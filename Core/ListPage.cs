using System;
using System.Collections.Generic;
using System.Text;
using HtmlAgilityPack;

namespace Core
{
	public sealed class ListPage
	{
		public List<GalleryLink> Galleries { get; set; }

		public int TotalCount { get; set; }

		// 可以直接计算
		public int TotalPage => 1 + (TotalCount - 1) / Galleries.Count;
		
		public static ListPage ParseHtml(string html)
		{
			// TODO
			throw new NotImplementedException();
		}
	}
}

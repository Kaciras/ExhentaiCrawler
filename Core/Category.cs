using System;
using System.Collections.Generic;
using System.Text;

namespace Core
{
	public enum Category
	{
		Doujinshi, Manga, Artistcg, Gamecg, Western,
		NonH, Imageset, Cosplay, Asianporn, Misc
	}

	public static class CategoryExtention
	{
		public static string GetString(this Category category)
		{
			if (category == Category.NonH)
			{
				return "non-h";
			}
			return Enum.GetName(typeof(Category), category).ToLower();
		}
	}
}

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

	// non-h 得特殊处理下
	public static class CategoryHelper
	{
		public static string GetString(this Category category)
		{
			if (category == Category.NonH)
			{
				return "non-h";
			}
			return Enum.GetName(typeof(Category), category).ToLower();
		}

		public static Category Parse(string text)
		{
			if(text == "non-h")
			{
				return Category.NonH;
			}
			return Enum.Parse<Category>(char.ToUpper(text[0]) + text.Substring(1));
		}
	}
}

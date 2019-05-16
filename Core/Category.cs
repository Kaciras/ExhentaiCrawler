using System;
using System.Collections.Generic;
using System.Text;

namespace Core
{
	[Flags]
	public enum Category
	{
		All = 0, 

		Doujinshi = 2, Manga = 4, Artistcg = 8, Gamecg = 16, Western = 512,
		NonH = 256, Imageset = 32, Cosplay = 64, Asianporn = 128, Misc = 1,
	}

	// non-h 得特殊处理下
	public static class CategoryHelper
	{
		public static string GetString(this Category category)
		{
			if (category == Category.NonH)
			{
				return "Non-H";
			}
			return Enum.GetName(typeof(Category), category).ToLower();
		}

		public static Category Parse(string text)
		{
			if(text == "Non-H")
			{
				return Category.NonH;
			}
			return Enum.Parse<Category>(char.ToUpper(text[0]) + text.Substring(1));
		}
	}
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Core
{
	[Flags]
	public enum Category
	{
		None, // 老实说我更喜欢JAVA的EnumSet

		Doujinshi = 1, Manga = 2, Artistcg = 4, Gamecg = 8, Western = 16,
		NonH = 32, Imageset = 64, Cosplay = 128, Asianporn = 256, Misc = 512,
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

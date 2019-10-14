using System;

namespace Core
{
	/// <summary>
	/// 在19年3月更新之后，分类参数采了用位开关形式。
	/// 参数里 f_cats=0 和 f_cats=1023 是一样的，全关等于全开。
	/// </summary>
	[Flags]
	public enum Category
	{
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
			if (text == "Non-H")
			{
				return Category.NonH;
			}
			return Enum.Parse<Category>(char.ToUpper(text[0]) + text.Substring(1));
		}
	}
}

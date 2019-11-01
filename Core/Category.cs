using System;
using EnumsNET;

namespace Core
{
	/// <summary>
	/// 在19年3月更新之后，分类参数采了用位开关形式。
	/// 参数里 f_cats=0 和 f_cats=1023 是一样的，全关等于全开。
	/// </summary>
	[Flags]
	public enum Category
	{
		Doujinshi = 2, Manga = 4, ArtistCG = 8, GameCG = 16, Western = 512,
		NonH = 256, ImageSet = 32, Cosplay = 64, AsianPorn = 128, Misc = 1,
	}

	// 跟分类按钮显示的文本一致
	public static class CategoryHelper
	{
		public static string GetString(this Category category) => category switch
		{
			Category.ArtistCG => "Artist CG",
			Category.GameCG => "Game CG",
			Category.NonH => "Non-H",
			Category.ImageSet => "Image Set",
			Category.AsianPorn => "Asian Porn",
			_ => category.GetName(),
		};

		public static Category Parse(string text) => text switch
		{
			"Artist CG" => Category.ArtistCG,
			"Game CG" => Category.GameCG,
			"Non-H" => Category.NonH,
			"Image Set" => Category.ImageSet,
			"Asian Porn" => Category.AsianPorn,
			_ => Enum.Parse<Category>(char.ToUpper(text[0]) + text.Substring(1)),
		};
	}
}

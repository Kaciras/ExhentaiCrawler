using System;
using System.Collections.Generic;
using System.Text;
using Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test
{
	[TestClass]
	public class CategoryTest
	{
		// GetString 和 Parse 正好是互逆的操作，这里一起测了
		[DataTestMethod]
		[DataRow("Doujinshi", Category.Doujinshi)]
		[DataRow("Manga", Category.Manga)]
		[DataRow("Artist CG", Category.ArtistCG)]
		[DataRow("Game CG", Category.GameCG)]
		[DataRow("Western", Category.Western)]
		[DataRow("Non-H", Category.NonH)]
		[DataRow("Image Set", Category.ImageSet)]
		[DataRow("Cosplay", Category.Cosplay)]
		[DataRow("Asian Porn", Category.AsianPorn)]
		[DataRow("Misc", Category.Misc)]
		public void Convert(string @string, Category category)
		{
			Assert.AreEqual(@string, category.GetString());
			Assert.AreEqual(category, CategoryHelper.Parse(@string));
		}

		[TestMethod]
		public void ParseFail()
		{
			Assert.ThrowsException<ArgumentException>(() => CategoryHelper.Parse("invalid"));
		}

		// 不存在的枚举 Enum.GetName 返回 null 而不是抛异常
		[TestMethod]
		public void GetStringFromInvalidValue()
		{
			Assert.IsNull(((Category)178).GetString());
		}
	}
}

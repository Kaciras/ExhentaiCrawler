using Core;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test
{
	[TestClass]
	public class FilterOptionsTest
	{
		[TestMethod]
		public void Default()
		{
			var except = "f_doujinshi=0&f_manga=0&f_artistcg=0&f_gamecg=0&f_western=0&f_non-h=0&f_imageset=0&f_cosplay=0&f_asianporn=0&f_misc=0&f_search=&f_apply=Apply+Filter";

			var options = new FilterOptions();

			options.ToString().Split("&").Should().BeEquivalentTo(except.Split("&"));
		}

		[TestMethod]
		public void Advanced()
		{
			var except = "f_doujinshi=1&f_manga=1&f_artistcg=0&f_gamecg=0&f_western=0&f_non-h=0&f_imageset=0&f_cosplay=0&f_asianporn=0&f_misc=0&f_search=&f_apply=Apply+Filter&advsearch=1&f_sname=on&f_stags=on&f_sh=on&f_srdd=2";
			var options = new FilterOptions
			{
				Categories = Category.Doujinshi | Category.Manga,
				AdvancedOptions = new FilterOptions.Advance()
			};
			options.AdvancedOptions.ShowExpungedGalleries = true;

			options.ToString().Split("&").Should().BeEquivalentTo(except.Split("&"));
		}
	}
}

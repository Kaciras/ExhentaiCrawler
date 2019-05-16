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
			new FilterOptions().ToString().Should().BeEmpty();
		}

		[TestMethod]
		public void Advanced()
		{
			var except = "f_cats=1017&advsearch=1&f_sname=on&f_stags=on&f_sh=on&f_srdd=2";
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

using Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Test
{
	[TestClass]
	public class FilterOptionsTest
	{
		[TestMethod]
		public void FilterOptionsDefault()
		{
			var options = new FilterOptions();
			var defaultFilterText = "f_doujinshi=1&f_manga=1&f_artistcg=1&f_gamecg=1&f_western=1" +
				"&f_non-h=1&f_imageset=1&f_cosplay=1&f_asianporn=1&f_misc=1&f_search=&f_apply=Apply+Filter";

			// Uri params is order insensitive
			var expect = new HashSet<string>(defaultFilterText.Split("&"));
			var artucal = new HashSet<string>(options.ToString().Split("&"));
			Assert.IsTrue(expect.SetEquals(artucal));
		}

		[TestMethod]
		public void FilterOptions()
		{

		}	
	}
}

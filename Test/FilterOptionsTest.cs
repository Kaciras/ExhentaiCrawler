using Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;

namespace Test
{
	[TestClass]
	public class FilterOptionsTest
	{
		[TestMethod]
		public void Default()
		{
			var defaultFilterText = "f_doujinshi=1&f_manga=1&f_artistcg=1&f_gamecg=1&f_western=1" +
				"&f_non-h=1&f_imageset=1&f_cosplay=1&f_asianporn=1&f_misc=1&f_search=&f_apply=Apply+Filter";

			var options = new FilterOptions();

			options.ToString().Split("&").Should().BeEquivalentTo(defaultFilterText.Split("&"));
		}

		[TestMethod]
		public void FilterOptions()
		{

		}	
	}
}

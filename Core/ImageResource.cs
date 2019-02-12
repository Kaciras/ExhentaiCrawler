using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Http;

namespace Core
{
	public class ImageResource
	{
		public Gallery Gallery { get; }
		public string ImageKey { get; }
		public string ShowKey { get; set; }

		ImageResource(Gallery gallery, string imageKey)
		{
			Gallery = gallery;
			ImageKey = imageKey;
		}

		//public ImageResource GetPrevious()
		//{
			
		//}

		//public ImageResource GetNext()
		//{

		//}

		public static void ParsePage(string html)
		{

		}

		public static void ParseApi(string content)
		{
			
		}
	}
}

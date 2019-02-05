using System;
using System.Collections.Generic;
using System.Text;

namespace Core
{
	public class ImageResource
	{
		public Gallery Gallery { get; }
		public string ImageKey { get; }

		ImageResource(Gallery gallery, string imageKey)
		{
			Gallery = gallery;
			ImageKey = imageKey;
		}


	}
}

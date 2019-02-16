using System;
using System.Collections.Generic;
using System.Text;

namespace Core
{
	public enum TagCredibility
	{
		Confidence, // 实线边框
		Unconfidence, // 虚线边框
		Incorrect, // 点线边框
	}

	public struct GalleryTag
	{
		public string Value;
		public TagCredibility Credibility;

		public GalleryTag(string value, TagCredibility cred)
		{
			Value = value;
			Credibility = cred;
		}
	}
}

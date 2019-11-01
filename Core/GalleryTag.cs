namespace Core
{
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

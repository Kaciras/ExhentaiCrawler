using System;
using System.Collections.Generic;
using System.Linq;

namespace Core
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	internal sealed class UriParamaterAttribute : Attribute
	{
		public string Name { get; }

		public UriParamaterAttribute(string name) => Name = name;
	}

	/// <summary>
	/// 网站最上面的搜索框，以及高级搜索选项。不包括文件搜索。
	/// </summary>
	public sealed class FilterOptions
	{
		/// <summary>
		/// 需要包含在结果中的分类，默认是全部
		/// </summary>
		public Category Categories { get; set; }

		public string SreachText { get; set; }
		public Advance AdvancedOptions { get; set; }
		
		public override string ToString()
		{
			return string.Join('&', AsParameters());
		}

		public IEnumerable<string> AsParameters()
		{
			var @params = new List<string>(13);

			if (Categories != Category.All)
			{
				@params.Add($"f_cats={(int)Categories ^ 1023}");
			}

			if (SreachText != null)
			{
				@params.Add("f_search=" + SreachText);
			}

			if (AdvancedOptions != null)
			{
				@params.Add("advsearch=1");
				SerlalizeParams(AdvancedOptions, @params);
			}

			return @params;
		}

		/// <summary>
		/// 一个个If判断太麻烦，搞个自动转换成Uri参数的方法
		/// </summary>
		private void SerlalizeParams(object @object, ICollection<string> collection)
		{
			foreach (var prop in @object.GetType().GetProperties())
			{
				var attrs = prop.GetCustomAttributes(typeof(UriParamaterAttribute), false);
				if (attrs.Length > 0)
				{
					var name = ((UriParamaterAttribute)attrs[0]).Name;
					var value = prop.GetValue(@object);

					switch (value)
					{
						case true:
							collection.Add(name + "=on");
							break;
						case false:
							break;
						default:
							collection.Add($"{name}={value}");
							break;
					}
				}
			}
		}

		/// <summary>
		/// 高级搜索选项，就是搜索框下面的 Show Advanced Options
		/// </summary>
		public sealed class Advance
		{
			[UriParamater("f_sname")]
			public bool SearchGalleryNamae { get; set; } = true;

			[UriParamater("f_stags")]
			public bool SearchGalleryTags { get; set; } = true;

			[UriParamater("f_sdesc")]
			public bool SearchGalleryDescription { get; set; }

			[UriParamater("f_storr")]
			public bool SearchTorrentFilenames { get; set; }

			[UriParamater("f_sdt1")]
			public bool SearchLowPowerTags { get; set; }

			[UriParamater("f_sto")]
			public bool OnlyShowGalleriesWithTorrents { get; set; }

			[UriParamater("f_sdt2")]
			public bool SearchDownvotedTags { get; set; }

			[UriParamater("f_sh")]
			public bool ShowExpungedGalleries { get; set; }

			[UriParamater("f_sr")]
			public bool EnableMinimumRating { get; set; }

			[UriParamater("f_srdd")]
			public int MinimumRating
			{
				get => ratingValue;

				set
				{
					if (value > 5 || value < 2)
					{
						throw new ArgumentException("MinimumRating must between 2 and 5");
					}
					ratingValue = value;
				}
			}

			private int ratingValue = 2;
		}
	}
}

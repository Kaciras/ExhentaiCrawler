using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using Core;

namespace Cli
{
	public sealed class LocalGalleryStore
	{
		public string Name { get; }

		readonly Gallery gallery;
		readonly string directory;

		public LocalGalleryStore(string repository, Gallery gallery)
		{
			this.gallery = gallery;

			// 因为本子名可以重名，所以得把ID带在目录名里
			var info = gallery.Info;
			Name = $"{gallery.Id} - {info.JapaneseName ?? info.Name}";
			directory = Path.Join(repository, Name);
		}

		// 使用序号并填充对齐作为文件名，保证文件顺序跟本子里的顺序一致。
		// 至于图片的原名就没什么用了。
		public FileInfo GetImageFile(int index)
		{
			var nums = (int)Math.Log10(gallery.Info.Length) + 1;
			var fileName = index.ToString().PadLeft(nums, '0');
			return new FileInfo(Path.Join(directory, fileName));
		}

		public void MigrateTo(LocalGalleryStore target)
		{
			Directory.Move(directory, target.directory);

			var nums = (int)Math.Log10(target.gallery.Info.Length) + 1;
			var oldNums = (int)Math.Log10(gallery.Info.Length) + 1;

			if (oldNums != nums)
			{
				new DirectoryInfo(directory)
					.EnumerateFiles()
					.ForEach((file, i) => file.MoveTo(target.GetImageFile(i).FullName));
			}
		}

		public bool Exists() => Directory.Exists(directory);

		public void Create() => Directory.CreateDirectory(directory);
	}
}

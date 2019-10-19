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
		public FileInfo GetImageFile(ImageResource image)
		{
			var name = Prefix(image.Link.Page - 1, image.FileName);
			return new FileInfo(Path.Join(directory, name));
		}

		string Prefix(int index, string rawName)
		{
			var nums = (int)Math.Log10(gallery.Info.Length) + 1;
			var prefix = index.ToString().PadLeft(nums, '0');
			return $"{prefix}_{rawName}";
		}

		// TODO: 这种文件的迁移假定了新版值会添加新图片，而不会修改或删除旧图
		public void MigrateTo(LocalGalleryStore target)
		{
			Directory.Move(directory, target.directory);

			var nums = (int)Math.Log10(target.gallery.Info.Length) + 1;
			var oldNums = (int)Math.Log10(gallery.Info.Length) + 1;

			void MigrageFile(FileInfo file, int index)
			{
				var rawName = file.Name.Split('_', 2)[1];
				var name = target.Prefix(index, rawName);
				file.MoveTo(Path.Join(target.directory, name));
			}

			if (oldNums != nums)
			{
				new DirectoryInfo(directory).EnumerateFiles().ForEach(MigrageFile);
			}
		}

		public bool Exists() => Directory.Exists(directory);

		public void Create() => Directory.CreateDirectory(directory);
	}
}

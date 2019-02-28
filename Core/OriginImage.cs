using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Core.Request;

namespace Core
{
	/// <summary>
	/// 因为获取原始图片地址需要消耗限额，所以单独建立一个对象表示原始图片，以便保存
	/// 相关信息进行更复杂的操作（重试、保存Peer等），而不必重复访问跳转链接。
	/// </summary>
	public sealed class OriginImage
	{
		public Uri Uri { get; }

		private readonly ExhentaiClient client;
		private readonly IPRecord bindIP;

		public OriginImage(ExhentaiClient client, IPRecord bindIP, Uri uri)
		{
			this.client = client;
			this.bindIP = bindIP;
			Uri = uri;
		}

		public Task<Stream> GetStream(bool useBindIP = true)
		{
			if(useBindIP)
			{
				return client.Request(new PeerImageRequest(Uri), bindIP);
			}
			else
			{
				return client.Request(new PeerImageRequest(Uri));
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
	/// <summary>
	/// 因为获取原始图片地址需要消耗限额，所以单独建立一个对象表示原始图片，以便保存
	/// 相关信息进行更复杂的操作（重试、保存Peer等），而不必重复访问跳转链接。
	/// </summary>
	public sealed class OriginImage
	{
		public Uri Uri { get; }

		private readonly IExhentaiClient client;

		public OriginImage(IExhentaiClient client, Uri uri)
		{
			this.client = client;
			Uri = uri;
		}

		public async Task<Stream> GetStream()
		{
			for (int i = 0; i < 2; i++)
			{
				try
				{
					var response = await client.Request(Uri);
					response.EnsureSuccessStatusCode();
					return await response.Content.ReadAsStreamAsync();
				}
				catch (TaskCanceledException)
				{
					// TODO: 据测试这里必须要重试一次，原因未知
				}
			}
			throw new TaskCanceledException();
		}
	}
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cli
{
	/// <summary>
	/// 读取浏览器Cookie存储的类
	/// </summary>
	public interface CookieReader : IAsyncDisposable
	{
		/// <summary>
		/// 读取之前别忘了先调用这个方法哦
		/// </summary>
		Task Open();

		/// <summary>
		/// 读取指定域名的Cookies，返回一个异步枚举器，其元素是以Cookie名为键，Cookie值为值的键值对。
		/// </summary>
		/// <param name="domain">域名</param>
		/// <returns>Cookie枚举器</returns>
		IAsyncEnumerable<KeyValuePair<string, string>> Read(string domain);
	}
}

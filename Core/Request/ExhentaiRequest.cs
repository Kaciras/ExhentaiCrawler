using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Core.Request
{
	/// <summary>
	/// HttpRequestMessage不能复用，而本接口的实现必须能。
	/// 对响应的解析应当放入Client的执行过程，以便对其做统一的异常处理，该接口包含解析逻辑。
	/// </summary>
	/// <typeparam name="T">返回结果类型</typeparam>
	public interface ExhentaiRequest<T>
	{
		/// <summary>
		/// 该请求需要消耗的配额。
		/// </summary>
		int Cost { get; }

		/// <summary>
		/// 在拿到可用的client后执行请求。因为懒得再套一层封装了，所以请勿在此方法里修改HttpClient。
		/// </summary>
		Task<T> Execute(IPRecord ip, HttpClient httpClient);
	}
}

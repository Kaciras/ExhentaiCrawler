using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using Core.Request;

namespace Core
{
	internal sealed class IPRecord
	{
		/// <summary>
		/// 最近被封禁的到期时间，没有被封禁则视为在DateTime.MinValue的时间封禁。
		/// </summary>
		public DateTime BanExpires { get; set; } = DateTime.MinValue;

		/// <summary>
		/// 最近达到限额的时间，没有达到过限额也视为在DateTime.MinValue达到过。
		/// <!--
		///		考虑到用户可能在使用本程序的同时，仍可能在其他地方消耗限额（边下边看），
		///		故不维护限额的具体数值，而是一旦被限制就认为剩余限额为0并记录时间。
		///	-->
		/// </summary>
		public DateTime LimitReached { get; set; } = DateTime.MinValue;

		/// <summary>
		/// 该IP所对应的客户端
		/// </summary>
		public ExhentaiClient Client { get; set; }

		/// <summary>
		/// 该IP是否处于中国特色网络环境中
		/// </summary>
		public bool GFW { get; }

		public IWebProxy Proxy { get; }

		public IPRecord(IWebProxy proxy, bool GFW)
		{
			Proxy = proxy;
			this.GFW = GFW;
		}
	}
}

﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Core
{
	public class ExhentaiClient
	{
		const string GALLERY_RE_TEXT = @"^https://exhentai.org/g/(\d+)/(\w+)/?$";

		static readonly Regex BAN = new Regex(@"ban expires in(?: (\d+) hours)?(?: and (\d)+ minutes)?");
		static readonly Regex GALLERY = new Regex(GALLERY_RE_TEXT, RegexOptions.Compiled | RegexOptions.IgnoreCase);

		readonly HttpClient client;
		readonly CookieContainer cookies;

		public ExhentaiClient(string memberId, string passHash)
		{
			cookies = new CookieContainer();
			cookies.Add(new Cookie("ipb_member_id", memberId, "/", ".exhentai.org"));
			cookies.Add(new Cookie("ipb_pass_hash", passHash, "/", ".exhentai.org"));
			client = new HttpClient(new SocketsHttpHandler { CookieContainer = cookies });
		}

		ExhentaiClient(HttpClient client, CookieContainer cookies)
		{
			this.client = client;
			this.cookies = cookies;
		}

		public Task<Gallery> GetGallery(string url)
		{
			var match = GALLERY.Match(url);
			if (!match.Success)
			{
				throw new ArgumentException(@"画册的URL格式不对，应当符合 " + GALLERY_RE_TEXT);
			}
			return GetGallery(int.Parse(match.Groups[1].Value), match.Groups[2].Value);
		}

		public async Task<Gallery> GetGallery(int id, string token)
		{
			var response = await client.GetAsync($"https://exhentai.org/g/{id}/{token}");
			var content = await response.Content.ReadAsStringAsync();
			CheckResponse(response, content);

			var gallery = new Gallery(this, id, token);
			GalleryParser.Parse(gallery, content);
			return gallery;
		}

		public static async Task<ExhentaiClient> Login(string username, string password)
		{
			var cookieContainer = new CookieContainer();
			var client = new HttpClient(new SocketsHttpHandler { CookieContainer = cookieContainer });

			var x = new HttpRequestMessage(HttpMethod.Post, "https://forums.e-hentai.org/index.php?act=Login&CODE=01");
			x.Headers.Referrer = new Uri("https://e-hentai.org/bounce_login.php?b=d&bt=1-1");

			// 据说这两个头不同会影响返回的页面
			x.Headers.UserAgent.ParseAdd(@"Mozilla/5.0 (Windows NT 6.3; Win64; x64; rv:65.0) Gecko/20100101 Firefox/65.0");
			x.Headers.AcceptLanguage.ParseAdd("zh,zh-CN;q=0.7,en;q=0.3");

			var content = new Dictionary<string, string>
			{
				{ "CookieDate", "1" },
				{ "b", "d" },
				{ "bt", "1-1" },
				{ "UserName", username },
				{ "PassWord", password },
				{ "ipb_login_submit", "Login!" },
			};
			x.Content = new FormUrlEncodedContent(content);

			var response = await client.SendAsync(x);
			response.EnsureSuccessStatusCode();

			var body = await response.Content.ReadAsStringAsync();
			if (!body.Contains("You are now logged in as"))
			{
				throw new ExhentaiException("登录失败");
			}
			return new ExhentaiClient(client, cookieContainer);
		}

		public static void CheckResponse(HttpResponseMessage response, string html)
		{
			var disposition = response.Content.Headers.ContentDisposition;
			if (disposition != null && disposition.FileName == "\"sadpanda.jpg\"")
			{
				throw new ExhentaiException("熊猫了");
			}

			var match = BAN.Match(html);
			if (match.Success)
			{
				var time = int.Parse(match.Groups[1]?.Value ?? "0");
				time += int.Parse(match.Groups[2]?.Value ?? "0") * 60;
				throw new TemporarilyBannedException(time);
			}
		}
	}
}

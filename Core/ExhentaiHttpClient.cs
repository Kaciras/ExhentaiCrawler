using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Core.Infrastructure;

namespace Core
{
	public class ExhentaiHttpClient
	{
		private const int TIMEOUT = 5;
		private const int LIMIT_PERIOD = 20;

		private static readonly Regex BAN = new Regex(@"ban expires in(?: (\d+) hours)?(?: and (\d)+ minutes)?", RegexOptions.Compiled);

		private readonly PriorityQueue<IPRecord> banQueue = new PriorityQueue<IPRecord>((a, b) => DateTime.Compare(a.BanExpires, b.BanExpires));
		private readonly PriorityQueue<IPRecord> limitQueue = new PriorityQueue<IPRecord>((a, b) => DateTime.Compare(a.LimitReached, b.LimitReached));

		private readonly CookieContainer cookieContainer;

		private HttpClient client;

		public LinkedList<IPRecord> Proxies { get; } = new LinkedList<IPRecord>();

		private ExhentaiHttpClient(CookieContainer cookies)
		{
			cookieContainer = cookies;
			Proxies.AddFirst(IPRecord.Local);
			SetHttpClient(null);
		}

		private void SetHttpClient(IPRecord iPRecord)
		{
			var handler = new SocketsHttpHandler
			{
				AllowAutoRedirect = true,
				CookieContainer = cookieContainer,
				AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
				ConnectTimeout = TimeSpan.FromSeconds(3),
			};
			//iPRecord.ConfigureHttpHandler(handler);

			client?.Dispose();
			client = new HttpClient(new RetryHandler(handler), true)
			{
				Timeout = TimeSpan.FromSeconds(TIMEOUT)
			};
		}

		// 查找一个可用的IP
		private bool TryGetAvailable(out IPRecord record)
		{
			var free = Proxies.First;
			if (free != null)
			{
				record = free.Value;
				Proxies.RemoveFirst();
			}
			else
			{
				record = FindInQueue(banQueue) ?? FindInQueue(limitQueue);
			}
			return record != null;
		}

		private IPRecord FindInQueue(PriorityQueue<IPRecord> queue)
		{
			if (queue.Count == 0)
			{
				return null;
			}
			var now = DateTime.Now;
			var aval = queue.Peek();

			if (aval.BanExpires < now)
			{
				// 每分钟回复3点限额
				var costReduction = (now - aval.LimitReached).Minutes * 3;
				if (costReduction > LIMIT_PERIOD)
				{
					return banQueue.Dequeue();
				}
			}

			return null;
		}

		internal async Task<HttpContent> RequestImage(string url)
		{
			var response = await client.GetAsync(url);

			// 有可能是fullimg.php跳转，说好的 AllowAutoRedirect 呢？
			if (response.StatusCode == HttpStatusCode.Redirect)
			{
				var redirect = response.Headers.Location;
				response.Dispose();
				response = await client.GetAsync(redirect);
			}

			response.EnsureSuccessStatusCode();
			return response.Content;
		}

		internal async Task<string> RequestPage(string url)
		{
			var request = new HttpRequestMessage(HttpMethod.Get, url);

			// 据说这两个头不同会影响返回的页面
			request.Headers.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
			request.Headers.UserAgent.ParseAdd(@"Mozilla/5.0 (Windows NT 6.3; Win64; x64; rv:65.0) Gecko/20100101 Firefox/65.0");
			request.Headers.AcceptLanguage.ParseAdd("zh,zh-CN;q=0.7,en;q=0.3");

			using (var response = await client.SendAsync(request))
			{
				var content = await response.Content.ReadAsStringAsync();
				CheckResponse(response, content);
				return content;
			}
		}

		internal async Task<JObject> RequestApi(object body)
		{
			var content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
			var response = await client.PostAsync("https://exhentai.org/api.php", content);
			var text = await response.Content.ReadAsStringAsync();

			try
			{
				return JsonConvert.DeserializeObject<JObject>(text);
			}
			catch (JsonException)
			{
				throw new ExhentaiException("API请求出错：" + text);
			}
		}

		public static ExhentaiHttpClient FromCookie(string memberId, string passHash)
		{
			var cookies = new CookieContainer();
			cookies.Add(new Cookie("ipb_member_id", memberId, "/", ".exhentai.org"));
			cookies.Add(new Cookie("ipb_pass_hash", passHash, "/", ".exhentai.org"));
			return new ExhentaiHttpClient(cookies);
		}

		public static async Task<ExhentaiHttpClient> Login(string username, string password)
		{
			var cookieContainer = new CookieContainer();
			var client = new HttpClient(new SocketsHttpHandler
			{
				AllowAutoRedirect = true,
				CookieContainer = cookieContainer,
				AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
			});

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
			return new ExhentaiHttpClient(cookieContainer);
		}

		/// <summary>
		/// 检查返回的页面，判断是否出现熊猫和封禁。
		/// </summary>
		/// <param name="response">响应</param>
		/// <param name="html">HTML页面</param>
		/// <exception cref="TempBannedException">如果被封禁了</exception>
		/// <exception cref="ExhentaiException">如果出现熊猫</exception>
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
				throw new TempBannedException(time);
			}
		}
	}
}

using Shortnr.Web.Data;
using Shortnr.Web.Entities;
using Shortnr.Web.Exceptions;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Shortnr.Web.Business.Implementations
{
	public class UrlManager : IUrlManager
	{
		private ShortnrContext context;
		public UrlManager(ShortnrContext context)
		{
			this.context = context;
		}
		public Task<ShortUrl> ShortenUrl(string longUrl, string ip, string segment = "")
		{
			return Task.Run(() =>
			{
				{
					ShortUrl url;

					if (!longUrl.StartsWith("http://") && !longUrl.StartsWith("https://"))
					{
						throw new ArgumentException("Invalid URL format");
					}
					Uri urlCheck = new Uri(longUrl);
					HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlCheck);
					request.Timeout = 10000;
					try
					{
						HttpWebResponse response = (HttpWebResponse)request.GetResponse();
					}
					catch (Exception)
					{
						throw new ShortnrNotFoundException();
					}

					int cap = 0;
					string capString = ConfigurationManager.AppSettings["MaxNumberShortUrlsPerHour"];
					int.TryParse(capString, out cap);
					DateTime dateToCheck = DateTime.Now.Subtract(new TimeSpan(1, 0, 0));

					if (!string.IsNullOrEmpty(segment))
					{
						if (context.ShortUrls.Where(u => u.Segment == segment).Any())
						{
							throw new ShortnrConflictException();
						}
						if (segment.Length > 20 || !Regex.IsMatch(segment, @"^[A-Za-z\d_-]+$"))
						{
							throw new ArgumentException("Malformed or too long segment");
						}
					}
					else
					{
						segment = this.NewSegment();
					}

					if (string.IsNullOrEmpty(segment))
					{
						throw new ArgumentException("Segment is empty");
					}

					url = new ShortUrl()
					{
						Added = DateTime.Now,
						Ip = ip,
						LongUrl = longUrl,
						NumOfClicks = 0,
						Segment = segment
					};

					context.ShortUrls.Add(url);

					context.SaveChanges();

					return url;
				}
			});
		}

		public Task<Stat> Click(string segment, string referer, string ip)
		{
			return Task.Run(() =>
			{
				using (var ctx = new ShortnrContext())
				{

					Stat stat = new Stat()
					{
						ClickDate = DateTime.Now,
						Ip = ip,
						Referer = referer
					};


					ctx.SaveChanges();

					return stat;
				}
			});
		}

		private string NewSegment()
		{
			using (var ctx = new ShortnrContext())
			{
				int i = 0;
				while (true)
				{
					string segment = Guid.NewGuid().ToString().Substring(0, 6);
					return segment;
				}
			}
		}
	}
}


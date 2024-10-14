using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commune.Html
{
	public class LinkDepot
	{
		readonly object lockObj = new();

		readonly string webRootPath;
		readonly Dictionary<string, string> urlBySubPath = new();

		public LinkDepot(string webRootPath)
		{
			this.webRootPath = webRootPath;
		}

		public string StaticUrl(string subPath)
		{
			lock (lockObj)
			{
				if (urlBySubPath.TryGetValue(subPath, out string? url))
					return url;

				url = FileUrl(webRootPath, subPath);
				urlBySubPath[subPath] = url;

				return url;
			}
		}

		public string DynamicUrl(string subPath)
		{
			return FileUrl(webRootPath, subPath);
		}


		readonly static long refTicks = new DateTime(2022, 1, 1).Ticks;
		public const long ticksInSecond = 10000000;

		public static string FileUrl(string webRootPath, string subPath)
		{
			string[] parts = subPath.Split('/');
			string filePath = Path.Combine(parts.Prepend(webRootPath).ToArray());
			FileInfo imageInfo = new(filePath);

			return string.Format(@"{0}?v={1}",
				subPath, Math.Max(0, imageInfo.LastWriteTimeUtc.Ticks - refTicks) / ticksInSecond);
		}
	}
}

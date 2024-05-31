using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commune.Basis
{
	public class LoggerHlp
	{
		public static void CreateFileLog(string logPath, long fileSizeLimitBytes)
		{
			Log.Logger = new LoggerConfiguration()
					.WriteTo.File(
						logPath,
						fileSizeLimitBytes: fileSizeLimitBytes,
						rollOnFileSizeLimit: true,
						retainedFileCountLimit: 2,
						encoding: Encoding.UTF8,
						outputTemplate: "[{Timestamp:dd.MM.yyyy HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}{NewLine}"
					)
					.CreateLogger();
		}

		public static void LogStarting()
		{
			Log.Information("{0}{0}{0}{1}{0}", System.Environment.NewLine,
				"<===============================================================================>"
			);

			Log.Information("Приложение стартовало");
		}


	}
}

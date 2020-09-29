using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;
using Autofac.Extensions.DependencyInjection;

namespace Repositorch.Web
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
			try
			{
				var host = Host.CreateDefaultBuilder(args)
					.UseServiceProviderFactory(new AutofacServiceProviderFactory())
					.ConfigureWebHostDefaults(webHostBuilder =>
					{
						webHostBuilder
							.UseContentRoot(Directory.GetCurrentDirectory())
							.UseIISIntegration()
							.UseStartup<Startup>();
					})
					.ConfigureLogging(logging =>
					{
						logging.ClearProviders();
						logging.SetMinimumLevel(LogLevel.Trace);
					})
					.UseNLog()
					.Build();

				host.Run();
			}
			catch (Exception exception)
			{
				logger.Error(exception);
				throw;
			}
			finally
			{
				NLog.LogManager.Shutdown();
			}
		}
	}
}

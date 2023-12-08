using System;
using System.IO;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Web;

namespace Repositorch.Web
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var logger = LogManager.Setup().LoadConfigurationFromFile("nlog.config").GetCurrentClassLogger();
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
						logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Information);
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
				LogManager.Shutdown();
			}
		}
	}
}

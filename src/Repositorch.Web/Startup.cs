using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Autofac;
using LiteDB;

namespace Repositorch.Web
{
	public class Startup
	{
		public Startup(IWebHostEnvironment env)
		{
			var builder = new ConfigurationBuilder()
				.SetBasePath(env.ContentRootPath)
				.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
				.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
				.AddEnvironmentVariables();
			this.Configuration = builder.Build();
		}

		public IConfiguration Configuration { get; }

		public void ConfigureServices(IServiceCollection services)
		{
			services.AddSignalR();
			services.AddHostedService<MappingService>();
			services.AddControllers().AddNewtonsoftJson();

			// In production, the React files will be served from this directory
			services.AddSpaStaticFiles(configuration =>
			{
				configuration.RootPath = "ClientApp/build";
			});
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler("/Error");
			}

			app.UseStaticFiles();
			app.UseSpaStaticFiles();
			
			app.UseRouting();
			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllerRoute(
					name: "default",
					pattern: "{controller}/{action=Index}/{id?}");
				endpoints.MapHub<MappingHub>("/Hubs/Mapping");
			});

			app.UseSpa(spa =>
			{
				spa.Options.SourcePath = "ClientApp";

				if (env.IsDevelopment())
				{
					spa.UseReactDevelopmentServer(npmScript: "start");
				}
			});
		}

		public void ConfigureContainer(ContainerBuilder builder)
		{
			RegisterLiteDB(builder);
			RegisterMetricsAndMenusForThem(builder);
		}

		private void RegisterLiteDB(ContainerBuilder builder)
		{
			var homePath = (Environment.OSVersion.Platform == PlatformID.Unix ||
				Environment.OSVersion.Platform == PlatformID.MacOSX)
				? Environment.GetEnvironmentVariable("HOME")
				: Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");
			var dbPath = Path.Combine(homePath, "repositorch.db");
			builder.Register<LiteDatabase>(c => new LiteDatabase(dbPath))
				.As<LiteDatabase>()
				.SingleInstance();
		}
		private void RegisterMetricsAndMenusForThem(ContainerBuilder builder)
		{
			var metrics = Assembly.GetExecutingAssembly().GetTypes()
				.Where(t => t.IsClass && t.IsAssignableTo<IMetric>())
				.ToArray();
			Dictionary<string, List<Type>> menus = new Dictionary<string, List<Type>>();
			var menuRootNamespace = "Repositorch.Web.Metrics";

			foreach (var metric in metrics)
			{
				builder.RegisterType(metric)
					.Keyed<IMetric>(metric.Name);

				var metricPath = metric.FullName.Replace(menuRootNamespace + '.', "");
				builder.Register(x =>
				{
					Console.WriteLine(x);
					return new string[] { };
				})
				.Keyed<string[]>(metricPath);

				//if (!menus.ContainsKey(metricNamespace))
				//{
				//	menus.Add(metricNamespace, new List<Type>());
				//}
				//menus[metricNamespace].Add(metric);
			}
		}
	}
}

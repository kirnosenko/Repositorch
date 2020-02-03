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
			Dictionary<string, List<object>> menus = new Dictionary<string, List<object>>();
			
			foreach (var metric in metrics)
			{
				var metricPath = metric.GetMetricPath();
				builder.RegisterType(metric)
					.Keyed<IMetric>(metricPath);

				var metricRootPath = metric.GetMetricRootPath();
				if (!menus.ContainsKey(metricRootPath))
				{
					menus.Add(metricRootPath, new List<object>());
				}
				menus[metricRootPath].Add(new
				{
					Name = metric.Name,
					Path = metricPath,
					IsMetric = true
				});
			}

			var rootPaths = new Stack<string>(menus.Keys
				.OrderBy(x => x.Length));
			while (rootPaths.Count > 1)
			{
				var path = rootPaths.Pop();
				var root = rootPaths
					.Where(x => path.StartsWith(x))
					.OrderByDescending(x => x.Length)
					.FirstOrDefault();
				if (root != null)
				{
					menus[root].Add(new
					{
						Name = root != string.Empty
							? path.Replace(root + '/', "")
							: path,
						Path = path,
						IsMetric = false
					});
					menus[path].Add(new
					{
						Path = root,
						IsMetric = false
					});
				}
			}

			foreach (var m in menus)
			{
				builder.RegisterInstance(m.Value)
					.Keyed<List<object>>(m.Key);
			}
		}
	}
}

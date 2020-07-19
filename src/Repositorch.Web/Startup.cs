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
using Repositorch.Web.Middleware;
using Repositorch.Web.Options;
using Microsoft.Extensions.Options;

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
			services.Configure<DataStoreOptionsCollection>(Configuration);
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

			app.UseMiddleware<TimerMiddleware>();

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

			builder.RegisterType<MappingNotifier>()
				.As<IMappingNotifier>()
				.SingleInstance();
			builder.RegisterType<ProjectManager>()
				.As<IProjectManager>()
				.SingleInstance();
		}

		private void RegisterLiteDB(ContainerBuilder builder)
		{
			var dbPath = Path.Combine(EnvironmentExtensions.GetHomePath(), "repositorch.db");
			builder.Register<LiteDatabase>(c => {
				var db = new LiteDatabase(dbPath);
				db.Mapper.EmptyStringToNull = false;
				return db;
			}).As<LiteDatabase>().SingleInstance();
		}
		private void RegisterMetricsAndMenusForThem(ContainerBuilder builder)
		{
			var metrics = Assembly.GetExecutingAssembly().GetTypes()
				.Where(t => t.IsClass && !t.IsAbstract && t.IsAssignableTo<IMetric>())
				.ToArray();
			
			foreach (var metric in metrics)
			{
				var metricPath = MetricMenu.GetMetricPath(metric);
				builder.RegisterType(metric)
					.Keyed<IMetric>(metricPath);
			}

			var menus = MetricMenu.GetLinkedMetricMenu(metrics);
			foreach (var m in menus)
			{
				builder.RegisterInstance(m.Value)
					.Keyed<List<MetricMenu.MetricMenuItem>>(m.Key);
			}
		}
	}
}

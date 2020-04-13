using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Repositorch.Web.Middleware
{
	public class TimerMiddleware
	{
		private readonly RequestDelegate _next;

		public TimerMiddleware(RequestDelegate next)
		{
			_next = next;
		}

		public async Task InvokeAsync(HttpContext context)
		{
			using (var time = TimeLogger.Start())
			{
				context.Response.OnStarting(state =>
				{
					var httpContext = (HttpContext)state;
					httpContext.Response.Headers.Add("Time", time.FormatedTime);
					return Task.CompletedTask;
				}, context);

				await _next(context);
			}
		}
	}
}

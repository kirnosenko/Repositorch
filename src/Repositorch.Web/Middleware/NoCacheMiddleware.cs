using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Repositorch.Web.Middleware
{
	public class NoCacheMiddleware
	{
		private readonly RequestDelegate _next;

		public NoCacheMiddleware(RequestDelegate next)
		{
			_next = next;
		}

		public async Task InvokeAsync(HttpContext context)
		{
			context.Response.OnStarting(state =>
			{
				var httpContext = (HttpContext)state;
				httpContext.Response.Headers["Cache-Control"] = "no-store";
				return Task.CompletedTask;
			}, context);

			await _next(context);
		}
	}
}

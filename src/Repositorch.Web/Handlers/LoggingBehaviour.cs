using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;

namespace Repositorch.Web.Handlers
{
	public interface ILogged
	{
	}

	public class LoggingBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
	{
		private readonly ILogger<LoggingBehaviour<TRequest, TResponse>> logger;

		public LoggingBehaviour(ILogger<LoggingBehaviour<TRequest, TResponse>> logger)
		{
			this.logger = logger;
		}
		public async Task<TResponse> Handle(
			TRequest request,
			CancellationToken cancellationToken,
			RequestHandlerDelegate<TResponse> next)
		{
			if (typeof(ILogged).IsAssignableFrom(typeof(TRequest)))
			{
				logger.LogInformation("Request: {0}", JsonConvert.SerializeObject(request));
			}
			var response = await next();
			if (typeof(ILogged).IsAssignableFrom(typeof(TResponse)))
			{
				logger.LogInformation("Response: {0}", JsonConvert.SerializeObject(response));
			}

			return response;
		}
	}
}

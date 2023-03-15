using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;

namespace Repositorch.Web.Handlers
{
	public interface ICustomLogObject
	{
		object CustomLogObject { get; }
	}

	public class LoggingBehaviour<TRequest, TResponse> :
		IPipelineBehavior<TRequest, TResponse>
		where TRequest : notnull
	{
		private readonly ILogger logger;

		public LoggingBehaviour(ILogger<LoggingBehaviour<TRequest, TResponse>> logger)
		{
			this.logger = logger;
		}

		public async Task<TResponse> Handle(
			TRequest request,
			RequestHandlerDelegate<TResponse> next,
			CancellationToken cancellationToken)
		{
			var requestToLog = typeof(ICustomLogObject).IsAssignableFrom(typeof(TRequest))
				? (request as ICustomLogObject).CustomLogObject
				: request;
			if (logger.IsEnabled(LogLevel.Trace) && requestToLog != null)
			{
				logger.LogTrace($"{typeof(TRequest)}: {JsonConvert.SerializeObject(requestToLog)}");
			}
			
			var response = await next();

			var responseToLog = typeof(ICustomLogObject).IsAssignableFrom(typeof(TResponse))
				? (response as ICustomLogObject).CustomLogObject
				: response;
			if (logger.IsEnabled(LogLevel.Trace) && responseToLog != null)
			{
				logger.LogTrace($"{typeof(TResponse)}: {JsonConvert.SerializeObject(responseToLog)}");
			}
			
			return response;
		}
	}
}

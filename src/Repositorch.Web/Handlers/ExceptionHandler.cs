using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MediatR.Pipeline;
using Microsoft.Extensions.Logging;

namespace Repositorch.Web.Handlers
{
	public class ExceptionHandler<TRequest, TResponse, TException> : IRequestExceptionHandler<TRequest, TResponse, TException>
		where TRequest : IRequest<TResponse>
		where TException : Exception
	{
		private readonly ILogger<ExceptionHandler<TRequest, TResponse, TException>> logger;

		public ExceptionHandler(ILogger<ExceptionHandler<TRequest, TResponse, TException>> logger)
		{
			this.logger = logger;
		}

		public Task Handle(TRequest request, TException exception, RequestExceptionHandlerState<TResponse> state, CancellationToken cancellationToken)
		{
			logger.LogError("Error: {0}", exception);
			
			return Task.CompletedTask;
		}
	}
}

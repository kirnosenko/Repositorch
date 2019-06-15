using System;
using Microsoft.Extensions.Logging;

namespace Repositorch.Data.Entities.EF
{
	public class InMemoryDataLogger : ILogger
	{
		public class Scope : IDisposable
		{
			public void Dispose()
			{
			}
		}

		private Action<string> output;

		public InMemoryDataLogger(Action<string> output)
		{
			this.output = output;
		}
		public void Log<TState>(
			LogLevel logLevel,
			EventId eventId,
			TState state,
			Exception exception,
			Func<TState, Exception, string> formatter)
		{
			if (formatter == null)
			{
				throw new ArgumentNullException(nameof(formatter));
			}

			string message = formatter(state, exception);
			if (string.IsNullOrEmpty(message) && exception == null)
			{
				return;
			}

			output(message);
			if (exception != null)
			{
				output(exception.ToString());
			}
		}
		public bool IsEnabled(LogLevel logLevel)
		{
			return true;
		}
		public IDisposable BeginScope<TState>(TState state)
		{
			return new Scope();
		}
	}
}

using System;

using Microsoft.Extensions.Logging;

namespace EntityFX.Core
{
	public class DataContextLoggerProvider : ILoggerProvider
	{
		public static ILoggerFactory CreateFactory(Action<LogLevel, string> printOut)
		{
			var loggerFactory = new LoggerFactory();
			loggerFactory.AddProvider(new DataContextLoggerProvider(printOut));
			return loggerFactory;
		}

		readonly Action<LogLevel, string> _printOut;

		public DataContextLoggerProvider(Action<LogLevel, string> printOut)
		{
			_printOut = printOut ?? throw new ArgumentNullException();
		}


		public void Dispose() { }

		public ILogger CreateLogger(string categoryName) => new DataContextLogger(_printOut);

		private class DataContextLogger : ILogger
		{
			readonly Action<LogLevel, string> _LogIt;

			public DataContextLogger(Action<LogLevel, string> printOut)
			{
				_LogIt = printOut ?? throw new ArgumentNullException();
			}

			public void Log<TState>(
				LogLevel logLevel,
				EventId eventId,
				TState state,
				Exception exception,
				Func<TState, Exception, string> formatter)
			{
				string result = formatter(state, exception);

				_LogIt(logLevel, result);
			}

			public bool IsEnabled(LogLevel logLevel) => true;

			public IDisposable BeginScope<TState>(TState state) => null;
		}
	}
}
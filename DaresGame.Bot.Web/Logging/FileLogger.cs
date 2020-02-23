using System;
using System.IO;
using Microsoft.Extensions.Logging;

namespace DaresGame.Bot.Web.Logging
{
    internal class FileLogger : ILogger
    {
        private readonly string _path;
        private readonly LogLevel _logLevel;

        private static readonly object Lock = new object();

        public FileLogger(string path, LogLevel logLevel)
        {
            _path = path;
            _logLevel = logLevel;
        }

        public IDisposable BeginScope<TState>(TState state) => null;

        public bool IsEnabled(LogLevel logLevel) => logLevel >= _logLevel;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
            Func<TState, Exception, string> formatter)
        {
            if (IsEnabled(logLevel) && (formatter != null))
            {
                lock (Lock)
                {
                    File.AppendAllText(_path, formatter(state, exception) + Environment.NewLine);
                }
            }
        }
    }
}
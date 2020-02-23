using Microsoft.Extensions.Logging;

namespace DaresGame.Bot.Web.Logging
{
    internal class FileLoggerProvider : ILoggerProvider
    {
        private readonly string _path;
        private readonly LogLevel _logLevel;

        public FileLoggerProvider(string path, LogLevel logLevel)
        {
            _path = path;
            _logLevel = logLevel;
        }

        public ILogger CreateLogger(string categoryName) { return new FileLogger(_path, _logLevel); }

        public void Dispose() { }
    }
}

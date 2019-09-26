using System;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Own.BlockchainExplorer.Common.Extensions;

namespace Own.BlockchainExplorer.Common
{
    public static class Log
    {
        private static ILogger _log;

        class ConsoleLog : ILogEventSink
        {
            public void Emit(LogEvent logEvent)
            {
                Console.WriteLine($"{DateTimeOffset.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff")} {logEvent.Level} | {logEvent.MessageTemplate.Render(logEvent.Properties)}");
            }
        }

        public static void Initialize(string fileName)
        {
            _log = new LoggerConfiguration()
                .WriteTo.File(fileName)
                .WriteTo.Sink(new ConsoleLog())
                .CreateLogger();
        }

        public static void Debug(string format, params object[] args)
        {
#if DEBUG
            _log.Write(LogEventLevel.Debug, format, args);
#endif
        }

        public static void Info(string format, params object[] args)
        {
            _log.Write(LogEventLevel.Information, format, args);
        }

        public static void Warning(string format, params object[] args)
        {
            _log.Write(LogEventLevel.Warning, format, args);
        }

        public static void Error(string format, params object[] args)
        {
            _log.Write(LogEventLevel.Error, format, args);
        }

        public static void Error(Exception ex)
        {
            _log.Write(LogEventLevel.Error, ex.LogFormat(), "");
        }
    }
}

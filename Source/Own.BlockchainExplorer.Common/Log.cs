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
        private static readonly ConsoleColor _defaultColor = Console.ForegroundColor;

        private class ConsoleLog : ILogEventSink
        {
            public void Emit(LogEvent logEvent)
            {
                Console.WriteLine("{0} {1} | {2}",
                    DateTimeOffset.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                    logEvent.Level.ToString().Substring(0, 3).ToUpper(),
                    logEvent.MessageTemplate.Render(logEvent.Properties));
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
            Console.ForegroundColor = ConsoleColor.DarkGray;
            _log.Write(LogEventLevel.Debug, format, args);
            Console.ForegroundColor = _defaultColor;
#endif
        }

        public static void Info(string format, params object[] args)
        {
            Console.ForegroundColor = ConsoleColor.White;
            _log.Write(LogEventLevel.Information, format, args);
            Console.ForegroundColor = _defaultColor;
        }

        public static void Warning(string format, params object[] args)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            _log.Write(LogEventLevel.Warning, format, args);
            Console.ForegroundColor = _defaultColor;
        }

        public static void Error(string format, params object[] args)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            _log.Write(LogEventLevel.Error, format, args);
            Console.ForegroundColor = _defaultColor;
        }

        public static void Error(Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            _log.Write(LogEventLevel.Error, ex.LogFormat(), "");
            Console.ForegroundColor = _defaultColor;
        }
    }
}

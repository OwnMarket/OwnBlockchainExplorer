using Own.BlockchainExplorer.Common.Extensions;
using Serilog;
using Serilog.Events;
using System;

namespace Own.BlockchainExplorer.Common
{
    public static class Log
    {
        private static ILogger _log;

        public static void Initialize(string fileName)
        {
            _log = new LoggerConfiguration().WriteTo.Console().CreateLogger();
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

using System;
using System.Collections.Generic;
using System.Linq;

namespace Own.BlockchainExplorer.Common.Extensions
{
    public static class ExceptionExtensions
    {
        public static IEnumerable<Exception> Flatten(this Exception ex)
        {
            var currentException = ex;
            while (currentException != null)
            {
                yield return currentException;
                currentException = currentException.InnerException;
            }
        }

        public static IEnumerable<string> GetAllMessages(this Exception ex)
        {
            return ex.Flatten()
                .Select(e => e.Message)
                .ToList();
        }

        public static string LogFormat(this Exception ex, bool messagesOnly = false)
        {
            var exceptions = ex.Flatten();

            var log = exceptions
                .Select(e => $"{e.GetType().FullName}: {e.Message}")
                .To(ms => string.Join("\n>>> ", ms));

            if (!messagesOnly)
            {
                log += "\n" + exceptions
                    .Select(e => e.StackTrace)
                    .To(sts => string.Join("\n--- INNER EXCEPTION ---\n", sts));
            }

            return log;
        }
    }
}

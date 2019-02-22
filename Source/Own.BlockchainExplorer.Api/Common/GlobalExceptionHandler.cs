using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Own.BlockchainExplorer.Common.Framework;

namespace Own.BlockchainExplorer.Api.Common
{
    public class GlobalExceptionHandler
    {
        private readonly RequestDelegate _nextAction;
        private ILogger _logger;
        public GlobalExceptionHandler(RequestDelegate next)
        {
            _nextAction = next;
        }

        private static Result GenericError =>
            Result.Failure("Something went wrong during request execution.");

        public async Task Invoke(HttpContext context, ILoggerFactory loggerFactory)
        {
            try
            {
                await _nextAction(context);
            }
            catch (Exception ex)
            {
                if (_logger == null)
                {
                    _logger = loggerFactory.CreateLogger("Debug");
                }

                _logger.LogError(ex, string.Empty);

                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonConvert.SerializeObject(GenericError));
            }
        }
    }
}

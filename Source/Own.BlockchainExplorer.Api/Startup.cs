using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Converters;
using Own.BlockchainExplorer.Api.Common;
using Own.BlockchainExplorer.Core;

namespace Own.BlockchainExplorer.Api
{
    public class Startup
    {
        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            ConfigureLogging(loggerFactory);

            app.UseMiddleware<GlobalExceptionHandler>()
                .UseCors("Default")
                .UseMvc();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            new Domain.DI.DomainModule().Load(services);
            new Infrastructure.DI.InfrastructureModule().Load(services);

            services
                .AddCors(options =>
                {
                    options.AddPolicy("Default", builder =>
                    {
                        builder
                            .WithOrigins(Config.AccessControlAllowOrigins)
                            .WithMethods(Config.AccessControlAllowMethods)
                            .WithHeaders(Config.AccessControlAllowHeaders)
                            .WithExposedHeaders(Config.AccessControlExposeHeaders);
                    });
                })
                .AddMvc()
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.Converters.Add(new StringEnumConverter());
                    options.SerializerSettings.DateFormatString = "yyyy-MM-ddTHH:mm:ss";
                });
        }

        private void ConfigureLogging(ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(
#if !DEBUG
                LogLevel.Warning
#endif
            );
        }
    }
}

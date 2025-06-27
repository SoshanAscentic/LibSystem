using LibSystem.Utils.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace LibSystem.Utils.Extensions
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder UseLibSystemSerilog(this IHostBuilder hostBuilder)
        {
            return hostBuilder.UseSerilog((context, services, configuration) =>
            {
                var logger = SerilogConfiguration.CreateLogger(
                    context.Configuration,
                    context.HostingEnvironment);

                configuration.ReadFrom.Services(services)
                    .ReadFrom.Configuration(context.Configuration)
                    .Enrich.FromLogContext()
                    .WriteTo.Logger(logger);
            });
        }
    }
}
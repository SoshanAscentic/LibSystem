using LibSystem.Utils.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace LibSystem.Utils.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddUtilsServices(
            this IServiceCollection services,
            IConfiguration configuration,
            IHostEnvironment environment)
        {
            // Configure Serilog
            services.AddSerilogServices(configuration, environment);
            return services;
        }

        private static IServiceCollection AddSerilogServices(
            this IServiceCollection services,
            IConfiguration configuration,
            IHostEnvironment environment)
        {
            // Create the logger
            var logger = SerilogConfiguration.CreateLogger(configuration, environment);

            // Set as the global logger
            Log.Logger = logger;

            // Register Serilog
            services.AddSerilog(logger);

            return services;
        }
    }
}
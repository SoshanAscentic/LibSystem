// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DependencyInjection.cs" company="Ascentic">
//   Copyright (c) Ascentic. All rights reserved.
// </copyright>
// <summary>
//   Provides methods for registering application services.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace LibSystem.Application
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using FluentValidation;
    using LibSystem.Application.Common.Behaviors;
    using MediatR;
    using Microsoft.Extensions.DependencyInjection;

    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Register AutoMapper with all profiles from this assembly
            services.AddAutoMapper(Assembly.GetExecutingAssembly());

            // Register FluentValidation validators from this assembly
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            // Register MediatR with handlers from this assembly
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            });

            // Registering Pipeline behaviour in Order matters: Validation -> Logging -> Performance -> Unhandled Exception
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehaviour<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>));

            return services;
        }
    }
}

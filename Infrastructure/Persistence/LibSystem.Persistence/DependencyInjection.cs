using LibSystem.Application.Contracts.Repositories;
using LibSystem.Application.Contracts.UoW;
using LibSystem.Domain.Common;
using LibSystem.Domain.ValueObjects;
using LibSystem.Persistence.Context;
using LibSystem.Persistence.Repositories;
using LibSystem.Persistence.UoW;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Persistence
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPersistenceServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Register DbContext with SQL Server provider
            services.AddDbContext<LibraryDbContext>(options =>
            {
                // Get connection string from configuration
                var connectionString = configuration.GetConnectionString("DefaultConnection")
                    ?? throw new InvalidOperationException("DefaultConnection string is not configured");

                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    // Configure SQL Server specific options
                    sqlOptions.MigrationsAssembly(typeof(LibraryDbContext).Assembly.FullName);
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                });

                // Configure EF Core options
                options.EnableSensitiveDataLogging(false);
                options.EnableDetailedErrors(false);

                // Suppress the pending model changes warning for dynamic values in HasData
                options.ConfigureWarnings(warnings =>
                    warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
            });

            // Register Generic Repository
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            // Register Specific Repository Implementations
            services.AddScoped<IBookRepository, BookRepository>();
            services.AddScoped<IMemberRepository, MemberRepository>();
            services.AddScoped<IBorrowingRepository, BorrowingRepository>();

            // Register Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}

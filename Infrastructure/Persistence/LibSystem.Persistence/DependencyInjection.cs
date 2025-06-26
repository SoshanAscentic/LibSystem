using LibSystem.Application.Common.Interfaces;
using LibSystem.Application.Repositories;
using LibSystem.Domain.Common;
using LibSystem.Domain.ValueObjects;
using LibSystem.Persistence.Repositories;
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

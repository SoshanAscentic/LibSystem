using LibSystem.Api.Endpoints;
using LibSystem.Api.Middleware;
using LibSystem.Application;
using LibSystem.Persistence;
using LibSystem.Persistence.Context;
using LibSystem.Utils.Extensions;
using LibSystem.Utils.Logging;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.OpenApi.Models;
using Serilog;

namespace LibSystem.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // Create bootstrap logger for startup
            Log.Logger = SerilogConfiguration.CreateBootstrapLogger();

            try
            {
                Log.Information("Starting Library System API");

                var builder = WebApplication.CreateBuilder(args);

                // Configure Serilog through Utils layer
                builder.Host.UseLibSystemSerilog();

                // Configure services
                ConfigureServices(builder);

                var app = builder.Build();

                // Configure middleware pipeline
                ConfigureMiddleware(app);

                // Configure endpoints
                ConfigureEndpoints(app);

                // Initialize database
                await InitializeDatabaseAsync(app);

                Log.Information("Library System API configured successfully");

                // Run the application
                await app.RunAsync();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static void ConfigureServices(WebApplicationBuilder builder)
        {
            // Add Utils services (including Serilog)
            builder.Services.AddUtilsServices(builder.Configuration, builder.Environment);

            // Add Swagger
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Library System API",
                    Version = "v1",
                    Description = "A comprehensive library management system API built with Clean Architecture, DDD, and CQRS patterns"
                });
            });

            // Add CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            // Add other services
            builder.Services.AddHealthChecks();
            builder.Services.AddResponseCaching();
            builder.Services.AddMemoryCache();

            // Add rate limiting
            builder.Services.AddRateLimiter(options =>
            {
                options.AddFixedWindowLimiter("api", limiterOptions =>
                {
                    limiterOptions.PermitLimit = 100;
                    limiterOptions.Window = TimeSpan.FromMinutes(1);
                });
            });

            // Register Application Layer services
            builder.Services.AddApplicationServices();

            // Register Infrastructure Layer services
            builder.Services.AddPersistenceServices(builder.Configuration);
        }

        private static void ConfigureMiddleware(WebApplication app)
        {
            #region Development vs Production Configuration

            if (app.Environment.IsDevelopment())
            {
                // Development-specific middleware
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Library System API v1");
                    c.RoutePrefix = string.Empty; // Serve Swagger UI at root

                    // Enhanced UI features for development
                    c.DefaultModelsExpandDepth(1);
                    c.DefaultModelExpandDepth(1);
                    c.DisplayRequestDuration();
                    c.EnableDeepLinking();
                    c.EnableFilter();
                    c.ShowExtensions();
                    c.EnableValidator();
                    c.ShowCommonExtensions();
                });

                app.UseDeveloperExceptionPage();
            }
            else
            {
                // Production-specific middleware
                app.UseHsts(); // HTTP Strict Transport Security
            }

            #endregion

            #region Security Middleware

            // Global exception handling (should be early in pipeline)
            app.UseGlobalExceptionHandler();

            // HTTPS redirection
            app.UseHttpsRedirection();

            // CORS
            var corsPolicy = app.Environment.IsDevelopment() ? "AllowAll" : "Production";
            app.UseCors(corsPolicy);

            #endregion


            // Response caching
            app.UseResponseCaching();

             app.UseRateLimiter();



            // Authentication and Authorization (when implemented)
            // app.UseAuthentication();
            // app.UseAuthorization();



            app.UseHealthChecks("/health");

        }

        private static void ConfigureEndpoints(WebApplication app)
        {
            // Map all endpoint groups
            app.MapBookEndpoints();
            app.MapMemberEndpoints();
            app.MapBorrowingEndpoints();

            // Root endpoint with API information
            app.MapGet("/", GetApiInfo)
                .WithName("GetApiInfo")
                .WithTags("General")
                .WithSummary("Get API information and available endpoints")
                .Produces<ApiInfoResponse>()
                .WithOpenApi();

            // API health check endpoint
            app.MapGet("/api/health", GetDetailedHealth)
                .WithName("GetDetailedHealth")
                .WithTags("General")
                .WithSummary("Get detailed health information")
                .Produces<HealthResponse>()
                .WithOpenApi();
        }


        private static async Task InitializeDatabaseAsync(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

            try
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();

                logger.LogInformation("Applying database migrations...");
                await dbContext.Database.EnsureCreatedAsync();

                logger.LogInformation("Database initialization completed successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while initializing the database");
                throw; // Re-throw to prevent application startup with invalid database
            }
        }


        private static IResult GetApiInfo()
        {
            var apiInfo = new ApiInfoResponse
            {
                Title = "Library System API",
                Version = "1.0.0",
                Description = "A comprehensive library management system built with Clean Architecture, DDD, and CQRS patterns",
                Documentation = "/swagger",
                ApiSpecification = "/swagger/v1/swagger.json",
                HealthCheck = "/health",
                Architecture = new
                {
                    Pattern = "Clean Architecture",
                    Layers = new[] { "Presentation", "Application", "Domain", "Infrastructure" },
                    Patterns = new[] { "CQRS", "DDD", "Repository Pattern", "Unit of Work", "Result Pattern" },
                    Technologies = new[] { "ASP.NET Core", "Entity Framework Core", "MediatR", "FluentValidation", "AutoMapper", "Serilog" }
                },
                Endpoints = new
                {
                    Books = new[] { "/api/books", "/api/books/{id}", "/api/books/category/{category}", "/api/books/author/{author}" },
                    Members = new[] { "/api/members", "/api/members/{id}", "/api/members/authenticate" },
                    Borrowing = new[] { "/api/borrowing/borrow", "/api/borrowing/return", "/api/borrowing/member/{memberId}" }
                }
            };

            return Results.Ok(apiInfo);
        }

        private static IResult GetDetailedHealth()
        {
            var health = new HealthResponse
            {
                Status = "Healthy",
                Timestamp = DateTime.UtcNow,
                Version = "1.0.0",
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
                Database = "SQL Server",
                Framework = "Entity Framework Core",
                Uptime = Environment.TickCount64
            };

            return Results.Ok(health);
        }


    }

    public class ApiInfoResponse
    {
        public string Title { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Documentation { get; set; } = string.Empty;
        public string ApiSpecification { get; set; } = string.Empty;
        public string HealthCheck { get; set; } = string.Empty;
        public object Architecture { get; set; } = new();
        public object Endpoints { get; set; } = new();
    }

    public class HealthResponse
    {
        public string Status { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Version { get; set; } = string.Empty;
        public string Environment { get; set; } = string.Empty;
        public string Database { get; set; } = string.Empty;
        public string Framework { get; set; } = string.Empty;
        public long Uptime { get; set; }
    }

}
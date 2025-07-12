using LibSystem.Api.Common;
using LibSystem.Api.Endpoints;
using LibSystem.Api.Middleware;
using LibSystem.Api.Security; 
using LibSystem.Application;
using LibSystem.Identity;
using LibSystem.Identity.Context;
using LibSystem.Persistence;
using LibSystem.Persistence.Context;
using LibSystem.Utils.Extensions;
using LibSystem.Utils.Logging;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Security.Claims;

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

                // Initialize databases
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

            // Add HTTP Context Accessor (Required for CurrentUserService)
            builder.Services.AddHttpContextAccessor();

            // ADD SECURITY ENHANCEMENTS HERE
            builder.Services.AddSecurityEnhancements(builder.Configuration);

            // Register security services
            builder.Services.AddScoped<ICsrfTokenService, CsrfTokenService>();
            builder.Services.AddScoped<IAuditService, AuditService>();

            // Add Swagger with JWT support
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Library System API",
                    Version = "v1",
                    Description = "A comprehensive library management system API built with Clean Architecture, DDD, CQRS, and JWT authentication"
                });

                // Add JWT authentication to Swagger
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter 'Bearer' [space] and then your valid token in the text input below.\r\n\r\nExample: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9\""
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            // Enhanced CORS with security settings
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });

                options.AddPolicy("Production", policy =>
                {
                    policy.WithOrigins("https://yourdomain.com") // Replace with your actual domain
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials() // IMPORTANT for httpOnly cookies
                          .WithExposedHeaders("X-CSRF-TOKEN"); // Expose CSRF token header
                });

                options.AddPolicy("Development", policy =>
                {
                    policy.WithOrigins("http://localhost:3000", "https://localhost:3000") // Your frontend URLs
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials() // IMPORTANT for httpOnly cookies
                          .WithExposedHeaders("X-CSRF-TOKEN");
                });
            });

            // Add Health Checks 
            builder.Services.AddHealthChecks()
                .AddDbContextCheck<LibraryDbContext>("library-database")
                .AddDbContextCheck<IdentityDbContext>("identity-database");

            builder.Services.AddResponseCaching();
            builder.Services.AddMemoryCache();

            // Enhanced Rate Limiting is now handled in SecurityEnhancements
            // Remove the old rate limiting code

            // Register Application Layer services
            builder.Services.AddApplicationServices();

            // Register Infrastructure Layer services
            builder.Services.AddPersistenceServices(builder.Configuration);

            // Register Identity Layer services
            builder.Services.AddIdentityServices(builder.Configuration);

            // Register API-specific services
            builder.Services.AddScoped<LibSystem.Application.Contracts.Identity.ICurrentUserService, LibSystem.Api.Authorization.CurrentUserService>();
        }

        private static void ConfigureMiddleware(WebApplication app)
        {
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
                app.UseGlobalExceptionHandler(); // Only in production
            }

            // Use the middleware extension method
            app.UseSecurityEnhancements();

            // Global exception handling (should be early in pipeline)
            if (app.Environment.IsDevelopment())
            {
                // In development, let DeveloperExceptionPage handle exceptions
            }
            else
            {
                app.UseGlobalExceptionHandler();
            }

            // HTTPS redirection
            app.UseHttpsRedirection();

            // CORS - Use appropriate policy based on environment
            var corsPolicy = app.Environment.IsDevelopment() ? "Development" : "Production";
            app.UseCors(corsPolicy);

            // Response caching
            app.UseResponseCaching();

            // Rate limiting is now handled in SecurityEnhancements
            // app.UseRateLimiter(); // This is called in UseSecurityEnhancements()

            // Serilog request logging
            app.UseSerilogRequestLogging();

            // Authentication and Authorization (CRITICAL ORDER)
            app.UseAuthentication();
            app.UseUserContext();
            app.UseAuthorization();

            // Health checks
            app.UseHealthChecks("/health");
        }

        private static void ConfigureEndpoints(WebApplication app)
        {
            // Map all endpoint groups with proper authorization
            app.MapBookEndpoints();
            app.MapMemberEndpoints();
            app.MapBorrowingEndpoints();

            // Map authentication endpoints (public/anonymous)
            app.MapAuthenticationEndpoints();

            // MAP NEW SECURE AUTHENTICATION ENDPOINTS
            app.MapSecureAuthenticationEndpoints();

            // MAP PERMISSION VERIFICATION ENDPOINTS
            app.MapPermissionVerificationEndpoints();

            // Map user management endpoints (Admin only)
            app.MapUserManagementEndpoints();

            // REMOVE THIS LINE - MapDebugEndpoints doesn't exist
            // app.MapDebugEndpoints(); 

            // Root endpoint with API information
            app.MapGet("/", GetApiInfo)
                .WithName("GetApiInfo")
                .WithTags("General")
                .WithSummary("Get API information and available endpoints")
                .Produces<ApiInfoResponse>()
                .WithOpenApi()
                .AllowAnonymous();

            // API health check endpoint with detailed info
            app.MapGet("/api/health", GetDetailedHealth)
                .WithName("GetDetailedHealth")
                .WithTags("General")
                .WithSummary("Get detailed health information")
                .Produces<HealthResponse>()
                .WithOpenApi()
                .AllowAnonymous();

            // Test endpoint to verify JWT is working
            app.MapGet("/api/test/auth", (ClaimsPrincipal user) =>
            {
                if (user.Identity?.IsAuthenticated != true)
                {
                    return Results.Json(new { message = "Not authenticated" }, statusCode: 401);
                }

                return Results.Ok(new
                {
                    message = "Authentication successful!",
                    user = user.Identity.Name,
                    roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray(),
                    claims = user.Claims.Select(c => new { c.Type, c.Value }).ToArray()
                });
            })
            .WithName("TestAuth")
            .WithTags("Testing")
            .WithSummary("Test JWT authentication")
            .RequireAuthorization()
            .WithOpenApi();

            // CSRF Token endpoint
            app.MapGet("/api/csrf-token", (ICsrfTokenService csrfService) =>
            {
                var token = csrfService.GenerateToken();
                return Results.Ok(new { csrfToken = token });
            })
            .WithName("GetCsrfToken")
            .WithTags("Security")
            .WithSummary("Get CSRF token")
            .AllowAnonymous();
        }

        private static async Task InitializeDatabaseAsync(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

            try
            {
                // Initialize Library Database
                logger.LogInformation("Initializing Library database...");
                var libraryDbContext = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
                await libraryDbContext.Database.EnsureCreatedAsync();
                logger.LogInformation("Library database initialization completed successfully");

                // Initialize Identity Database using the extension method
                logger.LogInformation("Initializing Identity database...");
                await app.Services.InitializeIdentityDatabaseAsync();
                logger.LogInformation("Identity database initialization completed successfully");

                logger.LogInformation("All database initialization completed successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while initializing the databases");
                throw; // Re-throw to prevent application startup with invalid database
            }
        }

        private static IResult GetApiInfo()
        {
            var apiInfo = new ApiInfoResponse
            {
                Title = "Library System API",
                Version = "1.0.0",
                Description = "A comprehensive library management system built with Clean Architecture, DDD, CQRS patterns, and JWT authentication",
                Documentation = "/swagger",
                ApiSpecification = "/swagger/v1/swagger.json",
                HealthCheck = "/health",
                Architecture = new
                {
                    Pattern = "Clean Architecture",
                    Layers = new[] { "Presentation", "Application", "Domain", "Infrastructure", "Identity" },
                    Patterns = new[] { "CQRS", "DDD", "Repository Pattern", "Unit of Work", "Result Pattern", "JWT Authentication" },
                    Technologies = new[] { "ASP.NET Core", "Entity Framework Core", "MediatR", "FluentValidation", "AutoMapper", "Serilog", "ASP.NET Core Identity" }
                },
                Endpoints = new
                {
                    Books = new[] {
                        "GET /api/books - Get all books",
                        "GET /api/books/{id} - Get book by ID",
                        "POST /api/books - Create new book (Management+)",
                        "DELETE /api/books/{id} - Delete book (Management+)",
                        "GET /api/books/category/{category} - Get books by category",
                        "GET /api/books/author/{author} - Get books by author"
                    },
                    Members = new[] {
                        "GET /api/members - Get all members (Staff+)",
                        "GET /api/members/{id} - Get member by ID",
                        "POST /api/members - Create new member",
                        "POST /api/members/authenticate - Authenticate member"
                    },
                    Borrowing = new[] {
                        "POST /api/borrowing/borrow - Borrow a book",
                        "POST /api/borrowing/return - Return a book",
                        "GET /api/borrowing/member/{memberId} - Get member borrowing status"
                    },
                    Authentication = new[] {
                        "POST /api/auth/login - User login",
                        "POST /api/auth/register - User registration",
                        "GET /api/auth/me - Get current user info",
                        "POST /api/auth/login (Secure) - Secure login with httpOnly cookies",
                        "GET /api/auth/permissions - Get user permissions",
                        "GET /api/auth/verify-access - Verify resource access"
                    },
                    UserManagement = new[] {
                        "GET /api/users - Get all users (Admin only)",
                        "GET /api/users/{id} - Get user by ID (Admin only)"
                    },
                    Security = new[] {
                        "GET /api/csrf-token - Get CSRF token",
                        "POST /api/auth/logout - Secure logout",
                        "GET /api/auth/verify - Verify authentication status"
                    }
                },
                Security = new
                {
                    Authentication = "JWT Bearer Token + httpOnly Cookies",
                    Roles = new[] { "Member", "MinorStaff", "ManagementStaff", "Administrator" },
                    RateLimiting = new
                    {
                        Api = "100 requests per minute",
                        Auth = "5 requests per minute",
                        PerUser = "200 requests per minute"
                    },
                    SecurityFeatures = new[] {
                        "CSRF Protection",
                        "Security Headers",
                        "IP Filtering (configurable)",
                        "Request Size Limits",
                        "httpOnly Cookies",
                        "Audit Logging"
                    }
                }
            };

            return Results.Ok(new ApiResponse<ApiInfoResponse>
            {
                Success = true,
                Data = apiInfo,
                Message = "API information retrieved successfully"
            });
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
                Authentication = "JWT + ASP.NET Core Identity + httpOnly Cookies",
                Uptime = Environment.TickCount64,
                Features = new
                {
                    Caching = "In-Memory",
                    Logging = "Serilog",
                    RateLimiting = "Enhanced ASP.NET Core Rate Limiting",
                    ExceptionHandling = "Global Exception Middleware",
                    Validation = "FluentValidation",
                    Mapping = "AutoMapper",
                    HealthChecks = "ASP.NET Core Health Checks",
                    Security = "Enhanced Security Headers + CSRF + Audit Logging"
                }
            };

            return Results.Ok(new ApiResponse<HealthResponse>
            {
                Success = true,
                Data = health,
                Message = "Health check completed successfully"
            });
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
        public object Security { get; set; } = new();
    }

    public class HealthResponse
    {
        public string Status { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Version { get; set; } = string.Empty;
        public string Environment { get; set; } = string.Empty;
        public string Database { get; set; } = string.Empty;
        public string Framework { get; set; } = string.Empty;
        public string Authentication { get; set; } = string.Empty;
        public long Uptime { get; set; }
        public object Features { get; set; } = new();
    }
}
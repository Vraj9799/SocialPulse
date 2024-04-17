
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using SocialMediaApp.Models.Entities;
using SocialMediaApp.Models;
using SocialMediaApp.Repositories;
using SocialMediaApp.Services;
using System.Text.Json;
using System.Text;
using SocialMediaApp.Filters;
using SendGrid.Extensions.DependencyInjection;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using SocialMediaApp.Middlewares;
using AuthenticationMiddleware = SocialMediaApp.Middlewares.AuthenticationMiddleware;
using Serilog;

namespace SocialMediaApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Host.UseSerilog((context, configuration) => configuration.ReadFrom.Configuration(context.Configuration));
            // Add services to the container.

            Configure(builder.Services, builder.Configuration);
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();
            await SeedRoles(builder.Services);

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseSerilogRequestLogging();

            app.UseHttpsRedirection();

            app.UseCors();

            app.UseMiddleware<ExceptionMiddleware>();

            app.UseMiddleware<AuthenticationMiddleware>();

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }

        private static void Configure(IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<AppSettings>(configuration);
            var appSettings = configuration.Get<AppSettings>();
            services.AddSendGrid(opts => opts.ApiKey = appSettings.EmailSettings.ApiKey);
            services.Configure<ApiBehaviorOptions>(opts =>
            {
                opts.SuppressModelStateInvalidFilter = true;
            });
            services.AddControllers(opts =>
            {
                opts.AllowEmptyInputInBodyModelBinding = true;
                opts.Filters.Add(typeof(ModelValidationFilter));
                opts.Filters.Add(new AuthorizeFilter());
            })
                .AddJsonOptions(opts =>
                {
                    opts.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                    opts.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                    opts.JsonSerializerOptions.WriteIndented = true;
                });
            services.AddCors(opts =>
            {
                opts.AddDefaultPolicy(policy =>
                        policy.WithOrigins(appSettings.FrontendUrl)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials()
                );
            });
            services.AddSingleton<IMongoClient>(opts =>
            {
                return new MongoClient(appSettings.MongoDBSettings.ConnectionString);
            });
            services.AddAuthentication(opts =>
            {
                opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opts.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(opts =>
                {
                    opts.SaveToken = true;
                    opts.RequireHttpsMetadata = true;
                    opts.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidIssuer = appSettings?.Jwt?.Issuer,
                        ValidAudience = appSettings?.Jwt?.Audience,
                        ValidateAudience = true,
                        ValidateIssuer = true,
                        ClockSkew = TimeSpan.Zero,
                        IncludeTokenOnFailedValidation = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(appSettings.Jwt.SecretKey))
                    };
                    opts.Events = new JwtBearerEvents()
                    {
                        OnTokenValidated = (context) =>
                        {
                            Console.WriteLine(context.Principal.Claims.FirstOrDefault(_ => _.Type == ClaimTypes.NameIdentifier));
                            return Task.CompletedTask;
                        },
                        OnAuthenticationFailed = async (context) =>
                        {
                            if (context.Exception is SecurityTokenExpiredException securityTokenExpiredException)
                            {
                                if (context.Request.Cookies.TryGetValue(Constants.ACCESS_TOKEN, out string accessToken) &&
                                context.Request.Cookies.TryGetValue(Constants.REFRESH_TOKEN, out string refreshToken)
                            )
                            {
                                try
                                {
                                    var serviceProvider = services.BuildServiceProvider();
                                    var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                                    var tokenService = serviceProvider.GetRequiredService<ITokenService>();
                                    var principal = tokenService.GetClaimsFromToken(accessToken);
                                    var userId = principal.Claims.First(_ => _.Type == ClaimTypes.NameIdentifier)?.Value;
                                    var appUser = await userManager.FindByIdAsync(userId);
                                    var roles = await userManager.GetRolesAsync(appUser);
                                    (accessToken, refreshToken) = tokenService.GenerateAuthToken(appUser, roles);
                                    await tokenService.DeleteRefreshToken(refreshToken, userId);
                                    context.Response.Cookies.Append(Constants.ACCESS_TOKEN, accessToken, new CookieOptions
                                    {
                                        Secure = true,
                                        HttpOnly = true,
                                        IsEssential = true,
                                    });
                                    context.HttpContext.Response.Cookies.Append(Constants.REFRESH_TOKEN, refreshToken, new CookieOptions
                                    {
                                        Secure = true,
                                        HttpOnly = true,
                                        IsEssential = true,
                                    });
                                    context.Principal = principal;
                                    context.Success();
                                }
                                catch (Exception ex)
                                {
                                    context.Response.Cookies.Delete(Constants.ACCESS_TOKEN);
                                    context.Response.Cookies.Delete(Constants.REFRESH_TOKEN);
                                    context.Fail("Login is required.");
                                }
                            }
                            }
                            else
                            {
                                context.Response.Cookies.Delete(Constants.ACCESS_TOKEN);
                                context.Response.Cookies.Delete(Constants.REFRESH_TOKEN);
                                context.Fail("Login is required.");
                            }
                        }
                    };
                });
            services.AddDbContext<ApplicationDbContext>(opts =>
            {
                opts.UseLazyLoadingProxies();
                opts.UseNpgsql(configuration.GetConnectionString("Default"));

                if (appSettings.IsDevelopment)
                {
                    opts.EnableDetailedErrors();
                    opts.EnableSensitiveDataLogging();
                    opts.EnableThreadSafetyChecks();
                }
            });
            services.AddIdentity<ApplicationUser, ApplicationRole>(opts =>
            {
                opts.User.RequireUniqueEmail = true;
                opts.Password.RequireDigit = true;
                opts.Password.RequiredLength = 8;
                opts.Password.RequireNonAlphanumeric = true;
                opts.Password.RequireLowercase = true;
                opts.Password.RequireUppercase = true;
                opts.SignIn.RequireConfirmedEmail = false;
                opts.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                opts.Lockout.MaxFailedAccessAttempts = 3;
            })
                .AddDefaultTokenProviders()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddMemoryCache(opts =>
            {
                opts.ExpirationScanFrequency = TimeSpan.FromMinutes(1);
                opts.TrackStatistics = true;
            });

            services.AddHttpContextAccessor();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<ITokenRepository, TokenRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IPostRepository, PostRepository>();
            services.AddScoped<IPostService, PostService>();
            services.AddScoped<ICommentRepository, CommentRepository>();
            services.AddScoped<ICommentService, CommentService>();
        }

        private static async Task SeedRoles(IServiceCollection services)
        {
            using (var scope = services.BuildServiceProvider())
            {
                var context = scope.GetService<ApplicationDbContext>();
                if (context.Database.GetPendingMigrations().Any()) 
                    context.Database.Migrate();
            }
            using var roleManager = services.BuildServiceProvider()
            .GetService<RoleManager<ApplicationRole>>();
            var role = await roleManager.FindByNameAsync("BASIC");
            if (role is null)
                await roleManager.CreateAsync(new ApplicationRole() { Name = "BASIC" });
        }

    }
}

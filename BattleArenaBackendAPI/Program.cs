using System.Text;
using BattleArenaBackendAPI.Data;
using BattleArenaBackendAPI.Hubs;
using BattleArenaBackendAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;

namespace BattleArenaBackendAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ---- Services ----------------------------------------------------

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            // Swagger with a JWT bearer security definition so the "Authorize"
            // button appears in the Swagger UI.
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "BattleArena Backend API",
                    Version = "v1"
                });

                var scheme = new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter the JWT token (without the 'Bearer ' prefix).",
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                };

                options.AddSecurityDefinition("Bearer", scheme);
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    { scheme, Array.Empty<string>() }
                });
            });

            // PostgreSQL / EF Core
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Redis (shared multiplexer)
            var redisConnection = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
            builder.Services.AddSingleton<IConnectionMultiplexer>(
                ConnectionMultiplexer.Connect(redisConnection));

            // SignalR
            builder.Services.AddSignalR();

            // Application services
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IShopService, ShopService>();
            builder.Services.AddScoped<ILeaderboardService, LeaderboardService>();

            // JWT authentication
            var jwtSection = builder.Configuration.GetSection("Jwt");
            var jwtSecret = jwtSection["Secret"]
                ?? throw new InvalidOperationException("Jwt:Secret is not configured.");

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSection["Issuer"],
                    ValidAudience = jwtSection["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                    ClockSkew = TimeSpan.Zero
                };

                // Allow SignalR clients to pass the JWT via the access_token query
                // string, since browsers can't set Authorization headers on WebSockets.
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) &&
                            path.StartsWithSegments("/hub"))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            builder.Services.AddAuthorization();

            var app = builder.Build();

            // ---- Middleware pipeline ----------------------------------------

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            app.MapHub<GlobalChatHub>("/hub/global-chat");

            app.Run();
        }
    }
}

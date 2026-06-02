using FinanceiroApi.CrossCutting.Notifications;
using FinanceiroApi.CrossCutting.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Text;

namespace FinanceiroApi.CrossCutting.IoC;

public static class DependencyInjection
{
    public static IServiceCollection AddCrossCutting(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<INotificationContext, NotificationContext>();
        services.AddScoped<NotificationContext>(sp =>
            (NotificationContext)sp.GetRequiredService<INotificationContext>());

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.AddScoped<TokenService>();
        services.AddJwtAuthentication(configuration);

        return services;
    }

    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
            ?? throw new InvalidOperationException("JWT settings not configured.");

        var key = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtSettings.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    NameClaimType = JwtRegisteredClaimNames.Sub,
                    RoleClaimType = "role"
                };
            });

        services.AddAuthorizationBuilder()
            .AddPolicy("RequireAdmin", p => p.RequireRole("Admin"))
            .AddPolicy("RequireManager", p => p.RequireRole("Admin", "Manager"))
            .AddPolicy("RequireEmployee", p => p.RequireRole("Admin", "Manager", "Employee"));

        return services;
    }
}

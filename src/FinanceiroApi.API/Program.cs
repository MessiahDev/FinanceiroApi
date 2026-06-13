using FinanceiroApi.API.Extensions;
using FinanceiroApi.API.Middlewares;
using FinanceiroApi.Application;
using FinanceiroApi.CrossCutting.IoC;
using FinanceiroApi.CrossCutting.Logging;
using FinanceiroApi.Infrastructure;
using FinanceiroApi.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using Serilog;

JsonWebTokenHandler.DefaultInboundClaimTypeMap.Clear();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog(SerilogConfiguration.Configure);

builder.Services.AddApiServices(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddCrossCutting(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<JwtSecuritySchemeTransformer>();
    options.AddDocumentTransformer<OpenApiServerTransformer>();
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.UseGlobalExceptionHandler();
app.UseCorrelationId();
app.UseCors("DefaultCors");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapOpenApi();

app.MapScalarApiReference(options =>
{
    options.Title = "FinanceiroApi V1";
    options.Theme = ScalarTheme.DeepSpace;
});

app.MapControllers();
app.MapHealthChecks("/health");
app.MapGet("/", () => Results.Redirect("/scalar/v1"));

app.Run();

internal sealed class JwtSecuritySchemeTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, OpenApiSecurityScheme>();

        document.Components.SecuritySchemes[JwtBearerDefaults.AuthenticationScheme] =
            new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Insira o token JWT."
            };

        return Task.CompletedTask;
    }
}

internal sealed class OpenApiServerTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        document.Servers.Clear();

        document.Servers.Add(new OpenApiServer
        {
            Url = "http://localhost:8080"
        });

        return Task.CompletedTask;
    }
}

public partial class Program { }
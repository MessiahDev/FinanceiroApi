using FinanceiroApi.API.Extensions;
using FinanceiroApi.Application;
using FinanceiroApi.CrossCutting.IoC;
using FinanceiroApi.CrossCutting.Logging;
using FinanceiroApi.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Scalar.AspNetCore;
using FinanceiroApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using FinanceiroApi.API.Middlewares;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog(SerilogConfiguration.Configure);
builder.Services.AddApiServices(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddCrossCutting(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Components ??= new Microsoft.OpenApi.OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, Microsoft.OpenApi.IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes[JwtBearerDefaults.AuthenticationScheme] =
            new Microsoft.OpenApi.OpenApiSecurityScheme
            {
                Type = Microsoft.OpenApi.SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Insira o token JWT no campo abaixo."
            };
        return Task.CompletedTask;
    });
});
var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}
app.UseGlobalExceptionHandler();
app.UseCors("DefaultCors");
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
app.Run();

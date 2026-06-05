using System.Net;
using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using FinanceiroApi.Domain.Exceptions;

namespace FinanceiroApi.API.Middlewares;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Falha de validação: {Errors}", ex.Errors.Select(e => e.ErrorMessage));
            await WriteResponse(context, HttpStatusCode.BadRequest, "Validation Error", ex.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()));
        }
        catch (UnauthorizedAccessException)
        {
            await WriteResponse(context, HttpStatusCode.Forbidden, "Forbidden", null);
        }
        catch (KeyNotFoundException ex)
        {
            await WriteResponse(context, HttpStatusCode.NotFound, ex.Message, null);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Operação inválida");
            await WriteResponse(context, HttpStatusCode.UnprocessableEntity, ex.Message, null);
        }
        catch (DomainException ex)
        {
            _logger.LogWarning("Erro de domínio: {Message}", ex.Message);
            await WriteResponse(context, HttpStatusCode.BadRequest, ex.Message, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado");
            await WriteResponse(context, HttpStatusCode.InternalServerError, "Ocorreu um erro interno. Tente novamente.", null);
        }
    }

    private static async Task WriteResponse(HttpContext context, HttpStatusCode statusCode, string title, object? errors)
    {
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)statusCode;

        var problem = new ProblemDetails
        {
            Status = (int)statusCode,
            Title = title,
            Instance = context.Request.Path
        };

        if (errors is not null)
            problem.Extensions["errors"] = errors;

        var json = JsonSerializer.Serialize(problem, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        await context.Response.WriteAsync(json);
    }
}

public static class GlobalExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
        => app.UseMiddleware<GlobalExceptionMiddleware>();
}

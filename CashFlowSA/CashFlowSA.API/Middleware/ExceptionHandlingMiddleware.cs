using System.Net;
using System.Text.Json;
using CashFlowSA.Application.Common.Exceptions;
using FluentValidation;

namespace CashFlowSA.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(
        HttpContext context,
        Exception exception)
    {
        var (statusCode, message) = exception switch
        {
            ValidationException => (HttpStatusCode.BadRequest, "Validation failed."),
            NotFoundException => (HttpStatusCode.NotFound, exception.Message),
            ConflictException => (HttpStatusCode.Conflict, exception.Message),
            ForbiddenException => (HttpStatusCode.Forbidden, exception.Message),
            AuthenticationFailedException => (HttpStatusCode.Unauthorized, exception.Message),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred.")
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        ApiErrorResponse response;

        if (exception is ValidationException validationException)
        {
            var errors = validationException.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray());

            response = new ApiErrorResponse(
                Success: false,
                Message: message,
                Status: (int)statusCode,
                Errors: errors);
        }
        else
        {
            response = new ApiErrorResponse(
                Success: false,
                Message: message,
                Status: (int)statusCode);
        }

        return context.Response.WriteAsync(
            JsonSerializer.Serialize(response));
    }
}

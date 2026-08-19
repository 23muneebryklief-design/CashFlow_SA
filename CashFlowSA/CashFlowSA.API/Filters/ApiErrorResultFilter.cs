using CashFlowSA.API.Middleware;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CashFlowSA.API.Filters;

/// <summary>
/// Normalizes controller-generated 4xx/5xx responses into the same error envelope
/// used by the global exception middleware.
/// </summary>
public sealed class ApiErrorResultFilter : IAsyncResultFilter
{
    public async Task OnResultExecutionAsync(
        ResultExecutingContext context,
        ResultExecutionDelegate next)
    {
        var statusCode = context.Result switch
        {
            ObjectResult objectResult when objectResult.StatusCode.HasValue
                => objectResult.StatusCode.Value,
            StatusCodeResult statusCodeResult
                => statusCodeResult.StatusCode,
            ChallengeResult => StatusCodes.Status401Unauthorized,
            ForbidResult => StatusCodes.Status403Forbidden,
            _ => context.HttpContext.Response.StatusCode
        };

        if (statusCode >= 400)
        {
            object? detail = context.Result switch
            {
                ObjectResult objectResult => objectResult.Value,
                _ => null
            };

            var message = detail switch
            {
                string text when !string.IsNullOrWhiteSpace(text) => text,
                ProblemDetails problemDetails when !string.IsNullOrWhiteSpace(problemDetails.Detail)
                    => problemDetails.Detail!,
                ProblemDetails problemDetails when !string.IsNullOrWhiteSpace(problemDetails.Title)
                    => problemDetails.Title!,
                _ => statusCode switch
                {
                    400 => "The request could not be processed.",
                    401 => "Authentication is required.",
                    403 => "You do not have permission to perform this action.",
                    404 => "The requested resource was not found.",
                    409 => "The request conflicts with the current resource state.",
                    _ => "An unexpected error occurred."
                }
            };

            object? errors = detail is ProblemDetails pd ? pd.Extensions : null;

            context.Result = new ObjectResult(
                new ApiErrorResponse(false, message, statusCode, errors))
            {
                StatusCode = statusCode
            };
        }

        await next();
    }
}

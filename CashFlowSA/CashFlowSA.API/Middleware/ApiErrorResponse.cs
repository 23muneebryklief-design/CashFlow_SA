namespace CashFlowSA.API.Middleware;

public sealed record ApiErrorResponse(
    bool Success,
    string Message,
    int Status,
    object? Errors = null);

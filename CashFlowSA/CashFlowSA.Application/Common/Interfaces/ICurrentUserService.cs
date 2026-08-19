namespace CashFlowSA.Application.Common.Interfaces
{
    /// <summary>
    /// Provides the authenticated application user and request information to infrastructure services.
    /// </summary>
    public interface ICurrentUserService
    {
        Guid? UserId { get; }
        string? IpAddress { get; }
        bool IsAuthenticated { get; }
    }
}

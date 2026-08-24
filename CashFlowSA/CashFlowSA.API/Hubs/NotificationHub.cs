using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CashFlowSA.API.Hubs;

[Authorize]
public sealed class NotificationHub : Hub
{
    private const string GroupPrefix = "notification-user:";

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        if (!userId.HasValue)
        {
            Context.Abort();
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, GetGroupName(userId.Value));
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        if (userId.HasValue)
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetGroupName(userId.Value));

        await base.OnDisconnectedAsync(exception);
    }

    public static string GetGroupName(Guid userId) => $"{GroupPrefix}{userId:N}";

    private Guid? GetUserId()
    {
        var raw = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? Context.User?.FindFirstValue(JwtClaimNames.Sub)
                  ?? Context.User?.FindFirstValue("userId");

        return Guid.TryParse(raw, out var userId) ? userId : null;
    }

    private static class JwtClaimNames
    {
        public const string Sub = "sub";
    }
}

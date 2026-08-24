using System.Security.Claims;
using CashFlowSA.Application.Features.Notification.GetNotificationHistory;
using CashFlowSA.Application.Features.Notification.MarkNotificationRead;
using CashFlowSA.Application.Common.Notifications;
using CashFlowSA.Domain.Models.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CashFlowSA.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly INotificationDispatcher _dispatcher;

        public NotificationController(
            IMediator mediator,
            INotificationDispatcher dispatcher)
        {
            _mediator = mediator;
            _dispatcher = dispatcher;
        }

        [HttpGet("{userId:guid}")]
        public async Task<IActionResult> GetNotificationHistory(
            Guid userId,
            CancellationToken cancellationToken)
        {
            var authenticatedUserId = GetAuthenticatedUserId();
            if (!authenticatedUserId.HasValue)
                return Unauthorized(new
                {
                    success = false,
                    message = "Authenticated user ID is missing or invalid."
                });

            // Notifications are private user resources. Never allow an
            // authenticated user to request another user's notification history.
            if (authenticatedUserId.Value != userId)
                return Forbid();

            var result = await _mediator.Send(
                new GetNotificationHistoryQuery { UserId = authenticatedUserId.Value },
                cancellationToken);

            return Ok(result);
        }

        [HttpPut("{notificationId:guid}/read")]
        public async Task<IActionResult> MarkAsRead(
            Guid notificationId,
            CancellationToken cancellationToken)
        {
            var userId = GetAuthenticatedUserId();
            if (!userId.HasValue)
                return Unauthorized(new
                {
                    success = false,
                    message = "Authenticated user ID is missing or invalid."
                });

            var marked = await _mediator.Send(
                new MarkNotificationReadCommand
                {
                    NotificationId = notificationId,
                    UserId = userId.Value
                },
                cancellationToken);

            if (!marked)
                return NotFound(new
                {
                    success = false,
                    message = "Notification not found."
                });

            return Ok(new
            {
                success = true,
                message = "Notification marked as read."
            });
        }


        [HttpPost("test")]
        public async Task<IActionResult> SendTestNotification(
            [FromBody] TestNotificationRequest request,
            CancellationToken cancellationToken)
        {
            var userId = GetAuthenticatedUserId();
            if (!userId.HasValue)
                return Unauthorized();

            var notificationId = await _dispatcher.DispatchAsync(
                userId.Value,
                NotificationEvent.SystemAnnouncement,
                string.IsNullOrWhiteSpace(request.Title) ? "CashFlowSA notification test" : request.Title,
                string.IsNullOrWhiteSpace(request.Message) ? "Real-time notification delivery is working." : request.Message,
                new[] { NotificationChannel.InApp },
                cancellationToken);

            return Ok(new { notificationId });
        }
        private Guid? GetAuthenticatedUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                              ?? User.FindFirstValue("sub")
                              ?? User.FindFirstValue("userId");

            return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
        }
        public sealed class TestNotificationRequest
        {
            public string Title { get; set; } = string.Empty;
            public string Message { get; set; } = string.Empty;
        }
    }
}

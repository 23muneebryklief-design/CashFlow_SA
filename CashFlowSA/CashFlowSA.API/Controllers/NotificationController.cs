using System.Security.Claims;
using CashFlowSA.Application.Features.Notification.GetNotificationHistory;
using CashFlowSA.Application.Features.Notification.MarkNotificationRead;
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

        public NotificationController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetNotificationHistory(Guid userId, CancellationToken cancellationToken)
        {
            var query = new GetNotificationHistoryQuery { UserId = userId };
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        [HttpPut("{notificationId:guid}/read")]
        public async Task<IActionResult> MarkAsRead(Guid notificationId, CancellationToken cancellationToken)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                              ?? User.FindFirstValue("sub")
                              ?? User.FindFirstValue("userId");

            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized(new { success = false, message = "Authenticated user ID is missing or invalid." });

            var marked = await _mediator.Send(
                new MarkNotificationReadCommand
                {
                    NotificationId = notificationId,
                    UserId = userId
                },
                cancellationToken);

            if (!marked)
                return NotFound(new { success = false, message = "Notification not found." });

            return Ok(new
            {
                success = true,
                message = "Notification marked as read."
            });
        }
    }
}

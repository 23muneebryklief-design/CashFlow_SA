using MediatR;
using Microsoft.AspNetCore.Mvc;
using CashFlowSA.Application.Features.Notification.GetNotificationHistory;
using Microsoft.AspNetCore.Authorization;

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
    }
}

using CashFlowSA.Application.Features.Admin.ReinstateUser;
using CashFlowSA.Application.Features.Admin.SuspendUser;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CashFlowSA.API.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "Admin,SuperAdmin")]
public sealed class UserManagementController : ControllerBase
{
    private readonly IMediator _mediator;

    public UserManagementController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("{userId:guid}/suspend")]
    public async Task<IActionResult> Suspend(
        Guid userId,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new SuspendUserCommand { UserId = userId }, cancellationToken);
        return Ok(new { message = "User account suspended successfully.", userId });
    }

    [HttpPost("{userId:guid}/reinstate")]
    public async Task<IActionResult> Reinstate(
        Guid userId,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new ReinstateUserCommand { UserId = userId }, cancellationToken);
        return Ok(new { message = "User account reinstated successfully.", userId });
    }
}

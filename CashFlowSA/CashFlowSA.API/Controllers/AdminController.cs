using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CashFlowSA.Application.Features.Admin.RegisterAdmin;

namespace CashFlowSA.API.Controllers
{
    // Account management for the admin portal itself. Only a SuperAdmin
    // (the seeded account, see AdminSeeder) may create new Admin accounts --
    // regular Admins cannot create more admins.
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "SuperAdmin")]
    public class AdminController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AdminController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("create-admin")]
        public async Task<IActionResult> CreateAdmin(
            [FromBody] RegisterAdminCommand command,
            CancellationToken cancellationToken)
        {
            var adminId = await _mediator.Send(command, cancellationToken);
            return Ok(new { AdminId = adminId });
        }
    }
}

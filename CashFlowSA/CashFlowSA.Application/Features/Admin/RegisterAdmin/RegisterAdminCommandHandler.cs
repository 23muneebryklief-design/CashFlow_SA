using CashFlowSA.Application.Common.Interfaces;
using CashFlowSA.Application.Common.Exceptions;
using CashFlowSA.Domain.Models;
using CashFlowSA.Domain.Models.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CashFlowSA.Application.Features.Admin.RegisterAdmin
{
    public class RegisterAdminCommandHandler : IRequestHandler<RegisterAdminCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher;

        public RegisterAdminCommandHandler(IApplicationDbContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<User>();
        }

        public async Task<Guid> Handle(RegisterAdminCommand request, CancellationToken cancellationToken)
        {
            var emailInUse = await _context.Users
                .AnyAsync(u => u.Email == request.Email, cancellationToken);
            if (emailInUse)
                throw new ConflictException("A user with this email already exists.");

            // Only ever creates Admin/CreditAnalyst/Auditor accounts -- SuperAdmin is
            // reserved for the single seeded account (see AdminSeeder) and is never
            // assignable through this endpoint. The validator already restricts
            // request.Role to the three allowed values, so Enum.Parse is safe here.
            var admin = new User
            {
                UserId = Guid.NewGuid(),
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Role = Enum.Parse<UsersRoles>(request.Role),
                Status = AccountStatus.Active
            };
            admin.PasswordHash = _passwordHasher.HashPassword(admin, request.Password);

            _context.Users.Add(admin);
            await _context.SaveChangesAsync(cancellationToken);

            return admin.UserId;
        }
    }
}

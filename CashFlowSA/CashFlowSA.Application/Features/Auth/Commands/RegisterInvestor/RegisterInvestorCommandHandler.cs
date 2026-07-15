using CashFlowSA.Application.Common.Interfaces;
using CashFlowSA.Domain.Models;
using CashFlowSA.Domain.Models.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CashFlowSA.Application.Features.Auth.Commands.RegisterInvestor
{
    public class RegisterInvestorCommandHandler : IRequestHandler<RegisterInvestorCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher;

        public RegisterInvestorCommandHandler(IApplicationDbContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<User>();
        }

        public async Task<Guid> Handle(RegisterInvestorCommand request, CancellationToken cancellationToken)
        {
            //Uniqueness Checks 
            var emailInUse = await _context.Users
                .AnyAsync(u => u.Email == request.Email, cancellationToken);
            if (emailInUse)
                throw new InvalidOperationException("A user with this email already exists.");

            //Create the user first -Investor depend in its user id 
            var user = new User
            {
                UserId = Guid.NewGuid(),
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Role = UsersRoles.Investor,
                Status = AccountStatus.PendingVerification
            };
            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

            _context.Users.Add(user);

            //reate the Investor profile Linked to the new user
            var investor = new Investor
            {
                InvestorId = Guid.NewGuid(),
                UserId = user.UserId,
                Address = request.Address,
                RiskAppetite = request.RiskAppetite
            };

            _context.Investors.Add(investor);
            await _context.SaveChangesAsync(cancellationToken);
            return investor.InvestorId;
        }

    }
}
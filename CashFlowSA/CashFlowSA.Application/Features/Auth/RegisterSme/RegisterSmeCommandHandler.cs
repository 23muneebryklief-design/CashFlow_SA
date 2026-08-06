using CashFlowSA.Application.Common.Interfaces;
using CashFlowSA.Application.Common.Exceptions;
using CashFlowSA.Domain.Models;
using CashFlowSA.Domain.Models.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CashFlowSA.Application.Features.Auth.RegisterSme
{
    public class RegisterSmeCommandHandler : IRequestHandler<RegisterSmeCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher;

        public RegisterSmeCommandHandler(IApplicationDbContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<User>();
        }

        public async Task<Guid> Handle(RegisterSmeCommand request, CancellationToken cancellationToken)
        {
            // Uniqueness checks — mirror the unique indexes already enforced at the DB level,
            // but checking here first gives a clean error instead of an ugly SQL exception.
            var emailInUse = await _context.Users
                .AnyAsync(u => u.Email == request.Email, cancellationToken);
            if (emailInUse)
                throw new ConflictException("A user with this email already exists.");

            var companyEmailInUse = await _context.SMEs
                .AnyAsync(s => s.CompanyEmail == request.CompanyEmail, cancellationToken);
            if (companyEmailInUse)
                throw new ConflictException("A company with this email is already registered.");

            var registrationNumberInUse = await _context.SMEs
                .AnyAsync(s => s.RegistrationNumber == request.RegistrationNumber, cancellationToken);
            if (registrationNumberInUse)
                throw new ConflictException("A company with this registration number is already registered.");

            // Create the User first — SME depends on its UserId
            var user = new User
            {
                UserId = Guid.NewGuid(),
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Role = UsersRoles.SME,
                Status = AccountStatus.PendingVerification
            };
            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

            _context.Users.Add(user);

            // Create the SME profile, linked to the new User
            var sme = new SME
            {
                SMEId = Guid.NewGuid(),
                UserId = user.UserId,
                CompanyName = request.CompanyName,
                ContactPerson = request.ContactPerson,
                CompanyEmail = request.CompanyEmail,
                CompanyPhoneNumber = request.CompanyPhoneNumber,
                RegistrationNumber = request.RegistrationNumber,
                TaxNumber = request.TaxNumber,
                Address = request.Address,
                Industry = request.Industry
            };

            _context.SMEs.Add(sme);

            // Every SME gets exactly one wallet, created here rather than lazily --
            // GetWalletBalanceQuery expects one to already exist and 404s otherwise.
            _context.Wallets.Add(new Domain.Models.Wallet
            {
                WalletId = Guid.NewGuid(),
                UserId = user.UserId,
                Balance = 0,
                Currency = "ZAR"
            });

            await _context.SaveChangesAsync(cancellationToken);

            return sme.SMEId;
        }
    }
}
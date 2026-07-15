# CashFlow SA (Imali Bridge)

An AI-assisted invoice financing marketplace for South African SMEs. SMEs upload unpaid invoices to raise short-term funding from investors, who bid on funding campaigns priced by an automated risk score. Built as a portfolio project for redAcademy's software development bootcamp, designed to reflect production-realistic patterns rather than a toy CRUD app.

**Stack:** ASP.NET Core (.NET 10) · Entity Framework Core · SQL Server · MediatR (CQRS) · FluentValidation · AutoMapper · JWT Authentication · React/TypeScript (frontend, separate repo path) · PostgreSQL-compatible design decisions carried from planning docs, currently running on SQL Server for local dev

---

## 1. Why this architecture

The system serves **two portals** (a Business Portal for SMEs/Investors, and an Ops Portal for Admin/Credit Analyst/Auditor roles) sharing **one backend**. That requirement — multiple frontends, one source of truth, several distinct user roles each with different permissions and data visibility — is exactly the case Clean Architecture is built for: business rules shouldn't care which portal is calling them, and the database shouldn't leak into business logic.

The solution is split into four projects, and the dependency direction only ever points inward:

```
CashFlowSA.API            → knows about HTTP, controllers, JWT middleware
    ↓ depends on
CashFlowSA.Infrastructure → knows about EF Core, SQL Server, the real DbContext
    ↓ depends on
CashFlowSA.Application    → knows about business logic (CQRS commands/queries), 
                             but NOT how data is stored or how requests arrive
    ↓ depends on
CashFlowSA.Domain         → knows about nothing except the business itself
                             (entities, enums) — zero external dependencies
```

**Why it matters in practice:** `Application` never references `Infrastructure` directly. Instead, `Application` defines *interfaces* describing what it needs (`IApplicationDbContext`, `ITokenService`), and `Infrastructure` provides the real implementations. This means the core business logic could theoretically be tested, or even ported to a different database or a different token strategy, without touching a single line of `Application` code. It's also what makes the project readable to someone auditing it — you can open `Application` and see exactly what the system does, without wading through EF Core plumbing.

---

## 2. Domain layer — the business, modeled in code

`CashFlowSA.Domain` contains every entity and enum, organized into folders matching the system's actual business areas (not generic "Models" bucket):

- **UserAccess** — `User`, `UserSession` (login/JWT refresh tracking)
- **SmeManagement** — `SME` (company profile)
- **InvestorManagement** — `Investor`, `InvestorPortfolio`
- **KYCCompliance** — `KYCApplication`, `KYCDocument`, `KYCReview`
- **Invoice Management** — `Invoice`, `InvoiceDocument`, `OCRResult`
- **Risk Assessment** — `RiskAssessment`, `RiskScoreHistory`, `AIExplanation`
- **Funding Marketplace** — `FundingRequest`, `FundingCampaign`, `MarketplaceListing`, `UnderwritingReview`
- **Investment Management** — `AuctionBid`, `Investment`
- **Wallet & Financial Simulation** — `Wallet`, `WalletTransaction`, `Settlement`, `ReturnDistribution`
- **Document Management** — `Document`, `Notification`, `NotificationHistory`
- **Audit & Governance** — `AuditLog`, `GeneratedReport`
- **Analytics & Reporting** — reporting-specific entities

Every entity inherits from a shared `BaseEntity`, which provides `CreatedAt`, `UpdatedAt`, `CreatedByUserId`, `UpdatedByUserId` — a consistent audit trail across the entire schema, which matters given this system handles money and needs to support the compliance/audit requirements in the SRS.

**A design habit worth calling out:** entities carry inline comments citing the specific SRS requirement they satisfy (e.g. why `FundingCampaign` has a `RowVersion` concurrency token — it exists specifically to prevent an over-funding race condition described in SRS §5.5, where two investors could theoretically fund past 100% of a campaign's target if their bids landed at the exact same time without optimistic concurrency control).

---

## 3. Infrastructure layer — EF Core, configured per-entity

`CashFlowDbContext` exposes ~29 `DbSet`s, and every entity has its own `IEntityTypeConfiguration<T>` class (grouped into 9 configuration files by business area, e.g. `InvoiceConfiguration`, `WalletConfiguration`, `RiskAndGovernanceConfiguration`) rather than one giant `OnModelCreating` method. Deliberate choices baked into the configuration:

- **`decimal(18,2)`** on every money field, **`decimal(5,2)`** on scores/rates — enough precision for ZAR currency without floating-point rounding risk.
- **`DeleteBehavior.Restrict`** on every foreign key, no cascading deletes — in a financial audit-trail system, silently cascading a delete through invoices → funding campaigns → investments → wallet transactions would be catastrophic. Deletes must be deliberate and explicit at the application level.
- **Unique indexes** on natural keys: `Users.Email`, `Invoices.InvoiceNumber`, `SMEs.CompanyEmail`/`RegistrationNumber`.
- **A `RowVersion` concurrency token** on `FundingCampaign` specifically, anticipating the over-funding race condition noted above.

### Bugs found and fixed during review

Two real issues surfaced from a full read-through against the SRS, both fixed before building on top of them:

1. **Nullable "hasn't happened yet" timestamps had database defaults of "now."** Fields like `UserSession.LogoutTimestamp`, `KYCApplication.ReviewedAt`, and `UpdatedAt` on `User`/`Invoice`/`Investor`/`Wallet` are `DateTime?` specifically because `null` means "this hasn't happened yet" — but the EF configuration gave them `HasDefaultValueSql("GETUTCDATE()")`, meaning every new row would look like it had already logged out / already been reviewed / already been updated, the instant it was created. Fixed by removing the default from these six fields, leaving `CreatedAt`-style fields (which genuinely should default to "now") untouched.

2. **Eight entities redeclared properties already inherited from `BaseEntity`.** `Invoice`, `InvestorPortfolio`, `Investor`, `Notification`, `WalletTransaction`, `Wallet`, `User`, and `FundingCampaign` each had their own `CreatedAt`/`UpdatedAt` declarations that shadowed (rather than overrode) the base class's — a `CS0108` compiler warning. EF Core happened to resolve this correctly (one column per table), but it was a landmine: any code casting to `BaseEntity` would silently read/write a different property slot. Fixed by removing the redundant declarations, letting all entities inherit cleanly.

### Package hygiene

- **AutoMapper** was upgraded from 12.0.1 → 13.0.1 → **16.1.1** over the course of the build, chasing a real, actively-disclosed denial-of-service CVE (`GHSA-rvv3-g6hj-g44x` / CVE-2026-32933 — uncontrolled recursion on self-referential object graphs, unpatched below 15.1.3).
- **`AutoMapper.Extensions.Microsoft.DependencyInjection`** was removed — deprecated as of AutoMapper 13.0, since `AddAutoMapper()` is now built into the core package.

---

## 4. Database — migrated and verified

- `InitialCreate` migration: all ~29 tables, foreign keys, and indexes generated from the model and applied to a local SQL Server LocalDB instance (`CashFlowSA`).
- `FixInvestorPortfolioUpdatedAt`: a follow-up migration correcting `InvestorPortfolio.UpdatedAt` from non-nullable to nullable, once it started correctly inheriting from `BaseEntity` (see bug #2 above).
- Both migrations were reviewed diff-by-diff before being applied, rather than trusted blindly — the philosophy throughout this build has been: generate, inspect, then apply.

---

## 5. Authentication — custom JWT, not ASP.NET Identity

The `User` entity already modeled `PasswordHash` as a plain string and `UserSession` already had `RefreshToken`/`RefreshTokenExpiry` fields — the schema was designed for a **custom JWT + refresh token flow**, not the heavier ASP.NET Core Identity system (which expects its own table shape). The implementation follows that:

- **`Microsoft.Extensions.Identity.Core`** provides `PasswordHasher<T>` — salted, adaptive PBKDF2 hashing — without pulling in the full Identity/EF store machinery.
- **`ITokenService`** (interface in `Application`) / **`JwtTokenService`** (implementation in `Infrastructure`) — same inversion-of-control pattern as the DbContext. `GenerateAccessToken` builds a short-lived signed JWT carrying `Sub` (UserId), `Email`, and `Role` claims; `GenerateRefreshToken` produces a separate, opaque, cryptographically random string with no embedded claims, meant to be stored server-side and swapped for a new access token without forcing re-login.
- **Secrets are not committed to source control.** The JWT signing key lives in .NET User Secrets locally (`dotnet user-secrets`), not in `appsettings.json` — `appsettings.json` only holds the shape of the config (`Issuer`, `Audience`, expiry), with an empty placeholder for `Key`.

---

## 6. CQRS pipeline — MediatR + FluentValidation

Every write operation follows the same shape:

```
Controller → MediatR.Send(command) → ValidationBehavior (automatic) → Handler → IApplicationDbContext → SaveChangesAsync
```

- **Commands** are plain data objects shaped around *what the caller provides*, not around the database schema — e.g. `RegisterSmeCommand` has no `UserId` (doesn't exist yet) and a plaintext `Password` field (hashing is the handler's job, not the command's).
- **Validators** (FluentValidation) mirror the database's own constraints (matching `MaximumLength` values to actual column widths) so a bad request fails with a clear message instead of surfacing as an ugly SQL exception later.
- **`ValidationBehavior<TRequest, TResponse>`** is a MediatR pipeline behavior registered once in `Program.cs` — it automatically runs the matching validator (if one exists) before any handler executes, for every command in the system, without needing to remember to call `.Validate()` manually in each handler.
- **Handlers** contain the actual business logic — uniqueness checks, password hashing, entity creation — and depend only on `IApplicationDbContext`, never the concrete `CashFlowDbContext`.

Registration decision: **one command per role** (`RegisterSmeCommand`, `RegisterInvestorCommand` planned, `RegisterStaffCommand` planned) rather than one generic `RegisterUserCommand` with a role switch — keeps each command's fields strictly typed to what that registration actually needs, with no nullable "only fill this if you're an SME" fields.

---

## 7. What's built and verified so far

| Layer | Status |
|---|---|
| Domain (entities, enums) | ✅ Complete, reviewed against SRS |
| Infrastructure (DbContext, EF config) | ✅ Complete, two bugs found & fixed |
| Database (migrated) | ✅ `InitialCreate` + one follow-up migration, both applied |
| JWT token generation | ✅ Built (`ITokenService`/`JwtTokenService`) |
| MediatR + FluentValidation pipeline | ✅ Wired and proven working |
| SME registration (`RegisterSmeCommand`) | ✅ **End-to-end tested** — hit via HTTP, confirmed real rows in `Users` and `SMEs` tables, password hash verified non-plaintext |
| Login / token issuing | ⬜ Next |
| Investor / Staff registration | ⬜ Planned |
| `[Authorize]`-protected endpoints | ⬜ Planned |
| Invoice upload, OCR, risk scoring, funding marketplace | ⬜ Not started |

---

## 8. Running it locally

```powershell
# Restore & build
dotnet restore
dotnet build

# Apply migrations (creates the LocalDB database if it doesn't exist)
dotnet ef database update --project CashFlowSA.Infrastructure --startup-project CashFlowSA.API

# Set the JWT signing key (one-time, per machine)
cd CashFlowSA.API
dotnet user-secrets init
dotnet user-secrets set "Jwt:Key" "<a long random base64 string>"
cd ..

# Run
dotnet run --project CashFlowSA.API
```

Default connection string targets `(localdb)\mssqllocaldb`, database `CashFlowSA` — see `CashFlowSA.API/appsettings.json`.

---

## 9. Notable lessons from the build so far

A few things worth remembering, since they cost real debugging time:

- **`ProjectReference`s can go missing silently** during package add/remove operations or manual edits — if a project suddenly can't see a type it could see a moment ago, check `.csproj` references before assuming a code bug.
- **Pipeline order matters in `Program.cs`.** `app.MapControllers()` must come *after* `UseAuthentication()`/`UseAuthorization()`, not before — otherwise routing happens before the request has been authenticated, producing confusing 404s instead of clean 401s.
- **DI registration is not automatic just because a class exists.** MediatR handlers, FluentValidation validators, and interface-to-implementation bindings (like `IApplicationDbContext` → `CashFlowDbContext`) all need an explicit line in `Program.cs`, or the container throws at startup with "unable to resolve service" — this was the single most common error class hit while wiring auth.
- **PowerShell terminal sessions don't share variables.** `$body` defined in one terminal window is gone in another — several "empty request body" errors traced back to this, not application code.

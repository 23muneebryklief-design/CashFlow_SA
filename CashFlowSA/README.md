CashFlow SA (Imali Bridge)

An AI-assisted invoice financing marketplace for South African SMEs. SMEs upload unpaid invoices to raise short-term funding from investors, who fund campaigns priced by an automated risk score. Built as a portfolio project for redAcademy's software development bootcamp, designed to reflect production-realistic patterns rather than a toy CRUD app.

Stack: ASP.NET Core (.NET 10) · Entity Framework Core · SQL Server · MediatR (CQRS) · FluentValidation · AutoMapper · JWT Authentication · React/TypeScript (frontend, separate repo path) · PostgreSQL-compatible design decisions carried from planning docs, currently running on SQL Server for local dev

1. Why this architecture

The system serves two portals (a Business Portal for SMEs/Investors, and an Ops Portal for Admin/Credit Analyst/Auditor roles) sharing one backend. That requirement — multiple frontends, one source of truth, several distinct user roles each with different permissions and data visibility — is exactly the case Clean Architecture is built for: business rules shouldn't care which portal is calling them, and the database shouldn't leak into business logic.

The solution is split into four projects, and the dependency direction only ever points inward:

CashFlowSA.API            → knows about HTTP, controllers, JWT middleware
    ↓ depends on
CashFlowSA.Infrastructure → knows about EF Core, SQL Server, the real DbContext
    ↓ depends on
CashFlowSA.Application    → knows about business logic (CQRS commands/queries),
                             but NOT how data is stored or how requests arrive
    ↓ depends on
CashFlowSA.Domain         → knows about nothing except the business itself
                             (entities, enums) — zero external dependencies

Why it matters in practice: Application never references Infrastructure directly. Instead, Application defines interfaces describing what it needs (IApplicationDbContext, ITokenService), and Infrastructure provides the real implementations. This means the core business logic could theoretically be tested, or even ported to a different database, without touching a single line of Application code.

2. Domain layer — the business, modeled in code

CashFlowSA.Domain contains every entity and enum, organized into folders matching the system's actual business areas:

UserAccess — User, UserSession (login/JWT refresh tracking)
SmeManagement — SME (company profile)
InvestorManagement — Investor, InvestorPortfolio, IndividualInvestorProfile, CorporateInvestorProfile, InstitutionalInvestorProfile
KYCCompliance — KYCApplication, KYCDocuments, KYCReview
Invoice Management — Invoice, InvoiceDocument, OCRResult
Risk Assessment — RiskAssessment, RiskScoreHistory, AIExplanation
Funding Marketplace — FundingRequest, FundingCampaign, MarketplaceListing, UnderwritingReview
Investment Management — AuctionBid, Investment
Wallet & Financial Simulation — Wallet, WalletTransaction, Settlement, ReturnDistribution
Document Management — Document, Notification, NotificationHistory
Audit & Governance — AuditLog, GeneratedReport

Every entity inherits from a shared BaseEntity, providing CreatedAt, UpdatedAt, CreatedByUserId, UpdatedByUserId — a consistent audit trail across the schema, matching the compliance/audit requirements in the SRS.

Entities carry inline comments citing the specific SRS requirement they satisfy (e.g. why FundingCampaign has a RowVersion concurrency token — SRS §5.5's over-funding race condition, where two investors could commit past 100% of a campaign's target if their requests landed at the same time without optimistic concurrency control).

3. Infrastructure layer — EF Core, configured per-entity

CashFlowDbContext exposes ~29 DbSets, and every entity has its own IEntityTypeConfiguration<T> class, grouped into config files by business area rather than one giant OnModelCreating method. Deliberate choices baked into the configuration:

decimal(18,2) on every money field, decimal(5,2) on scores/rates — enough precision for ZAR currency without floating-point rounding risk.
DeleteBehavior.Restrict on every foreign key, no cascading deletes — in a financial audit-trail system, silently cascading a delete through invoices → funding campaigns → investments → wallet transactions would be catastrophic.
Unique indexes on natural keys: Users.Email, Invoices.InvoiceNumber, SMEs.CompanyEmail/RegistrationNumber.
All enum properties store as string, not int. Every enum column across the schema (30 in total) uses HasConversion<string>() with a HasMaxLength sized to that specific enum's longest member (e.g. RiskGrade at 5 chars since it's just A–E, NotificationEvent at 40). This is a deliberate trade-off: int storage is marginally faster/smaller, but given this system's audit-heavy nature (a dedicated Auditor role, an Ops portal with human reviewers), reordering-safety and human-readability in raw SQL mattered more than the negligible performance cost at this scale.
A RowVersion concurrency token on FundingCampaign, used for optimistic-concurrency protection during fractional funding commits.
Bugs found and fixed during review
Nullable "hasn't happened yet" timestamps had database defaults of "now." Fields like UserSession.LogoutTimestamp and KYCApplication.ReviewedAt are DateTime? specifically because null means "this hasn't happened yet" — but the EF configuration gave them HasDefaultValueSql("GETUTCDATE()"). Fixed by removing the default from these fields.
Several entities redeclared properties already inherited from BaseEntity, shadowing rather than overriding the base class (a CS0108 warning) — a landmine for any code casting to BaseEntity. Fixed by removing the redundant declarations.
IApplicationDbContext repeatedly lagged behind CashFlowDbContext's actual DbSets as new modules were added — several handlers were written against DbSets (KYCApplications, Invoices, MarketplaceListings, FundingCampaigns, etc.) that the interface hadn't exposed yet, causing CS0246 failures. Now current as of the Funding module.
InvestorConfiguration was missing a mapping for Investor.InvestorType entirely, causing EF to silently default it to int while every other enum in the project had already been converted to string. Caught and fixed after the fact.
Package hygiene

AutoMapper was upgraded 12.0.1 → 13.0.1 → 16.1.1, chasing a real, actively-disclosed denial-of-service CVE (GHSA-rvv3-g6hj-g44x / CVE-2026-32933 — uncontrolled recursion on self-referential object graphs). AutoMapper.Extensions.Microsoft.DependencyInjection was removed as deprecated, since AddAutoMapper() is built into the core package as of 13.0.

4. Database — migrated and verified
Migration	Purpose
InitialCreate	All ~29 tables, foreign keys, and indexes generated from the model
FixInvestorPortfolioUpdatedAt	Corrected InvestorPortfolio.UpdatedAt to nullable, once it started correctly inheriting from BaseEntity
ConvertEnumsToStringStorage	Converted all enum columns from int to string storage project-wide, sized per-enum
ConfigureInvestorType	Closed the Investor.InvestorType gap noted above

Every migration is reviewed diff-by-diff before being applied, rather than trusted blindly — generate, inspect, then apply.

5. Authentication — custom JWT, not ASP.NET Identity

Microsoft.Extensions.Identity.Core provides PasswordHasher<T> — salted, adaptive PBKDF2 hashing — without pulling in the full Identity/EF store machinery. ITokenService (interface in Application) / JwtTokenService (implementation in Infrastructure) generates short-lived signed access tokens (carrying Sub, Email, Role claims) and opaque refresh tokens.

Secrets are not committed to source control — the JWT signing key lives in .NET User Secrets locally, not in appsettings.json.

6. CQRS pipeline — MediatR + FluentValidation

Every write operation follows the same shape:

Controller → MediatR.Send(command) → ValidationBehavior (automatic) → Handler → IApplicationDbContext → SaveChangesAsync
Commands are plain data objects shaped around what the caller provides, not the database schema.
Validators mirror the database's own constraints so a bad request fails with a clear message rather than an ugly SQL exception.
ValidationBehavior<TRequest, TResponse> runs the matching validator automatically for every command — no handler calls .Validate() manually.
Handlers hold the actual business logic (uniqueness checks, status-transition rules, entity creation) and depend only on IApplicationDbContext, never the concrete CashFlowDbContext.
A global ExceptionHandlingMiddleware maps custom exceptions to HTTP status codes: ValidationException → 400, NotFoundException → 404, ConflictException → 409, ForbiddenException → 403, AuthenticationFailedException → 401.
7. What's built and verified so far
Module	Status
Domain (entities, enums)	✅ Complete, reviewed against SRS
Infrastructure (DbContext, EF config, string-based enums)	✅ Complete
Database (migrated)	✅ 4 migrations applied, diff-reviewed
JWT token generation	✅ Built
MediatR + FluentValidation pipeline	✅ Wired and proven working
SME / Investor registration, Login	✅ End-to-end tested
KYC submission (SubmitKycApplicationCommand)	✅ Built — enforces resubmission-only-after-Rejected rule per SRS 5.2
KYC status check (GetKycStatusQuery)	✅ Built
Invoice upload (UploadInvoiceCommand)	✅ Built — blocks upload unless SME's KYC is Verified
Invoice get / list / correct fields / submit	✅ Built (GetInvoiceQuery, GetInvoicesBySmeQuery, CorrectInvoiceFieldsCommand, SubmitInvoiceCommand)
Marketplace listings (browse + detail)	✅ Built (GetListingsQuery, GetListingDetailQuery)
Funding — single-investor, fractional, auction bid, campaign status	✅ Built — fractional commits use RowVersion optimistic concurrency to prevent over-funding races (SRS 5.5)
Wallet, Settlement, Notification, Admin/Ops KYC review, Audit, Analytics	⬜ Controller routes scaffolded (return 501), CQRS slices not yet written
Auction winner determination at close	⬜ Not implemented — bids are recorded, but nothing yet resolves the winning bid at FundingDeadline (needs a scheduled job)
OCR extraction, RabbitMQ async pipeline, Azure Blob Storage	⬜ Not started
[Authorize]-protected endpoints / role enforcement	⬜ Planned
8. Running it locally
powershell
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

Default connection string targets (localdb)\mssqllocaldb, database CashFlowSA — see CashFlowSA.API/appsettings.json.

9. Notable lessons from the build so far
ProjectReferences can go missing silently during package add/remove operations or manual edits — if a project suddenly can't see a type it could see a moment ago, check .csproj references before assuming a code bug.
Pipeline order matters in Program.cs. app.MapControllers() must come after UseAuthentication()/UseAuthorization(), not before — otherwise routing happens before the request has been authenticated, producing confusing 404s instead of clean 401s.
DI registration is not automatic just because a class exists. MediatR handlers, FluentValidation validators, and interface-to-implementation bindings all need an explicit line in Program.cs, or the container throws at startup with "unable to resolve service."
IApplicationDbContext needs updating every time a new module's handler needs a new DbSet. This has been the single most repeated build error across KYC, Invoice, and Funding — the interface consistently lagged one module behind CashFlowDbContext's real DbSets. Worth checking first whenever a handler throws CS0246 on an entity type that clearly exists.
PowerShell terminal sessions don't share variables. $body defined in one terminal window is gone in another — several "empty request body" errors traced back to this, not application code.
dotnet ef commands are relative to the current directory, not the solution root — running them from inside a sub-project folder (e.g. CashFlowSA.API) breaks --project/--startup-project path resolution unless adjusted accordingly (..\CashFlowSA.Infrastructure instead of CashFlowSA.Infrastructure).
Changing an EF Core enum property from int to string storage is a real schema migration, not a free refactor — it alters the underlying SQL column type and needs migrations add + database update, same as any other schema change.

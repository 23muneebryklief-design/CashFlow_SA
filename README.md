# CashFlow SA (Imali Bridge)

## 1. What this project is

CashFlow SA is an AI-assisted invoice financing marketplace for South African SMEs. Small businesses upload unpaid invoices to raise short-term working capital; investors fund those invoices (individually, fractionally, or via auction) in exchange for a return once the debtor pays. An automated risk-scoring layer prices each opportunity so investors can make informed decisions.

Built as a portfolio project for redAcademy's software development bootcamp, deliberately designed to reflect production-realistic patterns — Clean Architecture, CQRS, role-based auth, audit trails, optimistic concurrency — rather than a toy CRUD app.

The system serves **two portals** sharing one backend:
- **Business Portal** — SMEs (upload invoices, request financing) and Investors (browse listings, fund campaigns)
- **Ops Portal** — Credit Analysts (review KYC and underwriting), Admins (trigger settlements), Auditors (read-only compliance access)

## 2. Stack

| Layer | Technology |
|---|---|
| Backend framework | ASP.NET Core (.NET 10) |
| Architecture | Clean Architecture — Domain / Application / Infrastructure / API |
| Data access | Entity Framework Core, SQL Server |
| CQRS / mediator | MediatR |
| Validation | FluentValidation (auto-run via a MediatR pipeline behavior) |
| Object mapping | AutoMapper |
| Authentication | Custom JWT (access + refresh tokens), `PasswordHasher<T>` for hashing — not ASP.NET Identity |
| Authorization | Role-based, `[Authorize(Roles = "...")]` per controller/action |
| Background processing | `BackgroundService` (auction close resolution) |
| Testing | xUnit — unit tests (EF Core InMemory) + integration tests (`WebApplicationFactory`) |
| Frontend | React / TypeScript / Vite (separate repo path, in progress) |

## 3. Architecture at a glance

```
CashFlowSA.API            → HTTP, controllers, JWT middleware, background services
    ↓ depends on
CashFlowSA.Infrastructure → EF Core, SQL Server, the real DbContext
    ↓ depends on
CashFlowSA.Application    → business logic (CQRS commands/queries) — does NOT know
                             how data is stored or how requests arrive
    ↓ depends on
CashFlowSA.Domain         → entities, enums — zero external dependencies
```

`Application` depends only on interfaces (`IApplicationDbContext`, `ITokenService`) that `Infrastructure` implements — the dependency arrow always points inward. Every enum in the schema is stored as `string`, not `int`, sized per-enum (e.g. `RiskGrade` at 5 chars, `NotificationEvent` at 40) — a deliberate trade-off favoring reordering-safety and human-readable audit logs over the negligible performance cost at this project's scale.

## 4. Running it on a new device

### Prerequisites
- .NET 10 SDK
- SQL Server / SQL Server LocalDB
- `dotnet-ef` global tool: `dotnet tool install --global dotnet-ef` (skip if already installed)

### Steps

```powershell
# 1. Clone and restore
git clone <repo-url>
cd CashFlowSA
dotnet restore

# 2. Build (from the solution root, not a subfolder)
dotnet build

# 3. Set the JWT signing key (one-time, per machine)
#    The real key is never committed -- it lives in .NET User Secrets locally.
cd CashFlowSA.API
dotnet user-secrets init
dotnet user-secrets set "Jwt:Key" "<a long random base64 string>"
cd ..

# 4. Apply database migrations (creates the LocalDB database if it doesn't exist)
#    Run this from the solution root -- dotnet ef paths are relative to your
#    current directory, not the solution root, so this exact form matters.
dotnet ef database update --project CashFlowSA.Infrastructure --startup-project CashFlowSA.API

# 5. Run the API
dotnet run --project CashFlowSA.API
```

Default connection string targets `(localdb)\mssqllocaldb`, database `CashFlowSA` — see `CashFlowSA.API/appsettings.json` to point elsewhere.

### Common setup gotchas

- **`dotnet ef` commands are relative to your current folder**, not the solution root. Running them from inside `CashFlowSA.API` breaks `--project`/`--startup-project` resolution unless you adjust the paths (e.g. `..\CashFlowSA.Infrastructure`).
- **`app.MapControllers()` must come after `UseAuthentication()`/`UseAuthorization()`** in the pipeline, or you'll get confusing 404s instead of clean 401s.
- **If `dotnet run` reports the app is already running / files locked**, stop the previous running instance before rebuilding — Windows locks the DLLs of a running process.

## 5. What's completed

| Area | Status | Notes |
|---|---|---|
| Domain (entities, enums) | ✅ | All enums stored as `string`, sized per-enum |
| Infrastructure (EF config, migrations) | ✅ | 6+ migrations applied, diff-reviewed before each apply |
| Authentication | ✅ | SME/Investor registration, login, JWT + refresh tokens |
| Role-based authorization | ✅ | `[Authorize(Roles=...)]` enforced across every controller, verified via integration tests |
| KYC | ✅ | Submit, status check, admin approve/reject, pending queue |
| Invoice | ✅ | Upload, get, list by SME, correct fields, submit (Draft→Submitted) |
| Marketplace | ✅ | Browse listings (filterable by risk/industry/amount), listing detail |
| Funding — commitments | ✅ | Single-investor, fractional (concurrency-safe via `RowVersion`), auction bid, campaign status |
| Funding — request creation | ✅ | `CreateFundingRequestCommand` — SME requests financing on an Approved invoice |
| Funding — SME wallet crediting | ✅ | SME's wallet is credited the moment their campaign reaches `Funded` (both single-investor and fractional paths) |
| Funding — auction close resolution | ✅ | Background service resolves the highest bid once `FundingDeadline` passes, credits SME, records the winning `Investment` |
| Wallet | ✅ | Balance, transaction history |
| Settlement | ✅ | Get, trigger — see open items below re: return-rate calculation |
| Notification | ✅ | History |
| Audit | ✅ | Filterable log query |
| Analytics | ✅ | Funding volume, risk distribution |

## 6. What's still to do

### Real gaps — need a design decision, not just more code

- **Underwriting decision (Phase 3 of the funding pipeline).** `CreateFundingRequestCommand` creates a `Pending` `FundingRequest`, but nothing yet lets a Credit Analyst approve/reject it — and approval is where a `FundingCampaign` + `MarketplaceListing` are actually supposed to get created. Right now nothing in the whole project creates those two entities at all.
- **`FundingCampaign.ExpectedReturnRate` exists on the model but nothing sets it yet.** It needs to be populated during the (not-yet-built) underwriting approval step, and `TriggerSettlementCommandHandler` still needs updating to read it instead of guessing a return from `SettledAmount - FundedAmount`.
- **Auction winning-bid rule is an assumption, not a confirmed spec.** Currently the winning bid does **not** need to fully cover `TargetAmount` — it just has to be the highest active bid. If auctions should behave like single-investor funding (must cover the full target), this needs revisiting.

### Not started

- OCR extraction pipeline for uploaded invoices (fields are currently only filled in manually via `CorrectInvoiceFieldsCommand`)
- RabbitMQ async messaging (OCR + notification fan-out currently run synchronously, if at all)
- SignalR real-time notification push
- Azure Blob Storage for actual file storage (uploads currently just accept a `FilePath` string; no real storage integration)
- OpenAI integration (`AIExplanation` entity exists in Domain, nothing populates it)
- `RiskAssessment` creation flow — nothing currently creates a `RiskAssessment` for a submitted invoice, which is the missing link between Invoice approval and Marketplace risk-grade filtering

### Smaller, known items

- `InvestorType` enum has a typo (`Corparate`) — not yet corrected
- No endpoint exists yet for an SME to view their own `FundingRequest` history/status

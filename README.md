

# CashFlow SA (Imali Bridge)

> **An AI-powered invoice financing marketplace for South African SMEs.**

CashFlow SA is a full-stack fintech platform that enables South African SMEs to unlock working capital by selling unpaid invoices to investors through a secure marketplace. The platform combines automated risk assessment, KYC compliance, fractional investment funding, wallet management, settlements, notifications, and audit logging within a production-inspired Clean Architecture.

Built as a portfolio project, the solution demonstrates enterprise software design patterns including CQRS, Clean Architecture, Entity Framework Core, JWT authentication, optimistic concurrency, and domain-driven modelling.

---

# Architecture

The solution follows **Clean Architecture**.

```
CashFlowSA.API
        │
        ▼
CashFlowSA.Application
        │
        ▼
CashFlowSA.Domain
        ▲
        │
CashFlowSA.Infrastructure
```

Each layer has a single responsibility.

### Domain

Contains

* Entities
* Enums
* Business Rules
* Aggregate Models

The Domain layer has no external dependencies.

### Application

Contains

* CQRS Commands
* CQRS Queries
* DTOs
* Validation
* Interfaces
* Mapping Profiles
* Business Logic

The Application layer depends only on the Domain.

### Infrastructure

Contains

* Entity Framework Core
* SQL Server
* Repository implementations
* JWT services
* Database configurations
* External integrations

### API

Provides

* REST endpoints
* Authentication
* Authorization
* Middleware
* Dependency Injection
* Swagger

---

# Technology Stack

| Layer             | Technology             |
| ----------------- | ---------------------- |
| Backend           | ASP.NET Core (.NET 10) |
| ORM               | Entity Framework Core  |
| Database          | SQL Server             |
| Architecture      | Clean Architecture     |
| Pattern           | CQRS (MediatR)         |
| Validation        | FluentValidation       |
| Mapping           | AutoMapper             |
| Authentication    | JWT                    |
| Password Security | PasswordHasher         |
| API Documentation | Swagger                |
| Frontend          | React + TypeScript     |

---

# Features

## Authentication

* User Registration
* Login
* JWT Authentication
* Refresh Tokens
* Password Hashing
* Role Claims

---

## SME Portal

* Company Registration
* KYC Submission
* Invoice Upload
* Invoice Correction
* Invoice Submission
* Invoice Status Tracking

---

## Investor Portal

* Marketplace Listings
* Funding Campaigns
* Fractional Investments
* Auction Bidding
* Portfolio Management

---

## Marketplace

* Browse Listings
* Listing Details
* Funding Progress
* Investment Tracking

---

## Funding

* Campaign Creation
* Fractional Funding
* Auction Bids
* Optimistic Concurrency
* Campaign Status Management

---

## Wallet

* Wallet Management
* Wallet Transactions
* Balance Tracking

---

## Settlement

* Settlement Processing
* Distribution Records

---

## Notifications

* User Notifications
* Notification History

---

## Operations Portal

Administrative functionality includes:

* KYC Reviews
* Audit Logs
* Analytics
* Reporting

---

# Security

The application includes several security measures:

* JWT Authentication
* Password Hashing
* Claims-based Authorization
* FluentValidation
* Optimistic Concurrency
* Audit Trail
* String-based Enum Storage
* Restricted Delete Behaviors

---

# Database

The project uses Entity Framework Core with SQL Server.

The schema includes approximately thirty business entities covering:

* Users
* SMEs
* Investors
* KYC
* Invoices
* Funding Campaigns
* Investments
* Wallets
* Settlements
* Notifications
* Audit Logs
* Analytics

Database design includes

* Entity configurations per entity
* Decimal precision for financial values
* Unique indexes
* Optimistic concurrency
* Enum-to-string conversion
* Explicit foreign key behaviour

---

# Current API Modules

| Module             | Status     |
| ------------------ | ---------- |
| Authentication     | ✅ Complete |
| KYC                | ✅ Complete |
| Admin KYC Review   | ✅ Complete |
| Invoice Management | ✅ Complete |
| Marketplace        | ✅ Complete |
| Funding            | ✅ Complete |
| Wallet             | ✅ Complete |
| Settlement         | ✅ Complete |
| Notifications      | ✅ Complete |
| Audit              | ✅ Complete |
| Analytics          | ✅ Complete |

---

# Running the Project

```bash
dotnet restore

dotnet build

dotnet ef database update --project CashFlowSA.Infrastructure --startup-project CashFlowSA.API

dotnet run --project CashFlowSA.API
```

Configure the JWT signing key using .NET User Secrets before running the application.

---

# Design Principles

The project was designed around enterprise software development practices including:

* Clean Architecture
* CQRS
* SOLID Principles
* Dependency Injection
* Domain-Driven Design concepts
* Validation Pipeline
* Optimistic Concurrency
* Secure Authentication
* Financial Data Integrity

---

# Future Improvements

Potential future enhancements include:

* OCR-powered invoice extraction
* Azure Blob Storage
* RabbitMQ background processing
* Scheduled auction resolution jobs
* Email/SMS notifications
* Real payment gateway integration
* AI-powered risk scoring
* Docker deployment
* CI/CD pipeline

---

# Author

Developed as a portfolio project demonstrating enterprise-level backend architecture, financial domain modelling, and modern ASP.NET Core development practices.

---

# Getting Started

## Prerequisites

Before running the project, ensure you have the following installed:

| Software | Version |
|----------|----------|
| .NET SDK | 10.0 |
| SQL Server | SQL Server Express or LocalDB |
| SQL Server Management Studio (optional) | Latest |
| Visual Studio 2022 | Latest with ASP.NET workload |
| Git | Latest |

---

## Clone the Repository

```bash
git clone https://github.com/23muneebryklief-design/CashFlow_SA.git
git clone 
cd CashFlowSA
```

---

## Restore Packages

```bash
dotnet restore
```

---

## Configure the Database

The default connection string uses SQL Server LocalDB.

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=CashFlowSA;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

If you are using SQL Server Express or another SQL Server instance, update the connection string in:

```
CashFlowSA.API/appsettings.json
```

---

## Configure JWT Secrets

The project stores the JWT signing key using .NET User Secrets.

Navigate to the API project.

```bash
cd CashFlowSA.API
```

Initialize User Secrets.

```bash
dotnet user-secrets init
```

Create a signing key.

```bash
dotnet user-secrets set "Jwt:Key" "ReplaceWithYourOwnLongRandomSecretKey"
```

You can also configure:

```bash
dotnet user-secrets set "Jwt:Issuer" "CashFlowSA"
dotnet user-secrets set "Jwt:Audience" "CashFlowSAUsers"
```

---

## Create the Database

Return to the solution folder and apply migrations.

```bash
dotnet ef database update \
--project CashFlowSA.Infrastructure \
--startup-project CashFlowSA.API
```

This command will:

- Create the CashFlowSA database
- Apply all migrations
- Create every table
- Configure indexes
- Configure foreign keys

---

## Build the Solution

```bash
dotnet build
```

---

## Run the API

```bash
dotnet run --project CashFlowSA.API
```

The API will typically start on

```
https://localhost:7xxx
```

---

## Scalar

Once the API is running, open:

```
https://localhost:7xxx/scalar
```

Swagger provides interactive documentation for every endpoint.

---

## Default Workflow

1. Register an SME account.
2. Log in.
3. Submit KYC documents.
4. Verify the KYC using the Admin endpoints.
5. Upload an invoice.
6. Submit the invoice.
7. Browse the Marketplace.
8. Create funding requests.
9. Invest in campaigns.
10. View wallet transactions and settlements.

---

## Troubleshooting

### Build Errors

Restore packages.

```bash
dotnet restore
```

---

### Database Connection Errors

Verify:

- SQL Server or LocalDB is installed
- Connection string is correct
- SQL Server service is running

---

### Migration Errors

Delete the database and re-run:

```bash
dotnet ef database update
```

---

### JWT Errors

Ensure a JWT signing key has been configured:

```bash
dotnet user-secrets list
```

---

### Port Already In Use

Stop the existing process or update the launch profile.

---

## Project Structure

```
CashFlowSA.sln

src/
│
├── CashFlowSA.API
├── CashFlowSA.Application
├── CashFlowSA.Domain
└── CashFlowSA.Infrastructure
```

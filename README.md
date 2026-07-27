

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

## One suggestion

Your old README was **very technical** (around 150 lines). Looking at the maturity of your project now, I would expand this into a **400–600 line GitHub README** with:

* architecture diagrams,
* folder tree,
* ERD,
* request flow diagrams,
* screenshots,
* API examples,
* sequence diagrams,
* and detailed explanations of each module.

That would make it look like documentation for a real fintech product rather than a student project and significantly strengthen your GitHub portfolio.

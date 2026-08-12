CashFlow SA (Imali Bridge)

An AI-assisted invoice financing marketplace for South African SMEs, connecting businesses that need short-term working capital with investors looking for structured funding opportunities.

CashFlow SA is a full-stack fintech portfolio project built to model a production-style invoice financing platform. SMEs can submit unpaid invoices for funding, while investors can discover marketplace opportunities and participate in funding campaigns. The platform also includes operational tooling for administrators, credit analysts, and auditors.

Overview

CashFlow SA is designed around two main user-facing portals backed by a shared API:

Business Portal — for SMEs and Investors

Operations Portal — for Admins, SuperAdmins, Credit Analysts, and Auditors

The backend follows Clean Architecture and CQRS principles so that business logic remains independent from HTTP, database, and infrastructure concerns.

Core Features

SME

Account registration and authentication

SME profile management

KYC/FICA application submission

KYC status tracking

Invoice upload and management

Invoice field correction and submission

Funding campaign workflow

Investor

Investor registration and authentication

Investor profile and portfolio management

Marketplace browsing

Funding campaign and listing details

Single-investor and fractional funding flows

Auction bidding support

Investment tracking

Operations

Admin authentication

Role-based access control

KYC review workflows

Auditor dashboard

Profile management

Audit and analytics foundations

Administrative controls and reporting foundations

Platform

JWT authentication with refresh-token support

Role-based protected routes

CQRS with MediatR

FluentValidation pipeline

Entity Framework Core migrations

Optimistic concurrency protection for funding campaigns

Global exception handling

File-storage integration

Background auction-closing service

OpenAPI/Scalar API documentation in development

Architecture

The solution is split into four backend projects with dependencies pointing inward:

CashFlowSA.API
    |
    v
CashFlowSA.Infrastructure
    |
    v
CashFlowSA.Application
    |
    v
CashFlowSA.Domain

Domain

Contains the core business model, entities, enums, and shared domain concepts.

Major business areas include:

User access

SME management

Investor management

KYC and compliance

Invoice management

Risk assessment

Funding marketplace

Investment management

Wallet and financial simulation

Document management

Notifications

Audit and governance

Application

Contains application-level business logic using CQRS.

Commands and queries are handled through MediatR, with interfaces such as IApplicationDbContext and ITokenService keeping application logic independent from infrastructure implementations.

Infrastructure

Provides the concrete implementations for:

Entity Framework Core

SQL Server

JWT token generation

File storage

Database configuration

Entity mappings

Database migrations

Seed data

API

The ASP.NET Core API exposes the application through controllers and handles:

Authentication and authorization

HTTP requests and responses

CORS

OpenAPI

Global exception handling

Dependency injection

Background services

Technology Stack

Backend

.NET 10

ASP.NET Core Web API

Entity Framework Core 10

SQL Server / LocalDB

MediatR

FluentValidation

AutoMapper

JWT Bearer Authentication

Scalar / OpenAPI

AWS S3 SDK

Supabase-compatible file storage integration

Frontend

React 19

TypeScript

Vite

React Router

Axios

CSS Modules

Repository Structure

CashFlow_SA/
├── CashFlowSA/
│   ├── CashFlowSA.API/
│   │   ├── Controllers/
│   │   ├── Middleware/
│   │   ├── Services/
│   │   └── Program.cs
│   │
│   ├── CashFlowSA.Application/
│   │   ├── Common/
│   │   ├── Features/
│   │   └── ...
│   │
│   ├── CashFlowSA.Domain/
│   │   └── Models/
│   │
│   ├── CashFlowSA.Infrastructure/
│   │   ├── Data/
│   │   ├── Migrations/
│   │   └── Services/
│   │
│   └── CashFlowSA.slnx
│
└── cashflow-sa-web/
    ├── public/
    ├── src/
    │   ├── components/
    │   ├── Context/
    │   ├── Hooks/
    │   └── pages/
    └── package.json

Frontend Routes

The React application currently includes routes for:

Route

Access

/

Public

/login

Public

/register

Public

/admin

Admin login

/investor-marketplace

Investor

/investor-dashboard

Investor

/sme-dashboard

SME

/invoices

SME

/fica-verification

SME

/profile

Authenticated users

/admin-dashboard

Admin / SuperAdmin / Credit Analyst

/auditor-kyc

Auditor / Admin / SuperAdmin

Protected routes are enforced using the application's ProtectedRoute component and role requirements.

Getting Started

Prerequisites

Install the following before running the project:

.NET 10 SDK

Node.js

SQL Server LocalDB or another compatible SQL Server instance

Git

1. Clone the repository

git clone <your-repository-url>
cd CashFlow_SA

2. Configure the backend

The backend currently uses SQL Server LocalDB by default.

The development connection string is configured in:

CashFlowSA/CashFlowSA/CashFlowSA.API/appsettings.json

Do not place production secrets in appsettings.json.

The application expects JWT configuration, including a signing key. For local development, use .NET User Secrets:

cd CashFlowSA/CashFlowSA/CashFlowSA.API

dotnet user-secrets set "Jwt:Key" "<your-development-jwt-secret>"

If file storage is enabled, configure the required storage settings through User Secrets or environment-specific configuration rather than committing credentials.

3. Restore and run the backend

From the API project:

dotnet restore
dotnet ef database update
dotnet run

The development API is configured to run on:

https://localhost:7052
http://localhost:5081

OpenAPI and Scalar API documentation are enabled when the application runs in the Development environment.

4. Run the frontend

Open another terminal:

cd cashflow-sa-web
npm install
npm run dev

The Vite development server normally runs at:

http://localhost:5173

The backend CORS policy is currently configured to allow the Vite development origin.

Database

Entity Framework Core migrations are stored in:

CashFlowSA/CashFlowSA/CashFlowSA.Infrastructure/Migrations/

Apply existing migrations with:

dotnet ef database update

The database model uses several deliberate financial-system safeguards:

decimal(18,2) for monetary values

String storage for enums

Restricted delete behavior on financial relationships

Unique indexes for important natural keys

Optimistic concurrency on funding campaigns

Audit timestamps inherited from BaseEntity

Funding Concurrency

Funding campaigns use optimistic concurrency protection to prevent over-funding when multiple investors attempt to fund the same campaign at the same time.

This is particularly important for fractional funding, where concurrent requests could otherwise cause the total funded amount to exceed the campaign target.

Authentication & Authorization

CashFlow SA uses custom JWT authentication rather than the full ASP.NET Identity stack.

Access tokens include claims such as:

User ID

Email

Role

Role-based access is used across the application.

Supported operational roles include:

SME

Investor

Admin

SuperAdmin

CreditAnalyst

Auditor

Passwords are hashed using Microsoft's password hashing implementation, while JWT signing configuration is kept outside committed source code.

CQRS Request Pipeline

Write operations generally follow this flow:

HTTP Request
    ↓
Controller
    ↓
MediatR Command
    ↓
FluentValidation
    ↓
Command Handler
    ↓
IApplicationDbContext
    ↓
SaveChangesAsync

This keeps controllers thin and centralizes business rules inside application handlers.

The API also uses a global exception-handling middleware that maps application exceptions to appropriate HTTP responses, including validation, not-found, conflict, forbidden, and authentication failures.

Development Commands

Backend

dotnet restore
dotnet build
dotnet run
dotnet ef database update

Frontend

npm install
npm run dev
npm run build
npm run lint
npm run preview

Current Implementation Status

The current version includes working foundations and flows for:

Domain model

EF Core infrastructure

Database migrations

JWT authentication

MediatR/CQRS pipeline

FluentValidation

SME registration and login

Investor registration and login

KYC submission and status checks

Invoice upload, retrieval, correction, and submission

Marketplace listing retrieval

Funding campaigns

Single-investor funding

Fractional funding

Auction bidding

Campaign status handling

Wallet/settlement/notification foundations

Admin and auditor KYC workflows

Audit and analytics foundations

React dashboards and protected navigation

Some operational modules remain under active development and refinement.

Security Notes

This repository is intended for development and portfolio use.

Before deploying to a production environment:

Replace development secrets

Configure production JWT signing keys securely

Configure a production database

Configure production CORS origins

Review authentication and authorization policies

Configure secure file storage

Enable appropriate HTTPS and infrastructure security controls

Review seeded admin credentials

Disable or replace development-only defaults

Never commit:

JWT signing keys

Database passwords

Cloud storage credentials

API keys

Production connection strings

Other secrets

Project Goals

CashFlow SA is intended to demonstrate more than a basic CRUD application. The project focuses on:

Clean Architecture

Separation of concerns

CQRS

Role-based security

Financial-domain modelling

Auditability

Concurrency handling

Validation pipelines

Database migration discipline

Full-stack integration

Production-oriented engineering practices

License

This project is currently a portfolio/educational project. Add a formal license here if the repository is intended for redistribution or open-source use.

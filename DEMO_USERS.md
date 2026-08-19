# CashFlow SA Development Demo Accounts

These accounts are seeded automatically when the API runs in the **Development** environment.

All accounts use the same development password:

**Password:** `CashFlow123!`

| Role | Email | Name | Notes |
|---|---|---|---|
| SME | `sme.demo@cashflowsa.co.za` | Sarah Mokoena | Active SME profile + ZAR 50,000 demo wallet |
| Investor | `investor.demo@cashflowsa.co.za` | David Naidoo | Active investor + ZAR 250,000 demo wallet + portfolio |
| Credit Analyst | `credit.demo@cashflowsa.co.za` | Thabo Molefe | Credit Analyst portal access |
| Admin | `admin.demo@cashflowsa.co.za` | Aisha Pillay | Admin portal access |
| Auditor | `auditor.demo@cashflowsa.co.za` | Lerato Dlamini | Auditor portal access |
| Super Admin | `superadmin.demo@cashflowsa.co.za` | Michael van der Merwe | Full admin access; can create Admin/Credit Analyst/Auditor accounts |

## Seeding behavior

- Demo accounts are seeded **only when `ASPNETCORE_ENVIRONMENT=Development`**.
- Seeding is idempotent: restarting the API does not create duplicate users.
- Existing demo accounts are reset to the configured demo password and `Active` status on startup so they remain usable during development.
- The password can be changed in `CashFlowSA.API/appsettings.Development.json` under `DemoUsers:Password`.

These credentials are for local development/testing only and must not be used in production.

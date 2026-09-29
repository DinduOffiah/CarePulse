# CarePulse – Digital Health Clinic API

Enterprise-grade backend API for multi-doctor private health clinics.

**Stack:** ASP.NET Core 9 · C# · PostgreSQL · EF Core · Redis · Hangfire · JWT + Identity · Clean Architecture + CQRS (MediatR) · FluentValidation · Serilog · Swagger

## Features (Foundation + Roadmap)

- Multi-role authentication (Admin, Doctor, Receptionist, Patient) with JWT + refresh tokens
- Patient & Doctor management
- Doctor availability scheduling
- Appointment booking with double-booking protection & optimistic concurrency
- Clinical notes (SOAP format)
- Billing / invoices
- Background jobs (reminders, notifications) via Hangfire
- Soft deletes + full audit columns
- Structured logging, health checks, global exception handling
- Standard response envelope
- Docker-ready

## Prerequisites

- .NET 9 SDK
- Docker & Docker Compose (recommended)
- PostgreSQL 16 + Redis 7 (or use Docker Compose)

## Quick Start with Docker

```bash
cd docker
docker compose up -d --build
```

API will be available at: http://localhost:8080  
Swagger: http://localhost:8080/swagger  
Hangfire Dashboard: http://localhost:8080/hangfire  
Health: http://localhost:8080/health

## Local Development

1. Start infrastructure:
   ```bash
   docker compose -f docker/docker-compose.yml up -d postgres redis
   ```

2. Update `src/CarePulse.Api/appsettings.Development.json` if needed.

3. Apply migrations (after creating them):
   ```bash
   dotnet ef migrations add InitialCreate --project src/CarePulse.Infrastructure --startup-project src/CarePulse.Api
   dotnet ef database update --project src/CarePulse.Infrastructure --startup-project src/CarePulse.Api
   ```

4. Run the API:
   ```bash
   dotnet run --project src/CarePulse.Api
   ```

## Project Structure

```
src/
├── CarePulse.Api              # Presentation layer (Controllers, Middleware, Program.cs)
├── CarePulse.Application      # Use cases, MediatR handlers, Validators, DTOs
├── CarePulse.Domain           # Entities, Enums, Business rules
├── CarePulse.Infrastructure   # EF Core, Identity, Redis, Hangfire, external services
└── CarePulse.Shared           # Shared result types, common utilities
```

## Architecture Principles

- Clean Architecture / Onion
- SOLID, DRY, KISS, YAGNI
- CQRS via MediatR
- Repository pattern (via EF Core DbContext + future repositories)
- Soft deletes + audit trail
- Optimistic concurrency on appointments
- Global exception handling → consistent JSON envelope
- Structured logging with correlation IDs

## Standard Response Format

**Success**
```json
{
  "success": true,
  "message": "User created successfully.",
  "data": {},
  "meta": {},
  "timestamp": "2026-09-29T14:00:00Z",
  "requestId": "0HN..."
}
```

**Failure**
```json
{
  "success": false,
  "message": "Validation failed.",
  "errors": ["Email is required"],
  "timestamp": "2026-09-29T14:00:00Z",
  "requestId": "0HN..."
}
```

## Next Milestones

1. ✅ Foundation (this release)
2. Identity & Access (register/login/refresh/roles)
3. Patients & Doctors + Availability
4. Appointments (core)
5. Clinical Notes + Billing
6. Notifications / Hangfire jobs
7. Full test suite + hardening

## License

MIT – built for portfolio demonstration.

## Milestone 2 – Identity & Access (Completed)

### Endpoints

| Method | Endpoint              | Auth     | Description                          |
|--------|-----------------------|----------|--------------------------------------|
| POST   | /api/v1/auth/register | Public   | Register new user with role          |
| POST   | /api/v1/auth/login    | Public   | Login and receive tokens             |
| POST   | /api/v1/auth/refresh  | Public   | Refresh access token                 |
| POST   | /api/v1/auth/logout   | Bearer   | Invalidate refresh token             |
| GET    | /api/v1/auth/me       | Bearer   | Get current authenticated user       |

### Roles

- Admin
- Doctor
- Receptionist
- Patient

Roles are automatically seeded on application startup.

### Example Register Request

```json
{
  "email": "doctor@clinic.com",
  "password": "SecureP@ssw0rd1",
  "firstName": "Jane",
  "lastName": "Smith",
  "role": "Doctor",
  "phoneNumber": "+1234567890"
}
```

### Example Login Response

```json
{
  "success": true,
  "message": "Login successful.",
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIs...",
    "refreshToken": "base64string...",
    "accessTokenExpiration": "2026-09-29T15:30:00Z",
    "refreshTokenExpiration": "2026-10-06T15:15:00Z",
    "user": {
      "id": "guid",
      "email": "doctor@clinic.com",
      "firstName": "Jane",
      "lastName": "Smith",
      "fullName": "Jane Smith",
      "roles": ["Doctor"]
    }
  },
  "timestamp": "...",
  "requestId": "..."
}
```

### Security Features Implemented

- JWT access tokens (15 min default) + rotating refresh tokens (7 days)
- Password requirements enforced (upper, lower, digit, special, min 8)
- Account lockout after 5 failed attempts
- Soft-delete aware authentication
- Role-based claims in JWT
- FluentValidation on all auth requests

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

## Milestone 3 – Patients, Doctors & Availability (Completed)

### Patients Endpoints (`/api/v1/patients`)

| Method | Endpoint       | Roles                     | Description                    |
|--------|----------------|---------------------------|--------------------------------|
| POST   | /              | Admin, Receptionist       | Create patient profile         |
| GET    | /{id}          | Authenticated             | Get patient by ID              |
| GET    | /              | Admin, Receptionist, Doctor | List + search + pagination  |
| PUT    | /{id}          | Admin, Receptionist       | Update patient                 |
| DELETE | /{id}          | Admin                     | Soft-delete patient            |

### Doctors Endpoints (`/api/v1/doctors`)

| Method | Endpoint              | Roles              | Description                          |
|--------|-----------------------|--------------------|--------------------------------------|
| POST   | /                     | Admin              | Create doctor profile                |
| GET    | /{id}                 | Public             | Get doctor + availability            |
| GET    | /                     | Public             | List + filter by specialty + search  |
| PUT    | /{id}                 | Admin, Doctor      | Update doctor                        |
| DELETE | /{id}                 | Admin              | Soft-delete doctor                   |
| PUT    | /{id}/availability    | Admin, Doctor      | Replace weekly availability slots    |

### Typical Flow

1. Register a user with role `Doctor` or `Patient` via `/api/v1/auth/register`
2. Create the corresponding profile (`/api/v1/doctors` or `/api/v1/patients`) using the `userId`
3. For doctors, set weekly availability via `PUT /api/v1/doctors/{id}/availability`

Example availability payload:
```json
[
  { "dayOfWeek": "Monday", "startTime": "09:00:00", "endTime": "12:00:00", "isActive": true },
  { "dayOfWeek": "Monday", "startTime": "14:00:00", "endTime": "17:00:00", "isActive": true },
  { "dayOfWeek": "Wednesday", "startTime": "09:00:00", "endTime": "13:00:00", "isActive": true }
]
```

## Milestone 4 – Appointments (Completed)

### Endpoints (`/api/v1/appointments`)

| Method | Endpoint                  | Roles                          | Description                              |
|--------|---------------------------|--------------------------------|------------------------------------------|
| POST   | /                         | Admin, Receptionist, Patient   | Book appointment (double-booking safe)   |
| GET    | /{id}                     | Authenticated                  | Get appointment details                  |
| GET    | /                         | Admin, Receptionist, Doctor    | List + filter (patient/doctor/status/date)|
| GET    | /available-slots          | Public                         | Get free slots for a doctor on a date    |
| PUT    | /{id}/reschedule          | Admin, Receptionist, Patient   | Reschedule                               |
| POST   | /{id}/cancel              | Admin, Receptionist, Patient, Doctor | Cancel                          |
| PATCH  | /{id}/status              | Admin, Receptionist, Doctor    | Advance status (status machine)          |

### Key Features

- **Double-booking protection**: overlap check + optimistic concurrency
- **Availability-aware**: only books inside doctor’s weekly schedule
- **Status machine** (enforced transitions):
  - Scheduled → Confirmed / Cancelled / NoShow
  - Confirmed → CheckedIn / Cancelled / NoShow
  - CheckedIn → InProgress / NoShow
  - InProgress → Completed
- Available slots generator respects duration + existing bookings
- Soft-delete aware, full audit columns

### Typical Booking Flow

1. `GET /api/v1/appointments/available-slots?doctorId=...&date=2026-10-05`
2. `POST /api/v1/appointments` with chosen `startTime`
3. Later: `PATCH /api/v1/appointments/{id}/status` to move through the workflow

## Milestone 5 – Clinical Notes + Billing (Completed)

### Clinical Notes (`/api/v1/notes`)

SOAP-format consultation notes attached 1:1 to appointments.

| Method | Endpoint                          | Roles                | Description                    |
|--------|-----------------------------------|----------------------|--------------------------------|
| POST   | /                                 | Admin, Doctor        | Create note                    |
| GET    | /by-appointment/{appointmentId}   | Admin, Doctor, Receptionist | Get note by appointment |
| PUT    | /{id}                             | Admin, Doctor        | Update note                    |

Notes can only be created when the appointment is `CheckedIn`, `InProgress`, or `Completed`.

### Billing (`/api/v1/billing`)

| Method | Endpoint                    | Roles                | Description                          |
|--------|-----------------------------|----------------------|--------------------------------------|
| POST   | /invoices                   | Admin, Receptionist  | Generate invoice from appointment    |
| GET    | /invoices/{id}              | Admin, Receptionist, Doctor | Get invoice                   |
| GET    | /invoices                   | Admin, Receptionist  | List + filter by status / patient    |
| POST   | /invoices/{id}/pay          | Admin, Receptionist  | Mark invoice as paid                 |

- Invoice amount is taken from the doctor’s `ConsultationFee`
- Optional tax rate (0–1)
- Statuses: Pending → Paid (also supports Cancelled / Refunded checks)

### End-to-end clinic flow now supported

1. Register users (Doctor / Patient / Receptionist / Admin)
2. Create Doctor + Patient profiles
3. Set doctor availability
4. Book appointment (double-booking safe)
5. Move appointment through status machine
6. Add SOAP consultation note
7. Generate invoice → mark as paid

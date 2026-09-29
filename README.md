# CarePulse - Digital Health Clinic API

Enterprise-grade backend API that digitizes multi-doctor private health clinic operations, including appointment scheduling, patient records, clinical documentation, and billing workflows.

**Tech Stack:** ASP.NET Core 9 · C# · PostgreSQL · Entity Framework Core · Redis · Hangfire · JWT Authentication · ASP.NET Identity · Clean Architecture · CQRS (MediatR) · FluentValidation · Serilog · Swagger

---

## Features

- JWT Authentication with Refresh Tokens
- Multi-role Access Control (Admin, Doctor, Receptionist, Patient)
- Patient & Doctor Management
- Doctor Availability Scheduling
- Appointment Booking with Double-Booking Protection
- Appointment Status Workflow
- SOAP Clinical Notes
- Billing & Invoice Management
- Background Processing with Hangfire
- Soft Deletes & Audit Tracking
- Structured Logging & Health Checks
- Global Exception Handling
- Standardized API Responses
- Docker Support

---

## Prerequisites

- .NET 9 SDK
- Docker & Docker Compose (Recommended)
- PostgreSQL 16
- Redis 7

---

## Quick Start (Docker)

```bash
cd docker
docker compose up -d --build
```

### Available Resources

| Resource | URL |
|-----------|-----|
| API | http://localhost:8080 |
| Swagger | http://localhost:8080/swagger |
| Hangfire Dashboard | http://localhost:8080/hangfire |
| Health Check | http://localhost:8080/health |

---

## Local Development

### Start Infrastructure

```bash
docker compose -f docker/docker-compose.yml up -d postgres redis
```

### Apply Database Migrations

```bash
dotnet ef migrations add InitialCreate \
  --project src/CarePulse.Infrastructure \
  --startup-project src/CarePulse.Api

dotnet ef database update \
  --project src/CarePulse.Infrastructure \
  --startup-project src/CarePulse.Api
```

### Run the API

```bash
dotnet run --project src/CarePulse.Api
```

> When running locally, verify the API URL in `launchSettings.json`.

---

## Project Structure

```text
src/
├── CarePulse.Api              # Presentation Layer
├── CarePulse.Application      # CQRS, DTOs, Validators, Use Cases
├── CarePulse.Domain           # Entities, Enums, Business Rules
├── CarePulse.Infrastructure   # EF Core, Identity, Redis, Hangfire
└── CarePulse.Shared           # Shared Utilities & Result Types
```

---

## Architecture

- Clean Architecture / Onion Architecture
- CQRS with MediatR
- SOLID Design Principles
- Optimistic Concurrency Control
- Soft Deletes with Audit Trail
- Role-Based Access Control (RBAC)
- Structured Logging with Correlation IDs
- Global Exception Handling
- Consistent JSON Response Envelope

---

## Standard API Response

### Success

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

### Failure

```json
{
  "success": false,
  "message": "Validation failed.",
  "errors": [
    "Email is required"
  ],
  "timestamp": "2026-09-29T14:00:00Z",
  "requestId": "0HN..."
}
```

---

## Authentication

**Base Route:** `/api/v1/auth`

| Method | Endpoint | Access | Description |
|----------|----------|----------|-------------|
| POST | `/register` | Public | Register a new user |
| POST | `/login` | Public | Login and receive tokens |
| POST | `/refresh` | Public | Refresh access token |
| POST | `/logout` | Authenticated | Invalidate refresh token |
| GET | `/me` | Authenticated | Get current user |

### Roles

- Admin
- Doctor
- Receptionist
- Patient

### Registration Example

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

### Security Features

- 15-Minute JWT Access Tokens
- 7-Day Rotating Refresh Tokens
- Strong Password Policy
- Account Lockout After 5 Failed Attempts
- Role Claims in JWT
- Soft Delete Aware Authentication

---

## Patient Management

**Base Route:** `/api/v1/patients`

| Method | Endpoint | Roles |
|----------|----------|---------|
| POST | `/` | Admin, Receptionist |
| GET | `/{id}` | Authenticated |
| GET | `/` | Admin, Receptionist, Doctor |
| PUT | `/{id}` | Admin, Receptionist |
| DELETE | `/{id}` | Admin |

---

## Doctor Management

**Base Route:** `/api/v1/doctors`

| Method | Endpoint | Roles |
|----------|----------|---------|
| POST | `/` | Admin |
| GET | `/{id}` | Public |
| GET | `/` | Public |
| PUT | `/{id}` | Admin, Doctor |
| DELETE | `/{id}` | Admin |
| PUT | `/{id}/availability` | Admin, Doctor |

### Availability Example

```json
[
  {
    "dayOfWeek": "Monday",
    "startTime": "09:00:00",
    "endTime": "12:00:00",
    "isActive": true
  },
  {
    "dayOfWeek": "Monday",
    "startTime": "14:00:00",
    "endTime": "17:00:00",
    "isActive": true
  }
]
```

---

## Appointment Management

**Base Route:** `/api/v1/appointments`

| Method | Endpoint | Roles |
|----------|----------|---------|
| POST | `/` | Admin, Receptionist, Patient |
| GET | `/{id}` | Authenticated |
| GET | `/` | Admin, Receptionist, Doctor |
| GET | `/available-slots` | Public |
| PUT | `/{id}/reschedule` | Admin, Receptionist, Patient |
| POST | `/{id}/cancel` | Admin, Receptionist, Patient, Doctor |
| PATCH | `/{id}/status` | Admin, Receptionist, Doctor |

### Appointment Status Workflow

```text
Scheduled
 ├─→ Confirmed
 │    ├─→ CheckedIn
 │    │    └─→ InProgress
 │    │         └─→ Completed
 │    └─→ Cancelled
 ├─→ Cancelled
 └─→ NoShow
```

### Booking Flow

```http
GET /api/v1/appointments/available-slots?doctorId={id}&date=2026-10-05

POST /api/v1/appointments

PATCH /api/v1/appointments/{id}/status
```

---

## Clinical Notes

**Base Route:** `/api/v1/notes`

SOAP-format consultation notes linked to appointments.

| Method | Endpoint | Roles |
|----------|----------|---------|
| POST | `/` | Admin, Doctor |
| GET | `/by-appointment/{appointmentId}` | Admin, Doctor, Receptionist |
| PUT | `/{id}` | Admin, Doctor |

> Notes can only be created when an appointment is in **CheckedIn**, **InProgress**, or **Completed** status.

---

## Billing

**Base Route:** `/api/v1/billing`

| Method | Endpoint | Roles |
|----------|----------|---------|
| POST | `/invoices` | Admin, Receptionist |
| GET | `/invoices/{id}` | Admin, Receptionist, Doctor |
| GET | `/invoices` | Admin, Receptionist |
| POST | `/invoices/{id}/pay` | Admin, Receptionist |

### Invoice Rules

- Amount is derived from the doctor's consultation fee.
- Optional tax rate (`0.0 - 1.0`).
- Status flow:

```text
Pending → Paid
```

---

## Typical End-to-

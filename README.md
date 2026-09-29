# CarePulse – Digital Health Clinic API

Enterprise-grade backend API for digitizing multi-doctor private clinic operations, including scheduling, patient records, clinical notes, and billing.

**Tech Stack:** ASP.NET Core 9 · C# · PostgreSQL · EF Core · Redis · Hangfire · JWT + Identity · Clean Architecture · CQRS (MediatR) · FluentValidation · Serilog · Swagger

---

## Features

* Multi-role authentication: Admin, Doctor, Receptionist, Patient
* JWT authentication with rotating refresh tokens
* Patient and Doctor management
* Doctor availability scheduling
* Appointment booking with double-booking protection
* Optimistic concurrency for appointments
* Appointment status workflow
* SOAP-format clinical notes
* Billing and invoice management
* Background jobs with Hangfire
* Soft deletes and audit columns
* Structured logging
* Health checks
* Global exception handling
* Consistent API response envelope
* Role-based access control (RBAC)
* Docker-ready

---

## Prerequisites

* [.NET 9 SDK](https://dotnet.microsoft.com/)
* [Docker](https://www.docker.com/) & Docker Compose *(recommended)*
* PostgreSQL 16
* Redis 7

> PostgreSQL and Redis can be run automatically using Docker Compose.

---

## Quick Start with Docker

From the project root:

```bash
cd docker
docker compose up -d --build
```

Once the containers are running:

| Resource           | URL                            |
| ------------------ | ------------------------------ |
| API                | http://localhost:8080          |
| Swagger            | http://localhost:8080/swagger  |
| Hangfire Dashboard | http://localhost:8080/hangfire |
| Health Check       | http://localhost:8080/health   |

---

## Local Development

### 1. Start Infrastructure

Start PostgreSQL and Redis:

```bash
docker compose -f docker/docker-compose.yml up -d postgres redis
```

### 2. Apply EF Core Migrations

```bash
dotnet ef migrations add InitialCreate \
  --project src/CarePulse.Infrastructure \
  --startup-project src/CarePulse.Api

dotnet ef database update \
  --project src/CarePulse.Infrastructure \
  --startup-project src/CarePulse.Api
```

### 3. Run the API

```bash
dotnet run --project src/CarePulse.Api
```

> When running with `dotnet run`, check the console output for the exact URL. The port comes from `launchSettings.json` and may not be `8080`.

---

## Project Structure

```text
src/
├── CarePulse.Api
│   └── Controllers, Middleware, Program.cs
│
├── CarePulse.Application
│   └── Use cases, MediatR handlers, Validators, DTOs
│
├── CarePulse.Domain
│   └── Entities, Enums, Business rules
│
├── CarePulse.Infrastructure
│   └── EF Core, Identity, Redis, Hangfire
│
└── CarePulse.Shared
    └── Shared result types and utilities
```

---

## Architecture

CarePulse follows **Clean Architecture / Onion Architecture** principles.

Key architectural practices include:

* SOLID
* DRY
* KISS
* YAGNI
* CQRS with MediatR
* Soft deletes
* Audit trails
* Optimistic concurrency
* Global exception handling
* Structured logging with correlation IDs
* Role-based access control (RBAC)

---

## Standard API Response Format

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

# API Overview

Base URL:

```text
/api/v1
```

## Authentication

**Base Route:** `/api/v1/auth`

| Method | Endpoint    | Auth   | Description                    |
| ------ | ----------- | ------ | ------------------------------ |
| POST   | `/register` | Public | Register a new user            |
| POST   | `/login`    | Public | Login and receive tokens       |
| POST   | `/refresh`  | Public | Refresh access token           |
| POST   | `/logout`   | Bearer | Invalidate refresh token       |
| GET    | `/me`       | Bearer | Get current authenticated user |

### Roles

* Admin
* Doctor
* Receptionist
* Patient

Roles are seeded automatically on startup.

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

### Security

* JWT access tokens: 15 minutes
* Rotating refresh tokens: 7 days
* Strong password policy
* Account lockout after 5 failed attempts
* Soft-delete-aware authentication
* Role claims included in JWT

---

# Patients

**Base Route:** `/api/v1/patients`

| Method | Endpoint | Roles                       | Description                        |
| ------ | -------- | --------------------------- | ---------------------------------- |
| POST   | `/`      | Admin, Receptionist         | Create patient profile             |
| GET    | `/{id}`  | Authenticated               | Get patient by ID                  |
| GET    | `/`      | Admin, Receptionist, Doctor | List, search and paginate patients |
| PUT    | `/{id}`  | Admin, Receptionist         | Update patient                     |
| DELETE | `/{id}`  | Admin                       | Soft-delete patient                |

---

# Doctors

**Base Route:** `/api/v1/doctors`

| Method | Endpoint             | Roles         | Description                 |
| ------ | -------------------- | ------------- | --------------------------- |
| POST   | `/`                  | Admin         | Create doctor profile       |
| GET    | `/{id}`              | Public        | Get doctor and availability |
| GET    | `/`                  | Public        | List/filter doctors         |
| PUT    | `/{id}`              | Admin, Doctor | Update doctor               |
| DELETE | `/{id}`              | Admin         | Soft-delete doctor          |
| PUT    | `/{id}/availability` | Admin, Doctor | Replace weekly availability |

Doctors can be filtered by specialty and searched by name.

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
  },
  {
    "dayOfWeek": "Wednesday",
    "startTime": "09:00:00",
    "endTime": "13:00:00",
    "isActive": true
  }
]
```

---

# Appointments

**Base Route:** `/api/v1/appointments`

| Method | Endpoint           | Roles                                | Description                  |
| ------ | ------------------ | ------------------------------------ | ---------------------------- |
| POST   | `/`                | Admin, Receptionist, Patient         | Book appointment             |
| GET    | `/{id}`            | Authenticated                        | Get appointment details      |
| GET    | `/`                | Admin, Receptionist, Doctor          | List and filter appointments |
| GET    | `/available-slots` | Public                               | Get available doctor slots   |
| PUT    | `/{id}/reschedule` | Admin, Receptionist, Patient         | Reschedule appointment       |
| POST   | `/{id}/cancel`     | Admin, Receptionist, Patient, Doctor | Cancel appointment           |
| PATCH  | `/{id}/status`     | Admin, Receptionist, Doctor          | Advance appointment status   |

### Appointment Status Machine

```text
Scheduled
   ├── Confirmed
   ├── Cancelled
   └── NoShow

Confirmed
   ├── CheckedIn
   ├── Cancelled
   └── NoShow

CheckedIn
   ├── InProgress
   └── NoShow

InProgress
   └── Completed
```

### Booking Flow

```text
GET /available-slots?doctorId=...&date=2026-10-05
        ↓
POST / with selected startTime
        ↓
PATCH /{id}/status
        ↓
Move appointment through the workflow
```

Appointments include protection against double booking and optimistic concurrency conflicts.

---

# Clinical Notes

**Base Route:** `/api/v1/notes`

Clinical notes use the **SOAP** format and are attached one-to-one with appointments.

| Method | Endpoint                          | Roles                       | Description             |
| ------ | --------------------------------- | --------------------------- | ----------------------- |
| POST   | `/`                               | Admin, Doctor               | Create clinical note    |
| GET    | `/by-appointment/{appointmentId}` | Admin, Doctor, Receptionist | Get note by appointment |
| PUT    | `/{id}`                           | Admin, Doctor               | Update clinical note    |

Clinical notes can only be created when the appointment is:

* `CheckedIn`
* `InProgress`
* `Completed`

---

# Billing

**Base Route:** `/api/v1/billing`

| Method | Endpoint             | Roles                       | Description                       |
| ------ | -------------------- | --------------------------- | --------------------------------- |
| POST   | `/invoices`          | Admin, Receptionist         | Generate invoice from appointment |
| GET    | `/invoices/{id}`     | Admin, Receptionist, Doctor | Get invoice                       |
| GET    | `/invoices`          | Admin, Receptionist         | List and filter invoices          |
| POST   | `/invoices/{id}/pay` | Admin, Receptionist         | Mark invoice as paid              |

### Billing Rules

* Invoice amount comes from the doctor's `ConsultationFee`
* Optional tax rate: `0–1`
* Invoice statuses:

  * `Pending`
  * `Paid`

---

# Typical End-to-End Flow

```text
1. Register users
   ├── Doctor
   ├── Patient
   ├── Receptionist
   └── Admin

2. Create Doctor and Patient profiles

3. Set Doctor availability

4. Book appointment
   └── Double-booking protection

5. Move appointment through status workflow

6. Add SOAP consultation note

7. Generate invoice

8. Mark invoice as paid
```

---

## Infrastructure

CarePulse uses the following infrastructure components:

| Component             | Purpose                        |
| --------------------- | ------------------------------ |
| PostgreSQL            | Primary relational database    |
| Redis                 | Caching and distributed data   |
| Hangfire              | Background job processing      |
| ASP.NET Core Identity | User and role management       |
| JWT                   | API authentication             |
| Serilog               | Structured application logging |
| Swagger               | API documentation              |

---

## Development Principles

The project is designed around maintainability, separation of concerns, and production-ready backend practices.

```text
Clean Architecture
        +
CQRS / MediatR
        +
Domain-driven business rules
        +
Role-based security
        +
Validation
        +
Structured logging
        +
Global error handling
        +
Optimistic concurrency
        =
Maintainable Clinic API
```

---

## License

This project is currently intended for development and demonstration purposes.

# CarePulse – Digital Health Clinic API

Enterprise-grade backend API that digitizes multi-doctor private health clinic operations: scheduling, patient records, clinical notes, and billing.

**Stack:** ASP.NET Core 9 · C# · PostgreSQL · EF Core · Redis · Hangfire · JWT + Identity · Clean Architecture + CQRS (MediatR) · FluentValidation · Serilog · Swagger

---

## Features

- Multi-role authentication (Admin, Doctor, Receptionist, Patient) with JWT + refresh tokens
- Patient & Doctor management
- Doctor availability scheduling
- Appointment booking with double-booking protection and optimistic concurrency
- Appointment status machine (Scheduled → Confirmed → CheckedIn → InProgress → Completed)
- Clinical notes (SOAP format)
- Billing / invoices
- Background jobs via Hangfire
- Soft deletes + full audit columns
- Structured logging, health checks, global exception handling
- Consistent API response envelope
- Docker-ready

---

## Prerequisites

- .NET 9 SDK
- Docker & Docker Compose (recommended)
- PostgreSQL 16 + Redis 7 (or use Docker Compose)

---

## Quick Start with Docker

```bash
cd docker
docker compose up -d --build

Resource,URL
API,http://localhost:8080
Swagger,http://localhost:8080/swagger
Hangfire Dashboard,http://localhost:8080/hangfire
Health,http://localhost:8080/health

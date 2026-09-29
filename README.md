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
Once running:

























ResourceURLAPIhttp://localhost:8080Swaggerhttp://localhost:8080/swaggerHangfire Dashboardhttp://localhost:8080/hangfireHealthhttp://localhost:8080/health

Local Development

Start infrastructure:

Bashdocker compose -f docker/docker-compose.yml up -d postgres redis

Apply migrations:

Bashdotnet ef migrations add InitialCreate --project src/CarePulse.Infrastructure --startup-project src/CarePulse.Api
dotnet ef database update --project src/CarePulse.Infrastructure --startup-project src/CarePulse.Api

Run the API:

Bashdotnet run --project src/CarePulse.Api
When running with dotnet run, check the console for the exact URL (port comes from launchSettings.json, not necessarily 8080).

Project Structure
textsrc/
├── CarePulse.Api              # Presentation (Controllers, Middleware, Program.cs)
├── CarePulse.Application      # Use cases, MediatR handlers, Validators, DTOs
├── CarePulse.Domain           # Entities, Enums, Business rules
├── CarePulse.Infrastructure   # EF Core, Identity, Redis, Hangfire
└── CarePulse.Shared           # Shared result types and utilities

Architecture

Clean Architecture / Onion
SOLID, DRY, KISS, YAGNI
CQRS via MediatR
Soft deletes + audit trail
Optimistic concurrency on appointments
Global exception handling → consistent JSON envelope
Structured logging with correlation IDs
Role-based access control (RBAC)


Standard Response Format
Success
JSON{
  "success": true,
  "message": "User created successfully.",
  "data": {},
  "meta": {},
  "timestamp": "2026-09-29T14:00:00Z",
  "requestId": "0HN..."
}
Failure
JSON{
  "success": false,
  "message": "Validation failed.",
  "errors": ["Email is required"],
  "timestamp": "2026-09-29T14:00:00Z",
  "requestId": "0HN..."
}

API Overview
Authentication (/api/v1/auth)









































MethodEndpointAuthDescriptionPOST/registerPublicRegister new user with rolePOST/loginPublicLogin and receive tokensPOST/refreshPublicRefresh access tokenPOST/logoutBearerInvalidate refresh tokenGET/meBearerGet current authenticated user
Roles: Admin, Doctor, Receptionist, Patient (seeded on startup)
Register example
JSON{
  "email": "doctor@clinic.com",
  "password": "SecureP@ssw0rd1",
  "firstName": "Jane",
  "lastName": "Smith",
  "role": "Doctor",
  "phoneNumber": "+1234567890"
}
Security

JWT access tokens (15 min) + rotating refresh tokens (7 days)
Strong password policy
Account lockout after 5 failed attempts
Soft-delete aware authentication
Role claims in JWT


Patients (/api/v1/patients)









































MethodEndpointRolesDescriptionPOST/Admin, ReceptionistCreate patient profileGET/{id}AuthenticatedGet patient by IDGET/Admin, Receptionist, DoctorList + search + paginationPUT/{id}Admin, ReceptionistUpdate patientDELETE/{id}AdminSoft-delete patient

Doctors (/api/v1/doctors)















































MethodEndpointRolesDescriptionPOST/AdminCreate doctor profileGET/{id}PublicGet doctor + availabilityGET/PublicList + filter by specialty + searchPUT/{id}Admin, DoctorUpdate doctorDELETE/{id}AdminSoft-delete doctorPUT/{id}/availabilityAdmin, DoctorReplace weekly availability slots
Availability example
JSON[
  { "dayOfWeek": "Monday", "startTime": "09:00:00", "endTime": "12:00:00", "isActive": true },
  { "dayOfWeek": "Monday", "startTime": "14:00:00", "endTime": "17:00:00", "isActive": true },
  { "dayOfWeek": "Wednesday", "startTime": "09:00:00", "endTime": "13:00:00", "isActive": true }
]

Appointments (/api/v1/appointments)





















































MethodEndpointRolesDescriptionPOST/Admin, Receptionist, PatientBook appointment (double-booking safe)GET/{id}AuthenticatedGet appointment detailsGET/Admin, Receptionist, DoctorList + filtersGET/available-slotsPublicFree slots for a doctor on a datePUT/{id}/rescheduleAdmin, Receptionist, PatientReschedulePOST/{id}/cancelAdmin, Receptionist, Patient, DoctorCancelPATCH/{id}/statusAdmin, Receptionist, DoctorAdvance status
Status machine
textScheduled → Confirmed / Cancelled / NoShow
Confirmed → CheckedIn / Cancelled / NoShow
CheckedIn → InProgress / NoShow
InProgress → Completed
Booking flow

GET /available-slots?doctorId=...&date=2026-10-05
POST / with chosen startTime
PATCH /{id}/status to move through the workflow


Clinical Notes (/api/v1/notes)
SOAP-format notes attached 1:1 to appointments.





























MethodEndpointRolesDescriptionPOST/Admin, DoctorCreate noteGET/by-appointment/{appointmentId}Admin, Doctor, ReceptionistGet note by appointmentPUT/{id}Admin, DoctorUpdate note
Notes can only be created when the appointment is CheckedIn, InProgress, or Completed.

Billing (/api/v1/billing)



































MethodEndpointRolesDescriptionPOST/invoicesAdmin, ReceptionistGenerate invoice from appointmentGET/invoices/{id}Admin, Receptionist, DoctorGet invoiceGET/invoicesAdmin, ReceptionistList + filter by status / patientPOST/invoices/{id}/payAdmin, ReceptionistMark invoice as paid

Amount comes from the doctor’s ConsultationFee
Optional tax rate (0–1)
Statuses: Pending → Paid


Typical End-to-End Flow

Register users (Doctor / Patient / Receptionist / Admin)
Create Doctor + Patient profiles
Set doctor availability
Book appointment (double-booking safe)
Move appointment through the status machine
Add SOAP consultation note
Generate invoice → mark as paid


License
MIT – built for portfolio demonstration.

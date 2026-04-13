# School Application (School Assessment API)

ASP.NET Core **Web API** for managing school structure (classes, sections, students), **JWT authentication**, **teacher section assignments**, **marks** (queued asynchronous processing), and **exam rankings**. Data is stored in **SQL Server** via **Entity Framework Core**.

## Documentation (design & assignment)

All design and interview-style documentation is under **[`docs/`](./docs/README.md)**:

- [Design & architecture](./docs/DESIGN.md)
- [Data model](./docs/DATA-MODEL.md)
- [API reference](./docs/API-REFERENCE.md)
- [Assumptions & limitations](./docs/ASSUMPTIONS-AND-LIMITATIONS.md)
- [Testing & quality](./docs/TESTING-AND-QUALITY.md)
- [Submission checklist](./docs/SUBMISSION-CHECKLIST.md)

## Requirements

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (the project targets `net8.0`; `RollForward` may allow a newer host runtime—see `SchoolApplication.csproj` comments.)
- **SQL Server** or **LocalDB** (default connection string uses `(localdb)\mssqllocaldb`).
- A database that matches the EF model (`SchoolAssessment` by default in `appsettings.json`). This repository does not ship EF migrations; align your database with `SchoolAssessmentContext` / your environment’s schema.

## Configuration

Edit `SchoolApplication/appsettings.json` (or use environment variables / user secrets for secrets):

| Setting | Purpose |
|--------|---------|
| `ConnectionStrings:SchoolAssessment` | SQL Server connection string |
| `Jwt:SigningKey` | Symmetric key for access tokens (**minimum 32 characters**; required at startup) |
| `Jwt:Issuer` / `Jwt:Audience` | JWT validation claims |
| `Jwt:AccessTokenMinutes` / `Jwt:RefreshTokenDays` | Token lifetimes |

**Serilog** is configured from the same file; logs go to the console.

## Run the API

From the repository root:

```bash
cd SchoolApplication
dotnet run
```

- **Development**: Swagger UI is available (typically `http://localhost:<port>/swagger`).
- **Health check** (no auth): `GET /api/health`

HTTPS redirection is **disabled in Development** so Swagger over HTTP works without a trusted dev certificate.

## Authentication

- `POST /api/auth/login` — returns access + refresh tokens.
- `POST /api/auth/refresh` — rotate refresh token.
- `POST /api/auth/logout` — revoke refresh token.

Most endpoints require `Authorization: Bearer <access_token>`.

### Roles

Role names must match the database (`AppRoles`):

- **Admin** — full user/student/section management.
- **Teacher** — assigned sections only (marks submission and section marks read where enforced in services).
- **Student** — profile/marks/rankings when the student record is linked to the login (see below).

User provisioning is done through **`/api/users`** (Admin), including `POST /api/users/teachers` and teacher section assignment via **`PUT /api/teachers/{teacherUserId}/sections`**.

## Identity vs enrollment

- **`AppUsers`** — login accounts (username, password hash, roles).
- **`Students`** — enrollment records (section, name, admission number, marks). Optional **`Students.UserId`** links a student row to an **`AppUsers`** account so **`GET /api/me/marks`** and rankings work for that login. Admins link accounts with **`PUT /api/students/{id}/linked-user`** (or full student update with `userId`).

## Main API areas

| Area | Base route | Notes |
|------|------------|--------|
| Auth | `/api/auth` | Anonymous |
| Health | `/api/health` | Anonymous |
| Current user | `/api/me` | Profile (`/profile`); students: `/marks`, `/rankings` |
| Classes | `/api/classes` | Admin + Teacher: list/get; **Admin only**: create/update/delete |
| Sections | `/api/...` | Nested under classes in `SectionsController` |
| Students | `/api/students` | **Admin** for CRUD and by-section listing |
| Users | `/api/users` | Admin user management |
| Teachers | `/api/teachers` | Admin: section assignments |
| Exams / Subjects | `/api/exams`, `/api/subjects` | Reference data |
| Marks | `/api/marks` | Submit marks; read section marks |
| Jobs | `/api/jobs/{jobId}` | Marks processing job status (Admin/Teacher) |
| Rankings | `/api/rankings` | Section/class rankings |

Exact authorization per action is defined on each controller; use Swagger or the source for details.

## Marks workflow

1. **Submit** — `POST /api/marks/submissions` (Admin/Teacher). Body: `examId` + list of `{ studentId, subjectId, score? }`.  
   - Header **`Idempotency-Key`** is **required** (unique per submission intent).  
   - Returns **202 Accepted** with a **job id** (async processing).
2. **Status** — `GET /api/jobs/{jobId}` until completed or failed.
3. **Read** — `GET /api/marks/sections/{sectionId}?examId=` for a section grid (Admin/Teacher with access).

A **hosted background service** (`MarkProcessingWorker`) processes pending mark jobs.

## Project layout

- `SchoolApplication/` — web host, controllers, services, validators, middleware.
- `SchoolApplication/Models/` — EF entities and `SchoolAssessmentContext`.

## Logging and diagnostics

- Request logging and **correlation IDs** are enabled (`CorrelationIdMiddleware`).
- Global exception handling returns problem details for API errors.

## License

Specify your organization’s license here if applicable.

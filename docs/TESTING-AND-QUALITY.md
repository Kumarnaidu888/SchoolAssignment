# Testing and quality

## 1. Manual verification (quick)

1. **Configure** `SchoolApplication/appsettings.json` (connection string + `Jwt:SigningKey` length ≥ 32).
2. **Run:** `cd SchoolApplication` then `dotnet run`.
3. **Health:** `GET /api/health` — expect `200` with JSON body.
4. **Swagger (Development):** open `/swagger`, authenticate via **Authorize** using a Bearer token from **`POST /api/auth/login`**.
5. **Happy path ideas:**
   - Admin creates class → section → student; creates teacher user; assigns sections via `PUT /api/teachers/{id}/sections`.
   - Teacher submits marks with **`Idempotency-Key`** header → **202** → poll **`GET /api/jobs/{jobId}`** until terminal state.
   - Link student user → **`GET /api/me/marks`** as Student token.

---

## 2. Automated testing strategy (recommended next steps)

This section describes what a reviewer would expect you to add if the assignment scope included tests.

### Unit tests

| Target | Examples |
|--------|----------|
| **FluentValidation** | `SubmitMarksRequestValidator` — empty marks, score bounds. |
| **Pure logic** | Any extracted ranking or scoring helpers (if refactored for testability). |

### Integration tests

| Target | Examples |
|--------|----------|
| **Auth** | Login returns tokens; refresh rotates; invalid credentials **401**. |
| **Authorization** | Student cannot call admin-only routes (**403**). |
| **Marks pipeline** | Submit marks → worker processes job → mark row exists (may require test doubles or shorter polling timeouts in test config). |

**Tooling:** `Microsoft.AspNetCore.Mvc.Testing` (`WebApplicationFactory`), optional **Testcontainers** for SQL Server, or a **SQLite** provider only if the model is compatible (often not worth the drift for SQL Server-specific features).

### API contract tests

- Generate or snapshot **OpenAPI** and detect breaking changes in CI.

---

## 3. Quality practices already present

- **Global exception handler** with Problem Details.
- **Structured logging** (Serilog) and **correlation IDs** on requests.
- **Input validation** via FluentValidation auto-validation.

---

## 4. Related documents

- [API-REFERENCE.md](./API-REFERENCE.md) — endpoints to hit during manual tests.
- [SUBMISSION-CHECKLIST.md](./SUBMISSION-CHECKLIST.md) — what to attach or mention for the assignment.

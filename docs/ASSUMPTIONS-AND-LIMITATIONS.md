# Assumptions and limitations

Use this document in an interview submission to show you understand **scope** and **trade-offs**.

## Assumptions

1. **SQL Server** is available (including **LocalDB** for local dev, as in the default connection string).
2. The **database schema** already exists and matches the EF model (`SchoolAssessmentContext`). This repository does **not** ship EF migrations; provisioning is environment-specific.
3. **Role names** in the database (`Admin`, `Teacher`, `Student`) match `SchoolApplication.Security.AppRoles`.
4. **JWT signing key** is configured with at least **32 characters** before the host starts (`Program.cs` validation).
5. Clients that submit marks can generate and persist a unique **`Idempotency-Key`** per submission attempt when retries are possible.

---

## Limitations (current codebase)

| Area | Limitation |
|------|------------|
| **Tests** | No automated test project is included in this repository; see [TESTING-AND-QUALITY.md](./TESTING-AND-QUALITY.md). |
| **Teacher discovery** | Teachers discover section ids via admin assignment APIs or UI knowledge; there is no dedicated `GET /api/me/assigned-sections` in the current API surface. |
| **Student roster for teachers** | `GET /api/students/by-section/{id}` is **Admin-only**; teachers may rely on **`GET /api/marks/sections/{sectionId}`** (includes student names and marks) to drive grids. |
| **Real-time marks** | Marks are **eventually consistent** until the background job completes. |
| **Horizontal scale** | Background job processing is in-process; multiple API instances would need a coordinated outbox / queue pattern to avoid duplicate processing (not implemented here). |
| **HTTPS in Development** | HTTPS redirection is disabled in Development to simplify Swagger over HTTP when dev certificates are not trusted. |

---

## Out of scope (typical for a focused assignment)

- Full front-end application.
- Email / password reset flows.
- File uploads (e.g. bulk CSV import), though the API supports bulk JSON marks.
- Internationalization and multi-tenant school isolation beyond a single database.

---

## Suggested future work

- Add **integration tests** (WebApplicationFactory + Testcontainers SQL Server).
- Add **teacher self-service** endpoints for assigned sections and rosters (if product requires).
- **CI pipeline** (build + test + optional DB migration step).
- **OpenAPI export** checked into `docs/openapi.json` on release tags.

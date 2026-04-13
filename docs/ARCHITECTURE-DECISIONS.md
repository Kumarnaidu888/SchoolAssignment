# Architecture decisions (summary)

Lightweight ADR-style notes for reviewers. Full narrative: [DESIGN.md](./DESIGN.md).

| ID | Decision | Status | Rationale |
|----|----------|--------|-----------|
| ADR-1 | JWT bearer access tokens + stored refresh tokens | Accepted | Stateless API; refresh supports rotation and logout per device. |
| ADR-2 | Role names stored in DB, mirrored in `AppRoles` constants | Accepted | Simple RBAC; aligns with SQL schema and policies. |
| ADR-3 | Marks applied via `MarkProcessingJob` + background worker | Accepted | Decouples HTTP from bulk DB work; enables retries and idempotency. |
| ADR-4 | Required `Idempotency-Key` on mark submission | Accepted | Safe client retries without duplicate mark application. |
| ADR-5 | EF Core with reverse-engineered context (Power Tools) | Accepted | Fast alignment with existing SQL schema; migrations not in repo. |
| ADR-6 | `AppUsers` separate from `Students` with optional `UserId` | Accepted | Enrollment exists without login; student portal only when linked. |
| ADR-7 | Teacher access scoped by `TeacherSections` | Accepted | Least privilege for mark read/submit paths in services. |

## Revisit in a larger product

- **Outbox / message broker** for mark jobs if multiple API instances run.
- **Centralized authorization policies** (e.g. resource-based) if rules grow beyond role + section checks.

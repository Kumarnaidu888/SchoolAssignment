# Assignment submission checklist

Use this when packaging the project for an **interview** or **academic submission**.

## Include in your submission

- [ ] **Source code** — full repository or zip (excluding `bin/`, `obj/` if the recipient prefers a clean restore).
- [ ] **Root README** — [`README.md`](../README.md) with how to run and configure.
- [ ] **Design documentation** — this **`docs/`** folder (all linked from [`docs/README.md`](./README.md)).

## Optional but strong additions

- [ ] Short **cover note** (email or PDF) summarizing:
  - What you built (1 paragraph).
  - How to run (3–5 bullet points).
  - **Two design highlights** (e.g. async marks + idempotency; AppUsers vs Students).
  - **One trade-off** you would revisit in production.
- [ ] **Swagger export** — save `swagger/v1/swagger.json` from a running dev instance as `docs/openapi-v1.json` (optional; not created by default here).
- [ ] **Screen recording** or screenshots of Swagger executing login + one marks flow.

## What to say in an interview

1. **Problem framing:** School assessment domain: structure, auth, marks, rankings.
2. **Architecture:** ASP.NET Core layered API, EF Core, background worker for marks.
3. **Security:** JWT, roles, teacher scoped by `TeacherSections`.
4. **Reliability:** Idempotent mark submissions; job retries and logs.
5. **Honesty:** Call out items under [ASSUMPTIONS-AND-LIMITATIONS.md](./ASSUMPTIONS-AND-LIMITATIONS.md) and how you would extend tests.

## Internal links (for reviewers)

| Doc | Path |
|-----|------|
| Documentation index | `docs/README.md` |
| System design | `docs/DESIGN.md` |
| Data model | `docs/DATA-MODEL.md` |
| API index | `docs/API-REFERENCE.md` |
| Assumptions | `docs/ASSUMPTIONS-AND-LIMITATIONS.md` |
| Testing | `docs/TESTING-AND-QUALITY.md` |

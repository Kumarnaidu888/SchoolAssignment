# API reference

Base URL is the host where the API runs (e.g. `https://localhost:7xxx` or `http://localhost:5xxx`).

Unless noted, responses are **JSON**. Errors use **Problem Details** (`application/problem+json`) where configured.

**Authentication:** send `Authorization: Bearer <access_token>` for protected endpoints.

**OpenAPI:** In **Development**, Swagger UI exposes the full interactive contract at `/swagger`.

---

## Anonymous

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/health` | Liveness probe |
| POST | `/api/auth/login` | Login → access + refresh tokens |
| POST | `/api/auth/refresh` | Rotate refresh token |
| POST | `/api/auth/logout` | Revoke refresh token |

---

## Authenticated (any valid JWT)

| Method | Path | Roles / notes |
|--------|------|----------------|
| GET | `/api/me/profile` | Any authenticated user; includes `studentPortal` when linked |
| GET | `/api/exams` | Any authenticated |
| GET | `/api/subjects` | Any authenticated |

---

## Student

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/me/marks?examId=` | Own marks (**404** if not linked to `Students`) |
| GET | `/api/me/rankings?examId=` | Own section rank (**404** / **204** as documented) |

---

## Admin only

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/users` | List users |
| GET | `/api/users/{userId}` | Get user |
| POST | `/api/users` | Create user with roles |
| POST | `/api/users/teachers` | Create teacher account |
| PUT | `/api/users/{userId}/roles` | Replace roles |
| GET | `/api/teachers/{teacherUserId}/sections` | List teacher’s section assignments |
| PUT | `/api/teachers/{teacherUserId}/sections` | Replace section assignments |
| GET | `/api/students/by-section/{sectionId}` | Students in section |
| GET | `/api/students/{studentId}` | Student by id |
| POST | `/api/students` | Create student |
| PUT | `/api/students/{studentId}` | Update student |
| PUT | `/api/students/{studentId}/linked-user` | Link student login |
| DELETE | `/api/students/{studentId}` | Delete student |
| POST | `/api/classes` | Create class |
| PUT | `/api/classes/{classId}` | Update class |
| DELETE | `/api/classes/{classId}` | Delete class |
| POST | `/api/classes/{classId}/sections` | Create section |
| PUT | `/api/sections/{sectionId}` | Update section |
| DELETE | `/api/sections/{sectionId}` | Delete section |

---

## Admin + Teacher

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/classes` | List classes |
| GET | `/api/classes/{classId}` | Get class |
| GET | `/api/classes/{classId}/sections` | List sections in class |
| GET | `/api/sections/{sectionId}` | Get section |
| POST | `/api/marks/submissions` | Queue marks (**see below**) |
| GET | `/api/marks/sections/{sectionId}?examId=` | Section marks grid |
| GET | `/api/jobs/{jobId}` | Mark job status |
| GET | `/api/rankings/sections/{sectionId}?examId=` | Section rankings |
| GET | `/api/rankings/classes/{classId}?examId=` | Class rankings |
| GET | `/api/rankings/top?examId=&scope=&scopeId=&n=` | Top N in scope |

---

## Marks submission (important)

**`POST /api/marks/submissions`**

- **Headers:** `Idempotency-Key` — **required**, unique per logical submit.
- **Body (conceptual):** `{ "examId": number, "marks": [ { "studentId", "subjectId", "score": number | null } ] }`
- **Response:** **202 Accepted** with job reference; poll **`GET /api/jobs/{jobId}`**.
- **Rules:** Teachers may only include students in **sections they are assigned to**; Admins unrestricted (subject to validation in service).

Score validation is defined in `SubmitMarksRequestValidator` (e.g. range when provided).

---

## Rankings query parameters

- **`GET /api/rankings/top`:** `scope` must be **`Section`** or **`Class`** (case-insensitive); `scopeId` is the section id or class id respectively; `examId` and optional `n` (default 10).

---

## Further detail

DTO shapes and status codes are best viewed in **Swagger** or the XML comments on controller actions in `SchoolApplication/Controllers/`.

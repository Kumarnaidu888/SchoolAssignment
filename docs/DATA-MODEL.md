# Data model

This document reflects the **Entity Framework** model in `SchoolApplication/Models` and schema configuration in `SchoolAssessmentContext`.

## 1. Schema namespaces (SQL Server)

| Schema | Tables (logical) |
|--------|-------------------|
| **auth** | `AppUsers`, `Roles`, `UserRoles`, `RefreshTokens` |
| **core** | `Classes`, `Sections`, `Students`, `Exams`, `Subjects`, `Marks`, ranking tables, teacher assignments |

Exact table names match EF `ToTable(...)` mappings in `SchoolAssessmentContext`.

---

## 2. Core entities (conceptual)

| Entity | Description |
|--------|-------------|
| **Class** | A school class level or cohort (e.g. “Grade 10”). |
| **Section** | A subdivision of a class (e.g. “10-A”); students belong to exactly one section. |
| **Student** | Enrollment record: name, admission number, `SectionId`, optional `UserId` → portal link. |
| **Exam** | Exam type / display metadata for assessments. |
| **Subject** | Subject catalog for marks. |
| **Mark** | One score per **(StudentId, ExamId, SubjectId)** with concurrency token and optional `EnteredByUserId`. |
| **TeacherSection** | Many-to-many style link: which teacher user is assigned to which section. |

---

## 3. Auth entities

| Entity | Description |
|--------|-------------|
| **AppUser** | Login account: username, email, password hash, active flag. |
| **Role** | Named role (`Admin`, `Teacher`, `Student` — must match app constants). |
| **UserRoles** | Join between users and roles. |
| **RefreshToken** | Stored refresh token for rotation / logout. |

---

## 4. Async processing entities

| Entity | Description |
|--------|-------------|
| **MarkProcessingJob** | Queued work: `IdempotencyKey`, `Status`, JSON `PayloadJson`, retry fields. |
| **JobAttemptLog** | Audit trail of job attempts. |

---

## 5. Ranking entities

| Entity | Description |
|--------|-------------|
| **RankingSnapshot** | Header for a computed ranking for a scope (section/class) and exam. |
| **RankingRow** | Per-student rank line within a snapshot. |

---

## 6. ER diagram (high level)

```mermaid
erDiagram
  Class ||--o{ Section : contains
  Section ||--o{ Student : enrolls
  AppUser ||--o| Student : "optional UserId"
  Student ||--o{ Mark : receives
  Exam ||--o{ Mark : for
  Subject ||--o{ Mark : for
  AppUser ||--o{ Mark : "EnteredByUserId"
  AppUser }o--o{ Role : UserRoles
  AppUser ||--o{ TeacherSection : assigned
  Section ||--o{ TeacherSection : assigned
  MarkProcessingJob ||--o{ JobAttemptLog : logs
```

---

## 7. Important relationships

- **`Student.SectionId`** → student’s home section (used for teacher access checks and rankings).
- **`Mark`** uniqueness / updates are handled in application logic and concurrency (`RowVersion`), not necessarily exposed as a single DB constraint in this model—see services and processor.
- **`Students.UserId`** when set: that **AppUser** (typically Student role) can use **`GET /api/me/marks`** and rankings via the “me” portal services.

---

## 8. Identity vs enrollment (interview note)

- **`AppUsers`** = *who can authenticate*.
- **`Students`** = *who exists in the school academic model*.
- Linking them is **optional** until you need the student self-service experience.

See [DESIGN.md](./DESIGN.md) for how this flows through the API.

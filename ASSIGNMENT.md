# Midterm: RoomHub (study room bookings)

**Duration: 70 minutes** = **60 minutes of coding** (AI allowed) + **10 minutes of paper quiz**, no laptop (see "Grading" and "AI rules").

## What you have to do

The project in `src/RoomHub.Api` is an API that **works**, but it is organized in layers
(`Endpoints`, `Services`, `Repositories`, `Models`), with anemic models and business errors as exceptions.

Your task is to transform it into an application in the style taught in the course (lectures 1–6):

1. **Vertical slice architecture**: one folder per feature (`Features/...`).
2. **Rich domain**: the rules live in the domain classes, not in endpoints or services.
3. **`Result<T>` and `Error`** instead of exceptions for business errors.
4. **CQRS handlers** (`ICommandHandler`) and **thin endpoints**.
5. **Persistence with EF Core + SQLite**, through **Repository + Unit of Work**.

Then: create a **public Git repository** with the result and send the link **before the quiz**.

> **Warning:** the starter is NOT guaranteed to be correct. If anything in the code contradicts
> the **API contract** or the **business rules** below, the document is right.

---

## API contract (do not change it, there are automated tests on it)

The format is JSON, with camelCase field names. Dates are in UTC, in ISO 8601 format
(for example `2026-12-01T10:00:00Z`). Enums appear in JSON as text.

| Operation | Request | Success response |
|---|---|---|
| Create room | `POST /api/rooms` with `{ "name": "...", "capacity": 10 }` | `201`, `Location` header, body `{ id, name, capacity }` |
| Get room | `GET /api/rooms/{id}` | `200` with `{ id, name, capacity }` |
| Create booking | `POST /api/rooms/{roomId}/bookings` with `{ "bookedBy": "...", "startUtc": "...", "endUtc": "...", "attendees": 4 }` | `201`, `Location` header, body `{ id, roomId, status }` |
| Get booking | `GET /api/bookings/{id}` | `200` with `{ id, roomId, bookedBy, startUtc, endUtc, attendees, status }` |
| Cancel booking | `POST /api/bookings/{id}/cancel` | `204` |

`status` is either `Active` or `Cancelled`.

Errors are returned as `ProblemDetails` (`application/problem+json`) with the right HTTP code:
**400** for validation, **404** for a missing resource, **409** for a conflict.
When the room does not exist, 404 is returned before any validation.

## Business rules

- **R1.** A room has a non-empty name and a strictly positive capacity.
- **R2.** `bookedBy` is required.
- **R3.** `endUtc` must be strictly after `startUtc`.
- **R4.** `attendees` is strictly positive and at most the room capacity.
- **R5.** A booking cannot start in the past. "Now" comes from `TimeProvider`,
  not from `DateTime.UtcNow` / `DateTime.Now`.
- **R6.** The maximum duration of a booking is 4 hours (exactly 4 hours is allowed).
- **R7.** Two **active** bookings of the same room cannot overlap (409).
  Bookings that only touch (one ends exactly when the other starts) do not overlap.
- **R8.** Cancelling moves a booking from `Active` to `Cancelled`. Cancelling an already
  cancelled booking gives 409. Cancelling a missing one gives 404. A cancelled booking frees its interval.

---

## Grading

Code grade = points / 10 (minimum 1). The maximum is 100 points.
The final grade also includes the quiz (see below).

| Level | Points | What we check | Max grade |
|---|---|---|---|
| **A** | 50 | The project compiles (5). `Features/` structure with at least 5 slices, each in its own folder (12). Basic contract tests: create/get room and booking, validations R1–R4, the R7 conflict (30). At least 3 commits during the exam (3). | 5 |
| **B** | +20 | Rules R5 and R6 (6). Domain without public setters (5). `Result<T>` instead of business exceptions (5). `ICommandHandler` handlers and thin endpoints (4). | 7 |
| **C** | +20 | EF Core + SQLite, data survives a restart (9). Repository and Unit of Work as interfaces in the domain (3). Rules R7 (touching intervals) and R8 correct (8). | 9 |
| **D** | +10 | A new requirement you receive during the exam (4). `TimeProvider` used correctly, no `DateTime.UtcNow` (2). The files written by you: `DECISIONS.md`, `BUGS.md`, `AI-LOG.md` (4). | 10 |

Cap rules:
- If `dotnet build` fails, the maximum grade is **4**.
- If the link does not lead to a **public** repository, or is missing, the grade is **1**.

The code is graded mostly automatically (your application is cloned, built, started and called
over HTTP), plus a check of the code structure and a reading of the files written by you.

### The 10-minute quiz (no laptop)

At the end of the 70 minutes you solve a paper quiz with **10 questions** from lectures 1–6,
about the same ideas you used in the project (`Result`, Repository + Unit of Work,
vertical slice, `TimeProvider`, HTTP statuses, intervals). It cannot be solved with AI.

**Final grade = 0.7 × code grade + 0.3 × quiz grade** (quiz grade = number of correct answers,
minimum 1), with these caps:

| Correct answers on the quiz | Maximum final grade |
|---|---|
| 7–10 | 10 |
| 5–6 | 8 |
| 0–4 | 6 |

So a perfectly submitted project that you do not understand does not go above 6.

---

## What you submit

A **public** Git repository with:

- your code in `src/` (do not commit `bin/`, `obj/`, `*.db`; the `.gitignore` is already there);
- `DECISIONS.md`, `BUGS.md`, `AI-LOG.md` filled in **by you, in your own words**.
  The templates are in the root of the repo. Do not paste them from AI: you may be asked about them;
- **at least 3 commits** made during the exam (not a single commit at the end).

Send the repository link at minute 60, before the quiz. Anything committed after minute 60 is not counted.

## AI rules

- You may use AI for code, during the 60 minutes. Write in `AI-LOG.md` what you asked and what you changed.
- The quiz at the end is solved without a laptop, without a phone, without AI.
- You write `DECISIONS.md`, `BUGS.md` and `AI-LOG.md` yourself. That means you type them,
  in your own words, referring to the files and classes of your own project.
- You may not get help from other colleagues.

## Time tips (approximate)

| Minutes | What you do |
|---|---|
| 0–5 | Clone, run, read the contract and the rules |
| 5–22 | Level A: move the code into `Features/`, check with `roomhub.http` |
| 22–40 | Level B: rich domain, `Result`, handlers |
| 40–54 | Level C: EF Core + repository |
| 54–60 | `DECISIONS.md`, `BUGS.md`, push, send the link |
| 60–70 | Paper quiz, no laptop |

A new requirement is announced at minute 35. Commit often. Run the application after each step.

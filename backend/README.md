# Todo API

ASP.NET Core Web API targeting .NET 10. Single project, SQLite persistence, minimal API endpoints.

---

## Setup

**Requires:** .NET 10 SDK

```bash
cd backend/TodoApi
dotnet run
```

The API starts at `http://localhost:5000`. On first run, EF Core creates `todos.db` in the project directory and applies the schema migration automatically. Data persists across restarts.

To run the tests:

```bash
cd backend
dotnet test
```

Interactive API docs (Scalar) are at `http://localhost:5000/scalar` in development mode.

---

## Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/todos` | Create a todo. Title required, max 200 characters. |
| GET | `/api/todos` | List active todos. Optional `?status=Pending` or `?status=Completed` filter. |
| PUT | `/api/todos/{id}/complete` | Mark a todo as complete. |
| PUT | `/api/todos/{id}/title` | Update a todo's title. Same validation rules as create. |
| DELETE | `/api/todos/{id}` | Archive (soft-delete) a todo. |

Errors follow RFC 7807 Problem Details: `400` for validation failures, `404` when an item doesn't exist, `409` when a domain rule is violated (completing an already-completed item, updating an archived item, etc.).

---

## Architecture

Single ASP.NET Core project. EF Core is used directly through `TodoDbContext`, no repository layer.

`TodoItem.cs` is the domain model. Methods like `Complete`, `UpdateTitle`, and `Archive` enforce business rules and throw `InvalidOperationException` on violations. The endpoints catch those and return 409.

`TodoDbContext.cs` maps `TodoItem` to a `todo_items` SQLite table. Schema is defined in `OnModelCreating`.

`TodoEndpoints.cs` registers all five endpoints under an `api/todos` group. Validation runs inline before touching the database.

`Program.cs` wires up DI, applies pending migrations on startup, enables CORS for `http://localhost:5173` (the React dev server), and mounts Scalar in development.

The delete operation is a soft delete. `IsArchived = true` removes an item from all list responses but the row stays in the database. I prefer to preserve data by default and run a cleanup job later rather than delete immediately.

---

## Authentication

There is no authentication. Every request can read and modify every todo.

This was a deliberate scope decision. JWT auth with per-user data isolation would have taken most of the available time on its own. The API works correctly for a local demo without it, but this is the biggest gap between the current state and something shippable.

---

## Tests

`TodoItemTests.cs` covers the domain model directly: creation defaults, state transitions, and the invariants that prevent invalid operations.

`TodoApiTests.cs` uses `WebApplicationFactory` with an in-memory SQLite connection to test the full HTTP layer. It covers:

- Valid creation returns 201
- Empty title returns 400
- Whitespace-only title returns 400
- Title over 200 characters returns 400
- Invalid `?status=` value returns 400
- Completing a todo that doesn't exist returns 404
- Completing a todo twice returns 409
- Archiving an already-archived todo returns 409
- Updating the title on a completed todo returns 409

33 tests total, all passing.

There are no ownership tests because there is no auth layer. Once auth is added, those come first: verifying that User A cannot read or modify User B's data through any API call.

---

## Trade-offs and assumptions

This is a todo app with five endpoints and one entity. Every decision came down to the same question: does this layer exist because the problem needs it, or just because it's the kind of thing you'd expect to see? A repository pattern, a CQRS split, a validation pipeline, a mediator bus - each of those is a real tool for a real problem. None of those problems exist here. Adding them anyway would not make the code better. It would make it harder to read, slower to change, and misleading about what the app actually does. The design is simple because the problem is simple.

**No repository layer.** The endpoints talk to `TodoDbContext` directly. A repository abstraction would only have one implementation here and would add indirection without benefit. If the data layer needed to swap between SQLite and a different store, or if multiple features needed to share complex query logic, a repository would earn its place. It doesn't here.

**Inline validation instead of a validation library.** FluentValidation would be reasonable for a larger API. For five endpoints with one validated field, it's overhead. The rules live in the endpoint handlers where they're easy to read and test. Validation runs before any database call, so invalid requests never produce I/O.

**Soft delete over hard delete.** Archiving sets `IsArchived = true` rather than removing the row. This is a conservative default: it's easy to add a cleanup job later, and it avoids the case where a user deletes something by mistake with no recovery path. The trade-off is that the database grows over time until cleanup runs.

**SQLite for persistence.** SQLite is more than enough for a local demo and keeps setup to a single command. The EF Core migration works against any relational database, so moving to PostgreSQL later is a configuration change, not a rewrite. The only real constraint is that SQLite doesn't handle high write concurrency, which isn't a concern here.

---

## Scalability notes

The current design is intentionally simple, but most of it scales without structural changes:

The SQLite database would be swapped for PostgreSQL or SQL Server. The EF Core setup is the same; only the connection string and provider package change.

The `GET /api/todos` endpoint returns all matching rows. That's fine for a personal todo list. At scale it needs cursor-based pagination and an index on `(is_archived, status, created_at_utc)`.

The biggest structural shift for a multi-tenant production service would be moving from a shared SQLite file to a proper database server with connection pooling. Everything above that layer stays the same.

Reads and writes are already on separate endpoints (`GET` vs `POST`/`PUT`/`DELETE`). If read load became a bottleneck, you could give the `GetTodosAsync` handler a read-replica connection string without touching any of the write handlers. Moving toward explicit CQRS with separate read and write models would formalize that split, but the current endpoint separation already makes the path straightforward.

What would actually need work before scaling: connection pooling configuration, a real database with indexes on status and is_archived for the list queries, HTTP response caching on the read endpoints, and a load balancer in front.

---

## What I'd build next

**Real database.** Swap SQLite for PostgreSQL with Npgsql. The EF Core setup is already relational-ready; it just needs the provider package and a connection string. EF migrations are already in place, so schema changes from that point forward are tracked and repeatable.

**Authentication and per-user data.** JWT-based auth, a `UserId` column on `TodoItem`, all queries filtered by the current user's identity. ASP.NET Core's built-in auth middleware handles most of the token plumbing without much ceremony. Once that's in, the first tests added should be the ownership checks: verifying that User A cannot read or modify User B's data through any API call.

**Due dates.** Stored as UTC, exposed as UTC in the API, converted to local time in the frontend. Would add a `DueDate` column to `TodoItem`, a validation rule that the date can't be in the past on creation, and a filter option on `GET /api/todos`.

**Pagination.** `GET /api/todos` returns everything. Cursor-based pagination is the right approach over offset because it handles rows inserted between pages correctly and works better with indexes.

**Domain events.** `Complete()` and `Archive()` change state but don't notify anything outside the model. In a real system you'd raise domain events (`TodoItemCompleted`, `TodoItemArchived`) and handle side effects separately, like sending a notification or updating a read model. Nothing in the current domain model blocks adding that.

**Background jobs.** Purging archived items after a retention period doesn't belong in the request pipeline. A hosted service or a tool like Hangfire would handle that cleanly. This also closes the trade-off with the soft delete: data is preserved by default, and the cleanup runs on a schedule.

**Structured logging.** Serilog with a middleware that stamps a correlation ID on every log entry so all lines from a single request can be tied together in production.

# Function Health — Technical Exercise

Full-stack todo application built as a technical exercise. The backend is an ASP.NET Core Web API and the frontend is a React + TypeScript app.

## Structure

```
backend/    ASP.NET Core API (.NET 10)
frontend/   React + TypeScript app (Vite)
```

Each folder has its own README with setup instructions, architecture notes, and design decisions:

- [backend/README.md](backend/README.md) — API setup, domain model, endpoint reference, assumptions, scalability notes
- [frontend/README.md](frontend/README.md) — frontend setup, component structure, testing

## Quick start

Run the backend first, then the frontend.

```bash
# backend
cd backend/Todo.Api
dotnet run

# frontend (separate terminal)
cd frontend/todoApp
npm install
npm run dev
```

Backend runs at `http://localhost:5000`. Frontend runs at `http://localhost:5173`.

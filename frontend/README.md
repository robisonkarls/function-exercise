# Todo App (frontend)

React + TypeScript frontend for the technical exercise. Built with Vite, styled with Tailwind CSS, and talks to the ASP.NET Core backend over plain axios calls.

---

## Setup and running

**Prerequisites:** Node.js 18+

```bash
cd frontend/todoApp
npm install
npm run dev
```

The app runs at `http://localhost:5173`. The backend must be running and `VITE_API_BASE_URL` must point to it (defaults to `http://localhost:5000` — copy `.env.example` to `.env` if needed).

To build for production:

```bash
npm run build
```

Output goes to `dist/`. You can preview it with `npm run preview`.

---

## Structure

State lives in `App.tsx`. There's no global state manager; the todo array, loading flag, and error string all live in a single `useState` calls at the top and get passed down as props. It's a small enough app that this is fine.

```
src/
  App.tsx               state + API calls
  components/
    Header.tsx          title and the "Send offer" button
    TodoForm.tsx        new todo input
    TodoFilter.tsx      All / Pending / Completed filter tabs
    TodoList.tsx        list rendering + TodoItem (inline edit)
    Button.tsx          reusable button with three style variants
  models/
    ITodos.tsx          ITodo interface
```

Each component gets exactly the props it needs. `TodoList` takes the list, loading state, and the three action handlers. Nothing knows about axios except `App.tsx`.

---

## Features

- Create, complete, and archive todos
- Filter by status (All, Pending, Completed)
- Inline title editing: click a pending todo's title to edit, press Enter to save or Escape to cancel
- Relative timestamps ("updated 3 minutes ago") for recently changed items, full date otherwise
- Loading and error states on every async operation

---

## Trade-offs and assumptions

**No state management library.** `useState` in `App.tsx` is sufficient here. If the app grew to need shared state across unrelated parts of the tree, I'd add Zustand or React Query, but adding that now would be over-engineering it.

**API base URL via env var.** The backend URL is configured through `VITE_API_BASE_URL` in `.env` (defaults to `http://localhost:5000`). Copy `.env.example` to `.env` to override for a different environment.

**No optimistic updates.** Every mutation waits for the server response before updating the UI. That means a brief delay on complete/archive, but it also means the displayed state always matches what the server actually has. For a todo app that tradeoff feels right.

**Error display with auto-dismiss.** Errors show for a couple of seconds then disappear. That works for simple cases but a more complete implementation would distinguish between recoverable and non-recoverable errors and handle them differently.

---

## What I'd add next

**React Query.** The current fetch-on-mount pattern works, but React Query would give you caching, background refetching, and cleaner loading/error states essentially for free.

**Optimistic updates.** For complete and archive, the outcome is predictable enough that you could update the UI immediately and roll back on failure. Makes the app feel faster.

**Toast notifications.** Right now errors appear inline and disappear. A toast system would let success messages show too ("Todo archived") without cluttering the layout permanently.

**Authentication.** Same note as the backend: the API is open, so any UI running on the right origin can read and mutate all todos. Adding auth would require login/logout UI and attaching a token to every request.

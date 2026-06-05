import Header from "./components/Header";
import { TodoForm } from "./components/TodoForm";
import { TodoList } from "./components/TodoList";
import { TodoFilter } from "./components/TodoFilter";
import { useEffect, useState } from "react";
import type { ITodo } from "./models/ITodos";
import axios from "axios";

const API_BASE = import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5000";

export default function App() {

  const [todos, setTodos] = useState<ITodo[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [mutationError, setMutationError] = useState<string | null>(null);

  useEffect(() => {
      axios.get<ITodo[]>(`${API_BASE}/api/todos`)
        .then((res) => { setTodos(res.data); setError(null); })
        .catch((err) => setError(err.message))
        .finally(() => setIsLoading(false));
  }, []);

  async function handleComplete(id: number) {
    setMutationError(null);
    try {
      const { data } = await axios.put<ITodo>(`${API_BASE}/api/todos/${id}/complete`);
      setTodos((prevTodos) => prevTodos.map((todo) => todo.id === id ? data : todo));
    } catch (err) {
      setMutationError(err instanceof Error ? err.message : "Request failed");
    }
  }

  async function handleArchive(id: number) {
    setMutationError(null);
    try {
      await axios.delete<ITodo>(`${API_BASE}/api/todos/${id}`);
      setTodos((prevTodos) => prevTodos.filter((todo) => todo.id !== id));
    } catch (err) {
      setMutationError(err instanceof Error ? err.message : "Request failed");
    }
  }

  async function handleUpdateTitle(id: number, title: string) {
    setMutationError(null);
    try {
      const { data } = await axios.put<ITodo>(`${API_BASE}/api/todos/${id}/title`, { title });
      setTodos((prevTodos) => prevTodos.map((todo) => todo.id === id ? data : todo));
    } catch (err) {
      setMutationError(err instanceof Error ? err.message : "Request failed");
    }
  }

  async function handleFilter(status: "All" | "Pending" | "Completed") {
    setIsLoading(true);
    setError(null);
    setMutationError(null);
    try {
        const url = status === "All"
          ? `${API_BASE}/api/todos`
          : `${API_BASE}/api/todos?status=${status}`;

        const { data } = await axios.get<ITodo[]>(url);
        setTodos(data);
    } catch (err) {
        setError(err instanceof Error ? err.message : "Unknown error");
    } finally {
        setIsLoading(false);
    }
  }

  return (
  <div className="max-w-2xl mx-auto p-4 flex flex-col gap-4">
      <Header />
      <TodoForm
        onAdd={(todo: ITodo) => setTodos((prevTodos) => [...prevTodos, todo])} />
      <TodoFilter onFilter={handleFilter} />
      {mutationError && (
        <p className="text-red-500 text-sm">{mutationError}</p>
      )}
      <TodoList
        todos={todos}
        isLoading={isLoading}
        error={error}
        onComplete={handleComplete}
        onArchive={handleArchive}
        onUpdateTitle={handleUpdateTitle} />
  </div>
  );
}

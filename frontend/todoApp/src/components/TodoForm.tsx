import { Button } from "./Button";
import type { ITodo } from "../models/ITodos";
import { useState, type FormEvent, useEffect } from "react";
import axios from "axios";

const API_BASE = import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5000";

interface TodoFormProps {
    onAdd: (todo: ITodo) => void;
}

export function TodoForm({onAdd}: TodoFormProps) {

    const [title, setTitle] = useState("");
    const [error, setError] = useState<string | null>(null);
    const [fading, setFading] = useState(false);

    useEffect(() => {
        if (!error) return;
        const faderTime = setTimeout(() => setFading(true), 2000);
        const clearTimer = setTimeout(() => { setError(null); setFading(false); }, 2000);
        return () => {
            clearTimeout(faderTime);
            clearTimeout(clearTimer);
        }
    }, [error]);

    async function handleSubmit(e: FormEvent) {
        e.preventDefault();

        if(!title.trim()) {
            setError("Title cannot be empty");
            return;
        }

        if(title.trim().length > 200) {
            setError("Title cannot exceed 200 characters");
            return;
        }

        try {
            const { data } = await axios.post<ITodo>(`${API_BASE}/api/todos`, { title });
            onAdd(data);
            setTitle("");
            setError(null);
        } catch (err) {
            setError(err instanceof Error ? err.message : "Unknown error");
        }
    }

    return (
    <div className="flex flex-col gap-1">
        <form className="flex gap-2" onSubmit={handleSubmit}>
            <input className="flex-1 rounded-lg bg-white text-zinc-800 px-4 py-2 
            focus:outline-none focus:ring-2 focus-visible:ring-2 focus-visible:ring-[#b05a36]" placeholder="What needs to be done?"
            value={title}
            onChange={(e) => setTitle(e.target.value)}
            maxLength={200}
            />
            <Button>Add Todo</Button>
        </form>
        {error && (
            <p className={`text-red-500 transition-opacity ${fading ? "opacity-0" : "opacity-100"}`}>{error}</p>
        )}    
    </div>
    );
}
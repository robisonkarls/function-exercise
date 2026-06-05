import { Button } from "./Button";
import { format, formatDistanceToNow, differenceInHours } from "date-fns";
import type { ITodo } from "../models/ITodos";
import { useState, useRef, useEffect } from "react";

interface TodoListProps {
    todos: ITodo[];
    isLoading: boolean;
    error: string | null;
    onComplete: (id: number) => void;
    onArchive: (id: number) => void;
    onUpdateTitle: (id: number, title: string) => void;
}
export function TodoList({todos, isLoading, error, onComplete, onArchive, onUpdateTitle }: TodoListProps) {
    
    if (isLoading) {
        return <p className="text-zinc-400">Loading...</p>;
    }

    if (error) {
        return <p className="text-red-500">Error: {error}</p>;
    }

    if (todos.length === 0) {
        return <p className="text-zinc-400">No todos available</p>;
    }

    return <div className="flex flex-col gap-3">
        {todos.map((todo) => (
            <TodoItem 
                key={todo.id} 
                todo={todo} 
                onComplete={onComplete}
                onArchive={onArchive}
                onUpdateTitle={onUpdateTitle} />
        ))}
    </div>    
}

function TodoItem({ todo, onComplete, onArchive, onUpdateTitle }: { todo: ITodo, onComplete: (id: number) => void, onArchive: (id: number) => void, onUpdateTitle: (id: number, title: string) => void }) {
    
    const variant = todo.status === "Pending" ? "Complete" : "Archive";
    const clickHandler = todo.status === "Pending" ? onComplete : onArchive;
    const isPending = todo.status === "Pending";
    const [isEditing, setIsEditing] = useState(false);
    const [editValue, setEditValue] = useState(todo.title);
    const inputRef = useRef<HTMLInputElement>(null);

    useEffect(() => {
        if (isEditing) inputRef.current?.focus();
    }, [isEditing]);

    useEffect(() => {
        setEditValue(todo.title);
    }, [todo.title]);

    function handleTitleClick() {
        if (!isPending) return;
        setIsEditing(true);
    }

    function commitEdit() {
        const trimmed = editValue.trim();
        if (trimmed && trimmed !== todo.title) {
            onUpdateTitle(todo.id, trimmed);
        } else {
            setEditValue(todo.title);
        }
        setIsEditing(false);
    }

    function handleKeyDown(e: React.KeyboardEvent<HTMLInputElement>) {
        if (e.key === "Enter") commitEdit();
        if (e.key === "Escape") { setEditValue(todo.title); setIsEditing(false); }
    }

    return <div className="flex items-center gap-4 rounded-xl bg-[#f9dcc8]/30 p-4 ring-1 ring-[#b05a36]/30">
        <div className={`self-stretch w-1 rounded-full flex-shrink-0 ${isPending ? "bg-[#b05a36]" : "bg-zinc-300"}`} />

        <div className="flex-1 min-w-0">
            {isEditing ? (
                <input
                    ref={inputRef}
                    className="font-semibold text-lg text-zinc-800 bg-white border border-[#b05a36] rounded px-1 w-full outline-none"
                    value={editValue}
                    onChange={(e) => setEditValue(e.target.value)}
                    onBlur={commitEdit}
                    onKeyDown={handleKeyDown}
                    maxLength={200}
                />
            ) : (
                <span
                    className={`font-semibold text-lg ${isPending ? "text-zinc-800 hover:bg-zinc-100 cursor-pointer" : "text-zinc-400 line-through"}`}
                    title={isPending ? "Click to edit" : undefined}
                    onClick={handleTitleClick}>
                    {todo.title}
                </span>
            )}
            <div className="flex items-center gap-3 mt-1">
                <span className="text-xs text-zinc-400">{format(new Date(todo.createdAtUtc), "PPP")}</span>
                {todo.updatedAtUtc && (
                    <span className="text-xs text-zinc-400">
                        · {differenceInHours(new Date(), new Date(todo.updatedAtUtc)) < 24
                            ? `updated ${formatDistanceToNow(new Date(todo.updatedAtUtc))} ago`
                            : `updated ${format(new Date(todo.updatedAtUtc), "PPP")}`}
                    </span>
                )}
            </div>
        </div>

        <span className={`text-base font-medium flex-shrink-0 ${
            isPending ? "text-[#b05a36]" : "text-zinc-400"
        }`}>
            {isPending ? "🕐 " : "✅ "}{todo.status}
        </span>

        <Button variant={variant} onClick={() => clickHandler(todo.id)}>{variant}</Button>
    </div>
}
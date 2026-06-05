import { useState } from "react";

type FilterStatus = "All" | "Pending" | "Completed";

interface TodoFilterProps {
    onFilter: (status: FilterStatus) => void;
}



export function TodoFilter({ onFilter }: TodoFilterProps) {
    const [activeFilter, setActiveFilter] = useState<FilterStatus>("All");

    function handleFilterClick(status: FilterStatus) {
        setActiveFilter(status);
        onFilter(status);
    }

    const baseClass = "px-3 py-1 rounded-full text-white transition";
    const activeClass = "bg-[#b05a36] hover:bg-[#c8683e]";
    const inactiveClass = "bg-gray-600 hover:bg-gray-300";

    return <div className="flex gap-2">
        <span className="text-sm text-zinc-500">Filter:</span>
        <button className={`${baseClass} ${activeFilter === "All" ? activeClass : inactiveClass}`} onClick={() => handleFilterClick("All")}>All</button>
        <button className={`${baseClass} ${activeFilter === "Pending" ? activeClass : inactiveClass}`} onClick={() => handleFilterClick("Pending")}>Pending</button>
        <button className={`${baseClass} ${activeFilter === "Completed" ? activeClass : inactiveClass}`} onClick={() => handleFilterClick("Completed")}>Completed</button>
    </div>
}
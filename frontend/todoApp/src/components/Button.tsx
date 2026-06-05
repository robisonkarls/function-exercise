import type { ComponentProps } from "react";
import { twMerge } from "tailwind-merge";

type ButtonVariant = "Pending" | "Complete" | "Archive" ;

type ButtonProps = {
    variant?: ButtonVariant
} & ComponentProps<"button">;

export function Button({variant = "Pending", ...props}: ButtonProps) {
    const variantStyles = getVariantStyles(variant);
    return <button {...props} className={twMerge(
        variantStyles,
        `px-4 py-2 text-white rounded-full font-medium tracking-wide shadow-sm cursor-pointer
         active:scale-95 active:shadow-none
         transition-all duration-150
         disabled:opacity-30 disabled:cursor-not-allowed`,
        props.className
    )}>{props.children}</button>
}

function getVariantStyles(variant: ButtonVariant) {
    switch (variant) {
        case "Pending":
            return "bg-[#b05a36] hover:bg-[#c8683e] hover:shadow-md";
        case "Archive":
            return "bg-zinc-500 hover:bg-zinc-400 hover:shadow-md";
        case "Complete":
            return "bg-[#b05a36] hover:bg-[#c8683e] hover:shadow-md";
        default:
            throw new Error(`Unknown variant: ${variant satisfies never}`);
    }
}
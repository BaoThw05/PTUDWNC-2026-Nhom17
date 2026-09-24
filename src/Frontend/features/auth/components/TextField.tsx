"use client";

import { useState, type InputHTMLAttributes } from "react";

type TextFieldProps = InputHTMLAttributes<HTMLInputElement> & {
  label: string;
  error?: string;
};

export function TextField({ label, error, id, type, ...inputProps }: TextFieldProps) {
  const [revealed, setRevealed] = useState(false);
  const inputId = id ?? inputProps.name;
  const errorId = error ? `${inputId}-error` : undefined;
  const isPassword = type === "password";

  return (
    <div className="flex flex-col gap-1.5">
      <label htmlFor={inputId} className="text-sm font-medium">
        {label}
      </label>
      <div className="relative">
        <input
          id={inputId}
          type={isPassword && revealed ? "text" : type}
          aria-invalid={Boolean(error)}
          aria-describedby={errorId}
          className={`w-full rounded-lg border border-black/15 bg-transparent px-3 py-2 outline-none transition focus:border-foreground focus:ring-2 focus:ring-foreground/15 aria-invalid:border-red-500 dark:border-white/20 ${isPassword ? "pr-16" : ""}`}
          {...inputProps}
        />
        {isPassword && (
          <button
            type="button"
            onClick={() => setRevealed((value) => !value)}
            aria-pressed={revealed}
            className="absolute inset-y-0 right-0 px-3 text-xs font-medium text-zinc-600 hover:text-foreground dark:text-zinc-400"
          >
            {revealed ? "Ẩn" : "Hiện"}
          </button>
        )}
      </div>
      {error && (
        <p id={errorId} className="text-sm text-red-600 dark:text-red-400">
          {error}
        </p>
      )}
    </div>
  );
}

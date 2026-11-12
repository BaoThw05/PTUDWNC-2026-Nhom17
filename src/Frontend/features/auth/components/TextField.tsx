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
          className={`w-full rounded-lg border border-(color:--auth-input-border) bg-(color:--auth-input-bg) px-3 py-2 text-(color:--auth-fg) placeholder:text-(color:--auth-subtle) shadow-sm outline-none transition focus:border-(color:--auth-fg) focus:ring-2 focus:ring-(color:--auth-focus) aria-invalid:border-(color:--auth-danger) ${isPassword ? "pr-16" : ""}`}
          {...inputProps}
        />
        {isPassword && (
          <button
            type="button"
            onClick={() => setRevealed((value) => !value)}
            aria-pressed={revealed}
            className="absolute inset-y-0 right-0 px-3 text-xs font-medium text-(color:--auth-muted) hover:text-(color:--auth-fg)"
          >
            {revealed ? "Ẩn" : "Hiện"}
          </button>
        )}
      </div>
      {error && (
        <p id={errorId} className="text-sm text-(color:--auth-danger)">
          {error}
        </p>
      )}
    </div>
  );
}

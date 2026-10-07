import type { ReactNode } from "react";

export function AuthCard({
  title,
  description,
  children,
  footer,
}: {
  title: string;
  description?: string;
  children: ReactNode;
  footer?: ReactNode;
}) {
  return (
    <section className="mx-auto flex w-full max-w-md flex-col gap-6 rounded-2xl border border-black/10 p-6 shadow-sm sm:p-8 dark:border-white/15">
      <header className="flex flex-col gap-1">
        <h1 className="text-2xl font-semibold tracking-tight">{title}</h1>
        {description && <p className="text-sm text-zinc-600 dark:text-zinc-400">{description}</p>}
      </header>
      {children}
      {footer && <footer className="text-sm text-zinc-600 dark:text-zinc-400">{footer}</footer>}
    </section>
  );
}

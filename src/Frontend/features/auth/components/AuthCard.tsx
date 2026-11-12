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
    <section className="mx-auto flex w-full max-w-md flex-col gap-6 rounded-2xl border border-(color:--auth-border) bg-(color:--auth-surface) text-(color:--auth-fg) p-6 shadow-sm sm:p-8">
      <header className="flex flex-col gap-1">
        <h1 className="text-2xl font-semibold tracking-tight">{title}</h1>
        {description && <p className="text-sm text-(color:--auth-muted)">{description}</p>}
      </header>
      {children}
      {footer && <footer className="text-sm text-(color:--auth-muted)">{footer}</footer>}
    </section>
  );
}

import type { ReactNode } from "react";

type PlaceholderPageProps = {
  title: string;
  owner: "TV1" | "TV2" | "TV3" | "TV4";
  requirements: string;
  children?: ReactNode;
};

/** Trang tạm cho route chưa được làm; người phụ trách thay bằng màn hình thật. */
export function PlaceholderPage({
  title,
  owner,
  requirements,
  children,
}: PlaceholderPageProps) {
  return (
    <section className="flex flex-col gap-4">
      <h1 className="text-3xl font-semibold tracking-tight">{title}</h1>
      <div className="rounded-lg border border-dashed border-amber-500/60 bg-amber-50 px-4 py-3 text-sm text-amber-900 dark:bg-amber-950/40 dark:text-amber-200">
        Đang phát triển — phụ trách: <strong>{owner}</strong> · {requirements}
      </div>
      {children}
    </section>
  );
}

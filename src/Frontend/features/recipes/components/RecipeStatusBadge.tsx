import type { RecipeStatus } from "../types";

export function RecipeStatusBadge({ status }: { status: RecipeStatus | number }) {
  if (status === "Published" || status === 1) {
    return (
      <span className="inline-flex items-center rounded-full bg-emerald-50 px-2.5 py-0.5 text-xs font-medium text-emerald-700 ring-1 ring-inset ring-emerald-600/20 dark:bg-emerald-950/50 dark:text-emerald-400">
        Đã xuất bản
      </span>
    );
  }
  if (status === "Archived" || status === 2) {
    return (
      <span className="inline-flex items-center rounded-full bg-amber-50 px-2.5 py-0.5 text-xs font-medium text-amber-700 ring-1 ring-inset ring-amber-600/20 dark:bg-amber-950/50 dark:text-amber-400">
        Đã lưu trữ
      </span>
    );
  }
  return (
    <span className="inline-flex items-center rounded-full bg-zinc-100 px-2.5 py-0.5 text-xs font-medium text-zinc-700 ring-1 ring-inset ring-zinc-500/20 dark:bg-zinc-800 dark:text-zinc-300">
      Bản nháp
    </span>
  );
}

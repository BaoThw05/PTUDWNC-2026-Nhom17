"use client";

import Link from "next/link";
import { useMyRecipes } from "../hooks/useMyRecipes";
import type { RecipeFilterTab } from "../types";
import { RecipeStatusBadge } from "./RecipeStatusBadge";

const STAT_TABS: { key: RecipeFilterTab; label: string }[] = [
  { key: "all", label: "Tổng công thức" },
  { key: "Published", label: "Đã xuất bản" },
  { key: "Draft", label: "Bản nháp" },
  { key: "Archived", label: "Đã lưu trữ" },
  { key: "trash", label: "Thùng rác" },
];

function StatCard({ tab, label }: { tab: RecipeFilterTab; label: string }) {
  const { data, isLoading, isError } = useMyRecipes(tab, 1, 1);
  const value = isLoading ? "…" : isError ? "—" : String(data?.totalCount ?? 0);

  return (
    <Link
      href="/dashboard/recipes"
      className="border border-black/10 dark:border-white/15 p-4 transition-colors hover:border-emerald-800 hover:bg-emerald-50 dark:hover:bg-emerald-950/40"
    >
      <p className="text-sm text-zinc-600 dark:text-zinc-400">{label}</p>
      <p className="mt-1 text-3xl font-semibold tabular-nums">{value}</p>
    </Link>
  );
}

export function DashboardOverview() {
  const recent = useMyRecipes("all", 1, 5);

  return (
    <div className="space-y-8">
      <div className="grid grid-cols-2 gap-3 sm:grid-cols-3 lg:grid-cols-5">
        {STAT_TABS.map((tab) => (
          <StatCard key={tab.key} tab={tab.key} label={tab.label} />
        ))}
      </div>

      <div className="flex flex-wrap gap-3">
        <Link
          href="/dashboard/recipes/new"
          className="rounded-full bg-emerald-800 px-5 py-2 text-sm font-medium text-white hover:bg-emerald-900"
        >
          + Tạo công thức
        </Link>
        <Link
          href="/dashboard/recipes"
          className="rounded-full border border-black/20 dark:border-white/25 px-5 py-2 text-sm font-medium hover:bg-black/5 dark:hover:bg-white/10"
        >
          Công thức của tôi
        </Link>
      </div>

      <section aria-labelledby="recent-recipes">
        <h2 id="recent-recipes" className="mb-3 text-lg font-semibold">
          Công thức gần đây
        </h2>
        {recent.isLoading && <p className="text-sm text-zinc-600 dark:text-zinc-400">Đang tải…</p>}
        {recent.isError && <p className="text-sm text-red-600 dark:text-red-400">Không tải được danh sách công thức.</p>}
        {recent.data && recent.data.items.length === 0 && (
          <p className="text-sm text-zinc-600 dark:text-zinc-400">Bạn chưa có công thức nào. Hãy tạo công thức đầu tiên.</p>
        )}
        {recent.data && recent.data.items.length > 0 && (
          <ul className="divide-y divide-black/10 dark:divide-white/15 border border-black/10 dark:border-white/15">
            {recent.data.items.map((recipe) => (
              <li key={recipe.id} className="flex items-center justify-between gap-4 px-4 py-3">
                <div className="min-w-0">
                  <p className="truncate font-medium">{recipe.title}</p>
                  <p className="text-xs text-zinc-600 dark:text-zinc-400">
                    Cập nhật {new Date(recipe.updatedAt).toLocaleDateString("vi-VN")}
                  </p>
                </div>
                <div className="flex shrink-0 items-center gap-3">
                  <RecipeStatusBadge status={recipe.status} />
                  <Link
                    href={`/dashboard/recipes/${recipe.id}/edit`}
                    className="text-sm font-medium text-emerald-800 dark:text-emerald-300 hover:underline"
                  >
                    Sửa
                  </Link>
                </div>
              </li>
            ))}
          </ul>
        )}
      </section>
    </div>
  );
}

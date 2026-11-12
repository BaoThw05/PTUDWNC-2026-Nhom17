import type { Metadata } from "next";
import { DashboardOverview } from "@/features/recipes/components/DashboardOverview";

export const metadata: Metadata = { title: "Bảng điều khiển" };

export default function DashboardPage() {
  return (
    <section className="space-y-6">
      <header className="border-b border-black/10 dark:border-white/15 pb-6">
        <p className="text-sm font-semibold text-emerald-800 dark:text-emerald-300">Khu vực tác giả</p>
        <h1 className="mt-2 text-3xl font-semibold tracking-tight">Bảng điều khiển</h1>
        <p className="mt-2 text-zinc-600 dark:text-zinc-400">Tổng quan công thức của bạn và lối tắt để chia sẻ món ăn mới.</p>
      </header>
      <DashboardOverview />
    </section>
  );
}

import type { Metadata } from "next";
import Link from "next/link";

export const metadata: Metadata = { title: "Bảng điều khiển" };

export default function DashboardPage() {
  return (
    <section className="space-y-6">
      <header className="border-b border-black/10 pb-6">
        <p className="text-sm font-semibold text-emerald-800">Khu vực tác giả</p>
        <h1 className="mt-2 text-3xl font-semibold tracking-tight">Bảng điều khiển</h1>
        <p className="mt-2 text-zinc-600">Quản lý công thức của bạn hoặc bắt đầu chia sẻ món ăn mới.</p>
      </header>
      <div className="grid gap-4 sm:grid-cols-2">
        <Link href="/dashboard/recipes" className="group border border-black/10 p-6 transition-colors hover:border-emerald-800 hover:bg-emerald-50">
          <h2 className="text-xl font-semibold group-hover:text-emerald-900">Công thức của tôi</h2>
          <p className="mt-2 text-sm text-zinc-600">Xem, chỉnh sửa và quản lý trạng thái các công thức đã tạo.</p>
          <span className="mt-5 inline-block text-sm font-semibold text-emerald-800">Mở danh sách →</span>
        </Link>
        <Link href="/dashboard/recipes/new" className="group border border-black/10 p-6 transition-colors hover:border-emerald-800 hover:bg-emerald-50">
          <h2 className="text-xl font-semibold group-hover:text-emerald-900">Tạo công thức</h2>
          <p className="mt-2 text-sm text-zinc-600">Thêm món ăn mới qua trình hướng dẫn từng bước.</p>
          <span className="mt-5 inline-block text-sm font-semibold text-emerald-800">Bắt đầu tạo →</span>
        </Link>
      </div>
    </section>
  );
}

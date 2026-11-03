import type { Metadata } from "next";
import Link from "next/link";
import { notFound } from "next/navigation";
import { listUsers } from "@/features/auth/api/backend";
import { UserAccessControls } from "@/features/auth/components/UserAccessControls";
import { ADMIN_USERS_PATH } from "@/features/auth/constants";
import { requireSession } from "@/features/auth/server";

export const metadata: Metadata = { title: "Quản lý người dùng" };

const ROLE_LABELS: Record<string, string> = { Admin: "Quản trị viên", Author: "Tác giả" };

const joinedDate = new Intl.DateTimeFormat("vi-VN", { dateStyle: "medium" });

function pageHref(search: string, page: number): string {
  const query = new URLSearchParams();
  if (search) query.set("search", search);
  if (page > 1) query.set("page", String(page));
  const text = query.toString();
  return text ? `${ADMIN_USERS_PATH}?${text}` : ADMIN_USERS_PATH;
}

export default async function AdminUsersPage({ searchParams }: PageProps<"/admin/users">) {
  const session = await requireSession(ADMIN_USERS_PATH);
  if (!session.user.roles.includes("Admin")) {
    notFound();
  }

  const params = await searchParams;
  const search = typeof params.search === "string" ? params.search.trim() : "";
  const page = Math.max(Number(params.page) || 1, 1);
  const result = await listUsers(session.accessToken, { search, page });

  return (
    <section className="flex w-full flex-col gap-6">
      <header className="flex flex-wrap items-end justify-between gap-4">
        <div>
          <h1 className="text-2xl font-semibold tracking-tight">Quản lý người dùng</h1>
          <p className="text-sm text-zinc-600 dark:text-zinc-400">
            {result.totalCount} tài khoản · khóa tài khoản sẽ đăng xuất người đó khỏi mọi thiết bị
          </p>
        </div>
        <form action={ADMIN_USERS_PATH} className="flex gap-2">
          <input
            type="search"
            name="search"
            defaultValue={search}
            placeholder="Email, tên đăng nhập, họ tên…"
            aria-label="Tìm người dùng"
            className="w-64 rounded-full border border-black/15 bg-transparent px-4 py-2 text-sm outline-none focus:border-foreground dark:border-white/20"
          />
          <button type="submit" className="rounded-full bg-foreground px-4 py-2 text-sm font-medium text-background">
            Tìm
          </button>
        </form>
      </header>

      <div className="overflow-x-auto rounded-2xl border border-black/10 dark:border-white/15">
        <table className="w-full text-left text-sm">
          <thead className="bg-black/[.03] text-xs whitespace-nowrap uppercase tracking-wide text-zinc-500 dark:bg-white/[.04]">
            <tr>
              <th className="px-4 py-3 font-medium">Người dùng</th>
              <th className="px-4 py-3 font-medium">Vai trò</th>
              <th className="px-4 py-3 font-medium">Trạng thái</th>
              <th className="px-4 py-3 font-medium">Tham gia</th>
              <th className="px-4 py-3 font-medium sr-only">Thao tác</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-black/5 dark:divide-white/10">
            {result.items.map((user) => (
              <tr key={user.id} className={user.isActive ? undefined : "bg-red-50/60 dark:bg-red-950/20"}>
                <td className="px-4 py-3">
                  <div className="flex items-center gap-3">
                    <span className="flex size-9 shrink-0 items-center justify-center rounded-full bg-foreground text-sm font-semibold text-background">
                      {user.fullName.charAt(0).toUpperCase()}
                    </span>
                    <div className="min-w-0">
                      <p className="truncate font-medium">
                        {user.fullName}
                        {user.id === session.user.id && <span className="ml-1 text-xs text-zinc-500">(bạn)</span>}
                      </p>
                      <p className="truncate text-xs text-zinc-500">
                        @{user.userName} · {user.email}
                      </p>
                    </div>
                  </div>
                </td>
                <td className="px-4 py-3">
                  <div className="flex flex-wrap gap-1">
                    {user.roles.map((role) => (
                      <span
                        key={role}
                        className={`rounded-full px-2 py-0.5 text-xs font-medium ${
                          role === "Admin"
                            ? "bg-amber-100 text-amber-900 dark:bg-amber-900/40 dark:text-amber-200"
                            : "bg-black/[.05] dark:bg-white/10"
                        }`}
                      >
                        {ROLE_LABELS[role] ?? role}
                      </span>
                    ))}
                  </div>
                </td>
                <td className="px-4 py-3 whitespace-nowrap">
                  {user.isActive ? (
                    <span className="text-emerald-700 dark:text-emerald-400">● Hoạt động</span>
                  ) : (
                    <span className="text-red-600 dark:text-red-400">● Đã khóa</span>
                  )}
                </td>
                <td className="px-4 py-3 whitespace-nowrap text-zinc-600 dark:text-zinc-400">
                  {joinedDate.format(new Date(user.createdAt))}
                </td>
                <td className="px-4 py-3">
                  <UserAccessControls user={user} isSelf={user.id === session.user.id} />
                </td>
              </tr>
            ))}
            {result.items.length === 0 && (
              <tr>
                <td colSpan={5} className="px-4 py-10 text-center text-zinc-500">
                  Không tìm thấy người dùng nào.
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>

      {result.totalPages > 1 && (
        <nav className="flex items-center justify-between text-sm" aria-label="Phân trang">
          {result.hasPreviousPage ? (
            <Link href={pageHref(search, page - 1)} className="hover:underline">
              ← Trang trước
            </Link>
          ) : (
            <span />
          )}
          <span className="text-zinc-500">
            Trang {result.page}/{result.totalPages}
          </span>
          {result.hasNextPage ? (
            <Link href={pageHref(search, page + 1)} className="hover:underline">
              Trang sau →
            </Link>
          ) : (
            <span />
          )}
        </nav>
      )}
    </section>
  );
}

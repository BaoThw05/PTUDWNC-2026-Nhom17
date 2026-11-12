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
          <p className="text-sm text-(color:--auth-muted)">
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
            className="w-64 rounded-full border border-(color:--auth-input-border) bg-(color:--auth-input-bg) text-(color:--auth-fg) px-4 py-2 text-sm outline-none focus:border-(color:--auth-fg)"
          />
          <button type="submit" className="rounded-full bg-(color:--auth-fg) px-4 py-2 text-sm font-medium text-(color:--auth-bg)">
            Tìm
          </button>
        </form>
      </header>

      <div className="overflow-x-auto rounded-2xl border border-(color:--auth-border)">
        <table className="w-full text-left text-sm">
          <thead className="bg-(color:--auth-row-tint) text-xs whitespace-nowrap uppercase tracking-wide text-(color:--auth-subtle)">
            <tr>
              <th className="px-4 py-3 font-medium">Người dùng</th>
              <th className="px-4 py-3 font-medium">Vai trò</th>
              <th className="px-4 py-3 font-medium">Trạng thái</th>
              <th className="px-4 py-3 font-medium">Tham gia</th>
              <th className="px-4 py-3 font-medium sr-only">Thao tác</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-(color:--auth-border)">
            {result.items.map((user) => (
              <tr key={user.id} className={user.isActive ? undefined : "bg-(color:--auth-danger-tint)"}>
                <td className="px-4 py-3">
                  <div className="flex items-center gap-3">
                    <span className="flex size-9 shrink-0 items-center justify-center rounded-full bg-(color:--auth-fg) text-sm font-semibold text-(color:--auth-bg)">
                      {user.fullName.charAt(0).toUpperCase()}
                    </span>
                    <div className="min-w-0">
                      <p className="truncate font-medium">
                        {user.fullName}
                        {user.id === session.user.id && <span className="ml-1 text-xs text-(color:--auth-subtle)">(bạn)</span>}
                      </p>
                      <p className="truncate text-xs text-(color:--auth-subtle)">
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
                            ? "bg-(color:--auth-warning-bg) text-(color:--auth-warning-strong)"
                            : "bg-(color:--auth-chip)"
                        }`}
                      >
                        {ROLE_LABELS[role] ?? role}
                      </span>
                    ))}
                  </div>
                </td>
                <td className="px-4 py-3 whitespace-nowrap">
                  {user.isActive ? (
                    <span className="text-(color:--auth-success)">● Hoạt động</span>
                  ) : (
                    <span className="text-(color:--auth-danger)">● Đã khóa</span>
                  )}
                </td>
                <td className="px-4 py-3 whitespace-nowrap text-(color:--auth-muted)">
                  {joinedDate.format(new Date(user.createdAt))}
                </td>
                <td className="px-4 py-3">
                  <UserAccessControls user={user} isSelf={user.id === session.user.id} />
                </td>
              </tr>
            ))}
            {result.items.length === 0 && (
              <tr>
                <td colSpan={5} className="px-4 py-10 text-center text-(color:--auth-subtle)">
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
          <span className="text-(color:--auth-subtle)">
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

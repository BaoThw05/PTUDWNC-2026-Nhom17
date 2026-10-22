"use client";

import { useMutation } from "@tanstack/react-query";
import { useRouter } from "next/navigation";
import { ApiError } from "@/lib/api/client";
import { updateUserAccess } from "../api/client";
import { authErrorMessage } from "../errors";
import type { AdminUser } from "../types";

const BUTTON_CLASS =
  "whitespace-nowrap rounded-full border border-black/15 px-3 py-1 text-xs font-medium transition hover:bg-black/[.04] disabled:cursor-not-allowed disabled:opacity-40 dark:border-white/20 dark:hover:bg-white/[.06]";

/** Nút khóa/mở và cấp/gỡ Admin cho một dòng; Admin không tự khóa hay tự gỡ quyền của mình (backend cũng chặn). */
export function UserAccessControls({ user, isSelf }: { user: AdminUser; isSelf: boolean }) {
  const router = useRouter();
  const mutation = useMutation({
    mutationFn: (values: { isActive?: boolean; roles?: string[] }) => updateUserAccess(user.id, values),
    onSuccess: () => router.refresh(),
  });

  const isAdmin = user.roles.includes("Admin");
  const disabled = isSelf || mutation.isPending;

  function toggleActive() {
    const action = user.isActive ? "Khóa" : "Mở khóa";
    if (window.confirm(`${action} tài khoản ${user.email}?`)) {
      mutation.mutate({ isActive: !user.isActive });
    }
  }

  function toggleAdmin() {
    mutation.mutate({ roles: isAdmin ? ["Author"] : ["Admin", "Author"] });
  }

  return (
    <div className="flex flex-col items-end gap-1">
      <div className="flex gap-2">
        <button type="button" onClick={toggleAdmin} disabled={disabled} className={BUTTON_CLASS}>
          {isAdmin ? "Gỡ Admin" : "Cấp Admin"}
        </button>
        <button
          type="button"
          onClick={toggleActive}
          disabled={disabled}
          className={`${BUTTON_CLASS} ${user.isActive ? "text-red-600 dark:text-red-400" : "text-emerald-700 dark:text-emerald-400"}`}
        >
          {user.isActive ? "Khóa" : "Mở khóa"}
        </button>
      </div>
      {mutation.error && (
        <p role="alert" className="text-xs text-red-600 dark:text-red-400">
          {authErrorMessage(mutation.error instanceof ApiError ? mutation.error.code : "SERVICE_UNAVAILABLE")}
        </p>
      )}
    </div>
  );
}

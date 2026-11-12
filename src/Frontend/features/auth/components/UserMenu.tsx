"use client";

import Link from "next/link";
import { useSession } from "next-auth/react";
import { ADMIN_USERS_PATH, LOGIN_PATH, PROFILE_PATH, REGISTER_PATH } from "../constants";
import { SignOutButton } from "./SignOutButton";

// Đọc phiên ở phía client để header không làm mọi trang phải render động.
export function UserMenu() {
  const { data: session, status } = useSession();

  if (status === "loading") {
    return <span className="h-8 w-24 animate-pulse rounded-full bg-(color:--auth-border)" aria-hidden />;
  }

  if (!session || session.error) {
    return (
      <div className="flex items-center gap-3">
        <Link href={REGISTER_PATH} className="hover:underline">
          Đăng ký
        </Link>
        <Link href={LOGIN_PATH} className="rounded-full bg-(color:--auth-fg) px-4 py-1.5 text-(color:--auth-bg)">
          Đăng nhập
        </Link>
      </div>
    );
  }

  return (
    <div className="flex items-center gap-3">
      {session.user.roles.includes("Admin") && (
        <>
          <Link href="/dashboard/categories" className="hover:underline">
            Quản lý danh mục
          </Link>
          <Link href={ADMIN_USERS_PATH} className="hover:underline">
            Quản trị
          </Link>
        </>
      )}
      <Link href={PROFILE_PATH} className="font-medium hover:underline">
        {session.user.fullName}
      </Link>
      <SignOutButton className="rounded-full border border-(color:--auth-input-border) px-4 py-1.5 hover:bg-(color:--auth-hover)" />
    </div>
  );
}

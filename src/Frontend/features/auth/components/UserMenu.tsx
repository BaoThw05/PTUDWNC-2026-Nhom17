"use client";

import Link from "next/link";
import { useSession } from "next-auth/react";
import { LOGIN_PATH, PROFILE_PATH, REGISTER_PATH } from "../constants";
import { SignOutButton } from "./SignOutButton";

// Đọc phiên ở phía client để header không làm mọi trang phải render động.
export function UserMenu() {
  const { data: session, status } = useSession();

  if (status === "loading") {
    return <span className="h-8 w-24 animate-pulse rounded-full bg-black/10 dark:bg-white/10" aria-hidden />;
  }

  if (!session || session.error) {
    return (
      <div className="flex items-center gap-3">
        <Link href={REGISTER_PATH} className="hover:underline">
          Đăng ký
        </Link>
        <Link href={LOGIN_PATH} className="rounded-full bg-foreground px-4 py-1.5 text-background">
          Đăng nhập
        </Link>
      </div>
    );
  }

  return (
    <div className="flex items-center gap-3">
      <Link href={PROFILE_PATH} className="font-medium hover:underline">
        {session.user.fullName}
      </Link>
      <SignOutButton className="rounded-full border border-black/15 px-4 py-1.5 hover:bg-black/[.04] dark:border-white/20 dark:hover:bg-white/[.06]" />
    </div>
  );
}

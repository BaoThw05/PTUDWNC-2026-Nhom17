"use client";

import { signOut } from "next-auth/react";
import { useState } from "react";

export function SignOutButton({ className }: { className?: string }) {
  const [pending, setPending] = useState(false);

  return (
    <button
      type="button"
      disabled={pending}
      onClick={() => {
        setPending(true);
        void signOut({ redirectTo: "/" });
      }}
      className={className}
    >
      {pending ? "Đang đăng xuất…" : "Đăng xuất"}
    </button>
  );
}

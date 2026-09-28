"use client";

import { SessionProvider, signOut, useSession } from "next-auth/react";
import { useEffect, type ReactNode } from "react";
import { LOGIN_PATH, SESSION_REFETCH_SECONDS } from "../constants";

/** Refresh token không dùng được nữa (hết hạn, bị thu hồi) thì đăng xuất và đưa về trang đăng nhập. */
function ExpiredSessionGuard() {
  const { data: session } = useSession();

  useEffect(() => {
    if (session?.error) {
      void signOut({ redirectTo: `${LOGIN_PATH}?reason=expired` });
    }
  }, [session?.error]);

  return null;
}

export function AuthSessionProvider({ children }: { children: ReactNode }) {
  return (
    <SessionProvider refetchInterval={SESSION_REFETCH_SECONDS} refetchOnWindowFocus>
      <ExpiredSessionGuard />
      {children}
    </SessionProvider>
  );
}

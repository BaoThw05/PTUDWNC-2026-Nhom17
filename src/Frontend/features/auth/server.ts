import { redirect } from "next/navigation";
import { auth } from "./auth";

/** Access token cho code chạy phía server (Server Component, Route Handler). */
export async function getServerAccessToken(): Promise<string | null> {
  const session = await auth();
  return session && !session.error ? (session.accessToken ?? null) : null;
}

/** Dùng trong trang cần đăng nhập: chưa đăng nhập thì chuyển về trang login. */
export async function requireSession(callbackUrl: string) {
  const session = await auth();
  if (!session || session.error || !session.accessToken) {
    redirect(`/auth/login?callbackUrl=${encodeURIComponent(callbackUrl)}`);
  }
  return { ...session, accessToken: session.accessToken };
}

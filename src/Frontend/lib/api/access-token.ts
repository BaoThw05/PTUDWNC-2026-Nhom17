import { getSession } from "next-auth/react";

/**
 * Access token của người dùng hiện tại trên trình duyệt (lấy từ phiên Auth.js).
 * Phía server trả về null: code server lấy token bằng `getServerAccessToken()` rồi truyền vào `apiClient`.
 */
export async function getAccessToken(): Promise<string | null> {
  if (typeof window === "undefined") {
    return null;
  }

  const session = await getSession();
  return session && !session.error ? (session.accessToken ?? null) : null;
}

import { apiClient } from "@/lib/api/client";
import type { PagedResult } from "@/lib/api/types";
import type { AdminUser, AuthResponse, LoginAttempt, UserProfile } from "../types";

const AUTH_PATH = "/api/v1/auth";

// Các hàm dưới đây chạy trong Auth.js (phía server), không có phiên trình duyệt nên luôn gửi accessToken rõ ràng.

/** IP và thiết bị của trình duyệt, để backend giới hạn tần suất và ghi nhật ký đăng nhập đúng người dùng. */
export function clientHeaders(source: Headers): Record<string, string> {
  const forwarded: Record<string, string> = {};
  for (const name of ["x-forwarded-for", "user-agent"]) {
    const value = source.get(name);
    if (value) forwarded[name] = value;
  }
  return forwarded;
}

export function loginWithPassword(
  credentials: { email: string; password: string },
  headers: Record<string, string>,
): Promise<AuthResponse> {
  return apiClient.post<AuthResponse>(`${AUTH_PATH}/login`, { body: credentials, accessToken: null, headers });
}

export function loginWithGoogle(idToken: string, headers: Record<string, string>): Promise<AuthResponse> {
  return apiClient.post<AuthResponse>(`${AUTH_PATH}/google`, { body: { idToken }, accessToken: null, headers });
}

export function refreshSession(refreshToken: string): Promise<AuthResponse> {
  return apiClient.post<AuthResponse>(`${AUTH_PATH}/refresh`, {
    body: { refreshToken },
    accessToken: null,
  });
}

export function revokeRefreshToken(refreshToken: string, accessToken?: string): Promise<void> {
  return apiClient.post<void>(`${AUTH_PATH}/logout`, {
    body: { refreshToken },
    accessToken: accessToken ?? null,
  });
}

export function getProfile(accessToken: string): Promise<UserProfile> {
  return apiClient.get<UserProfile>(`${AUTH_PATH}/me`, { accessToken, cache: "no-store" });
}

export function getLoginHistory(accessToken: string): Promise<LoginAttempt[]> {
  return apiClient.get<LoginAttempt[]>(`${AUTH_PATH}/me/logins`, { accessToken, cache: "no-store" });
}

export function listUsers(
  accessToken: string,
  { search, page }: { search?: string; page?: number },
): Promise<PagedResult<AdminUser>> {
  const query = new URLSearchParams();
  if (search) query.set("search", search);
  if (page && page > 1) query.set("page", String(page));
  return apiClient.get<PagedResult<AdminUser>>(`/api/v1/admin/users?${query}`, { accessToken, cache: "no-store" });
}

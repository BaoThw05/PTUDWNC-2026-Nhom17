import { apiClient } from "@/lib/api/client";
import type { AuthResponse, UserProfile } from "../types";

const AUTH_PATH = "/api/v1/auth";

// Các hàm dưới đây chạy trong Auth.js (phía server), không có phiên trình duyệt nên luôn gửi accessToken rõ ràng.

export function loginWithPassword(
  credentials: { email: string; password: string },
  forwardedFor: string | null,
): Promise<AuthResponse> {
  return apiClient.post<AuthResponse>(`${AUTH_PATH}/login`, {
    body: credentials,
    accessToken: null,
    headers: forwardedFor ? { "X-Forwarded-For": forwardedFor } : undefined,
  });
}

export function loginWithGoogle(idToken: string): Promise<AuthResponse> {
  return apiClient.post<AuthResponse>(`${AUTH_PATH}/google`, {
    body: { idToken },
    accessToken: null,
  });
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

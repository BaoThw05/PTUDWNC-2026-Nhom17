import { apiClient } from "@/lib/api/client";
import type { AdminUser, AuthResponse, UserProfile } from "../types";

const AUTH_PATH = "/api/v1/auth";

// Gọi từ trình duyệt qua rewrite /api/* của Next.js.

export function register(values: {
  fullName: string;
  email: string;
  userName: string;
  password: string;
}): Promise<AuthResponse> {
  return apiClient.post<AuthResponse>(`${AUTH_PATH}/register`, {
    body: values,
    accessToken: null,
  });
}

export function changePassword(values: { currentPassword: string; newPassword: string }): Promise<void> {
  return apiClient.post<void>(`${AUTH_PATH}/change-password`, { body: values });
}

export function forgotPassword(values: { email: string }): Promise<void> {
  return apiClient.post<void>(`${AUTH_PATH}/forgot-password`, { body: values, accessToken: null });
}

export function resetPassword(values: { userId: string; token: string; newPassword: string }): Promise<void> {
  return apiClient.post<void>(`${AUTH_PATH}/reset-password`, { body: values, accessToken: null });
}

export function updateProfile(values: { fullName: string }): Promise<UserProfile> {
  return apiClient.patch<UserProfile>(`${AUTH_PATH}/me`, { body: values });
}

export function updateUserAccess(
  userId: string,
  values: { isActive?: boolean; roles?: string[] },
): Promise<AdminUser> {
  return apiClient.patch<AdminUser>(`/api/v1/admin/users/${userId}`, { body: values });
}

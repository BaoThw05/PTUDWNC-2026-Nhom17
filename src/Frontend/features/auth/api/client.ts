import { apiClient } from "@/lib/api/client";
import type { AuthResponse, UserProfile } from "../types";

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

export function updateProfile(values: { fullName: string }): Promise<UserProfile> {
  return apiClient.patch<UserProfile>(`${AUTH_PATH}/me`, { body: values });
}

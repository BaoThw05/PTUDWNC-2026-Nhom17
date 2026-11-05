/** Khớp UserProfileResponse của backend. */
export type UserProfile = {
  id: string;
  email: string;
  userName: string;
  fullName: string;
  avatarUrl: string | null;
  roles: string[];
  createdAt: string;
  emailConfirmed: boolean;
};

/** Khớp LoginAttemptResponse của backend (nhật ký đăng nhập). */
export type LoginAttempt = {
  occurredAt: string;
  method: "Password" | "Google";
  result: "Succeeded" | "InvalidPassword" | "LockedOut" | "Disabled";
  ipAddress: string | null;
  userAgent: string | null;
};

/** Khớp AdminUserResponse của backend (trang quản trị người dùng). */
export type AdminUser = UserProfile & { isActive: boolean };

/** Khớp AuthResponse của backend. */
export type AuthResponse = {
  accessToken: string;
  accessTokenExpiresAt: string;
  refreshToken: string;
  refreshTokenExpiresAt: string;
  user: UserProfile;
};

/** Thông tin người dùng giữ trong phiên Auth.js. */
export type SessionUser = Omit<UserProfile, "createdAt" | "emailConfirmed">;

export type SessionError = "RefreshTokenError";

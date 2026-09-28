/** Khớp UserProfileResponse của backend. */
export type UserProfile = {
  id: string;
  email: string;
  userName: string;
  fullName: string;
  avatarUrl: string | null;
  roles: string[];
  createdAt: string;
};

/** Khớp AuthResponse của backend. */
export type AuthResponse = {
  accessToken: string;
  accessTokenExpiresAt: string;
  refreshToken: string;
  refreshTokenExpiresAt: string;
  user: UserProfile;
};

/** Thông tin người dùng giữ trong phiên Auth.js. */
export type SessionUser = Omit<UserProfile, "createdAt">;

export type SessionError = "RefreshTokenError";

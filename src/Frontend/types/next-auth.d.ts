import { DefaultSession } from "next-auth";

export interface BackendUser {
  id: string;
  fullName: string;
  email: string;
  userName: string;
  avatarUrl: string | null;
  roles: string[];
}

export type AuthError = "RefreshAccessTokenError" | "GoogleLoginError";

declare module "next-auth" {
  interface Session extends DefaultSession {
    accessToken?: string;
    error?: AuthError;
    user: BackendUser & DefaultSession["user"];
  }

  interface User extends BackendUser {
    accessToken: string;
    refreshToken: string;
    expiresAt: number;
  }
}

declare module "next-auth/jwt" {
  interface JWT {
    accessToken?: string;
    refreshToken?: string;
    expiresAt?: number;
    error?: AuthError;
    user?: BackendUser;
  }
}
